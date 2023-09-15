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
using AzerothCore.Logging;

namespace AzerothCore.Game;

public partial class Player
{
    internal bool LoadFromDB(ObjectGuid playerGuid, LoginQueryHolder holder)
    {
        ////                                                     0     1        2     3     4        5      6    7      8     9    10    11         12         13           14         15         16
        //QueryResult* result = CharacterDatabase.Query("SELECT guid, account, name, race, class, gender, level, xp, money, skin, face, hairStyle, hairColor, facialStyle, bankSlots, restState, playerFlags, "
        // 17          18          19          20   21           22        23        24         25         26          27           28                 29
        //"position_x, position_y, position_z, map, orientation, taximask, cinematic, totaltime, leveltime, rest_bonus, logout_time, is_logout_resting, resettalents_cost, "
        // 30                 31       32       33       34       35         36           37             38        39    40      41                 42         43
        //"resettalents_time, trans_x, trans_y, trans_z, trans_o, transguid, extra_flags, stable_slots, at_login, zone, online, death_expire_time, taxi_path, instance_mode_mask, "
        // 44           45                46                 47                    48          49          50              51           52               53              54
        //"arenaPoints, totalHonorPoints, todayHonorPoints, yesterdayHonorPoints, totalKills, todayKills, yesterdayKills, chosenTitle, knownCurrencies, watchedFaction, drunk, "
        // 55      56      57      58      59      60      61      62      63           64                 65                 66             67              68      69
        //"health, power1, power2, power3, power4, power5, power6, power7, instance_id, talentGroupsCount, activeTalentGroup, exploredZones, equipmentCache, ammoId, knownTitles,
        // 70          71               72            73
        //"actionBars, grantableLevels, innTriggerId, extraBonusTalentCount FROM characters WHERE guid = '{}'", guid);
        SQLResult result = holder.GetResult(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_FROM);

        if (result.IsEmpty())
        {
            logger.Error(LogFilter.PlayerLoading, $"Player ({playerGuid}) not found in table `characters`, can't load. ");

            return false;
        }

        SQLFields fields = result.GetFields();

        uint dbAccountId = fields.Read<uint>(1);

        // check if the character's account in the db and the logged in account match.
        // player should be able to load/delete character only with correct account!
        if (dbAccountId != GetSession().GetAccountId())
        {
            logger.Error(LogFilter.PlayerLoading, $"Player ({playerGuid}) loading from wrong account (is: {GetSession().GetAccountId()}, should be: {dbAccountId})");

            return false;
        }

        if (holder.GetResult(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_BANNED).IsEmpty())
        {
            logger.Error(LogFilter.PlayerLoading, $"Player ({playerGuid}) is banned, can't load.");

            return false;
        }

        uint guid = playerGuid.GetCounter();

        Create(guid, 0, HighGuid.Player);

        _name = fields.Read<string>(2) ?? string.Empty;

        // check name limitations
        if (
            ObjectMgr.CheckPlayerName(_name) != ResponseCodes.CHAR_NAME_SUCCESS ||
            (
             AccountMgr.IsPlayerAccount(GetSession().GetSecurity()) &&
             (
              Global.sObjectMgr.IsReservedName(_name) ||
              Global.sObjectMgr.IsProfanityName(_name)
             )
            )
           )
        {
            PreparedStatement stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_UPD_ADD_AT_LOGIN_FLAG);

            stmt.AddValue(0, (ushort)AtLoginFlags.AT_LOGIN_RENAME);
            stmt.AddValue(1, guid);

            DB.Characters.Execute(stmt);

            return false;
        }

        byte Gender = fields.Read<byte>(5);

        if (!IsValidGender(Gender))
        {
            logger.Error(LogFilter.PlayerLoading, $"Player (GUID: {guid}) has wrong gender ({Gender}), can't be loaded.");

            return false;
        }

        // overwrite some data fields
        uint bytes0 = 0;
        bytes0 |= fields.Read<byte>(3);                         // race
        bytes0 |= (uint)(fields.Read<byte>(4) << 8);            // class
        bytes0 |= (uint)(Gender << 16);                         // gender

        SetUInt32Value((ushort)EUnitFields.UNIT_FIELD_BYTES_0, bytes0);

        _realRace = fields.Read<byte>(3);   // set real race
        _race = fields.Read<byte>(3);       // set real race

        SetUInt32Value((ushort)EUnitFields.UNIT_FIELD_LEVEL, fields.Read<byte>(6));
        SetUInt32Value((ushort)EUnitFields.PLAYER_XP, fields.Read<uint>(7));

        if (!LoadIntoDataField(fields.Read<string>(66), (ushort)EUnitFields.PLAYER_EXPLORED_ZONES_1, (uint)PlayerConst.PLAYER_EXPLORED_ZONES_SIZE))
        {
            logger.Warn(LogFilter.PlayerLoading, $"Player::LoadFromDB: Player ({guid}) has invalid exploredzones data ({fields.Read<string>(66)}). Forcing partial load.");
        }

        if (!LoadIntoDataField(fields.Read<string>(69), (ushort)EUnitFields.PLAYER__FIELD_KNOWN_TITLES, (uint)PlayerConst.KNOWN_TITLES_SIZE * 2))
        {
            logger.Warn(LogFilter.PlayerLoading, $"Player::LoadFromDB: Player ({guid}) has invalid knowntitles mask ({fields.Read<string>(69)}). Forcing partial load.");
        }

        SetObjectScale(1.0f);
        SetFloatValue((ushort)EUnitFields.UNIT_FIELD_HOVERHEIGHT, 1.0f);



        // TODO: game: Player::LoadFromDB(ObjectGuid playerGuid, LoginQueryHolder holder)

        return true;
    }
}
