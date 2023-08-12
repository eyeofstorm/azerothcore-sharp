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

using System.Text;
using System.Text.RegularExpressions;

using AzerothCore.Collections;

namespace AzerothCore.Configuration;

public partial class ConfigMgr
{
    private static Dictionary<string, string> _configList = new();

    public static bool LoadAppConfigs(string fileName)
    {
        string path = AppContext.BaseDirectory + fileName;

        if (!File.Exists(path))
        {
            Console.WriteLine("{0} doesn't exist!", fileName);
            return false;
        }

        string[] ConfigContent = File.ReadAllLines(path, Encoding.UTF8);
        int lineCounter = 0;

        try
        {
            Regex sectionRegex = SectionDefineLineRegex();

            foreach (var line in ConfigContent)
            {
                lineCounter++;

                if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("-"))
                {
                    continue;
                }

                // match section
                MatchCollection matches = sectionRegex.Matches(line);

                if (matches.Count > 0)
                {
                    continue;
                }

                var configOption = new StringArray(line, '=');
                _configList.Add(configOption[0].Trim(), configOption[1].Replace("\"", "").Trim());
            }
        }
        catch
        {
            Console.WriteLine("Error in {0} on Line {1}", fileName, lineCounter);
            return false;
        }

        return true;
    }

    public static T GetValueOrDefault<T>(string name, T defaultValue) 
    {
        string? temp = _configList.LookupByKey(name);

        Type type = typeof(T).IsEnum ? typeof(T).GetEnumUnderlyingType() : typeof(T);

        if (temp == null || temp.IsEmpty())
        {
            return defaultValue;
        }

        if (Type.GetTypeCode(typeof(T)) == TypeCode.Boolean && temp.IsNumber())
        {
            return (T)Convert.ChangeType(temp == "1", typeof(T));
        }

        return (T)Convert.ChangeType(temp, type);
    }

    public static IEnumerable<string> GetKeysByString(string name)
    {
        return _configList.Where(p => p.Key.Contains(name)).Select(p => p.Key);
    }

    [GeneratedRegex("\\s*\\[.+\\]\\s*")]
    private static partial Regex SectionDefineLineRegex();
}
