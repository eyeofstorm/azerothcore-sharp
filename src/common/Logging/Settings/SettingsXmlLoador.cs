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
using System.Xml;
using System.Xml.Serialization;

namespace AzerothCore.Logging;

class SettingsXmlLoador<T>
    where T : class, new()
{
    internal T? Data { get; set; }

    public string? SettingsFilePath { get; private set; }

    public void Load(string fileFullPath)
    {
        SettingsFilePath = fileFullPath;

        LoadXml(fileFullPath);
    }

    public void Reload()
    {
        if (!string.IsNullOrEmpty(SettingsFilePath))
        {
            LoadXml(SettingsFilePath);
        }
    }

    protected virtual void LoadXml(string fileFullPath)
    {
        if (File.Exists(fileFullPath))
        {
            FileStream fs;
            XmlReader? xmlReader = null;

            try
            {
                fs = new FileStream(fileFullPath, FileMode.Open);
                xmlReader = new XmlTextReader(fs);

                XmlSerializer serializer = new XmlSerializer(typeof(T));

                if (serializer.CanDeserialize(xmlReader))
                {
                    object? obj = serializer.Deserialize(xmlReader);

                    if (obj != null)
                    {
                        Data = (T)obj;
                    }
                }
            }
            catch (Exception e)
            {
                Trace.Write(e.StackTrace);
                throw;
            }
            finally
            {
                if (xmlReader != null)
                {
                    xmlReader.Close();
                }
            }
        }
        else
        {
            throw new FileNotFoundException(fileFullPath);
        }
    }
}
