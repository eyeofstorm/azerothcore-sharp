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

// Provides methods and properties for implementing ARC4 data encryption.
internal sealed class ARC4Drop1024
{
    private static readonly int STATE_LENGTH = 256;

    private readonly byte[] _State;
    private int             _I;
    private int             _J;

    public ARC4Drop1024()
    {
        _State = new byte[STATE_LENGTH];
        _I = 0;
        _J = 0;
    }

    private static void Swap(byte[] array, int index1, int index2)
    {
        (array[index2], array[index1]) = (array[index1], array[index2]);
    }

    private void KSA(byte[]? key)
    {
        if (key == null)
        {
            return;
        }

        for (int i = 0, j = 0; i < STATE_LENGTH; i++)
        {
            j = (byte)((j + _State[i] + key[i % key.Length]) % STATE_LENGTH);
            Swap(_State, i, j);
        }
    }

    // Pseudo-random number generator
    // To generate the keystream, the cipher uses a hidden internal state, which consists of two parts:
    //   - A permutation containing all possible bytes from 0x00 to 0xFF (array _sblock).
    //   - Variables-counters I and J.
    private byte PRGA()
    {
        _I = (_I + 1) % STATE_LENGTH;
        _J = (_J + _State[_I]) % STATE_LENGTH;
        Swap(_State, _I, _J);

        return _State[(_State[_I] + _State[_J]) % STATE_LENGTH];
    }

    public void Init(byte[]? key)
    {
        for (int i = 0; i < STATE_LENGTH; i++)
        {
            _State[i] = (byte)i;
        }

        KSA(key);

        //--------------------------------------------------
        // Drop first 1024 bytes
        for (int i = 0; i < 1024; i++)
        {
            PRGA();
        }
        //--------------------------------------------------
    }

    public void UpdateData(byte[] input, int offset, int length)
    {
        for (int i = offset; i < length; i++)
        {
            input[i] = (byte)(input[i] ^ PRGA());
        }
    }
}
