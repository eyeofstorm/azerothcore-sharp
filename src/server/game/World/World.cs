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

using AzerothCore.Configuration;
using AzerothCore.Constants;
using AzerothCore.Database;
using AzerothCore.DataStores;
using AzerothCore.Logging;
using AzerothCore.Singleton;
using AzerothCore.Threading;
using AzerothCore.Utilities;

using SessionMap = System.Collections.Generic.Dictionary<uint, AzerothCore.Game.WorldSession>;

namespace AzerothCore.Game;

public enum ShutdownMask : int
{
    SHUTDOWN_MASK_RESTART = 1,
    SHUTDOWN_MASK_IDLE = 2,
}

public enum ShutdownExitCode : int
{
    SHUTDOWN_EXIT_CODE = 0,
    ERROR_EXIT_CODE = 1,
    RESTART_EXIT_CODE = 2,
}

// Timers for different object refresh rates
public enum WorldTimers : int
{
    WUPDATE_AUCTIONS,
    WUPDATE_WEATHERS,
    WUPDATE_UPTIME,
    WUPDATE_CORPSES,
    WUPDATE_EVENTS,
    WUPDATE_CLEANDB,
    WUPDATE_AUTOBROADCAST,
    WUPDATE_MAILBOXQUEUE,
    WUPDATE_PINGDB,
    WUPDATE_5_SECS,
    WUPDATE_WHO_LIST,
    WUPDATE_COUNT
}

// Can be used in SMSG_AUTH_RESPONSE packet
public enum BillingPlanFlags : byte
{
    SESSION_NONE = 0x00,
    SESSION_UNUSED = 0x01,
    SESSION_RECURRING_BILL = 0x02,
    SESSION_FREE_TRIAL = 0x04,
    SESSION_IGR = 0x08,
    SESSION_USAGE = 0x10,
    SESSION_TIME_MIXTURE = 0x20,
    SESSION_RESTRICTED = 0x40,
    SESSION_ENABLE_CAIS = 0x80,
}

public enum RealmZone : int
{
    REALM_ZONE_UNKNOWN = 0,                         // any language
    REALM_ZONE_DEVELOPMENT = 1,                     // any language
    REALM_ZONE_UNITED_STATES = 2,                   // extended-Latin
    REALM_ZONE_OCEANIC = 3,                         // extended-Latin
    REALM_ZONE_LATIN_AMERICA = 4,                   // extended-Latin
    REALM_ZONE_TOURNAMENT_5 = 5,                    // basic-Latin at create, any at login
    REALM_ZONE_KOREA = 6,                           // East-Asian
    REALM_ZONE_TOURNAMENT_7 = 7,                    // basic-Latin at create, any at login
    REALM_ZONE_ENGLISH = 8,                         // extended-Latin
    REALM_ZONE_GERMAN = 9,                          // extended-Latin
    REALM_ZONE_FRENCH = 10,                         // extended-Latin
    REALM_ZONE_SPANISH = 11,                        // extended-Latin
    REALM_ZONE_RUSSIAN = 12,                        // Cyrillic
    REALM_ZONE_TOURNAMENT_13 = 13,                          // basic-Latin at create, any at login
    REALM_ZONE_TAIWAN = 14,                         // East-Asian
    REALM_ZONE_TOURNAMENT_15 = 15,                          // basic-Latin at create, any at login
    REALM_ZONE_CHINA = 16,                          // East-Asian
    REALM_ZONE_CN1 = 17,                            // basic-Latin at create, any at login
    REALM_ZONE_CN2 = 18,                            // basic-Latin at create, any at login
    REALM_ZONE_CN3 = 19,                            // basic-Latin at create, any at login
    REALM_ZONE_CN4 = 20,                            // basic-Latin at create, any at login
    REALM_ZONE_CN5 = 21,                            // basic-Latin at create, any at login
    REALM_ZONE_CN6 = 22,                            // basic-Latin at create, any at login
    REALM_ZONE_CN7 = 23,                            // basic-Latin at create, any at login
    REALM_ZONE_CN8 = 24,                            // basic-Latin at create, any at login
    REALM_ZONE_TOURNAMENT_25 = 25,                  // basic-Latin at create, any at login
    REALM_ZONE_TEST_SERVER = 26,                    // any language
    REALM_ZONE_TOURNAMENT_27 = 27,                  // basic-Latin at create, any at login
    REALM_ZONE_QA_SERVER = 28,                      // any language
    REALM_ZONE_CN9 = 29,                            // basic-Latin at create, any at login
    REALM_ZONE_TEST_SERVER_2 = 30,                  // any language
    REALM_ZONE_CN10 = 31,                           // basic-Latin at create, any at login
    REALM_ZONE_CTC = 32,
    REALM_ZONE_CNC = 33,
    REALM_ZONE_CN1_4 = 34,                          // basic-Latin at create, any at login
    REALM_ZONE_CN2_6_9 = 35,                        // basic-Latin at create, any at login
    REALM_ZONE_CN3_7 = 36,                          // basic-Latin at create, any at login
    REALM_ZONE_CN5_8 = 37                           // basic-Latin at create, any at login
}

public class World : Singleton<World>, IWorld
{
    protected static readonly ILogger logger = LoggerFactory.GetLogger();

    protected float _maxVisibleDistanceOnContinents = ObjectDefines.DEFAULT_VISIBILITY_DISTANCE;
    protected float _maxVisibleDistanceInInstances  = ObjectDefines.DEFAULT_VISIBILITY_INSTANCE;
    protected float _maxVisibleDistanceInBGArenas   = ObjectDefines.DEFAULT_VISIBILITY_BGARENAS;

    private string?     _dbVersion;

    private Realm       _realm = new();

    private string      _newCharString;

    private float[]     _rate_values        = new float[(uint)Rates.MAX_RATES];
    private uint[]      _int_configs        = new uint[(uint)WorldIntConfigs.INT_CONFIG_VALUE_COUNT];
    private bool[]      _bool_configs       = new bool[(uint)WorldBoolConfigs.BOOL_CONFIG_VALUE_COUNT];
    private float[]     _float_configs      = new float[(uint)WorldFloatConfigs.FLOAT_CONFIG_VALUE_COUNT];

    private uint        _playerLimit;

    private AccountTypes _allowedSecurityLevel;

    private readonly LockedQueue<WorldSession> _addSessionQueue;
    private readonly SessionMap _sessions;
    private readonly SessionMap _offlineSessions;

    private AutoResetEvent _stopEvent;

    private string _dataPath;

    private IntervalTimer[] _timers;

    private World()
    {
        _allowedSecurityLevel = AccountTypes.SEC_PLAYER;
        _addSessionQueue = new ();
        _sessions = new SessionMap();
        _offlineSessions = new SessionMap();
        _stopEvent = new (false);
        _dataPath = string.Empty;

        _newCharString = string.Empty;

        _timers = new IntervalTimer[(int)WorldTimers.WUPDATE_COUNT];

        for (int i = 0; i < (int)WorldTimers.WUPDATE_COUNT; i++)
        {
            _timers[i] = new IntervalTimer();
        }
    }

    // Get a server configuration element (see #WorldConfigs)
    public uint GetIntConfig(WorldIntConfigs intConfigsIndex)
    {
        return (intConfigsIndex < WorldIntConfigs.INT_CONFIG_VALUE_COUNT) ? _int_configs[(uint)intConfigsIndex] : 0;
    }

    public void LoadDBVersion()
    {
        QueryResult result = DB.World.Query("SELECT db_version, cache_id FROM version LIMIT 1");

        if (!result.IsEmpty())
        {
            Fields fields = result.Fetch();

            _dbVersion = fields[0].Get<string>();

            // will be overwrite by config values if different and non-0
            _int_configs[(uint)WorldIntConfigs.CONFIG_CLIENTCACHE_VERSION] = fields[1].Get<uint>();
        }

        if (string.IsNullOrEmpty(_dbVersion))
        {
            _dbVersion = "Unknown world database.";
        }
    }

    public string GetDBVersion()
    {
        return _dbVersion ?? string.Empty;
    }

    public Realm GetRealm()
    {
        return _realm;
    }

    public void SetInitialWorldSettings()
    {
        // Server startup begin
        uint startupBegin = TimeHelper.GetMSTime();

        // Initialize config settings
        LoadConfigSettings();

        // Initialize Allowed Security Level
        LoadDBAllowedSecurityLevel();

        // TODO: worldserver: World::SetInitialWorldSettings()

        // Load the DBC files
        logger.Info(LogFilter.ServerLoading, $"Initialize Data Stores...");
        DataStores.LoadDBCStores(ConfigMgr.GetOption("DataDir", "./"));

        logger.Info(LogFilter.ServerLoading, $"Loading Player Create Data...");
        Global.sObjectMgr.LoadPlayerInfo();

        logger.Info(LogFilter.ServerLoading, $"Initializing Opcodes...");
        OpcodeTable.Instance.Initialize();

        logger.Info(LogFilter.ServerLoading, $"Loading Script Names...");
        Global.sObjectMgr.LoadScriptNames();

        logger.Info(LogFilter.ServerLoading, $"Loading Instance Template...");
        Global.sObjectMgr.LoadInstanceTemplate();

        logger.Info(LogFilter.ServerLoading, $"Loading Client Addons...");
        AddonMgr.LoadFromDB();
    }

    private void LoadDBAllowedSecurityLevel()
    {
        var stmt = LoginDatabase.GetPreparedStatement(LoginStatements.LOGIN_SEL_REALMLIST_SECURITY_LEVEL);
        stmt.SetData(0, GetRealm().Id.Index);

        QueryResult result = DB.Login.Query(stmt);

        if (!result.IsEmpty())
        {
            SetPlayerSecurityLimit((AccountTypes)result.Read<byte>(0));
        }
    }

    private void SetPlayerSecurityLimit(AccountTypes accountTypes)
    {
        AccountTypes sec = accountTypes < AccountTypes.SEC_CONSOLE ? accountTypes : AccountTypes.SEC_PLAYER;

        bool update = sec > _allowedSecurityLevel;
        _allowedSecurityLevel = sec;

        if (update)
        {
            KickAllLess(_allowedSecurityLevel);
        }
    }

    /// Kick (and save) all players with security level less `sec`
    private void KickAllLess(AccountTypes sec)
    {
        // session not removed at kick and will removed in next update tick
        foreach (var itr in _sessions)
        {
            if (itr.Value.GetSecurity() < sec)
            {
                itr.Value.KickPlayer("KickAllLess");
            }
        }
    }

    private void LoadConfigSettings(bool reload = false)
    {
        // TODO: game: World::LoadConfigSettings(bool reload = false)
        //if (reload)
        //{
        //    if (!ConfigMgr.Reload())
        //    {
        //        logger.Error(LogFilter.ServerLoading, "World settings reload fail: can't read settings.");
        //        return;
        //    }
        //}

        // load update time related configs
        Global.sWorldUpdateTime.LoadFromConfig();

        // Read the player limit and the Message of the day from the config file
        if (!reload)
        {
            SetPlayerAmountLimit(ConfigMgr.GetOption("PlayerLimit", 1000U));
        }

        // Read ticket system setting from the config file
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALLOW_TICKETS] = ConfigMgr.GetOption("AllowTickets", true);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_DELETE_CHARACTER_TICKET_TRACE] = ConfigMgr.GetOption("DeletedCharacterTicketTrace", false);

        // Get string for new logins (newly created characters)
        SetNewCharString(ConfigMgr.GetOption("PlayerStart.String", string.Empty));

        // Send server info on login?
        _int_configs[(uint)WorldIntConfigs.CONFIG_ENABLE_SINFO_LOGIN] = ConfigMgr.GetOption("Server.LoginInfo", 0U);

        // Read all rates from the config file
        _rate_values[(uint)Rates.RATE_HEALTH] = ConfigMgr.GetOption("Rate.Health", 1);

        if (_rate_values[(uint)Rates.RATE_HEALTH] < 0)
        {
            logger.Error(LogFilter.ServerLoading, $"Rate.Health ({_rate_values[(uint)Rates.RATE_HEALTH]}) must be > 0. Using 1 instead.");
            _rate_values[(uint)Rates.RATE_HEALTH] = 1;
        }

        _rate_values[(uint)Rates.RATE_POWER_MANA] = ConfigMgr.GetOption("Rate.Mana", 1);

        if (_rate_values[(uint)Rates.RATE_POWER_MANA] < 0)
        {
            logger.Error(LogFilter.ServerLoading, $"Rate.Mana ({_rate_values[(uint)Rates.RATE_POWER_MANA]}) must be > 0. Using 1 instead.");
            _rate_values[(uint)Rates.RATE_POWER_MANA] = 1;
        }

        _rate_values[(uint)Rates.RATE_POWER_RAGE_INCOME] = ConfigMgr.GetOption("Rate.Rage.Income", 1);
        _rate_values[(uint)Rates.RATE_POWER_RAGE_LOSS] = ConfigMgr.GetOption("Rate.Rage.Loss", 1);

        if (_rate_values[(uint)Rates.RATE_POWER_RAGE_LOSS] < 0)
        {
            logger.Error(LogFilter.ServerLoading, $"Rate.Rage.Loss ({_rate_values[(uint)Rates.RATE_POWER_RAGE_LOSS]}) must be > 0. Using 1 instead.");
            _rate_values[(uint)Rates.RATE_POWER_RAGE_LOSS] = 1;
        }

        _rate_values[(uint)Rates.RATE_POWER_RUNICPOWER_INCOME] = ConfigMgr.GetOption("Rate.RunicPower.Income", 1);
        _rate_values[(uint)Rates.RATE_POWER_RUNICPOWER_LOSS] = ConfigMgr.GetOption("Rate.RunicPower.Loss", 1);

        if (_rate_values[(uint)Rates.RATE_POWER_RUNICPOWER_LOSS] < 0)
        {
            logger.Error(LogFilter.ServerLoading, $"Rate.RunicPower.Loss ({_rate_values[(uint)Rates.RATE_POWER_RUNICPOWER_LOSS]}) must be > 0. Using 1 instead.");
            _rate_values[(uint)Rates.RATE_POWER_RUNICPOWER_LOSS] = 1;
        }

        _rate_values[(uint)Rates.RATE_POWER_FOCUS] = ConfigMgr.GetOption("Rate.Focus", 1.0f);
        _rate_values[(uint)Rates.RATE_POWER_ENERGY] = ConfigMgr.GetOption("Rate.Energy", 1.0f);

        _rate_values[(uint)Rates.RATE_SKILL_DISCOVERY] = ConfigMgr.GetOption("Rate.Skill.Discovery", 1.0f);

        _rate_values[(uint)Rates.RATE_DROP_ITEM_POOR] = ConfigMgr.GetOption("Rate.Drop.Item.Poor", 1.0f);
        _rate_values[(uint)Rates.RATE_DROP_ITEM_NORMAL] = ConfigMgr.GetOption("Rate.Drop.Item.Normal", 1.0f);
        _rate_values[(uint)Rates.RATE_DROP_ITEM_UNCOMMON] = ConfigMgr.GetOption("Rate.Drop.Item.Uncommon", 1.0f);
        _rate_values[(uint)Rates.RATE_DROP_ITEM_RARE] = ConfigMgr.GetOption("Rate.Drop.Item.Rare", 1.0f);
        _rate_values[(uint)Rates.RATE_DROP_ITEM_EPIC] = ConfigMgr.GetOption("Rate.Drop.Item.Epic", 1.0f);
        _rate_values[(uint)Rates.RATE_DROP_ITEM_LEGENDARY] = ConfigMgr.GetOption("Rate.Drop.Item.Legendary", 1.0f);
        _rate_values[(uint)Rates.RATE_DROP_ITEM_ARTIFACT] = ConfigMgr.GetOption("Rate.Drop.Item.Artifact", 1.0f);
        _rate_values[(uint)Rates.RATE_DROP_ITEM_REFERENCED] = ConfigMgr.GetOption("Rate.Drop.Item.Referenced", 1.0f);
        _rate_values[(uint)Rates.RATE_DROP_ITEM_REFERENCED_AMOUNT] = ConfigMgr.GetOption("Rate.Drop.Item.ReferencedAmount", 1.0f);
        _rate_values[(uint)Rates.RATE_DROP_ITEM_GROUP_AMOUNT] = ConfigMgr.GetOption("Rate.Drop.Item.GroupAmount", 1.0f);
        _rate_values[(uint)Rates.RATE_DROP_MONEY] = ConfigMgr.GetOption("Rate.Drop.Money", 1.0f);

        _rate_values[(uint)Rates.RATE_REWARD_BONUS_MONEY] = ConfigMgr.GetOption("Rate.RewardBonusMoney", 1.0f);
        _rate_values[(uint)Rates.RATE_XP_KILL] = ConfigMgr.GetOption("Rate.XP.Kill", 1.0f);
        _rate_values[(uint)Rates.RATE_XP_BG_KILL_AV] = ConfigMgr.GetOption("Rate.XP.BattlegroundKillAV", 1.0f);
        _rate_values[(uint)Rates.RATE_XP_BG_KILL_WSG] = ConfigMgr.GetOption("Rate.XP.BattlegroundKillWSG", 1.0f);
        _rate_values[(uint)Rates.RATE_XP_BG_KILL_AB] = ConfigMgr.GetOption("Rate.XP.BattlegroundKillAB", 1.0f);
        _rate_values[(uint)Rates.RATE_XP_BG_KILL_EOTS] = ConfigMgr.GetOption("Rate.XP.BattlegroundKillEOTS", 1.0f);
        _rate_values[(uint)Rates.RATE_XP_BG_KILL_SOTA] = ConfigMgr.GetOption("Rate.XP.BattlegroundKillSOTA", 1.0f);
        _rate_values[(uint)Rates.RATE_XP_BG_KILL_IC] = ConfigMgr.GetOption("Rate.XP.BattlegroundKillIC", 1.0f);
        _rate_values[(uint)Rates.RATE_XP_QUEST] = ConfigMgr.GetOption("Rate.XP.Quest", 1.0f);
        _rate_values[(uint)Rates.RATE_XP_QUEST_DF] = ConfigMgr.GetOption("Rate.XP.Quest.DF", 1.0f);
        _rate_values[(uint)Rates.RATE_XP_EXPLORE] = ConfigMgr.GetOption("Rate.XP.Explore", 1.0f);
        _rate_values[(uint)Rates.RATE_XP_PET] = ConfigMgr.GetOption("Rate.XP.Pet", 1.0f);
        _rate_values[(uint)Rates.RATE_XP_PET_NEXT_LEVEL] = ConfigMgr.GetOption("Rate.Pet.LevelXP", 0.05f);
        _rate_values[(uint)Rates.RATE_REPAIRCOST] = ConfigMgr.GetOption("Rate.RepairCost", 1.0f);

        _rate_values[(uint)Rates.RATE_SELLVALUE_ITEM_POOR] = ConfigMgr.GetOption("Rate.SellValue.Item.Poor", 1.0f);
        _rate_values[(uint)Rates.RATE_SELLVALUE_ITEM_NORMAL] = ConfigMgr.GetOption("Rate.SellValue.Item.Normal", 1.0f);
        _rate_values[(uint)Rates.RATE_SELLVALUE_ITEM_UNCOMMON] = ConfigMgr.GetOption("Rate.SellValue.Item.Uncommon", 1.0f);
        _rate_values[(uint)Rates.RATE_SELLVALUE_ITEM_RARE] = ConfigMgr.GetOption("Rate.SellValue.Item.Rare", 1.0f);
        _rate_values[(uint)Rates.RATE_SELLVALUE_ITEM_EPIC] = ConfigMgr.GetOption("Rate.SellValue.Item.Epic", 1.0f);
        _rate_values[(uint)Rates.RATE_SELLVALUE_ITEM_LEGENDARY] = ConfigMgr.GetOption("Rate.SellValue.Item.Legendary", 1.0f);
        _rate_values[(uint)Rates.RATE_SELLVALUE_ITEM_ARTIFACT] = ConfigMgr.GetOption("Rate.SellValue.Item.Artifact", 1.0f);
        _rate_values[(uint)Rates.RATE_SELLVALUE_ITEM_HEIRLOOM] = ConfigMgr.GetOption("Rate.SellValue.Item.Heirloom", 1.0f);

        _rate_values[(uint)Rates.RATE_BUYVALUE_ITEM_POOR] = ConfigMgr.GetOption("Rate.BuyValue.Item.Poor", 1.0f);
        _rate_values[(uint)Rates.RATE_BUYVALUE_ITEM_NORMAL] = ConfigMgr.GetOption("Rate.BuyValue.Item.Normal", 1.0f);
        _rate_values[(uint)Rates.RATE_BUYVALUE_ITEM_UNCOMMON] = ConfigMgr.GetOption("Rate.BuyValue.Item.Uncommon", 1.0f);
        _rate_values[(uint)Rates.RATE_BUYVALUE_ITEM_RARE] = ConfigMgr.GetOption("Rate.BuyValue.Item.Rare", 1.0f);
        _rate_values[(uint)Rates.RATE_BUYVALUE_ITEM_EPIC] = ConfigMgr.GetOption("Rate.BuyValue.Item.Epic", 1.0f);
        _rate_values[(uint)Rates.RATE_BUYVALUE_ITEM_LEGENDARY] = ConfigMgr.GetOption("Rate.BuyValue.Item.Legendary", 1.0f);
        _rate_values[(uint)Rates.RATE_BUYVALUE_ITEM_ARTIFACT] = ConfigMgr.GetOption("Rate.BuyValue.Item.Artifact", 1.0f);
        _rate_values[(uint)Rates.RATE_BUYVALUE_ITEM_HEIRLOOM] = ConfigMgr.GetOption("Rate.BuyValue.Item.Heirloom", 1.0f);

        if (_rate_values[(uint)Rates.RATE_REPAIRCOST] < 0.0f)
        {
            logger.Error(LogFilter.ServerLoading, $"Rate.RepairCost ({_rate_values[(uint)Rates.RATE_REPAIRCOST]}) must be >=0. Using 0.0 instead.");
            _rate_values[(uint)Rates.RATE_REPAIRCOST] = 0.0f;
        }

        _rate_values[(uint)Rates.RATE_REPUTATION_GAIN] = ConfigMgr.GetOption("Rate.Reputation.Gain", 1.0f);
        _rate_values[(uint)Rates.RATE_REPUTATION_LOWLEVEL_KILL] = ConfigMgr.GetOption("Rate.Reputation.LowLevel.Kill", 1.0f);
        _rate_values[(uint)Rates.RATE_REPUTATION_LOWLEVEL_QUEST] = ConfigMgr.GetOption("Rate.Reputation.LowLevel.Quest", 1.0f);
        _rate_values[(uint)Rates.RATE_REPUTATION_RECRUIT_A_FRIEND_BONUS] = ConfigMgr.GetOption("Rate.Reputation.RecruitAFriendBonus", 0.1f);
        _rate_values[(uint)Rates.RATE_CREATURE_NORMAL_DAMAGE] = ConfigMgr.GetOption("Rate.Creature.Normal.Damage", 1.0f);
        _rate_values[(uint)Rates.RATE_CREATURE_ELITE_ELITE_DAMAGE] = ConfigMgr.GetOption("Rate.Creature.Elite.Elite.Damage", 1.0f);
        _rate_values[(uint)Rates.RATE_CREATURE_ELITE_RAREELITE_DAMAGE] = ConfigMgr.GetOption("Rate.Creature.Elite.RAREELITE.Damage", 1.0f);
        _rate_values[(uint)Rates.RATE_CREATURE_ELITE_WORLDBOSS_DAMAGE] = ConfigMgr.GetOption("Rate.Creature.Elite.WORLDBOSS.Damage", 1.0f);
        _rate_values[(uint)Rates.RATE_CREATURE_ELITE_RARE_DAMAGE] = ConfigMgr.GetOption("Rate.Creature.Elite.RARE.Damage", 1.0f);
        _rate_values[(uint)Rates.RATE_CREATURE_NORMAL_HP] = ConfigMgr.GetOption("Rate.Creature.Normal.HP", 1.0f);
        _rate_values[(uint)Rates.RATE_CREATURE_ELITE_ELITE_HP] = ConfigMgr.GetOption("Rate.Creature.Elite.Elite.HP", 1.0f);
        _rate_values[(uint)Rates.RATE_CREATURE_ELITE_RAREELITE_HP] = ConfigMgr.GetOption("Rate.Creature.Elite.RAREELITE.HP", 1.0f);
        _rate_values[(uint)Rates.RATE_CREATURE_ELITE_WORLDBOSS_HP] = ConfigMgr.GetOption("Rate.Creature.Elite.WORLDBOSS.HP", 1.0f);
        _rate_values[(uint)Rates.RATE_CREATURE_ELITE_RARE_HP] = ConfigMgr.GetOption("Rate.Creature.Elite.RARE.HP", 1.0f);
        _rate_values[(uint)Rates.RATE_CREATURE_NORMAL_SPELLDAMAGE] = ConfigMgr.GetOption("Rate.Creature.Normal.SpellDamage", 1.0f);
        _rate_values[(uint)Rates.RATE_CREATURE_ELITE_ELITE_SPELLDAMAGE] = ConfigMgr.GetOption("Rate.Creature.Elite.Elite.SpellDamage", 1.0f);
        _rate_values[(uint)Rates.RATE_CREATURE_ELITE_RAREELITE_SPELLDAMAGE] = ConfigMgr.GetOption("Rate.Creature.Elite.RAREELITE.SpellDamage", 1.0f);
        _rate_values[(uint)Rates.RATE_CREATURE_ELITE_WORLDBOSS_SPELLDAMAGE] = ConfigMgr.GetOption("Rate.Creature.Elite.WORLDBOSS.SpellDamage", 1.0f);
        _rate_values[(uint)Rates.RATE_CREATURE_ELITE_RARE_SPELLDAMAGE] = ConfigMgr.GetOption("Rate.Creature.Elite.RARE.SpellDamage", 1.0f);
        _rate_values[(uint)Rates.RATE_CREATURE_AGGRO] = ConfigMgr.GetOption("Rate.Creature.Aggro", 1.0f);
        _rate_values[(uint)Rates.RATE_REST_INGAME] = ConfigMgr.GetOption("Rate.Rest.InGame", 1.0f);
        _rate_values[(uint)Rates.RATE_REST_OFFLINE_IN_TAVERN_OR_CITY] = ConfigMgr.GetOption("Rate.Rest.Offline.InTavernOrCity", 1.0f);
        _rate_values[(uint)Rates.RATE_REST_OFFLINE_IN_WILDERNESS] = ConfigMgr.GetOption("Rate.Rest.Offline.InWilderness", 1.0f);
        _rate_values[(uint)Rates.RATE_DAMAGE_FALL] = ConfigMgr.GetOption("Rate.Damage.Fall", 1.0f);
        _rate_values[(uint)Rates.RATE_AUCTION_TIME] = ConfigMgr.GetOption("Rate.Auction.Time", 1.0f);
        _rate_values[(uint)Rates.RATE_AUCTION_DEPOSIT] = ConfigMgr.GetOption("Rate.Auction.Deposit", 1.0f);
        _rate_values[(uint)Rates.RATE_AUCTION_CUT] = ConfigMgr.GetOption("Rate.Auction.Cut", 1.0f);
        _rate_values[(uint)Rates.RATE_HONOR] = ConfigMgr.GetOption("Rate.Honor", 1.0f);
        _rate_values[(uint)Rates.RATE_ARENA_POINTS] = ConfigMgr.GetOption("Rate.ArenaPoints", 1.0f);
        _rate_values[(uint)Rates.RATE_INSTANCE_RESET_TIME] = ConfigMgr.GetOption("Rate.InstanceResetTime", 1.0f);

        _rate_values[(uint)Rates.RATE_MISS_CHANCE_MULTIPLIER_TARGET_CREATURE] = ConfigMgr.GetOption("Rate.MissChanceMultiplier.TargetCreature", 11.0f);
        _rate_values[(uint)Rates.RATE_MISS_CHANCE_MULTIPLIER_TARGET_PLAYER] = ConfigMgr.GetOption("Rate.MissChanceMultiplier.TargetPlayer", 7.0f);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_MISS_CHANCE_MULTIPLIER_ONLY_FOR_PLAYERS] = ConfigMgr.GetOption("Rate.MissChanceMultiplier.OnlyAffectsPlayer", false);

        _rate_values[(uint)Rates.RATE_TALENT] = ConfigMgr.GetOption("Rate.Talent", 1.0f);

        if (_rate_values[(uint)Rates.RATE_TALENT] < 0.0f)
        {
            logger.Error(LogFilter.ServerLoading, $"Rate.Talent ({_rate_values[(uint)Rates.RATE_TALENT]}) must be > 0. Using 1 instead.");
            _rate_values[(uint)Rates.RATE_TALENT] = 1.0f;
        }

        _rate_values[(uint)Rates.RATE_MOVESPEED] = ConfigMgr.GetOption("Rate.MoveSpeed", 1.0f);

        if (_rate_values[(uint)Rates.RATE_MOVESPEED] < 0)
        {
            logger.Error(LogFilter.ServerLoading, $"Rate.MoveSpeed ({_rate_values[(uint)Rates.RATE_MOVESPEED]}) must be > 0. Using 1 instead.");
            _rate_values[(uint)Rates.RATE_MOVESPEED] = 1.0f;
        }

        for (byte i = 0; i < UnitConst.MAX_MOVE_TYPE; ++i)
        {
            UnitConst.PlayerBaseMoveSpeed[i] = UnitConst.BaseMoveSpeed[i] * _rate_values[(uint)Rates.RATE_MOVESPEED];
        }

        _rate_values[(uint)Rates.RATE_CORPSE_DECAY_LOOTED] = ConfigMgr.GetOption("Rate.Corpse.Decay.Looted", 0.5f);

        _rate_values[(uint)Rates.RATE_TARGET_POS_RECALCULATION_RANGE] = ConfigMgr.GetOption("TargetPosRecalculateRange", 1.5f);

        if (_rate_values[(uint)Rates.RATE_TARGET_POS_RECALCULATION_RANGE] < ObjectDefines.CONTACT_DISTANCE)
        {
            logger.Error(LogFilter.ServerLoading, $"TargetPosRecalculateRange ({_rate_values[(uint)Rates.RATE_TARGET_POS_RECALCULATION_RANGE]}) must be >= {ObjectDefines.CONTACT_DISTANCE}. Using {ObjectDefines.CONTACT_DISTANCE} instead.");
            _rate_values[(uint)Rates.RATE_TARGET_POS_RECALCULATION_RANGE] = ObjectDefines.CONTACT_DISTANCE;
        }
        else if (_rate_values[(uint)Rates.RATE_TARGET_POS_RECALCULATION_RANGE] > ObjectDefines.NOMINAL_MELEE_RANGE)
        {
            logger.Error(LogFilter.ServerLoading, $"TargetPosRecalculateRange ({_rate_values[(uint)Rates.RATE_TARGET_POS_RECALCULATION_RANGE]}) must be <= {ObjectDefines.NOMINAL_MELEE_RANGE}. Using {ObjectDefines.NOMINAL_MELEE_RANGE} instead.");
            _rate_values[(uint)Rates.RATE_TARGET_POS_RECALCULATION_RANGE] = ObjectDefines.NOMINAL_MELEE_RANGE;
        }

        _rate_values[(uint)Rates.RATE_DURABILITY_LOSS_ON_DEATH] = ConfigMgr.GetOption("DurabilityLoss.OnDeath", 10.0f);

        if (_rate_values[(uint)Rates.RATE_DURABILITY_LOSS_ON_DEATH] < 0.0f)
        {
            logger.Error(LogFilter.ServerLoading, $"DurabilityLoss.OnDeath ({_rate_values[(uint)Rates.RATE_DURABILITY_LOSS_ON_DEATH]}) must be >=0. Using 0.0 instead.");
            _rate_values[(uint)Rates.RATE_DURABILITY_LOSS_ON_DEATH] = 0.0f;
        }

        if (_rate_values[(uint)Rates.RATE_DURABILITY_LOSS_ON_DEATH] > 100.0f)
        {
            logger.Error(LogFilter.ServerLoading, $"DurabilityLoss.OnDeath ({_rate_values[(uint)Rates.RATE_DURABILITY_LOSS_ON_DEATH]}) must be <= 100. Using 100.0 instead.");
            _rate_values[(uint)Rates.RATE_DURABILITY_LOSS_ON_DEATH] = 0.0f;
        }

        _rate_values[(uint)Rates.RATE_DURABILITY_LOSS_ON_DEATH] = _rate_values[(uint)Rates.RATE_DURABILITY_LOSS_ON_DEATH] / 100.0f;

        _rate_values[(uint)Rates.RATE_DURABILITY_LOSS_DAMAGE] = ConfigMgr.GetOption("DurabilityLossChance.Damage", 0.5f);

        if (_rate_values[(uint)Rates.RATE_DURABILITY_LOSS_DAMAGE] < 0.0f)
        {
            logger.Error(LogFilter.ServerLoading, $"DurabilityLossChance.Damage ({_rate_values[(uint)Rates.RATE_DURABILITY_LOSS_DAMAGE]}) must be >=0. Using 0.0 instead.");
            _rate_values[(uint)Rates.RATE_DURABILITY_LOSS_DAMAGE] = 0.0f;
        }

        _rate_values[(uint)Rates.RATE_DURABILITY_LOSS_ABSORB] = ConfigMgr.GetOption("DurabilityLossChance.Absorb", 0.5f);

        if (_rate_values[(uint)Rates.RATE_DURABILITY_LOSS_ABSORB] < 0.0f)
        {
            logger.Error(LogFilter.ServerLoading, $"DurabilityLossChance.Absorb ({_rate_values[(uint)Rates.RATE_DURABILITY_LOSS_ABSORB]}) must be >=0. Using 0.0 instead.");
            _rate_values[(uint)Rates.RATE_DURABILITY_LOSS_ABSORB] = 0.0f;
        }

        _rate_values[(uint)Rates.RATE_DURABILITY_LOSS_PARRY] = ConfigMgr.GetOption("DurabilityLossChance.Parry", 0.05f);

        if (_rate_values[(uint)Rates.RATE_DURABILITY_LOSS_PARRY] < 0.0f)
        {
            logger.Error(LogFilter.ServerLoading, $"DurabilityLossChance.Parry ({_rate_values[(uint)Rates.RATE_DURABILITY_LOSS_PARRY]}) must be >=0. Using 0.0 instead.");
            _rate_values[(uint)Rates.RATE_DURABILITY_LOSS_PARRY] = 0.0f;
        }

        _rate_values[(uint)Rates.RATE_DURABILITY_LOSS_BLOCK] = ConfigMgr.GetOption("DurabilityLossChance.Block", 0.05f);

        if (_rate_values[(uint)Rates.RATE_DURABILITY_LOSS_BLOCK] < 0.0f)
        {
            logger.Error(LogFilter.ServerLoading, $"DurabilityLossChance.Block ({_rate_values[(uint)Rates.RATE_DURABILITY_LOSS_BLOCK]}) must be >=0. Using 0.0 instead.");
            _rate_values[(uint)Rates.RATE_DURABILITY_LOSS_BLOCK] = 0.0f;
        }

        // Read other configuration items from the config file

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_DURABILITY_LOSS_IN_PVP] = ConfigMgr.GetOption("DurabilityLoss.InPvP", false);

        _int_configs[(uint)WorldIntConfigs.CONFIG_COMPRESSION] = ConfigMgr.GetOption("Compression", 1U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_COMPRESSION] < 1 || _int_configs[(uint)WorldIntConfigs.CONFIG_COMPRESSION] > 9)
        {
            logger.Error(LogFilter.ServerLoading, $"Compression level ({_int_configs[(uint)WorldIntConfigs.CONFIG_COMPRESSION]}) must be in range 1..9. Using default compression level (1).");
            _int_configs[(uint)WorldIntConfigs.CONFIG_COMPRESSION] = 1;
        }

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ADDON_CHANNEL] = ConfigMgr.GetOption("AddonChannel", true);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_CLEAN_CHARACTER_DB] = ConfigMgr.GetOption("CleanCharacterDB", false);
        _int_configs[(uint)WorldIntConfigs.CONFIG_PERSISTENT_CHARACTER_CLEAN_FLAGS] = ConfigMgr.GetOption("PersistentCharacterCleanFlags", 0U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CHAT_CHANNEL_LEVEL_REQ] = ConfigMgr.GetOption("ChatLevelReq.Channel", 1U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CHAT_WHISPER_LEVEL_REQ] = ConfigMgr.GetOption("ChatLevelReq.Whisper", 1U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CHAT_SAY_LEVEL_REQ] = ConfigMgr.GetOption("ChatLevelReq.Say", 1U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_PARTY_LEVEL_REQ] = ConfigMgr.GetOption("PartyLevelReq", 1U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_TRADE_LEVEL_REQ] = ConfigMgr.GetOption("LevelReq.Trade", 1U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_TICKET_LEVEL_REQ] = ConfigMgr.GetOption("LevelReq.Ticket", 1U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_AUCTION_LEVEL_REQ] = ConfigMgr.GetOption("LevelReq.Auction", 1U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_MAIL_LEVEL_REQ] = ConfigMgr.GetOption("LevelReq.Mail", 1U);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALLOW_PLAYER_COMMANDS] = ConfigMgr.GetOption("AllowPlayerCommands", true);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_PRESERVE_CUSTOM_CHANNELS] = ConfigMgr.GetOption("PreserveCustomChannels", false);
        _int_configs[(uint)WorldIntConfigs.CONFIG_PRESERVE_CUSTOM_CHANNEL_DURATION] = ConfigMgr.GetOption("PreserveCustomChannelDuration", 14U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_INTERVAL_SAVE] = ConfigMgr.GetOption("PlayerSaveInterval", 15U * (uint)TimeConstants.MINUTE * (uint)TimeConstants.IN_MILLISECONDS);
        _int_configs[(uint)WorldIntConfigs.CONFIG_INTERVAL_DISCONNECT_TOLERANCE] = ConfigMgr.GetOption("DisconnectToleranceInterval", 0U);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_STATS_SAVE_ONLY_ON_LOGOUT] = ConfigMgr.GetOption("PlayerSave.Stats.SaveOnlyOnLogout", true);

        _int_configs[(uint)WorldIntConfigs.CONFIG_MIN_LEVEL_STAT_SAVE] = ConfigMgr.GetOption("PlayerSave.Stats.MinLevel", 0U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_MIN_LEVEL_STAT_SAVE] > DBCConst.MAX_LEVEL || _int_configs[(uint)WorldIntConfigs.CONFIG_MIN_LEVEL_STAT_SAVE] < 0)
        {
            logger.Error(LogFilter.ServerLoading, $"PlayerSave.Stats.MinLevel ({_int_configs[(uint)WorldIntConfigs.CONFIG_MIN_LEVEL_STAT_SAVE]}) must be in range 0..80. Using default, do not save character stats (0).");
            _int_configs[(uint)WorldIntConfigs.CONFIG_MIN_LEVEL_STAT_SAVE] = 0;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_INTERVAL_MAPUPDATE] = ConfigMgr.GetOption("MapUpdateInterval", 10U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_INTERVAL_MAPUPDATE] < GridDefines.MIN_MAP_UPDATE_DELAY)
        {
            logger.Error(LogFilter.ServerLoading, $"MapUpdateInterval ({_int_configs[(uint)WorldIntConfigs.CONFIG_INTERVAL_MAPUPDATE]}) must be greater {GridDefines.MIN_MAP_UPDATE_DELAY}. Use this minimal value.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_INTERVAL_MAPUPDATE] = (uint)GridDefines.MIN_MAP_UPDATE_DELAY;
        }

        if (reload)
        {
            Global.sMapMgr.SetMapUpdateInterval(_int_configs[(uint)WorldIntConfigs.CONFIG_INTERVAL_MAPUPDATE]);
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_INTERVAL_CHANGEWEATHER] = ConfigMgr.GetOption("ChangeWeatherInterval", 10U * (uint)TimeConstants.MINUTE * (uint)TimeConstants.IN_MILLISECONDS);

        if (reload)
        {
            uint val = ConfigMgr.GetOption("WorldServerPort", 8085U);

            if (val != _int_configs[(uint)WorldIntConfigs.CONFIG_PORT_WORLD])
            {
                logger.Error(LogFilter.ServerLoading, $"WorldServerPort option can't be changed at worldserver.conf reload, using current value ({_int_configs[(uint)WorldIntConfigs.CONFIG_PORT_WORLD]}).");
            }
        }
        else
        {
            _int_configs[(uint)WorldIntConfigs.CONFIG_PORT_WORLD] = ConfigMgr.GetOption("WorldServerPort", 8085U);
        }

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_CLOSE_IDLE_CONNECTIONS] = ConfigMgr.GetOption("CloseIdleConnections", true);
        _int_configs[(uint)WorldIntConfigs.CONFIG_SOCKET_TIMEOUTTIME] = ConfigMgr.GetOption("SocketTimeOutTime", 900000U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_SOCKET_TIMEOUTTIME_ACTIVE] = ConfigMgr.GetOption("SocketTimeOutTimeActive", 60000U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_SESSION_ADD_DELAY] = ConfigMgr.GetOption("SessionAddDelay", 10000U);

        _float_configs[(uint)WorldFloatConfigs.CONFIG_GROUP_XP_DISTANCE] = ConfigMgr.GetOption("MaxGroupXPDistance", 74.0f);
        _float_configs[(uint)WorldFloatConfigs.CONFIG_MAX_RECRUIT_A_FRIEND_DISTANCE] = ConfigMgr.GetOption("MaxRecruitAFriendBonusDistance", 100.0f);
        
        // Add MonsterSight in worldserver.conf or put it as define
        _float_configs[(uint)WorldFloatConfigs.CONFIG_SIGHT_MONSTER] = ConfigMgr.GetOption("MonsterSight", 50.0f);

        if (reload)
        {
            uint val = ConfigMgr.GetOption("GameType", 0U);

            if (val != _int_configs[(uint)WorldIntConfigs.CONFIG_GAME_TYPE])
            {
                logger.Error(LogFilter.ServerLoading, $"GameType option can't be changed at worldserver.conf reload, using current value ({_int_configs[(uint)WorldIntConfigs.CONFIG_GAME_TYPE]}).");
            }
        }
        else
        {
            _int_configs[(uint)WorldIntConfigs.CONFIG_GAME_TYPE] = ConfigMgr.GetOption("GameType", 0U);
        }

        if (reload)
        {
            uint val = ConfigMgr.GetOption("RealmZone", (uint)RealmZone.REALM_ZONE_DEVELOPMENT);

            if (val != _int_configs[(uint)WorldIntConfigs.CONFIG_REALM_ZONE])
            {
                logger.Error(LogFilter.ServerLoading, $"RealmZone option can't be changed at worldserver.conf reload, using current value ({_int_configs[(uint)WorldIntConfigs.CONFIG_REALM_ZONE]}).");
            }
        }
        else
        {
            _int_configs[(uint)WorldIntConfigs.CONFIG_REALM_ZONE] = ConfigMgr.GetOption("RealmZone", (uint)RealmZone.REALM_ZONE_DEVELOPMENT);
        }

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_STRICT_NAMES_RESERVED] = ConfigMgr.GetOption("StrictNames.Reserved", true);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_STRICT_NAMES_PROFANITY] = ConfigMgr.GetOption("StrictNames.Profanity", true);
        _int_configs[(uint)WorldIntConfigs.CONFIG_STRICT_PLAYER_NAMES] = ConfigMgr.GetOption("StrictPlayerNames", 0U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_STRICT_CHARTER_NAMES] = ConfigMgr.GetOption("StrictCharterNames", 0U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_STRICT_CHANNEL_NAMES] = ConfigMgr.GetOption("StrictChannelNames", 0U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_STRICT_PET_NAMES] = ConfigMgr.GetOption("StrictPetNames", 0U);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALLOW_TWO_SIDE_ACCOUNTS] = ConfigMgr.GetOption("AllowTwoSide.Accounts", true);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALLOW_TWO_SIDE_INTERACTION_CALENDAR] = ConfigMgr.GetOption("AllowTwoSide.Interaction.Calendar", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALLOW_TWO_SIDE_INTERACTION_CHAT] = ConfigMgr.GetOption("AllowTwoSide.Interaction.Chat", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALLOW_TWO_SIDE_INTERACTION_CHANNEL] = ConfigMgr.GetOption("AllowTwoSide.Interaction.Channel", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALLOW_TWO_SIDE_INTERACTION_GROUP] = ConfigMgr.GetOption("AllowTwoSide.Interaction.Group", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALLOW_TWO_SIDE_INTERACTION_GUILD] = ConfigMgr.GetOption("AllowTwoSide.Interaction.Guild", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALLOW_TWO_SIDE_INTERACTION_ARENA] = ConfigMgr.GetOption("AllowTwoSide.Interaction.Arena", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALLOW_TWO_SIDE_INTERACTION_AUCTION] = ConfigMgr.GetOption("AllowTwoSide.Interaction.Auction", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALLOW_TWO_SIDE_INTERACTION_MAIL] = ConfigMgr.GetOption("AllowTwoSide.Interaction.Mail", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALLOW_TWO_SIDE_WHO_LIST] = ConfigMgr.GetOption("AllowTwoSide.WhoList", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALLOW_TWO_SIDE_ADD_FRIEND] = ConfigMgr.GetOption("AllowTwoSide.AddFriend", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALLOW_TWO_SIDE_TRADE] = ConfigMgr.GetOption("AllowTwoSide.Trade", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALLOW_TWO_SIDE_INTERACTION_EMOTE] = ConfigMgr.GetOption("AllowTwoSide.Interaction.Emote", false);

        _int_configs[(uint)WorldIntConfigs.CONFIG_MIN_PLAYER_NAME] = ConfigMgr.GetOption("MinPlayerName", 2U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_MIN_PLAYER_NAME] < 1 || _int_configs[(uint)WorldIntConfigs.CONFIG_MIN_PLAYER_NAME] > ObjectMgr.MAX_PLAYER_NAME)
        {
            logger.Error(LogFilter.ServerLoading, $"MinPlayerName ({_int_configs[(uint)WorldIntConfigs.CONFIG_MIN_PLAYER_NAME]}) must be in range 1..{ObjectMgr.MAX_PLAYER_NAME}. Set to 2.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_MIN_PLAYER_NAME] = 2;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_MIN_CHARTER_NAME] = ConfigMgr.GetOption("MinCharterName", 2U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_MIN_CHARTER_NAME] < 1 || _int_configs[(uint)WorldIntConfigs.CONFIG_MIN_CHARTER_NAME] > ObjectMgr.MAX_CHARTER_NAME)
        {
            logger.Error(LogFilter.ServerLoading, $"MinCharterName ({_int_configs[(uint)WorldIntConfigs.CONFIG_MIN_CHARTER_NAME]}) must be in range 1..{ObjectMgr.MAX_CHARTER_NAME}. Set to 2.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_MIN_CHARTER_NAME] = 2;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_MIN_PET_NAME] = ConfigMgr.GetOption("MinPetName", 2U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_MIN_PET_NAME] < 1 || _int_configs[(uint)WorldIntConfigs.CONFIG_MIN_PET_NAME] > ObjectMgr.MAX_PET_NAME)
        {
            logger.Error(LogFilter.ServerLoading, $"MinPetName ({_int_configs[(uint)WorldIntConfigs.CONFIG_MIN_PET_NAME]}) must be in range 1..{ObjectMgr.MAX_PET_NAME}. Set to 2.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_MIN_PET_NAME] = 2;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_CHARTER_COST_GUILD] = ConfigMgr.GetOption("Guild.CharterCost", 1000U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CHARTER_COST_ARENA_2v2] = ConfigMgr.GetOption("ArenaTeam.CharterCost.2v2", 800000U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CHARTER_COST_ARENA_3v3] = ConfigMgr.GetOption("ArenaTeam.CharterCost.3v3", 1200000U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CHARTER_COST_ARENA_5v5] = ConfigMgr.GetOption("ArenaTeam.CharterCost.5v5", 2000000U);

        _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_WHO_LIST_RETURN] = ConfigMgr.GetOption("MaxWhoListReturns", 49U);

        _int_configs[(uint)WorldIntConfigs.CONFIG_CHARACTER_CREATING_DISABLED] = ConfigMgr.GetOption("CharacterCreating.Disabled", 0U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CHARACTER_CREATING_DISABLED_RACEMASK] = ConfigMgr.GetOption("CharacterCreating.Disabled.RaceMask", 0U);

        _int_configs[(uint)WorldIntConfigs.CONFIG_CHARACTER_CREATING_DISABLED_CLASSMASK] = ConfigMgr.GetOption("CharacterCreating.Disabled.ClassMask", 0U);

        _int_configs[(uint)WorldIntConfigs.CONFIG_CHARACTERS_PER_REALM] = ConfigMgr.GetOption("CharactersPerRealm", 10U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_CHARACTERS_PER_REALM] < 1 || _int_configs[(uint)WorldIntConfigs.CONFIG_CHARACTERS_PER_REALM] > 10)
        {
            logger.Error(LogFilter.ServerLoading, $"CharactersPerRealm ({_int_configs[(uint)WorldIntConfigs.CONFIG_CHARACTERS_PER_REALM]}) must be in range 1..10. Set to 10.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_CHARACTERS_PER_REALM] = 10;
        }

        // must be after CONFIG_CHARACTERS_PER_REALM
        _int_configs[(uint)WorldIntConfigs.CONFIG_CHARACTERS_PER_ACCOUNT] = ConfigMgr.GetOption("CharactersPerAccount", 50U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_CHARACTERS_PER_ACCOUNT] < _int_configs[(uint)WorldIntConfigs.CONFIG_CHARACTERS_PER_REALM])
        {
            logger.Error(LogFilter.ServerLoading, $"CharactersPerAccount ({_int_configs[(uint)WorldIntConfigs.CONFIG_CHARACTERS_PER_ACCOUNT]}) can't be less than CharactersPerRealm ({_int_configs[(uint)WorldIntConfigs.CONFIG_CHARACTERS_PER_REALM]}).");
            _int_configs[(uint)WorldIntConfigs.CONFIG_CHARACTERS_PER_ACCOUNT] = _int_configs[(uint)WorldIntConfigs.CONFIG_CHARACTERS_PER_REALM];
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_HEROIC_CHARACTERS_PER_REALM] = ConfigMgr.GetOption("HeroicCharactersPerRealm", 1U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_HEROIC_CHARACTERS_PER_REALM] < 0 || _int_configs[(uint)WorldIntConfigs.CONFIG_HEROIC_CHARACTERS_PER_REALM] > 10)
        {
            logger.Error(LogFilter.ServerLoading, $"HeroicCharactersPerRealm ({_int_configs[(uint)WorldIntConfigs.CONFIG_HEROIC_CHARACTERS_PER_REALM]}) must be in range 0..10. Set to 1.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_HEROIC_CHARACTERS_PER_REALM] = 1;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_CHARACTER_CREATING_MIN_LEVEL_FOR_HEROIC_CHARACTER] = ConfigMgr.GetOption("CharacterCreating.MinLevelForHeroicCharacter", 55U);

        _int_configs[(uint)WorldIntConfigs.CONFIG_SKIP_CINEMATICS] = ConfigMgr.GetOption("SkipCinematics", 0U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_SKIP_CINEMATICS] < 0 || _int_configs[(uint)WorldIntConfigs.CONFIG_SKIP_CINEMATICS] > 2)
        {
            logger.Error(LogFilter.ServerLoading, $"SkipCinematics ({_int_configs[(uint)WorldIntConfigs.CONFIG_SKIP_CINEMATICS]}) must be in range 0..2. Set to 0.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_SKIP_CINEMATICS] = 0;
        }

        if (reload)
        {
            uint val = ConfigMgr.GetOption("MaxPlayerLevel", (uint)DBCConst.DEFAULT_MAX_LEVEL);

            if (val != _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_PLAYER_LEVEL])
            {
                logger.Error(LogFilter.ServerLoading, $"MaxPlayerLevel option can't be changed at config reload, using current value ({_int_configs[(uint)WorldIntConfigs.CONFIG_MAX_PLAYER_LEVEL]}).");
            }
        }
        else
        {
            _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_PLAYER_LEVEL] = ConfigMgr.GetOption("MaxPlayerLevel", (uint)DBCConst.DEFAULT_MAX_LEVEL);
        }

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_MAX_PLAYER_LEVEL] > DBCConst.MAX_LEVEL || _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_PLAYER_LEVEL] < 1)
        {
            logger.Error(LogFilter.ServerLoading, $"MaxPlayerLevel ({_int_configs[(uint)WorldIntConfigs.CONFIG_MAX_PLAYER_LEVEL]}) must be in range 1..{DBCConst.MAX_LEVEL}. Set to {DBCConst.MAX_LEVEL}.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_PLAYER_LEVEL] = (uint)DBCConst.MAX_LEVEL;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_MIN_DUALSPEC_LEVEL] = ConfigMgr.GetOption("MinDualSpecLevel", 40U);

        _int_configs[(uint)WorldIntConfigs.CONFIG_START_PLAYER_LEVEL] = ConfigMgr.GetOption("StartPlayerLevel", 1U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_START_PLAYER_LEVEL] < 1 || _int_configs[(uint)WorldIntConfigs.CONFIG_START_PLAYER_LEVEL] > _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_PLAYER_LEVEL])
        {
            logger.Error(LogFilter.ServerLoading, $"StartPlayerLevel ({_int_configs[(uint)WorldIntConfigs.CONFIG_START_PLAYER_LEVEL]}) must be in range 1..MaxPlayerLevel({_int_configs[(uint)WorldIntConfigs.CONFIG_MAX_PLAYER_LEVEL]}). Set to 1.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_START_PLAYER_LEVEL] = 1;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_START_HEROIC_PLAYER_LEVEL] = ConfigMgr.GetOption("StartHeroicPlayerLevel", 55U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_START_HEROIC_PLAYER_LEVEL] < 1 || _int_configs[(uint)WorldIntConfigs.CONFIG_START_HEROIC_PLAYER_LEVEL] > _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_PLAYER_LEVEL])
        {
            logger.Error(LogFilter.ServerLoading, $"StartHeroicPlayerLevel ({_int_configs[(uint)WorldIntConfigs.CONFIG_START_HEROIC_PLAYER_LEVEL]}) must be in range 1..MaxPlayerLevel({_int_configs[(uint)WorldIntConfigs.CONFIG_MAX_PLAYER_LEVEL]}). Set to 55.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_START_HEROIC_PLAYER_LEVEL] = 55;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_START_PLAYER_MONEY] = ConfigMgr.GetOption("StartPlayerMoney", 0U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_START_PLAYER_MONEY] < 0 ||
            _int_configs[(uint)WorldIntConfigs.CONFIG_START_PLAYER_MONEY] > PlayerConst.MAX_MONEY_AMOUNT)
        {
            logger.Error(LogFilter.ServerLoading, $"StartPlayerMoney ({_int_configs[(uint)WorldIntConfigs.CONFIG_START_PLAYER_MONEY]}) must be in range 0..{PlayerConst.MAX_MONEY_AMOUNT}. Set to {0}.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_START_PLAYER_MONEY] = 0;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_START_HEROIC_PLAYER_MONEY] = ConfigMgr.GetOption("StartHeroicPlayerMoney", 2000U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_START_HEROIC_PLAYER_MONEY] < 0 ||
            _int_configs[(uint)WorldIntConfigs.CONFIG_START_HEROIC_PLAYER_MONEY] > PlayerConst.MAX_MONEY_AMOUNT)
        {
            logger.Error(LogFilter.ServerLoading, $"StartHeroicPlayerMoney ({_int_configs[(uint)WorldIntConfigs.CONFIG_START_HEROIC_PLAYER_MONEY]}) must be in range 0..{PlayerConst.MAX_MONEY_AMOUNT}. Set to {2000}.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_START_HEROIC_PLAYER_MONEY] = 2000;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_HONOR_POINTS] = ConfigMgr.GetOption("MaxHonorPoints", 75000U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_MAX_HONOR_POINTS] < 0)
        {
            logger.Error(LogFilter.ServerLoading, $"MaxHonorPoints ({_int_configs[(uint)WorldIntConfigs.CONFIG_MAX_HONOR_POINTS]}) can't be negative. Set to 0.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_HONOR_POINTS] = 0;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_HONOR_POINTS_MONEY_PER_POINT] = ConfigMgr.GetOption("MaxHonorPointsMoneyPerPoint", 0U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_MAX_HONOR_POINTS_MONEY_PER_POINT] < 0)
        {
            logger.Error(LogFilter.ServerLoading, $"MaxHonorPointsMoneyPerPoint ({_int_configs[(uint)WorldIntConfigs.CONFIG_MAX_HONOR_POINTS_MONEY_PER_POINT]}) can't be negative. Set to 0.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_HONOR_POINTS_MONEY_PER_POINT] = 0;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_START_HONOR_POINTS] = ConfigMgr.GetOption("StartHonorPoints", 0U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_START_HONOR_POINTS] < 0 ||
            _int_configs[(uint)WorldIntConfigs.CONFIG_START_HONOR_POINTS] > _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_HONOR_POINTS])
        {
            logger.Error(LogFilter.ServerLoading, $"StartHonorPoints ({_int_configs[(uint)WorldIntConfigs.CONFIG_START_HONOR_POINTS]}) must be in range 0..MaxHonorPoints({_int_configs[(uint)WorldIntConfigs.CONFIG_MAX_HONOR_POINTS]}). Set to {0}.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_START_HONOR_POINTS] = 0;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_ARENA_POINTS] = ConfigMgr.GetOption("MaxArenaPoints", 10000U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_MAX_ARENA_POINTS] < 0)
        {
            logger.Error(LogFilter.ServerLoading, $"MaxArenaPoints ({_int_configs[(uint)WorldIntConfigs.CONFIG_MAX_ARENA_POINTS]}) can't be negative. Set to 0.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_ARENA_POINTS] = 0;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_START_ARENA_POINTS] = ConfigMgr.GetOption("StartArenaPoints", 0U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_START_ARENA_POINTS] < 0 ||
            _int_configs[(uint)WorldIntConfigs.CONFIG_START_ARENA_POINTS] > _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_ARENA_POINTS])
        {
            logger.Error(LogFilter.ServerLoading, $"StartArenaPoints ({_int_configs[(uint)WorldIntConfigs.CONFIG_START_ARENA_POINTS]}) must be in range 0..MaxArenaPoints({_int_configs[(uint)WorldIntConfigs.CONFIG_MAX_ARENA_POINTS]}). Set to {0}.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_START_ARENA_POINTS] = 0;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_RECRUIT_A_FRIEND_BONUS_PLAYER_LEVEL] = ConfigMgr.GetOption("RecruitAFriend.MaxLevel", 60U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_MAX_RECRUIT_A_FRIEND_BONUS_PLAYER_LEVEL] > _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_PLAYER_LEVEL] ||
            _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_RECRUIT_A_FRIEND_BONUS_PLAYER_LEVEL] < 0)
        {
            logger.Error(LogFilter.ServerLoading, $"RecruitAFriend.MaxLevel ({_int_configs[(uint)WorldIntConfigs.CONFIG_MAX_RECRUIT_A_FRIEND_BONUS_PLAYER_LEVEL]}) must be in the range 0..MaxLevel({_int_configs[(uint)WorldIntConfigs.CONFIG_MAX_PLAYER_LEVEL]}). Set to {60}.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_RECRUIT_A_FRIEND_BONUS_PLAYER_LEVEL] = 60;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_RECRUIT_A_FRIEND_BONUS_PLAYER_LEVEL_DIFFERENCE] = ConfigMgr.GetOption("RecruitAFriend.MaxDifference", 4U);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALL_TAXI_PATHS] = ConfigMgr.GetOption("AllFlightPaths", false);
        _int_configs[(uint)WorldIntConfigs.CONFIG_INSTANT_TAXI] = ConfigMgr.GetOption("InstantFlightPaths", 0U);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_INSTANCE_IGNORE_LEVEL] = ConfigMgr.GetOption("Instance.IgnoreLevel", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_INSTANCE_IGNORE_RAID] = ConfigMgr.GetOption("Instance.IgnoreRaid", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_INSTANCE_GMSUMMON_PLAYER] = ConfigMgr.GetOption("Instance.GMSummonPlayer", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_INSTANCE_SHARED_ID] = ConfigMgr.GetOption("Instance.SharedNormalHeroicId", false);

        _int_configs[(uint)WorldIntConfigs.CONFIG_INSTANCE_RESET_TIME_HOUR] = ConfigMgr.GetOption("Instance.ResetTimeHour", 4U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_INSTANCE_RESET_TIME_RELATIVE_TIMESTAMP] = ConfigMgr.GetOption("Instance.ResetTimeRelativeTimestamp", 1135814400U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_INSTANCE_UNLOAD_DELAY] = ConfigMgr.GetOption("Instance.UnloadDelay", 30 * (uint)TimeConstants.MINUTE * (uint)TimeConstants.IN_MILLISECONDS);

        _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_PRIMARY_TRADE_SKILL] = ConfigMgr.GetOption("MaxPrimaryTradeSkill", 2U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_MIN_PETITION_SIGNS] = ConfigMgr.GetOption("MinPetitionSigns", 9U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_MIN_PETITION_SIGNS] > 9 || (int)_int_configs[(uint)WorldIntConfigs.CONFIG_MIN_PETITION_SIGNS] < 0)
        {
            logger.Error(LogFilter.ServerLoading, $"MinPetitionSigns ({_int_configs[(uint)WorldIntConfigs.CONFIG_MIN_PETITION_SIGNS]}) must be in range 0..9. Set to 9.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_MIN_PETITION_SIGNS] = 9;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_GM_LOGIN_STATE] = ConfigMgr.GetOption("GM.LoginState", 2U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_GM_VISIBLE_STATE] = ConfigMgr.GetOption("GM.Visible", 2U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_GM_CHAT] = ConfigMgr.GetOption("GM.Chat", 2U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_GM_WHISPERING_TO] = ConfigMgr.GetOption("GM.WhisperingTo", 2U);

        _int_configs[(uint)WorldIntConfigs.CONFIG_GM_LEVEL_IN_GM_LIST] = ConfigMgr.GetOption("GM.InGMList.Level", (uint)AccountTypes.SEC_ADMINISTRATOR);
        _int_configs[(uint)WorldIntConfigs.CONFIG_GM_LEVEL_IN_WHO_LIST] = ConfigMgr.GetOption("GM.InWhoList.Level", (uint)AccountTypes.SEC_ADMINISTRATOR);
        _int_configs[(uint)WorldIntConfigs.CONFIG_START_GM_LEVEL] = ConfigMgr.GetOption("GM.StartLevel", 1U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_START_GM_LEVEL] < _int_configs[(uint)WorldIntConfigs.CONFIG_START_PLAYER_LEVEL])
        {
            logger.Error(LogFilter.ServerLoading, $"GM.StartLevel ({_int_configs[(uint)WorldIntConfigs.CONFIG_START_GM_LEVEL]}) must be in range StartPlayerLevel({_int_configs[(uint)WorldIntConfigs.CONFIG_START_PLAYER_LEVEL]})..{DBCConst.MAX_LEVEL}. Set to {_int_configs[(uint)WorldIntConfigs.CONFIG_START_PLAYER_LEVEL]}.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_START_GM_LEVEL] = _int_configs[(uint)WorldIntConfigs.CONFIG_START_PLAYER_LEVEL];
        }
        else if (_int_configs[(uint)WorldIntConfigs.CONFIG_START_GM_LEVEL] > DBCConst.MAX_LEVEL)
        {
            logger.Error(LogFilter.ServerLoading, $"GM.StartLevel ({_int_configs[(uint)WorldIntConfigs.CONFIG_START_GM_LEVEL]}) must be in range 1..{DBCConst.MAX_LEVEL}. Set to {DBCConst.MAX_LEVEL}.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_START_GM_LEVEL] = (uint)DBCConst.MAX_LEVEL;
        }

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALLOW_GM_GROUP] = ConfigMgr.GetOption("GM.AllowInvite", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALLOW_GM_FRIEND] = ConfigMgr.GetOption("GM.AllowFriend", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_GM_LOWER_SECURITY] = ConfigMgr.GetOption("GM.LowerSecurity", false);
        _float_configs[(uint)WorldFloatConfigs.CONFIG_CHANCE_OF_GM_SURVEY] = ConfigMgr.GetOption("GM.TicketSystem.ChanceOfGMSurvey", 50.0f);

        _int_configs[(uint)WorldIntConfigs.CONFIG_GROUP_VISIBILITY] = ConfigMgr.GetOption("Visibility.GroupMode", 1U);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_OBJECT_SPARKLES] = ConfigMgr.GetOption("Visibility.ObjectSparkles", true);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_LOW_LEVEL_REGEN_BOOST] = ConfigMgr.GetOption("EnableLowLevelRegenBoost", true);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_OBJECT_QUEST_MARKERS] = ConfigMgr.GetOption("Visibility.ObjectQuestMarkers", true);

        _int_configs[(uint)WorldIntConfigs.CONFIG_MAIL_DELIVERY_DELAY] = ConfigMgr.GetOption("MailDeliveryDelay", (uint)TimeConstants.HOUR);

        _int_configs[(uint)WorldIntConfigs.CONFIG_UPTIME_UPDATE] = ConfigMgr.GetOption("UpdateUptimeInterval", 10U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_UPTIME_UPDATE] <= 0)
        {
            logger.Error(LogFilter.ServerLoading, $"UpdateUptimeInterval ({_int_configs[(uint)WorldIntConfigs.CONFIG_UPTIME_UPDATE]}) must be > 0, set to default 10.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_UPTIME_UPDATE] = 1;
        }

        if (reload)
        {
            _timers[(int)WorldTimers.WUPDATE_UPTIME].SetInterval(_int_configs[(uint)WorldIntConfigs.CONFIG_UPTIME_UPDATE] * (uint)TimeConstants.MINUTE * (uint)TimeConstants.IN_MILLISECONDS);
            _timers[(int)WorldTimers.WUPDATE_UPTIME].Reset();
        }

        // log db cleanup interval
        _int_configs[(uint)WorldIntConfigs.CONFIG_LOGDB_CLEARINTERVAL] = ConfigMgr.GetOption("LogDB.Opt.ClearInterval", 10U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_LOGDB_CLEARINTERVAL] <= 0)
        {
            logger.Error(LogFilter.ServerLoading, $"LogDB.Opt.ClearInterval ({_int_configs[(uint)WorldIntConfigs.CONFIG_LOGDB_CLEARINTERVAL]}) must be > 0, set to default 10.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_LOGDB_CLEARINTERVAL] = 10;
        }

        if (reload)
        {
            _timers[(int)WorldTimers.WUPDATE_CLEANDB].SetInterval(_int_configs[(uint)WorldIntConfigs.CONFIG_LOGDB_CLEARINTERVAL] * (uint)TimeConstants.MINUTE * (uint)TimeConstants.IN_MILLISECONDS);
            _timers[(int)WorldTimers.WUPDATE_CLEANDB].Reset();
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_LOGDB_CLEARTIME] = ConfigMgr.GetOption("LogDB.Opt.ClearTime", 1209600U); // 14 days default
        logger.Info(LogFilter.ServerLoading, $"Will clear `logs` table of entries older than {_int_configs[(uint)WorldIntConfigs.CONFIG_LOGDB_CLEARTIME]} seconds every {_int_configs[(uint)WorldIntConfigs.CONFIG_LOGDB_CLEARINTERVAL]} minutes.");

        _int_configs[(uint)WorldIntConfigs.CONFIG_TELEPORT_TIMEOUT_NEAR] = ConfigMgr.GetOption("TeleportTimeoutNear", 25U); // pussywizard
        _int_configs[(uint)WorldIntConfigs.CONFIG_TELEPORT_TIMEOUT_FAR] = ConfigMgr.GetOption("TeleportTimeoutFar", 45U); // pussywizard
        _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_ALLOWED_MMR_DROP] = ConfigMgr.GetOption("MaxAllowedMMRDrop", 500U); // pussywizard
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ENABLE_LOGIN_AFTER_DC] = ConfigMgr.GetOption("EnableLoginAfterDC", true); // pussywizard
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_DONT_CACHE_RANDOM_MOVEMENT_PATHS] = ConfigMgr.GetOption("DontCacheRandomMovementPaths", true); // pussywizard

        _int_configs[(uint)WorldIntConfigs.CONFIG_SKILL_CHANCE_ORANGE] = ConfigMgr.GetOption("SkillChance.Orange", 100U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_SKILL_CHANCE_YELLOW] = ConfigMgr.GetOption("SkillChance.Yellow", 75U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_SKILL_CHANCE_GREEN] = ConfigMgr.GetOption("SkillChance.Green", 25U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_SKILL_CHANCE_GREY] = ConfigMgr.GetOption("SkillChance.Grey", 0U);

        _int_configs[(uint)WorldIntConfigs.CONFIG_SKILL_CHANCE_MINING_STEPS] = ConfigMgr.GetOption("SkillChance.MiningSteps", 75U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_SKILL_CHANCE_SKINNING_STEPS] = ConfigMgr.GetOption("SkillChance.SkinningSteps", 75U);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_SKILL_PROSPECTING] = ConfigMgr.GetOption("SkillChance.Prospecting", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_SKILL_MILLING] = ConfigMgr.GetOption("SkillChance.Milling", false);

        _int_configs[(uint)WorldIntConfigs.CONFIG_SKILL_GAIN_CRAFTING] = ConfigMgr.GetOption("SkillGain.Crafting", 1U);

        _int_configs[(uint)WorldIntConfigs.CONFIG_SKILL_GAIN_DEFENSE] = ConfigMgr.GetOption("SkillGain.Defense", 1U);

        _int_configs[(uint)WorldIntConfigs.CONFIG_SKILL_GAIN_GATHERING] = ConfigMgr.GetOption("SkillGain.Gathering", 1U);

        _int_configs[(uint)WorldIntConfigs.CONFIG_SKILL_GAIN_WEAPON] = ConfigMgr.GetOption("SkillGain.Weapon", 1U);

        _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_OVERSPEED_PINGS] = ConfigMgr.GetOption("MaxOverspeedPings", 2U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_MAX_OVERSPEED_PINGS] != 0 && _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_OVERSPEED_PINGS] < 2)
        {
            logger.Error(LogFilter.ServerLoading, $"MaxOverspeedPings ({_int_configs[(uint)WorldIntConfigs.CONFIG_MAX_OVERSPEED_PINGS]}) must be in range 2..infinity (or 0 to disable check). Set to 2.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_OVERSPEED_PINGS] = 2;
        }

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_SAVE_RESPAWN_TIME_IMMEDIATELY] = ConfigMgr.GetOption("SaveRespawnTimeImmediately", true);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_WEATHER] = ConfigMgr.GetOption("ActivateWeather", true);

        _int_configs[(uint)WorldIntConfigs.CONFIG_DISABLE_BREATHING] = ConfigMgr.GetOption("DisableWaterBreath", (uint)AccountTypes.SEC_CONSOLE);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALWAYS_MAX_SKILL_FOR_LEVEL] = ConfigMgr.GetOption("AlwaysMaxSkillForLevel", false);

        if (reload)
        {
            uint val = ConfigMgr.GetOption("Expansion", 2U);

            if (val != _int_configs[(uint)WorldIntConfigs.CONFIG_EXPANSION])
            {
                logger.Error(LogFilter.ServerLoading, $"Expansion option can't be changed at worldserver.conf reload, using current value ({_int_configs[(uint)WorldIntConfigs.CONFIG_EXPANSION]}).");
            }
        }
        else
        {
            _int_configs[(uint)WorldIntConfigs.CONFIG_EXPANSION] = ConfigMgr.GetOption("Expansion", 2U);
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_CHATFLOOD_MESSAGE_COUNT] = ConfigMgr.GetOption("ChatFlood.MessageCount", 10U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CHATFLOOD_MESSAGE_DELAY] = ConfigMgr.GetOption("ChatFlood.MessageDelay", 1U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CHATFLOOD_ADDON_MESSAGE_COUNT] = ConfigMgr.GetOption("ChatFlood.AddonMessageCount", 100U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CHATFLOOD_ADDON_MESSAGE_DELAY] = ConfigMgr.GetOption("ChatFlood.AddonMessageDelay", 1U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CHATFLOOD_MUTE_TIME] = ConfigMgr.GetOption("ChatFlood.MuteTime", 10U);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_CHAT_MUTE_FIRST_LOGIN] = ConfigMgr.GetOption("Chat.MuteFirstLogin", false);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CHAT_TIME_MUTE_FIRST_LOGIN] = ConfigMgr.GetOption("Chat.MuteTimeFirstLogin", 120U);

        _int_configs[(uint)WorldIntConfigs.CONFIG_EVENT_ANNOUNCE] = ConfigMgr.GetOption("Event.Announce", 0U);

        _float_configs[(uint)WorldFloatConfigs.CONFIG_CREATURE_FAMILY_FLEE_ASSISTANCE_RADIUS] = ConfigMgr.GetOption("CreatureFamilyFleeAssistanceRadius", 30.0f);
        _float_configs[(uint)WorldFloatConfigs.CONFIG_CREATURE_FAMILY_ASSISTANCE_RADIUS] = ConfigMgr.GetOption("CreatureFamilyAssistanceRadius", 10.0f);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CREATURE_FAMILY_ASSISTANCE_DELAY] = ConfigMgr.GetOption("CreatureFamilyAssistanceDelay", 2000U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CREATURE_FAMILY_ASSISTANCE_PERIOD] = ConfigMgr.GetOption("CreatureFamilyAssistancePeriod", 3000U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CREATURE_FAMILY_FLEE_DELAY] = ConfigMgr.GetOption("CreatureFamilyFleeDelay", 7000U);

        _int_configs[(uint)WorldIntConfigs.CONFIG_WORLD_BOSS_LEVEL_DIFF] = ConfigMgr.GetOption("WorldBossLevelDiff", 3U);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_QUEST_ENABLE_QUEST_TRACKER] = ConfigMgr.GetOption("Quests.EnableQuestTracker", false);

        // note: disable value (-1) will assigned as 0xFFFFFFF, to prevent overflow at calculations limit it to max possible player level DBCConst.MAX_LEVEL(100)
        _int_configs[(uint)WorldIntConfigs.CONFIG_QUEST_LOW_LEVEL_HIDE_DIFF] = ConfigMgr.GetOption("Quests.LowLevelHideDiff", 4U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_QUEST_LOW_LEVEL_HIDE_DIFF] > DBCConst.MAX_LEVEL)
        {
            _int_configs[(uint)WorldIntConfigs.CONFIG_QUEST_LOW_LEVEL_HIDE_DIFF] = (uint)DBCConst.MAX_LEVEL;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_QUEST_HIGH_LEVEL_HIDE_DIFF] = ConfigMgr.GetOption("Quests.HighLevelHideDiff", 7U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_QUEST_HIGH_LEVEL_HIDE_DIFF] > DBCConst.MAX_LEVEL)
        {
            _int_configs[(uint)WorldIntConfigs.CONFIG_QUEST_HIGH_LEVEL_HIDE_DIFF] = (uint)DBCConst.MAX_LEVEL;
        }

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_QUEST_IGNORE_RAID] = ConfigMgr.GetOption("Quests.IgnoreRaid", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_QUEST_IGNORE_AUTO_ACCEPT] = ConfigMgr.GetOption("Quests.IgnoreAutoAccept", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_QUEST_IGNORE_AUTO_COMPLETE] = ConfigMgr.GetOption("Quests.IgnoreAutoComplete", false);

        _int_configs[(uint)WorldIntConfigs.CONFIG_RANDOM_BG_RESET_HOUR] = ConfigMgr.GetOption("Battleground.Random.ResetHour", 6U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_RANDOM_BG_RESET_HOUR] > 23)
        {
            logger.Error(LogFilter.ServerLoading, $"Battleground.Random.ResetHour ({_int_configs[(uint)WorldIntConfigs.CONFIG_RANDOM_BG_RESET_HOUR]}) can't be load. Set to 6.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_RANDOM_BG_RESET_HOUR] = 6;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_CALENDAR_DELETE_OLD_EVENTS_HOUR] = ConfigMgr.GetOption("Calendar.DeleteOldEventsHour", 6U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_CALENDAR_DELETE_OLD_EVENTS_HOUR] > 23 || (uint)_int_configs[(uint)WorldIntConfigs.CONFIG_CALENDAR_DELETE_OLD_EVENTS_HOUR] < 0)
        {
            logger.Error(LogFilter.ServerLoading, $"Calendar.DeleteOldEventsHour ({_int_configs[(uint)WorldIntConfigs.CONFIG_CALENDAR_DELETE_OLD_EVENTS_HOUR]}) can't be load. Set to 6.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_CALENDAR_DELETE_OLD_EVENTS_HOUR] = 6;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_GUILD_RESET_HOUR] = ConfigMgr.GetOption("Guild.ResetHour", 6U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_GUILD_RESET_HOUR] > 23)
        {
            logger.Error(LogFilter.ServerLoading, $"Guild.ResetHour ({_int_configs[(uint)WorldIntConfigs.CONFIG_GUILD_RESET_HOUR]}) can't be load. Set to 6.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_GUILD_RESET_HOUR] = 6;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_GUILD_BANK_INITIAL_TABS] = ConfigMgr.GetOption("Guild.BankInitialTabs", 0U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_GUILD_BANK_TAB_COST_0] = ConfigMgr.GetOption("Guild.BankTabCost0", 1000000U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_GUILD_BANK_TAB_COST_1] = ConfigMgr.GetOption("Guild.BankTabCost1", 2500000U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_GUILD_BANK_TAB_COST_2] = ConfigMgr.GetOption("Guild.BankTabCost2", 5000000U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_GUILD_BANK_TAB_COST_3] = ConfigMgr.GetOption("Guild.BankTabCost3", 10000000U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_GUILD_BANK_TAB_COST_4] = ConfigMgr.GetOption("Guild.BankTabCost4", 25000000U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_GUILD_BANK_TAB_COST_5] = ConfigMgr.GetOption("Guild.BankTabCost5", 50000000U);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_DETECT_POS_COLLISION] = ConfigMgr.GetOption("DetectPosCollision", true);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_RESTRICTED_LFG_CHANNEL] = ConfigMgr.GetOption("Channel.RestrictedLfg", true);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_SILENTLY_GM_JOIN_TO_CHANNEL] = ConfigMgr.GetOption("Channel.SilentlyGMJoin", false);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_TALENTS_INSPECTING] = ConfigMgr.GetOption("TalentsInspecting", true);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_CHAT_FAKE_MESSAGE_PREVENTING] = ConfigMgr.GetOption("ChatFakeMessagePreventing", false);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CHAT_STRICT_LINK_CHECKING_SEVERITY] = ConfigMgr.GetOption("ChatStrictLinkChecking.Severity", 0U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CHAT_STRICT_LINK_CHECKING_KICK] = ConfigMgr.GetOption("ChatStrictLinkChecking.Kick", 0U);

        _int_configs[(uint)WorldIntConfigs.CONFIG_CORPSE_DECAY_NORMAL] = ConfigMgr.GetOption("Corpse.Decay.NORMAL", 60U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CORPSE_DECAY_RARE] = ConfigMgr.GetOption("Corpse.Decay.RARE", 300U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CORPSE_DECAY_ELITE] = ConfigMgr.GetOption("Corpse.Decay.ELITE", 300U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CORPSE_DECAY_RAREELITE] = ConfigMgr.GetOption("Corpse.Decay.RAREELITE", 300U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CORPSE_DECAY_WORLDBOSS] = ConfigMgr.GetOption("Corpse.Decay.WORLDBOSS", 3600U);

        _int_configs[(uint)WorldIntConfigs.CONFIG_DEATH_SICKNESS_LEVEL] = ConfigMgr.GetOption("Death.SicknessLevel", 11U);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_DEATH_CORPSE_RECLAIM_DELAY_PVP] = ConfigMgr.GetOption("Death.CorpseReclaimDelay.PvP", true);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_DEATH_CORPSE_RECLAIM_DELAY_PVE] = ConfigMgr.GetOption("Death.CorpseReclaimDelay.PvE", true);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_DEATH_BONES_WORLD] = ConfigMgr.GetOption("Death.Bones.World", true);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_DEATH_BONES_BG_OR_ARENA] = ConfigMgr.GetOption("Death.Bones.BattlegroundOrArena", true);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_DIE_COMMAND_MODE] = ConfigMgr.GetOption("Die.Command.Mode", true);

        // always use declined names in the russian client
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_DECLINED_NAMES_USED] = (_int_configs[(uint)WorldIntConfigs.CONFIG_REALM_ZONE] == (uint)RealmZone.REALM_ZONE_RUSSIAN) ? true : ConfigMgr.GetOption("DeclinedNames", false);

        _float_configs[(uint)WorldFloatConfigs.CONFIG_LISTEN_RANGE_SAY] = ConfigMgr.GetOption("ListenRange.Say", 25.0f);
        _float_configs[(uint)WorldFloatConfigs.CONFIG_LISTEN_RANGE_TEXTEMOTE] = ConfigMgr.GetOption("ListenRange.TextEmote", 25.0f);
        _float_configs[(uint)WorldFloatConfigs.CONFIG_LISTEN_RANGE_YELL] = ConfigMgr.GetOption("ListenRange.Yell", 300.0f);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_BATTLEGROUND_DISABLE_QUEST_SHARE_IN_BG] = ConfigMgr.GetOption("Battleground.DisableQuestShareInBG", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_BATTLEGROUND_DISABLE_READY_CHECK_IN_BG] = ConfigMgr.GetOption("Battleground.DisableReadyCheckInBG", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_BATTLEGROUND_CAST_DESERTER] = ConfigMgr.GetOption("Battleground.CastDeserter", true);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_BATTLEGROUND_QUEUE_ANNOUNCER_ENABLE] = ConfigMgr.GetOption("Battleground.QueueAnnouncer.Enable", false);
        _int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_QUEUE_ANNOUNCER_LIMIT_MIN_LEVEL] = ConfigMgr.GetOption("Battleground.QueueAnnouncer.Limit.MinLevel", 0U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_QUEUE_ANNOUNCER_LIMIT_MIN_PLAYERS] = ConfigMgr.GetOption("Battleground.QueueAnnouncer.Limit.MinPlayers", 3U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_QUEUE_ANNOUNCER_SPAM_DELAY] = ConfigMgr.GetOption("Battleground.QueueAnnouncer.SpamProtection.Delay", 30U);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_BATTLEGROUND_QUEUE_ANNOUNCER_PLAYERONLY] = ConfigMgr.GetOption("Battleground.QueueAnnouncer.PlayerOnly", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_BATTLEGROUND_QUEUE_ANNOUNCER_TIMED] = ConfigMgr.GetOption("Battleground.QueueAnnouncer.Timed", false);
        _int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_QUEUE_ANNOUNCER_TIMER] = ConfigMgr.GetOption("Battleground.QueueAnnouncer.Timer", 30000U);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_BATTLEGROUND_STORE_STATISTICS_ENABLE] = ConfigMgr.GetOption("Battleground.StoreStatistics.Enable", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_BATTLEGROUND_TRACK_DESERTERS] = ConfigMgr.GetOption("Battleground.TrackDeserters.Enable", false);
        _int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_PREMATURE_FINISH_TIMER] = ConfigMgr.GetOption("Battleground.PrematureFinishTimer", 5 * (uint)TimeConstants.MINUTE * (uint)TimeConstants.IN_MILLISECONDS);
        _int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_INVITATION_TYPE] = ConfigMgr.GetOption("Battleground.InvitationType", 0U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_PREMADE_GROUP_WAIT_FOR_MATCH] = ConfigMgr.GetOption("Battleground.PremadeGroupWaitForMatch", 30 * (uint)TimeConstants.MINUTE * (uint)TimeConstants.IN_MILLISECONDS);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_BG_XP_FOR_KILL] = ConfigMgr.GetOption("Battleground.GiveXPForKills", false);
        _int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_REPORT_AFK_TIMER] = ConfigMgr.GetOption("Battleground.ReportAFK.Timer", 4U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_REPORT_AFK] = ConfigMgr.GetOption("Battleground.ReportAFK", 3U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_REPORT_AFK] < 1)
        {
            logger.Error(LogFilter.ServerLoading, $"Battleground.ReportAFK ({_int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_REPORT_AFK]}) must be >0. Using 3 instead.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_REPORT_AFK] = 3;
        }
        else if (_int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_REPORT_AFK] > 9)
        {
            logger.Error(LogFilter.ServerLoading, $"Battleground.ReportAFK ({_int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_REPORT_AFK]}) must be <10. Using 3 instead.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_REPORT_AFK] = 3;
        }


        _int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_PLAYER_RESPAWN] = ConfigMgr.GetOption("Battleground.PlayerRespawn", 30U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_PLAYER_RESPAWN] < 3)
        {
            logger.Error(LogFilter.ServerLoading, $"Battleground.PlayerRespawn ({_int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_PLAYER_RESPAWN]}) must be >2. Using 30 instead.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_PLAYER_RESPAWN] = 30;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_RESTORATION_BUFF_RESPAWN] = ConfigMgr.GetOption("Battleground.RestorationBuffRespawn", 20U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_RESTORATION_BUFF_RESPAWN] < 1)
        {
            logger.Error(LogFilter.ServerLoading, $"Battleground.RestorationBuffRespawn ({_int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_RESTORATION_BUFF_RESPAWN]}) must be > 0. Using 20 instead.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_RESTORATION_BUFF_RESPAWN] = 20;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_BERSERKING_BUFF_RESPAWN] = ConfigMgr.GetOption("Battleground.BerserkingBuffRespawn", 120U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_BERSERKING_BUFF_RESPAWN] < 1)
        {
            logger.Error(LogFilter.ServerLoading, $"Battleground.BerserkingBuffRespawn ({_int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_BERSERKING_BUFF_RESPAWN]}) must be > 0. Using 120 instead.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_BERSERKING_BUFF_RESPAWN] = 120;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_SPEED_BUFF_RESPAWN] = ConfigMgr.GetOption("Battleground.SpeedBuffRespawn", 150U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_SPEED_BUFF_RESPAWN] < 1)
        {
            logger.Error(LogFilter.ServerLoading, $"Battleground.SpeedBuffRespawn ({_int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_SPEED_BUFF_RESPAWN]}) must be > 0. Using 150 instead.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_BATTLEGROUND_SPEED_BUFF_RESPAWN] = 150;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_ARENA_MAX_RATING_DIFFERENCE] = ConfigMgr.GetOption("Arena.MaxRatingDifference", 150U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_ARENA_RATING_DISCARD_TIMER] = ConfigMgr.GetOption("Arena.RatingDiscardTimer", 10 * (uint)TimeConstants.MINUTE * (uint)TimeConstants.IN_MILLISECONDS);
        _int_configs[(uint)WorldIntConfigs.CONFIG_ARENA_PREV_OPPONENTS_DISCARD_TIMER] = ConfigMgr.GetOption("Arena.PreviousOpponentsDiscardTimer", 2 * (uint)TimeConstants.MINUTE * (uint)TimeConstants.IN_MILLISECONDS);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ARENA_AUTO_DISTRIBUTE_POINTS] = ConfigMgr.GetOption("Arena.AutoDistributePoints", false);
        _int_configs[(uint)WorldIntConfigs.CONFIG_ARENA_AUTO_DISTRIBUTE_INTERVAL_DAYS] = ConfigMgr.GetOption("Arena.AutoDistributeInterval", 7U); // spoiled by implementing constant day and hour, always 7 now
        _int_configs[(uint)WorldIntConfigs.CONFIG_ARENA_GAMES_REQUIRED] = ConfigMgr.GetOption("Arena.GamesRequired", 10U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_ARENA_SEASON_ID] = ConfigMgr.GetOption("Arena.ArenaSeason.ID", 1U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_ARENA_START_RATING] = ConfigMgr.GetOption("Arena.ArenaStartRating", 0U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_LEGACY_ARENA_POINTS_CALC] = ConfigMgr.GetOption("Arena.LegacyArenaPoints", 0U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_ARENA_START_PERSONAL_RATING] = ConfigMgr.GetOption("Arena.ArenaStartPersonalRating", 1000U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_ARENA_START_MATCHMAKER_RATING] = ConfigMgr.GetOption("Arena.ArenaStartMatchmakerRating", 1500U);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ARENA_SEASON_IN_PROGRESS] = ConfigMgr.GetOption("Arena.ArenaSeason.InProgress", true);
        _float_configs[(uint)WorldFloatConfigs.CONFIG_ARENA_WIN_RATING_MODIFIER_1] = ConfigMgr.GetOption("Arena.ArenaWinRatingModifier1", 48.0f);
        _float_configs[(uint)WorldFloatConfigs.CONFIG_ARENA_WIN_RATING_MODIFIER_2] = ConfigMgr.GetOption("Arena.ArenaWinRatingModifier2", 24.0f);
        _float_configs[(uint)WorldFloatConfigs.CONFIG_ARENA_LOSE_RATING_MODIFIER] = ConfigMgr.GetOption("Arena.ArenaLoseRatingModifier", 24.0f);
        _float_configs[(uint)WorldFloatConfigs.CONFIG_ARENA_MATCHMAKER_RATING_MODIFIER] = ConfigMgr.GetOption("Arena.ArenaMatchmakerRatingModifier", 24.0f);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ARENA_QUEUE_ANNOUNCER_ENABLE] = ConfigMgr.GetOption("Arena.QueueAnnouncer.Enable", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ARENA_QUEUE_ANNOUNCER_PLAYERONLY] = ConfigMgr.GetOption("Arena.QueueAnnouncer.PlayerOnly", false);
        _int_configs[(uint)WorldIntConfigs.CONFIG_ARENA_QUEUE_ANNOUNCER_DETAIL] = ConfigMgr.GetOption("Arena.QueueAnnouncer.Detail", 3U);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_OFFHAND_CHECK_AT_SPELL_UNLEARN] = ConfigMgr.GetOption("OffhandCheckAtSpellUnlearn", true);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CREATURE_STOP_FOR_PLAYER] = ConfigMgr.GetOption("Creature.MovingStopTimeForPlayer", 3 * (uint)TimeConstants.MINUTE * (uint)TimeConstants.IN_MILLISECONDS);

        _int_configs[(uint)WorldIntConfigs.CONFIG_WATER_BREATH_TIMER] = ConfigMgr.GetOption("WaterBreath.Timer", 180000U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_WATER_BREATH_TIMER] <= 0)
        {
            logger.Error(LogFilter.ServerLoading, $"WaterBreath.Timer ({_int_configs[(uint)WorldIntConfigs.CONFIG_WATER_BREATH_TIMER]}) must be > 0. Using 180000 instead.");
            _int_configs[(uint)WorldIntConfigs.CONFIG_WATER_BREATH_TIMER] = 180000;
        }

        uint clientCacheId = ConfigMgr.GetOption("ClientCacheVersion", 0U);

        if (clientCacheId != 0)
        {
            // overwrite DB/old value
            if (clientCacheId > 0)
            {
                _int_configs[(uint)WorldIntConfigs.CONFIG_CLIENTCACHE_VERSION] = clientCacheId;
                logger.Info(LogFilter.ServerLoading, $"Client cache version set to: {clientCacheId}");
            }
            else
            {
                logger.Error(LogFilter.ServerLoading, $"ClientCacheVersion can't be negative {clientCacheId}, ignored.");
            }
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_INSTANT_LOGOUT] = ConfigMgr.GetOption("InstantLogout", (uint)AccountTypes.SEC_MODERATOR);

        _int_configs[(uint)WorldIntConfigs.CONFIG_GUILD_EVENT_LOG_COUNT] = ConfigMgr.GetOption("Guild.EventLogRecordsCount", (uint)SharedConst.GUILD_EVENTLOG_MAX_RECORDS);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_GUILD_EVENT_LOG_COUNT] > SharedConst.GUILD_EVENTLOG_MAX_RECORDS)
        {
            _int_configs[(uint)WorldIntConfigs.CONFIG_GUILD_EVENT_LOG_COUNT] = (uint)SharedConst.GUILD_EVENTLOG_MAX_RECORDS;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_GUILD_BANK_EVENT_LOG_COUNT] = ConfigMgr.GetOption("Guild.BankEventLogRecordsCount", (uint)SharedConst.GUILD_BANKLOG_MAX_RECORDS);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_GUILD_BANK_EVENT_LOG_COUNT] > SharedConst.GUILD_BANKLOG_MAX_RECORDS)
        {
            _int_configs[(uint)WorldIntConfigs.CONFIG_GUILD_BANK_EVENT_LOG_COUNT] = (uint)SharedConst.GUILD_BANKLOG_MAX_RECORDS;
        }

        //visibility on continents
        _maxVisibleDistanceOnContinents = ConfigMgr.GetOption("Visibility.Distance.Continents", ObjectDefines.DEFAULT_VISIBILITY_DISTANCE);

        if (_maxVisibleDistanceOnContinents < 45 * Global.sWorld.GetRate(Rates.RATE_CREATURE_AGGRO))
        {
            logger.Error(LogFilter.ServerLoading, $"Visibility.Distance.Continents can't be less max aggro radius {45 * Global.sWorld.GetRate(Rates.RATE_CREATURE_AGGRO)}");
            _maxVisibleDistanceOnContinents = 45 * Global.sWorld.GetRate(Rates.RATE_CREATURE_AGGRO);
        }
        else if (_maxVisibleDistanceOnContinents > ObjectDefines.MAX_VISIBILITY_DISTANCE)
        {
            logger.Error(LogFilter.ServerLoading, $"Visibility.Distance.Continents can't be greater {ObjectDefines.MAX_VISIBILITY_DISTANCE}");
            _maxVisibleDistanceOnContinents = ObjectDefines.MAX_VISIBILITY_DISTANCE;
        }

        //visibility in instances
        _maxVisibleDistanceInInstances = ConfigMgr.GetOption("Visibility.Distance.Instances", ObjectDefines.DEFAULT_VISIBILITY_INSTANCE);

        if (_maxVisibleDistanceInInstances < 45 * Global.sWorld.GetRate(Rates.RATE_CREATURE_AGGRO))
        {
            logger.Error(LogFilter.ServerLoading, $"Visibility.Distance.Instances can't be less max aggro radius {45 * Global.sWorld.GetRate(Rates.RATE_CREATURE_AGGRO)}");
            _maxVisibleDistanceInInstances = 45 * Global.sWorld.GetRate(Rates.RATE_CREATURE_AGGRO);
        }
        else if (_maxVisibleDistanceInInstances > ObjectDefines.MAX_VISIBILITY_DISTANCE)
        {
            logger.Error(LogFilter.ServerLoading, $"Visibility.Distance.Instances can't be greater {ObjectDefines.MAX_VISIBILITY_DISTANCE}");
            _maxVisibleDistanceInInstances = ObjectDefines.MAX_VISIBILITY_DISTANCE;
        }

        //visibility in BG/Arenas
        _maxVisibleDistanceInBGArenas = ConfigMgr.GetOption("Visibility.Distance.BGArenas", ObjectDefines.DEFAULT_VISIBILITY_BGARENAS);

        if (_maxVisibleDistanceInBGArenas < 45 * Global.sWorld.GetRate(Rates.RATE_CREATURE_AGGRO))
        {
            logger.Error(LogFilter.ServerLoading, $"Visibility.Distance.BGArenas can't be less max aggro radius {45 * Global.sWorld.GetRate(Rates.RATE_CREATURE_AGGRO)}");
            _maxVisibleDistanceInBGArenas = 45 * Global.sWorld.GetRate(Rates.RATE_CREATURE_AGGRO);
        }
        else if (_maxVisibleDistanceInBGArenas > ObjectDefines.MAX_VISIBILITY_DISTANCE)
        {
            logger.Error(LogFilter.ServerLoading, $"Visibility.Distance.BGArenas can't be greater {ObjectDefines.MAX_VISIBILITY_DISTANCE}");
            _maxVisibleDistanceInBGArenas = ObjectDefines.MAX_VISIBILITY_DISTANCE;
        }

        // Load the CharDelete related config options
        _int_configs[(uint)WorldIntConfigs.CONFIG_CHARDELETE_METHOD] = ConfigMgr.GetOption("CharDelete.Method", 0U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CHARDELETE_MIN_LEVEL] = ConfigMgr.GetOption("CharDelete.MinLevel", 0U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_CHARDELETE_KEEP_DAYS] = ConfigMgr.GetOption("CharDelete.KeepDays", 30U);

        // Load the ItemDelete related config options
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ITEMDELETE_METHOD] = ConfigMgr.GetOption("ItemDelete.Method", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ITEMDELETE_VENDOR] = ConfigMgr.GetOption("ItemDelete.Vendor", false);
        _int_configs[(uint)WorldIntConfigs.CONFIG_ITEMDELETE_QUALITY] = ConfigMgr.GetOption("ItemDelete.Quality", 3U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_ITEMDELETE_ITEM_LEVEL] = ConfigMgr.GetOption("ItemDelete.ItemLevel", 80U);

        _int_configs[(uint)WorldIntConfigs.CONFIG_FFA_PVP_TIMER] = ConfigMgr.GetOption("FFAPvPTimer", 30U);

        _int_configs[(uint)WorldIntConfigs.CONFIG_LOOT_NEED_BEFORE_GREED_ILVL_RESTRICTION] = ConfigMgr.GetOption("LootNeedBeforeGreedILvlRestriction", 70U);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_PLAYER_SETTINGS_ENABLED] = ConfigMgr.GetOption("EnablePlayerSettings", false);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALLOW_JOIN_BG_AND_LFG] = ConfigMgr.GetOption("JoinBGAndLFG.Enable", false);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_LEAVE_GROUP_ON_LOGOUT] = ConfigMgr.GetOption("LeaveGroupOnLogout.Enabled", true);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_QUEST_POI_ENABLED] = ConfigMgr.GetOption("QuestPOI.Enabled", true);

        _int_configs[(uint)WorldIntConfigs.CONFIG_CHANGE_FACTION_MAX_MONEY] = ConfigMgr.GetOption("ChangeFaction.MaxMoney", 0U);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALLOWS_RANK_MOD_FOR_PET_HEALTH] = ConfigMgr.GetOption("Pet.RankMod.Health", true);

        _int_configs[(uint)WorldIntConfigs.CONFIG_AUCTION_HOUSE_SEARCH_TIMEOUT] = ConfigMgr.GetOption("AuctionHouse.SearchTimeout", 1000U);

        // Read the "Data" directory from the config file
        string dataPath = ConfigMgr.GetOption("DataDir", "./");

        if (dataPath.IsEmpty() || (dataPath[dataPath.Length - 1] != '/' && dataPath[dataPath.Length - 1] != '\\'))
        {
            dataPath.Append('/');
        }

        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            if (dataPath[0] == '~')
            {
                string? home = Environment.GetEnvironmentVariable("HOME");

                if (home != null)
                {
                    dataPath = string.Concat(home, dataPath.AsSpan(1));
                }
            }
        }

        if (reload)
        {
            if (dataPath != _dataPath)
            {
                logger.Error(LogFilter.ServerLoading, $"DataDir option can't be changed at worldserver.conf reload, using current value ({_dataPath}).");
            }
        }
        else
        {
            _dataPath = dataPath;
            logger.Info(LogFilter.ServerLoading, $"Using DataDir {_dataPath}");
        }

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_VMAP_INDOOR_CHECK] = ConfigMgr.GetOption("vmap.enableIndoorCheck", false);
        bool enableIndoor = ConfigMgr.GetOption("vmap.enableIndoorCheck", true);
        bool enableLOS = ConfigMgr.GetOption("vmap.enableLOS", true);
        bool enableHeight = ConfigMgr.GetOption("vmap.enableHeight", true);
        bool enablePetLOS = ConfigMgr.GetOption("vmap.petLOS", true);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_VMAP_BLIZZLIKE_PVP_LOS] = ConfigMgr.GetOption("vmap.BlizzlikePvPLOS", true);

        if (!enableHeight)
        {
            logger.Error(LogFilter.ServerLoading, "VMap height checking disabled! Creatures movements and other various things WILL be broken! Expect no support.");
        }

        VMapFactory.CreateOrGetVMapMgr().SetEnableLineOfSightCalc(enableLOS);
        VMapFactory.CreateOrGetVMapMgr().SetEnableHeightCalc(enableHeight);

        logger.Info(LogFilter.ServerLoading, $"WORLD: VMap support included. LineOfSight:{enableLOS}, getHeight:{enableHeight}, indoorCheck:{enableIndoor} PetLOS:{enablePetLOS}");

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_PET_LOS] = ConfigMgr.GetOption("vmap.petLOS", true);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_START_CUSTOM_SPELLS] = ConfigMgr.GetOption("PlayerStart.CustomSpells", false);
        _int_configs[(uint)WorldIntConfigs.CONFIG_HONOR_AFTER_DUEL] = ConfigMgr.GetOption("HonorPointsAfterDuel", 0U);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_START_ALL_EXPLORED] = ConfigMgr.GetOption("PlayerStart.MapsExplored", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_START_ALL_REP] = ConfigMgr.GetOption("PlayerStart.AllReputation", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALWAYS_MAXSKILL] = ConfigMgr.GetOption("AlwaysMaxWeaponSkill", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_PVP_TOKEN_ENABLE] = ConfigMgr.GetOption("PvPToken.Enable", false);
        _int_configs[(uint)WorldIntConfigs.CONFIG_PVP_TOKEN_MAP_TYPE] = ConfigMgr.GetOption("PvPToken.MapAllowType", 4U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_PVP_TOKEN_ID] = ConfigMgr.GetOption("PvPToken.ItemID", 29434U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_PVP_TOKEN_COUNT] = ConfigMgr.GetOption("PvPToken.ItemCount", 1U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_PVP_TOKEN_COUNT] < 1)
        {
            _int_configs[(uint)WorldIntConfigs.CONFIG_PVP_TOKEN_COUNT] = 1;
        }

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_NO_RESET_TALENT_COST] = ConfigMgr.GetOption("NoResetTalentsCost", false);
        _int_configs[(uint)WorldIntConfigs.CONFIG_TOGGLE_XP_COST] = ConfigMgr.GetOption("ToggleXP.Cost", 100000U);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_SHOW_KICK_IN_WORLD] = ConfigMgr.GetOption("ShowKickInWorld", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_SHOW_MUTE_IN_WORLD] = ConfigMgr.GetOption("ShowMuteInWorld", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_SHOW_BAN_IN_WORLD] = ConfigMgr.GetOption("ShowBanInWorld", false);
        _int_configs[(uint)WorldIntConfigs.CONFIG_NUMTHREADS] = ConfigMgr.GetOption("MapUpdate.Threads", 1U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_RESULTS_LOOKUP_COMMANDS] = ConfigMgr.GetOption("Command.LookupMaxResults", 0U);

        // Warden
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_WARDEN_ENABLED] = ConfigMgr.GetOption("Warden.Enabled", true);
        _int_configs[(uint)WorldIntConfigs.CONFIG_WARDEN_NUM_MEM_CHECKS] = ConfigMgr.GetOption("Warden.NumMemChecks", 3U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_WARDEN_NUM_LUA_CHECKS] = ConfigMgr.GetOption("Warden.NumLuaChecks", 1U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_WARDEN_NUM_OTHER_CHECKS] = ConfigMgr.GetOption("Warden.NumOtherChecks", 7U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_WARDEN_CLIENT_BAN_DURATION] = ConfigMgr.GetOption("Warden.BanDuration", 86400U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_WARDEN_CLIENT_CHECK_HOLDOFF] = ConfigMgr.GetOption("Warden.ClientCheckHoldOff", 30U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_WARDEN_CLIENT_FAIL_ACTION] = ConfigMgr.GetOption("Warden.ClientCheckFailAction", 0U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_WARDEN_CLIENT_RESPONSE_DELAY] = ConfigMgr.GetOption("Warden.ClientResponseDelay", 600U);

        // Dungeon finder
        _int_configs[(uint)WorldIntConfigs.CONFIG_LFG_OPTIONSMASK] = ConfigMgr.GetOption("DungeonFinder.OptionsMask", 5U);

        // DBC_ItemAttributes
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_DBC_ENFORCE_ITEM_ATTRIBUTES] = ConfigMgr.GetOption("DBC.EnforceItemAttributes", true);

        // Max instances per hour
        _int_configs[(uint)WorldIntConfigs.CONFIG_MAX_INSTANCES_PER_HOUR] = ConfigMgr.GetOption("AccountInstancesPerHour", 5U);

        // AutoBroadcast
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_AUTOBROADCAST] = ConfigMgr.GetOption("AutoBroadcast.On", false);
        _int_configs[(uint)WorldIntConfigs.CONFIG_AUTOBROADCAST_CENTER] = ConfigMgr.GetOption("AutoBroadcast.Center", 0U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_AUTOBROADCAST_INTERVAL] = ConfigMgr.GetOption("AutoBroadcast.Timer", 60000U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_AUTOBROADCAST_MIN_LEVEL_DISABLE] = ConfigMgr.GetOption("AutoBroadcast.MinDisableLevel", 0U);

        if (reload)
        {
            _timers[(int)WorldTimers.WUPDATE_AUTOBROADCAST].SetInterval(_int_configs[(uint)WorldIntConfigs.CONFIG_AUTOBROADCAST_INTERVAL]);
            _timers[(int)WorldTimers.WUPDATE_AUTOBROADCAST].Reset();
        }

        // MySQL ping time interval
        _int_configs[(uint)WorldIntConfigs.CONFIG_DB_PING_INTERVAL] = ConfigMgr.GetOption("MaxPingTime", 30U);

        // misc
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_PDUMP_NO_PATHS] = ConfigMgr.GetOption("PlayerDump.DisallowPaths", true);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_PDUMP_NO_OVERWRITE] = ConfigMgr.GetOption("PlayerDump.DisallowOverwrite", true);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ENABLE_MMAPS] = ConfigMgr.GetOption("MoveMaps.Enable", true);

        MMapFactory.InitializeDisabledMaps();

        // Wintergrasp
        _int_configs[(uint)WorldIntConfigs.CONFIG_WINTERGRASP_ENABLE] = ConfigMgr.GetOption("Wintergrasp.Enable", 1U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_WINTERGRASP_PLR_MAX] = ConfigMgr.GetOption("Wintergrasp.PlayerMax", 100U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_WINTERGRASP_PLR_MIN] = ConfigMgr.GetOption("Wintergrasp.PlayerMin", 0U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_WINTERGRASP_PLR_MIN_LVL] = ConfigMgr.GetOption("Wintergrasp.PlayerMinLvl", 77U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_WINTERGRASP_BATTLETIME] = ConfigMgr.GetOption("Wintergrasp.BattleTimer", 30U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_WINTERGRASP_NOBATTLETIME] = ConfigMgr.GetOption("Wintergrasp.NoBattleTimer", 150U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_WINTERGRASP_RESTART_AFTER_CRASH] = ConfigMgr.GetOption("Wintergrasp.CrashRestartTimer", 10U);

        _int_configs[(uint)WorldIntConfigs.CONFIG_BIRTHDAY_TIME] = ConfigMgr.GetOption("BirthdayTime", 1222964635U);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_MINIGOB_MANABONK] = ConfigMgr.GetOption("Minigob.Manabonk.Enable", true);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ENABLE_CONTINENT_TRANSPORT] = ConfigMgr.GetOption("IsContinentTransport.Enabled", true);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ENABLE_CONTINENT_TRANSPORT_PRELOADING] = ConfigMgr.GetOption("IsPreloadedContinentTransport.Enabled", false);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_IP_BASED_ACTION_LOGGING] = ConfigMgr.GetOption("Allow.IP.Based.Action.Logging", false);

        // Whether to use LoS from game objects
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_CHECK_GOBJECT_LOS] = ConfigMgr.GetOption("CheckGameObjectLoS", true);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_CALCULATE_CREATURE_ZONE_AREA_DATA] = ConfigMgr.GetOption("Calculate.Creature.Zone.Area.Data", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_CALCULATE_GAMEOBJECT_ZONE_AREA_DATA] = ConfigMgr.GetOption("Calculate.Gameoject.Zone.Area.Data", false);

        // Player can join LFG anywhere
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_LFG_LOCATION_ALL] = ConfigMgr.GetOption("LFG.Location.All", false);

        // Prevent players AFK from being logged out
        _int_configs[(uint)WorldIntConfigs.CONFIG_AFK_PREVENT_LOGOUT] = ConfigMgr.GetOption("PreventAFKLogout", 0U);

        // Preload all grids of all non-instanced maps
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_PRELOAD_ALL_NON_INSTANCED_MAP_GRIDS] = ConfigMgr.GetOption("PreloadAllNonInstancedMapGrids", false);

        // ICC buff override
        _int_configs[(uint)WorldIntConfigs.CONFIG_ICC_BUFF_HORDE] = ConfigMgr.GetOption("ICC.Buff.Horde", 73822U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_ICC_BUFF_ALLIANCE] = ConfigMgr.GetOption("ICC.Buff.Alliance", 73828U);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_SET_ALL_CREATURES_WITH_WAYPOINT_MOVEMENT_ACTIVE] = ConfigMgr.GetOption("SetAllCreaturesWithWaypointMovementActive", false);

        // packet spoof punishment
        _int_configs[(uint)WorldIntConfigs.CONFIG_PACKET_SPOOF_POLICY] = ConfigMgr.GetOption("PacketSpoof.Policy", (uint)DosProtectionPolicy.POLICY_KICK);
        _int_configs[(uint)WorldIntConfigs.CONFIG_PACKET_SPOOF_BANMODE] = ConfigMgr.GetOption("PacketSpoof.BanMode", 0U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_PACKET_SPOOF_BANMODE] > 1)
        {
            _int_configs[(uint)WorldIntConfigs.CONFIG_PACKET_SPOOF_BANMODE] = 0U;
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_PACKET_SPOOF_BANDURATION] = ConfigMgr.GetOption("PacketSpoof.BanDuration", 86400U);

        // Random Battleground Rewards
        _int_configs[(uint)WorldIntConfigs.CONFIG_BG_REWARD_WINNER_HONOR_FIRST] = ConfigMgr.GetOption("Battleground.RewardWinnerHonorFirst", 30U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_BG_REWARD_WINNER_ARENA_FIRST] = ConfigMgr.GetOption("Battleground.RewardWinnerArenaFirst", 25U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_BG_REWARD_WINNER_HONOR_LAST] = ConfigMgr.GetOption("Battleground.RewardWinnerHonorLast", 15U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_BG_REWARD_WINNER_ARENA_LAST] = ConfigMgr.GetOption("Battleground.RewardWinnerArenaLast", 0U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_BG_REWARD_LOSER_HONOR_FIRST] = ConfigMgr.GetOption("Battleground.RewardLoserHonorFirst", 5U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_BG_REWARD_LOSER_HONOR_LAST] = ConfigMgr.GetOption("Battleground.RewardLoserHonorLast", 5U);

        _int_configs[(uint)WorldIntConfigs.CONFIG_WAYPOINT_MOVEMENT_STOP_TIME_FOR_PLAYER] = ConfigMgr.GetOption("WaypointMovementStopTimeForPlayer", 120U);

        _int_configs[(uint)WorldIntConfigs.CONFIG_DUNGEON_ACCESS_REQUIREMENTS_PRINT_MODE] = ConfigMgr.GetOption("DungeonAccessRequirements.PrintMode", 1U);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_DUNGEON_ACCESS_REQUIREMENTS_PORTAL_CHECK_ILVL] = ConfigMgr.GetOption("DungeonAccessRequirements.PortalAvgIlevelCheck", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_DUNGEON_ACCESS_REQUIREMENTS_LFG_DBC_LEVEL_OVERRIDE] = ConfigMgr.GetOption("DungeonAccessRequirements.LFGLevelDBCOverride", false);
        _int_configs[(uint)WorldIntConfigs.CONFIG_DUNGEON_ACCESS_REQUIREMENTS_OPTIONAL_STRING_ID] = ConfigMgr.GetOption("DungeonAccessRequirements.OptionalStringID", 0U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_NPC_EVADE_IF_NOT_REACHABLE] = ConfigMgr.GetOption("NpcEvadeIfTargetIsUnreachable", 5U);
        _int_configs[(uint)WorldIntConfigs.CONFIG_NPC_REGEN_TIME_IF_NOT_REACHABLE_IN_RAID] = ConfigMgr.GetOption("NpcRegenHPTimeIfTargetIsUnreachable", 10U);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_REGEN_HP_CANNOT_REACH_TARGET_IN_RAID] = ConfigMgr.GetOption("NpcRegenHPIfTargetIsUnreachable", true);

        //Debug
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_DEBUG_BATTLEGROUND] = ConfigMgr.GetOption("Debug.Battleground", false);
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_DEBUG_ARENA] = ConfigMgr.GetOption("Debug.Arena", false);

        _int_configs[(uint)WorldIntConfigs.CONFIG_GM_LEVEL_CHANNEL_MODERATION] = ConfigMgr.GetOption("Channel.ModerationGMLevel", 1U);

        _bool_configs[(uint)WorldBoolConfigs.CONFIG_SET_BOP_ITEM_TRADEABLE] = ConfigMgr.GetOption("Item.SetItemTradeable", true);

        // Specifies if IP addresses can be logged to the database
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_ALLOW_LOGGING_IP_ADDRESSES_IN_DATABASE] = ConfigMgr.GetOption("AllowLoggingIPAddressesInDatabase", true);

        // LFG group mechanics.
        _int_configs[(uint)WorldIntConfigs.CONFIG_LFG_MAX_KICK_COUNT] = ConfigMgr.GetOption("LFG.MaxKickCount", 2U);

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_LFG_MAX_KICK_COUNT] > 3)
        {
            _int_configs[(uint)WorldIntConfigs.CONFIG_LFG_MAX_KICK_COUNT] = 3;
            logger.Error(LogFilter.ServerLoading, "LFG.MaxKickCount can't be higher than 3.");
        }

        _int_configs[(uint)WorldIntConfigs.CONFIG_LFG_KICK_PREVENTION_TIMER] = ConfigMgr.GetOption("LFG.KickPreventionTimer", 15 * (uint)TimeConstants.MINUTE * (uint)TimeConstants.IN_MILLISECONDS) * (uint)TimeConstants.IN_MILLISECONDS;

        if (_int_configs[(uint)WorldIntConfigs.CONFIG_LFG_KICK_PREVENTION_TIMER] > 15 * (uint)TimeConstants.MINUTE * (uint)TimeConstants.IN_MILLISECONDS)
        {
            _int_configs[(uint)WorldIntConfigs.CONFIG_LFG_KICK_PREVENTION_TIMER] = 15 * (uint)TimeConstants.MINUTE * (uint)TimeConstants.IN_MILLISECONDS;
            logger.Error(LogFilter.ServerLoading, "LFG.KickPreventionTimer can't be higher than 15 minutes.");
        }

        // Realm Availability
        _bool_configs[(uint)WorldBoolConfigs.CONFIG_REALM_LOGIN_ENABLED] = ConfigMgr.GetOption("World.RealmAvailability", true);
    }

    private void SetNewCharString(string str)
    {
        _newCharString = str;
    }

    private void SetPlayerAmountLimit(uint limit)
    {
        _playerLimit = limit;
    }

    private float GetRate(Rates rate)
    {
        return _rate_values[(uint)rate];
    }

    public bool IsClosed()
    {
        // TODO: worldserver: World::IsClosed()
        return false;
    }

    public bool IsStopped()
    {
        return _stopEvent.WaitOne(0);
    }

    internal AccountTypes GetPlayerSecurityLimit()
    {
        return _allowedSecurityLevel;
    }

    internal void AddSession(WorldSession worldSession)
    {
        _addSessionQueue.Add(worldSession);
    }

    // Update the World !
    public void Update(uint diff)
    {
        // TODO: worldserver: World::Update(uint diff)

        UpdateSessions(diff);

        // TODO: worldserver: World::Update(uint diff)
    }

    internal WorldSession? FindOfflineSession(uint id)
    {
        if (_offlineSessions.TryGetValue(id, out WorldSession? session))
        {
            return session;
        }
        else
        {
            return null;
        }
    }

    internal WorldSession? FindOfflineSessionForCharacterGUID(uint guidLow)
    {
        if (_offlineSessions.Empty())
        {
            return null;
        }

        foreach (var itr in _offlineSessions)
        {
            if (itr.Value.GetGuidLow() == guidLow)
            {
                return itr.Value;
            }
        }

        return null;
    }

    private void UpdateSessions(uint diff)
    {
        {
            // Add new sessions
            while (_addSessionQueue.Next(out WorldSession? session))
            {
                if (session != null)
                {
                    AddSessionCore(session);
                }
            }
        }

        foreach (uint accountId in _sessions.Keys)
        {
            WorldSession session = _sessions[accountId];
            WorldSessionFilter updater = new(session);

            if (session.HandleSocketClosed())
            {
                // TODO: worldserver: World::UpdateSessions(uint diff)

                continue;
            }

            if (!session.Update(diff, updater))
            {
                // TODO: worldserver: World::UpdateSessions(uint diff)
                //if (!RemoveQueuedPlayer(pSession) && getIntConfig(CONFIG_INTERVAL_DISCONNECT_TOLERANCE))
                //{
                //    _disconnects[pSession->GetAccountId()] = GameTime::GetGameTime().count();
                //}

                _sessions.Remove(accountId);
            }
        }

        // TODO: worldserver: World::UpdateSessions(uint diff)
    }

    private void AddSessionCore(WorldSession s)
    {
        // TODO: worldserver: World::AddSessionCore(WorldSession sess)

        if (_sessions.ContainsKey(s.GetAccountId()))
        {
            // TODO: worldserver: World::AddSessionCore(WorldSession sess)
            //WorldSession oldSess = _sessions[sess.GetAccountId()];
        }

        _sessions[s.GetAccountId()] = s;

        s.InitializeSession();

        // TODO: worldserver: World::AddSessionCore(WorldSession s) => UpdateMaxSessionCounters()
        //UpdateMaxSessionCounters();
    }
}
