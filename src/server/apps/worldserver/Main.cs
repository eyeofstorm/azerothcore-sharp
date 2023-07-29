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

using System.Diagnostics;
using System.Globalization;
using System.Net;

using AzerothCore.Configuration;
using AzerothCore.Constants;
using AzerothCore.Database;
using AzerothCore.Game;
using AzerothCore.Logging;

namespace AzerothCore;

internal class WorldServer
{
    private static readonly ILogger logger = LoggerFactory.GetLogger("worldserver");

    internal static void Main()
    {
        // Set Culture
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

        if (!ConfigMgr.LoadAppConfigs(Process.GetCurrentProcess().ProcessName + ".conf"))
        {
            ExitNow();
        }

        if (!StartDB())
        {
            ExitNow();
        }

        // set server offline (not connectable)
        DB.Login.DirectExecute($"UPDATE realmlist SET flag = (flag & ~{RealmFlags.REALM_FLAG_OFFLINE}) | {RealmFlags.REALM_FLAG_VERSION_MISMATCH} WHERE id = '{Global.sWorld.GetRealm().Id.Index}'");

        LoadRealmInfo();

        // Initialize the World
        Global.sWorld.SetInitialWorldSettings();

        // TODO: worldserver: Start the Remote Access port (acceptor) if enabled

        // TODO: worldserver: Launch the worldserver listener socket
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
        Global.sWorld.GetRealm().Id.Index = ConfigMgr.GetValueOrDefault("RealmID", 0u);

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

        logger.Info(LogFilter.ServerLoading, "Using World DB: {Global.WorldMgr.GetDBVersion()}");

        return true;
    }

    private static bool LoadRealmInfo()
    {
        SQLResult result = DB.Login.Query($"SELECT id, name, address, localAddress, localSubnetMask, port, icon, flag, timezone, allowedSecurityLevel, population, gamebuild FROM realmlist WHERE id = {Global.sWorld.GetRealm().Id.Index}");

        if (result.IsEmpty())
        {
            return false;
        }

        SQLFields fields = result.GetFields();

        Realm realm = Global.sWorld.GetRealm();

        realm.Name = fields.Read<string>(1) ?? string.Empty;

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
        DB.Login.DirectExecute($"UPDATE account SET online = 0 WHERE online = {Global.sWorld.GetRealm().Id.Index}");

        // Reset online status for all characters
        DB.Characters.DirectExecute("UPDATE characters SET online = 0 WHERE online <> 0");
    }

    private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
    {
        Exception ex = (Exception)e.ExceptionObject;

        logger.Fatal(LogFilter.Server, ex);
    }

    private static void ExitNow()
    {
        Console.WriteLine("Halting process...");

        Environment.Exit(-1);
    }
}

