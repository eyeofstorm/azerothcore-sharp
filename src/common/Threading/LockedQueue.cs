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

using DequeNet;

public interface IChecker<T>
{
	bool Process(T target);
}

public class LockedQueue<T>
{
	private readonly ConcurrentDeque<T> _queue = new();

	public LockedQueue() {  }

	public void Add(T item)
	{
		_queue.PushRight(item);
	}

	public bool Next(out T? result)
	{
        result = default;

        if (_queue.IsEmpty)
		{
			return false;
		}

		return _queue.TryPopLeft(out result);
	}

    public bool Next(out T? result, IChecker<T> checker)
    {
        result = default;

        if (_queue.IsEmpty)
        {
            return false;
        }

		bool ok = _queue.TryPopLeft(out result);

        if (ok)
		{
			if (!checker.Process(result))
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		else
		{
            return false;
        }
    }

	public int Size()
	{
		return _queue.Count;
	}
}

