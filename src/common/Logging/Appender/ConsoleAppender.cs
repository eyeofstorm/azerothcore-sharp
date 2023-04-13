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
using System.IO;

namespace AzerothCore.Logging;

/// <summary>
/// 
/// </summary>
public class ConsoleAppender : AppenderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:EotS.Log.ConsoleAppender"/> class.
    /// </summary>
    /// <param name="appenderInfo">Appender info.</param>
    public ConsoleAppender(AppenderInfo appenderInfo) : base(appenderInfo)
    {
    }

    /// <summary>
    /// Appends the log message.
    /// </summary>
    /// <param name="logItem">Log item.</param>
    protected override void AppendLogMessage(LogItem logItem)
    {
        using (StreamWriter sw = new StreamWriter(Console.OpenStandardOutput()))
        {
            WriteFormattedLogMessage(sw, logItem);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void CloseAppender()
    {
        // 何もしない
        return;
    }
}
