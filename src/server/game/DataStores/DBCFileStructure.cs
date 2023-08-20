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

public static class DBCStructureConst
{
    public static readonly int MAX_SPELL_ITEM_ENCHANTMENT_EFFECTS = 3;
}

[Flags]
public enum ChrRacesFlags : byte
{
    CHRRACES_FLAGS_NOT_PLAYABLE = 0x01,
    CHRRACES_FLAGS_BARE_FEET    = 0x02,
    CHRRACES_FLAGS_CAN_MOUNT    = 0x04
}

public class ChrRacesEntry : DBCFileEntry
{
    public uint RaceID              { get; set; }                           // 0
    public uint Flags               { get; set; }                           // 1
    public uint FactionID           { get; set; }                           // 2 facton template id
                                                                            // 3 unused
    public uint ModelMale           { get; set; }                           // 4
    public uint ModelFemale         { get; set; }                           // 5
                                                                            // 6 unused
    public uint TeamID              { get; set; }                           // 7 (7-Alliance 1-Horde)
                                                                            // 8-11 unused
    public uint CinematicSequence   { get; set; }                           // 12 id from CinematicSequences.dbc
                                                                            // 13 faction (0 alliance, 1 horde, 2 not available?)
    public List<string> Name        { get; set; }                           // 14-29 used for DBC language detection/selection
                                                                            // 30 string flags, unused
                                                                            // 31-46, if different from base (male) case
                                                                            // 47 string flags, unused
                                                                            // 48-63, if different from base (male) case
                                                                            // 64 string flags, unused
                                                                            // 65-67 unused
    public uint Expansion;                                                  // 68 (0 - original race, 1 - tbc addon, ...)


    public ChrRacesEntry()
    {
        Name = new List<string>();

        for (int i = 0; i < 16; i++)
        {
            Name.Add(string.Empty);
        }
    }

    public override void SetField<T>(DBCFieldInfo<T> fieldInfo)
    {
        if (fieldInfo.FieldValue is uint uint32Value)
        {
            switch (fieldInfo.FieldIdx)
            {
            case 0:
                RaceID = uint32Value;
                break;
            case 1:
                Flags = uint32Value;
                break;
            case 2:
                FactionID = uint32Value;
                break;
            case 4:
                ModelMale = uint32Value;
                break;
            case 5:
                ModelFemale = uint32Value;
                break;
            case 7:
                TeamID = uint32Value;
                break;
            case 12:
                CinematicSequence = uint32Value;
                break;
            case 68:
                Expansion = uint32Value;
                break;
            default:
                break;
            }
        }
        else if (fieldInfo.FieldValue is string stringValue)
        {
            if (fieldInfo.FieldIdx >= 14 && fieldInfo.FieldIdx <= 29)
            {
                Name[fieldInfo.FieldIdx - 14] = stringValue;
            }
        }
    }

    public bool HasFlag(ChrRacesFlags flag)
    {
        return (Flags & (byte)flag) != 0x00;
    }
}

public class ChrClassesEntry : DBCFileEntry
{
    public uint ClassID { get; set; }                           // 0
                                                                // 1, unused
    public uint PowerType { get; set; }                         // 2
                                                                // 3-4, unused
    public List<string> Name { get; set; }                      // 5-20 unused
                                                                // 21 string flag, unused
                                                                // 21-36 unused, if different from base (male) case
                                                                // 37 string flag, unused
                                                                // 38-53 unused, if different from base (male) case
                                                                // 54 string flag, unused
                                                                // 55, unused
    public uint SpellFamily { get; set; }                       // 56
                                                                // 57, unused
    public uint CinematicSequence { get; set; }                 // 58 id from CinematicSequences.dbc
    public uint Expansion { get; set; }                         // 59 (0 - original race, 1 - tbc addon, ...)

    public ChrClassesEntry()
    {
        Name = new List<string>();

        for (int i = 0; i < 16; i++)
        {
            Name.Add(string.Empty);
        }
    }

    public override void SetField<T>(DBCFieldInfo<T> fieldInfo)
    {
        if (fieldInfo.FieldValue is uint uint32Value)
        {
            switch (fieldInfo.FieldIdx)
            {
            case 0:
                ClassID = uint32Value;
                break;
            case 2:
                PowerType = uint32Value;
                break;
            case 56:
                SpellFamily = uint32Value;
                break;
            case 58:
                CinematicSequence = uint32Value;
                break;
            case 59:
                Expansion = uint32Value;
                break;
            default:
                break;
            }
        }
        else if (fieldInfo.FieldValue is string stringValue)
        {
            if (fieldInfo.FieldIdx >= 5 && fieldInfo.FieldIdx <= 20)
            {
                Name[fieldInfo.FieldIdx - 5] = stringValue;
            }
        }
    }
}

public class MapEntry : DBCFileEntry
{
    public uint MapID { get; set; }                             // 0
                                                                // 1 unused
    public uint MapType { get; set; }                           // 2
    public uint Flags { get; set; }                             // 3
                                                                // 4 0 or 1 for battlegrounds (not arenas)
    public List<string> Name { get; set; }                      // 5-20
                                                                // 21 name flags, unused
    public uint LinkedZone { get; set; }                        // 22 common zone for instance and continent map
                                                                // 23-38 text for PvP Zones
                                                                // 39 intro text flags
                                                                // 40-55 text for PvP Zones
                                                                // 56 intro text flags
    public uint MultimapId { get; set; }                        // 57
                                                                // 58
    public int EntranceMap { get; set; }                        // 59 map_id of entrance map
    public float EntranceX { get; set; }                        // 60 entrance x coordinate (if exist single entry)
    public float EntranceY { get; set; }                        // 61 entrance y coordinate (if exist single entry)
                                                                // 62 -1, 0 and 720
    public uint Addon { get; set; }                             // 63 (0-original maps, 1-tbc addon)
    public uint UnkTime { get; set; }                           // 64 some kind of time?
    public uint MaxPlayers { get; set; }                        // 65 max players, fallback if not present in MapDifficulty.dbc

    public MapEntry()
    {
        Name = new List<string>();

        for (int i = 0; i < 16; i++)
        {
            Name.Add(string.Empty);
        }
    }

    public override void SetField<T>(DBCFieldInfo<T> fieldInfo)
    {
        if (fieldInfo.FieldValue is uint uint32Value)
        {
            switch (fieldInfo.FieldIdx)
            {
            case 0:
                MapID = uint32Value;
                break;
            case 2:
                MapType = uint32Value;
                break;
            case 3:
                Flags = uint32Value;
                break;
            case 22:
                LinkedZone = uint32Value;
                break;
            case 57:
                MultimapId = uint32Value;
                break;
            case 63:
                Addon = uint32Value;
                break;
            case 64:
                UnkTime = uint32Value;
                break;
            case 65:
                MaxPlayers = uint32Value;
                break;
            default:
                break;
            }
        }
        else if (fieldInfo.FieldValue is int int32Value)
        {
            switch (fieldInfo.FieldIdx)
            {
            case 59:
                EntranceMap = int32Value;
                break;
            default:
                break;
            }
        }
        else if (fieldInfo.FieldValue is float floatValue)
        {
            switch (fieldInfo.FieldIdx)
            {
            case 60:
                EntranceX = floatValue;
                break;
            case 61:
                EntranceY = floatValue;
                break;
            default:
                break;
            }
        }
        else if (fieldInfo.FieldValue is string stringValue)
        {
            if (fieldInfo.FieldIdx >= 5 && fieldInfo.FieldIdx <= 20)
            {
                Name[fieldInfo.FieldIdx - 5] = stringValue;
            }
        }
    }

    // Helpers
    public bool IsDungeon() { return MapType == (uint)MapTypes.MAP_INSTANCE || MapType == (uint)MapTypes.MAP_RAID; }
    public bool IsNonRaidDungeon() { return MapType == (uint)MapTypes.MAP_INSTANCE; }
    public bool Instanceable() { return MapType == (uint)MapTypes.MAP_INSTANCE || MapType == (uint)MapTypes.MAP_RAID || MapType == (uint)MapTypes.MAP_BATTLEGROUND || MapType == (uint)MapTypes.MAP_ARENA; }
    public bool IsRaid() { return MapType == (uint)MapTypes.MAP_RAID; }
    public bool IsBattleground() { return MapType == (uint)MapTypes.MAP_BATTLEGROUND; }
    public bool IsBattleArena() { return MapType == (uint)MapTypes.MAP_ARENA; }
    public bool IsBattlegroundOrArena() { return MapType == (uint)MapTypes.MAP_BATTLEGROUND || MapType == (uint)MapTypes.MAP_ARENA; }
    public bool IsWorldMap() { return MapType == (uint)MapTypes.MAP_COMMON; }

    public bool GetEntrancePos(ref int mapid, ref float x, ref float y)
    {
        if (EntranceMap < 0)
        {
            return false;
        }

        mapid = EntranceMap;
        x = EntranceX;
        y = EntranceY;

        return true;
    }

    public bool IsContinent() 
    {
        return MapID == 0 || MapID == 1 || MapID == 530 || MapID == 571;
    }

    public bool IsDynamicDifficultyMap()  { return (Flags & (uint)MapFlags.MAP_FLAG_DYNAMIC_DIFFICULTY) != 0; }
}

public class SpellItemEnchantmentEntry : DBCFileEntry
{
    public uint ID { get; set; }                                             // 0        m_ID
    public uint Charges { get; set; }                                        // 1        m_charges
    public uint[] Type { get; set; }                                         // 2-4      m_effect[MAX_SPELL_ITEM_ENCHANTMENT_EFFECTS]
    public uint[] Amount { get; set; }                                       // 5-7      m_effectPointsMin[MAX_SPELL_ITEM_ENCHANTMENT_EFFECTS]
                                                                             // 8-10     m_effectPointsMax[MAX_SPELL_ITEM_ENCHANTMENT_EFFECTS]
    public uint[] SpellId { get; set; }                                      // 11-13    m_effectArg[MAX_SPELL_ITEM_ENCHANTMENT_EFFECTS]
    public List<string> Description { get; set; }                            // 14-29    m_name_lang[16]
                                                                             // 30       name flags
    public uint AuraId { get; set; }                                         // 31       m_itemVisual
    public uint Slot { get; set; }                                           // 32       m_flags
    public uint GemID { get; set; }                                          // 33       m_src_itemID
    public uint EnchantmentCondition { get; set; }                           // 34       m_condition_id
    public uint RequiredSkill { get; set; }                                  // 35       m_requiredSkillID
    public uint RequiredSkillValue { get; set; }                             // 36       m_requiredSkillRank
    public uint RequiredLevel { get; set; }                                  // 37       m_requiredLevel

    public SpellItemEnchantmentEntry()
    {
        Type = new uint[DBCStructureConst.MAX_SPELL_ITEM_ENCHANTMENT_EFFECTS];
        Amount = new uint[DBCStructureConst.MAX_SPELL_ITEM_ENCHANTMENT_EFFECTS];
        SpellId = new uint[DBCStructureConst.MAX_SPELL_ITEM_ENCHANTMENT_EFFECTS];

        Description = new List<string>();

        for (int i = 0; i < 16; i++)
        {
            Description.Add(string.Empty);
        }
    }

    public override void SetField<T>(DBCFieldInfo<T> fieldInfo)
    {
        if (fieldInfo.FieldValue is uint uint32Value)
        {
            switch (fieldInfo.FieldIdx)
            {
            case 0:
                ID = uint32Value;
                break;
            case 1:
                Charges = uint32Value;
                break;
            case 2:
                Type[0] = uint32Value;
                break;
            case 3:
                Type[1] = uint32Value;
                break;
            case 4:
                Type[2] = uint32Value;
                break;
            case 5:
                Amount[0] = uint32Value;
                break;
            case 6:
                Amount[1] = uint32Value;
                break;
            case 7:
                Amount[2] = uint32Value;
                break;
            case 11:
                SpellId[0] = uint32Value;
                break;
            case 12:
                SpellId[1] = uint32Value;
                break;
            case 13:
                SpellId[2] = uint32Value;
                break;
            case 31:
                AuraId = uint32Value;
                break;
            case 32:
                Slot = uint32Value;
                break;
            case 33:
                GemID = uint32Value;
                break;
            case 34:
                EnchantmentCondition = uint32Value;
                break;
            case 35:
                RequiredSkill = uint32Value;
                break;
            case 36:
                RequiredSkillValue = uint32Value;
                break;
            case 37:
                RequiredLevel = uint32Value;
                break;
            default:
                break;
            }
        }
        else if (fieldInfo.FieldValue is string stringValue)
        {
            if (fieldInfo.FieldIdx >= 14 && fieldInfo.FieldIdx <= 29)
            {
                Description[fieldInfo.FieldIdx - 5] = stringValue;
            }
        }
    }
}
