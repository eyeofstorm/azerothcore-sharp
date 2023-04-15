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
using System.Reflection;

namespace AzerothCore.Singleton;

public abstract class Singleton<T> where T : class
{
    private static volatile T? _instance;
    private static readonly object syncRoot = new();

    protected Singleton() { }

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (syncRoot)
                {
                    if (_instance == null)
                    {
                        ConstructorInfo? constructorInfo = typeof(T).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);

                        if (constructorInfo == null)
                        {
                            throw new InvalidOperationException("GetConstructor");
                        }

                        _instance = (T)constructorInfo.Invoke(Array.Empty<object>());
                    }
                }
            }

            return _instance;
        }
    }
}
