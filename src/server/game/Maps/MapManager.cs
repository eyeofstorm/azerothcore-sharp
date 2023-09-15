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

using AzerothCore.DataStores;
using AzerothCore.Singleton;
using AzerothCore.Utilities;

namespace AzerothCore.Game;

public class MapMgr : Singleton<MapMgr>
{
    private readonly IntervalTimer[] _timer; // continents, bgs/arenas, instances, total from the beginning

    private MapMgr()
    {
        _timer = new IntervalTimer[4];

        for (int i = 0; i < 4; i++)
        {
            _timer[i] = new IntervalTimer();
        }

        _timer[3].SetInterval(Global.sWorld.GetIntConfig(WorldIntConfigs.CONFIG_INTERVAL_MAPUPDATE));
    }

    public static bool IsValidMAP(uint mapid, bool startUp)
    {
        MapEntry? mEntry = Global.sMapStore.LookupEntry(mapid);

        if (startUp)
        {
            return mEntry != null;
        }
        else
        {
            return mEntry != null && (!mEntry.IsDungeon() || Global.sObjectMgr.GetInstanceTemplate(mapid) != null);
        }
    }

    public static bool IsValidMapCoord(uint mapId, float x, float y, float z, float o)
    {
        return IsValidMAP(mapId, false) && GridDefines.IsValidMapCoord(x, y, z, o);
    }

    public void SetMapUpdateInterval(uint t)
    {
        if (t < GridDefines.MIN_MAP_UPDATE_DELAY)
        {
            t = GridDefines.MIN_MAP_UPDATE_DELAY;
        }

        _timer[3].SetInterval(t);
        _timer[3].Reset();
    }
}
