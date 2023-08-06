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

using System.Collections.Concurrent;
using System.Reflection;

using AzerothCore.Game.Server;
using AzerothCore.Singleton;

namespace AzerothCore.Game;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class WorldPacketHandlerAttribute : Attribute
{
    public WorldPacketHandlerAttribute(Opcodes opcode)
    {
        Opcode = opcode;
        Status = SessionStatus.STATUS_LOGGEDIN;
        Processing = PacketProcessing.PROCESS_THREADUNSAFE;
    }

    public Opcodes Opcode { get; private set; }
    public SessionStatus Status { get; set; }
    public PacketProcessing Processing { get; set; }
}

public sealed class OpcodeTable : Singleton<OpcodeTable>
{
    private readonly ConcurrentDictionary<Opcodes, WorldPacketHandler?> _clientPacketTable = new();

    private OpcodeTable() { }

    public void Initialize()
    {
        Assembly currentAsm = Assembly.GetExecutingAssembly();

        foreach (var type in currentAsm.GetTypes())
        {
            foreach (var methodInfo in type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                foreach (var msgAttr in methodInfo.GetCustomAttributes<WorldPacketHandlerAttribute>())
                {
                    if (msgAttr == null)
                    {
                        continue;
                    }

                    if (_clientPacketTable.ContainsKey(msgAttr.Opcode))
                    {
                        continue;
                    }

                    var parameters = methodInfo.GetParameters();

                    if (parameters.Length == 0)
                    {
                        continue;
                    }

                    if (parameters[0].ParameterType.BaseType != typeof(ClientPacket))
                    {
                        continue;
                    }

                    _clientPacketTable[msgAttr.Opcode] = new WorldPacketHandler(methodInfo, msgAttr.Status, msgAttr.Processing, parameters[0].ParameterType);
                }
            }
        }
    }

    internal WorldPacketHandler? this[Opcodes opcode]
    {
        get
        {
            if (_clientPacketTable.ContainsKey(opcode))
            {
                return _clientPacketTable[opcode];
            }
            else
            {
                return null;
            }
        }

        set
        {
            _clientPacketTable[opcode] = value;
        }
    }
}
