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

using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using AzerothCore.Configuration;
using AzerothCore.Logging;
using AzerothCore.Singleton;
using AzerothCore.Utilities;

namespace AzerothCore.Game;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LogHeader
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] Signature;

    public UInt16 FormatVersion;
    public byte SnifferId;
    public UInt32 Build;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] Locale;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
    public byte[] SessionKey;

    public UInt32 SniffStartUnixtime;
    public UInt32 SniffStartTicks;
    public UInt32 OptionalDataSize;

    public LogHeader()
    {
        Signature = new byte[3];
        Locale = new byte[4];
        SessionKey = new byte[40];
    }
}

// used to uniquely identify a connection
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct OptionalData
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] SocketIPBytes;
    public UInt32 SocketPort;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PacketHeader
{
    public UInt32       Direction;
    public UInt32       ConnectionId;
    public UInt32       ArrivalTicks;
    public UInt32       OptionalDataSize;
    public UInt32       Length;
    public OptionalData OptionalData;
    public UInt32       Opcode;
}

public class PacketLog : Singleton<PacketLog>
{
    private static readonly object _logPacketLock;
    private static FileStream? _file;

    static PacketLog()
    {
        _logPacketLock = new object();

        Initialize();
    }

    private static void Initialize()
    {
        string logsDir = ConfigMgr.GetValueOrDefault("LogsDir", "");

        if (!string.IsNullOrEmpty(logsDir))
        {
            string logname = ConfigMgr.GetValueOrDefault("PacketLogFile", "");

            if (!string.IsNullOrEmpty(logname))
            {
                string fullPath = Path.Combine(logsDir, logname);

                try
                {
                    _file = File.Open(fullPath, FileMode.Create);
                }
                catch (Exception e)
                {
                    LoggerFactory.GetLogger().Error(LogFilter.Misc, e);
                    return;
                }

                if (_file != null)
                {
                    LogHeader header = new LogHeader();

                    header.Signature[0] = (byte)'P';
                    header.Signature[1] = (byte)'K';
                    header.Signature[2] = (byte)'T';

                    header.FormatVersion = 0x0301;
                    header.SnifferId = (byte)'T';
                    header.Build = 12340;

                    header.Locale[0] = (byte)'e';
                    header.Locale[1] = (byte)'n';
                    header.Locale[2] = (byte)'U';
                    header.Locale[3] = (byte)'S';

                    Array.Fill<byte>(header.SessionKey, 0, 0, header.SessionKey.Length);

                    header.SniffStartUnixtime = (uint)Time.UnixTime;
                    header.SniffStartTicks = Time.GetMSTime();
                    header.OptionalDataSize = 0;

                    using(var writer = new BinaryWriter(_file))
                    {
                        writer.Write(header.ToByteArray());
                        writer.Flush();
                    }
                }
            }
        }
    }

    public bool CanLogPacket()
    {
        return _file != null;
    }

    public void LogPacket(WorldPacket packet, Direction direction, IPEndPoint? ip)
    {
        if (_file == null)
        {
            return;
        }

        if (ip == null)
        {
            return;
        }

        lock (_logPacketLock)
        {
            PacketHeader header = MarshalHelper.CreateStructInstance<PacketHeader>();

            header.Direction = (direction == Direction.CLIENT_TO_SERVER) ? (UInt32)0x47534d43 : (UInt32)0x47534d53;
            header.ConnectionId = 0;
            header.ArrivalTicks = Time.GetMSTime();
            header.OptionalDataSize = (uint)Marshal.SizeOf(typeof(OptionalData));

            IPAddress addr = ip.Address;
            Array.Copy(addr.GetAddressBytes(), header.OptionalData.SocketIPBytes, addr.GetAddressBytes().Length);

            header.OptionalData.SocketPort = (uint)ip.Port;
            header.Length = packet.GetSize() + sizeof(UInt32);
            header.Opcode = packet.Opcode;

            using (var writer = new BinaryWriter(_file))
            {
                writer.Write(header.ToByteArray());

                if (packet.GetSize() > 0)
                {
                    writer.Write(packet.GetData());
                }

                writer.Flush();
            }
        }
    }
}
