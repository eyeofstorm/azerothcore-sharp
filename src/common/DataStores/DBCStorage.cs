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

namespace AzerothCore.DataStores;

public sealed class DBCStorage<T> where T : DBCFileEntry, new()
{
    private readonly string _name;
    private readonly Dictionary<uint, DBCFileEntry> _entries;

    public int Count
    {
        get
        {
            return _entries.Keys.Count;
        }
    }

    public DBCStorage(string name, Dictionary<uint, DBCFileEntry> entries)
    {
        _name = name;
        _entries = entries;
    }

    public string GetName() { return _name; }

    public static DBCStorage<T>? LoadDBC(string dataPath, string filename, string fieldFormat) 
    {
        string fullPath = Path.Combine(dataPath, "dbc", filename);

        DBCFileLoader.Load(fullPath, filename, fieldFormat, out DBCStorage<T>? storage);

        return storage;
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
