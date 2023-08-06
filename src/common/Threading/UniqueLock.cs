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

namespace AzerothCore.Threading;

public class UniqueLock : IDisposable
{
    private readonly Mutex _mutex;
    private bool disposedValue;

    public UniqueLock(bool isDeferLock) : this(null, isDeferLock) { }

    public UniqueLock(Mutex? mutex, bool isDeferLock)
    {
        if (mutex == null)
        {
            _mutex = new Mutex(isDeferLock, null);
        }
        else
        {
            _mutex = mutex;
        }
        
        if (!isDeferLock)
        {
            _mutex.WaitOne(Timeout.Infinite);
        }
    }

    public bool Lock()
    {
        try
        {
            return _mutex.WaitOne(Timeout.Infinite);
        }
        catch
        {
            return false;
        }
    }

    public void Unlock()
    {
        _mutex.ReleaseMutex();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                try
                {
                    _mutex.ReleaseMutex();
                    _mutex.Close();
                }
                catch { }
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
