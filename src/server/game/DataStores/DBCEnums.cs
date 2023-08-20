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

namespace AzerothCore.DataStores;

public enum MapTypes : uint
{
    MAP_COMMON = 0,                                 // none
    MAP_INSTANCE = 1,                               // party
    MAP_RAID = 2,                                   // raid
    MAP_BATTLEGROUND = 3,                           // pvp
    MAP_ARENA = 4                                   // arena
}

[Flags]
public enum MapFlags : uint
{
    MAP_FLAG_DYNAMIC_DIFFICULTY = 0x100
}

public static class DBCConst
{
    public static readonly int MAX_DUNGEON_DIFFICULTY = 3;
    public static readonly int MAX_RAID_DIFFICULTY = 4;
    public static readonly int MAX_DIFFICULTY = 4;
}
