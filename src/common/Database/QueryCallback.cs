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

namespace AzerothCore.Database;

public class QueryCallback : ISqlCallback
{
    private Task<QueryResult>? _result;
    private Queue<QueryCallbackData> _callbacks = new();

    public QueryCallback(Task<QueryResult>? result)
    {
        _result = result;
    }

    public QueryCallback WithCallback(Action<QueryResult> callback)
    {
        return WithChainingCallback((queryCallback, result) =>
        {
            callback(result);
        });
    }

    public QueryCallback WithCallback<T>(Action<T, QueryResult> callback, T obj)
    {
        return WithChainingCallback((queryCallback, result) =>
        {
            callback(obj, result);
        });
    }

    public QueryCallback WithChainingCallback(Action<QueryCallback, QueryResult> callback)
    {
        QueryCallbackData queryCallbackData = new QueryCallbackData(callback);

        _callbacks.Enqueue(queryCallbackData);

        return this;
    }

    public void SetNextQuery(QueryCallback next)
    {
        _result = next._result;
    }

    public bool InvokeIfReady()
    {
        QueryCallbackData callbackData = _callbacks.Peek();

        while (true)
        {
            if (_result != null && _result.Wait(0))
            {
                Task<QueryResult> f = _result;
                Action<QueryCallback, QueryResult>? cb = callbackData.callback;
                _result = null;

                if (cb != null)
                {
                    cb(this, f.Result);
                }

                _callbacks.Dequeue();
                bool hasNext = _result != null;

                if (_callbacks.Count == 0)
                {
                    return true;
                }

                // abort chain
                if (!hasNext)
                {
                    return true;
                }

                callbackData = _callbacks.Peek();
            }
            else
            {
                return false;
            }
        }
    }
}

public struct QueryCallbackData
{
    public QueryCallbackData(Action<QueryCallback, QueryResult> action)
    {
        callback = action;
    }

    public void Clear()
    {
        callback = null;
    }

    public Action<QueryCallback, QueryResult>? callback;
}
