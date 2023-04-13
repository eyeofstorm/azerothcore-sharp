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
using System.Linq;
using System.Reflection;
using System.Threading;

namespace AzerothCore.Logging;

internal sealed class LoggerThread : IDisposable, IHotConfigurable
{
    private Thread m_workThread;

    private EventWaitHandle m_hasDataEvent;

    private EventWaitHandle m_stopEvent;

    private Queue<LogItem> m_logItemQueue;

    private List<AppenderBase> m_appenders;

    private static readonly object s_lockObj = new object();

    private static readonly object s_hotConfLockObj = new object();

    private static volatile LoggerThread? s_instance;

    LoggerThread()
    {
        m_workThread = new Thread(ConsumeLogItem);
        m_workThread.IsBackground = true;

        m_hasDataEvent = new AutoResetEvent(false);
        m_stopEvent = new AutoResetEvent(false);

        m_logItemQueue = new Queue<LogItem>();

        m_appenders = CreateAppenders(GetAppenderInfoList());

        Start();
    }

    internal static LoggerThread GetInstance()
    {
        if (s_instance == null)
        {
            lock (s_lockObj)
            {
                if (s_instance == null)
                {
                    s_instance = new LoggerThread();
                }
            }
        }

        return s_instance;
    }

    internal void EnqueueLogItem(LogItem logitem)
    {
        lock (s_lockObj)
        {
            m_logItemQueue.Enqueue(logitem);
            m_hasDataEvent.Set();
        }
    }

    internal List<AppenderInfo> GetAppenderInfoList()
    {
        List<AppenderInfo>? appenderList = LoggerSettings.GetInstance().Configuration?.Appenders?.Where(
            (appender) =>
            {
                AppenderRefInfo? appenderRefInfo = LoggerSettings.GetInstance().Configuration?.Logger?.AppenderRefs?.FirstOrDefault(
                    (appenderRef) =>
                    {
                        return appenderRef.AppenderName == appender.AppenderName;
                    }
                );

                return appenderRefInfo != null;
            }
        ).ToList();

        return appenderList ?? new List<AppenderInfo>();
    }

    internal void ConsumeLogItem()
    {
        try
        {
            LogItem logItem;

            while (true)
            {
                if (m_hasDataEvent.WaitOne(-1))
                {
                    while (true)
                    {
                        lock (s_lockObj)
                        {
                            if (m_logItemQueue.Count > 0)
                            {
                                try
                                {
                                    logItem = m_logItemQueue.Dequeue();
                                }
                                catch (InvalidOperationException)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        lock (s_hotConfLockObj)
                        {
                            foreach (var appender in m_appenders)
                            {
                                try
                                {
                                    appender.Append(logItem);
                                }
                                catch (ThreadAbortException)
                                {
                                    throw;
                                }
                                catch (Exception e)
                                {
                                    Trace.Write(e.StackTrace);
                                    continue;
                                }
                            }
                        }
                    }
                }

                if (m_stopEvent.WaitOne(0))
                {
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Trace.Write(e.StackTrace);
        }
    }

    internal void Start()
    {
        if (m_workThread != null)
        {
            m_workThread.Start();
        }
    }

    internal void Stop()
    {
        if (m_workThread != null)
        {

            m_stopEvent.Set();
            m_hasDataEvent.Set();

            m_workThread.Join(3000);
        }
    }

    internal void Release()
    {

        m_hasDataEvent.Close();
        m_stopEvent.Close();

        lock (s_lockObj)
        {
            m_logItemQueue.Clear();
        }

        ReleaseAppenders();
    }

    internal List<AppenderBase> CreateAppenders(List<AppenderInfo> appendersInfo)
    {
        List<AppenderBase> appenders = new List<AppenderBase>();

        foreach (AppenderInfo info in appendersInfo)
        {
            try
            {
                if (!string.IsNullOrEmpty(info.AppenderClassFullName))
                {
                    string assemblyName =
                        string.IsNullOrEmpty(info.AssemblyName) ?
                            Assembly.GetCallingAssembly().GetName().FullName : info.AssemblyName;

                    string typeName = Assembly.CreateQualifiedName(assemblyName, info.AppenderClassFullName);
                    Type? typeOfAppender = Type.GetType(typeName);

                    if (typeOfAppender != null)
                    {
                        object[] param = { info };

                        AppenderBase? appender =
                            Activator.CreateInstance(typeOfAppender, param) as AppenderBase;

                        if (appender != null)
                        {
                            appenders.Add(appender);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Trace.Write(e.StackTrace);
            }
        }

        return appenders;
    }

    void ReleaseAppenders()
    {
        lock (s_hotConfLockObj)
        {
            foreach (IAppender appender in m_appenders)
            {
                appender.Close();
            }

            m_appenders.Clear();
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
                try
                {
                    Stop();

                    Release();
                }
                catch (Exception e)
                {
                    Trace.Write(e.StackTrace);
                }
            }

            m_isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
    #endregion

    #region IHotConfigurable
    public void ApplyNewConfiguration(LoggerConfiguration newConfig)
    {
        ReleaseAppenders();

        lock (s_hotConfLockObj)
        {
            m_appenders = CreateAppenders(newConfig.Appenders ?? new List<AppenderInfo>());
        }
    }
    #endregion
}
