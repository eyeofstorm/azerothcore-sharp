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
using System.Net;
using System.Net.Sockets;
using AzerothCore.Logging;
using AzerothCore.Utilities;

namespace AzerothCore.Networking;

public interface ISocket
{
    void Start();
    bool Update();
    bool IsOpen();
    void CloseSocket();
}

public delegate void SocketReadCallback(SocketAsyncEventArgs args);

public abstract class SocketBase : ISocket, IDisposable
{
    private static readonly ILogger logger = LoggerFactory.GetLogger();

    private Socket _socket;
    private IPEndPoint? _remoteIPEndPoint;
    private SocketAsyncEventArgs _receiveSocketAsyncEventArgs;

    private MessageBuffer _readBuffer;

    protected SocketBase(Socket socket)
    {
        _socket = socket;
        _remoteIPEndPoint = (IPEndPoint?)_socket.RemoteEndPoint;

        _readBuffer = new MessageBuffer();

        _receiveSocketAsyncEventArgs = new SocketAsyncEventArgs();
        _receiveSocketAsyncEventArgs.Completed += (sender, args) => ReadHandlerInternal(args);
    }

    public virtual void Dispose()
    {
        _socket.Dispose();
    }

    public abstract void Start();

    public virtual bool Update()
    {
        return IsOpen();
    }

    public IPEndPoint? GetRemoteIpAddress()
    {
        return _remoteIPEndPoint;
    }

    public void AsyncRead()
    {
        if (!IsOpen())
        {
            return;
        }

        _readBuffer.Normalize();
        _readBuffer.EnsureFreeSpace();

        _receiveSocketAsyncEventArgs.SetBuffer(_readBuffer.GetWritePointer());

        if (!_socket.ReceiveAsync(_receiveSocketAsyncEventArgs))
        {
            ReadHandlerInternal(_receiveSocketAsyncEventArgs);
        }
    }

    private void ReadHandlerInternal(SocketAsyncEventArgs args)
    {
        if (args.SocketError != SocketError.Success)
        {
            CloseSocket();
            return;
        }

        if (args.BytesTransferred == 0 || args.Buffer?.Length == 0)
        {
            CloseSocket();
            return;
        }

        ReadHandler();
    }

    public abstract void ReadHandler();

    public MessageBuffer GetReadBuffer()
    {
        return _readBuffer;
    }

    public void AsyncWrite(byte[] data)
    {
        if (!IsOpen())
        {
            return;
        }

        _socket.SendAsync(data);
    }

    public void CloseSocket()
    {
        if (_socket == null || !_socket.Connected)
        {
            return;
        }

        try
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }
        catch (Exception ex)
        {
            logger.Debug(LogFilter.Network, $"WorldSocket.CloseSocket: {GetRemoteIpAddress()} errored when shutting down socket: {ex.Message}");
        }

        OnClose();
    }

    public virtual void OnClose()
    {
        Dispose();
    }

    public bool IsOpen()
    {
        return _socket.Connected;
    }

    public void SetNoDelay(bool enable)
    {
        _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, enable);
    }
}
