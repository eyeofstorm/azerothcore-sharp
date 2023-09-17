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

using System.Runtime.InteropServices;

namespace AzerothCore.Game;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Damage
{
    public float DamageMin;
    public float DamageMax;
    public uint DamageType;                                     // id from Resistances.dbc
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ItemStat
{
    public uint ItemStatType;
    public int ItemStatValue;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Spell
{
    public int SpellId;                                          // id from Spell.dbc
    public uint SpellTrigger;
    public int SpellCharges;
    public float SpellPPMRate;
    public int SpellCooldown;
    public uint SpellCategory;                                   // id from SpellCategory.dbc
    public int SpellCategoryCooldown;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ItemSocket
{
    public uint Color;
    public uint Content;
};

public static class ItemTemplateConst
{
    public static readonly int MAX_ITEM_PROTO_DAMAGES = 2;                            // changed in 3.1.0
    public static readonly int MAX_ITEM_PROTO_SOCKETS = 3;
    public static readonly int MAX_ITEM_PROTO_SPELLS = 5;
    public static readonly int MAX_ITEM_PROTO_STATS = 10;
}

public struct ItemTemplate
{
    public uint ItemId;
    public uint Class;                                          // id from ItemClass.dbc
    public uint SubClass;                                       // id from ItemSubClass.dbc
    public int SoundOverrideSubclass;                           // < 0: id from ItemSubClass.dbc, used to override weapon sound from actual SubClass
    public string Name1;
    public uint DisplayInfoID;                                  // id from ItemDisplayInfo.dbc
    public uint Quality;
    public uint Flags;
    public uint Flags2;
    public uint BuyCount;
    public int BuyPrice;
    public uint SellPrice;
    public uint InventoryType;
    public uint AllowableClass;
    public uint AllowableRace;
    public uint ItemLevel;
    public uint RequiredLevel;
    public uint RequiredSkill;                                  // id from SkillLine.dbc
    public uint RequiredSkillRank;
    public uint RequiredSpell;                                  // id from Spell.dbc
    public uint RequiredHonorRank;
    public uint RequiredCityRank;
    public uint RequiredReputationFaction;                      // id from Faction.dbc
    public uint RequiredReputationRank;
    public int MaxCount;                                        // <= 0: no limit
    public int Stackable;                                       // 0: not allowed, -1: put in player coin info tab and don't limit stacking (so 1 slot)
    public uint ContainerSlots;
    public uint StatsCount;
    public ItemStat[] ItemStat;
    public uint ScalingStatDistribution;                        // id from ScalingStatDistribution.dbc
    public uint ScalingStatValue;                               // mask for selecting column in ScalingStatValues.dbc
    public Damage[] Damage;
    public uint Armor;
    public int HolyRes;
    public int FireRes;
    public int NatureRes;
    public int FrostRes;
    public int ShadowRes;
    public int ArcaneRes;
    public uint Delay;
    public uint AmmoType;
    public float RangedModRange;
    public Spell[] Spells;
    public uint Bonding;
    public string Description;
    public uint PageText;
    public uint LanguageID;
    public uint PageMaterial;
    public uint StartQuest;                                     // id from QuestCache.wdb
    public uint LockID;
    public int Material;                                        // id from Material.dbc
    public uint Sheath;
    public int RandomProperty;                                  // id from ItemRandomProperties.dbc
    public int RandomSuffix;                                    // id from ItemRandomSuffix.dbc
    public uint Block;
    public uint ItemSet;                                        // id from ItemSet.dbc
    public uint MaxDurability;
    public uint Area;                                           // id from AreaTable.dbc
    public uint Map;                                            // id from Map.dbc
    public uint BagFamily;                                      // bit mask (1 << id from ItemBagFamily.dbc)
    public uint TotemCategory;                                  // id from TotemCategory.dbc
    public ItemSocket[] Socket;
    public uint socketBonus;                                    // id from SpellItemEnchantment.dbc
    public uint GemProperties;                                  // id from GemProperties.dbc
    public uint RequiredDisenchantSkill;
    public float ArmorDamageModifier;
    public uint Duration;
    public uint ItemLimitCategory;                              // id from ItemLimitCategory.dbc
    public uint HolidayId;                                      // id from Holidays.dbc
    public uint ScriptId;
    public uint DisenchantID;
    public uint FoodType;
    public uint MinMoneyLoot;
    public uint MaxMoneyLoot;
    public uint FlagsCu;

    public ItemTemplate()
    {
        Name1 = string.Empty;
        Description = string.Empty;

        ItemStat = new ItemStat[ItemTemplateConst.MAX_ITEM_PROTO_STATS];
        Damage = new Damage[ItemTemplateConst.MAX_ITEM_PROTO_DAMAGES];
        Spells = new Spell[ItemTemplateConst.MAX_ITEM_PROTO_SPELLS];
        Socket = new ItemSocket[ItemTemplateConst.MAX_ITEM_PROTO_SOCKETS];
    }
}
