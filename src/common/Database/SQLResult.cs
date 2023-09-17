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

using System.Runtime.CompilerServices;
using MySqlConnector;

namespace AzerothCore.Database;

public class QueryResult
{
    MySqlDataReader? _reader;

    public QueryResult() { }

    public QueryResult(MySqlDataReader reader)
    {
        _reader = reader;

        NextRow();
    }

    ~QueryResult()
    {
        _reader = null;
    }

    public T? Read<T>(int column)
    {
        if (_reader == null)
            return default;

        if (_reader.IsDBNull(column))
            return default;

        var columnType = _reader.GetFieldType(column);

        if (columnType == typeof(T))
            return _reader.GetFieldValue<T>(column);

        switch (Type.GetTypeCode(columnType))
        {
            case TypeCode.SByte:
            {
                var value = _reader.GetSByte(column);
                return Unsafe.As<sbyte, T>(ref value);
            }
            case TypeCode.Byte:
            {
                var value = _reader.GetByte(column);
                return Unsafe.As<byte, T>(ref value);
            }
            case TypeCode.Int16:
            {
                var value = _reader.GetInt16(column);
                return Unsafe.As<short, T>(ref value);
            }
            case TypeCode.UInt16:
            {
                var value = _reader.GetUInt16(column);
                return Unsafe.As<ushort, T>(ref value);
            }
            case TypeCode.Int32:
            {
                var value = _reader.GetInt32(column);
                return Unsafe.As<int, T>(ref value);
            }
            case TypeCode.UInt32:
            {
                var value = _reader.GetUInt32(column);
                return Unsafe.As<uint, T>(ref value);
            }
            case TypeCode.Int64:
            {
                var value = _reader.GetInt64(column);
                return Unsafe.As<long, T>(ref value);
            }
            case TypeCode.UInt64:
            {
                var value = _reader.GetUInt64(column);
                return Unsafe.As<ulong, T>(ref value);
            }
            case TypeCode.Single:
            {
                var value = _reader.GetFloat(column);
                return Unsafe.As<float, T>(ref value);
            }
            case TypeCode.Double:
            {
                var value = _reader.GetDouble(column);
                return Unsafe.As<double, T>(ref value);
            }
        }

        return default;
    }

    public byte[]? ReadBytes(int column, int size)
    {
        if (_reader == null)
        {
            return null;
        }

        if (_reader.IsDBNull(column))
        {
            return null;
        }

        // The BLOB byte[] buffer to be filled by GetBytes.  
        byte[] outByte = new byte[size];

        // Read bytes into outByte[] and retain the number of bytes returned.  
        _reader.GetBytes(column, 0, outByte, 0, size);

        return outByte;
    }

    public T?[] ReadValues<T>(int startIndex, int numColumns)
    {
        T?[] values = new T[numColumns];

        for (var c = 0; c < numColumns; ++c)
        {
            values[c] = Read<T>(startIndex + c);
        }

        return values;
    }

    public bool IsNull(int column)
    {
        if (_reader == null)
            return true;

        return _reader.IsDBNull(column);
    }

    public int GetFieldCount()
    {
        if (_reader == null)
            return 0;

        return _reader.FieldCount;
    }

    public bool IsEmpty()
    {
        if (_reader == null)
            return true;
        
        return _reader.IsClosed || !_reader.HasRows;
    }

    public Fields Fetch()
    {
        if (_reader == null)
        {
            throw new NullReferenceException(nameof(_reader));
        }

        object[] values = new object[_reader.FieldCount];

        _reader.GetValues(values);

        Fields fields = new ();

        foreach (var val in values)
        {
            fields.Add(new Field(val));
        }

        return fields;
    }

    public bool NextRow()
    {
        if (_reader == null)
            return false;

        if (_reader.Read())
            return true;

        _reader.Close();

        return false;
    }
}

public class Field
{
    readonly object _currentCol;

    public Field(object col) { _currentCol = col; }

    public T? Get<T>()
    {
        if (_currentCol == DBNull.Value)
        {
            return default;
        }

        if (_currentCol.GetType() is not T)
        {
            // TODO: common: Remove me when all fields are the right type  this is super slow
            return (T)Convert.ChangeType(_currentCol, typeof(T));
        }

        return (T)_currentCol;
    }

    public bool IsNull()
    {
        return _currentCol == DBNull.Value;
    }
}

public class Fields : List<Field>
{
    public T?[] ReadValues<T>(int startIndex, int numColumns)
    {
        T?[] values = new T[numColumns];

        for (var c = 0; c < numColumns; ++c)
        {
            values[c] = this.ElementAt(startIndex + c).Get<T>();
        }

        return values;
    }
}
