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

using System.Collections.Concurrent;
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
    protected static readonly ILogger logger = LoggerFactory.GetLogger();

    private Socket _socket;
    private IPEndPoint? _remoteIPEndPoint;
    private SocketAsyncEventArgs _receiveSocketAsyncEventArgs;
    private SocketAsyncEventArgs _sendSocketAsyncEventArgs;
    private volatile bool _closing;
    private volatile bool _isWritingAsync;

    private MessageBuffer _readBuffer;

    private ConcurrentQueue<MessageBuffer> _writeQueue;

    protected SocketBase(Socket socket)
    {
        _socket = socket;
        _remoteIPEndPoint = (IPEndPoint?)_socket.RemoteEndPoint;

        _closing = false;
        _isWritingAsync = false;

        _readBuffer = new ();

        _writeQueue = new ();

        _receiveSocketAsyncEventArgs = new ();
        _receiveSocketAsyncEventArgs.Completed += (sender, args) => ReadHandlerInternal(args);

        _sendSocketAsyncEventArgs = new ();
        _receiveSocketAsyncEventArgs.Completed += (sender, args) => WriteHandlerWrapper(args);
    }

    public abstract void Start();

    public virtual bool Update()
    {
        if (_socket == null || !_socket.Connected)
        {
            return false;
        }

        if (_isWritingAsync || (_writeQueue.IsEmpty && !_closing))
        {
            return true;
        }

        for (; HandleQueue(););

        return true;
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

    public void QueuePacket(MessageBuffer buffer)
    {
        _writeQueue.Enqueue(buffer);
    }

    public MessageBuffer GetReadBuffer()
    {
        return _readBuffer;
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

    public virtual void Dispose()
    {
        _socket.Dispose();
    }

    public bool IsOpen()
    {
        return _socket.Connected && !_closing;
    }

    public void DelayedCloseSocket()
    {
        _closing = true;
    }

    protected virtual void OnClose() { Dispose();}

    protected abstract void ReadHandler();

    protected bool AsyncProcessQueue()
    {
        if (_isWritingAsync)
        {
            return false;
        }

        _isWritingAsync = true;
        _sendSocketAsyncEventArgs.SetBuffer(Array.Empty<byte>());

        if (_socket.SendAsync(_sendSocketAsyncEventArgs))
        {
            WriteHandlerWrapper(_sendSocketAsyncEventArgs);
        }

        return true;
    }

    protected void SetNoDelay(bool enable)
    {
        _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, enable);
    }

    private void ReadHandlerInternal(SocketAsyncEventArgs args)
    {
        if (args.SocketError != SocketError.Success)
        {
            CloseSocket();
            return;
        }

        if (args.BytesTransferred == 0)
        {
            CloseSocket();
            return;
        }

        _readBuffer.WriteCompleted(args.BytesTransferred);

        ReadHandler();
    }

    private void WriteHandlerWrapper(SocketAsyncEventArgs args)
    {
        _isWritingAsync = false;

        HandleQueue();
    }

    private bool HandleQueue()
    {

        if (!_writeQueue.TryPeek(out MessageBuffer? queuedMessage))
        {
            return false;
        }

        int bytesToSent = queuedMessage.GetActiveSize();
        int bytesSent = _socket.Send(queuedMessage.GetReadPointer().GetBytes(), 0, bytesToSent, SocketFlags.None, out SocketError error);

        if (error != SocketError.Success)
        {
            if (error == SocketError.WouldBlock || error == SocketError.TryAgain)
            {
                return AsyncProcessQueue();
            }

            _writeQueue.TryDequeue(out _);

            if (_closing && _writeQueue.IsEmpty)
            {
                CloseSocket();
            }

            return false;
        }
        else if (bytesSent == 0)
        {
            _writeQueue.TryDequeue(out _);

            if (_closing && _writeQueue.IsEmpty)
            {
                CloseSocket();
            }

            return false;
        }
        else if (bytesSent < bytesToSent)
        {
            queuedMessage.ReadCompleted(bytesSent);

            return AsyncProcessQueue();
        }

        _writeQueue.TryDequeue(out _);

        if (_closing && _writeQueue.IsEmpty)
        {
            CloseSocket();
        }

        return !_writeQueue.IsEmpty;
    }
}
