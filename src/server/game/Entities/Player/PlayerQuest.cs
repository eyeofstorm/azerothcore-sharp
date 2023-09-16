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

namespace AzerothCore.Game;

public partial class Player
{
    public void MoneyChanged(uint count)
    {
        // TODO: game: Player::MoneyChanged(uint count)

        //for (byte i = 0; i < MAX_QUEST_LOG_SIZE; ++i)
        //{
        //    uint questid = GetQuestSlotQuestId(i);

        //    if (!questid)
        //    {
        //        continue;
        //    }

        //    Quest? qInfo = Global.sObjectMgr.GetQuestTemplate(questid);

        //    if (qInfo != null)
        //    {
        //        int rewOrReqMoney = qInfo.GetRewOrReqMoney();

        //        if (rewOrReqMoney < 0)
        //        {
        //            QuestStatusData questStatus = _questStatus[questid];

        //            if (questStatus.Status == QUEST_STATUS_INCOMPLETE)
        //            {
        //                if ((int)count >= -rewOrReqMoney)
        //                {
        //                    if (CanCompleteQuest(questid))
        //                    {
        //                        CompleteQuest(questid);
        //                    }
        //                }
        //            }
        //            else if (_questStatus.Status == QUEST_STATUS_COMPLETE)
        //            {
        //                if ((int)count < -rewOrReqMoney)
        //                {
        //                    IncompleteQuest(questid);
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
