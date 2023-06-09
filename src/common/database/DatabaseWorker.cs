﻿/*
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

using AzerothCore.Threading;
using System.Threading;

namespace AzerothCore.Database;

public interface ISqlOperation
{
    bool Execute<T>(MySqlBase<T> mySqlBase) where T : notnull;
}

class DatabaseWorker<T> where T : notnull
{
    private Thread _workerThread;
    private volatile bool _cancelationToken;
    private ProducerConsumerQueue<ISqlOperation> _sqlOperationQueues;
    private MySqlBase<T> _mySqlBase;

    public DatabaseWorker(ProducerConsumerQueue<ISqlOperation> newQueue, MySqlBase<T> mySqlBase)
    {
        _sqlOperationQueues = newQueue;
        _mySqlBase = mySqlBase;
        _cancelationToken = false;

        _workerThread = new Thread(WorkerThread);
        _workerThread.Start();
    }

    private void WorkerThread()
    {
        if (_sqlOperationQueues == null)
        {
            return;
        }

        while(true)
        {
            ISqlOperation? sqlOperation;

            _sqlOperationQueues.WaitAndPop(out sqlOperation);

            if (_cancelationToken || sqlOperation == null)
            {
                return;
            }

            sqlOperation.Execute(_mySqlBase);
        }
    }
}
