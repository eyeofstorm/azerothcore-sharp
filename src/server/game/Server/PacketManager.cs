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

using System.Reflection;

namespace AzerothCore.Game.Server;

public enum SessionStatus
{
    STATUS_AUTHED = 0,                                      // Player authenticated (_player == nullptr, m_playerRecentlyLogout = false or will be reset before handler call, m_GUID have garbage)
    STATUS_LOGGEDIN,                                        // Player in game (_player != nullptr, m_GUID == _player->GetGUID(), inWorld())
    STATUS_TRANSFER,                                        // Player transferring to another map (_player != nullptr, m_GUID == _player->GetGUID(), !inWorld())
    STATUS_LOGGEDIN_OR_RECENTLY_LOGGOUT,                    // _player != nullptr or _player == nullptr && m_playerRecentlyLogout && m_playerLogout, m_GUID store last _player guid)
    STATUS_NEVER,                                           // Opcode not accepted from client (deprecated or server side only)
    STATUS_UNHANDLED,                                       // Opcode not handled yet
}

public enum PacketProcessing
{
    PROCESS_INPLACE = 0,                                    //process packet whenever we receive it - mostly for non-handled or non-implemented packets
    PROCESS_THREADUNSAFE,                                   //packet is not thread-safe - process it in World::UpdateSessions()
    PROCESS_THREADSAFE                                      //packet is thread-safe - process it in Map::Update()
}

public delegate void OpcodeHandlerFunction(WorldPacketData recvData);

internal abstract class OpcodeHandler
{
    internal SessionStatus Status { get; private set; }

    internal OpcodeHandler(SessionStatus status)
    {
        Status = status;
    }
}

internal abstract class ClientOpcodeHandler : OpcodeHandler
{
    internal PacketProcessing ProcessingPlace { get; private set; }

    internal ClientOpcodeHandler(SessionStatus status, PacketProcessing processing) : base(status)
    {
        ProcessingPlace = processing;
    }

    internal abstract void Call(WorldSession session, WorldPacketData packet);
}

internal sealed class WorldPacketHandler : ClientOpcodeHandler
{
    private readonly OpcodeHandlerFunction _method;

    internal WorldPacketHandler(SessionStatus status, PacketProcessing processing, OpcodeHandlerFunction method) : base(status, processing)
    {
        _method = method;
    }

    internal override void Call(WorldSession session, WorldPacketData packet)
    {
        // call opcode handler function
        _method.GetMethodInfo().Invoke(session, new object[] { packet });
    }
}

internal sealed class PacketHandler : ClientOpcodeHandler
{
    internal PacketHandler(SessionStatus status) : base(status, PacketProcessing.PROCESS_INPLACE) { }

    internal override void Call(WorldSession session, WorldPacketData packet)
    {
        // call opcode handler function
        OpcodeHandlerFunction method = WorldSession.OpcodeHandler.Handle_ServerSide;
        method.GetMethodInfo()?.Invoke(session, new object[] { packet });
    }
}
