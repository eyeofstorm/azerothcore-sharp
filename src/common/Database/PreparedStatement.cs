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

public class PreparedStatement
{
    public string CommandText;
    public Dictionary<int, object?> Parameters = new();

    public PreparedStatement(string commandText)
    {
        CommandText = commandText;
    }

    public void SetData(int index, sbyte value)
    {
        Parameters.Add(index, value);
    }

    public void SetData(int index, byte value)
    {
        Parameters.Add(index, value);
    }

    public void SetData(int index, short value)
    {
        Parameters.Add(index, value);
    }

    public void SetData(int index, ushort value)
    {
        Parameters.Add(index, value);
    }

    public void SetData(int index, int value)
    {
        Parameters.Add(index, value);
    }

    public void SetData(int index, uint value)
    {
        Parameters.Add(index, value);
    }

    public void SetData(int index, long value)
    {
        Parameters.Add(index, value);
    }

    public void SetData(int index, ulong value)
    {
        Parameters.Add(index, value);
    }

    public void SetData(int index, float value)
    {
        Parameters.Add(index, value);
    }

    public void SetData(int index, byte[]? value)
    {
        Parameters.Add(index, value);
    }

    public void SetData(int index, string? value)
    {
        Parameters.Add(index, value);
    }

    public void SetData(int index, bool value)
    {
        Parameters.Add(index, value);
    }

    public void SetNull(int index)
    {
        Parameters.Add(index, null);
    }

    public void Clear()
    {
        Parameters.Clear();
    }
}

public class PreparedStatementTask : ISqlOperation
{
    private PreparedStatement m_stmt;
    private bool _needsResult;
    private TaskCompletionSource<QueryResult>? m_taskFuture;

    public PreparedStatementTask(PreparedStatement stmt, bool needsResult = false)
    {
        m_stmt = stmt;
        _needsResult = needsResult;

        if (_needsResult)
        {
            m_taskFuture = new TaskCompletionSource<QueryResult>();
        }
    }

    public bool Execute<T>(MySqlBase<T> mySqlBase) where T : notnull
    {
        if (_needsResult && m_taskFuture != null)
        {
            QueryResult queryResult = mySqlBase.Query(m_stmt);

            if (queryResult == null)
            {
                m_taskFuture.SetResult(new QueryResult());
                return false;
            }
            else
            {
                m_taskFuture.SetResult(queryResult);
                return true;
            }
        }

        return mySqlBase.DirectExecute(m_stmt);
    }

    public Task<QueryResult>? GetFuture()
    {
        return m_taskFuture?.Task;
    }
}
