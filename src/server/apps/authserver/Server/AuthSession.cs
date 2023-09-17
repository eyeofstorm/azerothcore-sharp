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

using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

using AzerothCore.Auth;
using AzerothCore.Configuration;
using AzerothCore.Constants;
using AzerothCore.Cryptography;
using AzerothCore.Database;
using AzerothCore.Game;
using AzerothCore.Logging;
using AzerothCore.Utilities;

namespace AzerothCore.Networking;

internal struct AccountInfo
{
    internal uint Id = 0;
    internal string? Login;
    internal bool IsLockedToIP;
    internal string? LockCountry;
    internal string? LastIP;
    internal uint FailedLogins;
    internal bool IsBanned;
    internal bool IsPermanentlyBanned;
    internal AccountTypes SecurityLevel;

    public AccountInfo()
    {
        SecurityLevel = AccountTypes.SEC_PLAYER;
    }

    internal void LoadResult(Fields fields)
    {
        //          0        1          2           3             4             5
        // SELECT a.id, a.username, a.locked, a.lock_country, a.last_ip, a.failed_logins,
        //                                 6                                        7
        // ab.unbandate > UNIX_TIMESTAMP() OR ab.unbandate = ab.bandate, ab.unbandate = ab.bandate,
        //                                 8                                             9
        // ipb.unbandate > UNIX_TIMESTAMP() OR ipb.unbandate = ipb.bandate, ipb.unbandate = ipb.bandate,
        //      10
        // aa.gmlevel (, more query-specific fields)
        // FROM account a LEFT JOIN account_access aa ON a.id = aa.id LEFT JOIN account_banned ab ON ab.id = a.id AND ab.active = 1 LEFT JOIN ip_banned ipb ON ipb.ip = ? WHERE a.username = ?

        Id = fields[0].Get<uint>();
        Login = fields[1].Get<string>();
        IsLockedToIP = fields[2].Get<bool>();
        LockCountry = fields[3].Get<string> ();
        LastIP = fields[4].Get<string> ();
        FailedLogins = fields[5].Get<uint>();
        IsBanned = fields[6].Get<bool>() || fields[8].Get<bool>();
        IsPermanentlyBanned = fields[7].Get<bool>() || fields[9].Get<bool>();
        SecurityLevel = fields[10].Get<byte>() > (byte)AccountTypes.SEC_CONSOLE ? AccountTypes.SEC_CONSOLE : (AccountTypes)fields[10].Get<byte>();

        // Use our own uppercasing of the account name instead of using UPPER() in mysql query
        // This is how the account was created in the first place and changing it now would result in breaking
        // login for all accounts having accented characters in their name
        // TODO: authserver: Utf8ToUpperOnlyLatin(Login);
        Login = Login?.ToUpper();
    }
}

internal enum AuthCmd : byte
{
    AUTH_LOGON_CHALLENGE        = 0x00,
    AUTH_LOGON_PROOF            = 0x01,
    AUTH_RECONNECT_CHALLENGE    = 0x02,
    AUTH_RECONNECT_PROOF        = 0x03,
    REALM_LIST                  = 0x10,
    XFER_INITIATE               = 0x30,
    XFER_DATA                   = 0x31,
    XFER_ACCEPT                 = 0x32,
    XFER_RESUME                 = 0x33,
    XFER_CANCEL                 = 0x34
};

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct AUTH_LOGON_CHALLENGE_C
{
    internal byte cmd;

    internal byte error;

    internal ushort size;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    internal byte[] gamename;

    internal byte version1;

    internal byte version2;

    internal byte version3;

    internal ushort build;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
    internal string platform;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
    internal string os;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
    internal string country;

    internal uint timezone_bias;

    internal uint ip;

    internal byte I_len;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
    internal string I;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct AUTH_LOGON_PROOF_C
{
    internal byte cmd;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    internal byte[] A;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    internal byte[] clientM;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    internal byte[] crc_hash;

    internal byte number_of_keys;

    internal byte securityFlags;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct AUTH_LOGON_PROOF_S
{
    internal byte cmd;

    internal byte error;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    internal byte[] M2;

    internal uint AccountFlags;

    internal uint SurveyId;

    internal ushort LoginFlags;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct AUTH_LOGON_PROOF_S_OLD
{
    internal byte cmd;

    internal byte error;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    internal byte[] M2;

    internal uint unk2;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct AUTH_RECONNECT_PROOF_C
{
    internal byte cmd;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    internal byte[] R1;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    internal byte[] R2;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    internal byte[] R3;

    internal byte number_of_keys;
}

internal delegate bool AuthSessionHandler();

internal struct AuthHandler
{
    public AuthHandler(AuthStatus status, int size, AuthSessionHandler handler)
    {
        this.status = status;
        packetSize = size;
        this.handler = handler;
    }

    internal AuthStatus status;
    internal int packetSize;
    internal AuthSessionHandler handler;
}

internal class AuthSession : SocketBase
{
    private static readonly int MAX_ACCEPTED_CHALLENGE_SIZE = Marshal.SizeOf(typeof(AUTH_LOGON_CHALLENGE_C)) + 16;
    private static readonly int AUTH_LOGON_CHALLENGE_INITIAL_SIZE = 4;
    private static readonly int REALM_LIST_PACKET_SIZE = 5;
    private static readonly byte[] VERSION_CHALLENGE = new byte[] { 0xBA, 0xA3, 0x1E, 0x99, 0xA0, 0x0B, 0x21, 0x57, 0xFC, 0x37, 0x3F, 0xB3, 0x69, 0xCD, 0xD2, 0xF1 };

    private Dictionary<AuthCmd, AuthHandler>        _handlers;
    private AuthStatus                              _status;
    private AccountInfo                             _accountInfo;
    private byte[]?                                 _totpSecret;
    private string?                                 _localizationName;
    private string?                                 _os;
    private string?                                 _ipCountry;
    private ushort                                  _build;
    private byte                                    _expversion;
    private SRPServerAuth?                          _srp6;
    private byte[]?                                 _sessionKey;
    private byte[]                                  _reconnectProof = new byte[16];
    private AsyncCallbackProcessor<QueryCallback>   _queryProcessor;

    public AuthSession(Socket socket) : base(socket)
    {
        _queryProcessor = new();

        _handlers = new Dictionary<AuthCmd, AuthHandler>()
        {
            { AuthCmd.AUTH_LOGON_CHALLENGE,     new AuthHandler(AuthStatus.STATUS_CHALLENGE,        AUTH_LOGON_CHALLENGE_INITIAL_SIZE,              HandleLogonChallenge) },
            { AuthCmd.AUTH_LOGON_PROOF,         new AuthHandler(AuthStatus.STATUS_LOGON_PROOF,      Marshal.SizeOf(typeof(AUTH_LOGON_PROOF_C)),     HandleLogonProof) },
            { AuthCmd.AUTH_RECONNECT_CHALLENGE, new AuthHandler(AuthStatus.STATUS_CHALLENGE,        AUTH_LOGON_CHALLENGE_INITIAL_SIZE,              HandleReconnectChallenge) },
            { AuthCmd.AUTH_RECONNECT_PROOF,     new AuthHandler(AuthStatus.STATUS_RECONNECT_PROOF,  Marshal.SizeOf(typeof(AUTH_RECONNECT_PROOF_C)), HandleReconnectProof) },
            { AuthCmd.REALM_LIST,               new AuthHandler(AuthStatus.STATUS_AUTHED,           REALM_LIST_PACKET_SIZE,                         HandleRealmList) },
        };
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
        stmt.SetData(0, ipAddress.Address.ToString());

        QueryCallback asyncQuery = DB.Login.AsyncQuery(stmt);
        asyncQuery.WithCallback(CheckIpCallback);

        _queryProcessor.AddCallback(asyncQuery);
    }

    public override bool Update()
    {
        if (!base.Update())
        {
            return false;
        }

        _queryProcessor.ProcessReadyCallbacks();

        return true;
    }

    private void CheckIpCallback(QueryResult result)
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
                ByteBuffer pkt = new ByteBuffer();

                pkt.WriteByte((byte)AuthCmd.AUTH_LOGON_CHALLENGE);
                pkt.WriteByte(0x00);
                pkt.WriteByte((byte)AuthResult.WOW_FAIL_BANNED);

                SendPacket(pkt);

                logger.Debug(LogFilter.Session, $"[AuthSession::CheckIpCallback] Banned ip '{ipAddress.ToString()}' tries to login!");

                return;
            }
        }

        AsyncRead();
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
            AuthCmd cmd = (AuthCmd)packet.GetBasePointer()[packet.GetReadPos()];

            if (!_handlers.ContainsKey(cmd))
            {
                // well we dont handle this, lets just ignore it
                packet.Reset();
                break;
            }

            logger.Debug(LogFilter.Network, $"[{System.Enum.GetName(cmd)}] packet received");

            AuthHandler handler = _handlers[cmd];

            if (_status != handler.status)
            {
                CloseSocket();
                return;
            }

            int size = handler.packetSize;

            if (packet.GetActiveSize() < size)
            {
                break;
            }

            if (cmd == AuthCmd.AUTH_LOGON_CHALLENGE || cmd == AuthCmd.AUTH_RECONNECT_CHALLENGE)
            {
                AUTH_LOGON_CHALLENGE_C challenge = packet.CastTo<AUTH_LOGON_CHALLENGE_C>();
                size += challenge.size;

                if (size > MAX_ACCEPTED_CHALLENGE_SIZE)
                {
                    CloseSocket();
                    return;
                }
            }

            if (packet.GetActiveSize() < size)
            {
                break;
            }

            if (!handler.handler())
            {
                CloseSocket();
                return;
            }

            packet.ReadCompleted(size);
        }

        AsyncRead();
    }

    private void SendPacket(ByteBuffer packet)
    {
        if (!IsOpen())
        {
            return;
        }

        if (packet.GetSize() > 0)
        {
            MessageBuffer buffer = new MessageBuffer((int)packet.GetSize());
            buffer.Write(packet.GetData(), (int)packet.GetSize());

            QueuePacket(buffer);
        }
    }

    #region handlers
    private bool HandleLogonChallenge()
    {
        _status = AuthStatus.STATUS_CLOSED;

        AUTH_LOGON_CHALLENGE_C challenge = GetReadBuffer().CastTo<AUTH_LOGON_CHALLENGE_C>();

        if (challenge.size - (Marshal.SizeOf(typeof(AUTH_LOGON_CHALLENGE_C)) - AUTH_LOGON_CHALLENGE_INITIAL_SIZE - 1 - 16) != challenge.I_len)
        {
            return false;
        }

        string login = challenge.I;
        logger.Debug(LogFilter.Server, $"[AuthChallenge] '{login}'");

        _build = challenge.build;

        _expversion = (byte)(AuthHelper.IsPostBCAcceptedClientBuild(_build) ?
                                ExpansionFlags.POST_BC_EXP_FLAG :
                                (AuthHelper.IsPreBCAcceptedClientBuild(_build) ?
                                        ExpansionFlags.PRE_BC_EXP_FLAG :
                                        ExpansionFlags.NO_VALID_EXP_FLAG));

        // Restore string order as its byte order is reversed
        _os = new string(challenge.os.Reverse().ToArray());

        char[] nameArr = new string(challenge.country).ToCharArray();

        for (int i = 0; i < 4; i++)
        {
            nameArr[i] = challenge.country[4 - i - 1];
        }

        _localizationName = new string(nameArr);

        // Get the account details from the account table
        var stmt = LoginDatabase.GetPreparedStatement(LoginStatements.LOGIN_SEL_LOGONCHALLENGE);
        stmt.SetData(0, GetRemoteIpAddress()?.Address.ToString());
        stmt.SetData(1, login);

        _queryProcessor.AddCallback(DB.Login.AsyncQuery(stmt).WithCallback(LogonChallengeCallback));

        return true;
    }

    private void LogonChallengeCallback(QueryResult result)
    {
        ByteBuffer pkt = new ByteBuffer();

        pkt.WriteByte((byte)AuthCmd.AUTH_LOGON_CHALLENGE);
        pkt.WriteByte(0x00);

        if (result.IsEmpty())
        {
            pkt.WriteByte((byte)AuthResult.WOW_FAIL_UNKNOWN_ACCOUNT);
            SendPacket(pkt);
            return;
        }

        Fields fields = result.Fetch();
        _accountInfo.LoadResult(fields);

        string? ipAddress = GetRemoteIpAddress()?.Address.ToString();
        int? port = GetRemoteIpAddress()?.Port;

        if (_accountInfo.IsLockedToIP)
        {
            logger.Debug(LogFilter.Server, $"[AuthChallenge] Account '{_accountInfo.Login}' is locked to IP - '{_accountInfo.LastIP}' is logging in from '{ipAddress}'");

            if (_accountInfo.LastIP != ipAddress)
            {
                pkt.WriteByte((byte)AuthResult.WOW_FAIL_LOCKED_ENFORCED);
                SendPacket(pkt);
                return;
            }
        }
        else
        {
            // TODO: authserver: IPLocation
            _ipCountry = null;

            //IPLocation->GetLocationRecord(ipAddress)
            //if (IpLocationRecord const* location = sIPLocation->GetLocationRecord(ipAddress))
            //{
            //    _ipCountry = location->CountryCode;
            //}

            logger.Debug(LogFilter.Server, $"[AuthChallenge] Account '{_accountInfo.Login}' is not locked to ip");

            if (_accountInfo.LockCountry == null || _accountInfo.LockCountry.IsEmpty() || _accountInfo.LockCountry == "00")
            {
                logger.Debug(LogFilter.Server, $"[AuthChallenge] Account '{_accountInfo.Login}' is not locked to country");
            }
            else if (!_ipCountry.IsEmpty())
            {
                logger.Debug(LogFilter.Server, $"[AuthChallenge] Account '{_accountInfo.Login}' is locked to country: '{_accountInfo.LockCountry}' Player country is '{_ipCountry}'");

                if (_ipCountry != _accountInfo.LockCountry)
                {
                    pkt.WriteByte((byte)AuthResult.WOW_FAIL_UNLOCKABLE_LOCK);
                    SendPacket(pkt);
                    return;
                }
            }
        }

        // If the account is banned, reject the logon attempt
        if (_accountInfo.IsBanned)
        {
            if (_accountInfo.IsPermanentlyBanned)
            {
                pkt.WriteByte((byte)AuthResult.WOW_FAIL_BANNED);
                SendPacket(pkt);
                logger.Debug(LogFilter.Server, $"'{ipAddress}:{port}' [AuthChallenge] Banned account {_accountInfo.Login} tried to login!");
                return;
            }
            else
            {
                pkt.WriteByte((byte)AuthResult.WOW_FAIL_SUSPENDED);
                SendPacket(pkt);
                logger.Debug(LogFilter.Server, $"'{ipAddress}:{port}' [AuthChallenge] Temporarily banned account {_accountInfo.Login} tried to login!");
                return;
            }
        }

        byte securityFlags = 0;

        // Check if a TOTP token is needed
        if (!fields[11].IsNull())
        {
            securityFlags = 4;
            _totpSecret = result.ReadBytes(11, 128);

            // TODO: authserver: check TOTP token
            //if (auto const&secret = sSecretMgr->GetSecret(SECRET_TOTP_MASTER_KEY))
            //{
            //    bool success = Acore::Crypto::AEDecrypt<Acore::Crypto::AES>(*_totpSecret, *secret);
            //    if (!success)
            //    {
            //        pkt << uint8(WOW_FAIL_DB_BUSY);
            //        LOG_ERROR("server.authserver", "[AuthChallenge] Account '{}' has invalid ciphertext for TOTP token key stored", _accountInfo.Login);
            //        SendPacket(pkt);
            //        return;
            //    }
            //}
        }

        string accountName = _accountInfo.Login ?? string.Empty;
        byte[]? salt       = result.ReadBytes(12, 32) ?? Array.Empty<byte>();
        byte[]? verifier   = result.ReadBytes(13, 32) ?? Array.Empty<byte>();

        _srp6 = new SRPServerAuth(accountName, salt, verifier);

        if (AuthHelper.IsAcceptedClientBuild(_build))
        {
            pkt.WriteByte((byte)AuthResult.WOW_SUCCESS);
            
            pkt.WriteSrpInteger(_srp6.ServerPublicKey);
            pkt.WriteByte(1);
            pkt.WriteSrpInteger(SRPServerAuth.Generator);
            pkt.WriteByte(32);
            pkt.WriteSrpInteger(SRPServerAuth.SafePrime);
            pkt.WriteSrpInteger(_srp6.Salt);
            pkt.WriteBytes(VERSION_CHALLENGE);
            pkt.WriteByte(securityFlags);          // security flags (0x0...0x04)

            if ((securityFlags & 0x01) != 0x00)     // PIN input
            {
                pkt.WriteUInt((uint)0);
                pkt.WriteUInt64(0);
                pkt.WriteUInt64(0);                 // 16 bytes hash?
            }

            if ((securityFlags & 0x02) != 0x00)     // Matrix input
            {
                pkt.WriteByte(0);
                pkt.WriteByte(0);
                pkt.WriteByte(0);
                pkt.WriteByte(0);
                pkt.WriteUInt64(0);
            }

            if ((securityFlags & 0x04) != 0x00)     // Security token input
            {
                pkt.WriteByte(1);
            }

            logger.Debug(LogFilter.Server, $"'{ipAddress}:{port}' [AuthChallenge] account {_accountInfo.Login} is using '{_localizationName}' locale ({LocaleHelper.GetLocaleByName(_localizationName ?? "enUS")})");

            _status = AuthStatus.STATUS_LOGON_PROOF;
        }
        else
        {
            pkt.WriteByte((byte)AuthResult.WOW_FAIL_VERSION_INVALID);
        }

        SendPacket(pkt);
    }

    private bool HandleLogonProof()
    {
        logger.Debug(LogFilter.Server, $"Entering _HandleLogonProof");
        _status = AuthStatus.STATUS_CLOSED;

        // Read the packet
        AUTH_LOGON_PROOF_C logonProof = GetReadBuffer().CastTo<AUTH_LOGON_PROOF_C>();

        // If the client has no valid version
        if (_expversion == (byte)ExpansionFlags.NO_VALID_EXP_FLAG)
        {
            // Check if we have the appropriate patch on the disk
            logger.Debug(LogFilter.Network, $"Client with invalid version, patching is not implemented");
            return false;
        }

        IPEndPoint? ipAddress = GetRemoteIpAddress();

        if (ipAddress == null)
        {
            return false;
        }

        byte[]? k = _srp6?.VerifyChallengeResponse(logonProof.A, logonProof.clientM);

        if (k != null)
        {
            _sessionKey = k;

            // Check auth token
            bool tokenSuccess = false;
            bool sentToken = (logonProof.securityFlags & 0x04) != 0x00;

            if (sentToken && _totpSecret != null)
            {
                // TODO: authserver: Validate TOTP token.
                //uint8 size = *(GetReadBuffer().GetReadPointer() + sizeof(sAuthLogonProof_C));
                //std::string token(reinterpret_cast<char*>(GetReadBuffer().GetReadPointer() +sizeof(sAuthLogonProof_C) + sizeof(size)), size);
                //GetReadBuffer().ReadCompleted(sizeof(size) + size);

                //uint32 incomingToken = *Acore::StringTo<uint32>(token);
                //tokenSuccess = Acore::Crypto::TOTP::ValidateToken(*_totpSecret, incomingToken);
                //memset(_totpSecret->data(), 0, _totpSecret->size());
            }
            else if (!sentToken && _totpSecret == null)
            {
                tokenSuccess = true;
            }

            ByteBuffer packet;

            if (!tokenSuccess)
            {
                packet = new ByteBuffer();

                packet.WriteByte((byte)AuthCmd.AUTH_LOGON_PROOF);
                packet.WriteByte((byte)AuthResult.WOW_FAIL_UNKNOWN_ACCOUNT);
                packet.WriteUShort(0);    // LoginFlags, 1 has account message

                SendPacket(packet);

                return true;
            }

            if (!VerifyVersion(logonProof.A, logonProof.crc_hash, false))
            {
                packet = new ByteBuffer();

                packet.WriteByte((byte)AuthCmd.AUTH_LOGON_PROOF);
                packet.WriteByte((byte)AuthResult.WOW_FAIL_VERSION_INVALID);

                SendPacket(packet);

                return true;
            }

            logger.Debug(LogFilter.Server, $"'{ipAddress.Address}:{ipAddress.Port}' User '{_accountInfo.Login}' successfully authenticated");

            // Update the sessionkey, last_ip, last login time and reset number of failed logins in the account table for this account
            // No SQL injection (escaped user name) and IP address as received by socket
            string address = ConfigMgr.GetOption<bool>("AllowLoggingIPAddressesInDatabase", true) ? ipAddress.Address.ToString() : "0.0.0.0";

            PreparedStatement stmt = LoginDatabase.GetPreparedStatement(LoginStatements.LOGIN_UPD_LOGONPROOF);
            stmt.SetData(0, _sessionKey);
            stmt.SetData(1, address);
            stmt.SetData(2, (byte)LocaleHelper.GetLocaleByName(_localizationName));
            stmt.SetData(3, _os);
            stmt.SetData(4, _accountInfo.Login);

            DB.Login.DirectExecute(stmt);

            // Send response packet
            packet = new ByteBuffer();

            byte[] M2 = SRPServerAuth.ComputeM2(logonProof.A, logonProof.clientM, _sessionKey);

            if ((_expversion & (byte)ExpansionFlags.POST_BC_EXP_FLAG) != 0x00)                 // 2.x and 3.x clients
            {
                AUTH_LOGON_PROOF_S proof = new AUTH_LOGON_PROOF_S();

                proof.M2 = M2;
                proof.cmd = (byte)AuthCmd.AUTH_LOGON_PROOF;
                proof.error = 0;
                proof.AccountFlags = 0x00800000;    // 0x01 = GM, 0x08 = Trial, 0x00800000 = Pro pass (arena tournament)
                proof.SurveyId = 0;
                proof.LoginFlags = 0;               // 0x1 = has account message

                packet.WriteBytes(proof.ToByteArray());
            }
            else
            {
                AUTH_LOGON_PROOF_S_OLD proof = new AUTH_LOGON_PROOF_S_OLD();

                proof.M2 = M2;
                proof.cmd = (byte)AuthCmd.AUTH_LOGON_PROOF;
                proof.error = 0;
                proof.unk2 = 0x00;

                packet.WriteBytes(proof.ToByteArray());
            }

            SendPacket(packet);

            _status = AuthStatus.STATUS_AUTHED;
        }
        else
        {
            ByteBuffer packet = new();

            packet.WriteByte((byte)AuthCmd.AUTH_LOGON_PROOF);
            packet.WriteByte((byte)AuthResult.WOW_FAIL_UNKNOWN_ACCOUNT);
            packet.WriteUShort(0);    // LoginFlags, 1 has account message

            SendPacket(packet);

            logger.Info(LogFilter.Server, $"'{ipAddress.Address.ToString()}:{ipAddress.Port}' [AuthChallenge] account {_accountInfo.Login} tried to login with invalid password!");

            uint maxWrongPassCount = ConfigMgr.GetOption<UInt32>("WrongPass.MaxCount", 0);

            // We can not include the failed account login hook. However, this is a workaround to still log this.
            if (ConfigMgr.GetOption<bool>("WrongPass.Logging", false))
            {
                PreparedStatement logstmt = LoginDatabase.GetPreparedStatement(LoginStatements.LOGIN_INS_FALP_IP_LOGGING);
                logstmt.SetData(0, _accountInfo.Id);
                logstmt.SetData(1, ipAddress.Address.ToString());
                logstmt.SetData(2, "Login to WoW Failed - Incorrect Password");

                DB.Login.Execute(logstmt);
            }

            if (maxWrongPassCount > 0)
            {
                //Increment number of failed logins by one and if it reaches the limit temporarily ban that account or IP
                PreparedStatement stmt = LoginDatabase.GetPreparedStatement(LoginStatements.LOGIN_UPD_FAILEDLOGINS);
                stmt.SetData(0, _accountInfo.Login);
                DB.Login.Execute(stmt);

                if (++_accountInfo.FailedLogins >= maxWrongPassCount)
                {
                    uint wrongPassBanTime = ConfigMgr.GetOption<UInt32>("WrongPass.BanTime", 600);
                    bool wrongPassBanType = ConfigMgr.GetOption<bool>("WrongPass.BanType", false);

                    if (wrongPassBanType)
                    {
                        stmt = LoginDatabase.GetPreparedStatement(LoginStatements.LOGIN_INS_ACCOUNT_AUTO_BANNED);
                        stmt.SetData(0, _accountInfo.Id);
                        stmt.SetData(1, wrongPassBanTime);
                        DB.Login.Execute(stmt);

                        logger.Debug(LogFilter.Server, $"'{ipAddress.Address.ToString()}:{ipAddress.Port}' [AuthChallenge] account {_accountInfo.Login} got banned for '{wrongPassBanTime}' seconds because it failed to authenticate '{_accountInfo.FailedLogins}' times");
                    }
                    else
                    {
                        stmt = LoginDatabase.GetPreparedStatement(LoginStatements.LOGIN_INS_IP_AUTO_BANNED);
                        stmt.SetData(0, ipAddress.Address.ToString());
                        stmt.SetData(1, wrongPassBanTime);
                        DB.Login.Execute(stmt);

                        logger.Debug(LogFilter.Server, $"'{ipAddress.Address.ToString()}:{ipAddress.Port}' [AuthChallenge] IP got banned for '{wrongPassBanTime}' seconds because account {_accountInfo.Login} failed to authenticate '{_accountInfo.FailedLogins}' times");
                    }
                }
            }
        }

        return true;
    }

    private bool HandleReconnectChallenge()
    {
        logger.Debug(LogFilter.Server, "Entering _HandleReconnectChallenge");

        _status = AuthStatus.STATUS_CLOSED;

        AUTH_LOGON_CHALLENGE_C challenge = GetReadBuffer().CastTo<AUTH_LOGON_CHALLENGE_C>();

        if (challenge.size - (Marshal.SizeOf(typeof(AUTH_LOGON_CHALLENGE_C)) - AUTH_LOGON_CHALLENGE_INITIAL_SIZE - 1 - 16) != challenge.I_len)
        {
            return false;
        }

        string login = challenge.I;
        logger.Debug(LogFilter.Server, $"[ReconnectChallenge] '{login}'");

        _build = challenge.build;
        _expversion = (byte)(AuthHelper.IsPostBCAcceptedClientBuild(_build) ?
                                ExpansionFlags.POST_BC_EXP_FLAG :
                                (AuthHelper.IsPreBCAcceptedClientBuild(_build) ?
                                        ExpansionFlags.PRE_BC_EXP_FLAG :
                                        ExpansionFlags.NO_VALID_EXP_FLAG));

        // Restore string order as its byte order is reversed
        _os = challenge.os.Reverse().ToString();

        char[] nameArr = new string(challenge.country).ToCharArray();

        for (int i = 0; i < 4; i++)
        {
            nameArr[i] = challenge.country[4 - i - 1];
        }

        _localizationName = new string(nameArr);

        // Get the account details from the account table
        PreparedStatement stmt = LoginDatabase.GetPreparedStatement(LoginStatements.LOGIN_SEL_RECONNECTCHALLENGE);
        stmt.SetData(0, GetRemoteIpAddress()?.Address.ToString());
        stmt.SetData(1, login);

        _queryProcessor.AddCallback(DB.Login.AsyncQuery(stmt).WithCallback(ReconnectChallengeCallback));

        return true;
    }

    private void ReconnectChallengeCallback(QueryResult result)
    {
        ByteBuffer pkt = new ByteBuffer();

        pkt.WriteByte((byte)AuthCmd.AUTH_RECONNECT_CHALLENGE);

        if (result.IsEmpty())
        {
            pkt.WriteByte((byte)AuthResult.WOW_FAIL_UNKNOWN_ACCOUNT);

            SendPacket(pkt);

            return;
        }

        Fields fields = result.Fetch();

        _accountInfo.LoadResult(fields);
        _sessionKey = result.ReadBytes(11, 40);
        RandomHelper.NextBytes(_reconnectProof);
        _status = AuthStatus.STATUS_RECONNECT_PROOF;

        pkt.WriteByte((byte)AuthResult.WOW_SUCCESS);
        pkt.WriteBytes(_reconnectProof);
        pkt.WriteBytes(VERSION_CHALLENGE);

        SendPacket(pkt);
    }

    private bool HandleReconnectProof()
    {
        logger.Debug(LogFilter.Server, "Entering _HandleReconnectProof");

        _status = AuthStatus.STATUS_CLOSED;

        AUTH_RECONNECT_PROOF_C reconnectProof = GetReadBuffer().CastTo<AUTH_RECONNECT_PROOF_C>();

        if (_accountInfo.Login == null)
        {
            return false;
        }

        SrpInteger t1 = SrpInteger.FromByteArray(reconnectProof.R1);

        SHA1 sha = SHA1.Create();
        sha.Update(System.Text.Encoding.UTF8.GetBytes(_accountInfo.Login));
        sha.Update(t1.ToByteArray());
        sha.Update(_reconnectProof);
        sha.Final(_sessionKey);
        byte[] digest = sha.Hash;

        if (digest.SequenceEqual(reconnectProof.R2))
        {
            if (!VerifyVersion(reconnectProof.R1, reconnectProof.R3, true))
            {
                ByteBuffer pkt = new ByteBuffer();

                pkt.WriteByte((byte)AuthCmd.AUTH_RECONNECT_PROOF);
                pkt.WriteByte((byte)AuthResult.WOW_FAIL_VERSION_INVALID);

                SendPacket(pkt);

                return true;
            }
            else
            {
                // Sending response
                ByteBuffer pkt = new ByteBuffer();

                pkt.WriteByte((byte)AuthCmd.AUTH_RECONNECT_PROOF);
                pkt.WriteByte((byte)AuthResult.WOW_SUCCESS);
                pkt.WriteUShort(0);             // LoginFlags, 1 has account message

                SendPacket(pkt);
            }

            _status = AuthStatus.STATUS_AUTHED;

            return true;
        }
        else
        {
            IPEndPoint? ipAddress = GetRemoteIpAddress();

            if (ipAddress == null)
            {
                return false;
            }

            logger.Error(LogFilter.Server, $"'{ipAddress.Address.ToString()}:{ipAddress.Port}' [ERROR] user {_accountInfo.Login} tried to login, but session is invalid.");

            return false;
        }
    }

    private bool HandleRealmList()
    {
        logger.Debug(LogFilter.Server, "Entering _HandleRealmList");

        _status = AuthStatus.STATUS_WAITING_FOR_REALM_LIST;

        PreparedStatement stmt = LoginDatabase.GetPreparedStatement(LoginStatements.LOGIN_SEL_REALM_CHARACTER_COUNTS);
        stmt.SetData(0, _accountInfo.Id);

        _queryProcessor.AddCallback(DB.Login.AsyncQuery(stmt).WithCallback(RealmListCallback));

        return true;
    }

    private void RealmListCallback(QueryResult result)
    {
        Dictionary<UInt32, byte> characterCounts = new Dictionary<uint, byte>();

        if (!result.IsEmpty())
        {
            do
            {
                characterCounts[result.Read<UInt32>(0)] = result.Read<byte>(1);
            }
            while (result.NextRow());
        }

        // Circle through realms in the RealmList and construct the return packet (including # of user characters in each realm)
        ByteBuffer pkt = new ByteBuffer();

        int realmListSize = 0;

        foreach (Realm realm in RealmList.Instance.GetRealms())
        {
            // don't work with realms which not compatible with the client
            bool okBuild = ((_expversion & (byte)ExpansionFlags.POST_BC_EXP_FLAG) == (byte)ExpansionFlags.POST_BC_EXP_FLAG && realm.Build == _build) ||
                           ((_expversion & (byte)ExpansionFlags.PRE_BC_EXP_FLAG) == (byte)ExpansionFlags.PRE_BC_EXP_FLAG && !AuthHelper.IsPreBCAcceptedClientBuild(realm.Build));

            // No SQL injection. id of realm is controlled by the database.
            RealmFlags flag = realm.Flags;
            RealmBuildInfo? buildInfo = RealmList.Instance.GetBuildInfo(realm.Build);

            if (buildInfo == null)
            {
                continue;
            }

            if (!okBuild)
            {
                flag |= RealmFlags.REALM_FLAG_OFFLINE | RealmFlags.REALM_FLAG_SPECIFYBUILD;   // tell the client what build the realm is for
            }

            flag &= ~RealmFlags.REALM_FLAG_SPECIFYBUILD;

            string name = realm.Name;

            if ((_expversion & (byte)ExpansionFlags.PRE_BC_EXP_FLAG) == (byte)ExpansionFlags.PRE_BC_EXP_FLAG &&
                (flag & RealmFlags.REALM_FLAG_SPECIFYBUILD) == RealmFlags.REALM_FLAG_SPECIFYBUILD)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(name).Append(" (").Append(buildInfo?.MajorVersion).Append('.').Append(buildInfo?.MinorVersion).Append('.').Append(buildInfo?.BugfixVersion).Append(')');
                name = sb.ToString();
            }

            byte lockFlg = (realm.AllowedSecurityLevel > _accountInfo.SecurityLevel) ? (byte)0x01 : (byte)0x00;

            pkt.WriteByte(realm.Type);                         // realm type

            if ((_expversion & (byte)ExpansionFlags.POST_BC_EXP_FLAG) == (byte)ExpansionFlags.POST_BC_EXP_FLAG)                 // only 2.x and 3.x clients
            {
                pkt.WriteByte(lockFlg);                        // if 1, then realm locked
            }

            pkt.WriteByte((byte)flag);                         // RealmFlags
            pkt.WriteCString(name);
            pkt.WriteCString(realm.GetAddressForClient(GetRemoteIpAddress()?.Address).ToString());
            pkt.WriteFloat(realm.PopulationLevel);
            pkt.WriteByte(characterCounts[realm.Id.Index]);
            pkt.WriteByte(realm.Timezone);                     // realm category

            if ((_expversion & (byte)ExpansionFlags.POST_BC_EXP_FLAG) == (byte)ExpansionFlags.POST_BC_EXP_FLAG)                 // 2.x and 3.x clients
            {
                pkt.WriteByte((byte)realm.Id.Index);
            }
            else
            {
                pkt.WriteByte(0x0);                            // 1.12.1 and 1.12.2 clients
            }

            if ((_expversion & (byte)ExpansionFlags.POST_BC_EXP_FLAG) == (byte)ExpansionFlags.POST_BC_EXP_FLAG &&
                (flag & RealmFlags.REALM_FLAG_SPECIFYBUILD) == RealmFlags.REALM_FLAG_SPECIFYBUILD &&
                buildInfo != null)
            {
                pkt.WriteByte((byte)buildInfo.MajorVersion);
                pkt.WriteByte((byte)buildInfo.MinorVersion);
                pkt.WriteByte((byte)buildInfo.BugfixVersion);
                pkt.WriteUShort((ushort)buildInfo.Build);
            }

            ++realmListSize;
        }

        if ((_expversion & (byte)ExpansionFlags.POST_BC_EXP_FLAG) == (byte)ExpansionFlags.POST_BC_EXP_FLAG)                     // 2.x and 3.x clients
        {
            pkt.WriteByte(0x10);
            pkt.WriteByte(0x00);
        }
        else                                                    // 1.12.1 and 1.12.2 clients
        {
            pkt.WriteByte(0x00);
            pkt.WriteByte(0x02);
        }

        // make a ByteBuffer which stores the RealmList's size
        ByteBuffer realmListSizeBuffer = new ByteBuffer();
        realmListSizeBuffer.WriteUInt((uint)0);

        if ((_expversion & (byte)ExpansionFlags.POST_BC_EXP_FLAG) == (byte)ExpansionFlags.POST_BC_EXP_FLAG)                     // only 2.x and 3.x clients
        {
            realmListSizeBuffer.WriteUShort((ushort)realmListSize);
        }
        else
        {
            realmListSizeBuffer.WriteUInt((uint)realmListSize);
        }

        ByteBuffer hdr = new ByteBuffer();

        hdr.WriteByte((byte)AuthCmd.REALM_LIST);
        hdr.WriteUShort((ushort)(pkt.GetSize() + realmListSizeBuffer.GetSize()));
        hdr.WriteBytes(realmListSizeBuffer.GetData());          // append RealmList's size buffer
        hdr.WriteBytes(pkt.GetData());                          // append realms in the realmlist

        SendPacket(hdr);

        _status = AuthStatus.STATUS_AUTHED;
    }
    #endregion

    private bool VerifyVersion(byte[] clientPublicKey, byte[] versionProof, bool isReconnect)
    {
        if (!ConfigMgr.GetOption<bool>("StrictVersionCheck", false))
        {
            return true;
        }

        byte[]? zeros = new byte[20];
        byte[]? versionHash = null;

        if (!isReconnect)
        {
            RealmBuildInfo? buildInfo = RealmList.Instance?.GetBuildInfo(_build);

            if (buildInfo == null)
            {
                return false;
            }

            if (_os == "Win")
            {
                versionHash = buildInfo.Win64AuthSeed;
            }
            else if (_os == "OSX")
            {
                versionHash = buildInfo.Mac64AuthSeed;
            }

            if (versionHash == null)
            {
                return false;
            }

            if (zeros.SequenceEqual(versionHash))
            {
                return true;        // not filled serverside
            }
        }
        else
        {
            versionHash = zeros;
        }

        SHA1 sha1 = SHA1.Create();
        sha1.Update(clientPublicKey);
        sha1.Final(versionHash);
        byte[] version = sha1.Hash;

        return versionProof.SequenceEqual(version);
    }
}
