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

namespace AzerothCore.Game;

public class MapMgr : Singleton<MapMgr>
{
    private MapMgr() {  }

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
}
