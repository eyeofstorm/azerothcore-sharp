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
using AzerothCore.Constants;

namespace AzerothCore.Utilities;

public static class LocaleHelper
{
    public static Locale GetLocaleByName(string? name)
    {
        Locale locale;

        switch (name)
        {
            case "enUS":
                locale = Locale.LOCALE_enUS;
                break;
            case "koKR":
                locale = Locale.LOCALE_koKR;
                break;
            case "frFR":
                locale = Locale.LOCALE_frFR;
                break;
            case "deDE":
                locale = Locale.LOCALE_deDE;
                break;
            case "zhCN":
                locale = Locale.LOCALE_zhCN;
                break;
            case "zhTW":
                locale = Locale.LOCALE_zhTW;
                break;
            case "esES":
                locale = Locale.LOCALE_esES;
                break;
            case "esMX":
                locale = Locale.LOCALE_esMX;
                break;
            case "ruRU":
                locale = Locale.LOCALE_ruRU;
                break;
            default:
                locale = Locale.LOCALE_enUS;
                break;
        }

        return locale;
    }
}

