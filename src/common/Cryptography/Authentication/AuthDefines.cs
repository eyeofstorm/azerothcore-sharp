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

using System.Collections;
using System.Collections.Generic;

namespace AzerothCore.Cryptography;

public class SessionKey : ICloneable, ICollection, IEnumerable
{
    private byte[] _sessionKey;

    public SessionKey()
    {
        _sessionKey = new byte[40];
    }

    public byte[] GetBytes()
    {
        return _sessionKey;
    }

    public byte this[int index]
    {
        get => _sessionKey[index];
    }

    public int Length { get { return _sessionKey.Length; } }

    int ICollection.Count => _sessionKey.Length;

    bool ICollection.IsSynchronized => _sessionKey.IsSynchronized;

    object ICollection.SyncRoot => _sessionKey.SyncRoot;

    object ICloneable.Clone()
    {
        return _sessionKey.Clone();
    }

    void ICollection.CopyTo(Array array, int index)
    {
        _sessionKey.CopyTo(array, index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _sessionKey.GetEnumerator();
    }
}
