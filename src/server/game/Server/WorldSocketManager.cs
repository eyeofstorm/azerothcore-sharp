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

using AzerothCore.Configuration;
using AzerothCore.Logging;
using AzerothCore.Networking;

namespace AzerothCore.Game.Server;

public class WorldSocketThread : NetworkThread<WorldSocket>
{
    override protected void SocketAdded(WorldSocket sock) 
    {
        sock.SetSendBufferSize(WorldSocketManager.Instance.GetApplicationSendBufferSize());

        // TODO: game: WorldSocketThread::SocketAdded() Script Hook
    }

    override protected void SocketRemoved(WorldSocket sock) 
    {
        // TODO: game: WorldSocketThread::SocketRemoved() Script Hook
    }
}

public class WorldSocketManager : SocketManager<WorldSocket>
{
    private static volatile WorldSocketManager? _instance;
    private static readonly object _syncRoot = new();

    private bool _tcpNoDelay;
    private int _socketSystemSendBufferSize;
    private int _socketApplicationSendBufferSize;

    public int GetApplicationSendBufferSize()
    {
        return _socketApplicationSendBufferSize;
    }

    private WorldSocketManager()
    {
        _tcpNoDelay = true;
        _socketSystemSendBufferSize = -1;
        _socketApplicationSendBufferSize = 65535;
    }

    public static WorldSocketManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_syncRoot)
                {
                    _instance ??= new WorldSocketManager();
                }
            }

            return _instance;
        }
    }

    public override bool StartNetwork(string bindIp, int port, int threadCount = 1)
    {
        _tcpNoDelay = ConfigMgr.GetValueOrDefault("Network.TcpNodelay", true);

        int max_connections = 128;
        logger.Debug(LogFilter.Network, $"Max allowed socket connections {max_connections}");

        // -1 means use default
        _socketSystemSendBufferSize = ConfigMgr.GetValueOrDefault("Network.OutKBuff", -1);
        _socketApplicationSendBufferSize = ConfigMgr.GetValueOrDefault("Network.OutUBuff", 65536);

        if (_socketApplicationSendBufferSize <= 0)
        {
            logger.Debug(LogFilter.Network, $"Network.OutUBuff is wrong in your config file");
            return false;
        }

        if (!base.StartNetwork(bindIp, port, threadCount))
        {
            return false;
        }

        _acceptor?.AsyncAcceptWithCallback(OnSocketAccept);

        // TODO: game: WorldSocketManager::StartNetwork(string bindIp, int port, int threadCount) Script Hook

        return true;
    }

    public override void StopNetwork()
    {
        base.StopNetwork();

        // TODO: game: WorldSocketManager::StopNetwork() Script Hook
    }

    public override void OnSocketOpen(Socket sock)
    {
        // set some options here
        if (_socketSystemSendBufferSize >= 0)
        {
            try
            {
                sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, _socketSystemSendBufferSize);
            }
            catch (Exception e)
            {
                logger.Error(LogFilter.Network, $"WorldSocketMgr::OnSocketOpen sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer) err = {e.Message}");
                return;
            }
        }

        // Set TCP_NODELAY.
        if (_tcpNoDelay)
        {
            try
            {
                sock.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            }
            catch (Exception e)
            {
                logger.Error(LogFilter.Network, $"WorldSocketMgr::OnSocketOpen sock.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay err = {e.Message}");
                return;
            }
        }

        base.OnSocketOpen(sock);
    }

    protected override NetworkThread<WorldSocket>[] CreateThreads()
    {
        NetworkThread<WorldSocket>[] threads = new WorldSocketThread[GetNetworkThreadCount()];

        for (int i = 0; i < GetNetworkThreadCount(); ++i)
        {
            threads[i] = new WorldSocketThread();
        }
        
        return threads;
    }

    private void OnSocketAccept(Socket newSocket)
    {
        OnSocketOpen(newSocket);
    }
}
