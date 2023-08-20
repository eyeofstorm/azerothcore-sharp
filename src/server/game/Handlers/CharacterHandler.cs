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

namespace AzerothCore.Game;

public interface ICharacterHandler
{
    void HandleCharCreateOpcode(WorldPacketData recvData);
    void HandleCharCustomize(WorldPacketData recvData);
    void HandleCharDeleteOpcode(WorldPacketData recvData);
    void HandleCharEnumOpcode(WorldPacketData recvData);
    void HandleCharFactionOrRaceChange(WorldPacketData recvData);
    void HandleCharRenameOpcode(WorldPacketData recvData);
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

        _queryProcessor.AddCallback(DB.Characters.AsyncQuery(stmt).WithCallback(HandleCharEnum));
    }

    private void HandleCharEnum(SQLResult result)
    {
        WorldPacketData data = new (Opcodes.SMSG_CHAR_ENUM);                  // we guess size

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
}
