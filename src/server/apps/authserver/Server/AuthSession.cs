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
using System.Numerics;
using System.Runtime.InteropServices;

using AzerothCore.Auth;
using AzerothCore.Constants;
using AzerothCore.Cryptography;
using AzerothCore.Database;
using AzerothCore.Logging;
using AzerothCore.Utilities;

namespace AzerothCore.Networking;

public struct AccountInfo
{
    public uint Id = 0;
    public string? Login;
    public bool IsLockedToIP;
    public string? LockCountry;
    public string? LastIP;
    public uint FailedLogins;
    public bool IsBanned;
    public bool IsPermanentlyBanned;
    public AccountTypes SecurityLevel = AccountTypes.SEC_PLAYER;

    public AccountInfo()
    {
    }

    public void LoadResult(SQLFields fields)
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

        Id = fields.Read<UInt32>(0);
        Login = fields.Read<string>(1);
        IsLockedToIP = fields.Read<bool>(2);
        LockCountry = fields.Read<string> (3);
        LastIP = fields.Read<string> (4);
        FailedLogins = fields.Read<uint>(5);
        IsBanned = fields.Read<bool>(6) || fields.Read<bool>(8);
        IsPermanentlyBanned = fields.Read<bool>(7) || fields.Read<bool>(9);
        SecurityLevel = fields.Read<byte>(10) > (byte)AccountTypes.SEC_CONSOLE ? AccountTypes.SEC_CONSOLE : (AccountTypes)fields.Read<byte>(10);

        // Use our own uppercasing of the account name instead of using UPPER() in mysql query
        // This is how the account was created in the first place and changing it now would result in breaking
        // login for all accounts having accented characters in their name
        // TODO: Utf8ToUpperOnlyLatin(Login);
    }
}

public enum AuthCmd : byte
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
public struct AUTH_LOGON_CHALLENGE_C
{
    public byte cmd;

    public byte error;

    public ushort size;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] gamename;

    public byte version1;

    public byte version2;

    public byte version3;

    public ushort build;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
    public string platform;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
    public string os;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
    public string country;

    public uint timezone_bias;

    public uint ip;

    public byte I_len;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
    public string I;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct AUTH_LOGON_PROOF_C
{
    public byte cmd;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public byte[] A;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] clientM;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] crc_hash;

    public byte number_of_keys;

    public byte securityFlags;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct AUTH_RECONNECT_PROOF_C
{
    byte cmd;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] R1;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] R2;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] R3;

    byte number_of_keys;
}

public delegate bool AuthSessionHandler();

public struct AuthHandler
{
    public AuthHandler(AuthStatus status, int size, AuthSessionHandler handler)
    {
        this.status = status;
        this.packetSize = size;
        this.handler = handler;
    }

    public AuthStatus status;
    public int packetSize;
    public AuthSessionHandler handler;
}

public class AuthSession : SocketBase
{
    private static readonly int MAX_ACCEPTED_CHALLENGE_SIZE = Marshal.SizeOf(typeof(AUTH_LOGON_CHALLENGE_C)) + 16;
    private static readonly int AUTH_LOGON_CHALLENGE_INITIAL_SIZE = 4;
    private static readonly int REALM_LIST_PACKET_SIZE = 5;

    private static readonly ILogger                 logger = LoggerFactory.GetLogger();

    private Dictionary<AuthCmd, AuthHandler>        _handlers;
    private AuthStatus                              _status;
    private AccountInfo                             _accountInfo;
    private byte[]?                                 _totpSecret;
    private string?                                 _localizationName;
    private string?                                 _os;
    private string?                                 _ipCountry;
    private ushort                                  _build;
    private byte                                    _expversion;
    private SrpServerAuth?                          _srp6;
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
        stmt.AddValue(0, ipAddress.Address.ToString());

        _queryProcessor.AddCallback(DB.Login.AsyncQuery(stmt).WithCallback(CheckIpCallback));
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
                ByteBuffer pkt = new ByteBuffer();
                pkt.WriteUInt8((byte)AuthCmd.AUTH_LOGON_CHALLENGE);
                pkt.WriteUInt8(0x00);
                pkt.WriteUInt8((byte)AuthResult.WOW_FAIL_BANNED);

                SendPacket(pkt);

                logger.Debug(LogFilter.Session, $"[AuthSession::CheckIpCallback] Banned ip '{ipAddress.ToString()}' tries to login!");

                return;
            }
        }

        AsyncRead();
    }

    public override void ReadHandler()
    {
        MessageBuffer packet = GetReadBuffer();

        while (packet.GetActiveSize() > 0)
        {
            AuthCmd cmd = (AuthCmd)packet.GetReadPointer()[0];

            if (!_handlers.ContainsKey(cmd))
            {
                // well we dont handle this, lets just ignore it
                packet.Reset();
                break;
            }

            logger.Debug(LogFilter.Network, $"[{Enum.GetName(cmd)}] packet received");

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
                AUTH_LOGON_CHALLENGE_C challenge = packet.GetReadPointer().CastTo<AUTH_LOGON_CHALLENGE_C>();
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

            AsyncWrite(buffer.GetBasePointer().ToArray());
        }
    }

    #region handlers
    private bool HandleLogonChallenge()
    {
        _status = AuthStatus.STATUS_CLOSED;

        AUTH_LOGON_CHALLENGE_C challenge = GetReadBuffer().GetReadPointer().CastTo<AUTH_LOGON_CHALLENGE_C>();

        if (challenge.size - (Marshal.SizeOf(typeof(AUTH_LOGON_CHALLENGE_C)) - AUTH_LOGON_CHALLENGE_INITIAL_SIZE - 1) != challenge.I_len)
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
        _os = challenge.os.Reverse().ToString();

        char[] nameArr = new string(challenge.country).ToCharArray();

        for (int i = 0; i < 4; i++)
        {
            nameArr[i] = challenge.country[4 - i - 1];
        }

        _localizationName = new string(nameArr);

        // Get the account details from the account table
        var stmt = LoginDatabase.GetPreparedStatement(LoginStatements.LOGIN_SEL_LOGONCHALLENGE);
        stmt.AddValue(0, GetRemoteIpAddress()?.Address.ToString());
        stmt.AddValue(1, login);

        _queryProcessor.AddCallback(DB.Login.AsyncQuery(stmt).WithCallback(LogonChallengeCallback));

        return true;
    }

    private void LogonChallengeCallback(SQLResult result)
    {
        ByteBuffer pkt = new ByteBuffer();
        pkt.WriteUInt8((byte)AuthCmd.AUTH_LOGON_CHALLENGE);
        pkt.WriteUInt8(0x00);

        if (result.IsEmpty())
        {
            pkt.WriteUInt8((byte)AuthResult.WOW_FAIL_UNKNOWN_ACCOUNT);
            SendPacket(pkt);
            return;
        }

        SQLFields fields = result.GetFields();
        _accountInfo.LoadResult(fields);

        string? ipAddress = GetRemoteIpAddress()?.Address.ToString();
        int? port = GetRemoteIpAddress()?.Port;

        if (_accountInfo.IsLockedToIP)
        {
            logger.Debug(LogFilter.Server, $"[AuthChallenge] Account '{_accountInfo.Login}' is locked to IP - '{_accountInfo.LastIP}' is logging in from '{ipAddress}'");

            if (_accountInfo.LastIP != ipAddress)
            {
                pkt.WriteUInt8((byte)AuthResult.WOW_FAIL_LOCKED_ENFORCED);
                SendPacket(pkt);
                return;
            }
        }
        else
        {
            // TODO: sIPLocation->GetLocationRecord(ipAddress)
            _ipCountry = null;
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
                    pkt.WriteUInt8((byte)AuthResult.WOW_FAIL_UNLOCKABLE_LOCK);
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
                pkt.WriteUInt8((byte)AuthResult.WOW_FAIL_BANNED);
                SendPacket(pkt);
                logger.Debug(LogFilter.Server, $"'{ipAddress}:{port}' [AuthChallenge] Banned account {_accountInfo.Login} tried to login!");
                return;
            }
            else
            {
                pkt.WriteUInt8((byte)AuthResult.WOW_FAIL_SUSPENDED);
                SendPacket(pkt);
                logger.Debug(LogFilter.Server, $"'{ipAddress}:{port}' [AuthChallenge] Temporarily banned account {_accountInfo.Login} tried to login!");
                return;
            }
        }

        byte securityFlags = 0;

        // Check if a TOTP token is needed
        if (!fields.IsNull(11))
        {
            securityFlags = 4;
            _totpSecret = result.ReadBytes(11, 128);

            // TODO: check TOTP token
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

        _srp6 = new SrpServerAuth(accountName, salt, verifier);

        if (AuthHelper.IsAcceptedClientBuild(_build))
        {
            pkt.WriteUInt8((byte)AuthResult.WOW_SUCCESS);
            
            pkt.WriteSrpInteger(_srp6.ServerPublicKey);
            pkt.WriteUInt8(1);
            pkt.WriteSrpInteger(SrpServerAuth.Generator);
            pkt.WriteUInt8(32);
            pkt.WriteSrpInteger(SrpServerAuth.SafePrime);
            pkt.WriteSrpInteger(_srp6.Salt);
            pkt.WriteBytes(new byte[] { 0xBA, 0xA3, 0x1E, 0x99, 0xA0, 0x0B, 0x21, 0x57, 0xFC, 0x37, 0x3F, 0xB3, 0x69, 0xCD, 0xD2, 0xF1 });
            pkt.WriteUInt8(securityFlags);          // security flags (0x0...0x04)

            if ((securityFlags & 0x01) != 0x00)     // PIN input
            {
                pkt.WriteUInt32(0);
                pkt.WriteUInt64(0);
                pkt.WriteUInt64(0);                 // 16 bytes hash?
            }

            if ((securityFlags & 0x02) != 0x00)     // Matrix input
            {
                pkt.WriteUInt8(0);
                pkt.WriteUInt8(0);
                pkt.WriteUInt8(0);
                pkt.WriteUInt8(0);
                pkt.WriteUInt64(0);
            }

            if ((securityFlags & 0x04) != 0x00)     // Security token input
            {
                pkt.WriteUInt8(1);
            }

            logger.Debug(LogFilter.Server, $"'{ipAddress}:{port}' [AuthChallenge] account {_accountInfo.Login} is using '{_localizationName}' locale ({LocaleHelper.GetLocaleByName(_localizationName ?? "enUS")})");

            _status = AuthStatus.STATUS_LOGON_PROOF;
        }
        else
        {
            pkt.WriteUInt8((byte)AuthResult.WOW_FAIL_VERSION_INVALID);
        }

        SendPacket(pkt);
    }

    private bool HandleLogonProof()
    {
        // TODO: AuthSession.HandleLogonProof()
        return false;
    }

    private bool HandleReconnectChallenge()
    {
        // TODO: AuthSession.HandleReconnectChallenge()
        return false;
    }

    private bool HandleReconnectProof()
    {
        // TODO: AuthSession.HandleReconnectProof()
        return false;
    }

    private bool HandleRealmList()
    {
        // TODO: AuthSession.HandleRealmList()
        return false;
    }
    #endregion
}
