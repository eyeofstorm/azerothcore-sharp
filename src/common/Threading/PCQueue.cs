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

using System.Collections.Generic;
using System.Threading;

namespace AzerothCore.Threading;

public class ProducerConsumerQueue<T>
{
    private static readonly object _queueLock = new();
    private Queue<T> _queue = new();
    private volatile bool _shutdown;

    public ProducerConsumerQueue()
    {
        _shutdown = false;
    }

    public void Push(T value)
    {
        lock (_queueLock)
        {
            _queue.Enqueue(value);
            Monitor.PulseAll(_queueLock);
        }
    }

    public bool Empty()
    {
        lock (_queueLock)
        {
            return _queue.Count == 0;
        }
    }

    public bool Pop(out T? value)
    {
        value = default;

        lock (_queueLock)
        {
            if (_queue.Count == 0 || _shutdown)
                return false;

            value = _queue.Dequeue();
            return true;
        }
    }

    public void WaitAndPop(out T? value)
    {
        value = default;

        lock (_queueLock)
        {
            while (_queue.Count == 0 && !_shutdown)
            {
                Monitor.Wait(_queueLock);
            }

            if (_queue.Count == 0 || _shutdown)
            {
                return;
            }

            value = _queue.Dequeue();
        }
    }

    public void Cancel()
    {
        lock (_queueLock)
        {
            while (_queue.Count != 0)
            {
                _queue.Dequeue();
            }

            _shutdown = true;

            Monitor.PulseAll(_queueLock);
        }
    }
}
