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

using AzerothCore.Server.Protocol;
using AzerothCore.Utilities;

namespace AzerothCore.Server;

public class WorldPacket : ByteBuffer
{
    public DateTime     ReceivedTime    { get; set; }
    public ushort       Opcode          { get; set; } = (ushort)OpcodeMisc.NULL_OPCODE;

    public WorldPacket() : base() {  }

    public WorldPacket(byte[] data) : base(data) {  }

    public WorldPacket(WorldPacket packet)
    {
        byte[] copyFrom = packet.GetData();
        byte[] copyTo = new byte[copyFrom.Length];

        Array.Copy(copyFrom, copyTo, copyFrom.Length);

        _readStream = new BinaryReader(new MemoryStream(copyTo));
    }
}
