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
using AzerothCore.Logging;

namespace AzerothCore.Game;

public struct ObjectValue
{
    private byte[] _value;

    internal int Int32Value
    {
        readonly get => BitConverter.ToInt32(_value);
        set => _value = BitConverter.GetBytes(value);
    }

    internal uint UInt32Value
    {
        readonly get => BitConverter.ToUInt32(_value);
        set => _value = BitConverter.GetBytes(value);
    }

    internal float FloatValue
    {
        readonly get => BitConverter.ToSingle(_value);
        set => _value = BitConverter.GetBytes(value);
    }

    internal ObjectValue(int intValue)
    {
        _value = BitConverter.GetBytes(intValue);
    }

    internal ObjectValue(uint uintValue)
    {
        _value = BitConverter.GetBytes(uintValue);
    }

    internal ObjectValue(float floatValue)
    {
        _value = BitConverter.GetBytes(floatValue);
    }
}

public abstract class BaseObject
{
    protected static readonly ILogger logger = LoggerFactory.GetLogger();

    protected List<ObjectValue>? _values;
    protected ushort _valuesCount;
    protected ushort _objectType;
    protected TypeID _objectTypeId;
    protected bool _objectUpdated;
    protected PackedGuid _packGUID;
    protected UpdateMask _changesMask;
    protected bool _inWorld;

    protected BaseObject()
    {
        _values = null;
        _valuesCount = 0;
        _changesMask = new UpdateMask();
        _inWorld = false;
        _objectUpdated = false;
        _objectTypeId = TypeID.TYPEID_OBJECT;
        _objectType = (ushort)TypeMask.TYPEMASK_OBJECT;
        _packGUID = new PackedGuid();
    }

    protected void Create(uint guidlow, uint entry, HighGuid guidhigh)
    {
        if (_values == null)
        {
            InitValues();
        }

        ObjectGuid guid = new (guidhigh, entry, guidlow);

        SetGuidValue((ushort)EObjectFields.OBJECT_FIELD_GUID, guid);
        SetUInt32Value((ushort)EObjectFields.OBJECT_FIELD_TYPE, _objectType);

        _packGUID.Set(guid);
    }

    private void InitValues()
    {
        _values = new List<ObjectValue>();

        for (int i = 0; i < _valuesCount; i++)
        {
            _values.Add(new ObjectValue(0U));
        }

        _changesMask.SetCount(_valuesCount);

        _objectUpdated = false;
    }

    public ObjectGuid GetGUID()
    {
        return GetGuidValue((ushort)EObjectFields.OBJECT_FIELD_GUID);
    }

    public TypeID GetTypeId()
    {
        return _objectTypeId;
    }

    public int GetInt32Value(ushort index)
    {
        if (index < _valuesCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (_values == null)
        {
            throw new NullReferenceException(nameof(_values));
        }

        return _values[index].Int32Value;
    }

    public void SetInt32Value(ushort index, int value)
    {
        if (index < _valuesCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (_values != null)
        {
            if (_values[index].UInt32Value != value)
            {
                var objVal = _values[index];

                objVal.Int32Value = value;

                _changesMask.SetBit(index);

                AddToObjectUpdateIfNeeded();
            }
        }
    }

    public uint GetUInt32Value(ushort index)
    {
        if (index < _valuesCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (_values == null)
        {
            throw new NullReferenceException(nameof(_values));
        }

        return _values[index].UInt32Value;
    }

    public void SetUInt32Value(ushort index, uint value)
    {
        if (index < _valuesCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (_values != null)
        {
            if (_values[index].UInt32Value != value)
            {
                var objVal = _values[index];

                objVal.UInt32Value = value;

                _changesMask.SetBit(index);

                AddToObjectUpdateIfNeeded();
            }
        }
    }

    public float GetFloatValue(ushort index)
    {
        if (index < _valuesCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (_values == null)
        {
            throw new NullReferenceException(nameof(_values));
        }

        return _values[index].FloatValue;
    }

    public void SetFloatValue(ushort index, float value)
    {
        if (index < _valuesCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (_values != null)
        {
            if (_values[index].FloatValue != value)
            {
                var objVal = _values[index];
                objVal.FloatValue = value;

                _changesMask.SetBit(index);

                AddToObjectUpdateIfNeeded();
            }
        }
    }

    public ulong GetUInt64Value(ushort index)
    {
        if ((index + 1) < _valuesCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (_values == null)
        {
            throw new NullReferenceException(nameof(_values));
        }

        uint lo = _values[index].UInt32Value;
        uint hi = _values[index + 1].UInt32Value;

        byte[] uint64Arr = BitConverter.GetBytes(lo).Concat(BitConverter.GetBytes(hi)).ToArray();
        ulong uint64Val = BitConverter.ToUInt64(uint64Arr);

        return uint64Val;
    }

    public void SetUInt64Value(ushort index, ulong value)
    {
        if ((index + 1) < _valuesCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (_values != null)
        {
            var objValLow = _values[index];
            var objValHigh = _values[index + 1];

            byte[] oldValArr = BitConverter.GetBytes(objValLow.UInt32Value).Concat(BitConverter.GetBytes(objValHigh.UInt32Value)).ToArray();
            ulong oldValue = BitConverter.ToUInt64(oldValArr); ;

            if (oldValue != value)
            {
                objValLow.UInt32Value = PAIR64_LOPART(value);
                objValHigh.UInt32Value = PAIR64_HIPART(value);

                _changesMask.SetBit(index);
                _changesMask.SetBit(index + 1U);

                AddToObjectUpdateIfNeeded();
            }
        }
    }

    private static uint PAIR64_LOPART(ulong x)
    {
        return (uint)((x >> 32) & 0x00000000FFFFFFFFUL);
    }

    private static uint PAIR64_HIPART(ulong x)
    {
        return (uint)(x & 0x00000000FFFFFFFFUL);
    }

    public ObjectGuid GetGuidValue(ushort index)
    {
        if ((index + 1) < _valuesCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (_values == null)
        {
            throw new NullReferenceException(nameof(_values));
        }

        uint lo = _values[index].UInt32Value;
        uint hi = _values[index + 1].UInt32Value;

        byte[] guidRaw = BitConverter.GetBytes(lo).Concat(BitConverter.GetBytes(hi)).ToArray();
        ulong guid = BitConverter.ToUInt64(guidRaw);

        return ObjectGuid.Create(guid);
    }

    public void SetGuidValue(ushort index, ObjectGuid value)
    {
        if ((index + 1) < _valuesCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (_values != null)
        {
            if (ObjectGuid.Create(_values[index].UInt32Value) != value)
            {
                var objVal = _values[index];
                objVal.UInt32Value = (uint)value.GetRawValue();

                _changesMask.SetBit(index);
                _changesMask.SetBit(index + 1U);

                AddToObjectUpdateIfNeeded();
            }
        }
    }

    public byte GetByteValue(ushort index, byte offset)
    {
        if (index < _valuesCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (offset >= 4)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        if (_values == null)
        {
            throw new NullReferenceException(nameof(_values));
        }

        var objValBytes = BitConverter.GetBytes(_values[index].UInt32Value);

        return objValBytes[offset];
    }

    public void AddToObjectUpdateIfNeeded()
    {
        if (_inWorld && !_objectUpdated)
        {
            AddToObjectUpdate();
            _objectUpdated = true;
        }
    }

    public abstract void AddToObjectUpdate();

    public Player? ToPlayer()
    {
        if (GetTypeId() == TypeID.TYPEID_PLAYER)
        {
            return this as Player;
        }
        else
        {
            return null;
        }
    }

    public bool LoadIntoDataField(string? data, uint startOffset, uint count)
    {
        if (data == null || data.IsEmpty())
        {
            return false;
        }

        string[] tokens = data.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length != count)
        {
            return false;
        }

        for (uint index = 0; index < count; ++index)
        {
            uint? val = null;

            try { val = Convert.ToUInt32(tokens[index]); } catch{ }

            if (!val.HasValue)
            {
                return false;
            }

            if (_values != null)
            {
                _values[(int)(startOffset + index)] = new ObjectValue(val.Value);
                _changesMask.SetBit(startOffset + index);
            }
        }

        return true;
    }

    public virtual void SetObjectScale(float scale)
    {
        SetFloatValue((ushort)EObjectFields.OBJECT_FIELD_SCALE_X, scale);
    }

    public void SetByteValue(ushort index, byte offset, byte value)
    {
        if ((index + 1) < _valuesCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (offset > 3)
        {
            logger.Error(LogFilter.Object, $"BaseObject::SetByteValue: wrong offset {offset}");
            return;
        }

        if (_values != null)
        {
            if ((_values[index].UInt32Value >> (offset * 8)) != value)
            {
                var objVal = _values[index];

                objVal.UInt32Value &= ~((uint)0xFF << (offset * 8));
                objVal.UInt32Value |= (uint)value << (offset * 8);

                _changesMask.SetBit(index);

                AddToObjectUpdateIfNeeded();
            }
        }
    }
}

public class WorldObject : BaseObject
{
    protected string _name;

    private Map? _currMap;

    public WorldObject()
    {
        _name = string.Empty;
        _currMap = null;
    }

    public string GetName()
    {
        return _name;
    }

    public Map? FindMap()
    {
        return _currMap;
    }

    public Map GetMap()
    {
        if (_currMap == null)
        {
            throw new NullReferenceException(nameof(_currMap));
        }

        return _currMap;
    }

    public override void AddToObjectUpdate()
    {
        GetMap().AddUpdateObject(this);
    }
}
