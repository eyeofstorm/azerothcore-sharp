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

public interface IMiscOpcodeHandler
{
    void HandleRepopRequestOpcode(WorldPacketData recvData);
    void HandleGossipSelectOptionOpcode(WorldPacketData recvData);
    void HandleWhoOpcode(WorldPacketData recvData);
    void HandleLogoutRequestOpcode(WorldPacketData recvData);
    void HandlePlayerLogoutOpcode(WorldPacketData recvData);
    void HandleLogoutCancelOpcode(WorldPacketData recvData);
    void HandleTogglePvP(WorldPacketData recvData);
    void HandleZoneUpdateOpcode(WorldPacketData recvData);
    void HandleSetSelectionOpcode(WorldPacketData recvData);
    void HandleStandStateChangeOpcode(WorldPacketData recvData);
    void HandleBugOpcode(WorldPacketData recvData);
    void HandleReclaimCorpseOpcode(WorldPacketData recvData);
    void HandleReadyForAccountDataTimes(WorldPacketData recvData);
    void HandleRealmSplitOpcode(WorldPacketData recvData);
}

public partial class WorldSession : IMiscOpcodeHandler
{
    public bool HandleSocketClosed()
    {
        // TODO: game: WorldSession::HandleSocketClosed()
        return false;
    }

    public void HandleRepopRequestOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGossipSelectOptionOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleWhoOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePlayerLogoutOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLogoutCancelOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleTogglePvP(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleZoneUpdateOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSetSelectionOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleStandStateChangeOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleBugOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleReclaimCorpseOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleReadyForAccountDataTimes(WorldPacketData recvData)
    {
        logger.Debug(Logging.LogFilter.Network, "WORLD: CMSG_READY_FOR_ACCOUNT_DATA_TIMES");

        SendAccountDataTimes(GLOBAL_CACHE_MASK);
    }

    public void HandleRealmSplitOpcode(WorldPacketData recvData)
    {
        logger.Debug(Logging.LogFilter.Network, "WORLD: CMSG_REALM_SPLIT");

        string splitDate = "01/01/01";
        uint unk = recvData.ReadUInt32();

        WorldPacketData data = new (Opcodes.SMSG_REALM_SPLIT, 4 + 4 + splitDate.Length + 1);

        data.WriteUInt32(unk);

        data.WriteUInt32(0x00000000);                           // realm split state
                                                                // split states:
                                                                // 0x0 realm normal
                                                                // 0x1 realm split
                                                                // 0x2 realm split pending
        data.WriteCString(splitDate);

        SendPacket(data);
    }
}
