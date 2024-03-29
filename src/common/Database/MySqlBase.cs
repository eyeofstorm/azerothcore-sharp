﻿/*
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
using System.Text;
using System.Transactions;

using AzerothCore.Configuration;
using AzerothCore.Logging;
using AzerothCore.Threading;

using MySqlConnector;

namespace AzerothCore.Database;

public class MySqlConnectionInfo
{
    public MySqlConnection GetConnection()
    {
        return new MySqlConnection($"Server={Host};Port={PortOrSocket};User Id={Username};Password={Password};Database={Database};Allow User Variables=True;Pooling=true;ConnectionIdleTimeout=1800;Command Timeout=0");
    }

    public string? Host;
    public string? PortOrSocket;
    public bool UseSSL;
    public string? Username;
    public string? Password;
    public string? Database;
    public int Poolsize;
}

public struct DBVersion
{
    public int Major { get; }
    public int Minor { get; }
    public int Build { get; }
    public bool IsMariaDB { get; }

    public DBVersion(int major, int minor, int build, bool isMariaDB)
    {
        Major = major;
        Minor = minor;
        Build = build;
        IsMariaDB = isMariaDB;
    }

    public static DBVersion Parse(string versionString)
    {
        int start = 0;
        int index = versionString.IndexOf('.', start);

        string val = versionString.Substring(start, index - start).Trim();
        int major = Convert.ToInt32(val, System.Globalization.NumberFormatInfo.InvariantInfo);

        start = index + 1;
        index = versionString.IndexOf('.', start);

        val = versionString.Substring(start, index - start).Trim();
        int minor = Convert.ToInt32(val, System.Globalization.NumberFormatInfo.InvariantInfo);

        start = index + 1;
        int i = start;

        while (i < versionString.Length && Char.IsDigit(versionString, i))
        {
            i++;
        }

        val = versionString.Substring(start, i - start).Trim();
        int build = Convert.ToInt32(val, System.Globalization.NumberFormatInfo.InvariantInfo);

        return new DBVersion(major, minor, build, versionString.Contains("Maria"));
    }

    public bool IsAtLeast(int majorNum, int minorNum, int buildNum)
    {
        if (Major > majorNum)
            return true;

        if (Major == majorNum && Minor > minorNum)
            return true;

        if (Major == majorNum && Minor == minorNum && Build >= buildNum)
            return true;

        return false;
    }
}

public abstract class MySqlBase<T> where T : notnull
{
    private static readonly ILogger logger = LoggerFactory.GetLogger();

    private static Dictionary<T, string> _preparedQueries = new();

    private ProducerConsumerQueue<ISqlOperation> _sqlOperationQueue = new();
    private MySqlConnectionInfo? _connectionInfo;
    private DatabaseUpdater<T>? _updater;
    private DatabaseWorker<T>? _worker;
    private DBVersion version;

    public MySqlErrorCode Initialize(MySqlConnectionInfo connectionInfo)
    {
        _connectionInfo = connectionInfo;
        _updater = new DatabaseUpdater<T>(this);
        _worker = new DatabaseWorker<T>(_sqlOperationQueue, this);

        try
        {
            using var connection = _connectionInfo.GetConnection();
            connection.Open();

            version = DBVersion.Parse(connection.ServerVersion);
            logger.Info(LogFilter.SqlDriver, $"Connected to DB: {_connectionInfo.Database} Server: {(version.IsMariaDB ? "MariaDB" : "MySQL")} Ver: {connection.ServerVersion}");

            return MySqlErrorCode.None;
        }
        catch (MySqlException ex)
        {
            return MySqlBase<T>.HandleMySQLException(ex);
        }
    }

    public bool DirectExecute(string sql, params object[] args)
    {
        string finalSql = string.Format(sql, args);
        return DirectExecute(new PreparedStatement(finalSql));
    }

    public bool DirectExecute(PreparedStatement stmt)
    {
        try
        {
            if (_connectionInfo == null)
            {
                throw new NullReferenceException(nameof(_connectionInfo));
            }

            using var Connection = _connectionInfo.GetConnection();
            Connection.Open();

            using MySqlCommand cmd = Connection.CreateCommand();
            cmd.CommandText = stmt.CommandText;

            foreach (var parameter in stmt.Parameters)
            {
                cmd.Parameters.AddWithValue("@" + parameter.Key, parameter.Value);
            }

            cmd.ExecuteNonQuery();

            return true;
        }
        catch (MySqlException ex)
        {
            MySqlBase<T>.HandleMySQLException(ex, stmt.CommandText, stmt.Parameters);

            return false;
        }
    }

    public void Execute(string sql, params object[] args)
    {
        Execute(new PreparedStatement(string.Format(sql, args)));
    }

    public void Execute(PreparedStatement stmt)
    {
        PreparedStatementTask task = new(stmt);
        _sqlOperationQueue.Push(task);
    }

    public void ExecuteOrAppend(SQLTransaction trans, PreparedStatement stmt)
    {
        if (trans == null)
        {
            Execute(stmt);
        }
        else
        {
            trans.Append(stmt);
        }
    }

    public QueryResult Query(string sql, params object[] args)
    {
        return Query(new PreparedStatement(string.Format(sql, args)));
    }

    public QueryResult Query(PreparedStatement stmt)
    {
        try
        {
            if (_connectionInfo == null)
            {
                throw new NullReferenceException(nameof(_connectionInfo));
            }

            MySqlConnection Connection = _connectionInfo.GetConnection();

            Connection.Open();

            MySqlCommand cmd = Connection.CreateCommand();
            cmd.CommandText = stmt.CommandText;

            foreach (var parameter in stmt.Parameters)
            {
                cmd.Parameters.AddWithValue("@" + parameter.Key, parameter.Value);
            }

            return new QueryResult(cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection));
        }
        catch (MySqlException ex)
        {
            MySqlBase<T>.HandleMySQLException(ex, stmt.CommandText, stmt.Parameters);

            return new QueryResult();
        }
    }

    public QueryCallback AsyncQuery(PreparedStatement stmt)
    {
        PreparedStatementTask preparedStmtExecTask = new PreparedStatementTask(stmt, true);

        // Store future result before enqueueing - task might get already processed and deleted before returning from this method
        Task<QueryResult>? preparedStmtExecTaskResult = preparedStmtExecTask.GetFuture();
        _sqlOperationQueue.Push(preparedStmtExecTask);

        QueryCallback callback = new QueryCallback(preparedStmtExecTaskResult);

        return callback;
    }

    public SQLQueryHolderCallback<R> DelayQueryHolder<R>(SQLQueryHolder<R> holder) where R : notnull
    {
        SQLQueryHolderTask<R> task = new(holder);

        // Store future result before enqueueing - task might get already processed and deleted before returning from this method
        Task<SQLQueryHolder<R>> result = task.GetFuture();
        _sqlOperationQueue.Push(task);

        return new(result);
    }

    public void LoadPreparedStatements()
    {
        PreparedStatements();
    }

    public void PrepareStatement(T statement, string sql)
    {
        StringBuilder sb = new();
        int index = 0;

        for (var i = 0; i < sql.Length; i++)
        {
            if (sql[i].Equals('?'))
            {
                sb.Append("@" + index++);
            }
            else
            {
                sb.Append(sql[i]);
            }
        }

        _preparedQueries[statement] = sb.ToString();
    }

    public static PreparedStatement GetPreparedStatement(T statement)
    {
        return new PreparedStatement(_preparedQueries[statement]);
    }

    public void ApplyFile(string path, bool useDatabase = true)
    {
        if (_connectionInfo == null)
        {
            return;
        }

        // CLI Client connection info
        string args = $"-h{_connectionInfo.Host} ";
        args += $"-u{_connectionInfo.Username} ";

        if (!_connectionInfo.Password.IsEmpty())
        {
            args += $"-p{_connectionInfo.Password} ";
        }

        // Check if we want to connect through ip or socket (Unix only)
        if (OperatingSystem.IsWindows())
        {
            if (_connectionInfo.Host == ".")
            {
                args += "--protocol=PIPE ";
            }
            else
            {
                args += $"-P{_connectionInfo.PortOrSocket} ";
            }
        }
        else
        {
            if (_connectionInfo.PortOrSocket != null && _connectionInfo.PortOrSocket.Length > 0)
            {
                char c = _connectionInfo.PortOrSocket[0];

                if (!char.IsDigit(c))
                {
                    // We can't check if host == "." here, because it is named localhost if socket option is enabled
                    args += "-P0 ";
                    args += "--protocol=SOCKET ";
                    args += $"-S{_connectionInfo.PortOrSocket} ";
                }
                else
                {
                    // generic case
                    args += $"-P{_connectionInfo.PortOrSocket} ";
                }
            }
        }

        // Set the default charset to utf8
        args += "--default-character-set=utf8 ";

        // Set max allowed packet to 1 GB
        args += "--max-allowed-packet=1GB ";

        if (!version.IsMariaDB && version.IsAtLeast(8, 0, 0))
        {
            if (_connectionInfo.UseSSL)
            {
                args += "--ssl-mode=REQUIRED ";
            }
        }
        else
        {
            if (_connectionInfo.UseSSL)
            {
                args += "--ssl ";
            }
        }

        // Execute sql file
        args += "-e ";
        args += "\"BEGIN; SOURCE \"" + path + "\"; COMMIT;\" ";

        // Database
        if (useDatabase && !_connectionInfo.Database.IsEmpty())
        {
            args += _connectionInfo.Database;
        }

        // Invokes a mysql process which doesn't leak credentials to logs
        Process process = new()
        {
            StartInfo = new(DBExecutableUtil.GetMySQLExecutable())
        };

        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.Arguments = args;

        process.Start();

        process.WaitForExit();

        logger.Info(LogFilter.SqlUpdates, process.StandardOutput.ReadToEnd());
        logger.Error(LogFilter.SqlUpdates, process.StandardError.ReadToEnd());

        if (process.ExitCode != 0)
        {
            logger.Fatal(LogFilter.SqlUpdates,
                            $"Applying of file \'{path}\' to database \'{GetDatabaseName()}\' failed!" +
                            " If you are a user, please pull the latest revision from the repository. " +
                            "Also make sure you have not applied any of the databases with your sql client. " +
                            "You cannot use auto-update system and import sql files from CypherCore repository with your sql client. " +
                            "If you are a developer, please fix your sql query.");

            throw new Exception("update failed");
        }
    }

    public static void EscapeString(ref string str)
    {
        str = MySqlHelper.EscapeString(str);
    }

    public void CommitTransaction(SQLTransaction transaction)
    {
        _sqlOperationQueue.Push(new TransactionTask(transaction));
    }

    public TransactionCallback AsyncCommitTransaction(SQLTransaction transaction)
    {
        TransactionWithResultTask task = new(transaction);

        Task<bool> result = task.GetFuture();
        _sqlOperationQueue.Push(task);

        return new TransactionCallback(result);
    }

    public MySqlErrorCode DirectCommitTransaction(SQLTransaction transaction)
    {
        if (_connectionInfo == null)
        {
            return MySqlErrorCode.UnknownError;
        }

        using (var Connection = _connectionInfo.GetConnection())
        {
            string query = "";

            Connection.Open();

            using (MySqlTransaction trans = Connection.BeginTransaction())
            {
                try
                {
                    using (var scope = new TransactionScope())
                    {
                        foreach (var cmd in transaction.commands)
                        {
                            cmd.Transaction = trans;
                            cmd.Connection = Connection;
                            cmd.ExecuteNonQuery();
                            query = cmd.CommandText;
                        }

                        trans.Commit();
                        scope.Complete();
                    }

                    return  MySqlErrorCode.None;
                }
                catch (MySqlException ex) //error occurred
                {
                    trans.Rollback();

                    return MySqlBase<T>.HandleMySQLException(ex, query);
                }
            }
        }
    }

    private static MySqlErrorCode HandleMySQLException(MySqlException ex, string query = "", Dictionary<int, object?>? parameters = null)
    {
        MySqlErrorCode code = (MySqlErrorCode)ex.Number;

        if (ex.InnerException is MySqlException)
        {
            code = (MySqlErrorCode)((MySqlException)ex.InnerException).Number;
        }

        StringBuilder stringBuilder = new($"SqlException: MySqlErrorCode: {code} Message: {ex.Message} SqlQuery: {query} ");

        if (parameters != null)
        {
            stringBuilder.Append("Parameters: ");

            foreach (var pair in parameters)
            {
                stringBuilder.Append($"{pair.Key} : {pair.Value}");
            }
        }

        logger.Debug(LogFilter.Sql, ex);
        logger.Error(LogFilter.Sql, stringBuilder.ToString());

        switch (code)
        {
            case MySqlErrorCode.BadFieldError:
            case MySqlErrorCode.NoSuchTable:
                logger.Error(LogFilter.Sql, "Your database structure is not up to date. Please make sure you've executed all queries in the sql/updates folders.");
                break;
            case MySqlErrorCode.ParseError:
                logger.Error(LogFilter.Sql, "Error while parsing SQL. Core fix required.");
                break;
        }

        return code;
    }

    public DatabaseUpdater<T>? GetUpdater()
    {
        return _updater;
    }

    public bool IsAutoUpdateEnabled(DatabaseTypeFlags updateMask)
    {
        switch (GetType().Name)
        {
            case "LoginDatabase":
                return updateMask.HasAnyFlag(DatabaseTypeFlags.Login);
            case "CharacterDatabase":
                return updateMask.HasAnyFlag(DatabaseTypeFlags.Character);
            case "WorldDatabase":
                return updateMask.HasAnyFlag(DatabaseTypeFlags.World);
            case "HotfixDatabase":
                return updateMask.HasAnyFlag(DatabaseTypeFlags.Hotfix);
        }
        return false;
    }

    public string? GetDatabaseName()
    {
        return _connectionInfo?.Database;
    }

    public abstract void PreparedStatements();
}

public static class DBExecutableUtil
{
    private static readonly ILogger logger = LoggerFactory.GetLogger();
    private static string mysqlExecutablePath = string.Empty;

    public static string GetMySQLExecutable()
    {
        return mysqlExecutablePath;
    }

    public static bool CheckExecutable()
    {
        string mysqlExePath = ConfigMgr.GetOption("MySQLExecutable", "");

        if (mysqlExePath.IsEmpty() || !File.Exists(mysqlExePath))
        {
            logger.Fatal(LogFilter.SqlUpdates, $"Didn't find any executable MySQL binary at \'{mysqlExePath}\' or in path, correct the path in the *.conf (\"MySQLExecutable\").");

            return false;
        }

        // Correct the path to the cli
        mysqlExecutablePath = mysqlExePath;

        return true;
    }
}
