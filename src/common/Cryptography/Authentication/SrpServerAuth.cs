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

using System.Security.Cryptography;
using System.Text;

namespace AzerothCore.Cryptography;

public class SrpServerAuth
{
	public static readonly int			SALT_LENGTH = 32;
	public static readonly int			VERIFIER_LENGTH = 32;
	public static readonly int			EPHEMERAL_KEY_LENGTH = 32;
	public static readonly int			SHA1_DIGEST_LENGTH_BYTES = 20;
	public static readonly int			SESSION_KEY_LENGTH = 40;

	private static readonly SHA1		s_sha1;

	public static readonly SrpInteger	Generator;
	public static readonly SrpInteger	SafePrime;

	public SrpInteger					ServerPublicKey { get; set; }
	public SrpInteger					Salt            { get; private set; }

	private readonly byte[]				m_accountNameHash;
	private readonly SrpInteger			m_verifier;
	private readonly SrpInteger			m_serverPrivateKey;

	static SrpServerAuth()
    {
		s_sha1 = SHA1.Create();

		Generator = 7;

		SafePrime = SrpInteger.FromHex(@"894B645E89E1535BBDAD5B8B290650530801B18EBFBF5E8FAB3C82872A3E9BB7");
	}

	public SrpServerAuth(string accountName, byte[] salt, byte[] verifier)
	{
		m_accountNameHash = s_sha1.ComputeHash(Encoding.UTF8.GetBytes(accountName));

		Salt = SrpInteger.FromByteArray(salt.Reverse().ToArray());

		m_verifier = SrpInteger.FromByteArray(verifier.Reverse().ToArray());

		m_serverPrivateKey = 2;

		ServerPublicKey = SrpServerAuth.ComputeB(m_verifier, m_serverPrivateKey);
	}

	public static byte[] ComputeM2(byte[] clientPublicKey, byte[] clientM1, byte[] sessionKey)
	{
		var buff = new byte[(clientPublicKey.Length + clientM1.Length + sessionKey.Length)];

		Buffer.BlockCopy(clientPublicKey, 0, buff, 0, clientPublicKey.Length);
		Buffer.BlockCopy(clientM1, 0, buff, clientPublicKey.Length, clientM1.Length);
		Buffer.BlockCopy(sessionKey, 0, buff, clientPublicKey.Length + clientM1.Length, sessionKey.Length);

		return s_sha1.ComputeHash(buff);
	}

	/// <summary>
	/// verify client proof
	/// </summary>
	/// <param name="clientPublicKey">client public key</param>
	/// <param name="clientM1">client m1</param>
	/// <returns>session key</returns>
	public byte[]? VerifyChallengeResponse(byte[] clientPublicKey, byte[] clientM)
    {
        SrpInteger A = SrpInteger.FromByteArray(clientPublicKey.Reverse().ToArray());
        SrpInteger M = SrpInteger.FromByteArray(clientM.Reverse().ToArray());

        // calculate server side M
        (SrpInteger? serverM, byte[]? K) = ComputeM(A);

		// compare to server side M
		if (M == serverM)
		{
			return K;
		}
		else
		{
			return null;
		}
    }

	/// <summary>
	/// calculate temprory public key. <br/>
	/// B = (g^b % N + k * v) % N <br/>
	/// k: constant number 3  <br/>
	/// v: verifier which fetch from db <br/> 
	/// b: private key which is a random number <br/> 
	/// </summary>
	/// <param name="verifier">verifier</param>
	/// <param name="privateKey">private key</param>
	/// <returns>temprory public key</returns>
	private static SrpInteger ComputeB(SrpInteger verifier, SrpInteger privateKey)
	{
		SrpInteger g = SrpServerAuth.Generator;
		SrpInteger N = SrpServerAuth.SafePrime;
		SrpInteger b = privateKey;
		SrpInteger v = verifier;
		SrpInteger k = 3;

		// B = (g^b % N + k * v) % N
		SrpInteger publicKey = (g.ModPow(b, N) + (k * v)) % N;

		return publicKey;
	}

	/// <summary>
	/// 
	/// Calculate server side M <br/>
	/// 
	/// M = sha(sha(N) xor sha(g), sha(I), s, A, B, K) <br/>
	/// u = sha(A, B) <br/>
	/// S = (A * (v.ModExp(u, N))).ModExp(b, N) <br/>
	/// K = H(S) <br/>
	/// 
	/// A: client temprory public key, calculate at client <br/>
	/// B: server temprory public key, calculate at server <br/>
	/// I: user account name <br/>
	/// v: user verifier. created at user acount creation <br/>
	/// s: user salt. created at user acount creation <br/>
	/// g: constant value, a generator <br/>
	/// N: constant value, a safe prime. <br/>
	/// b: server private temprory key, a random number <br/>
	/// 
	/// </summary>
	/// 
	/// <param name="clientPublicKey"></param>
	/// <returns>server side M</returns>
	private (SrpInteger? M, byte[]? K) ComputeM(SrpInteger clientPublicKey)
	{
		SrpInteger? serverM;
		byte[]? K;

		if ((clientPublicKey % SafePrime) == SrpInteger.Zero)
		{
			serverM = null;
			K = null;

			return (serverM, K);
		}

		// u = sha(A, B)
		SrpInteger u = ComputeU(clientPublicKey, ServerPublicKey);

		// S = (A * (v.ModExp(u, N))).ModExp(b, N)
		SrpInteger S = ComputeS(clientPublicKey, m_serverPrivateKey, u, m_verifier);

		// K = H(S)
		K = ComputeK(S);

		// M = sha(sha(N) xor sha(g), sha(I), s, A, B, K)
		byte[] ngXorHash = new byte[SHA1_DIGEST_LENGTH_BYTES];
        byte[] nHash = s_sha1.ComputeHash(SrpServerAuth.SafePrime.ToByteArray().Reverse().ToArray());
        byte[] gHash = s_sha1.ComputeHash(Generator.ToByteArray().Reverse().ToArray());

        for (var i = 0; i < SHA1_DIGEST_LENGTH_BYTES; i++)
        {
			ngXorHash[i] = (byte)(nHash[i] ^ gHash[i]);
        }

		serverM = ComputeHash(
						ngXorHash,
						m_accountNameHash,
						Salt,
						clientPublicKey,
						ServerPublicKey,
						K);

		// return value
		return (serverM, K);
	}

	private static byte[] ComputeK(SrpInteger S)
    {
		byte[] bufferS = S.ToByteArray().Reverse().ToArray();

        // split S into two buffers
        byte[] bufOdd = new byte[EPHEMERAL_KEY_LENGTH / 2];
        byte[] bufEven = new byte[EPHEMERAL_KEY_LENGTH / 2];

        for (int i = 0; i < EPHEMERAL_KEY_LENGTH / 2; i++)
        {
            bufOdd[i]  = bufferS[2 * i + 0];
            bufEven[i] = bufferS[2 * i + 1];
        }

        // find position of first nonzero byte
        int p = 0;

        while (p < EPHEMERAL_KEY_LENGTH && bufferS[p] == 0)
        {
            p++;
        }

        if ((p & 1) == 1)
        {
            p++;    // skip one extra byte if p is odd
        }

        p /= 2; // offset into buffers

        // hash each of the halves, starting at the first nonzero byte
        byte[] hashDataOdd = new byte[EPHEMERAL_KEY_LENGTH / 2 - p];
        Array.Copy(bufOdd, p, hashDataOdd, 0, hashDataOdd.Length);
        byte[] hashOdd = s_sha1.ComputeHash(hashDataOdd);

        byte[] hashDataEven = new byte[EPHEMERAL_KEY_LENGTH / 2 - p];
        Array.Copy(bufEven, p, hashDataEven, 0, hashDataEven.Length);
        byte[] hashEven = s_sha1.ComputeHash(hashDataEven);

        // stick the two hashes back together
        byte[] K = new byte[SESSION_KEY_LENGTH];

        for (int i = 0; i < SHA1_DIGEST_LENGTH_BYTES; ++i)
        {
            K[2 * i + 0] = hashOdd[i];
            K[2 * i + 1] = hashEven[i];
        }

        return K;
    }

	private static SrpInteger ComputeU(SrpInteger clientPublicKey, SrpInteger serverPublicKey)
    {
		// u = sha(A, B)
		SrpInteger u = ComputeHash(clientPublicKey, serverPublicKey);

		return u;
	}

	private static SrpInteger ComputeS(SrpInteger A, SrpInteger b, SrpInteger u, SrpInteger v)
    {
		SrpInteger N = SrpServerAuth.SafePrime;

		return (A * v.ModPow(u, N)).ModPow(b, N);
	}

	// ==================== Helper methods =================== //


	private static SrpInteger ComputeHash(byte[] data)
	{
        byte[]? hash = s_sha1.ComputeHash(data);

		return SrpInteger.FromByteArray(hash.Reverse().ToArray());
	}

	private static byte[] GetBytes(object obj)
	{
		if (obj == null)
		{
			return Array.Empty<byte>();
		}

		var str = obj as string;

		if (!string.IsNullOrEmpty(str))
		{
			return Encoding.UTF8.GetBytes(str);
		}

		var srpInt = obj as SrpInteger;

		if (srpInt != null)
		{
			return srpInt.ToByteArray().Reverse().ToArray();
		}

		if (obj is byte[] byteArr)
		{
			return byteArr;
        }

		return Array.Empty<byte>();
	}

    /// <summary>
    /// Computes the hash of the specified <see cref="string"/> or <see cref="SrpInteger"/> values.
    /// </summary>
    /// <param name="values">The values.</param>
    public static SrpInteger ComputeHash(params object[] values)
    {
        return ComputeHash(Combine(values.Select(v => GetBytes(v))));
    }

    private static byte[] Combine(IEnumerable<byte[]> arrays)
	{
		var rv = new byte[arrays.Sum(a => a.Length)];
		var offset = 0;

		foreach (var array in arrays)
		{
			Buffer.BlockCopy(array, 0, rv, offset, array.Length);
			offset += array.Length;
		}

		return rv;
	}
}
