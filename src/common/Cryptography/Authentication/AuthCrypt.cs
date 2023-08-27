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

public class AuthCrypt
{
    private readonly ARC4Drop1024 _clientDecrypt;
    private readonly ARC4Drop1024 _serverEncrypt;

    private static readonly byte[] SERVER_ENCRYPTION_KEY = new byte[] { 0xCC, 0x98, 0xAE, 0x04, 0xE8, 0x97, 0xEA, 0xCA, 0x12, 0xDD, 0xC0, 0x93, 0x42, 0x91, 0x53, 0x57 };
    private static readonly byte[] SERVER_DECRYPTION_KEY = new byte[] { 0xC2, 0xB3, 0x72, 0x3C, 0xC6, 0xAE, 0xD9, 0xB5, 0x34, 0x3C, 0x53, 0xEE, 0x2F, 0x43, 0x67, 0xCE };

    public bool IsInitialized { get; set; } = false;

    public AuthCrypt()
    {
        _clientDecrypt = new ARC4Drop1024();
        _serverEncrypt = new ARC4Drop1024();
    }

    public void Init(byte[] sessionKey)
    {
        _serverEncrypt.Init(new HMACSHA1(SERVER_ENCRYPTION_KEY).GetDigestOf(sessionKey));
        _clientDecrypt.Init(new HMACSHA1(SERVER_DECRYPTION_KEY).GetDigestOf(sessionKey));

        IsInitialized = true;
    }

    public void DecryptRecv(byte[] src, int offset, int len)
    {
        _clientDecrypt.UpdateData(src, offset, len);
    }

    public void EncryptSend(byte[] src, int offset, int len)
    {
        _serverEncrypt.UpdateData(src, offset, len);
    }
}
