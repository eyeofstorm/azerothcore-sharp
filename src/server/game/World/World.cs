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

namespace AzerothCore.Game;

public class World : Singleton<World>, IWorld
{
    protected static readonly ILogger logger = LoggerFactory.GetLogger();

    private string?     _dbVersion;

    private Realm       _realm = new();

    private uint[]      _int_configs        = new uint[(uint)WorldIntConfigs.INT_CONFIG_VALUE_COUNT];
    private bool[]      _bool_configs       = new bool[(uint)WorldBoolConfigs.BOOL_CONFIG_VALUE_COUNT];
    private float[]     _float_configs      = new float[(uint)WorldFloatConfigs.FLOAT_CONFIG_VALUE_COUNT];

    private AccountTypes _allowedSecurityLevel;

    private World()
    {
        _allowedSecurityLevel = AccountTypes.SEC_PLAYER;
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

        logger.Info(LogFilter.ServerLoading, "Initializing Opcodes...");
        OpcodeTable.Instance.Initialize();
    }

    internal bool IsClosed()
    {
        // TODO: worldserver: World::IsClosed()
        return false;
    }

    internal AccountTypes GetPlayerSecurityLimit()
    {
        return _allowedSecurityLevel;
    }

    internal void AddSession(WorldSession worldSession)
    {
        // TODO: worldserver: World::AddSession(WorldSession worldSession)
    }
}
