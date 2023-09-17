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

using System.Globalization;
using System.Net;

using AzerothCore.Configuration;
using AzerothCore.Constants;
using AzerothCore.Database;
using AzerothCore.Game;
using AzerothCore.Game.Server;
using AzerothCore.Logging;
using AzerothCore.Utilities;

namespace AzerothCore;

internal class WorldServer
{
    private static readonly ILogger logger = LoggerFactory.GetLogger("worldserver");

    internal static void Main()
    {
        // Set Culture
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
        Console.CancelKeyPress += Console_CancelKeyPress;

        if (!ConfigMgr.LoadAppConfigs("worldserver.conf"))
        {
            ExitNow(1);
        }

        Banner.Show();

        // Initialize the database connection
        if (!StartDB())
        {
            ExitNow(1);
        }

        // set server offline (not connectable)
        DB.Login.DirectExecute(
                    "UPDATE realmlist SET flag = (flag & ~{0}) | {1} WHERE id = '{2}'",
                    (byte)RealmFlags.REALM_FLAG_OFFLINE,
                    (byte)RealmFlags.REALM_FLAG_VERSION_MISMATCH,
                    Global.sWorld.GetRealm().Id.Index);

        LoadRealmInfo();

        // Initialize the World
        Global.sWorld.SetInitialWorldSettings();

        // TODO: worldserver: Start the Remote Access port (acceptor) if enabled

        // worldserver: Launch the worldserver listener socket
        string bindIp = ConfigMgr.GetOption("BindIP", "0.0.0.0");
        int port = ConfigMgr.GetOption("WorldServerPort", 3724);
        
        if (port < 0 || port > 0xFFFF)
        {
            logger.Error(LogFilter.ServerLoading, "Specified port out of allowed range (1-65535)");
            ExitNow(1);
        }

        int networkThreads = ConfigMgr.GetOption("Network.Threads", 1);

        if (networkThreads <= 0)
        {
            logger.Error(LogFilter.ServerLoading, "Network.Threads must be greater than 0");

            ExitNow(1);
        }

        if (!WorldSocketManager.Instance.StartNetwork(bindIp, port, networkThreads))
        {
            logger.Error(LogFilter.ServerLoading, "Failed to start worldserver Network");

            ExitNow(-1);
        }

        // Set server online (allow connecting now)
        Realm realm = Global.sWorld.GetRealm();
        DB.Login.DirectExecute("UPDATE realmlist SET flag = flag & ~{0}, population = 0 WHERE id = '{1}'", (byte)RealmFlags.REALM_FLAG_VERSION_MISMATCH, realm.Id.Index);
        realm.PopulationLevel = 0.0f;
        realm.Flags &= ~RealmFlags.REALM_FLAG_VERSION_MISMATCH;

        logger.Info(LogFilter.ServerLoading, "worldserver-daemon ready...");

        WorldUpdateLoop();

        // set server offline
        DB.Login.DirectExecute("UPDATE realmlist SET flag = flag | {0} WHERE id = '{1}'", (byte)RealmFlags.REALM_FLAG_OFFLINE, realm.Id.Index);
    }

    private static void WorldUpdateLoop()
    {
        // TODO: worldserver: WorldUpdateLoop()
        uint minUpdateDiff = ConfigMgr.GetOption<uint>("MinWorldUpdateTime", 1);
        uint realPrevTime = TimeHelper.GetMSTime();

        uint maxCoreStuckTime = ConfigMgr.GetOption<uint>("MaxCoreStuckTime", 60) * 1000;
        uint halfMaxCoreStuckTime = maxCoreStuckTime / 2;

        if (halfMaxCoreStuckTime == 0)
        {
            halfMaxCoreStuckTime = uint.MaxValue;
        }

        // While we have not World::m_stopEvent, update the world
        while (!Global.sWorld.IsStopped())
        {
            uint realCurrTime = TimeHelper.GetMSTime();

            uint diff = TimeHelper.GetMSTimeDiff(realPrevTime, realCurrTime);

            if (diff < minUpdateDiff)
            {
                uint sleepTime = minUpdateDiff - diff;

                if (sleepTime >= halfMaxCoreStuckTime)
                {
                    logger.Error(LogFilter.Server, $"WorldUpdateLoop() waiting for {sleepTime} ms with MaxCoreStuckTime set to {maxCoreStuckTime} ms");
                }

                // sleep until enough time passes that we can update all timers
                Thread.Sleep((int)sleepTime);

                continue;
            }

            Global.sWorld.Update(diff);

            realPrevTime = realCurrTime;
        }
    }

    private static bool StartDB()
    {
        // Load databases
        DatabaseLoader loader = new(DatabaseTypeFlags.All);
        loader.AddDatabase(DB.Login, "Login");
        loader.AddDatabase(DB.Characters, "Character");
        loader.AddDatabase(DB.World, "World");

        if (!loader.Load())
        {
            return false;
        }

        // Get the realm Id from the configuration file
        Global.sWorld.GetRealm().Id.Index = ConfigMgr.GetOption("RealmID", 0u);

        if (Global.sWorld.GetRealm().Id.Index == 0)
        {
            logger.Error(LogFilter.Server, "Realm ID not defined in configuration file");

            return false;
        }

        logger.Info(LogFilter.ServerLoading, $"Realm running as realm ID {Global.sWorld.GetRealm().Id.Index} ");

        // Clean the database before starting
        ClearOnlineAccounts();

        // Insert version info into DB
        // TODO: worldserver: update version table
        //DB.World.Execute("UPDATE version SET core_version = '{}', core_revision = '{}'", GitRevision::GetFullVersion(), GitRevision::GetHash());        // One-time query

        Global.sWorld.LoadDBVersion();

        logger.Info(LogFilter.ServerLoading, $"Using World DB: {Global.sWorld.GetDBVersion()}");

        return true;
    }

    private static bool LoadRealmInfo()
    {
        QueryResult result = DB.Login.Query($"SELECT id, name, address, localAddress, localSubnetMask, port, icon, flag, timezone, allowedSecurityLevel, population, gamebuild FROM realmlist WHERE id = {Global.sWorld.GetRealm().Id.Index}");

        if (result.IsEmpty())
        {
            return false;
        }

        Fields fields = result.Fetch();

        Realm realm = Global.sWorld.GetRealm();

        realm.Name = fields[1].Get<string>() ?? string.Empty;

        if (!IPAddress.TryParse(result.Read<string>(2), out realm.ExternalAddress))
        {
            logger.Error(LogFilter.Server, $"Could not resolve address {result.Read<string>(2)}");
            return false;
        }

        if (!IPAddress.TryParse(result.Read<string>(3), out realm.LocalAddress))
        {
            logger.Error(LogFilter.Server, $"Could not resolve address {result.Read<string>(3)}");
            return false;
        }

        if (!IPAddress.TryParse(result.Read<string>(4), out realm.LocalSubnetMask))
        {
            logger.Error(LogFilter.Server, $"Could not resolve address {result.Read<string>(4)}");
            return false;
        }

        realm.Port = result.Read<ushort>(5);
        realm.Type = result.Read<byte>(6);
        realm.Flags = (RealmFlags)result.Read<byte>(7);
        realm.Timezone = result.Read<byte>(8);
        realm.AllowedSecurityLevel = (AccountTypes)result.Read<byte>(9);
        realm.PopulationLevel = result.Read<float>(10);
        realm.Build = result.Read<uint>(11);

        return true;
    }

    private static void ClearOnlineAccounts()
    {
        // Reset online status for all accounts with characters on the current realm
        DB.Login.DirectExecute("UPDATE account SET online = 0 WHERE online = {0}", Global.sWorld.GetRealm().Id.Index);

        // Reset online status for all characters
        DB.Characters.DirectExecute("UPDATE characters SET online = 0 WHERE online <> 0");
    }

    private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
    {
        Exception ex = (Exception)e.ExceptionObject;

        logger.Fatal(LogFilter.Server, ex);
    }

    private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;

        ExitNow();
    }

    private static void ExitNow(int exitCode = 0)
    {
        Console.WriteLine("Halting process...");

        // TODO: WorldServer::ExitNow do some Cleannig stuff.
        //sWorld->KickAll();              // save and kick all players
        //sWorld->UpdateSessions(1);      // real players unload required UpdateSessions call

        WorldSocketManager.Instance.StopNetwork();

        // Clean database before leaving
        ClearOnlineAccounts();

        Environment.Exit(exitCode);
    }
}
