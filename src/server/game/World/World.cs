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

using AzerothCore.Database;
using AzerothCore.Realms;
using AzerothCore.Singleton;

namespace AzerothCore.Game;

public class World : Singleton<World>, IWorld
{
    private string?     _dbVersion;

    private Realm       _realm = new Realm();

    private UInt32[]    _int_configs        = new UInt32[(uint)WorldIntConfigs.INT_CONFIG_VALUE_COUNT];
    private bool[]      _bool_configs       = new bool[(uint)WorldBoolConfigs.BOOL_CONFIG_VALUE_COUNT];
    private float[]     _float_configs      = new float[(uint)WorldFloatConfigs.FLOAT_CONFIG_VALUE_COUNT];

    private World() { }

    public void LoadDBVersion()
    {
        SQLResult result = DB.World.Query("SELECT db_version, cache_id FROM version LIMIT 1");

        if (!result.IsEmpty())
        {
            SQLFields fields = result.GetFields();

            _dbVersion = fields.Read<string> (0);

            // will be overwrite by config values if different and non-0
            _int_configs[(uint)WorldIntConfigs.CONFIG_CLIENTCACHE_VERSION] = fields.Read<UInt32>(1);
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
        // TODO: worldserver: SetInitialWorldSettings
    }
}
