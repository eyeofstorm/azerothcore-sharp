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

namespace AzerothCore.Game;

using System.Collections.Generic;

public struct InstanceTemplate
{
    public uint Parent;
    public uint ScriptId;
    public bool AllowMount;
}

public class Map
{
    private HashSet<BaseObject> _updateObjects;

    public Map(uint id, uint instanceId, byte spawnMode, Map? parent = null)
    {
        _updateObjects = new HashSet<BaseObject>();
    }

    internal void AddUpdateObject(BaseObject obj)
    {
        _updateObjects.Add(obj);
    }

    internal void RemoveUpdateObject(BaseObject obj)
    {
        _updateObjects.Remove(obj);
    }
}
