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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace AzerothCore.Logging;

public sealed class AppenderInfo
{
    [XmlAttribute("name")]
    public string? AppenderName { get; set; }

    [XmlAttribute("level")]
    public string? Level { get; set; }

    [XmlAttribute("class")]
    public string? AppenderClassFullName { get; set; }

    [XmlAttribute("assembly")]
    public string? AssemblyName { get; set; }

    [XmlElement("file")]
    public string? FilePath { get; set; }

    [XmlElement("max-file-size")]
    public string? MaxFileSize { get; set; }

    [XmlElement("max-backup-index")]
    public string? MaxBackupIndex;

    [XmlElement("pattern")]
    public string? Pattern { get; set; }
}

public sealed class AppenderRefInfo
{
    [XmlAttribute("ref")]
    public string? AppenderName { get; set; }
}

public sealed class LoggerInfo
{
    [XmlAttribute("level")]
    public string? Level { get; set; }

    [XmlElement("appender-ref")]
    public AppenderRefInfo[]? AppenderRefs { get; set; }
}

[XmlRoot("configuration")]
public sealed class LoggerConfiguration
{
    private static readonly object s_lockObj = new object();

    private static LoggerConfiguration? s_defaultValue;

    [XmlElement("appender")]
    public List<AppenderInfo>? Appenders { get; set; }

    [XmlElement("logger")]
    public LoggerInfo? Logger { get; set; }

    public static LoggerConfiguration DefaultValue
    {
        get
        {
            if (s_defaultValue == null)
            {
                lock (s_lockObj)
                {
                    if (s_defaultValue == null)
                    {
                        s_defaultValue = new LoggerConfiguration();

                        s_defaultValue.Logger = new LoggerInfo()
                        {
                            Level = "INFO",

                            AppenderRefs = new AppenderRefInfo[]
                            {
                                new AppenderRefInfo()
                                {
                                    AppenderName = "STDOUT"
                                }
                            }
                        };

                        s_defaultValue.Appenders = new List<AppenderInfo>();

                        AppenderInfo stdoutAppender = new AppenderInfo()
                        {
                            AppenderName = "STDOUT",
                            Level = "INFO",
                            AppenderClassFullName = "AzerothCoreSharp.Common.Logging.ConsoleAppender",
                            Pattern = "%d{yyyy/MM/dd HH:mm:ss.fff} [%level] - %message %newline"
                        };

                        s_defaultValue.Appenders.Add(stdoutAppender);
                    }
                }
            }

            return s_defaultValue;
        }
    }
}

internal delegate void LoggerSettingsChangedEventHandler(object sender, LoggerSettingsChangedEventArgs args);

internal sealed class LoggerSettings : IDisposable
{
    private static readonly object s_lockObj = new object();

    private static volatile LoggerSettings? s_instance;

    private readonly SettingsXmlLoador<LoggerConfiguration> m_loador = new SettingsXmlLoador<LoggerConfiguration>();

    private LoggerConfiguration? m_configuration;

    private FileSystemWatcher m_settingsXmlFileWatcher = new FileSystemWatcher();

    internal LoggerConfiguration? Configuration
    {
        get
        {
            lock (s_lockObj)
            {
                return m_configuration;
            }
        }

        set
        {
            lock (s_lockObj)
            {
                m_configuration = value;
            }
        }
    }

    internal event LoggerSettingsChangedEventHandler? LoggerSettingsChanged;

    internal static LoggerSettings GetInstance()
    {
        if (s_instance == null)
        {
            lock (s_lockObj)
            {
                if (s_instance == null)
                {
                    s_instance = new LoggerSettings();
                }
            }
        }

        return s_instance;
    }

    LoggerSettings()
    {
        try
        {
            string? workDir = Assembly.GetExecutingAssembly().Location;
            string fullPath = string.Empty;
            FileInfo fi = new FileInfo(workDir);
            string confFileName = "Logger.xml";

            workDir = fi.Directory?.FullName;
            fullPath = Path.Combine(workDir ?? "", confFileName);

            m_loador.Load(fullPath);
            m_configuration = m_loador.Data;

            m_settingsXmlFileWatcher.Path = workDir ?? ".";
            m_settingsXmlFileWatcher.Filter = confFileName;
            m_settingsXmlFileWatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.Size;
            m_settingsXmlFileWatcher.Changed += new FileSystemEventHandler(OnLoggerSettingsXmlFileChanged);
            m_settingsXmlFileWatcher.EnableRaisingEvents = true;
        }
        catch (Exception e)
        {
            Trace.Write(e.StackTrace);
            Configuration = LoggerConfiguration.DefaultValue;
        }
    }

    internal void RegisteLoggerSettingsChangedEventHandler(LoggerSettingsChangedEventHandler handler)
    {
        LoggerSettingsChanged += handler;
    }

    private void OnLoggerSettingsXmlFileChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            m_loador.Reload();
            Configuration = m_loador.Data;

        }
        catch (Exception ex)
        {

            Trace.Write(ex.StackTrace);
            Configuration = LoggerConfiguration.DefaultValue;
        }

        if (LoggerSettingsChanged != null)
        {
            LoggerSettingsChanged(this, new LoggerSettingsChangedEventArgs(Configuration ?? LoggerConfiguration.DefaultValue));
        }
    }

    #region IDisposable
    bool m_isDisposed = false;

    void Dispose(bool disposing)
    {
        if (!m_isDisposed)
        {

            if (disposing)
            {
                m_settingsXmlFileWatcher.Dispose();
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
