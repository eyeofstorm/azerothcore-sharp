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

using System.Drawing;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using AzerothCore.Constants;
using AzerothCore.Database;
using AzerothCore.Logging;
using AzerothCore.Networking;
using AzerothCore.Server.Protocol;
using AzerothCore.Utilities;
using LocklessQueue.Queues;

namespace AzerothCore.Server;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ClientPktHeader
{
    public UInt16 size;
    public UInt32 cmd;

    public bool IsValidSize() { return size >= 4 && size< 10240; }
    public bool IsValidOpcode() { return cmd < (UInt32)OpcodeMisc.NUM_OPCODE_HANDLERS; }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ServerPktHeader
{
    public UInt32 size;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public byte[] header;

    /**
     * size is the length of the payload _plus_ the length of the opcode
     */
    public ServerPktHeader(UInt32 size, UInt16 cmd)
    {

        this.header = new byte[5];
        this.size = size;

        byte headerIndex = 0;

        if (isLargePacket())
        {
            LoggerFactory.GetLogger().Debug(LogFilter.Network, $"initializing large server to client packet. Size: {size}, cmd: {cmd}");
            header[headerIndex++] = (byte)(0x80 | (0xFF & (size >> 16)));
        }

        header[headerIndex++] = (byte)(0xFF & (size >> 8));
        header[headerIndex++] = (byte)(0xFF & size);

        header[headerIndex++] = (byte)(0xFF & cmd);
        header[headerIndex++] = (byte)(0xFF & (cmd >> 8));
    }

    public int getHeaderLength()
    {
        // cmd = 2 bytes, size= 2||3bytes
        return 2 + (isLargePacket() ? 3 : 2);
    }

    public bool isLargePacket()
    {
        return size > 0x7FFF;
    }
}

public class EncryptablePacket : WorldPacket
{
    public EncryptablePacket? SocketQueueLink { get; set; } = null;
    private bool _encrypt;

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
    private MessageBuffer                               _headerBuffer;
    private MessageBuffer                               _packetBuffer;
    private MPSCQueue<EncryptablePacket>                _bufferQueue;
    private int                                         _sendBufferSize;
    private AsyncCallbackProcessor<QueryCallback>       _queryProcessor;

    public WorldSocket(Socket socket) : base(socket)
    {
        _headerBuffer = new MessageBuffer();
        _packetBuffer = new MessageBuffer();
        _bufferQueue = new MPSCQueue<EncryptablePacket>(4096);
        _sendBufferSize = 4096;

        RandomHelper.NextBytes(_authSeed);

        _headerBuffer.ExpandStorageSize(Marshal.SizeOf(typeof(ClientPktHeader)));

        _queryProcessor = new();
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
        MessageBuffer buffer = new MessageBuffer(_sendBufferSize);

        while (_bufferQueue.TryDequeue(out EncryptablePacket? queued))
        {
            ServerPktHeader header = new ServerPktHeader(queued.GetSize() +2, queued.Opcode);

            // TODO: common: WorldSocket::Update()
            //if (queued.NeedsEncryption())
            //{
            //    _authCrypt.EncryptSend(header.header, header.getHeaderLength());
            //}
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
        throw new NotImplementedException();
    }

    private void SendAuthResponseError(ResponseCodes reseaon)
    {
        throw new NotImplementedException();
    }

    protected override void OnClose()
    {
        throw new NotImplementedException();
    }

    protected override void ReadHandler()
    {
        throw new NotImplementedException();
    }

    protected enum ReadDataHandlerResult
    {
        Ok = 0,
        Error = 1,
        WaitingForQuery = 2
    }
}
