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
using System.Text;
using System.Threading;

namespace AzerothCore.Logging;

/// <summary>
/// ファイルにログ出力用クラス
/// </summary>
public class DefaultFileAppender : AppenderBase, IDisposable
{
    /// <summary>
    /// クロース済みフラグ
    /// </summary>
    private volatile bool m_closed;

    /// <summary>
    /// ログファイルサイズの上限
    /// </summary>
    private long m_maxFileSize;

    /// <summary>
    /// ログファイルのフルパス
    /// </summary>
    private string? m_filePath;

    /// <summary>
    /// ログ出力同期用Mutex
    /// </summary>
    private readonly Mutex m_loggerWriteMutex;

    /// <summary>
    /// コントラクター
    /// </summary>
    /// <param name="appenderInfo">Appender info.</param>
    public DefaultFileAppender(AppenderInfo? appenderInfo) : base(appenderInfo)
    {
        m_maxFileSize = LogFileHelper.GetLogFileSizeLong(appenderInfo?.MaxFileSize);

        m_filePath = appenderInfo?.FilePath;

        string mutexName = string.Format(@"Global\{0}", LogFileHelper.GetFileNameHash(m_filePath ?? ""));
        m_loggerWriteMutex = new Mutex(false, mutexName);

        // 最大サイズを超えた既存のログファイルは削除
        if (File.Exists(m_filePath))
        {
            if (m_maxFileSize <= new FileInfo(m_filePath).Length)
            {
                try
                {
                    File.Delete(m_filePath);
                    string msg =
                            string.Format("ログファイルサイズが最大値[{0}MB]を超えていたため既存ファイルを削除しました。",
                                          m_maxFileSize / 1024L / 1024L);

                    LogItem log = new LogItem
                    {
                        Caller = new StackFrame(0),
                        LogDateTime = DateTime.Now,
                        LogLevel = LogLevel.Info,
                        Message = msg
                    };

                    AppendLogItem(log);
                }
                catch
                {
                    // 何もしない
                }
            }
        }
    }

    /// <summary>
    /// Appends the log item.
    /// </summary>
    /// <param name="logItem">Log item.</param>
    void AppendLogItem(LogItem logItem)
    {
        if (m_closed)
        {
            return;
        }

        m_loggerWriteMutex.WaitOne();

        try
        {
            FileStream fs = new FileStream(m_filePath ?? "", FileMode.Append, FileAccess.Write, FileShare.None);

            using (StreamWriter logWriter = new StreamWriter(fs, Encoding.UTF8))
            {
                WriteFormattedLogMessage(logWriter, logItem);
            }
        }
        finally
        {
            m_loggerWriteMutex.ReleaseMutex();
        }
    }

    /// <summary>
    /// Appends the log message.
    /// </summary>
    /// <param name="logItem">Log item.</param>
    protected override void AppendLogMessage(LogItem logItem)
    {
        AppendLogItem(logItem);
    }

    /// <summary>
    /// リソースを開放
    /// </summary>
    protected override void CloseAppender()
    {
        m_loggerWriteMutex.Close();
        m_closed = true;
    }

    #region IDisposable 実装
    /// <summary>
    /// 重複コールテスト用
    /// </summary>
    bool m_isDisposed = false;

    /// <summary>
    /// リソースを開放する。
    /// </summary>
    /// <param name="disposing">リソースが開放中であるか</param>
    void Dispose(bool disposing)
    {
        if (!m_isDisposed)
        {

            if (disposing)
            {
                m_loggerWriteMutex.Close();
            }

            m_isDisposed = true;
        }
    }

    /// <summary>
    /// リソースを開放する。
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }
    #endregion
}
