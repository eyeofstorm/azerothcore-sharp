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

namespace AzerothCore.Cryptography;

public class HMACSHA1 : System.Security.Cryptography.HMACSHA1
{
    public byte[]? Digest { get; private set; }

    public HMACSHA1() : base()
    {
        Initialize();
    }

    public HMACSHA1(byte[] key) : base(key)
    {
        Initialize();
    }

    public void UpdateData(byte[] data, int length)
    {
        TransformBlock(data, 0, length, data, 0);
    }

    public void Finalize(byte[] data, int length)
    {
        TransformFinalBlock(data, 0, length);
        Digest = Hash;
    }

    public byte[]? GetDigestOf(byte[] data)
    {
        UpdateData(data, data.Length);
        Finalize(data, data.Length);

        return Digest;
    }
}
