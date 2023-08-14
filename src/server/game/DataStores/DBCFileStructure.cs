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

namespace AzerothCore.DataStores;

[Flags]
public enum ChrRacesFlags : byte
{
    CHRRACES_FLAGS_NOT_PLAYABLE = 0x01,
    CHRRACES_FLAGS_BARE_FEET    = 0x02,
    CHRRACES_FLAGS_CAN_MOUNT    = 0x04
}

public class ChrRacesEntry : DBCFileEntry
{
    public uint RaceID              { get; set; }                           // 0
    public uint Flags               { get; set; }                           // 1
    public uint FactionID           { get; set; }                           // 2 facton template id
                                                                            // 3 unused
    public uint ModelMale           { get; set; }                           // 4
    public uint ModelFemale         { get; set; }                           // 5
                                                                            // 6 unused
    public uint TeamID              { get; set; }                           // 7 (7-Alliance 1-Horde)
                                                                            // 8-11 unused
    public uint CinematicSequence   { get; set; }                           // 12 id from CinematicSequences.dbc
                                                                            // 13 faction (0 alliance, 1 horde, 2 not available?)
    public List<string> Name        { get; set; }                           // 14-29 used for DBC language detection/selection
                                                                            // 30 string flags, unused
                                                                            // 31-46, if different from base (male) case
                                                                            // 47 string flags, unused
                                                                            // 48-63, if different from base (male) case
                                                                            // 64 string flags, unused
                                                                            // 65-67 unused
    public uint Expansion;                                                  // 68 (0 - original race, 1 - tbc addon, ...)


    public ChrRacesEntry()
    {
        Name = new List<string>();

        for (int i = 0; i < 16; i++)
        {
            Name.Add(string.Empty);
        }
    }

    /// <summary>
    /// Parse to field
    /// </summary>
    /// <typeparam name="T">type of field</typeparam>
    /// <param name="fieldInfo">field information</param>
    public override void ParseToField<T>(DBCFieldInfo<T> fieldInfo)
    {
        if (fieldInfo.FieldValue is uint uint32Value)
        {
            switch (fieldInfo.FieldIdx)
            {
            case 0:
                RaceID = uint32Value;
                break;
            case 1:
                Flags = uint32Value;
                break;
            case 2:
                FactionID = uint32Value;
                break;
            case 4:
                ModelMale = uint32Value;
                break;
            case 5:
                ModelFemale = uint32Value;
                break;
            case 7:
                TeamID = uint32Value;
                break;
            case 12:
                CinematicSequence = uint32Value;
                break;
            default:
                break;
            }
        }
        else if (fieldInfo.FieldValue is string stringValue)
        {
            if (fieldInfo.FieldIdx >= 14 && fieldInfo.FieldIdx <= 29)
            {
                Name[fieldInfo.FieldIdx - 14] = stringValue;
            }
        }
    }

    public bool HasFlag(ChrRacesFlags flag)
    {
        return (Flags & (byte)flag) != 0x00;
    }
}
