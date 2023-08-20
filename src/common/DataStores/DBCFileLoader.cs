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
using System.Text;

using AzerothCore.Logging;

namespace AzerothCore.DataStores;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DBCFileHeader
{
    public uint Magic;              // always 'WDBC'
    public uint RecordCount;        // records per file
    public uint FieldCount;         // fields per record
    public uint RecordSize;         // sum (sizeof (field_type_i)) | 0 <= i < field_count. field_type_i is NOT defined in the files.
    public uint StringBlockSize;    // strings table
}

public enum DBCFieldFormat
{
    FT_NA = 'x',                    // not used or unknown, 4 byte size
    FT_NA_BYTE = 'X',               // not used or unknown, byte
    FT_STRING = 's',                // char*
    FT_FLOAT = 'f',                 // float
    FT_INT = 'i',                   // uint32
    FT_BYTE = 'b',                  // uint8
    FT_SORT = 'd',                  // sorted by this field, field is not included
    FT_IND = 'n',                   // the same, but parsed to data
    FT_LOGIC = 'l'                  // Logical (boolean)
}

public struct DBCFieldInfo<T>
{
    public int              FieldIdx;
    public DBCFieldFormat   FieldType;
    public T                FieldValue;
}

public abstract class DBCFileEntry
{
    public DBCFileEntry() { }

    public abstract void SetField<T>(DBCFieldInfo<T> fieldInfo);
}

public sealed class DBCFileLoader
{
    private static readonly ILogger logger = LoggerFactory.GetLogger();

    internal static bool LoadFromFile<T>(DBCStorage<T> dbcStore, string dbcFilename) where T : DBCFileEntry, new ()
    {
        try
        {
            if (!File.Exists(dbcFilename))
            {
                return false;
            }

            using var dbc = new FileStream(dbcFilename, FileMode.Open, FileAccess.Read, FileShare.Read);

            // read dbc file header
            int headerSize = Marshal.SizeOf(typeof(DBCFileHeader));
            byte[] headerBuff = new byte[headerSize];

            if (dbc.Read(headerBuff, 0, headerSize) < headerSize)
            {
                return false;
            }

            DBCFileHeader dbcFileHeader = ReinterpretCast<DBCFileHeader>(headerBuff);

            if (dbcStore.FieldFormat.Length != dbcFileHeader.FieldCount)
            {
                return false;
            }

            // read records
            Memory<byte> dataTable = new byte[dbcFileHeader.RecordSize * dbcFileHeader.RecordCount];
            dbc.Read(dataTable.Span);

            // read strings table
            byte[] stringTable = new byte[dbcFileHeader.StringBlockSize];
            dbc.Read(stringTable);

            int dataOffset = 0;

            // read fields
            for (int recordIdx = 0; recordIdx < dbcFileHeader.RecordCount; recordIdx++)
            {
                T dbcEntry = new();

                int key = int.MinValue;

                // parse record raw data into entry fields.
                for (int fieldIdx = 0; fieldIdx < dbcFileHeader.FieldCount; fieldIdx++)
                {
                    switch (dbcStore.FieldFormat[fieldIdx])
                    { 
                    case (char)DBCFieldFormat.FT_FLOAT:
                        {
                            DBCFieldInfo<float> floatField = new()
                            {
                                FieldIdx = fieldIdx,
                                FieldType = DBCFieldFormat.FT_FLOAT,
                                FieldValue = BitConverter.ToSingle(dataTable.Slice(dataOffset, sizeof(float)).Span)
                            };

                            dbcEntry.SetField(floatField);
                        }
                        dataOffset += sizeof(float);
                        break;
                    case (char)DBCFieldFormat.FT_IND:
                        {
                            DBCFieldInfo<uint> intField = new()
                            {
                                FieldIdx = fieldIdx,
                                FieldType = DBCFieldFormat.FT_IND,
                                FieldValue = BitConverter.ToUInt32(dataTable.Slice(dataOffset, sizeof(uint)).Span)
                            };

                            key = (int)intField.FieldValue;
                            dbcEntry.SetField(intField);
                        }
                        dataOffset += sizeof(uint);
                        break;
                    case (char)DBCFieldFormat.FT_INT:
                        {
                            DBCFieldInfo<uint> intField = new()
                            {
                                FieldIdx = fieldIdx,
                                FieldType = DBCFieldFormat.FT_INT,
                                FieldValue = BitConverter.ToUInt32(dataTable.Slice(dataOffset, sizeof(uint)).Span)
                            };

                            dbcEntry.SetField(intField);
                        }
                        dataOffset += sizeof(uint);
                        break;
                    case (char)DBCFieldFormat.FT_BYTE:
                        {
                            DBCFieldInfo<byte> byteField = new()
                            {
                                FieldIdx = fieldIdx,
                                FieldType = DBCFieldFormat.FT_BYTE,
                                FieldValue = dataTable.Span[dataOffset]
                            };

                            dbcEntry.SetField(byteField);
                        }
                        dataOffset += sizeof(byte);
                        break;
                    case (char)DBCFieldFormat.FT_STRING:
                        {
                            DBCFieldInfo<string> stringField = new()
                            {
                                FieldIdx = fieldIdx,
                                FieldType = DBCFieldFormat.FT_STRING,
                            };

                            uint stringTableOffset = BitConverter.ToUInt32(dataTable.Slice(dataOffset, sizeof(uint)).Span);

                            

                            if (stringTableOffset != 0 && stringTableOffset < dbcFileHeader.StringBlockSize)
                            {
                                List<byte> cstring = new();

                                // get c string.
                                while (stringTable[(int)stringTableOffset] != 0x00)
                                {
                                    cstring.Add(stringTable[(int)stringTableOffset]);
                                    stringTableOffset++;
                                }

                                stringField.FieldValue = Encoding.UTF8.GetString(cstring.ToArray());

                                dbcEntry.SetField(stringField);
                            }
                        }
                        dataOffset += sizeof(uint);
                        break;
                    case (char)DBCFieldFormat.FT_NA:
                        dataOffset += sizeof(uint);
                        break;
                    case (char)DBCFieldFormat.FT_NA_BYTE:
                        dataOffset += sizeof(byte);
                        break;
                    case (char)DBCFieldFormat.FT_SORT:
                        key = (int)BitConverter.ToUInt32(dataTable.Slice(dataOffset, sizeof(uint)).Span);
                        dataOffset += sizeof(uint);
                        break;
                    case (char)DBCFieldFormat.FT_LOGIC:
                        throw new ApplicationException("Attempted to load DBC files that does not have field types that match what is in the core. Check DbcFieldFormat or your DBC files.");
                    }
                }

                if (key != int.MinValue)
                {
                    dbcStore.Entries.Add((uint)key, dbcEntry);
                }
            }

            return true;
        }
        catch(Exception e)
        {
            logger.Fatal(LogFilter.ServerLoading, e);

            return false;
        }
    }

    internal static bool LoadStringsFromFile<T>(DBCStorage<T> dbcStore, string dbcFilename) where T : DBCFileEntry, new()
    {
        try
        {
            if (!File.Exists(dbcFilename))
            {
                return false;
            }

            using var dbc = new FileStream(dbcFilename, FileMode.Open, FileAccess.Read, FileShare.Read);

            // read dbc file header
            int headerSize = Marshal.SizeOf(typeof(DBCFileHeader));
            byte[] headerBuff = new byte[headerSize];

            if (dbc.Read(headerBuff, 0, headerSize) < headerSize)
            {
                return false;
            }

            DBCFileHeader dbcFileHeader = ReinterpretCast<DBCFileHeader>(headerBuff);

            if (dbcStore.FieldFormat.Length != dbcFileHeader.FieldCount)
            {
                return false;
            }

            // read records
            Memory<byte> dataTable = new byte[dbcFileHeader.RecordSize * dbcFileHeader.RecordCount];
            dbc.Read(dataTable.Span);

            // read strings table
            byte[] stringTable = new byte[dbcFileHeader.StringBlockSize];
            dbc.Read(stringTable);

            int dataOffset = 0;
            List<DBCFieldInfo<string>> stringsToUpdate = new();

            // read fields
            for (int recordIdx = 0; recordIdx < dbcFileHeader.RecordCount; recordIdx++)
            {
                int key = int.MinValue;

                // parse record raw data into entry fields.
                for (int fieldIdx = 0; fieldIdx < dbcFileHeader.FieldCount; fieldIdx++)
                {
                    switch (dbcStore.FieldFormat[fieldIdx])
                    {
                    case (char)DBCFieldFormat.FT_FLOAT:
                        dataOffset += sizeof(float);
                        break;
                    case (char)DBCFieldFormat.FT_IND:
                        key = (int)BitConverter.ToUInt32(dataTable.Slice(dataOffset, sizeof(uint)).Span);
                        dataOffset += sizeof(uint);
                        break;
                    case (char)DBCFieldFormat.FT_INT:
                        dataOffset += sizeof(uint);
                        break;
                    case (char)DBCFieldFormat.FT_BYTE:
                        dataOffset += sizeof(byte);
                        break;
                    case (char)DBCFieldFormat.FT_STRING:
                        {
                            DBCFieldInfo<string> stringField = new()
                            {
                                FieldIdx = fieldIdx,
                                FieldType = DBCFieldFormat.FT_STRING,
                            };

                            uint stringTableOffset = BitConverter.ToUInt32(dataTable.Slice(dataOffset, sizeof(uint)).Span);

                            if (stringTableOffset != 0 && stringTableOffset < dbcFileHeader.StringBlockSize)
                            {
                                List<byte> cstring = new();

                                // get c string.
                                while (stringTable[(int)stringTableOffset] != 0x00)
                                {
                                    cstring.Add(stringTable[(int)stringTableOffset]);
                                    stringTableOffset++;
                                }

                                stringField.FieldValue = Encoding.UTF8.GetString(cstring.ToArray());

                                stringsToUpdate.Add(stringField);
                            }
                        }
                        dataOffset += sizeof(uint);
                        break;
                    case (char)DBCFieldFormat.FT_NA:
                        dataOffset += sizeof(uint);
                        break;
                    case (char)DBCFieldFormat.FT_NA_BYTE:
                        dataOffset += sizeof(byte);
                        break;
                    case (char)DBCFieldFormat.FT_SORT:
                        key = (int)BitConverter.ToUInt32(dataTable.Slice(dataOffset, sizeof(uint)).Span);
                        dataOffset += sizeof(uint);
                        break;
                    case (char)DBCFieldFormat.FT_LOGIC:
                        throw new ApplicationException("Attempted to load DBC files that does not have field types that match what is in the core. Check DbcFieldFormat or your DBC files.");
                    }
                }

                if (key != int.MinValue)
                {
                    if (dbcStore.Entries.ContainsKey(key))
                    {
                        T entryToUpdate = (T)dbcStore.Entries[(uint)key];

                        for (int i = 0; i < stringsToUpdate.Count; i++)
                        {
                            entryToUpdate.SetField(stringsToUpdate[i]);
                        }
                    }
                }
            }

            return true;
        }
        catch (Exception e)
        {
            logger.Fatal(LogFilter.ServerLoading, e);

            return false;
        }
    }

    internal static void LoadFromDB<T>(DBCStorage<T> storage, string dbTable) where T : DBCFileEntry, new()
    {
        // TODO: game: DBCFileLoader::LoadFromDB<T>(DBCStorage<T> storage, string dbTable)
    }

    private static T ReinterpretCast<T>(byte[] buffer) where T : struct
    {
        Type objType = typeof(T);

        if (buffer.Length > 0)
        {
            IntPtr ptrObj = IntPtr.Zero;

            try
            {
                int objSize = Marshal.SizeOf(objType);

                if (objSize > 0)
                {
                    if (buffer.Length > objSize)
                    {
                        throw new Exception($"Buffer greater than needed for creation of object of type {objType}");
                    }

                    ptrObj = Marshal.AllocHGlobal(objSize);

                    if (ptrObj != IntPtr.Zero)
                    {
                        int copySize = buffer.Length <= objSize ? buffer.Length : objSize;
                        Marshal.Copy(buffer, 0, ptrObj, copySize);
                        object? obj = Marshal.PtrToStructure(ptrObj, objType);

                        return (T)(obj ?? default(T));
                    }
                    else
                    {
                        throw new Exception($"Couldn't allocate memory to create object of type {objType}");
                    }
                }
            }
            finally
            {
                if (ptrObj != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptrObj);
                }
            }
        }

        return default;
    }
}
