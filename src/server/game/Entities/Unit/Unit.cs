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

using AzerothCore.Constants;

namespace AzerothCore.Game;

public enum UnitMoveType : uint
{
    MOVE_WALK = 0,
    MOVE_RUN = 1,
    MOVE_RUN_BACK = 2,
    MOVE_SWIM = 3,
    MOVE_SWIM_BACK = 4,
    MOVE_TURN_RATE = 5,
    MOVE_FLIGHT = 6,
    MOVE_FLIGHT_BACK = 7,
    MOVE_PITCH_RATE = 8
};

public static class UnitConst
{
    public static readonly uint MAX_CREATURE_SPELLS = 8;
    public static readonly uint InfinityCooldownDelay = 0x9A7EC800;         // used for set "infinity cooldowns" for spells and check, MONTH*IN_MILLISECONDS
    public static readonly uint InfinityCooldownDelayCheck = 0x4D3F6400;    // MONTH*IN_MILLISECONDS/2;
    public static readonly uint MAX_MOVE_TYPE = 9;

    public static readonly float[] BaseMoveSpeed = new float[]
    {
        2.5f,                  // MOVE_WALK
        7.0f,                  // MOVE_RUN
        4.5f,                  // MOVE_RUN_BACK
        4.722222f,             // MOVE_SWIM
        2.5f,                  // MOVE_SWIM_BACK
        3.141594f,             // MOVE_TURN_RATE
        7.0f,                  // MOVE_FLIGHT
        4.5f,                  // MOVE_FLIGHT_BACK
        3.14f                  // MOVE_PITCH_RATE
    };

    public static readonly float[] PlayerBaseMoveSpeed = new float[]
    {
        2.5f,                  // MOVE_WALK
        7.0f,                  // MOVE_RUN
        4.5f,                  // MOVE_RUN_BACK
        4.722222f,             // MOVE_SWIM
        2.5f,                  // MOVE_SWIM_BACK
        3.141594f,             // MOVE_TURN_RATE
        7.0f,                  // MOVE_FLIGHT
        4.5f,                  // MOVE_FLIGHT_BACK
        3.14f                  // MOVE_PITCH_RATE
    };
}

public class Unit : WorldObject
{
    protected byte _realRace;
    protected byte _race;

    public Unit()
    {
    }

    public byte GetRace(bool original = false)
    {
        if (GetTypeId() == TypeID.TYPEID_PLAYER)
        {
            if (original)
            {
                return _realRace;
            }
            else
            {
                return _race;
            }
        }

        return GetByteValue((ushort)EUnitFields.UNIT_FIELD_BYTES_0, 0);
    }

    public byte GetClass()
    {
        return GetByteValue((ushort)EUnitFields.UNIT_FIELD_BYTES_0, 1);
    }

    public byte GetGender()
    {
        return GetByteValue((ushort)EUnitFields.UNIT_FIELD_BYTES_0, 2);
    }

    public void SetDisplayId(ushort modelId)
    {
        SetUInt32Value((ushort)EUnitFields.UNIT_FIELD_DISPLAYID, modelId);

        // TODO: game: Unit::SetDisplayId(ushort modelId)

        // Set Gender by modelId. 
        //CreatureModelInfo? minfo = Global.sObjectMgr.GetCreatureModelInfo(modelId);

        //if (minfo != null)
        //{
        //    SetByteValue((ushort)EUnitFields.UNIT_FIELD_BYTES_0, 2, minfo.gender);
        //}
    }

    public void SetNativeDisplayId(ushort modelId)
    {
        // TODO: game: Unit::SetNativeDisplayId(ushort modelId)
    }
}
