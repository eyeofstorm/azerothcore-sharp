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

public class SQLQueryHolder<T> where T : notnull
{
    public Dictionary<T, PreparedStatement> Queries { get; private set; }  = new();
    public Dictionary<T, QueryResult> Results { get; private set; } = new();

    public void SetQuery(T index, string sql, params object[] args)
    {
        SetQuery(index, new PreparedStatement(string.Format(sql, args)));
    }

    public void SetQuery(T index, PreparedStatement stmt)
    {
        Queries[index] = stmt;
    }

    public void SetResult(T index, QueryResult result)
    {
        Results[index] = result;
    }

    public QueryResult GetResult(T index)
    {
        if (!Results.ContainsKey(index))
        {
            return new QueryResult();
        }

        return Results[index];
    }
}

class SQLQueryHolderTask<R> : ISqlOperation where R : notnull
{
    private readonly SQLQueryHolder<R> m_holder;
    private readonly TaskCompletionSource<SQLQueryHolder<R>> m_result;

    public SQLQueryHolderTask(SQLQueryHolder<R> holder)
    {
        m_holder = holder;
        m_result = new TaskCompletionSource<SQLQueryHolder<R>>();
    }

    public bool Execute<T>(MySqlBase<T> mySqlBase) where T : notnull
    {
        if (m_holder == null)
        {
            return false;
        }

        // execute all queries in the holder and pass the results
        foreach (var pair in m_holder.Queries)
        {
            m_holder.SetResult(pair.Key, mySqlBase.Query(pair.Value));
        }

        return m_result.TrySetResult(m_holder);
    }

    public Task<SQLQueryHolder<R>> GetFuture() { return m_result.Task; }
}

public class SQLQueryHolderCallback<R> : ISqlCallback where R : notnull
{
    private readonly Task<SQLQueryHolder<R>> m_future;
    private Action<SQLQueryHolder<R>>? m_callback;

    public SQLQueryHolderCallback(Task<SQLQueryHolder<R>> future)
    {
        m_future = future;
    }

    public void AfterComplete(Action<SQLQueryHolder<R>> callback)
    {
        m_callback = callback;
    }

    public bool InvokeIfReady()
    {
        if (m_future != null && m_future.Wait(0))
        {
            m_callback?.Invoke(m_future.Result);

            return true;
        }

        return false;
    }
}
