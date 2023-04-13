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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace AzerothCore.Logging;

internal class PatternLayout
{
    internal const string DEFAULT_PATTERN = "%d{yyyy/MM/dd HH:mm:ss.fff} [%level] - %message %newline";

    private readonly string m_pattern = "%d{yyyy/MM/dd HH:mm:ss.fff} [%level] [%method] - %message %newline";

    private readonly SortedDictionary<int, KeyValuePair<string, Match>> m_logPartsIndexDic;

    private PatternLayout()
    {
        m_logPartsIndexDic = new SortedDictionary<int, KeyValuePair<string, Match>>();
    }

    internal PatternLayout(string pattern) : this()
    {
        m_pattern = pattern;

        AddParts("date", @"%d{(?<dateformat>.+)}");
        AddParts("level", @"%level");
        AddParts("method", @"%method");
        AddParts("message", @"%message");
        AddParts("newline", @"%newline");
    }

    private PatternLayout AddParts(string key, string partsPattern)
    {
        Regex regEx = new Regex(partsPattern);
        MatchCollection matches = regEx.Matches(m_pattern);

        foreach (Match match in matches)
        {
            m_logPartsIndexDic[match.Index] = new KeyValuePair<string, Match>(key, match);
        }

        return this;
    }

    public string FormatLogItem(LogItem logItem)
    {
        string methodName = string.Empty;

        if (logItem.Caller != null)
        {
            MethodBase? methodInfo = logItem.Caller.GetMethod();

            if (methodInfo != null)
            {
                if (methodInfo.DeclaringType != null)
                {
                    methodName = string.Format(
                                            "{0}.{1}",
                                            methodInfo.DeclaringType?.Name,
                                            methodInfo.Name);
                }
                else
                {
                    methodName = methodInfo.Name;
                }
            }
        }

        StringBuilder sb = new StringBuilder();
        int preMatchIndex = 0;
        int preMatchCount = 0;

        foreach (int key in m_logPartsIndexDic.Keys)
        {
            string type = m_logPartsIndexDic[key].Key;
            Match curMatch = m_logPartsIndexDic[key].Value;
            string part = string.Empty;

            if (curMatch.Index > 0)
            {
                part = m_pattern.Substring(preMatchIndex + preMatchCount, curMatch.Index - preMatchIndex - preMatchCount);
            }

            sb.Append(part);

            switch (type)
            {
                case "date":
                    string dateFormat = curMatch.Groups["dateformat"].Value.Trim();
                    string dateString = logItem.LogDateTime.ToString(dateFormat);
                    part = dateString;
                    break;
                case "level":
                    part = logItem.LogLevel.ToString().ToUpper().PadLeft(5, ' ');
                    break;
                case "method":
                    part = methodName;
                    break;
                case "message":
                    part = logItem.Message;
                    break;
                case "newline":
                    part = Environment.NewLine;
                    break;
                default:
                    part = string.Empty;
                    break;
            }

            sb.Append(part);

            preMatchIndex = curMatch.Index;
            preMatchCount = curMatch.Length;
        }

        if (preMatchIndex > 0)
        {
            string remains = m_pattern.Substring(preMatchIndex + preMatchCount);
            sb.Append(remains);
        }

        return sb.ToString();
    }
}
