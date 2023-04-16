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

namespace AzerothCore.Logging;

public sealed class LoggerFactory
{
    private static readonly ConcurrentDictionary<string, ILogger> s_loggers;

    static LoggerFactory()
    {
        s_loggers = new ConcurrentDictionary<string, ILogger>();
    }

    public static ILogger GetLogger(String name = "ROOT")
    {
        if (s_loggers.TryGetValue(name, out ILogger? value))
        {
            return value;
        }
        else
        {
            ILogger logger = new Logger();
            s_loggers[name] = logger;

            return logger;
        }
    }
}
