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

public abstract class SHA1
{
    public byte[] Hash { get; private set; }

    public abstract void Update(byte[]? data);
    public abstract void Final(byte[]? data);

    protected SHA1()
    {
        Hash = Array.Empty<byte>();
    }

    public static SHA1 Create()
    {
        return new Implement();
    }

    private sealed class Implement : SHA1
    {
        private System.Security.Cryptography.SHA1 serviceProvidor;

        internal Implement()
        {
            serviceProvidor = System.Security.Cryptography.SHA1.Create();
            serviceProvidor.Initialize();
        }

        public override void Update(byte[]? data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            serviceProvidor?.TransformBlock(data, 0, data.Length, data, 0);
        }

        public override void Final(byte[]? data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            serviceProvidor?.TransformFinalBlock(data, 0, data.Length);
            Hash = serviceProvidor?.Hash ?? Array.Empty<byte>();
        }
    }
}
