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

namespace AzerothCore.Game;

public static class GridDefines
{
    public static float MAP_SIZE        = (MapDefines.SIZE_OF_GRIDS * MapDefines.MAX_NUMBER_OF_GRIDS);
    public static float MAP_HALFSIZE    = (MAP_SIZE / 2);

    public static bool IsValidMapCoord(float c)
    {
        return float.IsFinite(c) && (Math.Abs(c) <= MAP_HALFSIZE - 0.5f);
    }

    public static bool IsValidMapCoord(float x, float y)
    {
        return IsValidMapCoord(x) && IsValidMapCoord(y);
    }

    public static bool IsValidMapCoord(float x, float y, float z)
    {
        return IsValidMapCoord(x, y) && IsValidMapCoord(z);
    }

    public static bool IsValidMapCoord(float x, float y, float z, float o)
    {
        return IsValidMapCoord(x, y, z) && float.IsFinite(o);
    }
}
