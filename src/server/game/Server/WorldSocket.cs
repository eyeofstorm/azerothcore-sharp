/*
 * This file is part of the AzerothCore Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Affero General Public License as published by the
 * Free Software Foundation; either version 3 of the License, or (at your
 * option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for
 * more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Net.Sockets;
using System.Runtime.InteropServices;

using AzerothCore.Constants;
using AzerothCore.Cryptography;
using AzerothCore.Database;
using AzerothCore.Logging;
using AzerothCore.Networking;
using AzerothCore.Threading;
using AzerothCore.Utilities;

using LocklessQueue.Queues;

namespace AzerothCore.Game;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ClientPktHeader
{
    public ushort size;
    public uint cmd;

    public bool IsValidSize() { return size >= 4 && size< 10240; }
    public bool IsValidOpcode() { return cmd < (uint)OpcodeMisc.NUM_OPCODE_HANDLERS; }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ServerPktHeader
{
    public uint size;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public byte[] header;

    /**
     * size is the length of the payload _plus_ the length of the opcode
     */
    public ServerPktHeader(uint size, ushort cmd)
    {

        this.header = new byte[5];
        this.size = size;

        byte headerIndex = 0;

        if (IsLargePacket())
        {
            LoggerFactory.GetLogger().Debug(LogFilter.Network, $"initializing large server to client packet. Size: {size}, cmd: {cmd}");
            header[headerIndex++] = (byte)(0x80 | (0xFF & (size >> 16)));
        }

        header[headerIndex++] = (byte)(0xFF & (size >> 8));
        header[headerIndex++] = (byte)(0xFF & size);

        header[headerIndex++] = (byte)(0xFF & cmd);
        header[headerIndex++] = (byte)(0xFF & (cmd >> 8));
    }

    public int GetHeaderLength()
    {
        // cmd = 2 bytes, size= 2||3bytes
        return 2 + (IsLargePacket() ? 3 : 2);
    }

    public bool IsLargePacket()
    {
        return size > 0x7FFF;
    }
}

public class EncryptablePacket : WorldPacket
{
    public EncryptablePacket? SocketQueueLink { get; set; } = null;
    private readonly bool _encrypt;

    public EncryptablePacket(WorldPacket packet, bool encrypt) : base(packet)
    {
        _encrypt = encrypt;
    }

    public bool NeedsEncryption()
    {
        return _encrypt;
    }
}

public partial class WorldSocket : SocketBase
{
    private byte[]                                      _authSeed = new byte[4];
    private AuthCrypt                                   _authCrypt;
    private MessageBuffer                               _headerBuffer;
    private MessageBuffer                               _packetBuffer;
    private MPSCQueue<EncryptablePacket>                _bufferQueue;
    private int                                         _sendBufferSize;
    private AsyncCallbackProcessor<QueryCallback>       _queryProcessor;
    private bool                                        _authed;
    private WorldSession?                               _worldSession;

    public WorldSocket(Socket socket) : base(socket)
    {
        RandomHelper.NextBytes(_authSeed);

        _authCrypt = new AuthCrypt();

        _headerBuffer = new MessageBuffer();
        _headerBuffer.Resize(Marshal.SizeOf(typeof(ClientPktHeader)));
        _packetBuffer = new MessageBuffer();
        _bufferQueue = new MPSCQueue<EncryptablePacket>(4096);
        _sendBufferSize = 4096;

        _queryProcessor = new();

        _authed = false;

        _worldSession = null;
    }

    public override void Start()
    {
        var ipAddress = GetRemoteIpAddress();

        if (ipAddress == null)
        {
            CloseSocket();
            return;
        }

        logger.Debug(LogFilter.Session, $"Accepted connection from {ipAddress.Address.ToString()}");

        var stmt = LoginDatabase.GetPreparedStatement(LoginStatements.LOGIN_SEL_IP_INFO);
        stmt.AddValue(0, ipAddress.Address.ToString());

        QueryCallback asyncQuery = DB.Login.AsyncQuery(stmt);
        asyncQuery.WithCallback(CheckIpCallback);

        _queryProcessor.AddCallback(asyncQuery);
    }

    private void CheckIpCallback(SQLResult result)
    {
        var ipAddress = GetRemoteIpAddress();

        if (ipAddress == null)
        {
            CloseSocket();
            return;
        }

        if (!result.IsEmpty())
        {
            bool banned = false;

            do
            {
                if (result.Read<UInt64>(0) != 0)
                {
                    banned = true;
                    break;
                }
            }
            while (result.NextRow());

            if (banned)
            {
                SendAuthResponseError(ResponseCodes.AUTH_REJECT);

                logger.Debug(LogFilter.Network, $"WorldSocket.CheckIpCallback: Sent Auth Response (IP {ipAddress.ToString()} banned).");

                DelayedCloseSocket();

                return;
            }
        }

        AsyncRead();

        HandleSendAuthSession();
    }

    public override bool Update()
    {
        MessageBuffer buffer = new (_sendBufferSize);

        while (_bufferQueue.TryDequeue(out EncryptablePacket? queued))
        {
            ServerPktHeader header = new (queued.GetSize() +2, queued.Opcode);

            if (queued.NeedsEncryption())
            {
                _authCrypt.EncryptSend(header.header, 0, header.GetHeaderLength());
            }

            if (buffer.GetRemainingSpace() < queued.GetSize() + header.GetHeaderLength())
            {
                buffer.Resize(_sendBufferSize);
                QueuePacket(buffer);
            }

            if (buffer.GetRemainingSpace() >= queued.GetSize() + header.GetHeaderLength())
            {
                buffer.Write(header.header, header.GetHeaderLength());

                if (queued.GetSize() > 0)
                {
                    buffer.Write(queued.GetData(), (int)queued.GetSize());
                }
            }
            else
            {
                // single packet larger than 4096 bytes

                MessageBuffer packetBuffer = new ((int)queued.GetSize() + header.GetHeaderLength());
                packetBuffer.Write(header.header, header.GetHeaderLength());

                if (queued.GetSize() > 0)
                {
                    packetBuffer.Write(queued.GetData(), (int)queued.GetSize());
                }

                QueuePacket(packetBuffer);
            }

            queued = null;
        }

        if (buffer.GetActiveSize() > 0)
        {
            QueuePacket(buffer);
        }

        if (!base.Update())
        {
            return false;
        }

        _queryProcessor.ProcessReadyCallbacks();

        return true;
    }

    private void HandleSendAuthSession()
    {
        WorldPacket packet = new (Opcodes.SMSG_AUTH_CHALLENGE, 40);

        packet.WriteUInt32(1);                                    // 1...31
        packet.WriteBytes(_authSeed);

        packet.WriteBytes(RandomHelper.NextBytes(32));            // new encryption seeds

        SendPacketAndLogOpcode(packet);
    }

    private void SendPacketAndLogOpcode(WorldPacket packet)
    {
        logger.Trace(LogFilter.Network, $"S->C: { GetRemoteIpAddress()?.ToString() ?? "Unknow IP Address" } { Enum.GetName((Opcodes)packet.Opcode) ?? "Unkown Opcode" }");

        SendPacket(packet);
    }

    private void SendPacket(WorldPacket packet)
    {
        if (!IsOpen())
        {
            return;
        }

        if (Global.sPacketLog.CanLogPacket())
        {
            Global.sPacketLog.LogPacket(packet, Direction.SERVER_TO_CLIENT, GetRemoteIpAddress());
        }

        if (!_bufferQueue.TryEnqueue(new EncryptablePacket(packet, _authCrypt.IsInitialized)))
        {
            logger.Fatal(LogFilter.Network, $"can't enqueue packet to MPSC queue. queue is full.");
        }
    }

    private void SendAuthResponseError(ResponseCodes reseaon)
    {
        WorldPacket packet = new WorldPacket(Opcodes.SMSG_AUTH_RESPONSE, 1);

        packet.WriteUInt8((byte)reseaon);

        SendPacketAndLogOpcode(packet);
    }

    protected override void OnClose()
    {
        // TODO: game: WorldSocket::OnClose()
    }

    protected override void ReadHandler()
    {
        if (!IsOpen())
        {
            return;
        }

        MessageBuffer packet = GetReadBuffer();

        while (packet.GetActiveSize() > 0)
        {
            if (_headerBuffer.GetRemainingSpace() > 0)
            {
                // need to receive the header
                int readHeaderSize = Math.Min(packet.GetActiveSize(), _headerBuffer.GetRemainingSpace());
                _headerBuffer.Write(packet.GetBasePointer(), packet.GetReadPos(), readHeaderSize);
                packet.ReadCompleted(readHeaderSize);

                if (_headerBuffer.GetRemainingSpace() > 0)
                {
                    // Couldn't receive the whole header this time.
                    break;
                }

                // We just received nice new header
                if (!ReadHeaderHandler())
                {
                    CloseSocket();
                    return;
                }
            }

            // We have full read header, now check the data payload
            if (_packetBuffer.GetRemainingSpace() > 0)
            {
                // need more data in the payload
                int readDataSize = Math.Min(packet.GetActiveSize(), _packetBuffer.GetRemainingSpace());
                _packetBuffer.Write(packet.GetBasePointer(), packet.GetReadPos(), readDataSize);
                packet.ReadCompleted(readDataSize);

                if (_packetBuffer.GetRemainingSpace() > 0)
                {
                    // Couldn't receive the whole data this time.
                    break;
                }
            }

            // just received fresh new payload
            ReadDataHandlerResult result = ReadDataHandler();
            _headerBuffer.Reset();

            if (result != ReadDataHandlerResult.Ok)
            {
                if (result != ReadDataHandlerResult.WaitingForQuery)
                {
                    CloseSocket();
                }

                return;
            }
        }

        AsyncRead();
    }

    protected bool ReadHeaderHandler()
    {
        if (_headerBuffer.GetActiveSize() != Marshal.SizeOf(typeof(ClientPktHeader)))
        {
            throw new InvalidDataException();
        }

        if (_authCrypt.IsInitialized)
        {
            _authCrypt.DecryptRecv(_headerBuffer.GetBasePointer(), _headerBuffer.GetReadPos(), _headerBuffer.GetActiveSize());
        }

        ClientPktHeader header = _packetBuffer.CastTo<ClientPktHeader>();

        if (!header.IsValidSize() || !header.IsValidOpcode())
        {
            logger.Error(LogFilter.Network, $"WorldSocket::ReadHeaderHandler(): client {GetRemoteIpAddress()?.ToString() ?? "Unknow IP Address" } sent malformed packet (size: { header.size }, cmd: { header.cmd })");

            return false;
        }

        header.size -= sizeof(uint);
        _packetBuffer.Resize(header.size);

        return true;
    }

    protected ReadDataHandlerResult ReadDataHandler()
    {
        ClientPktHeader header = _headerBuffer.CastTo<ClientPktHeader>();
        Opcodes opcode = (Opcodes)header.cmd;

        WorldPacket packet = new WorldPacket(opcode, new Memory<byte>(_packetBuffer.GetBasePointer(), _packetBuffer.GetReadPos(), _packetBuffer.GetActiveSize()).ToArray());
        WorldPacket? packetToQueue = null;

        if (Global.sPacketLog.CanLogPacket())
        {
            Global.sPacketLog.LogPacket(packet, Direction.CLIENT_TO_SERVER, GetRemoteIpAddress());
        }

        UniqueLock sessionGuard = new (true);

        switch (opcode)
        {
            case Opcodes.CMSG_PING:
            {
                LogOpcodeText(Opcodes.CMSG_PING);

                try
                {
                    return HandlePing(packet);
                }
                catch
                {
                    return ReadDataHandlerResult.Error;
                }
            }
            case Opcodes.CMSG_AUTH_SESSION:
            {
                LogOpcodeText(Opcodes.CMSG_PING);

                if (_authed)
                {
                    if (sessionGuard.Lock())
                    {
                        // TODO: game: _worldSession->GetPlayerInfo()
                        //logger.Error(LogFilter.Network, $"WorldSocket::ProcessIncoming: received duplicate CMSG_AUTH_SESSION from { _worldSession->GetPlayerInfo() }");
                    }

                    return ReadDataHandlerResult.Error;
                }

                try
                {
                    HandleAuthSession(packet);
                    return ReadDataHandlerResult.WaitingForQuery;
                }
                catch
                {
                    logger.Error(LogFilter.Network, $"WorldSocket::ReadDataHandler(): client {GetRemoteIpAddress()?.ToString()} sent malformed CMSG_AUTH_SESSION");
                    return ReadDataHandlerResult.Error;
                }
            }
            case Opcodes.CMSG_KEEP_ALIVE:
            {
                // TODO: game: WorldSocket::ReadDataHandler()
                throw new NotImplementedException();
            }
            case Opcodes.CMSG_TIME_SYNC_RESP:
            {
                // TODO: game: WorldSocket::ReadDataHandler()
                throw new NotImplementedException();
            }
            default:
            {
                packetToQueue = new WorldPacket(packet);
                break;
            }
        }

        sessionGuard.Lock();

        LogOpcodeText(opcode);

        if (_worldSession == null)
        {
            logger.Error(LogFilter.Network, $"ProcessIncoming: Client not authed opcode = {Enum.GetName(typeof(Opcodes), opcode)}");

            packetToQueue = null;

            return ReadDataHandlerResult.Error;
        }



        // TODO: game: WorldSocket::ReadDataHandler()

        return ReadDataHandlerResult.Ok;
    }

    private void HandleAuthSession(WorldPacket packet)
    {
        // TODO: game: WorldSocket::HandleAuthSession()
        throw new NotImplementedException();
    }

    private ReadDataHandlerResult HandlePing(WorldPacket packet)
    {
        // TODO: game: WorldSocket::HandlePing()
        throw new NotImplementedException();
    }

    private void LogOpcodeText(Opcodes opcode)
    {
        logger.Trace(LogFilter.Network, $"C->S: {GetRemoteIpAddress()?.ToString()} {Enum.GetName(typeof(Opcodes), opcode)}");
    }

    protected enum ReadDataHandlerResult
    {
        Ok = 0,
        Error = 1,
        WaitingForQuery = 2
    }
}
