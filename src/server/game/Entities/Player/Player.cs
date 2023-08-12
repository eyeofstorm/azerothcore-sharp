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

using AzerothCore.Constants;
using AzerothCore.Database;

namespace AzerothCore.Game;

public struct PlayerCreateInfoItems
{
    public uint ItemId;
    public uint ItemAmount;

    public PlayerCreateInfoItems() { }

    public PlayerCreateInfoItems(uint id, uint amount)
    {
        ItemId = id;
        ItemAmount = amount;
    }
}

public struct PlayerCreateInfoSkills
{

}

public struct PlayerCreateInfoActions
{

}

public struct PlayerCreateInfoSpells
{

}

public struct PlayerLevelInfo
{
    public uint[] Stats = new uint[SharedConst.MAX_STATS];

    public PlayerLevelInfo() { }
}

public struct PlayerInfo
{
    public uint MapId;
    public uint AreaId;
    public float PositionX;
    public float PositionY;
    public float PositionZ;
    public float Orientation;
    public ushort DisplayId_m;
    public ushort DisplayId_f;
    public PlayerCreateInfoItems Item;
    public PlayerCreateInfoSpells CustomSpells;
    public PlayerCreateInfoSpells CastSpells;
    public PlayerCreateInfoActions Action;
    public PlayerCreateInfoSkills Skills;
    public PlayerLevelInfo LevelInfo;                             // [level-1] 0..MaxPlayerLevel - 1

    public PlayerInfo() { }
}

public partial class Player
{
	public Player() { }

    internal static bool BuildEnumData(SQLResult result, WorldPacketData data)
    {
        //             0               1                2                3                 4                  5                 6               7
        //    "SELECT characters.guid, characters.name, characters.race, characters.class, characters.gender, characters.skin, characters.face, characters.hairStyle,
        //     8                     9                       10              11               12              13                     14                     15
        //    characters.hairColor, characters.facialStyle, character.level, characters.zone, characters.map, characters.position_x, characters.position_y, characters.position_z,
        //    16                    17                      18                   19                   20                     21                   22               23
        //    guild_member.guildid, characters.playerFlags, characters.at_login, character_pet.entry, character_pet.modelid, character_pet.level, characters.equipmentCache, character_banned.guid,
        //    24                      25
        //    characters.extra_flags, character_declinedname.genitive

        SQLFields fields = result.GetFields();

        uint guidLow = fields.Read<uint>(0);
        string? plrName = fields.Read<string>(1);
        byte plrRace = fields.Read<byte>(2);
        byte plrClass = fields.Read<byte>(3);
        byte gender = fields.Read<byte>(4);

        ObjectGuid guid = ObjectGuid.Create(HighGuid.Player, guidLow);

        PlayerInfo? info = Global.sObjectMgr.GetPlayerInfo(plrRace, plrClass);

        //if (!info)
        //{
        //    LOG_ERROR("entities.player", "Player {} has incorrect race/class pair. Don't build enum.", guid.ToString());
        //    return false;
        //}
        //else if (!IsValidGender(gender))
        //{
        //    LOG_ERROR("entities.player", "Player ({}) has incorrect gender ({}), don't build enum.", guid.ToString(), gender);
        //    return false;
        //}

        //data << guid;
        //data.WriteCString(plrName);                             // name
        //data << uint8(plrRace);                                 // race
        //data << uint8(plrClass);                                // class
        //data << uint8(gender);                                  // gender
        
        //uint8 skin = fields[5].Get<uint8>();
        //uint8 face = fields[6].Get<uint8>();
        //uint8 hairStyle = fields[7].Get<uint8>();
        //uint8 hairColor = fields[8].Get<uint8>();
        //uint8 facialStyle = fields[9].Get<uint8>();

        //uint32 charFlags = 0;
        //uint32 playerFlags = fields[17].Get<uint32>();
        //uint16 atLoginFlags = fields[18].Get<uint16>();
        //uint32 zone = (atLoginFlags & AT_LOGIN_FIRST) != 0 ? 0 : fields[11].Get<uint16>(); // if first login do not show the zone

        //*data << uint8(skin);
        //*data << uint8(face);
        //*data << uint8(hairStyle);
        //*data << uint8(hairColor);
        //*data << uint8(facialStyle);

        //*data << uint8(fields[10].Get<uint8>());                   // level
        //*data << uint32(zone);                                   // zone
        //*data << uint32(fields[12].Get<uint16>());                 // map

        //*data << fields[13].Get<float>();                          // x
        //*data << fields[14].Get<float>();                          // y
        //*data << fields[15].Get<float>();                          // z

        //*data << uint32(fields[16].Get<uint32>());                 // guild id

        //if (atLoginFlags & AT_LOGIN_RESURRECT)
        //    playerFlags &= ~PLAYER_FLAGS_GHOST;

        //if (playerFlags & PLAYER_FLAGS_HIDE_HELM)
        //    charFlags |= CHARACTER_FLAG_HIDE_HELM;

        //if (playerFlags & PLAYER_FLAGS_HIDE_CLOAK)
        //    charFlags |= CHARACTER_FLAG_HIDE_CLOAK;

        //if (playerFlags & PLAYER_FLAGS_GHOST)
        //    charFlags |= CHARACTER_FLAG_GHOST;

        //if (atLoginFlags & AT_LOGIN_RENAME)
        //    charFlags |= CHARACTER_FLAG_RENAME;

        //if (fields[23].Get<uint32>())
        //    charFlags |= CHARACTER_FLAG_LOCKED_BY_BILLING;

        //if (sWorld->getBoolConfig(CONFIG_DECLINED_NAMES_USED))
        //{
        //    if (!fields[25].Get < std::string> ().empty())
        //        charFlags |= CHARACTER_FLAG_DECLINED;
        //}
        //else
        //{
        //    charFlags |= CHARACTER_FLAG_DECLINED;
        //}

        //*data << uint32(charFlags);                              // character flags

        //// character customize flags
        //if (atLoginFlags & AT_LOGIN_CUSTOMIZE)
        //    *data << uint32(CHAR_CUSTOMIZE_FLAG_CUSTOMIZE);
        //else if (atLoginFlags & AT_LOGIN_CHANGE_FACTION)
        //    *data << uint32(CHAR_CUSTOMIZE_FLAG_FACTION);
        //else if (atLoginFlags & AT_LOGIN_CHANGE_RACE)
        //    *data << uint32(CHAR_CUSTOMIZE_FLAG_RACE);
        //else
        //    *data << uint32(CHAR_CUSTOMIZE_FLAG_NONE);

        //// First login
        //*data << uint8(atLoginFlags & AT_LOGIN_FIRST ? 1 : 0);

        //// Pets info
        //uint petDisplayId = 0;
        //uint petLevel = 0;
        //uint petFamily = 0;

        //// show pet at selection character in character list only for non-ghost character
        //if (result &&
        //    !(playerFlags & PLAYER_FLAGS_GHOST) &&
        //    (plrClass == CLASS_WARLOCK ||
        //    plrClass == CLASS_HUNTER ||
        //    (plrClass == CLASS_DEATH_KNIGHT && (fields[21].Get<uint32>() & PLAYER_EXTRA_SHOW_DK_PET))))
        //{
        //    uint entry = fields[19].Get<uint>();
        //    CreatureTemplate creatureInfo = sObjectMgr->GetCreatureTemplate(entry);

        //    if (creatureInfo)
        //    {
        //        petDisplayId = fields[20].Get<uint>();
        //        petLevel = fields[21].Get<ushort>();
        //        petFamily = creatureInfo->family;
        //    }
        //}

        //*data << uint32(petDisplayId);
        //*data << uint32(petLevel);
        //*data << uint32(petFamily);

        //std::vector<std::string_view> equipment = Acore::Tokenize(fields[22].Get<std::string_view>(), ' ', false);

        //for (byte slot = 0; slot < INVENTORY_SLOT_BAG_END; ++slot)
        //{
        //    uint const visualBase = slot * 2;
        //    Optional<uint> itemId;

        //    if (visualBase < equipment.size())
        //    {
        //        itemId = Acore::StringTo<uint32>(equipment[visualBase]);
        //    }

        //    ItemTemplate? proto = null;

        //    if (itemId)
        //    {
        //        proto = sObjectMgr->GetItemTemplate(*itemId);
        //    }

        //    if (!proto)
        //    {
        //        if (!itemId || *itemId)
        //        {
        //            LOG_WARN("entities.player.loading", "Player {} has invalid equipment '{}' in `equipmentcache` at index {}. Skipped.", guid.ToString(), (visualBase < equipment.size()) ? equipment[visualBase] : "<none>", visualBase);
        //        }

        //        *data << uint32(0);
        //        *data << uint8(0);
        //        *data << uint32(0);

        //        continue;
        //    }

        //    SpellItemEnchantmentEntry? enchant = null;

        //    Optional<uint32> enchants = { };

        //    if ((visualBase + 1) < equipment.size())
        //    {
        //        enchants = Acore::StringTo<uint32>(equipment[visualBase + 1]);
        //    }

        //    if (!enchants)
        //    {
        //        LOG_WARN("entities.player.loading", "Player {} has invalid enchantment info '{}' in `equipmentcache` at index {}. Skipped.", guid.ToString(), ((visualBase + 1) < equipment.size()) ? equipment[visualBase + 1] : "<none>", visualBase + 1);

        //        enchants = 0;
        //    }

        //    for (byte enchantSlot = PERM_ENCHANTMENT_SLOT; enchantSlot <= TEMP_ENCHANTMENT_SLOT; ++enchantSlot)
        //    {
        //        // values stored in 2 uint16
        //        uint enchantId = 0x0000FFFF & ((*enchants) >> enchantSlot * 16);

        //        if (enchantId == 0)
        //        {
        //            continue;
        //        }

        //        enchant = sSpellItemEnchantmentStore.LookupEntry(enchantId);

        //        if (enchant != null)
        //        {
        //            break;
        //        }
        //    }

        //    *data << uint32(proto->DisplayInfoID);
        //    *data << uint8(proto->InventoryType);
        //    *data << uint32(enchant ? enchant->aura_id : 0);
        //}

        return true;
    }

    public bool IsInWorld()
    {
        // TODO: game: Player::IsInWorld()
        return false;
    }
}
