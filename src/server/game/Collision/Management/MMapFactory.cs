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

namespace AzerothCore.Game;

public static class MMapFactory
{
    private static bool[] forbiddenMaps;

    static MMapFactory()
    {
        forbiddenMaps = new bool[100];
    }

    public static MMapMgr CreateOrGetVMapMgr()
    {
        return MMapMgr.Instance;
    }

    public static void InitializeDisabledMaps()
    {
        int[] f = { 616 /*EoE*/, 649 /*ToC25*/, 650 /*ToC5*/, -1 };
        uint i = 0;

        while (f[i] >= 0)
        {
            forbiddenMaps[f[i++]] = true;
        }
    }
}
