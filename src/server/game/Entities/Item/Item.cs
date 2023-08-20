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

// -1 from client enchantment slot number
public enum EnchantmentSlot : byte
{
    PERM_ENCHANTMENT_SLOT = 0,
    TEMP_ENCHANTMENT_SLOT = 1,
    SOCK_ENCHANTMENT_SLOT = 2,
    SOCK_ENCHANTMENT_SLOT_2 = 3,
    SOCK_ENCHANTMENT_SLOT_3 = 4,
    BONUS_ENCHANTMENT_SLOT = 5,
    PRISMATIC_ENCHANTMENT_SLOT = 6,                    // added at apply special permanent enchantment
    MAX_INSPECTED_ENCHANTMENT_SLOT = 7,

    PROP_ENCHANTMENT_SLOT_0 = 7,                    // used with RandomSuffix and RandomProperty
    PROP_ENCHANTMENT_SLOT_1 = 8,                    // used with RandomSuffix and RandomProperty
    PROP_ENCHANTMENT_SLOT_2 = 9,                    // used with RandomSuffix and RandomProperty
    PROP_ENCHANTMENT_SLOT_3 = 10,                   // used with RandomSuffix and RandomProperty
    PROP_ENCHANTMENT_SLOT_4 = 11,                   // used with RandomSuffix and RandomProperty
    MAX_ENCHANTMENT_SLOT = 12
};
