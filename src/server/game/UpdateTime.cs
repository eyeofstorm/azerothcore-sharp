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

using AzerothCore.Configuration;
using AzerothCore.Logging;
using AzerothCore.Utilities;

namespace AzerothCore.Game;

public class UpdateTime
{
    protected static readonly ILogger logger = LoggerFactory.GetLogger();

    private uint[] _updateTimeDataTable = new uint[500];
    private uint _averageUpdateTime;
    private uint _totalUpdateTime;
    private uint _updateTimeTableIndex;
    private uint _maxUpdateTime;
    private uint _maxUpdateTimeOfLastTable;
    private uint _maxUpdateTimeOfCurrentTable;

    private uint _recordedTime;

    public uint GetAverageUpdateTime()
    {
        return _averageUpdateTime;
    }

    public uint GetTimeWeightedAverageUpdateTime()
    {
        uint sum = 0, weightsum = 0;

        foreach (uint diff in _updateTimeDataTable)
        {
            sum += diff * diff;
            weightsum += diff;
        }

        if (weightsum == 0)
        {
            return 0;
        }

        return sum / weightsum;
    }

    public uint GetMaxUpdateTime()
    {
        return _maxUpdateTime;
    }

    public uint GetMaxUpdateTimeOfCurrentTable()
    {
        return Math.Max(_maxUpdateTimeOfCurrentTable, _maxUpdateTimeOfLastTable);
    }

    public uint GetLastUpdateTime()
    {
        return _updateTimeDataTable[_updateTimeTableIndex != 0 ? _updateTimeTableIndex - 1 : _updateTimeDataTable.Length - 1u];
    }

    public void UpdateWithDiff(uint diff)
    {
        _totalUpdateTime = _totalUpdateTime - _updateTimeDataTable[_updateTimeTableIndex] + diff;
        _updateTimeDataTable[_updateTimeTableIndex] = diff;

        if (diff > _maxUpdateTime)
        {
            _maxUpdateTime = diff;
        }

        if (diff > _maxUpdateTimeOfCurrentTable)
        {
            _maxUpdateTimeOfCurrentTable = diff;
        }

        if (++_updateTimeTableIndex >= _updateTimeDataTable.Length)
        {
            _updateTimeTableIndex = 0;
            _maxUpdateTimeOfLastTable = _maxUpdateTimeOfCurrentTable;
            _maxUpdateTimeOfCurrentTable = 0;
        }

        if (_updateTimeDataTable[^1] != 0)
            _averageUpdateTime = (uint)(_totalUpdateTime / _updateTimeDataTable.Length);
        else if (_updateTimeTableIndex != 0)
            _averageUpdateTime = _totalUpdateTime / _updateTimeTableIndex;
    }

    public void RecordUpdateTimeReset()
    {
        _recordedTime = TimeHelper.GetMSTime();
    }

    public void RecordUpdateTimeDuration(string text, uint minUpdateTime)
    {
        uint thisTime = TimeHelper.GetMSTime();
        uint diff = TimeHelper.GetMSTimeDiff(_recordedTime, thisTime);

        if (diff > minUpdateTime)
        {
            logger.Info(LogFilter.Misc, $"Recored Update Time of {text}: {diff}.");
        }

        _recordedTime = thisTime;
    }
}

public class WorldUpdateTime : UpdateTime
{
    private uint _recordUpdateTimeInverval;
    private uint _recordUpdateTimeMin;
    private uint _lastRecordTime;

    public void LoadFromConfig()
    {
        _recordUpdateTimeInverval = ConfigMgr.GetOption("RecordUpdateTimeDiffInterval", 60000u);
        _recordUpdateTimeMin = ConfigMgr.GetOption("MinRecordUpdateTimeDiff", 100u);
    }

    public void SetRecordUpdateTimeInterval(uint t)
    {
        _recordUpdateTimeInverval = t;
    }

    public void RecordUpdateTime(uint gameTimeMs, uint diff, uint sessionCount)
    {
        if (_recordUpdateTimeInverval > 0 && diff > _recordUpdateTimeMin)
        {
            if (TimeHelper.GetMSTimeDiff(_lastRecordTime, gameTimeMs) > _recordUpdateTimeInverval)
            {
                logger.Debug(LogFilter.Misc, $"Update time diff: {GetAverageUpdateTime()}. Players online: {sessionCount}.");
                _lastRecordTime = gameTimeMs;
            }
        }
    }

    public void RecordUpdateTimeDuration(string text)
    {
        RecordUpdateTimeDuration(text, _recordUpdateTimeMin);
    }
}
