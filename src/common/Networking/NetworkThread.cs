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

using AzerothCore.Logging;
using AzerothCore.Utilities;

namespace AzerothCore.Networking;

public class NetworkThread<TSocketType> where TSocketType : ISocket
{
    private static readonly ILogger logger = LoggerFactory.GetLogger();

    private long _connections;
    private readonly AutoResetEvent _stopEvent = new(false);

    private Thread? _thread;

    private List<TSocketType> _Sockets = new();
    private List<TSocketType> _newSockets = new();

    public void Stop()
    {
        _stopEvent.Set();
    }

    public bool Start()
    {
        if (_thread != null)
        {
            return false;
        }

        _thread = new Thread(Run);
        _thread.Start();

        return true;
    }

    public void Wait()
    {
        _thread?.Join();
        _thread = null;
    }

    public long GetConnectionCount()
    {
        return Interlocked.Read(ref _connections);
    }

    public virtual void AddSocket(TSocketType sock)
    {
        Interlocked.Increment(ref _connections);
        _newSockets.Add(sock);
        SocketAdded(sock);
    }

    protected virtual void SocketAdded(TSocketType sock) { }
    protected virtual void SocketRemoved(TSocketType sock) { }

    private void AddNewSockets()
    {
        if (_newSockets.Empty())
        {
            return;
        }

        foreach (var socket in _newSockets.ToArray())
        {
            if (!socket.IsOpen())
            {
                SocketRemoved(socket);

                Interlocked.Decrement(ref _connections);
            }
            else
            {
                _Sockets.Add(socket);
            }
        }

        _newSockets.Clear();
    }

    private void Run()
    {
        logger.Debug(LogFilter.Network, "Network Thread Starting");

        int sleepTime = 1;

        while (true)
        {
            if (_stopEvent.WaitOne(sleepTime))
            {
                break;
            }

            uint tickStart = Time.GetMSTime();

            AddNewSockets();

            for (var i = 0; i < _Sockets.Count; ++i)
            {
                TSocketType socket = _Sockets[i];

                if (!socket.Update())
                {
                    if (socket.IsOpen())
                    {
                        socket.CloseSocket();
                    }

                    SocketRemoved(socket);
                    Interlocked.Decrement(ref _connections);
                    _Sockets.Remove(socket);
                }
            }

            uint diff = Time.GetMSTimeDiffToNow(tickStart);
            sleepTime = (int)(diff > 1 ? 0 : 1 - diff);
        }

        logger.Debug(LogFilter.Network, "Network Thread exits");

        _newSockets.Clear();
        _Sockets.Clear();
    }
}
