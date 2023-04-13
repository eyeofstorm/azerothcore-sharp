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

using System;
using System.Collections;

namespace AzerothCore.Collections;

public class StringArray
{
    public int Length => _str != null ? _str.Length : 0;

    private string[] _str;

    public StringArray(int size)
    {
        _str = new string[size];

        for (var i = 0; i < size; ++i)
        {
            _str[i] = string.Empty;
        }
    }

    public StringArray(string str, params string[] separator)
    {
        _str = new string[0];

        if (str.IsEmpty())
        {
            return;
        }

        _str = str.Split(separator, StringSplitOptions.TrimEntries);
    }

    public StringArray(string str, params char[] separator)
    {
        _str = Array.Empty<string>();

        if (str.IsEmpty())
        {
            return;
        }

        _str = str.Split(separator, StringSplitOptions.TrimEntries);
    }

    public string this[int index]
    {
        get { return _str[index]; }
        set { _str[index] = value; }
    }

    public IEnumerator GetEnumerator()
    {
        return _str.GetEnumerator();
    }

    public bool IsEmpty()
    {
        return _str == null || _str.Length == 0;
    }
}
