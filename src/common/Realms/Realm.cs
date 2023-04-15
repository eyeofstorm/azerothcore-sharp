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

using System.Net;
using System.Net.Sockets;

using AzerothCore.Constants;
using AzerothCore.Utilities;

namespace AzerothCore.Realms;

[Flags]
public enum RealmFlags : byte
{
    REALM_FLAG_NONE             = 0x00,
    REALM_FLAG_VERSION_MISMATCH = 0x01,
    REALM_FLAG_OFFLINE          = 0x02,
    REALM_FLAG_SPECIFYBUILD     = 0x04,
    REALM_FLAG_UNK1             = 0x08,
    REALM_FLAG_UNK2             = 0x10,
    REALM_FLAG_RECOMMENDED      = 0x20,
    REALM_FLAG_NEW              = 0x40,
    REALM_FLAG_FULL             = 0x80
}

public enum RealmType
{
    REALM_TYPE_NORMAL           = 0,
    REALM_TYPE_PVP              = 1,
    REALM_TYPE_NORMAL2          = 4,
    REALM_TYPE_RP               = 6,
    REALM_TYPE_RPPVP            = 8,

    MAX_CLIENT_REALM_TYPE       = 14,

    REALM_TYPE_FFA_PVP          = 16    // custom, free for all pvp mode like arena PvP in all zones except rest activated places and sanctuaries
                                        // replaced by REALM_PVP in realm list
}

public class Realm : IEquatable<Realm>
{
    public RealmHandle  Id;
    public uint         Build;
    public IPAddress    ExternalAddress = IPAddress.None;
    public IPAddress    LocalAddress = IPAddress.None;
    public IPAddress    LocalSubnetMask = IPAddress.None;
    public ushort       Port;
    public string       Name = string.Empty;
    public string       NormalizedName = string.Empty;
    public byte         Type;
    public RealmFlags   Flags;
    public byte         Timezone;
    public AccountTypes AllowedSecurityLevel;
    public float        PopulationLevel;

    public void SetName(string name)
    {
        Name            = name;
        NormalizedName  = name;
        NormalizedName  = NormalizedName.Replace(" ", "");
    }

    public IPEndPoint GetAddressForClient(IPAddress? clientAddr)
    {
        if (clientAddr == null)
        {
            throw new NullReferenceException(nameof(clientAddr));
        }

        IPAddress realmIp;

        // Attempt to send best address for client
        if (IPAddress.IsLoopback(clientAddr))
        {
            // Try guessing if realm is also connected locally
            if (IPAddress.IsLoopback(LocalAddress) || IPAddress.IsLoopback(ExternalAddress))
            {
                realmIp = clientAddr;
            }
            else
            {
                // Assume that user connecting from the machine that authserver is located on
                // has all realms available in his local network
                realmIp = LocalAddress;
            }
        }
        else
        {
            if (clientAddr.AddressFamily == AddressFamily.InterNetwork &&
                clientAddr.GetNetworkAddress(LocalSubnetMask).Equals(LocalAddress.GetNetworkAddress(LocalSubnetMask)))
            {
                realmIp = LocalAddress;
            }
            else
            {
                realmIp = ExternalAddress;
            }
        }

        IPEndPoint endpoint = new(realmIp, Port);

        // Return external IP
        return endpoint;
    }

    public uint GetConfigId()
    {
        return ConfigIdByType[Type];
    }

    private uint[] ConfigIdByType =
    {
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14
    };

    public override bool Equals(object? obj)
    {
        return obj != null && obj is Realm && Equals((Realm)obj);
    }

    public bool Equals(Realm? other)
    {
        if (other == null)
        {
            return false;
        }

        return other.ExternalAddress.Equals(ExternalAddress)
            && other.LocalAddress.Equals(LocalAddress)
            && other.LocalSubnetMask.Equals(LocalSubnetMask)
            && other.Port == Port
            && other.Name == Name
            && other.Type == Type
            && other.Flags == Flags
            && other.Timezone == Timezone
            && other.AllowedSecurityLevel == AllowedSecurityLevel
            && other.PopulationLevel == PopulationLevel;
    }

    public override int GetHashCode()
    {
        return new { ExternalAddress, LocalAddress, LocalSubnetMask, Port, Name, Type, Flags, Timezone, AllowedSecurityLevel, PopulationLevel }.GetHashCode();
    }
}
