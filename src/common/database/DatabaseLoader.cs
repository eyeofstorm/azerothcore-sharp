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

using AzerothCore.Configuration;
using AzerothCore.Logging;

using MySqlConnector;

namespace AzerothCore.Database;

public class DatabaseLoader
{
    private static readonly Logger logger = LoggerFactory.GetLogger();

    private bool _autoSetup;
    private DatabaseTypeFlags _updateFlags;
    private List<Func<bool>> _open = new();
    private List<Func<bool>> _populate = new();
    private List<Func<bool>> _update = new();
    private List<Func<bool>> _prepare = new();

    public DatabaseLoader(DatabaseTypeFlags defaultUpdateMask)
    {
        _autoSetup = ConfigMgr.GetValueOrDefault("Updates.AutoSetup", true);
        _updateFlags = ConfigMgr.GetValueOrDefault("Updates.EnableDatabases", defaultUpdateMask);
    }

    public void AddDatabase<T>(MySqlBase<T> database, string baseDBName) where T : notnull
    {
        bool updatesEnabled = database.IsAutoUpdateEnabled(_updateFlags);

        _open.Add(() =>
        {
            MySqlConnectionInfo connectionObject = new()
            {
                Host = ConfigMgr.GetValueOrDefault(baseDBName + "DatabaseInfo.Host", ""),
                PortOrSocket = ConfigMgr.GetValueOrDefault(baseDBName + "DatabaseInfo.Port", ""),
                Username = ConfigMgr.GetValueOrDefault(baseDBName + "DatabaseInfo.Username", ""),
                Password = ConfigMgr.GetValueOrDefault(baseDBName + "DatabaseInfo.Password", ""),
                Database = ConfigMgr.GetValueOrDefault(baseDBName + "DatabaseInfo.Database", ""),
                UseSSL = ConfigMgr.GetValueOrDefault(baseDBName + "DatabaseInfo.SSL", false)
            };

            var error = database.Initialize(connectionObject);

            if (error != MySqlErrorCode.None)
            {
                // Database does not exist
                if (error == MySqlErrorCode.UnknownDatabase && updatesEnabled && _autoSetup)
                {
                    // Try to create the database and connect again if auto setup is enabled
                    if (CreateDatabase(connectionObject, database))
                    {
                        error = database.Initialize(connectionObject);
                    }
                }

                // If the error wasn't handled quit
                if (error != MySqlErrorCode.None)
                {
                    logger.Error(LogFilter.ServerLoading, $"\nDatabase {connectionObject.Database} NOT opened. There were errors opening the MySQL connections. Check your SQLErrors for specific errors.");

                    return false;
                }
            }

            return true;
        });

        if (updatesEnabled)
        {
            // Populate and update only if updates are enabled for this pool
            _populate.Add(() =>
            {
                var updater = database.GetUpdater();

                if (updater != null && !updater.Populate())
                {
                    logger.Error(LogFilter.ServerLoading, $"Could not populate the {database.GetDatabaseName()} database, see log for details.");

                    return false;
                }

                return true;
            });

            _update.Add(() =>
            {
                var updater = database.GetUpdater();

                if (updater != null && !updater.Update())
                {
                    logger.Error(LogFilter.ServerLoading, $"Could not update the {database.GetDatabaseName()} database, see log for details.");

                    return false;
                }

                return true;
            });
        }

        _prepare.Add(() =>
        {
            database.LoadPreparedStatements();
            return true;
        });
    }

    public bool CreateDatabase<T>(MySqlConnectionInfo connectionObject, MySqlBase<T> database) where T : notnull
    {
        logger.Info(LogFilter.ServerLoading, $"Database \"{connectionObject.Database}\" does not exist, do you want to create it? [yes (default) / no]: ");

        string? answer = Console.ReadLine();

        if (answer != null && answer[0] != 'y')
        {
            return false;
        }

        logger.Info(LogFilter.ServerLoading, $"Creating database \"{connectionObject.Database}\"...");

        // Path of temp file
        string temp = "create_table.sql";

        // Create temporary query to use external MySQL CLi
        try
        {
            using StreamWriter streamWriter = new(File.Open(temp, FileMode.Create, FileAccess.Write));
            streamWriter.Write($"CREATE DATABASE `{connectionObject.Database}` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci");
        }
        catch (Exception)
        {
            logger.Fatal(LogFilter.SqlUpdates, $"Failed to create temporary query file \"{temp}\"!");

            return false;
        }

        try
        {
            database.ApplyFile(temp, false);
        }
        catch (Exception)
        {
            logger.Fatal(LogFilter.SqlUpdates, $"Failed to create database {database.GetDatabaseName()}! Does the user (named in *.conf) have `CREATE`, `ALTER`, `DROP`, `INSERT` and `DELETE` privileges on the MySQL server?");

            File.Delete(temp);

            return false;
        }

        logger.Info(LogFilter.SqlUpdates, "Done.");

        File.Delete(temp);

        return true;
    }

    public bool Load()
    {
        if (_updateFlags == 0)
        {
            logger.Info(LogFilter.SqlUpdates, "Automatic database updates are disabled for all databases!");
        }

        if (_updateFlags != 0 && !DBExecutableUtil.CheckExecutable())
            return false;

        if (!OpenDatabases())
            return false;

        if (!PopulateDatabases())
            return false;

        if (!UpdateDatabases())
            return false;

        if (!PrepareStatements())
            return false;

        return true;
    }

    bool OpenDatabases()
    {
        return Process(_open);
    }

    // Processes the elements of the given stack until a predicate returned false.
    bool Process(List<Func<bool>> list)
    {
        while (!list.Empty())
        {
            if (!list[0].Invoke())
                return false;

            list.RemoveAt(0);
        }

        return true;
    }

    bool PopulateDatabases()
    {
        return Process(_populate);
    }

    bool UpdateDatabases()
    {
        return Process(_update);
    }

    bool PrepareStatements()
    {
        return Process(_prepare);
    }
}

public enum DatabaseTypeFlags
{
    None = 0,

    Login = 1,
    Character = 2,
    World = 4,
    Hotfix = 8,

    All = Login | Character | World | Hotfix
}
