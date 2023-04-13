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
using System.Text;

namespace AzerothCore.Logging;

public sealed class Logger : ILogger, IDisposable
{
    private LogLevel m_rootLogLevel;

    private readonly LoggerThread m_loggerThread;

    public Logger()
    {
        LoggerSettings settings = LoggerSettings.GetInstance();

        m_rootLogLevel = LogFileHelper.GetLogLevel(settings.Configuration?.Logger?.Level?.Trim() ?? "INFO");

        m_loggerThread = LoggerThread.GetInstance();

        settings.RegisteLoggerSettingsChangedEventHandler(LoggerSettingsChanged);
    }

    private void LoggerSettingsChanged(object sender, LoggerSettingsChangedEventArgs args)
    {
        m_rootLogLevel = LogFileHelper.GetLogLevel(args.NewConfigration.Logger?.Level?.Trim() ?? "INFO");
        (m_loggerThread as IHotConfigurable).ApplyNewConfiguration(args.NewConfigration);
    }

    void AppendLog(LogLevel level, string message)
    {
        LogItem item = new LogItem
        {
            LogDateTime = DateTime.Now,
            LogLevel = level,
            Message = message,
            Caller = new StackFrame(2)
        };

        m_loggerThread.EnqueueLogItem(item);
    }

    #region ILogger implements
    public bool IsTraceEnabled
    {
        get { return (int)m_rootLogLevel >= (int)LogLevel.Trace; }
    }

    public bool IsDebugEnabled
    {
        get { return (int)m_rootLogLevel >= (int)LogLevel.Debug; }
    }

    public bool IsFatalEnabled
    {
        get { return (int)m_rootLogLevel >= (int)LogLevel.Fatal; }
    }

    public bool IsErrorEnabled
    {
        get
        {
            return (int)m_rootLogLevel >= (int)LogLevel.Error;
        }
    }

    public bool IsWarnEnabled
    {
        get { return (int)m_rootLogLevel >= (int)LogLevel.Warn; }
    }

    public bool IsInfoEnabled
    {
        get
        {
            return m_rootLogLevel >= (int)LogLevel.Info;
        }
    }

    public void Trace(LogFilter filter, string message)
    {
        if (IsTraceEnabled)
        {
            AppendLog(LogLevel.Trace, message);
        }
    }

    public void Trace(LogFilter filter, Exception e)
    {
        if (IsTraceEnabled)
        {
            StringBuilder logMsg = new();

            logMsg
                .AppendLine(e.Message)
                .AppendLine(e.StackTrace);

            if (e.InnerException is not null)
            {
                logMsg
                    .AppendLine(e.InnerException.Message)
                    .AppendLine(e.InnerException.StackTrace);
            }

            AppendLog(LogLevel.Trace, logMsg.ToString());
        }
    }

    public void Debug(LogFilter filter, string message)
    {
        if (IsDebugEnabled)
        {
            AppendLog(LogLevel.Debug, message);
        }
    }

    public void Debug(LogFilter filter, Exception e)
    {
        if (IsDebugEnabled)
        {
            StringBuilder logMsg = new();

            logMsg
                .AppendLine(e.Message)
                .AppendLine(e.StackTrace);

            if (e.InnerException is not null)
            {
                logMsg
                    .AppendLine(e.InnerException.Message)
                    .AppendLine(e.InnerException.StackTrace);
            }

            AppendLog(LogLevel.Debug, logMsg.ToString());
        }
    }

    public void Error(LogFilter filter, string message)
    {
        if (IsErrorEnabled)
        {
            AppendLog(LogLevel.Error, message);
        }
    }

    public void Error(LogFilter filter, Exception e)
    {
        if (IsErrorEnabled)
        {
            StringBuilder logMsg = new();

            logMsg
                .AppendLine(e.Message)
                .AppendLine(e.StackTrace);

            if (e.InnerException is not null)
            {
                logMsg
                    .AppendLine(e.InnerException.Message)
                    .AppendLine(e.InnerException.StackTrace);
            }

            AppendLog(LogLevel.Error, logMsg.ToString());
        }
    }

    public void Fatal(LogFilter filter, string message)
    {
        if (IsFatalEnabled)
        {
            AppendLog(LogLevel.Fatal, message);
        }
    }

    public void Fatal(LogFilter filter, Exception e)
    {
        if (IsFatalEnabled)
        {
            StringBuilder logMsg = new();

            logMsg
                .AppendLine(e.Message)
                .AppendLine(e.StackTrace);

            if (e.InnerException is not null)
            {
                logMsg
                    .AppendLine(e.InnerException.Message)
                    .AppendLine(e.InnerException.StackTrace);
            }

            AppendLog(LogLevel.Fatal, logMsg.ToString());
        }
    }

    public void Info(LogFilter filter, string message)
    {
        if (IsInfoEnabled)
        {
            AppendLog(LogLevel.Info, message);
        }
    }

    public void Info(LogFilter filter, Exception e)
    {
        if (IsInfoEnabled)
        {
            StringBuilder logMsg = new();

            logMsg
                .AppendLine(e.Message)
                .AppendLine(e.StackTrace);

            if (e.InnerException is not null)
            {
                logMsg
                    .AppendLine(e.InnerException.Message)
                    .AppendLine(e.InnerException.StackTrace);
            }

            AppendLog(LogLevel.Info, logMsg.ToString());
        }
    }

    public void Warn(LogFilter filter, string message)
    {
        if (IsWarnEnabled)
        {
            AppendLog(LogLevel.Warn, message);
        }
    }

    public void Warn(LogFilter filter, Exception e)
    {
        if (IsWarnEnabled)
        {
            StringBuilder logMsg = new();

            logMsg
                .AppendLine(e.Message)
                .AppendLine(e.StackTrace);

            if (e.InnerException is not null)
            {
                logMsg
                    .AppendLine(e.InnerException.Message)
                    .AppendLine(e.InnerException.StackTrace);
            }

            AppendLog(LogLevel.Warn, logMsg.ToString());
        }
    }
    #endregion

    #region IDisposable implements
    private bool m_isDisposed = false;

    void Dispose(bool disposing)
    {
        if (!m_isDisposed)
        {
            if (disposing)
            {
                m_loggerThread.Dispose();
            }

            m_isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
    #endregion
}
