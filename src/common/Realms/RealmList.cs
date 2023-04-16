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

using System.Collections.Concurrent;
using System.Net;
using System.Timers;

using AzerothCore.Constants;
using AzerothCore.Database;
using AzerothCore.Logging;
using AzerothCore.Singleton;

namespace AzerothCore.Realms;

public class RealmList : Singleton<RealmList>
{
    private static readonly ILogger logger = LoggerFactory.GetLogger();

    private List<RealmBuildInfo>                        _builds = new();
    private ConcurrentDictionary<RealmHandle, Realm>    _realms = new();
    private System.Timers.Timer?                        _updateTimer;

    private RealmList() { }

    public void Initialize(int updateInterval)
    {
        LoadBuildInfo();

        UpdateRealms(null, null);

        _updateTimer = new System.Timers.Timer(TimeSpan.FromSeconds(updateInterval).TotalMilliseconds);
        _updateTimer.Elapsed += UpdateRealms;
        _updateTimer.Start();
    }

    public void Close()
    {
        _updateTimer?.Close();
    }

    void LoadBuildInfo()
    {
        //                                         0             1             2              3              4      5                6
        SQLResult result = DB.Login.Query("SELECT majorVersion, minorVersion, bugfixVersion, hotfixVersion, build, winChecksumSeed, macChecksumSeed FROM build_info ORDER BY build ASC");

        if (!result.IsEmpty())
        {
            do
            {
                RealmBuildInfo buildInfo = new();

                uint    majorVersion        = result.Read<uint>(0);
                uint    minorVersion        = result.Read<uint>(1);
                uint    bugfixVersion       = result.Read<uint>(2);
                string? hotfixVersion       = result.Read<string>(3);
                uint    build               = result.Read<uint>(4);
                string? win64AuthSeedHexStr = result.Read<string>(5);
                string? mac64AuthSeedHexStr = result.Read<string>(6);

                buildInfo.MajorVersion = majorVersion;
                buildInfo.MinorVersion = minorVersion;
                buildInfo.BugfixVersion = bugfixVersion;

                if (hotfixVersion != null && hotfixVersion.Length <= buildInfo.HotfixVersion.Length)
                {
                    buildInfo.HotfixVersion = hotfixVersion.ToCharArray();
                }

                buildInfo.Build = build;

                if (win64AuthSeedHexStr != null && win64AuthSeedHexStr.Length == buildInfo.Win64AuthSeed.Length * 2)
                {
                    buildInfo.Win64AuthSeed = win64AuthSeedHexStr.ToByteArray();
                }

                if (mac64AuthSeedHexStr != null && mac64AuthSeedHexStr.Length == buildInfo.Mac64AuthSeed.Length * 2)
                {
                    buildInfo.Mac64AuthSeed = mac64AuthSeedHexStr.ToByteArray();
                }

                _builds.Add(buildInfo);

            } while (result.NextRow());
        }
    }

    void UpdateRealm(Realm realm)
    {
        var oldRealm = _realms.LookupByKey(realm.Id);

        if (oldRealm != null && oldRealm == realm)
        {
            return;
        }

        _realms[realm.Id] = realm;
    }

    void UpdateRealms(object? source, ElapsedEventArgs? e)
    {
        PreparedStatement               stmt            = LoginDatabase.GetPreparedStatement(LoginStatements.LOGIN_SEL_REALMLIST);
        SQLResult                       result          = DB.Login.Query(stmt);
        Dictionary<RealmHandle, string> existingRealms  = new();

        foreach (var p in _realms)
        {
            existingRealms[p.Key] = p.Value.Name;
        }

        _realms.Clear();

        // Circle through results and add them to the realm map
        if (!result.IsEmpty())
        {
            do
            {
                var realm = new Realm();

                realm.Id                = new RealmHandle(result.Read<uint>(0));;
                realm.Name              = result.Read<string>(1) ?? string.Empty;
                realm.ExternalAddress   = IPAddress.Parse(result.Read<string>(2) ?? string.Empty);
                realm.LocalAddress      = IPAddress.Parse(result.Read<string>(3) ?? string.Empty);
                realm.LocalSubnetMask   = IPAddress.Parse(result.Read<string>(4) ?? string.Empty);
                realm.Port              = result.Read<ushort>(5);

                RealmType realmType = (RealmType)result.Read<byte>(6);

                if (realmType == RealmType.REALM_TYPE_FFA_PVP)
                {
                    realmType = RealmType.REALM_TYPE_PVP;
                }

                if (realmType >= RealmType.MAX_CLIENT_REALM_TYPE)
                {
                    realmType = RealmType.REALM_TYPE_NORMAL;
                }

                realm.Type = (byte)realmType;

                realm.Flags = (RealmFlags)result.Read<byte>(7);
                realm.Timezone = result.Read<byte>(8);

                AccountTypes allowedSecurityLevel = (AccountTypes)result.Read<byte>(9);
                realm.AllowedSecurityLevel = (allowedSecurityLevel <= AccountTypes.SEC_ADMINISTRATOR ? allowedSecurityLevel : AccountTypes.SEC_ADMINISTRATOR);

                realm.PopulationLevel = result.Read<float>(10);

                realm.Build = result.Read<uint>(11);

                

                UpdateRealm(realm);

                if (!existingRealms.ContainsKey(realm.Id))
                {
                    logger.Info(LogFilter.Realmlist, $"Added realm \"{realm.Name}\" at {realm.ExternalAddress.ToString()}:{realm.Port}");
                }
                else
                {
                    logger.Debug(LogFilter.Realmlist, $"Updating realm \"{realm.Name}\" at {realm.ExternalAddress.ToString()}:{realm.Port}");
                }

                existingRealms.Remove(realm.Id);
            }
            while (result.NextRow());
        }

        foreach (var pair in existingRealms)
        {
            logger.Debug(LogFilter.Realmlist, $"Removed realm \"{pair.Value}\".");
        }
    }

    public Realm? GetRealm(RealmHandle id)
    {
        return _realms.LookupByKey(id);
    }

    public RealmBuildInfo? GetBuildInfo(uint build)
    {
        foreach (var clientBuild in _builds)
        {
            if (clientBuild.Build == build)
            {
                return clientBuild;
            }
        }

        return null;
    }

    public uint GetMinorMajorBugfixVersionForBuild(uint build)
    {
        RealmBuildInfo? buildInfo = _builds.FirstOrDefault(p => p.Build < build);
        return buildInfo != null ? (buildInfo.MajorVersion * 10000 + buildInfo.MinorVersion * 100 + buildInfo.BugfixVersion) : 0;
    }

    public ICollection<Realm> GetRealms()
    {
        return _realms.Values;
    }
}

public class RealmBuildInfo
{
    public uint Build;
    public uint MajorVersion;
    public uint MinorVersion;
    public uint BugfixVersion;
    public char[] HotfixVersion = new char[4];
    public byte[] Win64AuthSeed = new byte[16];
    public byte[] Mac64AuthSeed = new byte[16];
}
