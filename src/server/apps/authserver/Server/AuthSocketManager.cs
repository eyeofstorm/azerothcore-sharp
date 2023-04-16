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

using System.Net.Sockets;
using System.Reflection;

using AzerothCore.Logging;

namespace AzerothCore.Networking;

internal class AuthSocketManager : SocketManager<AuthSession>
{
    private static volatile AuthSocketManager? _instance;
    private static readonly object syncRoot = new();

    private AuthSocketManager() {  }

    internal static AuthSocketManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (syncRoot)
                {
                    if (_instance == null)
                    {
                        _instance = new AuthSocketManager();
                    }
                }
            }

            return _instance;
        }
    }
}
