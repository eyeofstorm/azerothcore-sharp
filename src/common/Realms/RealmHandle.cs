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

namespace AzerothCore.Realm;

public struct RealmHandle : IEquatable<RealmHandle>
{   
    public uint Index   { get; set; }

    public RealmHandle(uint index)
    {
        Index = index;
    }

    public override bool Equals(object? obj)
    {
        return obj != null && obj is RealmHandle && Equals((RealmHandle)obj);
    }

    public bool Equals(RealmHandle other)
    {
        return other.Index == Index;
    }

    public override int GetHashCode()
    {
        return new { Index }.GetHashCode();
    }
}
