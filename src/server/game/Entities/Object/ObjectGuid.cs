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

using System.Text;

using AzerothCore.Constants;
using AzerothCore.Utilities;

namespace AzerothCore.Game;

public static class ByteBufferExtension
{
    public static void WriteObjectGuid(this ByteBuffer buffer, ObjectGuid guid)
    {
        buffer.WriteUInt64(guid.GetRawValue());
    }
}

public class ObjectGuid
{
    public static readonly ObjectGuid Empty = new();

    private ulong _guid;

	private ObjectGuid()
	{
        _guid = 0;
	}

    private ObjectGuid(HighGuid hi, uint counter)
    {
        _guid = counter != 0 ? counter | ((ulong)hi << 48) : 0;
    }

    private ObjectGuid(HighGuid hi, uint entry, uint counter)
    {
        _guid = counter != 0 ? counter | (ulong)(entry << 24) | ((ulong)hi << 48) : 0;
    }

    // Global guid
    public static ObjectGuid Create(HighGuid type, uint counter)
    {
        return new ObjectGuid(type, counter);
    }

    // Map specific guid
    public static ObjectGuid Create(HighGuid type, uint entry, uint counter)
    {
        return new ObjectGuid(type, entry, counter);
    }

    public string GetTypeName()
    {
        return GetTypeName(GetHigh());
    }

    public static string GetTypeName(HighGuid high)
    {
        return high switch
        {
            HighGuid.Item => "Item",
            HighGuid.Player => "Player",
            HighGuid.GameObject => "Gameobject",
            HighGuid.Transport => "Transport",
            HighGuid.Unit => "Creature",
            HighGuid.Pet => "Pet",
            HighGuid.Vehicle => "Vehicle",
            HighGuid.DynamicObject => "DynObject",
            HighGuid.Corpse => "Corpse",
            HighGuid.Mo_Transport => "MoTransport",
            HighGuid.Instance => "InstanceID",
            HighGuid.Group => "Group",
            _ => "<unknown>",
        };
    }

    public ulong GetRawValue()
    {
        return _guid;
    }

    public HighGuid GetHigh()
    {
        return (HighGuid)((_guid >> 48) & 0x0000FFFFU);
    }

    public uint GetEntry()
    {
        return HasEntry() ? (uint)((_guid >> 24) & 0x0000000000FFFFFFUL) : 0;
    }

    public TypeID GetTypeId() 
    {
        return GetTypeId(GetHigh());
    }

    private static TypeID GetTypeId(HighGuid high)
    {
        return high switch
        {
            HighGuid.Item => TypeID.TYPEID_ITEM,
            HighGuid.Unit => TypeID.TYPEID_UNIT,
            HighGuid.Pet => TypeID.TYPEID_UNIT,
            HighGuid.Player => TypeID.TYPEID_PLAYER,
            HighGuid.GameObject => TypeID.TYPEID_GAMEOBJECT,
            HighGuid.DynamicObject => TypeID.TYPEID_DYNAMICOBJECT,
            HighGuid.Corpse => TypeID.TYPEID_CORPSE,
            HighGuid.Mo_Transport => TypeID.TYPEID_GAMEOBJECT,
            HighGuid.Vehicle => TypeID.TYPEID_UNIT,
            HighGuid.Transport => TypeID.TYPEID_OBJECT,
            HighGuid.Instance => TypeID.TYPEID_OBJECT,
            HighGuid.Group => TypeID.TYPEID_OBJECT,
            _ => TypeID.TYPEID_OBJECT,
        };
    }

    public uint GetCounter()
    {
        return HasEntry() ? (uint)(_guid & 0x0000000000FFFFFFUL) : (uint)(_guid & 0x00000000FFFFFFFFUL);
    }

    public uint GetMaxCounter()
    {
        return GetMaxCounter(GetHigh());
    }

    private static uint GetMaxCounter(HighGuid high)
    {
        return HasEntry(high) ? 0x00FFFFFFU : 0xFFFFFFFFU;
    }

    public bool IsEmpty()              { return _guid == 0; }
    public bool IsCreature()           { return GetHigh() == HighGuid.Unit; }
    public bool IsPet()                { return GetHigh() == HighGuid.Pet; }
    public bool IsVehicle()            { return GetHigh() == HighGuid.Vehicle; }
    public bool IsCreatureOrPet()      { return IsCreature() || IsPet(); }
    public bool IsCreatureOrVehicle()  { return IsCreature() || IsVehicle(); }
    public bool IsAnyTypeCreature()    { return IsCreature() || IsPet() || IsVehicle(); }
    public bool IsPlayer()             { return !IsEmpty() && GetHigh() == HighGuid.Player; }
    public bool IsUnit()               { return IsAnyTypeCreature() || IsPlayer(); }
    public bool IsItem()               { return GetHigh() == HighGuid.Item; }
    public bool IsGameObject()         { return GetHigh() == HighGuid.GameObject; }
    public bool IsDynamicObject()      { return GetHigh() == HighGuid.DynamicObject; }
    public bool IsCorpse()             { return GetHigh() == HighGuid.Corpse; }
    public bool IsTransport()          { return GetHigh() == HighGuid.Transport; }
    public bool IsMOTransport()        { return GetHigh() == HighGuid.Mo_Transport; }
    public bool IsAnyTypeGameObject()  { return IsGameObject() || IsTransport() || IsMOTransport(); }
    public bool IsInstance()           { return GetHigh() == HighGuid.Instance; }
    public bool IsGroup()              { return GetHigh() == HighGuid.Group; }

    private bool HasEntry()
    {
        return HasEntry(GetHigh());
    }

    private static bool HasEntry(HighGuid high)
    {
        return high switch
        {
            HighGuid.Item or HighGuid.Player or HighGuid.DynamicObject or HighGuid.Corpse or HighGuid.Mo_Transport or HighGuid.Instance or HighGuid.Group => false,
            _ => true,
        };
    }

    public override string? ToString()
    {
        StringBuilder str = new();

        str.Append($"GUID Full: 0x{_guid:x} Type: {GetTypeName()}");

        if (HasEntry())
        {
            str.Append((IsPet() ? " Pet number: " : " Entry: ")).Append(GetEntry());
        }

        str.Append($" Low: {GetCounter()}");

        return str.ToString();
    }
}
