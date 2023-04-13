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
/// <remarks></remarks>
public class RollingFileAppender : AppenderBase, IDisposable
{
    /// <summary>
    /// クロース済みフラグ
    /// </summary>
    volatile bool m_closed;

    /// <summary>
    /// ログファイルサイズの上限
    /// </summary>
    long m_maxFileSize;

    /// <summary>
    /// The number of files would be kept.
    /// </summary>
    int m_maxBackupIndex;

    /// <summary>
    /// ログファイルのフルパス
    /// </summary>
    string m_filePath;

    /// <summary>
    /// ログ出力同期用Mutex
    /// </summary>
    readonly Mutex m_loggerWriteMutex;

    /// <summary>
    /// コントラクター
    /// </summary>
    /// <param name="appenderInfo">appender information.</param>
    public RollingFileAppender(AppenderInfo? appenderInfo) : base(appenderInfo)
    {
        m_maxFileSize = LogFileHelper.GetRollingLogFileSizeLong(appenderInfo?.MaxFileSize);

        if (false == int.TryParse(appenderInfo?.MaxBackupIndex, out m_maxBackupIndex))
        {
            m_maxBackupIndex = Consts.LogFile.DEFAULT_MAX_BACKUP_INDEX;
        }

        FileInfo fi = new FileInfo(appenderInfo?.FilePath ?? "");
        m_filePath = fi.FullName;

        string mutexName = string.Format(@"Global\{0}", LogFileHelper.GetFileNameHash(m_filePath));
        m_loggerWriteMutex = new Mutex(false, mutexName);
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
            FileInfo fi = new(m_filePath);

            if (fi.Directory != null && !fi.Directory.Exists)
            {
                Directory.CreateDirectory(fi.Directory.FullName);
            }

            FileStream fs = new FileStream(m_filePath, FileMode.Append, FileAccess.Write, FileShare.None);

            using (StreamWriter logWriter = new StreamWriter(fs, Encoding.UTF8))
            {
                WriteFormattedLogMessage(logWriter, logItem);
            }

            // ファイルバックアップ
            if (fi.Length > m_maxFileSize)
            {
                LogFileHelper.RollFile(fi, m_maxBackupIndex);
            }
        }
        catch
        {
            // Do nothing.
            return;
        }
        finally
        {
            m_loggerWriteMutex.ReleaseMutex();
        }
    }

    /// <summary>
    /// Appends the log item.
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
