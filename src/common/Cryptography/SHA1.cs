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

using System;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;

namespace AzerothCore.Cryptography;

public abstract class SHA1
{
	private SHA1() { }

    public abstract void Update(byte[]? array);
    public abstract byte[] Final();

    public static SHA1 Create()
	{
		return new Implement();
	}

    private sealed class Implement : SHA1
    {
        private System.Security.Cryptography.SHA1 serviceProvidorserviceProvider = System.Security.Cryptography.SHA1.Create();

        public override void Update(byte[]? array)
        {
            if (array == null)
            {
                throw new NullReferenceException(nameof(array));
            }

            Type invoker = serviceProvidorserviceProvider.GetType();

            invoker.InvokeMember(
                        "HashCore",
                        BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance,
                        null,
                        serviceProvidorserviceProvider,
                        new object[] { array, 0, array.Length });
        }

        public override byte[] Final()
        {
            Type invoker = serviceProvidorserviceProvider.GetType();

            byte[]? hashedValue = invoker.InvokeMember(
                                    "HashFinal",
                                    BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance,
                                    null,
                                    serviceProvidorserviceProvider,
                                    Array.Empty<object?>()) as byte[];

            if (hashedValue == null)
            {
                throw new InvalidOperationException();
            }

            return hashedValue;
        }
    }
}
