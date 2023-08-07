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

using System.Net;
using System.Net.Sockets;

using AzerothCore.Logging;

namespace AzerothCore.Networking;

public delegate void SocketAcceptDelegate(Socket newSocket);

public class AsyncAcceptor
{
    private static readonly ILogger logger = LoggerFactory.GetLogger();

    private TcpListener? _listener;
    private volatile bool _closed;

    public bool Start(string ip, int port)
    {
        if (!IPAddress.TryParse(ip, out IPAddress? bindIP))
        {
            logger.Error(LogFilter.Network, $"Server can't be started: Invalid IP-Address: {ip}");
            return false;
        }

        try
        {
            _listener = new TcpListener(bindIP, port);
            _listener.Start();
        }
        catch (SocketException ex)
        {
            logger.Fatal(LogFilter.Network, ex);
            return false;
        }

        return true;
    }

    public async void AsyncAcceptWithCallback(SocketAcceptDelegate mgrHandler)
    {
        try
        {
            Socket? socket = null;

            if (_listener != null)
            {
                socket = await _listener.AcceptSocketAsync();
            }

            if (socket != null)
            {
                mgrHandler(socket);

                if (!_closed)
                {
                    AsyncAcceptWithCallback(mgrHandler);
                }
            }
        }
        catch (ObjectDisposedException ex)
        {
            logger.Fatal(LogFilter.Network, ex);
        }
    }

    //public async void AsyncAccept<T>() where T : ISocket
    //{
    //    try
    //    {
    //        Socket? socket = null;

    //        if (_listener != null)
    //        {
    //            socket = await _listener.AcceptSocketAsync();
    //        }

    //        if (socket != null)
    //        {
    //            T? newSocket = (T?)Activator.CreateInstance(typeof(T), socket);

    //            newSocket?.Start();

    //            if (!_closed)
    //            {
    //                AsyncAccept<T>();
    //            }
    //        }
    //    }
    //    catch (ObjectDisposedException ex)
    //    {
    //        logger.Fatal(LogFilter.Network, ex);
    //    }
    //}

    public void Close()
    {
        if (_closed)
        {
            return;
        }

        _closed = true;
    }
}
