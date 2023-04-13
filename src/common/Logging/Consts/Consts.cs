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

internal static class Consts
{
    internal static class ErrorCode
    {
        internal static readonly int R_NORMAL = 0;

        internal static readonly int R_FILEREAD_ERR = 2103;

        internal static readonly int R_FILEDELETE_ERR = 2105;

        internal static readonly int R_ENV_PARA = 2501;

        internal static readonly int R_EXP_ERR = 2900;
    }

    internal static class LogFile
    {
        internal static readonly long DEFAULT_LOG_MAX_FILE_SIZE = 30 * 1024 * 1024;

        internal static readonly long DEFAULT_ROLLING_LOG_MAX_FILE_SIZE = 1024 * 1024;

        internal static readonly int DEFAULT_MAX_BACKUP_INDEX = 30;
    }
}
