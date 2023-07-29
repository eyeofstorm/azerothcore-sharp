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

using System.Buffers;
using System.Runtime.InteropServices;

namespace AzerothCore.Cryptography;

public class AuthCrypt
{
    private ARC4 _clientDecrypt;
    private ARC4 _serverEncrypt;

    public bool IsInitialized { get; set; } = false;

    public AuthCrypt()
    {
        _clientDecrypt = new ARC4();
        _serverEncrypt = new ARC4();
    }

    public void Init(byte[] sessionKey)
    {
        byte[] serverEncryptionKey = new byte[] { 0xCC, 0x98, 0xAE, 0x04, 0xE8, 0x97, 0xEA, 0xCA, 0x12, 0xDD, 0xC0, 0x93, 0x42, 0x91, 0x53, 0x57 };
        _serverEncrypt.Init(new HMACSHA1(serverEncryptionKey).GetDigestOf(sessionKey));

        byte[] serverDecryptionKey = new byte[] { 0xC2, 0xB3, 0x72, 0x3C, 0xC6, 0xAE, 0xD9, 0xB5, 0x34, 0x3C, 0x53, 0xEE, 0x2F, 0x43, 0x67, 0xCE };
        _clientDecrypt.Init(new HMACSHA1(serverDecryptionKey).GetDigestOf(sessionKey));

        // Drop first 1024 bytes, as WoW uses ARC4-drop1024.
        byte[] syncBuf = new byte[1024];

        _serverEncrypt.UpdateData(syncBuf, syncBuf.Length);
        _clientDecrypt.UpdateData(syncBuf, syncBuf.Length);

        IsInitialized = true;
    }

    public void DecryptRecv(byte[] src, int offset, int len)
    {
        byte[] data = new byte[len];
        Array.Copy(src, offset, data, 0, len);

        _clientDecrypt.UpdateData(data, len);

        Memory<byte> updSrc = data;
        updSrc.CopyTo(src);
    }

    public void EncryptSend(byte[] src, int offset, int len)
    {
        byte[] data = new byte[len];
        Array.Copy(src, offset, data, 0, len);

        _serverEncrypt.UpdateData(data, len);

        Memory<byte> updSrc = data;
        updSrc.CopyTo(src);
    }
}
