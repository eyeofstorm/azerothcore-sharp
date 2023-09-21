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

namespace AzerothCore.Game;

public class DataStores
{
    public static void LoadDBCStores(string dataPath)
    {
        string dbcPath = Path.Combine(dataPath, "dbc");

        // speed up
        uint availableDbcLocales = 0xFFFFFFFF;

        // ...
        DBCStorage<ChrClassesEntry>.LoadDBC(ref availableDbcLocales, Global.sChrClassesStore, dbcPath, "ChrClasses.dbc", "chrclasses_dbc");
        DBCStorage<ChrRacesEntry>.LoadDBC(ref availableDbcLocales, Global.sChrRacesStore, dbcPath, "ChrRaces.dbc", "chrraces_dbc");
        DBCStorage<MapEntry>.LoadDBC(ref availableDbcLocales, Global.sMapStore, dbcPath, "Map.dbc", "map_dbc");
        DBCStorage<SpellItemEnchantmentEntry>.LoadDBC(ref availableDbcLocales, Global.sSpellItemEnchantmentStore, dbcPath, "SpellItemEnchantment.dbc", "spellitemenchantment_dbc");
        DBCStorage<ItemEntry>.LoadDBC(ref availableDbcLocales, Global.sItemStore, dbcPath, "Item.dbc", "item_dbc");
        // ...
    }
}
