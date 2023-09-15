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
using System.Text;

using AzerothCore.Configuration;
using AzerothCore.Constants;
using AzerothCore.Cryptography;
using AzerothCore.Database;
using AzerothCore.Game.Server;
using AzerothCore.Logging;
using AzerothCore.Networking;
using AzerothCore.Threading;
using AzerothCore.Utilities;

using LocklessQueue.Queues;

namespace AzerothCore.Game;

public struct AuthSession
{
    public uint         BattlegroupID;
    public uint         LoginServerType;
    public uint         RealmID;
    public uint         Build;
    public byte[]       LocalChallenge;
    public uint         LoginServerID;
    public uint         RegionID;
    public ulong        DosResponse;
    public byte[]       Digest;
    public string       Account;
    public ByteBuffer   AddonInfo;
}

public struct AccountInfo
{
    public uint         Id;
    public byte[]?      SessionKey;
    public string?      LastIP;
    public bool         IsLockedToIP;
    public string?      LockCountry;
    public byte         Expansion;
    public long         MuteTime;
    public Locale       Locale;
    public uint         Recruiter;
    public string?      OS;
    public bool         IsRectuiter;
    public AccountTypes Security;
    public bool         IsBanned;
    public uint         TotalTime;

    public AccountInfo(SQLResult result)
    {
        //           0             1          2         3               4            5           6         7            8     9           10          11
        // SELECT a.id, a.sessionkey, a.last_ip, a.locked, a.lock_country, a.expansion, a.mutetime, a.locale, a.recruiter, a.os, a.totaltime, aa.gmLevel,
        //                                                           12    13
        // ab.unbandate > UNIX_TIMESTAMP() OR ab.unbandate = ab.bandate, r.id
        // FROM account a
        // LEFT JOIN account_access aa ON a.id = aa.AccountID AND aa.RealmID IN (-1, ?)
        // LEFT JOIN account_banned ab ON a.id = ab.id
        // LEFT JOIN account r ON a.id = r.recruiter
        // WHERE a.username = ? ORDER BY aa.RealmID DESC LIMIT 1

        Id = result.Read<uint>(0);
        SessionKey = result.ReadBytes(1, 40);
        LastIP = result.Read<string>(2);
        IsLockedToIP = result.Read<bool>(3);
        LockCountry = result.Read<string>(4);
        Expansion = result.Read<byte>(5);
        MuteTime = result.Read<long>(6);
        Locale = (Locale)result.Read<byte>(7);
        Recruiter = result.Read<uint>(8);
        OS = result.Read<string>(9);
        TotalTime = result.Read<uint>(10);
        Security = (AccountTypes)result.Read<byte>(11);
        IsBanned = result.Read<long>(12) != 0;
        IsRectuiter = result.Read<uint>(13) != 0;

        uint expansion = ConfigMgr.GetOption<uint>("Expansion", 2);

        if (Expansion > expansion)
        {
            Expansion = (byte)expansion;
        }

        if (Locale >= Locale.TOTAL_LOCALES)
        {
            Locale = Locale.LOCALE_enUS;
        }
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ClientPktHeader
{
    public ushort size;
    public uint cmd;

    public readonly bool IsValidSize() { return size >= 4 && size< 10240; }
    public readonly bool IsValidOpcode() { return cmd < (uint)OpcodeMisc.NUM_OPCODE_HANDLERS; }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ServerPktHeader
{
    public uint Size;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public byte[] Header;

    /**
     * size is the length of the payload _plus_ the length of the opcode
     */
    public ServerPktHeader(uint size, ushort cmd)
    {
        Size = size;
        Header = new byte[5];

        byte headerIndex = 0;

        if (IsLargePacket())
        {
            LoggerFactory.GetLogger().Debug(LogFilter.Network, $"initializing large server to client packet. Size: {size}, cmd: {cmd}");
            Header[headerIndex++] = (byte)(0x80 | (0xFF & (size >> 16)));
        }

        Header[headerIndex++] = (byte)(0xFF & (size >> 8));
        Header[headerIndex++] = (byte)(0xFF & size);

        Header[headerIndex++] = (byte)(0xFF & cmd);
        Header[headerIndex++] = (byte)(0xFF & (cmd >> 8));
    }

    public readonly int GetHeaderLength()
    {
        // cmd = 2 bytes, size= 2 || 3bytes
        return 2 + (IsLargePacket() ? 3 : 2);
    }

    public readonly bool IsLargePacket()
    {
        return Size > 0x7FFF;
    }
}

public class EncryptablePacket : WorldPacketData
{
    private readonly bool _encrypt;

    public EncryptablePacket(WorldPacketData packet, bool encrypt) : base(packet)
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
    private AsyncCallbackProcessor<QueryCallback>       _queryProcessor;
    private bool                                        _authed;
    private readonly object                             _worldSessionLock = new();
    private WorldSession?                               _worldSession;

    private int                                         _sendBufferSize;
    private MessageBuffer                               _sendBuffer;

    private DateTime                                    _lastPingTime;
    private uint                                        _overSpeedPings;

    private string?                                     _ipCountry;

    public void SetSendBufferSize(int sendBufferSize)
    {
        _sendBufferSize = sendBufferSize;
        _sendBuffer.Resize(sendBufferSize);
    }

    public WorldSocket(Socket socket) : base(socket)
    {
        RandomHelper.NextBytes(_authSeed);

        _authCrypt = new AuthCrypt();

        _headerBuffer = new MessageBuffer();
        _headerBuffer.Resize(Marshal.SizeOf(typeof(ClientPktHeader)));
        _packetBuffer = new MessageBuffer();
        _bufferQueue = new MPSCQueue<EncryptablePacket>(4096);

        _sendBufferSize = 4096;
        _sendBuffer = new MessageBuffer(_sendBufferSize);

        _queryProcessor = new();

        _authed = false;
        _worldSession = null;
        _ipCountry = null;
    }

    public override void Start()
    {
        var ipAddress = GetRemoteIpAddress();

        if (ipAddress == null)
        {
            CloseSocket();
            return;
        }

        logger.Debug(LogFilter.Session, $"Accepted connection from {ipAddress.Address}");

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
                if (result.Read<ulong>(0) != 0)
                {
                    banned = true;
                    break;
                }
            }
            while (result.NextRow());

            if (banned)
            {
                SendAuthResponseError(ResponseCodes.AUTH_REJECT);

                logger.Debug(LogFilter.Network, $"WorldSocket.CheckIpCallback: Sent Auth Response (IP {ipAddress} banned).");

                DelayedCloseSocket();

                return;
            }
        }

        AsyncRead();

        SendAuthChallenge();
    }

    public override bool Update()
    {
        while (_bufferQueue.TryDequeue(out EncryptablePacket? queued))
        {
            ServerPktHeader header = new (queued.GetSize() + 2, queued.Opcode);

            if (queued.NeedsEncryption())
            {
                //byte[] dumpBytes = new byte[header.GetHeaderLength()];
                //Array.Copy(header.Header, 0, dumpBytes, 0, header.GetHeaderLength());
                //logger.Debug(LogFilter.Network, $"{Enum.GetName(typeof(Opcodes), queued.Opcode)} Header Before Encryption: {Environment.NewLine}{dumpBytes.DumpHex()}{Environment.NewLine}");

                _authCrypt.EncryptSend(header.Header, 0, header.GetHeaderLength());

                //dumpBytes.Clear();
                //Array.Copy(header.Header, 0, dumpBytes, 0, header.GetHeaderLength());
                //logger.Debug(LogFilter.Network, $"{Enum.GetName(typeof(Opcodes), queued.Opcode)} Header After Encryption: {Environment.NewLine}{dumpBytes.DumpHex()}{Environment.NewLine}");
            }

            if (_sendBuffer.GetRemainingSpace() < queued.GetSize() + header.GetHeaderLength())
            {
                _sendBuffer.Normalize();
                _sendBuffer.EnsureFreeSpace();

                QueuePacket(_sendBuffer);
            }

            if (_sendBuffer.GetRemainingSpace() >= queued.GetSize() + header.GetHeaderLength())
            {
                _sendBuffer.Write(header.Header, header.GetHeaderLength());

                if (queued.GetSize() > 0)
                {
                    _sendBuffer.Write(queued.GetData(), (int)queued.GetSize());
                }
            }
            else
            {
                // single packet larger than 4096 bytes

                MessageBuffer packetBuffer = new ((int)queued.GetSize() + header.GetHeaderLength());
                packetBuffer.Write(header.Header, header.GetHeaderLength());

                if (queued.GetSize() > 0)
                {
                    packetBuffer.Write(queued.GetData(), (int)queued.GetSize());
                }

                QueuePacket(packetBuffer);
            }
        }

        if (_sendBuffer.GetActiveSize() > 0)
        {
            QueuePacket(_sendBuffer);
        }

        if (!base.Update())
        {
            return false;
        }

        _queryProcessor.ProcessReadyCallbacks();

        return true;
    }

    private void SendAuthChallenge()
    {
        WorldPacketData packet = new (Opcodes.SMSG_AUTH_CHALLENGE);

        packet.WriteUInt(1U);                                    // 1...31
        packet.WriteBytes(_authSeed);

        packet.WriteBytes(RandomHelper.NextBytes(32));            // new encryption seeds

        SendPacketAndLogOpcode(packet);
    }

    private void SendPacketAndLogOpcode(WorldPacketData packet)
    {
        logger.Debug(LogFilter.Network, $"Server->Client: { GetRemoteIpAddress()?.ToString() ?? "Unknow IP Address" } { Enum.GetName((Opcodes)packet.Opcode) ?? "Unkown Opcode" }");

        SendPacket(packet);
    }

    public void SendPacket(WorldPacketData packet)
    {
        if (!IsOpen())
        {
            return;
        }

        if (PacketFileLogger.CanLogPacket())
        {
            PacketFileLogger.LogPacket(packet, PacketDirection.SERVER_TO_CLIENT, GetRemoteIpAddress());
        }

        EncryptablePacket encrytPacket = new(packet, _authCrypt.IsInitialized);

        if (!_bufferQueue.TryEnqueue(encrytPacket))
        {
            logger.Fatal(LogFilter.Network, $"can't enqueue packet to MPSC queue. queue is full.");
        }
    }

    private void SendAuthResponseError(ResponseCodes reseaon)
    {
        WorldPacketData packet = new(Opcodes.SMSG_AUTH_RESPONSE);

        packet.WriteByte((byte)reseaon);

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

        ClientPktHeader header = _headerBuffer.CastTo<ClientPktHeader>();

        // The size is in big endian order, so we should convert it into little endian.
        header.size = BitConverter.ToUInt16(BitConverter.GetBytes(header.size).Reverse().ToArray());

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

        WorldPacketData packet = new(opcode, new Memory<byte>(_packetBuffer.GetBasePointer(), _packetBuffer.GetReadPos(), _packetBuffer.GetActiveSize()).ToArray());

        if (PacketFileLogger.CanLogPacket())
        {
            PacketFileLogger.LogPacket(packet, PacketDirection.CLIENT_TO_SERVER, GetRemoteIpAddress());
        }

        UniqueLock sessionGuard = new (true);

        WorldPacketData? packetToQueue;

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
                    LogOpcodeText(Opcodes.CMSG_AUTH_SESSION);

                    if (_authed)
                    {
                        if (sessionGuard.Lock())
                        {
                            logger.Error(LogFilter.Network, $"WorldSocket::ReadDataHandler: received duplicate CMSG_AUTH_SESSION from {_worldSession?.GetPlayerInfo()}");
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
                    sessionGuard.Lock();

                    LogOpcodeText(Opcodes.CMSG_KEEP_ALIVE);

                    if (_worldSession != null)
                    {
                        _worldSession.ResetTimeOutTime(true);

                        return ReadDataHandlerResult.Ok;
                    }

                    logger.Error(LogFilter.Network, $"WorldSocket::ReadDataHandler: client {GetRemoteIpAddress()} sent CMSG_KEEP_ALIVE without being authenticated");

                    return ReadDataHandlerResult.Error;
                }
            case Opcodes.CMSG_TIME_SYNC_RESP:
                {
                    packetToQueue = new WorldPacketData(packet, DateTime.Now);
                    break;
                }
            default:
                {
                    packetToQueue = new WorldPacketData(packet);
                    break;
                }
        }

        sessionGuard.Lock();

        LogOpcodeText(opcode);

        if (_worldSession == null)
        {
            logger.Error(LogFilter.Network, $"ProcessIncoming: Client not authed opcode = {Enum.GetName(typeof(Opcodes), opcode)}");

            return ReadDataHandlerResult.Error;
        }

        OpcodeTable opcodeTable = OpcodeTable.Instance;
        ClientOpcodeHandler? opcodeHandler = opcodeTable[opcode];

        if (opcodeHandler == null)
        {
            logger.Error(LogFilter.Network, $"No defined handler for opcode {Enum.GetName(typeof(Opcodes), opcode)} sent by {_worldSession.GetPlayerInfo()}");

            return ReadDataHandlerResult.Error;
        }

        // Our Idle timer will reset on any non PING opcodes on login screen, allowing us to catch people idling.
        if (packetToQueue.Opcode != (ushort)Opcodes.CMSG_WARDEN_DATA)
        {
            _worldSession.ResetTimeOutTime(false);
        }

        // Copy the packet to the heap before enqueuing
        _worldSession.QueuePacket(packetToQueue);

        return ReadDataHandlerResult.Ok;
    }

    private void HandleAuthSession(WorldPacketData recvPacket)
    {
        AuthSession authSession = new()
        {
            // Read the content of the packet
            Build = recvPacket.ReadUInt32(),
            LoginServerID = recvPacket.ReadUInt32(),
            Account = recvPacket.ReadCString(),
            LoginServerType = recvPacket.ReadUInt32(),
            LocalChallenge = recvPacket.ReadBytes(4),
            RegionID = recvPacket.ReadUInt32(),
            BattlegroupID = recvPacket.ReadUInt32(),
            RealmID = recvPacket.ReadUInt32(),               // realmId from auth_database.realmlist table
            DosResponse = recvPacket.ReadUInt64(),
            Digest = recvPacket.ReadBytes(20),
            AddonInfo = new ByteBuffer(recvPacket.ReadBytes(recvPacket.GetSize() - recvPacket.GetReadPosition()))
        };

        // Get the account details from the account table
        var stmt = LoginDatabase.GetPreparedStatement(LoginStatements.LOGIN_SEL_ACCOUNT_INFO_BY_NAME);
        stmt.AddValue(0, Global.sWorld.GetRealm().Id.Index);
        stmt.AddValue(1, authSession.Account);

        _queryProcessor.AddCallback(DB.Login.AsyncQuery(stmt).WithCallback(HandleAuthSessionCallback, authSession));
    }

    private void HandleAuthSessionCallback(AuthSession authSession, SQLResult result)
    {
        // Stop if the account is not found
        if (result.IsEmpty())
        {
            // We can not log here, as we do not know the account. Thus, no accountId.
            SendAuthResponseError(ResponseCodes.AUTH_UNKNOWN_ACCOUNT);
            logger.Error(LogFilter.Network, $"WorldSocket::HandleAuthSession: Sent Auth Response (unknown account).");

            DelayedCloseSocket();

            return;
        }

        AccountInfo account = new(result);

        string? address = ConfigMgr.GetOption("AllowLoggingIPAddressesInDatabase", true) ? GetRemoteIpAddress()?.Address.ToString() : "0.0.0.0";

        // As we don't know if attempted login process by ip works, we update last_attempt_ip right away
        var stmt = LoginDatabase.GetPreparedStatement(LoginStatements.LOGIN_UPD_LAST_ATTEMPT_IP);
        stmt.AddValue(0, address);
        stmt.AddValue(1, authSession.Account);

        DB.Login.Execute(stmt);

        if (account.SessionKey == null || account.SessionKey.Length != 40)
        {
            SendAuthResponseError(ResponseCodes.AUTH_REJECT);
            logger.Error(LogFilter.Network, $"WorldSocket::HandleAuthSession: Sent Auth Response (unknown account).");

            DelayedCloseSocket();

            return;
        }

        // even if auth credentials are bad, try using the session key we have - client cannot read auth response error without it
        _authCrypt.Init(account.SessionKey);

        // First reject the connection if packet contains invalid data or realm state doesn't allow logging in
        if (Global.sWorld.IsClosed())
        {
            SendAuthResponseError(ResponseCodes.AUTH_REJECT);
            logger.Error(LogFilter.Network, $"WorldSocket::HandleAuthSession: World closed, denying client ({GetRemoteIpAddress()}).");

            DelayedCloseSocket();

            return;
        }

        if (authSession.RealmID != Global.sWorld.GetRealm().Id.Index)
        {
            SendAuthResponseError(ResponseCodes.REALM_LIST_REALM_NOT_FOUND);
            logger.Error(LogFilter.Network, $"WorldSocket::HandleAuthSession: Client {GetRemoteIpAddress()} requested connecting with realm id {authSession.RealmID} but this realm has id {Global.sWorld.GetRealm().Id.Index} set in config.");

            DelayedCloseSocket();

            return;
        }

        // Must be done before WorldSession is created
        bool wardenActive = ConfigMgr.GetOption("Warden.Enabled", true);

        if (wardenActive && account.OS != "Win" && account.OS != "OSX")
        {
            SendAuthResponseError(ResponseCodes.AUTH_REJECT);
            logger.Error(LogFilter.Network, $"WorldSocket::HandleAuthSession: Client {address} attempted to log in using invalid client OS ({account.OS}).");

            DelayedCloseSocket();

            return;
        }

        // Check that Key and account name are the same on client and server
        byte[] t = { 0x00, 0x00, 0x00, 0x00 };

        SHA1 sha = SHA1.Create();
        sha.Update(Encoding.ASCII.GetBytes(authSession.Account));
        sha.Update(t);
        sha.Update(authSession.LocalChallenge);
        sha.Update(_authSeed);
        sha.Final(account.SessionKey);
        byte[] digest = sha.Hash;

        if (!digest.Compare(authSession.Digest))
        {
            SendAuthResponseError(ResponseCodes.AUTH_REJECT);
            logger.Error(LogFilter.Network, $"WorldSocket::HandleAuthSession: Authentication failed for account: {account.Id} ('{authSession.Account}') address: {address}");

            DelayedCloseSocket();

            return;
        }

        // TODO: game: WorldSocket::HandleAuthSessionCallback(SQLResult result) => sIPLocation->GetLocationRecord(address)
        //if (IpLocationRecord const* location = sIPLocation->GetLocationRecord(address))
        //{
        //    _ipCountry = location->CountryCode;
        //}

        // Re-check ip locking (same check as in auth).
        if (account.IsLockedToIP)
        {
            if (account.LastIP != address)
            {
                SendAuthResponseError(ResponseCodes.AUTH_REJECT);
                logger.Error(LogFilter.Network, $"WorldSocket::HandleAuthSession: Sent Auth Response (Account IP differs. Original IP: {account.LastIP}, new IP: {address}).");

                DelayedCloseSocket();

                return;
            }
        }
        else if (!account.LockCountry.IsEmpty() && account.LockCountry != "00" && !_ipCountry.IsEmpty())
        {
            if (account.LockCountry != _ipCountry)
            {
                SendAuthResponseError(ResponseCodes.AUTH_REJECT);
                logger.Error(LogFilter.Network, $"WorldSocket::HandleAuthSession: Sent Auth Response (Account country differs. Original country: {account.LockCountry}, new country: {_ipCountry}).");

                DelayedCloseSocket();
                return;
            }
        }

        //! Negative mutetime indicates amount of minutes to be muted effective on next login - which is now.
        if (account.MuteTime < 0)
        {
            account.MuteTime = TimeHelper.UnixTime + Math.Abs(account.MuteTime);

            stmt = LoginDatabase.GetPreparedStatement(LoginStatements.LOGIN_UPD_MUTE_TIME_LOGIN);
            stmt.AddValue(0, account.MuteTime);
            stmt.AddValue(1, account.Id);

            DB.Login.Execute(stmt);
        }

        if (account.IsBanned)
        {
            SendAuthResponseError(ResponseCodes.AUTH_BANNED);
            logger.Error(LogFilter.Network, $"WorldSocket::HandleAuthSession: Sent Auth Response (Account banned).");

            DelayedCloseSocket();

            return;
        }

        // Check locked state for server
        AccountTypes allowedAccountType = Global.sWorld.GetPlayerSecurityLimit();
        logger.Error(LogFilter.Network, $"Allowed Level: {allowedAccountType} Player Level {account.Security}");

        if (allowedAccountType > AccountTypes.SEC_PLAYER && account.Security < allowedAccountType)
        {
            SendAuthResponseError(ResponseCodes.AUTH_UNAVAILABLE);
            logger.Error(LogFilter.Network, $"WorldSocket::HandleAuthSession: User tries to login but his security level is not enough");

            DelayedCloseSocket();

            return;
        }

        logger.Debug(LogFilter.Network, $"WorldSocket::HandleAuthSession: Client '{authSession.Account}' authenticated successfully from {address}.");

        // Update the last_ip in the database as it was successful for login
        stmt = LoginDatabase.GetPreparedStatement(LoginStatements.LOGIN_UPD_LAST_IP);
        stmt.AddValue(0, address);
        stmt.AddValue(1, authSession.Account);

        DB.Login.Execute(stmt);

        _authed = true;

        _worldSession = new WorldSession(
                                account.Id,
                                authSession.Account,
                                this,
                                account.Security,
                                account.Expansion,
                                account.MuteTime,
                                account.Locale,
                                account.Recruiter,
                                account.IsRectuiter,
                                account.Security != AccountTypes.SEC_PLAYER,
                                account.TotalTime);

        _worldSession.ReadAddonsInfo(authSession.AddonInfo);

        // Initialize Warden system only if it is enabled by config
        if (wardenActive)
        {
            _worldSession.InitWarden(account.SessionKey, account.OS);
        }

        Global.sWorld.AddSession(_worldSession);

        AsyncRead();
    }

    private ReadDataHandlerResult HandlePing(WorldPacketData recvPacket)
    {
        uint ping = recvPacket.ReadUInt32();
        uint latency = recvPacket.ReadUInt32();

        if (_lastPingTime == DateTime.Now)
        {
            _lastPingTime = DateTime.Now;
        }
        else
        {
            DateTime now = DateTime.Now;
            TimeSpan diff = now - _lastPingTime;

            _lastPingTime = now;

            if (diff < TimeSpan.FromSeconds(27))
            {
                ++_overSpeedPings;

                uint maxAllowed = ConfigMgr.GetOption<uint>("MaxOverspeedPings", 2);

                if (maxAllowed != 0 && _overSpeedPings > maxAllowed)
                {
                    lock (_worldSessionLock)
                    {

                        if (_worldSession != null && AccountMgr.IsPlayerAccount(_worldSession.GetSecurity()))
                        {
                            logger.Error(LogFilter.Network, $"WorldSocket::HandlePing: {_worldSession?.GetPlayerInfo()} kicked for over-speed pings (address: {GetRemoteIpAddress()})");

                            return ReadDataHandlerResult.Error;
                        }
                    }
                }
            }
            else
            {
                _overSpeedPings = 0;
            }
        }

        lock (_worldSessionLock)
        {
            if (_worldSession != null)
            {
                _worldSession.SetLatency(latency);
            }
            else
            {
                logger.Error(LogFilter.Network, $"WorldSocket::HandlePing: peer sent CMSG_PING, but is not authenticated or got recently kicked, address = {GetRemoteIpAddress()}");

                return ReadDataHandlerResult.Error;
            }
        }

        WorldPacketData packet = new(Opcodes.SMSG_PONG);
        packet.WriteUInt(ping);
        SendPacketAndLogOpcode(packet);

        return ReadDataHandlerResult.Ok;
    }

    private void LogOpcodeText(Opcodes opcode)
    {
        logger.Debug(LogFilter.Network, $"Client->Server: {GetRemoteIpAddress()?.ToString()} {Enum.GetName(typeof(Opcodes), opcode)}");
    }

    protected enum ReadDataHandlerResult
    {
        Ok = 0,
        Error = 1,
        WaitingForQuery = 2
    }
}
