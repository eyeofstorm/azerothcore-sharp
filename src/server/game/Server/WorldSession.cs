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

using System;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;

using AzerothCore.Constants;
using AzerothCore.Database;
using AzerothCore.Game.Server;
using AzerothCore.Logging;
using AzerothCore.Threading;
using AzerothCore.Utilities;

namespace AzerothCore.Game;

// class to deal with packet processing
// allows to determine if next packet is safe to be processed
public class PacketFilter : IChecker<WorldPacketData>
{
    protected readonly WorldSession _session;

    public PacketFilter(WorldSession session)
    {
        _session = session;
    }

    public virtual bool Process(WorldPacketData packet)
    {
        return true;
    }

    public virtual bool ProcessUnsafe()
    {
        return true;
    }
}

// class used to filer only thread-unsafe packets from queue
// in order to update only be used in World::UpdateSessions()
public class WorldSessionFilter : PacketFilter
{
    public WorldSessionFilter(WorldSession session) : base(session) {  }

    public override bool Process(WorldPacketData packet)
    {
        ClientOpcodeHandler? opHandle = OpcodeTable.Instance[(Opcodes)packet.Opcode];

        // check if packet handler is supposed to be safe
        if (opHandle?.ProcessingPlace == PacketProcessing.PROCESS_INPLACE)
        {
            return true;
        }

        //thread-unsafe packets should be processed in World::UpdateSessions()
        if (opHandle?.ProcessingPlace == PacketProcessing.PROCESS_THREADUNSAFE)
        {
            return true;
        }

        // no player attached? -> our client! ^^
        Player? player = _session.GetPlayer();

        if (player == null)
        {
            return true;
        }

        // lets process all packets for non-in-the-world player
        return !player.IsInWorld();
    }
}

public enum AccountInfoQueryIndex : uint
{
    GLOBAL_ACCOUNT_DATA = 0,
    TUTORIALS,
    MAX_QUERIES
}

public class AccountInfoQueryHolderPerRealm : SQLQueryHolder<AccountInfoQueryIndex>
{
    public void Initialize(uint accountId)
    {
        PreparedStatement stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_ACCOUNT_DATA);
        stmt.AddValue(0, accountId);
        SetQuery(AccountInfoQueryIndex.GLOBAL_ACCOUNT_DATA, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_TUTORIALS);
        stmt.AddValue(0, accountId);
        SetQuery(AccountInfoQueryIndex.TUTORIALS, stmt);
    }
}

public struct AccountData
{
    public long Time;
    public string? Data;

    public AccountData()
    {
        Data = string.Empty;
    }
}

public enum DosProtectionPolicy : uint
{
    POLICY_LOG,
    POLICY_KICK,
    POLICY_BAN
}

public partial class WorldSession : IOpcodeHandler
{
    public static readonly int NUM_ACCOUNT_DATA_TYPES = 8;
    public static readonly byte GLOBAL_CACHE_MASK = 0x15;
    public static readonly byte PER_CHARACTER_CACHE_MASK = 0xEA;

    private static readonly ILogger logger = LoggerFactory.GetLogger();
    private static readonly IOpcodeHandler _opcodeHandler = new WorldSession();

    private ConcurrentHashSet<ObjectGuid> _legitCharacters;
    private uint _accountId;
    private string? _accountName;
    private WorldSocket? _socket;
    private AccountTypes _security;
    private byte _expansion;
    private long _muteTime;
    private uint _recruiterId;
    private bool _isRecruiter;
    private bool _skipQueue;
    private bool _inQueue;
    private bool _playerLoading;                               // code processed in LoginPlayer
    //private bool _playerLogout;                                // code processed in LogoutPlayer
    //private bool _playerRecentlyLogout;
    //private bool _playerSave;
    //private Locale _sessionDbcLocale;
    //private Locale _sessionDbLocaleIndex;
    private uint _totalTime;
    private AccountData[] _accountData;
    private uint[] _tutorials;
    private bool _tutorialsChanged;
    private List<AddonInfo> _addonsList;
    private LockedQueue<WorldPacketData> _recvQueue;
    private AsyncCallbackProcessor<QueryCallback> _queryProcessor;
    private AsyncCallbackProcessor<TransactionCallback> _transactionCallbacks;
    private AsyncCallbackProcessor<ISqlCallback> _queryHolderProcessor;
    private uint _guidLow;
    private Player? _player;
    private bool _kicked;

    public static IOpcodeHandler OpcodeHandler
    {
        get
        {
            return _opcodeHandler;
        }
    }

    public uint GetAccountId() { return _accountId; }
    public byte GetExpansion() { return _expansion; }
    public AccountData GetAccountData(AccountDataType type) { return _accountData[(byte)type]; }

    public void SetInQueue(bool state) { _inQueue = state; }

    private WorldSession()
    {
        _legitCharacters = new ConcurrentHashSet<ObjectGuid>();
        _inQueue = false;
        _recvQueue = new();
        _queryProcessor = new();
        _transactionCallbacks = new();
        _queryHolderProcessor = new();
        _accountData = new AccountData[NUM_ACCOUNT_DATA_TYPES];
        _tutorials = new uint[SharedConst.MAX_ACCOUNT_TUTORIAL_VALUES];
        _addonsList = new ();
    }

    public WorldSession(
                uint id,
                string account,
                WorldSocket? worldSocket,
                AccountTypes security,
                byte expansion,
                long muteTime,
                Locale locale,
                uint recruiter,
                bool isARecruiter,
                bool skipQueue,
                uint totalTime) : this()
    {
        _accountId = id;
        _accountName = account;
        _socket = worldSocket;
        _security = security;
        _expansion = expansion;
        _muteTime = muteTime;
        //_sessionDbLocaleIndex = locale;
        _recruiterId = recruiter;
        _isRecruiter = isARecruiter;
        _skipQueue = skipQueue;
        _totalTime = totalTime;
        _player = null;
    }

    internal void InitializeSession()
    {
        uint cacheVersion = Global.sWorld.GetIntConfig(WorldIntConfigs.CONFIG_CLIENTCACHE_VERSION);

        AccountInfoQueryHolderPerRealm realmHolder = new();
        realmHolder.Initialize(GetAccountId());

        AddQueryHolderCallback(DB.Characters.DelayQueryHolder(realmHolder)).AfterComplete(result =>
        {
            if (result != null)
            {
                InitializeSessionCallback(result, cacheVersion);
            }
        });
    }

    private void InitializeSessionCallback(SQLQueryHolder<AccountInfoQueryIndex> realmHolder, uint clientCacheVersion)
    {
        LoadAccountData(realmHolder.GetResult(AccountInfoQueryIndex.GLOBAL_ACCOUNT_DATA), GLOBAL_CACHE_MASK);
        LoadTutorialsData(realmHolder.GetResult(AccountInfoQueryIndex.TUTORIALS));

        if (!_inQueue)
        {
            SendAuthResponse(ResponseCodes.AUTH_OK, true);
        }
        else
        {
            SendAuthWaitQueue(0);
        }

        SetInQueue(false);
        ResetTimeOutTime(false);

        SendAddonsInfo();
        SendClientCacheVersion(clientCacheVersion);
        SendTutorialsData();
    }

    internal void SendAuthResponse(ResponseCodes code, bool shortForm, uint queuePos = 0)
    {
        WorldPacketData packet = new(Opcodes.SMSG_AUTH_RESPONSE);

        packet.WriteByte((byte)code);
        packet.WriteUInt(0);                                    // BillingTimeRemaining
        packet.WriteByte(6);                                    // BillingPlanFlags
        packet.WriteUInt((uint)0);                              // BillingTimeRested
        packet.WriteByte(GetExpansion());                       // 0 - normal, 1 - TBC, 2 - WOTLK, must be set in database manually for each account

        if (!shortForm)
        {
            packet.WriteUInt(queuePos);                         // Queue position
            packet.WriteByte(0);                                // Realm has a free character migration - bool
        }

        SendPacket(packet);
    }

    /// Send a packet to the client
    internal void SendPacket(WorldPacketData packet)
    {
        if ((ushort)OpcodeMisc.NULL_OPCODE == packet.Opcode)
        {
            logger.Error(LogFilter.Network, $"{GetPlayerInfo()} send NULL_OPCODE");
            return;
        }

        if (_socket == null)
        {
            return;
        }

        logger.Debug(LogFilter.Network, $"Server->Client: {GetPlayerInfo()} {Enum.GetName((Opcodes)packet.Opcode) ?? "Unkown Opcode"}");

        _socket.SendPacket(packet);
    }

    internal void SendAuthWaitQueue(uint position)
    {
        if (position == 0)
        {
            WorldPacketData packet = new(Opcodes.SMSG_AUTH_RESPONSE);

            packet.WriteByte((byte)ResponseCodes.AUTH_OK);

            SendPacket(packet);
        }
        else
        {
            WorldPacketData packet = new(Opcodes.SMSG_AUTH_RESPONSE);

            packet.WriteByte((byte)ResponseCodes.AUTH_WAIT_QUEUE);
            packet.WriteUInt(position);
            packet.WriteByte(0);                                 // unk

            SendPacket(packet);
        }
    }

    internal void ReadAddonsInfo(ByteBuffer data)
    {
        if (data.GetReadPosition() + 4 > data.GetSize())
        {
            return;
        }

        uint size = data.ReadUInt32();

        if (size == 0)
        {
            return;
        }

        if (size > 0xFFFFF)
        {
            logger.Error(LogFilter.Network, $"WorldSession::ReadAddonsInfo addon info too big, size {size}");

            return;
        }

        int uSize = (int)size;
        int pos = (int)data.GetReadPosition();
        ReadOnlySpan<byte> source = new(data.GetData(), pos, (int)(data.GetSize() - pos));
        byte[] unpackedAddonInfo = new byte[uSize];

        ZlibError zlibError = Zlib.Unpack(unpackedAddonInfo, ref uSize, source, source.Length);

        if (zlibError == ZlibError.Okay)
        {
            ByteBuffer addonInfo = new(unpackedAddonInfo);
            uint addonsCount = addonInfo.ReadUInt32();  // addons count                       

            for (uint i = 0; i < addonsCount; ++i)
            {
                string addonName;
                byte enabled;
                uint crc, unk1;

                // check next addon data format correctness
                if (addonInfo.GetReadPosition() + 1 > addonInfo.GetSize())
                {
                    return;
                }

                addonName = addonInfo.ReadCString();
                enabled = addonInfo.ReadUInt8();
                crc = addonInfo.ReadUInt32();
                unk1 = addonInfo.ReadUInt32();

                logger.Debug(LogFilter.Network, $"ADDON: Name: {addonName}, Enabled: 0x{enabled:x}, CRC: 0x{crc:x}, Unknown2: 0x{unk1:x}");

                AddonInfo addon = new(addonName, enabled, crc, 2, true);
                SavedAddon? savedAddon = AddonMgr.GetAddonInfo(addonName);

                if (savedAddon != null)
                {
                    bool match = true;

                    if (addon.CRC != savedAddon?.CRC)
                    {
                        match = false;
                    }

                    if (!match)
                    {
                        logger.Debug(LogFilter.Network, $"ADDON: {addon.Name} was known, but didn't match known CRC (0x{savedAddon?.CRC:x})!");
                    }
                    else
                    {
                        logger.Debug(LogFilter.Network, $"ADDON: {addon.Name} was known, CRC is correct (0x{savedAddon?.CRC:x})");
                    }
                }
                else
                {
                    AddonMgr.SaveAddon(addon);

                    logger.Debug(LogFilter.Network, $"ADDON: {addon.Name} (0x{addon.CRC:x}) was not known, saving...");
                }

                // TODO: Find out when to not use CRC/pubkey, and other possible states.
                _addonsList.Add(addon);
            }

            uint currentTime = addonInfo.ReadUInt32();

            logger.Debug(LogFilter.Network, $"ADDON: CurrentTime: {currentTime}");

            if (addonInfo.GetReadPosition() != addonInfo.GetSize())
            {
                logger.Debug(LogFilter.Network, $"packet under-read!");
            }
        }
        else
        {
            logger.Error(LogFilter.Network, $"Addon packet uncompress error! ({Enum.GetName(typeof(ZlibError), zlibError)})");
        }
    }

    private void SendAddonsInfo()
    {
        byte[] addonPublicKey = new byte[]
        {
            0xC3, 0x5B, 0x50, 0x84, 0xB9, 0x3E, 0x32, 0x42, 0x8C, 0xD0, 0xC7, 0x48, 0xFA, 0x0E, 0x5D, 0x54,
            0x5A, 0xA3, 0x0E, 0x14, 0xBA, 0x9E, 0x0D, 0xB9, 0x5D, 0x8B, 0xEE, 0xB6, 0x84, 0x93, 0x45, 0x75,
            0xFF, 0x31, 0xFE, 0x2F, 0x64, 0x3F, 0x3D, 0x6D, 0x07, 0xD9, 0x44, 0x9B, 0x40, 0x85, 0x59, 0x34,
            0x4E, 0x10, 0xE1, 0xE7, 0x43, 0x69, 0xEF, 0x7C, 0x16, 0xFC, 0xB4, 0xED, 0x1B, 0x95, 0x28, 0xA8,
            0x23, 0x76, 0x51, 0x31, 0x57, 0x30, 0x2B, 0x79, 0x08, 0x50, 0x10, 0x1C, 0x4A, 0x1A, 0x2C, 0xC8,
            0x8B, 0x8F, 0x05, 0x2D, 0x22, 0x3D, 0xDB, 0x5A, 0x24, 0x7A, 0x0F, 0x13, 0x50, 0x37, 0x8F, 0x5A,
            0xCC, 0x9E, 0x04, 0x44, 0x0E, 0x87, 0x01, 0xD4, 0xA3, 0x15, 0x94, 0x16, 0x34, 0xC6, 0xC2, 0xC3,
            0xFB, 0x49, 0xFE, 0xE1, 0xF9, 0xDA, 0x8C, 0x50, 0x3C, 0xBE, 0x2C, 0xBB, 0x57, 0xED, 0x46, 0xB9,
            0xAD, 0x8B, 0xC6, 0xDF, 0x0E, 0xD6, 0x0F, 0xBE, 0x80, 0xB3, 0x8B, 0x1E, 0x77, 0xCF, 0xAD, 0x22,
            0xCF, 0xB7, 0x4B, 0xCF, 0xFB, 0xF0, 0x6B, 0x11, 0x45, 0x2D, 0x7A, 0x81, 0x18, 0xF2, 0x92, 0x7E,
            0x98, 0x56, 0x5D, 0x5E, 0x69, 0x72, 0x0A, 0x0D, 0x03, 0x0A, 0x85, 0xA2, 0x85, 0x9C, 0xCB, 0xFB,
            0x56, 0x6E, 0x8F, 0x44, 0xBB, 0x8F, 0x02, 0x22, 0x68, 0x63, 0x97, 0xBC, 0x85, 0xBA, 0xA8, 0xF7,
            0xB5, 0x40, 0x68, 0x3C, 0x77, 0x86, 0x6F, 0x4B, 0xD7, 0x88, 0xCA, 0x8A, 0xD7, 0xCE, 0x36, 0xF0,
            0x45, 0x6E, 0xD5, 0x64, 0x79, 0x0F, 0x17, 0xFC, 0x64, 0xDD, 0x10, 0x6F, 0xF3, 0xF5, 0xE0, 0xA6,
            0xC3, 0xFB, 0x1B, 0x8C, 0x29, 0xEF, 0x8E, 0xE5, 0x34, 0xCB, 0xD1, 0x2A, 0xCE, 0x79, 0xC3, 0x9A,
            0x0D, 0x36, 0xEA, 0x01, 0xE0, 0xAA, 0x91, 0x20, 0x54, 0xF0, 0x72, 0xD8, 0x1E, 0xC7, 0x89, 0xD2
        };

        WorldPacketData data = new (Opcodes.SMSG_ADDON_INFO);

        foreach (AddonInfo addonInfo in _addonsList)
        {
            data.WriteByte(addonInfo.State);

            byte crcpub = addonInfo.UsePublicKeyOrCRC ? (byte)0x01: (byte)0x00;
            data.WriteByte(crcpub);

            if (crcpub != 0)
            {
                bool usepk = addonInfo.CRC != AddonMgr.STANDARD_ADDON_CRC; // If addon is Standard addon CRC
                data.WriteByte(usepk ? (byte)0x01 : (byte)0x00);

                if (usepk)                                      // if CRC is wrong, add public key (client need it)
                {
                    logger.Debug(LogFilter.Network, $"ADDON: CRC (0x{addonInfo.CRC:x}) for addon {addonInfo.Name} is wrong (does not match expected 0x{AddonMgr.STANDARD_ADDON_CRC:x}), sending pubkey");

                    data.WriteBytes(addonPublicKey);
                }

                data.WriteUInt(0);                              /// @todo: Find out the meaning of this.
            }

            byte unk3 = 0;                                     // 0 is sent here
            data.WriteByte(unk3);

            if (unk3 != 0)
            {
                // String, length 256 (null terminated)
                data.WriteByte(0);
            }
        }

        _addonsList.Clear();

        LockedQueue<BannedAddon> bannedAddons = AddonMgr.GetBannedAddons();
        data.WriteUInt((uint)bannedAddons.Size());

        while (bannedAddons.Next(out var bannedAddon))
        {
            data.WriteUInt(bannedAddon.Id);
            data.WriteBytes(bannedAddon.NameMD5);
            data.WriteBytes(bannedAddon.VersionMD5);
            data.WriteUInt(bannedAddon.Timestamp);
            data.WriteUInt(1);  // IsBanned
        }

        SendPacket(data);
    }

    internal void SendClientCacheVersion(uint clientCacheVersion)
    {
        WorldPacketData data = new (Opcodes.SMSG_CLIENTCACHE_VERSION);

        data.WriteUInt(clientCacheVersion);

        SendPacket(data);
    }

    internal void LoadAccountData(SQLResult result, uint mask)
    {
        for (uint i = 0; i < NUM_ACCOUNT_DATA_TYPES; ++i)
        {
            if ((mask & (1 << (byte)i)) != 0)
            {
                _accountData[i] = new AccountData();
            }
        }

        if (result.IsEmpty())
        {
            return;
        }

        do
        {
            SQLFields fields = result.GetFields();

            byte type = fields.Get<byte>(0);

            string tableName = mask == GLOBAL_CACHE_MASK ? "account_data" : "character_account_data";

            if (type >= NUM_ACCOUNT_DATA_TYPES)
            {
                logger.Error(LogFilter.Network, $"Table `{tableName}` have invalid account data type ({type}), ignore.");

                continue;
            }

            if ((mask & (1 << type)) == 0)
            {
                logger.Error(LogFilter.Network, $"Table `{tableName}` have non appropriate for table  account data type ({type}), ignore.");

                continue;
            }

            byte[] accountData = fields.Get<byte[]>(2) ?? Array.Empty<byte>();

            _accountData[type].Time = fields.Get<uint>(1);
            _accountData[type].Data = Encoding.UTF8.GetString(accountData);
        }
        while (result.NextRow());
    }

    internal void SendAccountDataTimes(uint mask)
    {
        WorldPacketData data = new (Opcodes.SMSG_ACCOUNT_DATA_TIMES);    // changed in WotLK

        data.WriteUInt((uint)TimeHelper.UnixTime);                                        // unix time of something
        data.WriteByte(1);
        data.WriteUInt(mask);                                                             // type mask

        for (uint i = 0; i < NUM_ACCOUNT_DATA_TYPES; ++i)
        {
            if ((mask & (1 << (byte)i)) != 0)
            {
                data.WriteUInt((uint)GetAccountData((AccountDataType)i).Time);            // also unix time
            }
        }

        SendPacket(data);
    }

    internal void LoadTutorialsData(SQLResult result)
    {
        for (byte i = 0; i < SharedConst.MAX_ACCOUNT_TUTORIAL_VALUES; i++ )
        {
            _tutorials[i] = 0;
        }

        if (!result.IsEmpty())
        {
            for (byte i = 0; i < SharedConst.MAX_ACCOUNT_TUTORIAL_VALUES; ++i)
            {
                _tutorials[i] = result.Read<uint>(i);
            }
        }

        _tutorialsChanged = false;
    }

    internal void SendTutorialsData()
    {
        WorldPacketData data = new (Opcodes.SMSG_TUTORIAL_FLAGS);

        for (byte i = 0; i < SharedConst.MAX_ACCOUNT_TUTORIAL_VALUES; ++i)
        {
            data.WriteUInt(_tutorials[i]);
        }

        SendPacket(data);
    }

    internal void SaveTutorialsData(SQLTransaction trans)
    {
        if (!_tutorialsChanged)
        {
            return;
        }

        var stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_HAS_TUTORIALS);
        stmt.AddValue(0, GetAccountId());

        bool hasTutorials = !DB.Characters.Query(stmt).IsEmpty();

        stmt = CharacterDatabase.GetPreparedStatement(hasTutorials ? CharStatements.CHAR_UPD_TUTORIALS : CharStatements.CHAR_INS_TUTORIALS);

        for (byte i = 0; i < SharedConst.MAX_ACCOUNT_TUTORIAL_VALUES; ++i)
        {
            stmt.AddValue(i, _tutorials[i]);
        }

        stmt.AddValue(SharedConst.MAX_ACCOUNT_TUTORIAL_VALUES, GetAccountId());

        trans.Append(stmt);

        _tutorialsChanged = false;
    }

    internal void ResetTimeOutTime(bool onlyActive)
    {
        // TODO: game: WorldSession::ResetTimeOutTime(bool onlyActive)
    }

    internal void QueuePacket(WorldPacketData packetToQueue)
    {
        _recvQueue.Add(packetToQueue);
    }

    internal string GetPlayerInfo()
    {
        // TODO: game: WorldSession::GetPlayerInfo()

        return "Dummy";
    }

    internal void SetLatency(uint latency)
    {
        // TODO: game: WorldSession::SetLatency(uint latency)
    }

    internal AccountTypes GetSecurity()
    {
        // TODO: game: WorldSession::GetSecurity()

        return AccountTypes.SEC_PLAYER;
    }

    internal void InitWarden(byte[] sessionKey, string? os)
    {
        // TODO: game: WorldSession::InitWarden(byte[] sessionKey, string? os)
    }

    internal Player? GetPlayer()
    {
        return _player;
    }

    internal void SetPlayer(Player? player)
    {
        _player = player;

        // set m_GUID that can be used while player loggined and later until m_playerRecentlyLogout not reset
        if (_player != null)
        {
            _guidLow = _player.GetGUID().GetCounter();
        }
    }

    internal bool Update(uint diff, PacketFilter updater)
    {
        // TODO: game: WorldSession::Update(uint diff, PacketFilter updater)

        while (_socket != null && _recvQueue.Next(out WorldPacketData? packet, updater))
        {
            if (packet != null)
            {
                Opcodes opcode = (Opcodes)packet.Opcode;

                ClientOpcodeHandler? opHandle = OpcodeTable.Instance[opcode];

                try
                {
                    switch (opHandle?.Status)
                    {
                        //case SessionStatus.STATUS_LOGGEDIN:
                        //    break;
                        case SessionStatus.STATUS_AUTHED:
                            // TODO: game: WorldSession::Update(uint diff, PacketFilter updater)
                            opHandle?.Call(this, packet);
                            break;
                        //case SessionStatus.STATUS_TRANSFER:
                        //    break;
                        //case SessionStatus.STATUS_LOGGEDIN_OR_RECENTLY_LOGGOUT:
                        //    break;
                        //case SessionStatus.STATUS_NEVER:
                        //    break;
                        //case SessionStatus.STATUS_UNHANDLED:
                        //    break;
                        default:
                            logger.Error(LogFilter.Network, $"Received not handled opcode 0x{packet.Opcode:x} from {GetPlayerInfo()}");
                            break;
                    }
                }
                catch (Exception e)
                {
                    logger.Error(LogFilter.Session, $"WorldSession::Update({diff}, PacketFilter updater) => ¥n{e.StackTrace}");
                }
            }
        }

        // TODO: game: WorldSession::Update(uint diff, PacketFilter updater)

        ProcessQueryCallbacks();

        // TODO: game: WorldSession::Update(uint diff, PacketFilter updater)

        return true;
    }

    internal void KickPlayer(bool setKicked = true)
    {
        KickPlayer("Unknown reason", setKicked);
    }

    internal void KickPlayer(string reason, bool setKicked = true)
    {
        if (_socket != null)
        {
            string playerName = _player != null ? _player.GetName() : "<none>";
            string guid = _player != null ? _player.GetGUID().ToString() ?? "" : "";

            logger.Info(LogFilter.Network, $"Account: {GetAccountId()} Character: '{playerName}' {guid} kicked with reason: {reason}");

            _socket.CloseSocket();
        }

        if (setKicked)
        {
            SetKicked(true); // the session won't be left ingame for 60 seconds and to also kick offline session
        }
    }

    internal bool IsKicked()
    {
        return _kicked;
    }

    internal void SetKicked(bool val)
    {
        _kicked = val;
    }

    internal bool IsPlayerLoading()
    {
        return _playerLoading;
    }

    internal uint GetGuidLow()
    {
        Player? player = GetPlayer();

        return player != null ? player.GetGUID().GetCounter() : 0;
    }

    private void ProcessQueryCallbacks()
    {
        _queryProcessor.ProcessReadyCallbacks();
        _transactionCallbacks.ProcessReadyCallbacks();
        _queryHolderProcessor.ProcessReadyCallbacks();
    }

    private SQLQueryHolderCallback<TQueryIndexEnum> AddQueryHolderCallback<TQueryIndexEnum>(SQLQueryHolderCallback<TQueryIndexEnum> callback) where TQueryIndexEnum : Enum
    {
        return (SQLQueryHolderCallback<TQueryIndexEnum>)_queryHolderProcessor.AddCallback(callback);
    }
}
