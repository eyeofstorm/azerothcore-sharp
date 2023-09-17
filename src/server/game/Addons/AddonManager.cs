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

using System.Security.Cryptography;
using System.Text;

using AzerothCore.Database;
using AzerothCore.Logging;
using AzerothCore.Threading;
using AzerothCore.Utilities;

namespace AzerothCore.Game;

public struct AddonInfo
{
    public string   Name;
    public byte     Enabled;
    public uint     CRC;
    public byte     State;
    public bool     UsePublicKeyOrCRC;

    public AddonInfo(string name, byte enabled, uint crc, byte state, bool crcOrPubKey)
    {
        Name = name;
        Enabled = enabled;
        CRC = crc;
        State = state;
        UsePublicKeyOrCRC = crcOrPubKey;
    }
}

public struct SavedAddon
{
    public string Name;
    public uint CRC;

    public SavedAddon(string name, uint crc)
    {
        Name = name;
        CRC = crc;
    }
}

public struct BannedAddon
{
    public uint Id;
    public byte[] NameMD5;
    public byte[] VersionMD5;
    public uint Timestamp;

    public BannedAddon()
    {
        NameMD5 = new byte[16];
        VersionMD5 = new byte[16];
    }
}

public static class AddonMgr
{
    public static readonly uint STANDARD_ADDON_CRC = 0x4c1c776d;

    private static readonly LockedQueue<SavedAddon> _knownAddons = new();
    private static readonly LockedQueue<BannedAddon> _bannedAddons = new();

    public static void LoadFromDB()
    {
        uint oldMSTime = TimeHelper.GetMSTime();

        QueryResult result = DB.Characters.Query("SELECT name, crc FROM addons");

        if (result.IsEmpty())
        {
            LoggerFactory.GetLogger().Warn(LogFilter.ServerLoading, $">> Loaded 0 known addons. DB table `addons` is empty!");
            LoggerFactory.GetLogger().Info(LogFilter.ServerLoading, $"");

            return;
        }

        uint count = 0;

        do
        {
            Fields fields = result.Fetch();

            string? name = fields[0].Get<string>();
            uint crc = fields[1].Get<uint>();

            _knownAddons.Add(new SavedAddon(name ?? string.Empty, crc));

            ++count;
        }
        while (result.NextRow());

        LoggerFactory.GetLogger().Info(LogFilter.ServerLoading, $">> Loaded {count} known addons in {TimeHelper.GetMSTimeDiffToNow(oldMSTime)} ms");
        LoggerFactory.GetLogger().Info(LogFilter.ServerLoading, $" ");

        oldMSTime = TimeHelper.GetMSTime();
        result = DB.Characters.Query("SELECT id, name, version, UNIX_TIMESTAMP(timestamp) FROM banned_addons");

        if (!result.IsEmpty())
        {
            uint count2 = 0;
            uint offset = 102;

            do
            {
                Fields fields = result.Fetch();

                BannedAddon addon = new();

                addon.Id = fields[0].Get<uint>() + offset;
                addon.Timestamp = (uint)fields[3].Get<ulong>();
                addon.NameMD5 = MD5.HashData(Encoding.ASCII.GetBytes(fields[1].Get<string>() ?? string.Empty));
                addon.VersionMD5 = MD5.HashData(Encoding.ASCII.GetBytes(fields[2].Get<string>() ?? string.Empty));

                _bannedAddons.Add(addon);

                ++count2;
            }
            while (result.NextRow());

            LoggerFactory.GetLogger().Info(LogFilter.ServerLoading, $">> Loaded {count2} banned addons in {TimeHelper.GetMSTimeDiffToNow(oldMSTime)} ms");
            LoggerFactory.GetLogger().Info(LogFilter.ServerLoading, $" ");
        }
    }

    public static void SaveAddon(AddonInfo addon)
    {
        var stmt = CharacterDatabase.GetPreparedStatement(CharStatements.CHAR_INS_ADDON);

        stmt.SetData(0, addon.Name);
        stmt.SetData(1, addon.CRC);

        DB.Characters.Execute(stmt);

        _knownAddons.Add(new SavedAddon(addon.Name, addon.CRC));
    }

    public static SavedAddon? GetAddonInfo(string name)
    {
        while (_knownAddons.Next(out SavedAddon addon))
        {
            if (addon.Name == name)
            {
                return addon;
            }
        }

        return null;
    }

    public static LockedQueue<BannedAddon> GetBannedAddons()
    {
        return _bannedAddons;
    }
}
