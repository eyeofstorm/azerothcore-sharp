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
using System.Security.Cryptography;

namespace AzerothCore.Cryptography;

internal abstract class CryptoProvider
{
    public static unsafe void EraseArray(ref byte[] array)
    {
        if (array != null && array.Length != 0)
        {
            int length = array.Length;

            fixed (byte* ptr = array)
            {
                for (int i = 0; i < length; i++)
                {
                    *(ptr + i) = 0;
                }
            }
        }
    }

    public abstract void Cipher(byte[] buffer, int count);
}

/// <summary>
///     Represents the initial state of the cryptographic algorithm <see cref = "ARC4" />.
///     This class could not be inherited.
/// </summary> 
[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 256)]
internal sealed class ARC4SBlock : IDisposable, ICloneable
{
    private static readonly byte[] _A =
    {
        0x09, 0x0D, 0x11, 0x15, 0x19, 0x1D, 0x21, 0x25,
        0x29, 0x2D, 0x31, 0x35, 0x39, 0x3D, 0x41, 0x45,
        0x49, 0x4D, 0x51, 0x55, 0x59, 0x5D, 0x61, 0x65,
        0x69, 0x6D, 0x71, 0x75, 0x79, 0x7D, 0x81, 0x85,
        0x89, 0x8D, 0x91, 0x95, 0x99, 0x9D, 0xA1, 0xA5,
        0xA9, 0xAD, 0xB1, 0xB5, 0xB9, 0xBD, 0xC1, 0xC5,
        0xC9, 0xCD, 0xD1, 0xD5, 0xD9, 0xDD, 0xE1, 0xE5,
        0xE9, 0xED, 0xF1, 0xF5, 0xF9
    };

    private static readonly byte[] _C =
    {
        0x05, 0x07, 0x0B, 0x0D, 0x11, 0x13, 0x17, 0x1D,
        0x1F, 0x25, 0x29, 0x2B, 0x2F, 0x35, 0x3B, 0x3D,
        0x43, 0x47, 0x49, 0x4F, 0x53, 0x59, 0x61, 0x65,
        0x67, 0x6B, 0x6D, 0x71, 0x7F, 0x83, 0x89, 0x8B,
        0x95, 0x97, 0x9D, 0xA3, 0xA7, 0xAD, 0xB3, 0xB5,
        0xBF, 0xC1, 0xC5, 0xC7, 0xD3, 0xDF, 0xE3, 0xE5,
        0xE9, 0xEF, 0xF1, 0xFB
    };

    public static readonly ARC4SBlock DefaultSBlock = new ARC4SBlock();

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    private byte[] _bytes = new byte[256];

    /// <summary>
    ///     Initializes an instance <see cref = "ARC4SBlock" />,
    ///     filled with pseudo-random values,
    ///     using the linear congruential random.
    /// </summary>
    /// <returns>
    ///     Instance <see cref = "ARC4SBlock" />.
    /// </returns> 
    public static ARC4SBlock GenerateRandom()
    {
        byte[] random = new byte[4];

        RandomNumberGenerator.Create().GetBytes(random);

        int r = random[0];
        int x = random[1];
        int a = _A[random[2] % _A.Length];
        int c = _C[random[3] % _C.Length];

        return new ARC4SBlock(x, a, c, r);
    }

    /// <summary>
    ///     Initializes an instance <see cref = "ARC4SBlock" />,
    ///     using the specified values.
    /// </summary>
    /// <param name = "bytes">
    ///     The initialization vector <see cref = "ARC4SBlock" />,
    ///     must be filled with 256 non-duplicate values.
    /// </param>
    /// <returns>
    ///     Instance <see cref = "ARC4SBlock" />.
    /// </returns> 
    public static ARC4SBlock FromBytes(params byte[] bytes)
    {
        if (!ValidBytes(bytes))
        {
            throw new DuplicateWaitObjectException("bytes");
        }

        return new ARC4SBlock(bytes);
    }

    /// <summary>
    ///     Initializes an instance <see cref = "ARC4SBlock" />,
    ///     using the specified salt.
    /// </summary>
    /// <param name = "salt">
    ///     Salt for the LCR algorithm. It must contain at least 4 bytes.
    /// </param>
    /// <returns>
    ///     Instance <see cref = "ARC4SBlock" />.
    /// </returns> 
    public static ARC4SBlock FromSalt(params byte[] salt)
    {
        if (salt.Length < 4)
        {
            throw new DuplicateWaitObjectException("bytes");
        }

        return new ARC4SBlock(salt[2], _A[salt[1] % _A.Length], _C[salt[0] % _C.Length], salt[3]);
    }

    /// <summary>
    ///     Converts <see cref="ARC4SBlock"/> to <see cref="byte"/> array.
    /// </summary>
    /// <param name="sblock">
    ///     Instance <see cref="ARC4SBlock"/> for converting.
    /// </param>
    public static implicit operator byte[](ARC4SBlock sblock)
    {
        if (sblock == null)
        {
            throw new ArgumentNullException(nameof(sblock));
        }
        if (sblock._bytes == null)
        {
            throw new ObjectDisposedException(nameof(sblock));
        }

        byte[] bytes = new byte[256];
        Array.Copy(sblock._bytes ?? DefaultSBlock._bytes, bytes, 256);
        return bytes;
    }

    /// <summary>
    ///     Converts <see cref="byte"/> array to <see cref="ARC4SBlock"/>.
    /// </summary>
    /// <param name="bytes">
    ///     Array for converting.
    /// </param>
    public static explicit operator ARC4SBlock(byte[] bytes)
    {
        if (bytes == null)
        {
            throw new ArgumentNullException(nameof(bytes));
        }
        if (!ValidBytes(bytes))
        {
            throw new DuplicateWaitObjectException(nameof(bytes));
        }

        return new ARC4SBlock(bytes);
    }

    // Checks that all 256 values should not be duplicated.
    internal static bool ValidBytes(byte[] bytes)
    {
        if (bytes == null || bytes.Length != 256)
        {
            return false;
        }

        for (int i = 0; i < 256; i++)
        {
            for (int j = i + 1; j < 256; j++)
            {
                if (bytes[i] == bytes[j])
                {
                    return false;
                }
            }
        }

        return true;
    }

    // Default S-Block.
    private ARC4SBlock()
    {
        for (int i = 0; i < 256; i++)
        {
            _bytes[i] = (byte)i;
        }
    }

    // Specified S-Block.
    internal ARC4SBlock(byte[] bytes)
    {
        Array.Copy(bytes, _bytes, 256);
    }

    // Random S-Block.
    internal ARC4SBlock(int x, int a, int c, int r)
    {
        const int m = 256;

        for (int i = 0; i < m; i++)
        {
            _bytes[i] = (byte)(r ^ (x = (a * x + c) % m));
        }
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        if (_bytes == null)
        {
            return;
        }

        CryptoProvider.EraseArray(ref _bytes);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="ICloneable.Clone"/>
    public object Clone()
    {
        byte[] result = new byte[256];

        for (int i = 0; i < 256; i++)
        {
            result[i] = _bytes[i];
        }

        return result;
    }
}

// Provides methods and properties for implementing ARC4 data encryption.
internal sealed class ARC4CryptoProvider : CryptoProvider, IDisposable
{
    private byte[] _sblock = new byte[256];
    private int x = 0;
    private int y = 0;

    public ARC4SBlock State => new ARC4SBlock(_sblock);

    private static void Swap(byte[] array, int index1, int index2)
    {
        byte b = array[index1];
        array[index1] = array[index2];
        array[index2] = b;
    }

    /* 
       Pseudo-random number generator
	    To generate the keystream, the cipher uses a hidden internal state, which consists of two parts:
		  - A permutation containing all possible bytes from 0x00 to 0xFF (array _sblock).
		  - Variables-counters x and y.
	*/
    public byte NextByte() // PRGA
    {
        x = (x + 1) % 256;
        y = (y + _sblock[x]) % 256;

        Swap(_sblock, x, y);

        return _sblock[(_sblock[x] + _sblock[y]) % 256];
    }

    public ARC4CryptoProvider(byte[] key) // KSA
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        int keyLength = key.Length;

        if (keyLength == 0)
        {
            throw new ArgumentException(null, nameof(key));
        }

        try
        {
            _sblock = ARC4SBlock.DefaultSBlock;

            int j = 0;
            for (int i = 0; i < 256; i++)
            {
                j = (j + _sblock[i] + key[i % keyLength]) % 256;
                Swap(_sblock, i, j);
            }
        }
        catch (Exception e)
        {
            throw new CryptographicException(e.Message, e);
        }
    }

    public override void Cipher(byte[] buffer, int count)
    {
        if (buffer == null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        int bufferLength = buffer.Length;

        if (bufferLength == 0)
        {
            throw new ArgumentException(null, nameof(buffer));
        }

        if (count >= 0 && count <= bufferLength)
        {
            if (count == 0)
            {
                return;
            }

            try
            {
                for (int i = 0; i < count; i++)
                {
                    buffer[i] = (byte)(buffer[i] ^ NextByte());
                }
            }
            catch (Exception e)
            {
                throw new CryptographicException(e.Message, e);
            }
        }
        else
        {
            throw new ArgumentException(null, nameof(count));
        }
    }

    #region IDisposable implements
    private bool _disposed = false;

    internal unsafe void EraseState()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            fixed (int* ptr = &x) *ptr = -1;
            fixed (int* ptr = &y) *ptr = -1;

            EraseArray(ref _sblock);
        }
        finally
        {
            _disposed = true;
        }
    }

    public void Dispose()
    {
        EraseState();

        GC.SuppressFinalize(this);
    }

    ~ARC4CryptoProvider()
    {
        EraseState();
    }
    #endregion
}

internal sealed class ARC4
{
    private bool _disposed = false;
    private ARC4CryptoProvider? _arc4;

    public void Init(byte[]? key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (key.Length == 0)
        {
            throw new ArgumentException(null, nameof(key));
        }

        _arc4 = new ARC4CryptoProvider(key);
    }

    public void UpdateData(byte[] data, int len)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ARC4));
        }

        if (_arc4 == null)
        {
            throw new NullReferenceException(nameof(_arc4));
        }

        _arc4.Cipher(data, len);
    }

    private void EraseState()
    {
        if (_disposed)
        {
            return;
        }

        _arc4?.EraseState();

        _disposed = true;
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        EraseState();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// <inheritdoc cref="object.Finalize"/>.
    /// </summary>
    ~ARC4()
    {
        EraseState();
    }
}
