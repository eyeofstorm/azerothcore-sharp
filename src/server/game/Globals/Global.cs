﻿/*
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

public static class Global
{
    // Main
    public static readonly World                        sWorld              = World.Instance;
    public static readonly RealmList                    sRealmList          = RealmList.Instance;
    public static readonly PacketFileLogger             sPacketLog          = PacketFileLogger.Instance;
    public static readonly ObjectMgr                    sObjectMgr          = ObjectMgr.Instance;
    public static readonly MapMgr                       sMapMgr             = MapMgr.Instance;
    public static readonly WorldUpdateTime              sWorldUpdateTime    = new();

    // Data stores
    public static readonly DBCStorage<ChrClassesEntry>              sChrClassesStore            = new(DBCFmt.ChrClassesEntryfmt);
    public static readonly DBCStorage<ChrRacesEntry>                sChrRacesStore              = new(DBCFmt.ChrRacesEntryfmt);
    public static readonly DBCStorage<MapEntry>                     sMapStore                   = new(DBCFmt.MapEntryfmt);
    public static readonly DBCStorage<SpellItemEnchantmentEntry>    sSpellItemEnchantmentStore  = new(DBCFmt.SpellItemEnchantmentfmt);
    public static readonly DBCStorage<ItemEntry>                    sItemStore                  = new(DBCFmt.Itemfmt);
}
