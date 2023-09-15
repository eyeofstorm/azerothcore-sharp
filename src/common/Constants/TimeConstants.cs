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


namespace AzerothCore.Constants;

public static class TimeConstants
{
    // TimeConstants
    public static readonly int MINUTE = 60;
    public static readonly int HOUR = MINUTE* 60;
    public static readonly int DAY = HOUR* 24;
    public static readonly int WEEK = DAY* 7;
    public static readonly int MONTH = DAY* 30;
    public static readonly int YEAR = MONTH* 12;
    public static readonly int IN_MILLISECONDS = 1000;
}

