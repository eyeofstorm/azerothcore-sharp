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
using System.Buffers;
using System.Runtime.InteropServices;

namespace AzerothCore.Utilities;

public class MessageBuffer
{
    private Memory<byte>    _storage;
    private int             _wpos;
    private int             _rpos;

    public MessageBuffer(int initialSize = 4096)
    {
        _storage = new byte[initialSize];
    }

    private void ExpandStorageSize(int size)
    {
        Memory<byte> newStorage = new byte[size];
        _storage.CopyTo(newStorage);
        _storage = newStorage;
        newStorage = null;
    }

    public void Reset()
    {
        _wpos = 0;
        _rpos = 0;
    }

    public Memory<byte> GetBasePointer()
    {
        return _storage;
    }

    public Memory<byte> GetReadPointer()
    {
        return _storage.Slice(_rpos);
    }

    public Memory<byte> GetWritePointer()
    {
        return _storage.Slice(_wpos);
    }

    public void ReadCompleted(int bytes)
    {
        _rpos += bytes;
    }

    public void WriteCompleted(int bytes)
    {
        _wpos += bytes;
    }

    public int GetActiveSize()
    {
        return _wpos - _rpos;
    }

    public int GetRemainingSpace()
    {
        return _storage.Length - _wpos;
    }

    public int GetBufferSize()
    {
        return _storage.Length;
    }

    // Discards inactive data
    public void Normalize()
    {        
        if (_rpos != 0)
        {
            if (_rpos != _wpos)
            {
                GetReadPointer().Slice(0, GetActiveSize()).CopyTo(GetBasePointer());
            }

            _wpos -= _rpos;
            _rpos = 0;
        }
    }

    // Ensures there's "some" free space, make sure to call Normalize() before this
    public void EnsureFreeSpace()
    {
        // resize buffer if it's already full
        if (GetRemainingSpace() == 0)
        {
            ExpandStorageSize(_storage.Length * 3 / 2);
        }
    }

    public void Write(byte[] arr, int size)
    {
        if (size > 0)
        {
            Memory<byte> arrWrapper = new Memory<byte>(arr);
            Memory<byte> copyFrom = arrWrapper.Slice(0, size);

            if (copyFrom.TryCopyTo(_storage))
            {
                WriteCompleted(size);
            }
        }
    }
}
