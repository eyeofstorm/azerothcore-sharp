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
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AzerothCore.Logging;

internal static class LogFileHelper
{
    internal static string GetFileNameHash(string fileFullPath)
    {
        byte[] data = Encoding.UTF8.GetBytes(fileFullPath);

        SHA1 sha1 = SHA1.Create();
        byte[] bs = sha1.ComputeHash(data);
        sha1.Clear();

        string result = BitConverter.ToString(bs).ToLower().Replace("-", "");

        return result;
    }

    internal static long GetLogFileSizeLong(string? logMaxSizeConf)
    {
        long logSize = Consts.LogFile.DEFAULT_LOG_MAX_FILE_SIZE;

        try
        {
            if (!long.TryParse(logMaxSizeConf, out logSize))
            {
                if (logMaxSizeConf?.EndsWith("m", true, System.Globalization.CultureInfo.InvariantCulture) ?? false)
                {
                    logMaxSizeConf = logMaxSizeConf?.TrimEnd('m', 'M');

                    if (long.TryParse(logMaxSizeConf, out logSize))
                    {
                        logSize = logSize * 1024 * 1024;
                    }
                }
            }
        }
        catch (Exception)
        {
            // Do nothing. 
        }

        return logSize;
    }

    internal static long GetRollingLogFileSizeLong(string? logMaxSizeConf)
    {
        long logSize = Consts.LogFile.DEFAULT_ROLLING_LOG_MAX_FILE_SIZE;

        try
        {
            if (!long.TryParse(logMaxSizeConf, out logSize))
            {
                if (logMaxSizeConf?.EndsWith("m", true, System.Globalization.CultureInfo.InvariantCulture) ?? false)
                {
                    logMaxSizeConf = logMaxSizeConf.TrimEnd('m', 'M');

                    if (long.TryParse(logMaxSizeConf, out logSize))
                    {
                        logSize = logSize * 1024 * 1024;
                    }
                }
            }
        }
        catch (Exception)
        {
            // Do nothing. 
        }

        return logSize;
    }

    internal static int GetCurrentBackupIndex(FileInfo logFileInfo)
    {
        int maxBackupIndex = 0;
        string pathSearchPattern = string.Format("{0}{1}", logFileInfo.Name, ".*");

        FileInfo[]? logFilesInfo =
            logFileInfo.Directory?.GetFiles(pathSearchPattern, SearchOption.TopDirectoryOnly);

        if (logFilesInfo != null)
        {
            foreach (FileInfo file in logFilesInfo)
            {

                int fileIndex;

                if (int.TryParse(file.Extension.Replace(".", ""), out fileIndex))
                {
                    maxBackupIndex = Math.Max(maxBackupIndex, fileIndex);
                }
            }
        }

        return maxBackupIndex;
    }

    internal static void RollFile(FileInfo fi, int maxBackupIndex)
    {
        string filename;
        int curBakIdx = GetCurrentBackupIndex(fi);

        if (curBakIdx >= maxBackupIndex)
        {

            curBakIdx--;

            filename = string.Format("{0}.{1}",
                                     fi.FullName,
                                     maxBackupIndex);

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }

        for (int i = curBakIdx; i >= 1; i--)
        {

            string srcFilePath = string.Format("{0}.{1}", fi.FullName, i);
            string dstFilePath = string.Format("{0}.{1}", fi.FullName, i + 1);

            try
            {
                File.Move(srcFilePath, dstFilePath);
            }
            catch (Exception e)
            {
                Trace.Write(e.StackTrace);
                continue;
            }
        }

        filename = string.Format("{0}.{1}", fi.FullName, 1);
        File.Move(fi.FullName, filename);
    }

    internal static LogLevel GetLogLevel(string logLevelString)
    {
        LogLevel logLevel;

        switch (logLevelString.ToUpper())
        {
            case "TRACE":
                logLevel = LogLevel.Trace;
                break;
            case "DEBUG":
                logLevel = LogLevel.Debug;
                break;
            case "INFO":
                logLevel = LogLevel.Info;
                break;
            case "WARN":
                logLevel = LogLevel.Warn;
                break;
            case "ERROR":
                logLevel = LogLevel.Error;
                break;
            case "FATAL":
                logLevel = LogLevel.Fatal;
                break;
            default:
                logLevel = LogLevel.Info;
                break;
        }

        return logLevel;
    }
}
