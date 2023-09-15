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

public class ObjectDefines
{
    public static readonly float CONTACT_DISTANCE = 0.5f;
    public static readonly float INTERACTION_DISTANCE = 5.5f;
    public static readonly float ATTACK_DISTANCE = 5.0f;
    public static readonly float VISIBILITY_COMPENSATION = 15.0f;                           // increase searchers
    public static readonly float INSPECT_DISTANCE = 28.0f;
    public static readonly float VISIBILITY_INC_FOR_GOBJECTS = 30.0f;
    public static readonly float SPELL_SEARCHER_COMPENSATION = 30.0f;                       // increase searchers size in case we have large npc near cell border
    public static readonly float TRADE_DISTANCE = 11.11f;
    public static readonly float MAX_VISIBILITY_DISTANCE = 250.0f;                          // max distance for visible objects, experimental
    public static readonly float SIGHT_RANGE_UNIT = 50.0f;
    public static readonly float MAX_SEARCHER_DISTANCE = 150.0f;                            // pussywizard: replace the use of MAX_VISIBILITY_DISTANCE in searchers, because MAX_VISIBILITY_DISTANCE is quite too big for this purpose
    public static readonly float VISIBILITY_DISTANCE_INFINITE = 533.0f;
    public static readonly float VISIBILITY_DISTANCE_GIGANTIC = 400.0f;
    public static readonly float VISIBILITY_DISTANCE_LARGE = 200.0f;
    public static readonly float VISIBILITY_DISTANCE_NORMAL = 100.0f;
    public static readonly float VISIBILITY_DISTANCE_SMALL = 50.0f;
    public static readonly float VISIBILITY_DISTANCE_TINY = 25.0f;
    public static readonly float DEFAULT_VISIBILITY_DISTANCE = 100.0f;                      // default visible distance, 100 yards on continents
    public static readonly float DEFAULT_VISIBILITY_INSTANCE = 170.0f;                      // default visible distance in instances, 170 yards
    public static readonly float VISIBILITY_DIST_WINTERGRASP = 175.0f;
    public static readonly float DEFAULT_VISIBILITY_BGARENAS = 533.0f;                      // default visible distance in BG/Arenas, roughly 533 yards

    public static readonly float DEFAULT_WORLD_OBJECT_SIZE = 0.388999998569489f;            // player size, also currently used (correctly?) for any non Unit world objects
    public static readonly float DEFAULT_COMBAT_REACH = 1.5f;
    public static readonly float MIN_MELEE_REACH = 2.0f;
    public static readonly float NOMINAL_MELEE_RANGE = 5.0f;
    public static readonly float MELEE_RANGE = (NOMINAL_MELEE_RANGE - MIN_MELEE_REACH * 2); //center to center for players
    public static readonly float DEFAULT_COLLISION_HEIGHT = 2.03128f;                       // Most common value in dbc
}

