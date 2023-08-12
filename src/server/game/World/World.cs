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

using AzerothCore.Constants;
using AzerothCore.Database;
using AzerothCore.Logging;
using AzerothCore.Singleton;
using AzerothCore.Threading;

using SessionMap = System.Collections.Generic.Dictionary<uint, AzerothCore.Game.WorldSession>;

namespace AzerothCore.Game;

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

    private string?     _dbVersion;

    private Realm       _realm = new();

    private uint[]      _int_configs        = new uint[(uint)WorldIntConfigs.INT_CONFIG_VALUE_COUNT];
    private bool[]      _bool_configs       = new bool[(uint)WorldBoolConfigs.BOOL_CONFIG_VALUE_COUNT];
    private float[]     _float_configs      = new float[(uint)WorldFloatConfigs.FLOAT_CONFIG_VALUE_COUNT];

    private AccountTypes _allowedSecurityLevel;

    private readonly LockedQueue<WorldSession> _addSessionQueue;
    private readonly SessionMap _sessions;

    private AutoResetEvent _stopEvent;

    private World()
    {
        _allowedSecurityLevel = AccountTypes.SEC_PLAYER;
        _addSessionQueue = new ();
        _sessions = new SessionMap();
        _stopEvent = new (false);
    }

    // Get a server configuration element (see #WorldConfigs)
    public uint GetIntConfig(WorldIntConfigs intConfigsIndex)
    {
        return (intConfigsIndex < WorldIntConfigs.INT_CONFIG_VALUE_COUNT) ? _int_configs[(uint)intConfigsIndex] : 0;
    }

    public void LoadDBVersion()
    {
        SQLResult result = DB.World.Query("SELECT db_version, cache_id FROM version LIMIT 1");

        if (!result.IsEmpty())
        {
            SQLFields fields = result.GetFields();

            _dbVersion = fields.Read<string> (0);

            // will be overwrite by config values if different and non-0
            _int_configs[(uint)WorldIntConfigs.CONFIG_CLIENTCACHE_VERSION] = fields.Read<uint>(1);
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
        // TODO: worldserver: World::SetInitialWorldSettings()

        logger.Info(LogFilter.ServerLoading, $"Loading Player Create Data...");
        Global.sObjectMgr.LoadPlayerInfo();

        logger.Info(LogFilter.ServerLoading, $"Loading Client Addons...");
        AddonMgr.LoadFromDB();

        logger.Info(LogFilter.ServerLoading, $"Initializing Opcodes...");
        OpcodeTable.Instance.Initialize();
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
