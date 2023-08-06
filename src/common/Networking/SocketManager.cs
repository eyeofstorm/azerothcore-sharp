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

using AzerothCore.Logging;

namespace AzerothCore.Networking;

public abstract class SocketManager<SocketType> where SocketType : ISocket
{
    protected static readonly ILogger logger = LoggerFactory.GetLogger();

    protected AsyncAcceptor? _acceptor;
    protected NetworkThread<SocketType>[] _threads = Array.Empty<NetworkThread<SocketType>>();
    protected int _threadCount = 0;

    protected abstract NetworkThread<SocketType>[] CreateThreads();

    public virtual bool StartNetwork(string bindIp, int port, int threadCount = 1)
    {
        _acceptor = new AsyncAcceptor();

        if (!_acceptor.Start(bindIp, port))
        {
            logger.Error(LogFilter.Network, "StartNetwork failed to Start AsyncAcceptor");
            return false;
        }

        _threadCount = threadCount;
        _threads = CreateThreads();

        for (int i = 0; i < _threadCount; ++i)
        {
            _threads[i].Start();
        }

        _acceptor.AsyncAcceptWithCallback(OnSocketOpen);

        return true;
    }

    public virtual void StopNetwork()
    {
        _acceptor?.Close();

        if (_threadCount != 0)
        {
            for (int i = 0; i < _threadCount; ++i)
            {
                _threads[i].Stop();
            }
        }

        Wait();

        _acceptor = null;
        _threads = Array.Empty<NetworkThread<SocketType>>(); ;
        _threadCount = 0;
    }

    public void Wait()
    {
        if (_threadCount != 0)
        {
            for (int i = 0; i < _threadCount; ++i)
            {
                _threads[i].Wait();
            }
        }
    }

    public virtual void OnSocketOpen(Socket sock)
    {
        try
        {
            SocketType? newSocket = (SocketType?)Activator.CreateInstance(typeof(SocketType), sock);

            if (newSocket != null)
            {
                newSocket.Start();
                _threads[SelectThreadWithMinConnections()].AddSocket(newSocket);
            }
        }
        catch (Exception err)
        {
            logger.Fatal(LogFilter.Network, err);
        }
    }

    public int GetNetworkThreadCount()
    {
        return _threadCount;
    }

    public uint SelectThreadWithMinConnections()
    {
        uint min = 0;

        for (uint i = 1; i < _threadCount; ++i)
        {
            if (_threads[i].GetConnectionCount() < _threads[min].GetConnectionCount())
            { 
                min = i;
            }
        }

        return min;
    }
}
