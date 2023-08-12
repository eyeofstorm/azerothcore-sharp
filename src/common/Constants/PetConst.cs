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

namespace AzerothCore.Constants;

// stored in character_pet.slot
public enum PetSaveMode : sbyte
{
    PET_SAVE_AS_DELETED = -1,                                       // not saved in fact
    PET_SAVE_AS_CURRENT = 0,                                        // in current slot (with player)
    PET_SAVE_FIRST_STABLE_SLOT = 1,
    PET_SAVE_LAST_STABLE_SLOT = PetConst.MAX_PET_STABLES,           // last in DB stable slot index (including), all higher have same meaning as PET_SAVE_NOT_IN_SLOT
    PET_SAVE_NOT_IN_SLOT = 100                                      // for avoid conflict with stable size grow will use 100
}

public static class PetConst
{
    public const sbyte MAX_PET_STABLES = 4;
}
