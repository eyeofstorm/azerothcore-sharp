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

using AzerothCore.Constants;
using AzerothCore.Utilities;

namespace AzerothCore.Game;

public partial class WorldSession
{
    private uint _accountId;
    private string _accountName;
    private WorldSocket _socket;
    private AccountTypes _security;
    private byte _expansion;
    private long _muteTime;
    private Locale _sessionDbLocaleIndex;
    private uint _recruiterId;
    private bool _isRecruiter;
    private bool _skipQueue;
    private uint _totalTime;

    public WorldSession(uint id, string account, WorldSocket worldSocket, AccountTypes security, byte expansion, long muteTime, Locale locale, uint recruiter, bool isARecruiter, bool skipQueue, uint totalTime)
    {
        _accountId = id;
        _accountName = account;
        _socket = worldSocket;
        _security = security;
        _expansion = expansion;
        _muteTime = muteTime;
        _sessionDbLocaleIndex = locale;
        _recruiterId = recruiter;
        _isRecruiter = isARecruiter;
        _skipQueue = skipQueue;
        _totalTime = totalTime;
    }

    internal void ResetTimeOutTime(bool onlyActive)
    {
        // TODO: game: WorldSession::ResetTimeOutTime(bool onlyActive)
    }

    internal void QueuePacket(WorldPacketData packetToQueue)
    {
        // TODO: game: WorldSession::QueuePacket(WorldPacketData packetToQueue)
    }

    internal object GetPlayerInfo()
    {
        // TODO: game: WorldSession::GetPlayerInfo()
        return "Dummy";
    }

    internal void SetLatency(uint latency)
    {
        // TODO: game: WorldSession::SetLatency(uint latency)
    }

    internal AccountTypes GetSecurity()
    {
        // TODO: game: WorldSession::GetSecurity()
        return AccountTypes.SEC_PLAYER;
    }

    internal void InitWarden(byte[] sessionKey, string? os)
    {
        // TODO: game: WorldSession::InitWarden(byte[] sessionKey, string? os)
    }

    internal void ReadAddonsInfo(ByteBuffer addonInfo)
    {
        // TODO: game: WorldSession::ReadAddonsInfo(ByteBuffer addonInfo)
    }
}
