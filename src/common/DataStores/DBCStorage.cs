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

using AzerothCore.Constants;

namespace AzerothCore.DataStores;

public sealed class DBCStorage<T> where T : DBCFileEntry, new()
{
    private readonly string _fieldFormat;
    private readonly Dictionary<uint, DBCFileEntry> _entries;

    public string FieldFormat
    {
        get
        {
            return _fieldFormat;
        }
    }

    public Dictionary<uint, DBCFileEntry> Entries
    {
        get
        {
            return _entries;
        }
    }

    public int Count
    {
        get
        {
            return _entries.Keys.Count;
        }
    }

    public DBCStorage(string fieldFormat)
    {
        _fieldFormat = fieldFormat;
        _entries = new Dictionary<uint, DBCFileEntry>();
    }

    public static void LoadDBC(ref uint availableDbcLocales, DBCStorage<T> storage, string dbcPath, string filename, string? dbTable = null) 
    {
        string dbcFileName = Path.Combine(dbcPath, filename);

        if (DBCFileLoader.LoadFromFile(storage, dbcFileName))
        {
            for (byte i = 0; i < (byte)Locale.TOTAL_LOCALES; ++i)
            {
                if ((availableDbcLocales & (1 << i)) == 0)
                {
                    continue;
                }

                string localizedDBCFileName = Path.Combine(dbcPath, SharedConst.LocaleNames[i], filename);

                if (!DBCFileLoader.LoadStringsFromFile(storage, localizedDBCFileName))
                {
                    availableDbcLocales &= ~(1U << i);             // mark as not available for speedup next checks
                }
            }
        }

        if (dbTable != null)
        {
            DBCFileLoader.LoadFromDB(storage, dbTable);
        }
    }

    public T? LookupEntry(uint id)
    {
        if (_entries.Keys.Count > 0)
        {
            if (_entries.ContainsKey(id))
            {
                return (T?)_entries[id];
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }
}
