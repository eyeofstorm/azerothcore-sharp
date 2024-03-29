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

using System.Security.Cryptography;
using System.Text;

using AzerothCore.Configuration;
using AzerothCore.Logging;
using AzerothCore.Utilities;

namespace AzerothCore.Database;

public class DatabaseUpdater<T> where T : notnull
{
    private static readonly ILogger logger = LoggerFactory.GetLogger();

    private MySqlBase<T> _database;

    public DatabaseUpdater(MySqlBase<T> database)
    {
        _database = database;
    }

    public bool Populate()
    {
        QueryResult result = _database.Query("SHOW TABLES");

        if (!result.IsEmpty() && !result.IsEmpty())
            return true;

        logger.Info(LogFilter.SqlUpdates, $"Database {_database.GetDatabaseName()} is empty, auto populating it...");

        string path = GetSourceDirectory();
        string fileName = "Unknown";

        switch (_database.GetType().Name)
        {
            case "LoginDatabase":
                fileName = @"/sql/base/auth_database.sql";
                break;
            case "CharacterDatabase":
                fileName = @"/sql/base/characters_database.sql";
                break;
            case "WorldDatabase":
                fileName = @"/sql/TDB_full_world_1005.23021_2023_02_03.sql";
                break;
            case "HotfixDatabase":
                fileName = @"/sql/TDB_full_hotfixes_1005.23021_2023_02_03.sql";
                break;
        }

        if (!File.Exists(path + fileName))
        {
            logger.Error(LogFilter.SqlUpdates, $"File \"{path + fileName}\" is missing, download it from \"https://github.com/TrinityCore/TrinityCore/releases\" and place it in your sql directory.");
            return false;
        }

        // Update database
        logger.Info(LogFilter.SqlUpdates, $"Applying \'{fileName}\'...");

        try
        {
            ApplyFile(path + fileName);
        }
        catch (Exception)
        {
            return false;
        }

        logger.Info(LogFilter.SqlUpdates, $"Done Applying \'{fileName}\'");

        return true;
    }

    public bool Update()
    {
        logger.Info(LogFilter.SqlUpdates, $"Updating {_database.GetDatabaseName()} database...");

        string sourceDirectory = GetSourceDirectory();

        if (!Directory.Exists(sourceDirectory))
        {
            logger.Error(LogFilter.SqlUpdates, $"DBUpdater: The given source directory {sourceDirectory} does not exist, change the path to the directory where your sql directory exists. Shutting down.");

            return false;
        }

        var availableFiles = GetFileList();
        var appliedFiles = ReceiveAppliedFiles();

        bool redundancyChecks = ConfigMgr.GetOption("Updates.Redundancy", true);
        bool archivedRedundancy = ConfigMgr.GetOption("Updates.Redundancy", true);

        UpdateResult result = new();

        // Count updates
        foreach (var entry in appliedFiles)
        {
            if (entry.Value.State == State.RELEASED)
                ++result.recent;
            else
                ++result.archived;
        }

        foreach (var availableQuery in availableFiles)
        {
            logger.Debug(LogFilter.SqlUpdates, $"Checking update \"{availableQuery.GetFileName()}\"...");

            var applied = appliedFiles.LookupByKey(availableQuery.GetFileName());

            if (applied != null)
            {
                // If redundancy is disabled skip it since the update is already applied.
                if (!redundancyChecks)
                {
                    logger.Debug(LogFilter.SqlUpdates, "Update is already applied, skipping redundancy checks.");
                    appliedFiles.Remove(availableQuery.GetFileName());
                    continue;
                }

                // If the update is in an archived directory and is marked as archived in our database skip redundancy checks (archived updates never change).
                if (!archivedRedundancy && (applied.State == State.ARCHIVED) && (availableQuery.state == State.ARCHIVED))
                {
                    logger.Debug(LogFilter.SqlUpdates, "Update is archived and marked as archived in database, skipping redundancy checks.");
                    appliedFiles.Remove(availableQuery.GetFileName());
                    continue;
                }
            }

            // Calculate hash
            string hash = CalculateHash(availableQuery.path);

            UpdateMode mode = UpdateMode.Apply;

            // Update is not in our applied list
            if (applied == null)
            {
                // Catch renames (different filename but same hash)
                var hashIter = appliedFiles.Values.FirstOrDefault(p => p.Hash == hash);
                if (hashIter != null)
                {
                    // Check if the original file was removed if not we've got a problem.
                    var renameFile = availableFiles.Find(p => p.GetFileName() == hashIter.Name);

                    if (renameFile != null)
                    {
                        logger.Warn(LogFilter.SqlUpdates, $"Seems like update \"{availableQuery.GetFileName()}\" \'{hash.Substring(0, 7)}\' was renamed, but the old file is still there! Trade it as a new file! (Probably its an unmodified copy of file \"{renameFile.GetFileName()}\")");
                    }
                    // Its save to trade the file as renamed here
                    else
                    {
                        logger.Info(LogFilter.SqlUpdates, $"Renaming update \"{hashIter.Name}\" to \"{availableQuery.GetFileName()}\" \'{hash.Substring(0, 7)}\'.");

                        RenameEntry(hashIter.Name, availableQuery.GetFileName());
                        appliedFiles.Remove(hashIter.Name);
                        continue;
                    }
                }
                // Apply the update if it was never seen before.
                else
                {
                    logger.Info(LogFilter.SqlUpdates, $"Applying update \"{availableQuery.GetFileName()}\" \'{hash.Substring(0, 7)}\'...");
                }
            }
            // Rehash the update entry if it is contained in our database but with an empty hash.
            else if (ConfigMgr.GetOption("Updates.AllowRehash", true) && string.IsNullOrEmpty(applied.Hash))
            {
                mode = UpdateMode.Rehash;

                logger.Info(LogFilter.SqlUpdates, $"Re-hashing update \"{availableQuery.GetFileName()}\" \'{hash.Substring(0, 7)}\'...");
            }
            else
            {
                // If the hash of the files differs from the one stored in our database reapply the update (because it was changed).
                if (applied.Hash != hash && applied.State != State.ARCHIVED)
                {
                    logger.Info(LogFilter.SqlUpdates, $"Reapplying update \"{availableQuery.GetFileName()}\" \'{applied.Hash.Substring(0, 7)}\' . \'{hash.Substring(0, 7)}\' (it changed)...");
                }
                else
                {
                    // If the file wasn't changed and just moved update its state if necessary.
                    if (applied.State != availableQuery.state)
                    {
                        logger.Debug(LogFilter.SqlUpdates, $"Updating state of \"{availableQuery.GetFileName()}\" to \'{availableQuery.state}\'...");

                        UpdateState(availableQuery.GetFileName(), availableQuery.state);
                    }

                    logger.Debug(LogFilter.SqlUpdates, $"Update is already applied and is matching hash \'{hash.Substring(0, 7)}\'.");

                    appliedFiles.Remove(applied.Name);
                    continue;
                }
            }

            uint speed = 0;
            AppliedFileEntry file = new(availableQuery.GetFileName(), hash, availableQuery.state, 0);

            switch (mode)
            {
                case UpdateMode.Apply:
                    speed = ApplyTimedFile(availableQuery.path);
                    goto case UpdateMode.Rehash;
                case UpdateMode.Rehash:
                    UpdateEntry(file, speed);
                    break;
            }

            if (applied != null)
                appliedFiles.Remove(applied.Name);

            if (mode == UpdateMode.Apply)
                ++result.updated;
        }

        // Cleanup up orphaned entries if enabled
        if (!appliedFiles.Empty())
        {
            int cleanDeadReferencesMaxCount = ConfigMgr.GetOption("Updates.CleanDeadRefMaxCount", 3);
            bool doCleanup = (cleanDeadReferencesMaxCount < 0) || (appliedFiles.Count <= cleanDeadReferencesMaxCount);

            foreach (var entry in appliedFiles)
            {
                logger.Warn(LogFilter.SqlUpdates, $"File \'{entry.Key}\' was applied to the database but is missing in your update directory now!");

                if (doCleanup)
                    logger.Info(LogFilter.SqlUpdates, $"Deleting orphaned entry \'{entry.Key}\'...");
            }

            if (doCleanup)
                CleanUp(appliedFiles);
            else
            {
                logger.Error(LogFilter.SqlUpdates, $"Cleanup is disabled! There are {appliedFiles.Count} dirty files that were applied to your database but are now missing in your source directory!");
            }
        }

        string info = $"Containing {result.recent} new and {result.archived} archived updates.";

        if (result.updated == 0)
            logger.Info(LogFilter.SqlUpdates, $"{_database.GetDatabaseName()} database is up-to-date! {info}");
        else
            logger.Info(LogFilter.SqlUpdates, $"Applied {result.updated} query(s). {info}");

        return true;
    }

    string GetSourceDirectory()
    {
        return ConfigMgr.GetOption("Updates.SourcePath", "../../../");
    }

    uint ApplyTimedFile(string path)
    {
        // Benchmark query speed
        uint oldMSTime = TimeHelper.GetMSTime();

        // Update database
        ApplyFile(path);

        // Return time the query took to apply
        return TimeHelper.GetMSTimeDiffToNow(oldMSTime);
    }

    void ApplyFile(string path)
    {
        _database.ApplyFile(path);
    }
    
    void Apply(string query)
    {
        _database.Execute(query);
    }

    void UpdateEntry(AppliedFileEntry entry, uint speed)
    {
        string update = $"REPLACE INTO `updates` (`name`, `hash`, `state`, `speed`) VALUES (\"{entry.Name}\", \"{entry.Hash}\", \'{entry.State}\', {speed})";

        // Update database
        Apply(update);
    }

    void RenameEntry(string from, string to)
    {
        // Delete target if it exists
        {
            string update = $"DELETE FROM `updates` WHERE `name`=\"{to}\"";

            // Update database
            Apply(update);
        }

        // Rename
        {
            string update = $"UPDATE `updates` SET `name`=\"{to}\" WHERE `name`=\"{from}\"";

            // Update database
            Apply(update);
        }
    }

    void CleanUp(Dictionary<string, AppliedFileEntry> storage)
    {
        if (storage.Empty())
            return;

        int remaining = storage.Count;
        string update = "DELETE FROM `updates` WHERE `name` IN(";

        foreach (var entry in storage)
        {
            update += $"\"{entry.Key}\"";
            if ((--remaining) > 0)
                update += ", ";
        }

        update += ")";

        // Update database
        Apply(update);
    }

    void UpdateState(string name, State state)
    {
        string update = $"UPDATE `updates` SET `state`=\'{state}\' WHERE `name`=\"{name}\"";

        // Update database
        Apply(update);
    }

    List<FileEntry> GetFileList()
    {
        List<FileEntry> fileList = new();

        QueryResult result = _database.Query("SELECT `path`, `state` FROM `updates_include`");

        if (result.IsEmpty())
            return fileList;

        do
        {
            string? path = result.Read<string>(0);

            if (path != null && path[0] == '$')
            {
                path = GetSourceDirectory() + path.Substring(1);
            }

            if (!Directory.Exists(path))
            {
                logger.Warn(LogFilter.SqlUpdates, $"DBUpdater: Given update include directory \"{path}\" isn't existing, skipped!");
                continue;
            }

            State state = result.Read<string>(1).ToEnum<State>();

            foreach (var file in GetFilesFromDirectory(path, state))
            {
                fileList.Add(file);
            }

            logger.Debug(LogFilter.SqlUpdates, $"Added applied file \"{path}\" from remote.");

        } while (result.NextRow());

        return fileList;
    }

    Dictionary<string, AppliedFileEntry> ReceiveAppliedFiles()
    {
        Dictionary<string, AppliedFileEntry> map = new();

        QueryResult result = _database.Query("SELECT `name`, `hash`, `state`, UNIX_TIMESTAMP(`timestamp`) FROM `updates` ORDER BY `name` ASC");

        if (result.IsEmpty())
        {
            return map;
        }

        do
        {
            AppliedFileEntry entry = new AppliedFileEntry(
                                            result.Read<string>(0) ?? string.Empty,
                                            result.Read<string>(1) ?? string.Empty,
                                            result.Read<string>(2).ToEnum<State>(),
                                            result.Read<ulong>(3));

            map.Add(entry.Name, entry);
        }
        while (result.NextRow());

        return map;
    }

    IEnumerable<FileEntry> GetFilesFromDirectory(string directory, State state)
    {
        Queue<string> queue = new();
        queue.Enqueue(directory);
        while (queue.Count > 0)
        {
            directory = queue.Dequeue();
            try
            {
                foreach (string subDir in Directory.GetDirectories(directory).OrderBy(p => p))
                {
                    queue.Enqueue(subDir);
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(LogFilter.SqlUpdates, $"DBUpdater: {directory} Exception: {ex}");
            }

            string[] files = Directory.GetFiles(directory, "*.sql").OrderBy(p => p).ToArray();
            for (int i = 0; i < files.Length; i++)
            {
                yield return new FileEntry(files[i], state);
            }
        }
    }

    string CalculateHash(string fileName)
    {
        using (SHA1 sha1 = SHA1.Create())
        {
            string text = File.ReadAllText(fileName).Replace("\r", "");
            return sha1.ComputeHash(Encoding.UTF8.GetBytes(text)).ToHexString();
        }
    }
}

public class AppliedFileEntry
{
    public AppliedFileEntry(string name, string hash, State state, ulong timestamp)
    {
        Name = name;
        Hash = hash;
        State = state;
        Timestamp = timestamp;
    }

    public string Name;
    public string Hash;
    public State State;
    public ulong Timestamp;
}

public class FileEntry
{
    public FileEntry(string _path, State _state)
    {
        path = _path.Replace(@"\", @"/");
        state = _state;
    }

    public string GetFileName()
    {
        return Path.GetFileName(path);
    }

    public string path;
    public State state;
}

struct UpdateResult
{
    public int updated;
    public int recent;
    public int archived;
}

public enum State
{
    RELEASED,
    ARCHIVED
}

enum UpdateMode
{
    Apply,
    Rehash
}
