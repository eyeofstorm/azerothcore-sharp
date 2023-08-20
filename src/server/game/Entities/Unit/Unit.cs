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

public static class UnitConst
{
    public static readonly uint MAX_CREATURE_SPELLS = 8;
    public static readonly uint InfinityCooldownDelay = 0x9A7EC800;         // used for set "infinity cooldowns" for spells and check, MONTH*IN_MILLISECONDS
    public static readonly uint InfinityCooldownDelayCheck = 0x4D3F6400;    // MONTH*IN_MILLISECONDS/2;
}

public class Unit
{
    public Unit()
    {
    }
}
