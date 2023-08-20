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

using AzerothCore.Utilities;

namespace AzerothCore.Game;

public class WorldPacketData : ByteBuffer, IDisposable
{
    public ushort Opcode { get; set; } = (ushort)OpcodeMisc.NULL_OPCODE;
    public DateTime ReceivedTime { get; private set; }

    public WorldPacketData() : base() { }

    public WorldPacketData(Opcodes opcode) : base()
    {
        Opcode = (ushort)opcode;
    }

    public WorldPacketData(Opcodes opcode, byte[] data) : base(data)
    {
        Opcode = (ushort)opcode;
    }

    public WorldPacketData(byte[] data) : base(data) { }

    public WorldPacketData(WorldPacketData packet) : base(packet.GetData())
    {
        Opcode = packet.Opcode;
        ReceivedTime = packet.ReceivedTime;
    }

    public WorldPacketData(WorldPacketData packet, DateTime receivedTime) : this(packet)
    {
        ReceivedTime = receivedTime;
    }
}

public enum PacketDirection : ushort
{
    CLIENT_TO_SERVER,
    SERVER_TO_CLIENT
}

public abstract class ClientPacket : IDisposable
{
    protected WorldPacketData _packetData;

    protected ClientPacket(WorldPacketData packetData)
    {
        _packetData = packetData;
    }

    public abstract void Read();

    public void Dispose()
    {
        _packetData.Dispose();
    }

    public Opcodes GetOpcode() { return (Opcodes)_packetData.Opcode; }
}

public abstract class ServerPacket
{
    private byte[]? buffer;
    protected WorldPacketData _packetData;

    protected ServerPacket(Opcodes opcode)
    {
        _packetData = new WorldPacketData(opcode);
    }

    public void Clear()
    {
        _packetData.Clear();
        buffer?.Clear();
    }

    public Opcodes GetOpcode()
    {
        return (Opcodes)_packetData.Opcode;
    }

    public byte[]? GetData()
    {
        return buffer;
    }

    public abstract void Write();

    public void WritePacketData()
    {
        if (buffer != null)
        {
            return;
        }

        Write();

        buffer = _packetData.GetData();
        _packetData.Dispose();
    }
}
