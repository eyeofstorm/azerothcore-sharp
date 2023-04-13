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

namespace AzerothCore.Logging;

public interface ILogger
{
    bool IsTraceEnabled { get; }

    bool IsDebugEnabled { get; }

    bool IsErrorEnabled { get; }

    bool IsFatalEnabled { get; }

    bool IsInfoEnabled { get; }

    bool IsWarnEnabled { get; }

    void Trace(LogFilter filter, string message);
    void Trace(LogFilter filter, Exception e);

    void Debug(LogFilter filter, string message);
    void Debug(LogFilter filter, Exception e);

    void Error(LogFilter filter, string message);
    void Error(LogFilter filter, Exception e);

    void Fatal(LogFilter filter, string message);
    void Fatal(LogFilter filter, Exception e);

    void Info(LogFilter filter, string message);
    void Info(LogFilter filter, Exception e);

    void Warn(LogFilter filter, string message);
    void Warn(LogFilter filter, Exception e);
}
