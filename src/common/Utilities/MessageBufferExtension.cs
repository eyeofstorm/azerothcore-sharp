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

using System.Runtime.InteropServices;

namespace AzerothCore.Utilities
{
    public static class MessageBufferExtensions
	{
        public static T CastTo<T>(this MessageBuffer msgBuff) where T : struct
        {
            Type objType = typeof(T);

            byte[] buffer = new Memory<byte>(msgBuff.GetBasePointer(), msgBuff.GetReadPos(), msgBuff.GetActiveSize()).ToArray();

            if (buffer.Length > 0)
            {
                IntPtr ptrObj = IntPtr.Zero;

                try
                {
                    int objSize = Marshal.SizeOf(objType);

                    if (objSize > 0)
                    {
                        if (buffer.Length > objSize)
                        {
                            throw new Exception($"Buffer greater than needed for creation of object of type {objType}");
                        }

                        ptrObj = Marshal.AllocHGlobal(objSize);

                        if (ptrObj != IntPtr.Zero)
                        {
                            int copySize = buffer.Length <= objSize ? buffer.Length : objSize;
                            Marshal.Copy(buffer, 0, ptrObj, copySize);
                            object? obj = Marshal.PtrToStructure(ptrObj, objType);

                            return (T)(obj ?? default(T));
                        }
                        else
                        {
                            throw new Exception($"Couldn't allocate memory to create object of type {objType}");
                        }
                    }
                }
                finally
                {
                    if (ptrObj != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(ptrObj);
                    }
                }
            }

            return default(T);
        }
    }
}
