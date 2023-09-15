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

namespace AzerothCore.Game;

public enum UpdateMaskCount : uint
{
    CLIENT_UPDATE_MASK_BITS = sizeof(uint) * 8,
}

public class UpdateMask 
{
    private uint _fieldCount;
    private uint _blockCount;
    private byte[]? _bits;

    public UpdateMask()
    {
        _fieldCount = 0;
        _blockCount = 0;
        _bits = null;
    }

    internal void SetBit(uint index)
    {
        if (_bits != null)
        {
            _bits[index] = 1;
        }
    }

    internal void UnsetBit(uint index)
    {
        if (_bits != null)
        {
            _bits[index] = 0;
        }
    }

    internal void SetCount(ushort valuesCount)
    {
        _bits = null;

        _fieldCount = valuesCount;
        _blockCount = (valuesCount + (uint)UpdateMaskCount.CLIENT_UPDATE_MASK_BITS - 1) / (uint)UpdateMaskCount.CLIENT_UPDATE_MASK_BITS;

        _bits = new byte[_blockCount * (uint)UpdateMaskCount.CLIENT_UPDATE_MASK_BITS];
    }
}
