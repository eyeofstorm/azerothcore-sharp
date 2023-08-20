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
using AzerothCore.DataStores;

namespace AzerothCore.Game;

public static class CreatureDataConst
{
    public static readonly int MAX_AGGRO_RESET_TIME = 10; // in seconds

    public static readonly int MAX_KILL_CREDIT = 2;
    public static readonly int CREATURE_REGEN_INTERVAL = 2 * SharedConst.IN_MILLISECONDS;

    public static readonly int MAX_CREATURE_QUEST_ITEMS = 6;

    public static readonly int MAX_EQUIPMENT_ITEMS = 3;
}

public struct CreatureTemplate
{
    public uint Entry;
    public uint[] DifficultyEntry;
    public uint[] KillCredit;
    public uint Modelid1;
    public uint Modelid2;
    public uint Modelid3;
    public uint Modelid4;
    public string Name;
    public string SubName;
    public string IconName;
    public uint GossipMenuId;
    public byte MinLevel;
    public byte MaxLevel;
    public uint Expansion;
    public uint Faction;
    public uint NPCFlag;
    public float SpeedWalk;
    public float SpeedRun;
    public float SpeedSwim;
    public float SpeedFlight;
    public float DetectionRange;                                // Detection Range for Line of Sight aggro
    public float Scale;
    public uint Rank;
    public uint DmgSchool;
    public float DamageModifier;
    public uint BaseAttackTime;
    public uint RangeAttackTime;
    public float BaseVariance;
    public float RangeVariance;
    public uint UnitClass;                                     // enum Classes. Note only 4 classes are known for creatures.
    public uint UnitFlags;                                     // enum UnitFlags mask values
    public uint UnitFlags2;                                    // enum UnitFlags2 mask values
    public uint DynamicFlags;
    public uint Family;                                         // enum CreatureFamily values (optional)
    public uint TrainerType;
    public uint TrainerSpell;
    public uint TrainerClass;
    public uint TrainerRace;
    public uint Type;                                           // enum CreatureType values
    public uint TypeFlags;                                     // enum CreatureTypeFlags mask values
    public uint LootId;
    public uint pickpocketLootId;
    public uint SkinLootId;
    public int[] Resistance;
    public uint[] Spells;
    public uint PetSpellDataId;
    public uint VehicleId;
    public uint MinGold;
    public uint MaxGold;
    public string? AIName;
    public uint MovementType;
    public CreatureMovementData Movement;
    public float HoverHeight;
    public float ModHealth;
    public float ModMana;
    public float ModArmor;
    public float ModExperience;
    public bool RacialLeader;
    public uint MovementId;
    public bool RegenHealth;
    public uint MechanicImmuneMask;
    public byte SpellSchoolImmuneMask;
    public uint FlagsExtra;
    public uint ScriptID;

    public WorldPacketData? queryData;

    public CreatureTemplate()
    {
        Name = string.Empty;
        SubName = string.Empty;
        IconName = string.Empty;

        DifficultyEntry = new uint[DBCConst.MAX_DIFFICULTY - 1];
        KillCredit = new uint[CreatureDataConst.MAX_KILL_CREDIT];

        Resistance = new int[SharedConst.MAX_SPELL_SCHOOL];
        Spells = new uint[UnitConst.MAX_CREATURE_SPELLS];
    }
}

public enum CreatureGroundMovementType : byte
{
    None,
    Run,
    Hover,

    Max
};

public enum CreatureFlightMovementType : byte
{
    None,
    DisableGravity,
    CanFly,

    Max
};

public enum CreatureChaseMovementType : byte
{
    Run,
    CanWalk,
    AlwaysWalk,

    Max
};

public enum CreatureRandomMovementType : byte
{
    Walk,
    CanRun,
    AlwaysRun,

    Max
}

public struct CreatureMovementData
{
    public CreatureGroundMovementType Ground;
    public CreatureFlightMovementType Flight;
    public bool Swim;
    public bool Rooted;
    public CreatureChaseMovementType Chase;
    public CreatureRandomMovementType Random;
    public uint InteractionPauseTimer;
}
