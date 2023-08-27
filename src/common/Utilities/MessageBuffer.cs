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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AzerothCore.Utilities;

public class MessageBuffer
{
    private byte[] _storage;
    private int _wpos;
    private int _rpos;

    public MessageBuffer(int initialSize = 4096)
    {
        _storage = new byte[initialSize];
    }

    public void Resize(int size)
    {
        Array.Resize(ref _storage, size);

        if (size > 0)
        {
            if (_wpos >= size)
            {
                _wpos = size - 1;
            }

            if (_rpos >= size)
            {
                _rpos = size - 1;
            }
        }
        else
        {
            _wpos = 0;
            _rpos = 0;
        }
    }

    public void Reset()
    {
        _wpos = 0;
        _rpos = 0;
    }

    public byte[] GetBasePointer()
    {
        return _storage;
    }

    public int GetReadPos()
    {
        return _rpos;
    }

    public int GetWritePos()
    {
        return _wpos;
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
                unsafe
                {
                    byte* readPointer = (byte *)Unsafe.AsPointer(ref _storage[_rpos]);
                    Marshal.Copy((IntPtr)readPointer, _storage, 0, GetActiveSize());
                }
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
            Resize(_storage.Length * 3 / 2);
        }
    }

    public void Write(byte[] arr, int size)
    {
        if (size > 0)
        {
            Write(arr, 0, size);
        }
    }

    public void Write(byte[] arr, int offset, int size)
    {
        if (size > 0)
        {
            Array.Copy(arr, offset, _storage, _wpos, size);
            WriteCompleted(size);
        }
    }
}
