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

/// <summary>
/// ログ出力実装クラスのインタフェース
/// </summary>
public interface IAppender
{
    /// <summary>
    /// 名前
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// ログを出力する
    /// </summary>
    /// <param name="logItem">ログ出力内容</param>
    void Append(LogItem logItem);

    /// <summary>
    /// リソースを開放する。
    /// </summary>
    void Close();
}
