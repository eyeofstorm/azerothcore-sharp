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

using System.Diagnostics;
using System.Runtime.Serialization;

namespace System.Collections.Concurrent;

[DebuggerDisplay("Count = {Count}")]
[Serializable]
public class ConcurrentHashSet<T> : ICollection<T>, ISet<T>, ISerializable, IDeserializationCallback, IDisposable
{
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    private readonly HashSet<T> _hashSet = new();

    public ConcurrentHashSet()
    {
    }

    public ConcurrentHashSet(IEqualityComparer<T> comparer)
    {
        _hashSet = new HashSet<T>(comparer);
    }

    public ConcurrentHashSet(IEnumerable<T> collection)
    {
        _hashSet = new HashSet<T>(collection);
    }

    public ConcurrentHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
    {
        _hashSet = new HashSet<T>(collection, comparer);
    }

    protected ConcurrentHashSet(SerializationInfo info, StreamingContext context)
    {
        _hashSet = new HashSet<T>();

        // not sure about this one really...
        var iSerializable = _hashSet as ISerializable;
        iSerializable.GetObjectData(info, context);
    }

    #region Dispose

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
            if (_lock != null)
                _lock.Dispose();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _hashSet.GetEnumerator();
    }

    ~ConcurrentHashSet()
    {
        Dispose(false);
    }

    public void OnDeserialization(object? sender)
    {
        _hashSet.OnDeserialization(sender);
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        _hashSet.GetObjectData(info, context);
    }

    #endregion

    public void Add(T item)
    {
        _lock.EnterWriteLock();
        try
        {
            _hashSet.Add(item);
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
        }
    }

    public void UnionWith(IEnumerable<T> other)
    {
        _lock.EnterWriteLock();
        _lock.EnterReadLock();
        try
        {
            _hashSet.UnionWith(other);
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            if (_lock.IsReadLockHeld) _lock.ExitReadLock();
        }
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        _lock.EnterWriteLock();
        _lock.EnterReadLock();
        try
        {
            _hashSet.IntersectWith(other);
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            if (_lock.IsReadLockHeld) _lock.ExitReadLock();
        }
    }

    public void ExceptWith(IEnumerable<T> other)
    {
        _lock.EnterWriteLock();
        _lock.EnterReadLock();
        try
        {
            _hashSet.ExceptWith(other);
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            if (_lock.IsReadLockHeld) _lock.ExitReadLock();
        }
    }

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        _lock.EnterWriteLock();
        try
        {
            _hashSet.SymmetricExceptWith(other);
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
        }
    }

    public bool IsSubsetOf(IEnumerable<T> other)
    {
        _lock.EnterWriteLock();
        try
        {
            return _hashSet.IsSubsetOf(other);
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
        }
    }

    public bool IsSupersetOf(IEnumerable<T> other)
    {
        _lock.EnterWriteLock();
        try
        {
            return _hashSet.IsSupersetOf(other);
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
        }
    }

    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        _lock.EnterWriteLock();
        try
        {
            return _hashSet.IsProperSupersetOf(other);
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
        }
    }

    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        _lock.EnterWriteLock();
        try
        {
            return _hashSet.IsProperSubsetOf(other);
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
        }
    }

    public bool Overlaps(IEnumerable<T> other)
    {
        _lock.EnterWriteLock();
        try
        {
            return _hashSet.Overlaps(other);
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
        }
    }

    public bool SetEquals(IEnumerable<T> other)
    {
        _lock.EnterWriteLock();
        try
        {
            return _hashSet.SetEquals(other);
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
        }
    }

    bool ISet<T>.Add(T item)
    {
        _lock.EnterWriteLock();
        try
        {
            return _hashSet.Add(item);
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
        }
    }

    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            _hashSet.Clear();
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
        }
    }

    public bool Contains(T item)
    {
        _lock.EnterWriteLock();
        try
        {
            return _hashSet.Contains(item);
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
        }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _lock.EnterWriteLock();
        try
        {
            _hashSet.CopyTo(array, arrayIndex);
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
        }
    }

    public bool Remove(T item)
    {
        _lock.EnterWriteLock();
        try
        {
            return _hashSet.Remove(item);
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public int Count
    {
        get
        {
            _lock.EnterWriteLock();
            try
            {
                return _hashSet.Count;
            }
            finally
            {
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }

        }
    }

    public bool IsReadOnly
    {
        get { return false; }
    }
}
