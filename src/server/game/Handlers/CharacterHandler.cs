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

using AzerothCore.Configuration;
using AzerothCore.Constants;
using AzerothCore.Database;
using AzerothCore.Logging;
using AzerothCore.Utilities;

namespace AzerothCore.Game;

public interface ICharacterHandler
{
    void HandleCharCreateOpcode(WorldPacketData recvData);
    void HandleCharCustomize(WorldPacketData recvData);
    void HandleCharDeleteOpcode(WorldPacketData recvData);
    void HandleCharEnumOpcode(WorldPacketData recvData);
    void HandleCharFactionOrRaceChange(WorldPacketData recvData);
    void HandleCharRenameOpcode(WorldPacketData recvData);
    void HandlePlayerLoginOpcode(WorldPacketData recvData);
}

public partial class WorldSession : ICharacterHandler
{
    public void HandleCharCreateOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::HandleCharCreateOpcode(WorldPacketData recvData)
    }

    public void HandleCharDeleteOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::HandleCharDeleteOpcode(WorldPacketData recvData)
    }

    public void HandleCharCustomize(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::HandleCharCustomize(WorldPacketData recvData)
    }

    public void HandleCharFactionOrRaceChange(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::HandleCharFactionOrRaceChange(WorldPacketData recvData)
    }

    public void HandleCharRenameOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::HandleCharRenameOpcode(WorldPacketData recvData)
    }

    public void HandleCharEnumOpcode(WorldPacketData recvData)
    {
        logger.Debug(Logging.LogFilter.Network, "WORLD: CMSG_CHAR_ENUM");

        RealmZone zone = ConfigMgr.GetOption("RealmZone", RealmZone.REALM_ZONE_DEVELOPMENT);
        bool isDeclinedNamesUsed = zone == RealmZone.REALM_ZONE_RUSSIAN || ConfigMgr.GetOption("DeclinedNames", false);

        PreparedStatement stmt = isDeclinedNamesUsed ?
            CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_ENUM_DECLINED_NAME) :
            CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_ENUM);

        // get all the data necessary for loading all characters (along with their pets) on the account
        stmt.AddValue(0, (sbyte)PetSaveMode.PET_SAVE_AS_CURRENT);
        stmt.AddValue(1, GetAccountId());

        _queryProcessor.AddCallback(DB.Characters.AsyncQuery(stmt).WithCallback(HandleCharEnumCallback));
    }

    private void HandleCharEnumCallback(SQLResult result)
    {
        WorldPacketData data = new(Opcodes.SMSG_CHAR_ENUM);

        byte numberOfCharacters = 0;
        data.WriteByte(numberOfCharacters);

        _legitCharacters.Clear();

        if (!result.IsEmpty())
        {
            do
            {
                ObjectGuid guid = ObjectGuid.Create(HighGuid.Player, result.Read<uint>(0));

                logger.Debug(Logging.LogFilter.Network, $"Loading char {guid} from account {GetAccountId()}.");

                if (Player.BuildEnumData(result, ref data))
                {
                    _legitCharacters.Add(guid);
                    ++numberOfCharacters;
                }
            }
            while (result.NextRow());
        }

        data.PutByte(0, numberOfCharacters);

        SendPacket(data);
    }

    public void HandlePlayerLoginOpcode(WorldPacketData recvData)
    {
        _playerLoading = true;
        ObjectGuid playerGuid = ObjectGuid.Create(recvData);

        if (IsPlayerLoading() || GetPlayer() != null || !playerGuid.IsPlayer())
        {
            // limit player interaction with the world
            if (!ConfigMgr.GetOption("World.RealmAvailability", true))
            {
                WorldPacketData data = new (Opcodes.SMSG_CHARACTER_LOGIN_FAILED);

                // see LoginFailureReason enum for more reasons
                data.WriteByte((byte)LoginFailureReason.NoWorld);

                SendPacket(data);

                return;
            }
        }

        if (!playerGuid.IsPlayer() || !IsLegitCharacterForAccount(playerGuid))
        {
            logger.Error(LogFilter.Network, $"Account ({GetAccountId()}) can't login with that character ({playerGuid}).");

            KickPlayer("Account can't login with this character");

            return;
        }

        var SendCharLogin = (ResponseCodes result) =>
        {
            WorldPacketData data = new(Opcodes.SMSG_CHARACTER_LOGIN_FAILED);

            data.WriteByte((byte)result);

            SendPacket(data);
        };

        WorldSession? sess = Global.sWorld.FindOfflineSessionForCharacterGUID(playerGuid.GetCounter());

        if (sess != null)
        {
            if (sess.GetAccountId() != GetAccountId())
            {
                SendCharLogin(ResponseCodes.CHAR_LOGIN_DUPLICATE_CHARACTER);

                return;
            }
        }

        sess = Global.sWorld.FindOfflineSession(GetAccountId());

        if (sess != null)
        {
            Player? p = sess.GetPlayer();

            if (p == null || sess.IsKicked())
            {
                SendCharLogin(ResponseCodes.CHAR_LOGIN_DUPLICATE_CHARACTER);

                return;
            }

            if (p.GetGUID() != playerGuid)
            {
                sess.KickPlayer("No return, go to normal loading"); // no return, go to normal loading
            }
            else
            {
                // this somehow froze (probably, ahh crash logs ...),
                // and while (far) have never frozen in LogoutPlayer o_O
                // maybe it's the combination of while(far); while(near);
                byte limitA = 10, limitB = 10, limitC = 10;

                while (p != null && (p.IsBeingTeleportedFar() || (p.IsInWorld() && p.IsBeingTeleportedNear())))
                {
                    if (limitA == 0 || --limitA == 0)
                    {
                        logger.Info(LogFilter.Misc, "HandlePlayerLoginOpcode A");
                        break;
                    }

                    while (p != null && p.IsBeingTeleportedFar())
                    {
                        if (limitB == 0 || --limitB == 0)
                        {
                            logger.Info(LogFilter.Misc, "HandlePlayerLoginOpcode B");
                            break;
                        }

                        sess.HandleMoveWorldportAck();
                    }

                    while (p != null && p.IsInWorld() && p.IsBeingTeleportedNear())
                    {
                        if (limitC == 0 || --limitC == 0)
                        {
                            logger.Info(LogFilter.Misc, "HandlePlayerLoginOpcode C");
                            break;
                        }

                        Player? plMover = p.Mover?.ToPlayer();

                        if (plMover == null)
                        {
                            break;
                        }

                        // TODO: game: CharacterHandler::HandlePlayerLoginOpcode(WorldPacketdata recvData)
                        //WorldPacketData pkt = new (Opcodes.MSG_MOVE_TELEPORT_ACK);

                        //pkt.WriteObjectGuid(plMover.GetPackGUID());
                        //pkt.WriteUInt(0); // flags
                        //pkt.WriteUInt(0); // time

                        //sess.HandleMoveTeleportAck(pkt);
                    }
                }

                if (p != null && (p.FindMap() == null || !p.IsInWorld() || sess.IsKicked()))
                {
                    SendCharLogin(ResponseCodes.CHAR_LOGIN_DUPLICATE_CHARACTER);

                    return;
                }

                // TODO: game: CharacterHandler::HandlePlayerLoginOpcode(WorldPacketdata recvData)
                //sess->SetPlayer(nullptr);
                //SetPlayer(p);
                //p->SetSession(this);
                //delete p->PlayerTalkClass;
                //p->PlayerTalkClass = new PlayerMenu(p->GetSession());
                //HandlePlayerLoginToCharInWorld(p);

                return;
            }
        }

        LoginQueryHolder holder = new (GetAccountId(), playerGuid);
        holder.Initialize();

        AddQueryHolderCallback(DB.Characters.DelayQueryHolder(holder)).AfterComplete(holder =>
        {
            if (holder is LoginQueryHolder result)
            {
                HandlePlayerLoginFromDB(result);
            }
        });
    }

    private void HandlePlayerLoginFromDB(LoginQueryHolder holder)
    {
        ObjectGuid playerGuid = holder.GetGuid();

        Player pCurrChar = new(this);

        // for send server info and strings (config)
        ChatHandler chH = new ChatHandler(pCurrChar.GetSession());

        // "GetAccountId() == db stored account id" checked in LoadFromDB (prevent login not own character using cheating tools)
        if (!pCurrChar.LoadFromDB(playerGuid, holder))
        {
            SetPlayer(null);

            // disconnect client, player no set to session and it will not deleted or saved at kick
            KickPlayer("WorldSession::HandlePlayerLogin Player::LoadFromDB failed");

            _playerLoading = false;

            return;
        }



        // TODO: game: WorldSession::HandlePlayerLoginFromDB(LoginQueryHolder holder)
    }

    private bool IsLegitCharacterForAccount(ObjectGuid playerGuid)
    {
        return _legitCharacters.Contains(playerGuid);
    }
}

public class LoginQueryHolder : SQLQueryHolder<PlayerLoginQueryIndex>
{
    private readonly uint m_accountId;
    private readonly ObjectGuid m_guid;

    public LoginQueryHolder(uint accountId, ObjectGuid guid)
    {
        m_accountId = accountId;
        m_guid = guid;
    }

    public ObjectGuid GetGuid() { return m_guid; }

    public uint GetAccountId() { return m_accountId; }

    public void Initialize()
    {
        uint lowGuid = m_guid.GetCounter();

        var stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_FROM, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_AURAS);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_AURAS, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_SPELL);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_SPELLS, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_QUESTSTATUS);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_QUEST_STATUS, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_DAILYQUESTSTATUS);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_DAILY_QUEST_STATUS, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_WEEKLYQUESTSTATUS);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_WEEKLY_QUEST_STATUS, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_MONTHLYQUESTSTATUS);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_MONTHLY_QUEST_STATUS, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_SEASONALQUESTSTATUS);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_SEASONAL_QUEST_STATUS, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_REPUTATION);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_REPUTATION, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_INVENTORY);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_INVENTORY, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_ACTIONS);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_ACTIONS, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_MAIL);
        stmt.AddValue(0, lowGuid);
        stmt.AddValue(1, TimeHelper.UnixTime);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_MAILS, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_MAILITEMS);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_MAIL_ITEMS, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_SOCIALLIST);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_SOCIAL_LIST, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_HOMEBIND);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_HOME_BIND, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_SPELLCOOLDOWNS);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_SPELL_COOLDOWNS, stmt);

        RealmZone zone = ConfigMgr.GetOption("RealmZone", RealmZone.REALM_ZONE_DEVELOPMENT);
        bool isDeclinedNamesUsed = zone == RealmZone.REALM_ZONE_RUSSIAN || ConfigMgr.GetOption("DeclinedNames", false);

        if (isDeclinedNamesUsed)
        {
            stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_DECLINEDNAMES);
            stmt.AddValue(0, lowGuid);
            SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_DECLINED_NAMES, stmt);
        }

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_ACHIEVEMENTS);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_ACHIEVEMENTS, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_CRITERIAPROGRESS);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_CRITERIA_PROGRESS, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_EQUIPMENTSETS);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_EQUIPMENT_SETS, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_ENTRY_POINT);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_ENTRY_POINT, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_GLYPHS);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_GLYPHS, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_TALENTS);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_TALENTS, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_PLAYER_ACCOUNT_DATA);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_ACCOUNT_DATA, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_SKILLS);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_SKILLS, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_RANDOMBG);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_RANDOM_BG, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_BANNED);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_BANNED, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_QUESTSTATUSREW);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_QUEST_STATUS_REW, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_BREW_OF_THE_MONTH);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_BREW_OF_THE_MONTH, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_ACCOUNT_INSTANCELOCKTIMES);
        stmt.AddValue(0, m_accountId);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_INSTANCE_LOCK_TIMES, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CORPSE_LOCATION);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_CORPSE_LOCATION, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHAR_SETTINGS);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_CHARACTER_SETTINGS, stmt);

        stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_SEL_CHAR_PETS);
        stmt.AddValue(0, lowGuid);
        SetQuery(PlayerLoginQueryIndex.PLAYER_LOGIN_QUERY_LOAD_PET_SLOTS, stmt);
    }
}
