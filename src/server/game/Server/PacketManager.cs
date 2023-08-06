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

internal abstract class OpcodeHandler
{
    public SessionStatus Status { get; protected set; }

    public OpcodeHandler(SessionStatus status)
    {
        Status = status;
    }
}

internal class ServerOpcodeHandler : OpcodeHandler
{
    public ServerOpcodeHandler(SessionStatus status) : base(status) { }
}

internal abstract class ClientOpcodeHandler : OpcodeHandler
{
    public PacketProcessing ProcessingPlace { get; protected set; }

    public ClientOpcodeHandler(SessionStatus status, PacketProcessing processing) : base(status)
    {
        ProcessingPlace = processing;
    }

    protected abstract void Call(WorldSession session, WorldPacketData packet);
}

internal sealed class WorldPacketHandler : ClientOpcodeHandler
{
    private readonly Action<WorldSession, ClientPacket>? _methodCaller;
    private readonly Type? _packetType;

    public WorldPacketHandler(SessionStatus status, PacketProcessing processing) : base(status, processing) {  }

    public WorldPacketHandler(MethodInfo info, SessionStatus status, PacketProcessing processing, Type type) : this(status, processing)
    {
        _methodCaller = (Action<WorldSession, ClientPacket>?)GetType()
                            .GetMethod("CreateDelegate", BindingFlags.Static | BindingFlags.NonPublic)?
                            .MakeGenericMethod(type)
                            .Invoke(null, new object[] { info });

        _packetType = type;
    }

    protected override void Call(WorldSession session, WorldPacketData packet)
    {
        if (_packetType == null)
        {
            return;
        }

        using var clientPacket = (ClientPacket?)Activator.CreateInstance(_packetType, packet);

        if (clientPacket != null)
        {
            clientPacket.Read();
            _methodCaller?.Invoke(session, clientPacket);
        }
    }

    public static Action<WorldSession, WorldPacketData> CreateDelegate<P1>(MethodInfo method) where P1 : WorldPacketData
    {
        // create first delegate. It is not fine because its 
        // signature contains unknown types T and P1
        Action<WorldSession, P1> d = (Action<WorldSession, P1>)method.CreateDelegate(typeof(Action<WorldSession, P1>));

        // create another delegate having necessary signature. 
        // It encapsulates first delegate with a closure
        return delegate (WorldSession target, WorldPacketData p) { d(target, (P1)p); };
    }
}
