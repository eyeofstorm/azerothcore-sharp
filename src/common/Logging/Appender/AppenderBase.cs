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

namespace AzerothCore.Logging;

public abstract class AppenderBase : IAppender
{
    private string m_appenderName;

    private readonly LogLevel m_lowestLevel;

    private readonly PatternLayout m_layout;

    public AppenderBase(AppenderInfo? appenderInfo) : this()
    {
        m_appenderName = appenderInfo?.AppenderName ?? "";
        m_layout = new PatternLayout(appenderInfo?.Pattern ?? PatternLayout.DEFAULT_PATTERN);
        m_lowestLevel = LogFileHelper.GetLogLevel(appenderInfo?.Level ?? "INFO");
    }

    AppenderBase()
    {
        m_lowestLevel = LogLevel.Info;
        m_appenderName = string.Empty;
        m_layout = new PatternLayout(PatternLayout.DEFAULT_PATTERN);
    }

    AppenderBase(string appendName, PatternLayout patternLayout, LogLevel logLevel)
    {
        m_appenderName = appendName;
        m_layout = patternLayout;
        m_lowestLevel = logLevel;
    }

    protected abstract void AppendLogMessage(LogItem logItem);

    protected abstract void CloseAppender();

    protected void WriteFormattedLogMessage(StreamWriter writer, LogItem logItem)
    {
        try
        {
            string? logMsg = m_layout.FormatLogItem(logItem);

            if (string.IsNullOrEmpty(logMsg))
            {
                logMsg = logItem?.Message;
            }

            writer.Write(logMsg);
            writer.Flush();
        }
        catch (Exception e)
        {
            Trace.WriteLine(e.StackTrace);
        }
    }

    protected virtual void CreateLogMessage(StreamWriter writer, LogItem logItem)
    {
        WriteFormattedLogMessage(writer, logItem);
    }

    #region IAppender実装
    public string Name
    {
        get
        {
            return m_appenderName;
        }

        set
        {
            m_appenderName = value;
        }
    }

    public void Append(LogItem logItem)
    {
        try
        {
            if ((int)m_lowestLevel >= (int)logItem.LogLevel)
            {
                AppendLogMessage(logItem);
            }
        }
        catch
        {
            // do nothing.
        }
    }

    public void Close()
    {
        try
        {
            CloseAppender();
        }
        catch
        {
            // do nothing.
        }
    }
    #endregion
}
