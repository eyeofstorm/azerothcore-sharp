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

namespace AzerothCore.Database;

public class CharacterDatabase : MySqlBase<CharStatements>
{
    public override void PreparedStatements()
    {
        PrepareStatement(CharStatements.CHAR_DEL_QUEST_POOL_SAVE, "DELETE FROM pool_quest_save WHERE pool_id = ?");
        PrepareStatement(CharStatements.CHAR_INS_QUEST_POOL_SAVE, "INSERT INTO pool_quest_save (pool_id, quest_id) VALUES (?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_NONEXISTENT_GUILD_BANK_ITEM, "DELETE FROM guild_bank_item WHERE guildid = ? AND TabId = ? AND SlotId = ?");
        PrepareStatement(CharStatements.CHAR_DEL_EXPIRED_BANS, "UPDATE character_banned SET active = 0 WHERE unbandate <= UNIX_TIMESTAMP() AND unbandate <> bandate");
        PrepareStatement(CharStatements.CHAR_SEL_DATA_BY_NAME, "SELECT guid, account, name, gender, race, class, level FROM characters WHERE deleteDate IS NULL AND name = ?");
        PrepareStatement(CharStatements.CHAR_SEL_DATA_BY_GUID, "SELECT guid, account, name, gender, race, class, level FROM characters WHERE deleteDate IS NULL AND guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHECK_NAME, "SELECT 1 FROM characters WHERE name = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHECK_GUID, "SELECT 1 FROM characters WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_SUM_CHARS, "SELECT COUNT(guid) FROM characters WHERE account = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_CREATE_INFO, "SELECT level, race, class FROM characters WHERE account = ? LIMIT 0, ?");
        PrepareStatement(CharStatements.CHAR_INS_CHARACTER_BAN, "INSERT INTO character_banned VALUES (?, UNIX_TIMESTAMP(), UNIX_TIMESTAMP()+?, ?, ?, 1)");
        PrepareStatement(CharStatements.CHAR_UPD_CHARACTER_BAN, "UPDATE character_banned SET active = 0 WHERE guid = ? AND active != 0");
        PrepareStatement(CharStatements.CHAR_DEL_CHARACTER_BAN, "DELETE cb FROM character_banned cb INNER JOIN characters c ON c.guid = cb.guid WHERE c.account = ?");
        PrepareStatement(CharStatements.CHAR_SEL_BANINFO, "SELECT FROM_UNIXTIME(bandate, '%Y-%m-%d %H:%i:%s'), unbandate-bandate, active, unbandate, banreason, bannedby FROM character_banned WHERE guid = ? ORDER BY bandate ASC");
        PrepareStatement(CharStatements.CHAR_SEL_GUID_BY_NAME_FILTER, "SELECT guid, name FROM characters WHERE name LIKE CONCAT('%%', ?, '%%')");
        PrepareStatement(CharStatements.CHAR_SEL_BANINFO_LIST, "SELECT bandate, unbandate, bannedby, banreason FROM character_banned WHERE guid = ? ORDER BY unbandate");
        PrepareStatement(CharStatements.CHAR_SEL_BANNED_NAME, "SELECT characters.name FROM characters, character_banned WHERE character_banned.guid = ? AND character_banned.guid = characters.guid");
        PrepareStatement(CharStatements.CHAR_SEL_ENUM, @"SELECT c.guid, c.name, c.race, c.class, c.gender, c.skin, c.face, c.hairStyle, c.hairColor, c.facialStyle, c.level, c.zone, c.map, c.position_x, c.position_y, c.position_z,
                         gm.guildid, c.playerFlags, c.at_login, cp.entry, cp.modelid, cp.level, c.equipmentCache, cb.guid, c.extra_flags 
                         FROM characters AS c LEFT JOIN character_pet AS cp ON c.guid = cp.owner AND cp.slot = ? LEFT JOIN guild_member AS gm ON c.guid = gm.guid 
                         LEFT JOIN character_banned AS cb ON c.guid = cb.guid AND cb.active = 1 WHERE c.account = ? AND c.deleteInfos_Name IS NULL ORDER BY COALESCE(c.order, c.guid)");
        PrepareStatement(CharStatements.CHAR_SEL_ENUM_DECLINED_NAME, @"SELECT c.guid, c.name, c.race, c.class, c.gender, c.skin, c.face, c.hairStyle, c.hairColor, c.facialStyle, c.level, c.zone, c.map, 
                         c.position_x, c.position_y, c.position_z, gm.guildid, c.playerFlags, c.at_login, cp.entry, cp.modelid, cp.level, c.equipmentCache, 
                         cb.guid, c.extra_flags, cd.genitive FROM characters AS c LEFT JOIN character_pet AS cp ON c.guid = cp.owner AND cp.slot = ? 
                         LEFT JOIN character_declinedname AS cd ON c.guid = cd.guid LEFT JOIN guild_member AS gm ON c.guid = gm.guid 
                         LEFT JOIN character_banned AS cb ON c.guid = cb.guid AND cb.active = 1 WHERE c.account = ? AND c.deleteInfos_Name IS NULL ORDER BY COALESCE(c.order, c.guid)");
        PrepareStatement(CharStatements.CHAR_SEL_FREE_NAME, "SELECT guid, name, at_login FROM characters WHERE guid = ? AND account = ? AND NOT EXISTS (SELECT NULL FROM characters WHERE name = ?)");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_ZONE, "SELECT zone FROM characters WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_NAME_DATA, "SELECT race, class, gender, level FROM characters WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_POSITION_XYZ, "SELECT map, position_x, position_y, position_z FROM characters WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_POSITION, "SELECT position_x, position_y, position_z, orientation, map, taxi_path FROM characters WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_QUEST_STATUS_DAILY, "DELETE FROM character_queststatus_daily");
        PrepareStatement(CharStatements.CHAR_DEL_QUEST_STATUS_WEEKLY, "DELETE FROM character_queststatus_weekly");
        PrepareStatement(CharStatements.CHAR_DEL_QUEST_STATUS_MONTHLY, "DELETE FROM character_queststatus_monthly");
        PrepareStatement(CharStatements.CHAR_DEL_QUEST_STATUS_SEASONAL, "DELETE FROM character_queststatus_seasonal WHERE event = ?");
        PrepareStatement(CharStatements.CHAR_DEL_QUEST_STATUS_DAILY_CHAR, "DELETE FROM character_queststatus_daily WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_QUEST_STATUS_WEEKLY_CHAR, "DELETE FROM character_queststatus_weekly WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_QUEST_STATUS_MONTHLY_CHAR, "DELETE FROM character_queststatus_monthly WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_QUEST_STATUS_SEASONAL_CHAR, "DELETE FROM character_queststatus_seasonal WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_BATTLEGROUND_RANDOM, "DELETE FROM character_battleground_random");
        PrepareStatement(CharStatements.CHAR_INS_BATTLEGROUND_RANDOM, "INSERT INTO character_battleground_random (guid) VALUES (?)");

        // Start LoginQueryHolder content
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER, @"SELECT guid, account, name, race, class, gender, level, xp, money, skin, face, hairStyle, hairColor, facialStyle, bankSlots, restState, playerFlags, 
                         position_x, position_y, position_z, map, orientation, taximask, cinematic, totaltime, leveltime, rest_bonus, logout_time, is_logout_resting, resettalents_cost, 
                         resettalents_time, trans_x, trans_y, trans_z, trans_o, transguid, extra_flags, stable_slots, at_login, zone, online, death_expire_time, taxi_path, instance_mode_mask, 
                         arenaPoints, totalHonorPoints, todayHonorPoints, yesterdayHonorPoints, totalKills, todayKills, yesterdayKills, chosenTitle, knownCurrencies, watchedFaction, drunk, 
                         health, power1, power2, power3, power4, power5, power6, power7, instance_id, talentGroupsCount, activeTalentGroup, exploredZones, equipmentCache, ammoId, 
                         knownTitles, actionBars, grantableLevels, innTriggerId FROM characters WHERE guid = ?");

        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_AURAS, @"SELECT casterGuid, itemGuid, spell, effectMask, recalculateMask, stackCount, amount0, amount1, amount2, 
                         base_amount0, base_amount1, base_amount2, maxDuration, remainTime, remainCharges FROM character_aura WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_SPELL, "SELECT spell, specMask FROM character_spell WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_QUESTSTATUS, @"SELECT quest, status, explored, timer, mobcount1, mobcount2, mobcount3, mobcount4, 
                         itemcount1, itemcount2, itemcount3, itemcount4, itemcount5, itemcount6, playercount FROM character_queststatus WHERE guid = ? AND status <> 0");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_DAILYQUESTSTATUS, "SELECT quest, time FROM character_queststatus_daily WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_WEEKLYQUESTSTATUS, "SELECT quest FROM character_queststatus_weekly WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_MONTHLYQUESTSTATUS, "SELECT quest FROM character_queststatus_monthly WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_SEASONALQUESTSTATUS, "SELECT quest, event FROM character_queststatus_seasonal WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_INS_CHARACTER_DAILYQUESTSTATUS, "INSERT INTO character_queststatus_daily (guid, quest, time) VALUES (?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_INS_CHARACTER_WEEKLYQUESTSTATUS, "INSERT INTO character_queststatus_weekly (guid, quest) VALUES (?, ?)");
        PrepareStatement(CharStatements.CHAR_INS_CHARACTER_MONTHLYQUESTSTATUS, "INSERT INTO character_queststatus_monthly (guid, quest) VALUES (?, ?)");
        PrepareStatement(CharStatements.CHAR_INS_CHARACTER_SEASONALQUESTSTATUS, "INSERT IGNORE INTO character_queststatus_seasonal (guid, quest, event) VALUES (?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_REPUTATION, "SELECT faction, standing, flags FROM character_reputation WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_INVENTORY, @"SELECT creatorGuid, giftCreatorGuid, count, duration, charges, flags, enchantments, randomPropertyId, durability, playedTime, text, bag, slot, 
                         item, itemEntry FROM character_inventory ci JOIN item_instance ii ON ci.item = ii.guid WHERE ci.guid = ? ORDER BY bag, slot");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_ACTIONS, "SELECT a.button, a.action, a.type FROM character_action as a, characters as c WHERE a.guid = c.guid AND a.spec = c.activeTalentGroup AND a.guid = ? ORDER BY button");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_MAILCOUNT_UNREAD, "SELECT COUNT(id) FROM mail WHERE receiver = ? AND (checked & 1) = 0 AND deliver_time <= ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_MAILCOUNT_UNREAD_SYNCH, "SELECT COUNT(id) FROM mail WHERE receiver = ? AND (checked & 1) = 0 AND deliver_time <= ?");
        PrepareStatement(CharStatements.CHAR_SEL_MAIL_SERVER_CHARACTER, "SELECT mailId from mail_server_character WHERE guid = ? and mailId = ?");
        PrepareStatement(CharStatements.CHAR_REP_MAIL_SERVER_CHARACTER, "REPLACE INTO mail_server_character (guid, mailId) values (?, ?)");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_SOCIALLIST, "SELECT friend, flags, note FROM character_social JOIN characters ON characters.guid = character_social.friend WHERE character_social.guid = ? AND deleteinfos_name IS NULL LIMIT 255");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_HOMEBIND, "SELECT mapId, zoneId, posX, posY, posZ, posO FROM character_homebind WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_SPELLCOOLDOWNS, "SELECT spell, category, item, time, needSend FROM character_spell_cooldown WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_DECLINEDNAMES, "SELECT genitive, dative, accusative, instrumental, prepositional FROM character_declinedname WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_ACHIEVEMENTS, "SELECT achievement, date FROM character_achievement WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_CRITERIAPROGRESS, "SELECT criteria, counter, date FROM character_achievement_progress WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_EQUIPMENTSETS, @"SELECT setguid, setindex, name, iconname, ignore_mask, item0, item1, item2, item3, item4, item5, item6, item7, item8, 
                         item9, item10, item11, item12, item13, item14, item15, item16, item17, item18 FROM character_equipmentsets WHERE guid = ? ORDER BY setindex");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_ENTRY_POINT, "SELECT joinX, joinY, joinZ, joinO, joinMapId, taxiPath0, taxiPath1, mountSpell FROM character_entry_point WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_GLYPHS, "SELECT talentGroup, glyph1, glyph2, glyph3, glyph4, glyph5, glyph6 FROM character_glyphs WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_TALENTS, "SELECT spell, specMask FROM character_talent WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_SKILLS, "SELECT skill, value, max FROM character_skills WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_RANDOMBG, "SELECT guid FROM character_battleground_random WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_BANNED, "SELECT guid FROM character_banned WHERE guid = ? AND active = 1");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_QUESTSTATUSREW, "SELECT quest FROM character_queststatus_rewarded WHERE guid = ? AND active = 1");
        PrepareStatement(CharStatements.CHAR_SEL_ACCOUNT_INSTANCELOCKTIMES, "SELECT instanceId, releaseTime FROM account_instance_times WHERE accountId = ?");
        PrepareStatement(CharStatements.CHAR_SEL_BREW_OF_THE_MONTH, "SELECT lastEventId FROM character_brew_of_the_month WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_REP_BREW_OF_THE_MONTH, "REPLACE INTO character_brew_of_the_month (guid, lastEventId) VALUES (?, ?)");
        // End LoginQueryHolder content

        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_ACTIONS_SPEC, "SELECT button, action, type FROM character_action WHERE guid = ? AND spec = ? ORDER BY button");
        PrepareStatement(CharStatements.CHAR_SEL_MAILITEMS, "SELECT creatorGuid, giftCreatorGuid, count, duration, charges, flags, enchantments, randomPropertyId, durability, playedTime, text, item_guid, itemEntry, ii.owner_guid, m.id FROM mail_items mi INNER JOIN mail m ON mi.mail_id = m.id LEFT JOIN item_instance ii ON mi.item_guid = ii.guid WHERE m.receiver = ?");
        PrepareStatement(CharStatements.CHAR_SEL_AUCTION_ITEMS, "SELECT creatorGuid, giftCreatorGuid, count, duration, charges, flags, enchantments, randomPropertyId, durability, playedTime, text, itemguid, itemEntry FROM auctionhouse ah JOIN item_instance ii ON ah.itemguid = ii.guid");
        PrepareStatement(CharStatements.CHAR_SEL_AUCTIONS, "SELECT id, houseid, itemguid, itemEntry, count, itemowner, buyoutprice, time, buyguid, lastbid, startbid, deposit FROM auctionhouse ah INNER JOIN item_instance ii ON ii.guid = ah.itemguid");
        PrepareStatement(CharStatements.CHAR_INS_AUCTION, "INSERT INTO auctionhouse (id, houseid, itemguid, itemowner, buyoutprice, time, buyguid, lastbid, startbid, deposit) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_AUCTION, "DELETE FROM auctionhouse WHERE id = ?");
        PrepareStatement(CharStatements.CHAR_UPD_AUCTION_BID, "UPDATE auctionhouse SET buyguid = ?, lastbid = ? WHERE id = ?");
        PrepareStatement(CharStatements.CHAR_INS_MAIL, "INSERT INTO mail(id, messageType, stationery, mailTemplateId, sender, receiver, subject, body, has_items, expire_time, deliver_time, money, cod, checked) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_MAIL_BY_ID, "DELETE FROM mail WHERE id = ?");
        PrepareStatement(CharStatements.CHAR_INS_MAIL_ITEM, "INSERT INTO mail_items(mail_id, item_guid, receiver) VALUES (?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_MAIL_ITEM, "DELETE FROM mail_items WHERE item_guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_INVALID_MAIL_ITEM, "DELETE FROM mail_items WHERE item_guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_EXPIRED_MAIL, "SELECT id, messageType, sender, receiver, has_items, expire_time, stationery, checked, mailTemplateId FROM mail WHERE expire_time < ?");
        PrepareStatement(CharStatements.CHAR_SEL_EXPIRED_MAIL_ITEMS, "SELECT item_guid, itemEntry, mail_id FROM mail_items mi INNER JOIN item_instance ii ON ii.guid = mi.item_guid LEFT JOIN mail mm ON mi.mail_id = mm.id WHERE mm.id IS NOT NULL AND mm.expire_time < ?");
        PrepareStatement(CharStatements.CHAR_UPD_MAIL_RETURNED, "UPDATE mail SET sender = ?, receiver = ?, expire_time = ?, deliver_time = ?, cod = 0, checked = ? WHERE id = ?");
        PrepareStatement(CharStatements.CHAR_UPD_MAIL_ITEM_RECEIVER, "UPDATE mail_items SET receiver = ? WHERE item_guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_ITEM_OWNER, "UPDATE item_instance SET owner_guid = ? WHERE guid = ?");

        PrepareStatement(CharStatements.CHAR_SEL_ITEM_REFUNDS, "SELECT player_guid, paidMoney, paidExtendedCost FROM item_refund_instance WHERE item_guid = ? AND player_guid = ? LIMIT 1");
        PrepareStatement(CharStatements.CHAR_SEL_ITEM_BOP_TRADE, "SELECT allowedPlayers FROM item_soulbound_trade_data WHERE itemGuid = ? LIMIT 1");
        PrepareStatement(CharStatements.CHAR_DEL_ITEM_BOP_TRADE, "DELETE FROM item_soulbound_trade_data WHERE itemGuid = ? LIMIT 1");
        PrepareStatement(CharStatements.CHAR_INS_ITEM_BOP_TRADE, "INSERT INTO item_soulbound_trade_data VALUES (?, ?)");
        PrepareStatement(CharStatements.CHAR_REP_INVENTORY_ITEM, "REPLACE INTO character_inventory (guid, bag, slot, item) VALUES (?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_REP_ITEM_INSTANCE, "REPLACE INTO item_instance (itemEntry, owner_guid, creatorGuid, giftCreatorGuid, count, duration, charges, flags, enchantments, randomPropertyId, durability, playedTime, text, guid) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_UPD_ITEM_INSTANCE, "UPDATE item_instance SET itemEntry = ?, owner_guid = ?, creatorGuid = ?, giftCreatorGuid = ?, count = ?, duration = ?, charges = ?, flags = ?, enchantments = ?, randomPropertyId = ?, durability = ?, playedTime = ?, text = ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_ITEM_INSTANCE_ON_LOAD, "UPDATE item_instance SET duration = ?, flags = ?, durability = ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_ITEM_INSTANCE, "DELETE FROM item_instance WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_ITEM_INSTANCE_BY_OWNER, "DELETE FROM item_instance WHERE owner_guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_GIFT_OWNER, "UPDATE character_gifts SET guid = ? WHERE item_guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_GIFT, "DELETE FROM character_gifts WHERE item_guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_GIFT_BY_ITEM, "SELECT entry, flags FROM character_gifts WHERE item_guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_ACCOUNT_BY_NAME, "SELECT account FROM characters WHERE name = ?");
        PrepareStatement(CharStatements.CHAR_DEL_ACCOUNT_INSTANCE_LOCK_TIMES, "DELETE FROM account_instance_times WHERE accountId = ?");
        PrepareStatement(CharStatements.CHAR_INS_ACCOUNT_INSTANCE_LOCK_TIMES, "INSERT INTO account_instance_times (accountId, instanceId, releaseTime) VALUES (?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_SEL_MATCH_MAKER_RATING, "SELECT matchMakerRating, maxMMR  FROM character_arena_stats WHERE guid = ? AND slot = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_COUNT, "SELECT account, COUNT(guid) FROM characters WHERE account = ? GROUP BY account");
        PrepareStatement(CharStatements.CHAR_UPD_NAME_BY_GUID, "UPDATE characters SET name = ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_DECLINED_NAME, "DELETE FROM character_declinedname WHERE guid = ?");

        // Guild handling
        // 0: uint32, 1: string, 2: uint32, 3: string, 4: string, 5: uint64, 6-10: uint32, 11: uint64
        PrepareStatement(CharStatements.CHAR_INS_GUILD, "INSERT INTO guild (guildid, name, leaderguid, info, motd, createdate, EmblemStyle, EmblemColor, BorderStyle, BorderColor, BackgroundColor, BankMoney) VALUES(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_GUILD, "DELETE FROM guild WHERE guildid = ?"); // 0: uint32
                                                                                                   // 0: string, 1: uint32
        PrepareStatement(CharStatements.CHAR_UPD_GUILD_NAME, "UPDATE guild SET name = ? WHERE guildid = ?");
        // 0: uint32, 1: uint32, 2: uint8, 4: string, 5: string
        PrepareStatement(CharStatements.CHAR_INS_GUILD_MEMBER, "INSERT INTO guild_member (guildid, guid, `rank`, pnote, offnote) VALUES (?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_GUILD_MEMBER, "DELETE FROM guild_member WHERE guid = ?"); // 0: uint32
        PrepareStatement(CharStatements.CHAR_DEL_GUILD_MEMBERS, "DELETE FROM guild_member WHERE guildid = ?"); // 0: uint32
        PrepareStatement(CharStatements.CHAR_SEL_GUILD_MEMBER_EXTENDED, @"SELECT g.guildid, g.name, gr.rname, gm.pnote, gm.offnote
                         FROM guild g JOIN guild_member gm ON g.guildid = gm.guildid
                         JOIN guild_rank gr ON g.guildid = gr.guildid AND gm.`rank` = gr.rid WHERE gm.guid = ?");
        // 0: uint32, 1: uint8, 3: string, 4: uint32, 5: uint32
        PrepareStatement(CharStatements.CHAR_INS_GUILD_RANK, "INSERT INTO guild_rank (guildid, rid, rname, rights, BankMoneyPerDay) VALUES (?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_GUILD_RANKS, "DELETE FROM guild_rank WHERE guildid = ?"); // 0: uint32
        PrepareStatement(CharStatements.CHAR_DEL_GUILD_LOWEST_RANK, "DELETE FROM guild_rank WHERE guildid = ? AND rid >= ?"); // 0: uint32, 1: uint8
        PrepareStatement(CharStatements.CHAR_INS_GUILD_BANK_TAB, "INSERT INTO guild_bank_tab (guildid, TabId) VALUES (?, ?)"); // 0: uint32, 1: uint8
        PrepareStatement(CharStatements.CHAR_DEL_GUILD_BANK_TAB, "DELETE FROM guild_bank_tab WHERE guildid = ? AND TabId = ?"); // 0: uint32, 1: uint8
        PrepareStatement(CharStatements.CHAR_DEL_GUILD_BANK_TABS, "DELETE FROM guild_bank_tab WHERE guildid = ?"); // 0: uint32
                                                                                                                      // 0: uint32, 1: uint8, 2: uint8, 3: uint32, 4: uint32
        PrepareStatement(CharStatements.CHAR_INS_GUILD_BANK_ITEM, "INSERT INTO guild_bank_item (guildid, TabId, SlotId, item_guid) VALUES (?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_GUILD_BANK_ITEM, "DELETE FROM guild_bank_item WHERE guildid = ? AND TabId = ? AND SlotId = ?"); // 0: uint32, 1: uint8, 2: uint8
        PrepareStatement(CharStatements.CHAR_DEL_GUILD_BANK_ITEMS, "DELETE FROM guild_bank_item WHERE guildid = ?"); // 0: uint32
                                                                                                                        // 0: uint32, 1: uint8, 2: uint8, 3: uint8, 4: uint32
        PrepareStatement(CharStatements.CHAR_INS_GUILD_BANK_RIGHT, @"INSERT INTO guild_bank_right (guildid, TabId, rid, gbright, SlotPerDay) VALUES (?, ?, ?, ?, ?) 
                         ON DUPLICATE KEY UPDATE gbright = VALUES(gbright), SlotPerDay = VALUES(SlotPerDay)");
        PrepareStatement(CharStatements.CHAR_DEL_GUILD_BANK_RIGHTS, "DELETE FROM guild_bank_right WHERE guildid = ?"); // 0: uint32
        PrepareStatement(CharStatements.CHAR_DEL_GUILD_BANK_RIGHTS_FOR_RANK, "DELETE FROM guild_bank_right WHERE guildid = ? AND rid = ?"); // 0: uint32, 1: uint8
                                                                                                                                               // 0-1: uint32, 2-3: uint8, 4-5: uint32, 6: uint16, 7: uint8, 8: uint64
        PrepareStatement(CharStatements.CHAR_INS_GUILD_BANK_EVENTLOG, "INSERT INTO guild_bank_eventlog (guildid, LogGuid, TabId, EventType, PlayerGuid, ItemOrMoney, ItemStackCount, DestTabId, TimeStamp) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_GUILD_BANK_EVENTLOG, "DELETE FROM guild_bank_eventlog WHERE guildid = ? AND LogGuid = ? AND TabId = ?"); // 0: uint32, 1: uint32, 2: uint8
        PrepareStatement(CharStatements.CHAR_DEL_GUILD_BANK_EVENTLOGS, "DELETE FROM guild_bank_eventlog WHERE guildid = ?"); // 0: uint32
                                                                                                                                // 0-1: uint32, 2: uint8, 3-4: uint32, 5: uint8, 6: uint64
        PrepareStatement(CharStatements.CHAR_INS_GUILD_EVENTLOG, "INSERT INTO guild_eventlog (guildid, LogGuid, EventType, PlayerGuid1, PlayerGuid2, NewRank, TimeStamp) VALUES (?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_GUILD_EVENTLOG, "DELETE FROM guild_eventlog WHERE guildid = ? AND LogGuid = ?"); // 0: uint32, 1: uint32
        PrepareStatement(CharStatements.CHAR_DEL_GUILD_EVENTLOGS, "DELETE FROM guild_eventlog WHERE guildid = ?"); // 0: uint32
        PrepareStatement(CharStatements.CHAR_UPD_GUILD_MEMBER_PNOTE, "UPDATE guild_member SET pnote = ? WHERE guid = ?"); // 0: string, 1: uint32
        PrepareStatement(CharStatements.CHAR_UPD_GUILD_MEMBER_OFFNOTE, "UPDATE guild_member SET offnote = ? WHERE guid = ?"); // 0: string, 1: uint32
        PrepareStatement(CharStatements.CHAR_UPD_GUILD_MEMBER_RANK, "UPDATE guild_member SET `rank` = ? WHERE guid = ?"); // 0: uint8, 1: uint32
        PrepareStatement(CharStatements.CHAR_UPD_GUILD_MOTD, "UPDATE guild SET motd = ? WHERE guildid = ?"); // 0: string, 1: uint32
        PrepareStatement(CharStatements.CHAR_UPD_GUILD_INFO, "UPDATE guild SET info = ? WHERE guildid = ?"); // 0: string, 1: uint32
        PrepareStatement(CharStatements.CHAR_UPD_GUILD_LEADER, "UPDATE guild SET leaderguid = ? WHERE guildid = ?"); // 0: uint32, 1: uint32
        PrepareStatement(CharStatements.CHAR_UPD_GUILD_RANK_NAME, "UPDATE guild_rank SET rname = ? WHERE rid = ? AND guildid = ?"); // 0: string, 1: uint8, 2: uint32
        PrepareStatement(CharStatements.CHAR_UPD_GUILD_RANK_RIGHTS, "UPDATE guild_rank SET rights = ? WHERE rid = ? AND guildid = ?"); // 0: uint32, 1: uint8, 2: uint32
                                                                                                                                          // 0-5: uint32
        PrepareStatement(CharStatements.CHAR_UPD_GUILD_EMBLEM_INFO, "UPDATE guild SET EmblemStyle = ?, EmblemColor = ?, BorderStyle = ?, BorderColor = ?, BackgroundColor = ? WHERE guildid = ?");
        // 0: string, 1: string, 2: uint32, 3: uint8
        PrepareStatement(CharStatements.CHAR_UPD_GUILD_BANK_TAB_INFO, "UPDATE guild_bank_tab SET TabName = ?, TabIcon = ? WHERE guildid = ? AND TabId = ?");
        PrepareStatement(CharStatements.CHAR_UPD_GUILD_BANK_MONEY, "UPDATE guild SET BankMoney = ? WHERE guildid = ?"); // 0: uint64, 1: uint32
                                                                                                                           // 0: uint8, 1: uint32, 2: uint8, 3: uint32
        PrepareStatement(CharStatements.CHAR_UPD_GUILD_BANK_EVENTLOG_TAB, "UPDATE guild_bank_eventlog SET TabId = ? WHERE guildid = ? AND TabId = ? AND LogGuid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_GUILD_RANK_BANK_MONEY, "UPDATE guild_rank SET BankMoneyPerDay = ? WHERE rid = ? AND guildid = ?"); // 0: uint32, 1: uint8, 2: uint32
        PrepareStatement(CharStatements.CHAR_UPD_GUILD_BANK_TAB_TEXT, "UPDATE guild_bank_tab SET TabText = ? WHERE guildid = ? AND TabId = ?"); // 0: string, 1: uint32, 2: uint8

        PrepareStatement(CharStatements.CHAR_INS_GUILD_MEMBER_WITHDRAW,
                         @"INSERT INTO guild_member_withdraw (guid, tab0, tab1, tab2, tab3, tab4, tab5, money) VALUES (?, ?, ?, ?, ?, ?, ?, ?) 
                         ON DUPLICATE KEY UPDATE tab0 = VALUES (tab0), tab1 = VALUES (tab1), tab2 = VALUES (tab2), tab3 = VALUES (tab3), tab4 = VALUES (tab4), tab5 = VALUES (tab5)");
        PrepareStatement(CharStatements.CHAR_DEL_GUILD_MEMBER_WITHDRAW, "TRUNCATE guild_member_withdraw");

        // 0: uint32, 1: uint32, 2: uint32
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_DATA_FOR_GUILD, "SELECT name, level, class, gender, zone, account FROM characters WHERE guid = ?");

        // Chat channel handling
        PrepareStatement(CharStatements.CHAR_INS_CHANNEL, "INSERT INTO channels(channelId, name, team, announce, lastUsed) VALUES (?, ?, ?, ?, UNIX_TIMESTAMP())");
        PrepareStatement(CharStatements.CHAR_UPD_CHANNEL, "UPDATE channels SET announce = ?, password = ?, lastUsed = UNIX_TIMESTAMP() WHERE channelId = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHANNEL, "DELETE FROM channels WHERE name = ? AND team = ?");
        PrepareStatement(CharStatements.CHAR_UPD_CHANNEL_USAGE, "UPDATE channels SET lastUsed = UNIX_TIMESTAMP() WHERE channelId = ?");
        PrepareStatement(CharStatements.CHAR_DEL_OLD_CHANNELS, "DELETE FROM channels WHERE lastUsed + ? < UNIX_TIMESTAMP()");
        PrepareStatement(CharStatements.CHAR_DEL_OLD_CHANNELS_BANS, "DELETE cb.* FROM channels_bans cb LEFT JOIN channels cn ON cb.channelId=cn.channelId WHERE cn.channelId IS NULL OR cb.banTime <= UNIX_TIMESTAMP()");
        PrepareStatement(CharStatements.CHAR_INS_CHANNEL_BAN, "REPLACE INTO channels_bans VALUES (?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_CHANNEL_BAN, "DELETE FROM channels_bans WHERE channelId = ? AND playerGUID = ?");

        // Equipmentsets
        PrepareStatement(CharStatements.CHAR_UPD_EQUIP_SET, @"UPDATE character_equipmentsets SET name=?, iconname=?, ignore_mask=?, item0=?, item1=?, item2=?, item3=?, 
                         item4=?, item5=?, item6=?, item7=?, item8=?, item9=?, item10=?, item11=?, item12=?, item13=?, item14=?, item15=?, item16=?, 
                         item17=?, item18=? WHERE guid=? AND setguid=? AND setindex=?");
        PrepareStatement(CharStatements.CHAR_INS_EQUIP_SET, @"INSERT INTO character_equipmentsets (guid, setguid, setindex, name, iconname, ignore_mask, item0, item1, item2, item3, 
                         item4, item5, item6, item7, item8, item9, item10, item11, item12, item13, item14, item15, item16, item17, item18) 
                         VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_EQUIP_SET, "DELETE FROM character_equipmentsets WHERE setguid=?");

        // Auras
        PrepareStatement(CharStatements.CHAR_INS_AURA, @"INSERT INTO character_aura (guid, casterGuid, itemGuid, spell, effectMask, recalculateMask, stackcount, amount0, amount1, amount2, base_amount0, base_amount1, base_amount2, maxDuration, remainTime, remainCharges) 
                         VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");

        // Account data
        PrepareStatement(CharStatements.CHAR_SEL_ACCOUNT_DATA, "SELECT type, time, data FROM account_data WHERE accountId = ?");
        PrepareStatement(CharStatements.CHAR_REP_ACCOUNT_DATA, "REPLACE INTO account_data (accountId, type, time, data) VALUES (?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_ACCOUNT_DATA, "DELETE FROM account_data WHERE accountId = ?");
        PrepareStatement(CharStatements.CHAR_SEL_PLAYER_ACCOUNT_DATA, "SELECT type, time, data FROM character_account_data WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_REP_PLAYER_ACCOUNT_DATA, "REPLACE INTO character_account_data(guid, type, time, data) VALUES (?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_PLAYER_ACCOUNT_DATA, "DELETE FROM character_account_data WHERE guid = ?");

        // Tutorials
        PrepareStatement(CharStatements.CHAR_SEL_TUTORIALS, "SELECT tut0, tut1, tut2, tut3, tut4, tut5, tut6, tut7 FROM account_tutorial WHERE accountId = ?");
        PrepareStatement(CharStatements.CHAR_SEL_HAS_TUTORIALS, "SELECT 1 FROM account_tutorial WHERE accountId = ?");
        PrepareStatement(CharStatements.CHAR_INS_TUTORIALS, "INSERT INTO account_tutorial(tut0, tut1, tut2, tut3, tut4, tut5, tut6, tut7, accountId) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_UPD_TUTORIALS, "UPDATE account_tutorial SET tut0 = ?, tut1 = ?, tut2 = ?, tut3 = ?, tut4 = ?, tut5 = ?, tut6 = ?, tut7 = ? WHERE accountId = ?");
        PrepareStatement(CharStatements.CHAR_DEL_TUTORIALS, "DELETE FROM account_tutorial WHERE accountId = ?");

        // Instance saves
        PrepareStatement(CharStatements.CHAR_INS_INSTANCE_SAVE, "INSERT INTO instance (id, map, resettime, difficulty, completedEncounters, data) VALUES (?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_UPD_INSTANCE_SAVE_DATA, "UPDATE instance SET data=? WHERE id=?");
        PrepareStatement(CharStatements.CHAR_UPD_INSTANCE_SAVE_ENCOUNTERMASK, "UPDATE instance SET completedEncounters=? WHERE id=?");

        // Game event saves
        PrepareStatement(CharStatements.CHAR_DEL_GAME_EVENT_SAVE, "DELETE FROM game_event_save WHERE eventEntry = ?");
        PrepareStatement(CharStatements.CHAR_INS_GAME_EVENT_SAVE, "INSERT INTO game_event_save (eventEntry, state, next_start) VALUES (?, ?, ?)");

        // Game event condition saves
        PrepareStatement(CharStatements.CHAR_DEL_ALL_GAME_EVENT_CONDITION_SAVE, "DELETE FROM game_event_condition_save WHERE eventEntry = ?");
        PrepareStatement(CharStatements.CHAR_DEL_GAME_EVENT_CONDITION_SAVE, "DELETE FROM game_event_condition_save WHERE eventEntry = ? AND condition_id = ?");
        PrepareStatement(CharStatements.CHAR_INS_GAME_EVENT_CONDITION_SAVE, "INSERT INTO game_event_condition_save (eventEntry, condition_id, done) VALUES (?, ?, ?)");

        // Petitions
        PrepareStatement(CharStatements.CHAR_DEL_ALL_PETITION_SIGNATURES, "DELETE FROM petition_sign WHERE playerguid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_PETITION_SIGNATURE, "DELETE FROM petition_sign WHERE playerguid = ? AND type = ?");

        // Arena teams
        PrepareStatement(CharStatements.CHAR_INS_ARENA_TEAM, "INSERT INTO arena_team (arenaTeamId, name, captainGuid, type, rating, backgroundColor, emblemStyle, emblemColor, borderStyle, borderColor) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_INS_ARENA_TEAM_MEMBER, "INSERT INTO arena_team_member (arenaTeamId, guid) VALUES (?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_ARENA_TEAM, "DELETE FROM arena_team WHERE arenaTeamId = ?");
        PrepareStatement(CharStatements.CHAR_DEL_ARENA_TEAM_MEMBERS, "DELETE FROM arena_team_member WHERE arenaTeamId = ?");
        PrepareStatement(CharStatements.CHAR_UPD_ARENA_TEAM_CAPTAIN, "UPDATE arena_team SET captainGuid = ? WHERE arenaTeamId = ?");
        PrepareStatement(CharStatements.CHAR_DEL_ARENA_TEAM_MEMBER, "DELETE FROM arena_team_member WHERE arenaTeamId = ? AND guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_ARENA_TEAM_STATS, "UPDATE arena_team SET rating = ?, weekGames = ?, weekWins = ?, seasonGames = ?, seasonWins = ?, `rank` = ? WHERE arenaTeamId = ?");
        PrepareStatement(CharStatements.CHAR_UPD_ARENA_TEAM_MEMBER, "UPDATE arena_team_member SET personalRating = ?, weekGames = ?, weekWins = ?, seasonGames = ?, seasonWins = ? WHERE arenaTeamId = ? AND guid = ?");
        PrepareStatement(CharStatements.CHAR_REP_CHARACTER_ARENA_STATS, "REPLACE INTO character_arena_stats (guid, slot, matchMakerRating, maxMMR) VALUES (?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_SEL_PLAYER_ARENA_TEAMS, "SELECT arena_team_member.arenaTeamId FROM arena_team_member JOIN arena_team ON arena_team_member.arenaTeamId = arena_team.arenaTeamId WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_ARENA_TEAM_NAME, "UPDATE arena_team SET name = ? WHERE arenaTeamId = ?");

        // Character battleground data
        PrepareStatement(CharStatements.CHAR_INS_PLAYER_ENTRY_POINT, "INSERT INTO character_entry_point (guid, joinX, joinY, joinZ, joinO, joinMapId, taxiPath0, taxiPath1, mountSpell) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_PLAYER_ENTRY_POINT, "DELETE FROM character_entry_point WHERE guid = ?");

        // Character homebind
        PrepareStatement(CharStatements.CHAR_INS_PLAYER_HOMEBIND, "INSERT INTO character_homebind (guid, mapId, zoneId, posX, posY, posZ, posO) VALUES (?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_UPD_PLAYER_HOMEBIND, "UPDATE character_homebind SET mapId = ?, zoneId = ?, posX = ?, posY = ?, posZ = ?, posO = ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_PLAYER_HOMEBIND, "DELETE FROM character_homebind WHERE guid = ?");

        // Corpse
        PrepareStatement(CharStatements.CHAR_SEL_CORPSES, "SELECT posX, posY, posZ, orientation, mapId, displayId, itemCache, bytes1, bytes2, guildId, flags, dynFlags, time, corpseType, instanceId, phaseMask, guid FROM corpse WHERE mapId = ? AND instanceId = ?");
        PrepareStatement(CharStatements.CHAR_INS_CORPSE, "INSERT INTO corpse (guid, posX, posY, posZ, orientation, mapId, displayId, itemCache, bytes1, bytes2, guildId, flags, dynFlags, time, corpseType, instanceId, phaseMask) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_CORPSE, "DELETE FROM corpse WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CORPSES_FROM_MAP, "DELETE FROM corpse WHERE mapId = ? AND instanceId = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CORPSE_LOCATION, "SELECT mapId, posX, posY, posZ, orientation FROM corpse WHERE guid = ?");

        // Creature respawn
        PrepareStatement(CharStatements.CHAR_SEL_CREATURE_RESPAWNS, "SELECT guid, respawnTime FROM creature_respawn WHERE mapId = ? AND instanceId = ?");
        PrepareStatement(CharStatements.CHAR_REP_CREATURE_RESPAWN, "REPLACE INTO creature_respawn (guid, respawnTime, mapId, instanceId) VALUES (?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_CREATURE_RESPAWN, "DELETE FROM creature_respawn WHERE guid = ? AND mapId = ? AND instanceId = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CREATURE_RESPAWN_BY_INSTANCE, "DELETE FROM creature_respawn WHERE mapId = ? AND instanceId = ?");

        // Gameobject respawn
        PrepareStatement(CharStatements.CHAR_SEL_GO_RESPAWNS, "SELECT guid, respawnTime FROM gameobject_respawn WHERE mapId = ? AND instanceId = ?");
        PrepareStatement(CharStatements.CHAR_REP_GO_RESPAWN, "REPLACE INTO gameobject_respawn (guid, respawnTime, mapId, instanceId) VALUES (?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_GO_RESPAWN, "DELETE FROM gameobject_respawn WHERE guid = ? AND mapId = ? AND instanceId = ?");
        PrepareStatement(CharStatements.CHAR_DEL_GO_RESPAWN_BY_INSTANCE, "DELETE FROM gameobject_respawn WHERE mapId = ? AND instanceId = ?");

        // GM Tickets
        PrepareStatement(CharStatements.CHAR_SEL_GM_TICKETS, "SELECT id, type, playerGuid, name, description, createTime, mapId, posX, posY, posZ, lastModifiedTime, closedBy, assignedTo, comment, response, completed, escalated, viewed, needMoreHelp, resolvedBy FROM gm_ticket");
        PrepareStatement(CharStatements.CHAR_REP_GM_TICKET, "REPLACE INTO gm_ticket (id, type, playerGuid, name, description, createTime, mapId, posX, posY, posZ, lastModifiedTime, closedBy, assignedTo, comment, response, completed, escalated, viewed, needMoreHelp, resolvedBy) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_GM_TICKET, "DELETE FROM gm_ticket WHERE id = ?");
        PrepareStatement(CharStatements.CHAR_DEL_PLAYER_GM_TICKETS, "DELETE FROM gm_ticket WHERE playerGuid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_PLAYER_GM_TICKETS_ON_CHAR_DELETION, "UPDATE gm_ticket SET type = 2 WHERE playerGuid = ?");

        // GM Survey/subsurvey/lag report
        PrepareStatement(CharStatements.CHAR_INS_GM_SURVEY, "INSERT INTO gm_survey (guid, surveyId, mainSurvey, comment, createTime) VALUES (?, ?, ?, ?, UNIX_TIMESTAMP(NOW()))");
        PrepareStatement(CharStatements.CHAR_INS_GM_SUBSURVEY, "INSERT INTO gm_subsurvey (surveyId, questionId, answer, answerComment) VALUES (?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_INS_LAG_REPORT, "INSERT INTO lag_reports (guid, lagType, mapId, posX, posY, posZ, latency, createTime) VALUES (?, ?, ?, ?, ?, ?, ?, ?)");

        // LFG Data
        PrepareStatement(CharStatements.CHAR_REP_LFG_DATA, "REPLACE INTO lfg_data (guid, dungeon, state) VALUES (?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_LFG_DATA, "DELETE FROM lfg_data WHERE guid = ?");

        // Player saving
        PrepareStatement(CharStatements.CHAR_INS_CHARACTER, @"INSERT INTO characters (guid, account, name, race, class, gender, level, xp, money, skin, face, hairStyle, hairColor, facialStyle, bankSlots, restState, playerFlags, 
                         map, instance_id, instance_mode_mask, position_x, position_y, position_z, orientation, trans_x, trans_y, trans_z, trans_o, transguid, 
                         taximask, cinematic, 
                         totaltime, leveltime, rest_bonus, logout_time, is_logout_resting, resettalents_cost, resettalents_time, 
                         extra_flags, stable_slots, at_login, zone, 
                         death_expire_time, taxi_path, arenaPoints, totalHonorPoints, todayHonorPoints, yesterdayHonorPoints, totalKills, 
                         todayKills, yesterdayKills, chosenTitle, knownCurrencies, watchedFaction, drunk, health, power1, power2, power3, 
                         power4, power5, power6, power7, latency, talentGroupsCount, activeTalentGroup, exploredZones, equipmentCache, 
                         ammoId, knownTitles, actionBars, grantableLevels, innTriggerId) VALUES 
                         (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)");
        PrepareStatement(CharStatements.CHAR_UPD_CHARACTER, @"UPDATE characters SET name=?,race=?,class=?,gender=?,level=?,xp=?,money=?,skin=?,face=?,hairStyle=?,hairColor=?,facialStyle=?,bankSlots=?,restState=?,playerFlags=?,
                         map=?,instance_id=?,instance_mode_mask=?,position_x=?,position_y=?,position_z=?,orientation=?,trans_x=?,trans_y=?,trans_z=?,trans_o=?,transguid=?,taximask=?,cinematic=?,totaltime=?,leveltime=?,rest_bonus=?,
                         logout_time=?,is_logout_resting=?,resettalents_cost=?,resettalents_time=?,extra_flags=?,stable_slots=?,at_login=?,zone=?,death_expire_time=?,taxi_path=?,
                         arenaPoints=?,totalHonorPoints=?,todayHonorPoints=?,yesterdayHonorPoints=?,totalKills=?,todayKills=?,yesterdayKills=?,chosenTitle=?,knownCurrencies=?,
                         watchedFaction=?,drunk=?,health=?,power1=?,power2=?,power3=?,power4=?,power5=?,power6=?,power7=?,latency=?,talentGroupsCount=?,activeTalentGroup=?,exploredZones=?,
                         equipmentCache=?,ammoId=?,knownTitles=?,actionBars=?,grantableLevels=?,innTriggerId=?,online=? WHERE guid=?");

        PrepareStatement(CharStatements.CHAR_UPD_ADD_AT_LOGIN_FLAG, "UPDATE characters SET at_login = at_login | ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_REM_AT_LOGIN_FLAG, "UPDATE characters set at_login = at_login & ~ ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_ALL_AT_LOGIN_FLAGS, "UPDATE characters SET at_login = at_login | ?");
        PrepareStatement(CharStatements.CHAR_INS_BUG_REPORT, "INSERT INTO bugreport (type, content) VALUES(?, ?)");
        PrepareStatement(CharStatements.CHAR_UPD_PETITION_NAME, "UPDATE petition SET name = ? WHERE petitionguid = ?");
        PrepareStatement(CharStatements.CHAR_INS_PETITION_SIGNATURE, "INSERT INTO petition_sign (ownerguid, petitionguid, playerguid, player_account) VALUES (?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_UPD_ACCOUNT_ONLINE, "UPDATE characters SET online = 0 WHERE account = ?");
        PrepareStatement(CharStatements.CHAR_INS_GROUP, "INSERT INTO `groups` (guid, leaderGuid, lootMethod, looterGuid, lootThreshold, icon1, icon2, icon3, icon4, icon5, icon6, icon7, icon8, groupType, difficulty, raidDifficulty, masterLooterGuid) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_REP_GROUP_MEMBER, "REPLACE INTO group_member (guid, memberGuid, memberFlags, subgroup, roles) VALUES(?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_GROUP_MEMBER, "DELETE FROM group_member WHERE memberGuid = ? AND guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_GROUP_LEADER, "UPDATE `groups` SET leaderGuid = ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_GROUP_TYPE, "UPDATE `groups` SET groupType = ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_GROUP_MEMBER_SUBGROUP, "UPDATE group_member SET subgroup = ? WHERE memberGuid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_GROUP_MEMBER_FLAG, "UPDATE group_member SET memberFlags = ? WHERE memberGuid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_GROUP_DIFFICULTY, "UPDATE `groups` SET difficulty = ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_GROUP_RAID_DIFFICULTY, "UPDATE `groups` SET raidDifficulty = ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_ALL_GM_TICKETS, "TRUNCATE TABLE gm_ticket");
        PrepareStatement(CharStatements.CHAR_DEL_INVALID_SPELL_TALENTS, "DELETE FROM character_talent WHERE spell = ?");
        PrepareStatement(CharStatements.CHAR_DEL_INVALID_SPELL_SPELLS, "DELETE FROM character_spell WHERE spell = ?");
        PrepareStatement(CharStatements.CHAR_UPD_DELETE_INFO, "UPDATE characters SET deleteInfos_Name = name, deleteInfos_Account = account, deleteDate = UNIX_TIMESTAMP(), name = '', account = 0 WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_UDP_RESTORE_DELETE_INFO, "UPDATE characters SET name = ?, account = ?, deleteDate = NULL, deleteInfos_Name = NULL, deleteInfos_Account = NULL WHERE deleteDate IS NOT NULL AND guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_ZONE, "UPDATE characters SET zone = ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_LEVEL, "UPDATE characters SET level = ?, xp = 0 WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_XP_ACCUMULATIVE, "UPDATE characters SET  xp = xp + ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_INVALID_ACHIEV_PROGRESS_CRITERIA, "DELETE FROM character_achievement_progress WHERE criteria = ?");
        PrepareStatement(CharStatements.CHAR_DEL_INVALID_ACHIEVMENT, "DELETE FROM character_achievement WHERE achievement = ?");
        PrepareStatement(CharStatements.CHAR_INS_ADDON, "INSERT INTO addons (name, crc) VALUES (?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_INVALID_PET_SPELL, "DELETE FROM pet_spell WHERE spell = ?");
        PrepareStatement(CharStatements.CHAR_UPD_GLOBAL_INSTANCE_RESETTIME, "UPDATE instance_reset SET resettime = ? WHERE mapid = ? AND difficulty = ?");
        PrepareStatement(CharStatements.CHAR_UPD_CHAR_ONLINE, "UPDATE characters SET online = 1 WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_CHAR_NAME_AT_LOGIN, "UPDATE characters set name = ?, at_login = ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_WORLDSTATE, "UPDATE worldstates SET value = ? WHERE entry = ?");
        PrepareStatement(CharStatements.CHAR_INS_WORLDSTATE, "INSERT INTO worldstates (entry, value) VALUES (?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_INSTANCE_BY_INSTANCE, "DELETE FROM character_instance WHERE instance = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_INSTANCE_BY_INSTANCE_NOT_EXTENDED, "DELETE FROM character_instance WHERE instance = ? AND extended = 0");
        PrepareStatement(CharStatements.CHAR_UPD_CHAR_INSTANCE_SET_NOT_EXTENDED, "UPDATE character_instance SET extended = 0 WHERE instance = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_INSTANCE_BY_INSTANCE_GUID, "DELETE FROM character_instance WHERE guid = ? AND instance = ?");
        PrepareStatement(CharStatements.CHAR_UPD_CHAR_INSTANCE, "UPDATE character_instance SET instance = ?, permanent = ?, extended = 0 WHERE guid = ? AND instance = ?");
        PrepareStatement(CharStatements.CHAR_UPD_CHAR_INSTANCE_EXTENDED, "UPDATE character_instance SET extended = ? WHERE guid = ? AND instance = ?");
        PrepareStatement(CharStatements.CHAR_INS_CHAR_INSTANCE, "INSERT INTO character_instance (guid, instance, permanent, extended) VALUES (?, ?, ?, 0)");
        PrepareStatement(CharStatements.CHAR_INS_ARENA_LOG_FIGHT, "INSERT INTO log_arena_fights VALUES (?, NOW(), ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_INS_ARENA_LOG_MEMBERSTATS, "INSERT INTO log_arena_memberstats VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_UPD_GENDER_AND_APPEARANCE, "UPDATE characters SET gender = ?, skin = ?, face = ?, hairStyle = ?, hairColor = ?, facialStyle = ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHARACTER_SKILL, "DELETE FROM character_skills WHERE guid = ? AND skill = ?");
        PrepareStatement(CharStatements.CHAR_UPD_ADD_CHARACTER_SOCIAL_FLAGS, "UPDATE character_social SET flags = flags | ? WHERE guid = ? AND friend = ?");
        PrepareStatement(CharStatements.CHAR_UPD_REM_CHARACTER_SOCIAL_FLAGS, "UPDATE character_social SET flags = flags & ~ ? WHERE guid = ? AND friend = ?");
        PrepareStatement(CharStatements.CHAR_INS_CHARACTER_SOCIAL, "REPLACE INTO character_social (guid, friend, flags) VALUES (?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_CHARACTER_SOCIAL, "DELETE FROM character_social WHERE guid = ? AND friend = ?");
        PrepareStatement(CharStatements.CHAR_UPD_CHARACTER_SOCIAL_NOTE, "UPDATE character_social SET note = ? WHERE guid = ? AND friend = ?");
        PrepareStatement(CharStatements.CHAR_UPD_CHARACTER_POSITION, "UPDATE characters SET position_x = ?, position_y = ?, position_z = ?, orientation = ?, map = ?, zone = ?, trans_x = 0, trans_y = 0, trans_z = 0, transguid = 0, taxi_path = '', cinematic = 1 WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_AURA_FROZEN, "SELECT characters.name FROM characters LEFT JOIN character_aura ON (characters.guid = character_aura.guid) WHERE character_aura.spell = 9454");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_ONLINE, "SELECT name, account, map, zone FROM characters WHERE online > 0");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_DEL_INFO_BY_GUID, "SELECT guid, deleteInfos_Name, deleteInfos_Account, deleteDate FROM characters WHERE deleteDate IS NOT NULL AND guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_DEL_INFO_BY_NAME, "SELECT guid, deleteInfos_Name, deleteInfos_Account, deleteDate FROM characters WHERE deleteDate IS NOT NULL AND deleteInfos_Name LIKE CONCAT('%%', ?, '%%')");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_DEL_INFO, "SELECT guid, deleteInfos_Name, deleteInfos_Account, deleteDate FROM characters WHERE deleteDate IS NOT NULL");
        PrepareStatement(CharStatements.CHAR_SEL_CHARS_BY_ACCOUNT_ID, "SELECT guid FROM characters WHERE account = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_PINFO, "SELECT totaltime, level, money, account, race, class, map, zone, gender, health, playerFlags FROM characters WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_PINFO_BANS, "SELECT unbandate, bandate = unbandate, bannedby, banreason FROM character_banned WHERE guid = ? AND active ORDER BY bandate ASC LIMIT 1");
        PrepareStatement(CharStatements.CHAR_SEL_PINFO_MAILS, "SELECT SUM(CASE WHEN (checked & 1) THEN 1 ELSE 0 END) AS 'readmail', COUNT(*) AS 'totalmail' FROM mail WHERE `receiver` = ?");
        PrepareStatement(CharStatements.CHAR_SEL_PINFO_XP, "SELECT a.xp, b.guid FROM characters a LEFT JOIN guild_member b ON a.guid = b.guid WHERE a.guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_HOMEBIND, "SELECT mapId, zoneId, posX, posY, posZ, posO FROM character_homebind WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_GUID_NAME_BY_ACC, "SELECT guid, name FROM characters WHERE account = ?");
        PrepareStatement(CharStatements.CHAR_SEL_POOL_QUEST_SAVE, "SELECT quest_id FROM pool_quest_save WHERE pool_id = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHARACTER_AT_LOGIN, "SELECT at_login FROM characters WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_CLASS_LVL_AT_LOGIN, "SELECT class, level, at_login, knownTitles FROM characters WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_CUSTOMIZE_INFO, "SELECT name, race, class, gender, at_login FROM characters WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_RACE_OR_FACTION_CHANGE_INFOS, "SELECT at_login, knownTitles, money FROM characters WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_AT_LOGIN_TITLES_MONEY, "SELECT at_login, knownTitles, money FROM characters WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_COD_ITEM_MAIL, "SELECT id, messageType, mailTemplateId, sender, subject, body, money, has_items FROM mail WHERE receiver = ? AND has_items <> 0 AND cod <> 0");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_SOCIAL, "SELECT DISTINCT guid FROM character_social WHERE friend = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_OLD_CHARS, "SELECT guid, deleteInfos_Account FROM characters WHERE deleteDate IS NOT NULL AND deleteDate < ?");
        PrepareStatement(CharStatements.CHAR_SEL_ARENA_TEAM_ID_BY_PLAYER_GUID, "SELECT arena_team_member.arenateamid FROM arena_team_member JOIN arena_team ON arena_team_member.arenateamid = arena_team.arenateamid WHERE guid = ? AND type = ? LIMIT 1");
        PrepareStatement(CharStatements.CHAR_SEL_MAIL, "SELECT id, messageType, sender, receiver, subject, body, expire_time, deliver_time, money, cod, checked, stationery, mailTemplateId FROM mail WHERE receiver = ? AND deliver_time <= ? ORDER BY id DESC");
        PrepareStatement(CharStatements.CHAR_SEL_NEXT_MAIL_DELIVERYTIME, "SELECT MIN(deliver_time) FROM mail WHERE receiver = ? AND deliver_time > ? AND (checked & 1) = 0 LIMIT 1");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_AURA_FROZEN, "DELETE FROM character_aura WHERE spell = 9454 AND guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_INVENTORY_COUNT_ITEM, "SELECT COUNT(itemEntry) FROM character_inventory ci INNER JOIN item_instance ii ON ii.guid = ci.item WHERE itemEntry = ?");
        PrepareStatement(CharStatements.CHAR_SEL_MAIL_COUNT_ITEM, "SELECT COUNT(itemEntry) FROM mail_items mi INNER JOIN item_instance ii ON ii.guid = mi.item_guid WHERE itemEntry = ?");
        PrepareStatement(CharStatements.CHAR_SEL_AUCTIONHOUSE_COUNT_ITEM, "SELECT COUNT(itemEntry) FROM auctionhouse ah INNER JOIN item_instance ii ON ii.guid = ah.itemguid WHERE itemEntry = ?");
        PrepareStatement(CharStatements.CHAR_SEL_GUILD_BANK_COUNT_ITEM, "SELECT COUNT(itemEntry) FROM guild_bank_item gbi INNER JOIN item_instance ii ON ii.guid = gbi.item_guid WHERE itemEntry = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_INVENTORY_ITEM_BY_ENTRY, @"SELECT ci.item, cb.slot AS bag, ci.slot, ci.guid, c.account, c.name FROM characters c 
                         INNER JOIN character_inventory ci ON ci.guid = c.guid 
                         INNER JOIN item_instance ii ON ii.guid = ci.item 
                         LEFT JOIN character_inventory cb ON cb.item = ci.bag WHERE ii.itemEntry = ? LIMIT ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_INVENTORY_ITEM_BY_ENTRY_AND_OWNER, "SELECT ci.item FROM character_inventory ci INNER JOIN item_instance ii ON ii.guid = ci.item WHERE ii.itemEntry = ? AND ii.owner_guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_MAIL_ITEMS_BY_ENTRY, @"SELECT mi.item_guid, m.sender, m.receiver, cs.account, cs.name, cr.account, cr.name 
                         FROM mail m INNER JOIN mail_items mi ON mi.mail_id = m.id INNER JOIN item_instance ii ON ii.guid = mi.item_guid 
                         INNER JOIN characters cs ON cs.guid = m.sender INNER JOIN characters cr ON cr.guid = m.receiver WHERE ii.itemEntry = ? LIMIT ?");
        PrepareStatement(CharStatements.CHAR_SEL_AUCTIONHOUSE_ITEM_BY_ENTRY, "SELECT  ah.itemguid, ah.itemowner, c.account, c.name FROM auctionhouse ah INNER JOIN characters c ON c.guid = ah.itemowner INNER JOIN item_instance ii ON ii.guid = ah.itemguid WHERE ii.itemEntry = ? LIMIT ?");
        PrepareStatement(CharStatements.CHAR_SEL_GUILD_BANK_ITEM_BY_ENTRY, "SELECT gi.item_guid, gi.guildid, g.name FROM guild_bank_item gi INNER JOIN guild g ON g.guildid = gi.guildid INNER JOIN item_instance ii ON ii.guid = gi.item_guid WHERE ii.itemEntry = ? LIMIT ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_ACHIEVEMENT, "DELETE FROM character_achievement WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_ACHIEVEMENT_PROGRESS, "DELETE FROM character_achievement_progress WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_INS_CHAR_ACHIEVEMENT, "INSERT INTO character_achievement (guid, achievement, date) VALUES (?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_ACHIEVEMENT_PROGRESS_BY_CRITERIA, "DELETE FROM character_achievement_progress WHERE guid = ? AND criteria = ?");
        PrepareStatement(CharStatements.CHAR_INS_CHAR_ACHIEVEMENT_PROGRESS, "INSERT INTO character_achievement_progress (guid, criteria, counter, date) VALUES (?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_REPUTATION_BY_FACTION, "DELETE FROM character_reputation WHERE guid = ? AND faction = ?");
        PrepareStatement(CharStatements.CHAR_INS_CHAR_REPUTATION_BY_FACTION, "INSERT INTO character_reputation (guid, faction, standing, flags) VALUES (?, ?, ? , ?)");
        PrepareStatement(CharStatements.CHAR_UPD_CHAR_ARENA_POINTS, "UPDATE characters SET arenaPoints = (arenaPoints + ?) WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_ITEM_REFUND_INSTANCE, "DELETE FROM item_refund_instance WHERE item_guid = ?");
        PrepareStatement(CharStatements.CHAR_INS_ITEM_REFUND_INSTANCE, "INSERT INTO item_refund_instance (item_guid, player_guid, paidMoney, paidExtendedCost) VALUES (?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_GROUP, "DELETE FROM `groups` WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_GROUP_MEMBER_ALL, "DELETE FROM group_member WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_INS_CHAR_GIFT, "INSERT INTO character_gifts (guid, item_guid, entry, flags) VALUES (?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_INSTANCE_BY_INSTANCE, "DELETE FROM instance WHERE id = ?");
        PrepareStatement(CharStatements.CHAR_DEL_MAIL_ITEM_BY_ID, "DELETE FROM mail_items WHERE mail_id = ?");
        PrepareStatement(CharStatements.CHAR_INS_PETITION, "INSERT INTO petition (ownerguid, petitionguid, name, type) VALUES (?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_PETITION_BY_GUID, "DELETE FROM petition WHERE petitionguid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_PETITION_SIGNATURE_BY_GUID, "DELETE FROM petition_sign WHERE petitionguid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_DECLINED_NAME, "DELETE FROM character_declinedname WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_INS_CHAR_DECLINED_NAME, "INSERT INTO character_declinedname (guid, genitive, dative, accusative, instrumental, prepositional) VALUES (?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_UPD_CHAR_RACE, "UPDATE characters SET race = ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_SKILL_LANGUAGES, "DELETE FROM character_skills WHERE skill IN (98, 113, 759, 111, 313, 109, 115, 315, 673, 137) AND guid = ?");
        PrepareStatement(CharStatements.CHAR_INS_CHAR_SKILL_LANGUAGE, "INSERT INTO `character_skills` (guid, skill, value, max) VALUES (?, ?, 300, 300)");
        PrepareStatement(CharStatements.CHAR_UPD_CHAR_TAXI_PATH, "UPDATE characters SET taxi_path = '' WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_CHAR_TAXIMASK, "UPDATE characters SET taximask = ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_QUESTSTATUS, "DELETE FROM character_queststatus WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_SOCIAL_BY_GUID, "DELETE FROM character_social WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_SOCIAL_BY_FRIEND, "DELETE FROM character_social WHERE friend = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_ACHIEVEMENT_BY_ACHIEVEMENT, "DELETE FROM character_achievement WHERE achievement = ? AND guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_CHAR_ACHIEVEMENT, "UPDATE character_achievement SET achievement = ? WHERE achievement = ? AND guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_CHAR_INVENTORY_FACTION_CHANGE, "UPDATE item_instance ii, character_inventory ci SET ii.itemEntry = ? WHERE ii.itemEntry = ? AND ci.guid = ? AND ci.item = ii.guid");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_SPELL_BY_SPELL, "DELETE FROM character_spell WHERE guid = ? AND spell = ?");
        PrepareStatement(CharStatements.CHAR_UPD_CHAR_SPELL_FACTION_CHANGE, "UPDATE character_spell SET spell = ? WHERE spell = ? AND guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_REP_BY_FACTION, "SELECT standing FROM character_reputation WHERE faction = ? AND guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_REP_BY_FACTION, "DELETE FROM character_reputation WHERE faction = ? AND guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_CHAR_REP_FACTION_CHANGE, "UPDATE character_reputation SET faction = ?, standing = ? WHERE faction = ? AND guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_CHAR_TITLES_FACTION_CHANGE, "UPDATE characters SET knownTitles = ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_RES_CHAR_TITLES_FACTION_CHANGE, "UPDATE characters SET chosenTitle = 0 WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_SPELL_COOLDOWN, "DELETE FROM character_spell_cooldown WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHARACTER, "DELETE FROM characters WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_ACTION, "DELETE FROM character_action WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_AURA, "DELETE FROM character_aura WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_GIFT, "DELETE FROM character_gifts WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_INSTANCE, "DELETE FROM character_instance WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_INVENTORY, "DELETE FROM character_inventory WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_QUESTSTATUS_REWARDED, "DELETE FROM character_queststatus_rewarded WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_REPUTATION, "DELETE FROM character_reputation WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_SPELL, "DELETE FROM character_spell WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_MAIL, "DELETE FROM mail WHERE receiver = ?");
        PrepareStatement(CharStatements.CHAR_DEL_MAIL_ITEMS, "DELETE FROM mail_items WHERE receiver = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_ACHIEVEMENTS, "DELETE FROM character_achievement WHERE guid = ? AND achievement NOT BETWEEN '456' AND '467' AND achievement NOT BETWEEN '1400' AND '1427' AND achievement NOT IN(1463, 3117, 3259)");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_EQUIPMENTSETS, "DELETE FROM character_equipmentsets WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_GUILD_EVENTLOG_BY_PLAYER, "DELETE FROM guild_eventlog WHERE PlayerGuid1 = ? OR PlayerGuid2 = ?");
        PrepareStatement(CharStatements.CHAR_DEL_GUILD_BANK_EVENTLOG_BY_PLAYER, "DELETE FROM guild_bank_eventlog WHERE PlayerGuid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_GLYPHS, "DELETE FROM character_glyphs WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_TALENT, "DELETE FROM character_talent WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_SKILLS, "DELETE FROM character_skills WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_UDP_CHAR_HONOR_POINTS, "UPDATE characters SET totalHonorPoints = ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_UDP_CHAR_HONOR_POINTS_ACCUMULATIVE, "UPDATE characters SET totalHonorPoints = totalHonorPoints + ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_UDP_CHAR_ARENA_POINTS, "UPDATE characters SET arenaPoints = ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_UDP_CHAR_ARENA_POINTS_ACCUMULATIVE, "UPDATE characters SET arenaPoints = arenaPoints + ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_UDP_CHAR_MONEY, "UPDATE characters SET money = ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_UDP_CHAR_MONEY_ACCUMULATIVE, "UPDATE characters SET money = money + ? WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_CHAR_REMOVE_GHOST, "UPDATE characters SET playerFlags = (playerFlags & (~16)) WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_INS_CHAR_ACTION, "INSERT INTO character_action (guid, spec, button, action, type) VALUES (?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_UPD_CHAR_ACTION, "UPDATE character_action SET action = ?, type = ? WHERE guid = ? AND button = ? AND spec = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_ACTION_BY_BUTTON_SPEC, "DELETE FROM character_action WHERE guid = ? AND button = ? AND spec = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_INVENTORY_BY_ITEM, "DELETE FROM character_inventory WHERE item = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_INVENTORY_BY_BAG_SLOT, "DELETE FROM character_inventory WHERE bag = ? AND slot = ? AND guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_MAIL, "UPDATE mail SET has_items = ?, expire_time = ?, deliver_time = ?, money = ?, cod = ?, checked = ? WHERE id = ?");
        PrepareStatement(CharStatements.CHAR_REP_CHAR_QUESTSTATUS, "REPLACE INTO character_queststatus (guid, quest, status, explored, timer, mobcount1, mobcount2, mobcount3, mobcount4, itemcount1, itemcount2, itemcount3, itemcount4, itemcount5, itemcount6, playercount) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_QUESTSTATUS_BY_QUEST, "DELETE FROM character_queststatus WHERE guid = ? AND quest = ?");
        PrepareStatement(CharStatements.CHAR_INS_CHAR_QUESTSTATUS_REWARDED, "INSERT IGNORE INTO character_queststatus_rewarded (guid, quest, active) VALUES (?, ?, 1)");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_QUESTSTATUS_REWARDED_BY_QUEST, "DELETE FROM character_queststatus_rewarded WHERE guid = ? AND quest = ?");
        PrepareStatement(CharStatements.CHAR_UPD_CHAR_QUESTSTATUS_REWARDED_FACTION_CHANGE, "UPDATE character_queststatus_rewarded SET quest = ? WHERE quest = ? AND guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_CHAR_QUESTSTATUS_REWARDED_ACTIVE, "UPDATE character_queststatus_rewarded SET active = 1 WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_UPD_CHAR_QUESTSTATUS_REWARDED_ACTIVE_BY_QUEST, "UPDATE character_queststatus_rewarded SET active = 0 WHERE quest = ? AND guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_SKILL_BY_SKILL, "DELETE FROM character_skills WHERE guid = ? AND skill = ?");
        PrepareStatement(CharStatements.CHAR_INS_CHAR_SKILLS, "INSERT INTO character_skills (guid, skill, value, max) VALUES (?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_UDP_CHAR_SKILLS, "UPDATE character_skills SET value = ?, max = ? WHERE guid = ? AND skill = ?");
        PrepareStatement(CharStatements.CHAR_INS_CHAR_SPELL, "INSERT INTO character_spell (guid, spell, specMask) VALUES (?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_STATS, "DELETE FROM character_stats WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_INS_CHAR_STATS, @"INSERT INTO character_stats (guid, maxhealth, maxpower1, maxpower2, maxpower3, maxpower4, maxpower5, maxpower6, maxpower7, strength, agility, stamina, intellect, spirit, 
                         armor, resHoly, resFire, resNature, resFrost, resShadow, resArcane, blockPct, dodgePct, parryPct, critPct, rangedCritPct, spellCritPct, attackPower, rangedAttackPower, 
                         spellPower, resilience) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_STATS, "SELECT maxhealth, strength, agility, stamina, intellect, spirit, armor, attackPower, spellPower, resilience FROM character_stats WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_PETITION_BY_OWNER, "DELETE FROM petition WHERE ownerguid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_PETITION_SIGNATURE_BY_OWNER, "DELETE FROM petition_sign WHERE ownerguid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_PETITION_BY_OWNER_AND_TYPE, "DELETE FROM petition WHERE ownerguid = ? AND type = ?");
        PrepareStatement(CharStatements.CHAR_DEL_PETITION_SIGNATURE_BY_OWNER_AND_TYPE, "DELETE FROM petition_sign WHERE ownerguid = ? AND type = ?");
        PrepareStatement(CharStatements.CHAR_INS_CHAR_GLYPHS, "INSERT INTO character_glyphs VALUES(?, ?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_TALENT_BY_SPELL, "DELETE FROM character_talent WHERE guid = ? AND spell = ?");
        PrepareStatement(CharStatements.CHAR_INS_CHAR_TALENT, "INSERT INTO character_talent (guid, spell, specMask) VALUES (?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_ACTION_EXCEPT_SPEC, "DELETE FROM character_action WHERE spec<>? AND guid = ?");

        // Items that hold loot or money
        PrepareStatement(CharStatements.CHAR_SEL_ITEMCONTAINER_ITEMS, "SELECT containerGUID, itemid, count, item_index, randomPropertyId, randomSuffix, follow_loot_rules, freeforall, is_blocked, is_counted, is_underthreshold, needs_quest, conditionLootId FROM item_loot_storage");
        PrepareStatement(CharStatements.CHAR_DEL_ITEMCONTAINER_SINGLE_ITEM, "DELETE FROM item_loot_storage WHERE containerGUID = ? AND itemid = ? AND count = ? AND item_index = ? LIMIT 1");
        PrepareStatement(CharStatements.CHAR_INS_ITEMCONTAINER_SINGLE_ITEM, "INSERT INTO item_loot_storage (containerGUID, itemid, item_index, count, randomPropertyId, randomSuffix, follow_loot_rules, freeforall, is_blocked, is_counted, is_underthreshold, needs_quest, conditionLootId) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_ITEMCONTAINER_CONTAINER, "DELETE FROM item_loot_storage WHERE containerGUID = ?");

        // Calendar
        PrepareStatement(CharStatements.CHAR_REP_CALENDAR_EVENT, "REPLACE INTO calendar_events (id, creator, title, description, type, dungeon, eventtime, flags, time2) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_CALENDAR_EVENT, "DELETE FROM calendar_events WHERE id = ?");
        PrepareStatement(CharStatements.CHAR_REP_CALENDAR_INVITE, "REPLACE INTO calendar_invites (id, event, invitee, sender, status, statustime, `rank`, text) VALUES (?, ?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_CALENDAR_INVITE, "DELETE FROM calendar_invites WHERE id = ?");

        // Pet
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_PET_IDS, "SELECT id FROM character_pet WHERE owner = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_PET_DECLINEDNAME_BY_OWNER, "DELETE FROM character_pet_declinedname WHERE owner = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_PET_DECLINEDNAME, "DELETE FROM character_pet_declinedname WHERE id = ?");
        PrepareStatement(CharStatements.CHAR_ADD_CHAR_PET_DECLINEDNAME, "INSERT INTO character_pet_declinedname (id, owner, genitive, dative, accusative, instrumental, prepositional) VALUES (?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_SEL_PET_DECLINED_NAME, "SELECT genitive, dative, accusative, instrumental, prepositional FROM character_pet_declinedname WHERE owner = ? AND id = ?");
        PrepareStatement(CharStatements.CHAR_SEL_PET_AURA, "SELECT casterGuid, spell, effectMask, recalculateMask, stackCount, amount0, amount1, amount2, base_amount0, base_amount1, base_amount2, maxDuration, remainTime, remainCharges FROM pet_aura WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_PET_SPELL, "SELECT spell, active FROM pet_spell WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_PET_SPELL_COOLDOWN, "SELECT spell, category, time FROM pet_spell_cooldown WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_PET_AURAS, "DELETE FROM pet_aura WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_PET_SPELLS, "DELETE FROM pet_spell WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_DEL_PET_SPELL_COOLDOWNS, "DELETE FROM pet_spell_cooldown WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_INS_PET_SPELL_COOLDOWN, "INSERT INTO pet_spell_cooldown (guid, spell, category, time) VALUES (?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_PET_SPELL_BY_SPELL, "DELETE FROM pet_spell WHERE guid = ? AND spell = ?");
        PrepareStatement(CharStatements.CHAR_INS_PET_SPELL, "INSERT INTO pet_spell (guid, spell, active) VALUES (?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_INS_PET_AURA, @"INSERT INTO pet_aura (guid, casterGuid, spell, effectMask, recalculateMask, stackCount, amount0, amount1, amount2, 
                         base_amount0, base_amount1, base_amount2, maxDuration, remainTime, remainCharges) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_PETS, "SELECT id, entry, modelid, level, exp, Reactstate, slot, name, renamed, curhealth, curmana, curhappiness, abdata, savetime, CreatedBySpell, PetType FROM character_pet WHERE owner = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_PET_BY_OWNER, "DELETE FROM character_pet WHERE owner = ?");
        PrepareStatement(CharStatements.CHAR_UPD_CHAR_PET_NAME, "UPDATE character_pet SET name = ?, renamed = 1 WHERE owner = ? AND id = ?");
        PrepareStatement(CharStatements.CHAR_UPD_CHAR_PET_SLOT_BY_ID, "UPDATE character_pet SET slot = ? WHERE owner = ? AND id = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_PET_BY_ID, "DELETE FROM character_pet WHERE id = ?");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_PET_BY_SLOT, "DELETE FROM character_pet WHERE owner = ? AND (slot = ? OR slot > ?)");
        PrepareStatement(CharStatements.CHAR_REP_CHAR_PET, "REPLACE INTO character_pet (id, entry, owner, modelid, CreatedBySpell, PetType, level, exp, Reactstate, name, renamed, slot, curhealth, curmana, curhappiness, savetime, abdata) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");

        // PvPstats
        PrepareStatement(CharStatements.CHAR_SEL_PVPSTATS_MAXID, "SELECT MAX(id) FROM pvpstats_battlegrounds");
        PrepareStatement(CharStatements.CHAR_INS_PVPSTATS_BATTLEGROUND, "INSERT INTO pvpstats_battlegrounds (id, winner_faction, bracket_id, type, date) VALUES (?, ?, ?, ?, NOW())");
        PrepareStatement(CharStatements.CHAR_INS_PVPSTATS_PLAYER, "INSERT INTO pvpstats_players (battleground_id, character_guid, winner, score_killing_blows, score_deaths, score_honorable_kills, score_bonus_honor, score_damage_done, score_healing_done, attr_1, attr_2, attr_3, attr_4, attr_5) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_SEL_PVPSTATS_FACTIONS_OVERALL, "SELECT winner_faction, COUNT(*) AS count FROM pvpstats_battlegrounds WHERE DATEDIFF(NOW(), date) < 7 GROUP BY winner_faction ORDER BY winner_faction ASC");
        PrepareStatement(CharStatements.CHAR_SEL_PVPSTATS_BRACKET_MONTH, "SELECT character_guid, COUNT(character_guid) AS count, characters.name as character_name FROM pvpstats_players INNER JOIN pvpstats_battlegrounds ON pvpstats_players.battleground_id = pvpstats_battlegrounds.id AND bracket_id = ? AND MONTH(date) = MONTH(NOW()) AND YEAR(date) = YEAR(NOW()) INNER JOIN characters ON pvpstats_players.character_guid = characters.guid AND characters.deleteDate IS NULL WHERE pvpstats_players.winner = 1 GROUP BY character_guid ORDER BY count(character_guid) DESC LIMIT 0, ?");

        // Deserter tracker
        PrepareStatement(CharStatements.CHAR_INS_DESERTER_TRACK, "INSERT INTO battleground_deserters (guid, type, datetime) VALUES (?, ?, NOW())");

        // QuestTracker
        PrepareStatement(CharStatements.CHAR_INS_QUEST_TRACK, "INSERT INTO quest_tracker (id, character_guid, quest_accept_time, core_hash, core_revision) VALUES (?, ?, NOW(), ?, ?)");
        PrepareStatement(CharStatements.CHAR_UPD_QUEST_TRACK_GM_COMPLETE, "UPDATE quest_tracker SET completed_by_gm = 1 WHERE id = ? AND character_guid = ? ORDER BY quest_accept_time DESC LIMIT 1");
        PrepareStatement(CharStatements.CHAR_UPD_QUEST_TRACK_COMPLETE_TIME, "UPDATE quest_tracker SET quest_complete_time = NOW() WHERE id = ? AND character_guid = ? ORDER BY quest_accept_time DESC LIMIT 1");
        PrepareStatement(CharStatements.CHAR_UPD_QUEST_TRACK_ABANDON_TIME, "UPDATE quest_tracker SET quest_abandon_time = NOW() WHERE id = ? AND character_guid = ? ORDER BY quest_accept_time DESC LIMIT 1");

        // Recovery Item
        PrepareStatement(CharStatements.CHAR_INS_RECOVERY_ITEM, "INSERT INTO recovery_item (Guid, ItemEntry, Count) VALUES (?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_SEL_RECOVERY_ITEM, "SELECT id, itemEntry, Count, Guid FROM recovery_item WHERE id = ?");
        PrepareStatement(CharStatements.CHAR_SEL_RECOVERY_ITEM_LIST, "SELECT id, itemEntry, Count FROM recovery_item WHERE Guid = ? ORDER BY id DESC");
        PrepareStatement(CharStatements.CHAR_DEL_RECOVERY_ITEM, "DELETE FROM recovery_item WHERE Guid = ? AND ItemEntry = ? AND Count = ? ORDER BY Id DESC LIMIT 1");
        PrepareStatement(CharStatements.CHAR_DEL_RECOVERY_ITEM_BY_RECOVERY_ID, "DELETE FROM recovery_item WHERE id = ?");

        PrepareStatement(CharStatements.CHAR_SEL_HONORPOINTS, "SELECT totalHonorPoints FROM characters WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_SEL_ARENAPOINTS, "SELECT arenaPoints FROM characters WHERE guid = ?");

        // Character names
        PrepareStatement(CharStatements.CHAR_INS_RESERVED_PLAYER_NAME, "INSERT IGNORE INTO reserved_name (name) VALUES (?)");
        PrepareStatement(CharStatements.CHAR_INS_PROFANITY_PLAYER_NAME, "INSERT IGNORE INTO profanity_name (name) VALUES (?)");

        // Character settings
        PrepareStatement(CharStatements.CHAR_SEL_CHAR_SETTINGS, "SELECT source, data FROM character_settings WHERE guid = ?");
        PrepareStatement(CharStatements.CHAR_REP_CHAR_SETTINGS, "REPLACE INTO character_settings (guid, source, data) VALUES (?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DEL_CHAR_SETTINGS, "DELETE FROM character_settings WHERE guid = ?");

        // Instance saved data. Stores the states of gameobjects in instances to be loaded on server start
        PrepareStatement(CharStatements.CHAR_SELECT_INSTANCE_SAVED_DATA, "SELECT id, guid, state FROM instance_saved_go_state_data");
        PrepareStatement(CharStatements.CHAR_UPDATE_INSTANCE_SAVED_DATA, "UPDATE instance_saved_go_state_data SET state = ? WHERE guid = ? AND id = ?");
        PrepareStatement(CharStatements.CHAR_INSERT_INSTANCE_SAVED_DATA, "INSERT INTO instance_saved_go_state_data (id, guid, state) VALUES (?, ?, ?)");
        PrepareStatement(CharStatements.CHAR_DELETE_INSTANCE_SAVED_DATA, "DELETE FROM instance_saved_go_state_data WHERE id = ?");
        PrepareStatement(CharStatements.CHAR_SANITIZE_INSTANCE_SAVED_DATA, "DELETE FROM instance_saved_go_state_data WHERE id NOT IN (SELECT instance.id FROM instance)");
    }
}

public enum CharStatements
{
    CHAR_DEL_QUEST_POOL_SAVE,
    CHAR_INS_QUEST_POOL_SAVE,
    CHAR_DEL_NONEXISTENT_GUILD_BANK_ITEM,
    CHAR_DEL_EXPIRED_BANS,
    CHAR_SEL_DATA_BY_NAME,
    CHAR_SEL_DATA_BY_GUID,
    CHAR_SEL_CHECK_NAME,
    CHAR_SEL_CHECK_GUID,
    CHAR_SEL_SUM_CHARS,
    CHAR_SEL_CHAR_CREATE_INFO,
    CHAR_INS_CHARACTER_BAN,
    CHAR_UPD_CHARACTER_BAN,
    CHAR_DEL_CHARACTER_BAN,
    CHAR_SEL_BANINFO,
    CHAR_SEL_GUID_BY_NAME_FILTER,
    CHAR_SEL_BANINFO_LIST,
    CHAR_SEL_BANNED_NAME,
    CHAR_SEL_ENUM,
    CHAR_SEL_ENUM_DECLINED_NAME,
    CHAR_SEL_FREE_NAME,
    CHAR_SEL_CHAR_ZONE,
    CHAR_SEL_CHARACTER_NAME_DATA,
    CHAR_SEL_CHAR_POSITION_XYZ,
    CHAR_SEL_CHAR_POSITION,
    CHAR_DEL_QUEST_STATUS_DAILY,
    CHAR_DEL_QUEST_STATUS_WEEKLY,
    CHAR_DEL_QUEST_STATUS_MONTHLY,
    CHAR_DEL_QUEST_STATUS_SEASONAL,
    CHAR_DEL_QUEST_STATUS_DAILY_CHAR,
    CHAR_DEL_QUEST_STATUS_WEEKLY_CHAR,
    CHAR_DEL_QUEST_STATUS_MONTHLY_CHAR,
    CHAR_DEL_QUEST_STATUS_SEASONAL_CHAR,
    CHAR_DEL_BATTLEGROUND_RANDOM,
    CHAR_INS_BATTLEGROUND_RANDOM,

    CHAR_SEL_CHARACTER,
    CHAR_SEL_CHARACTER_AURAS,
    CHAR_SEL_CHARACTER_SPELL,
    CHAR_SEL_CHARACTER_QUESTSTATUS,
    CHAR_SEL_CHARACTER_DAILYQUESTSTATUS,
    CHAR_SEL_CHARACTER_WEEKLYQUESTSTATUS,
    CHAR_SEL_CHARACTER_MONTHLYQUESTSTATUS,
    CHAR_SEL_CHARACTER_SEASONALQUESTSTATUS,
    CHAR_INS_CHARACTER_DAILYQUESTSTATUS,
    CHAR_INS_CHARACTER_WEEKLYQUESTSTATUS,
    CHAR_INS_CHARACTER_MONTHLYQUESTSTATUS,
    CHAR_INS_CHARACTER_SEASONALQUESTSTATUS,
    CHAR_SEL_CHARACTER_REPUTATION,
    CHAR_SEL_CHARACTER_INVENTORY,
    CHAR_SEL_CHARACTER_ACTIONS,
    CHAR_SEL_CHARACTER_ACTIONS_SPEC,
    CHAR_SEL_CHARACTER_MAILCOUNT_UNREAD,
    CHAR_SEL_CHARACTER_MAILCOUNT_UNREAD_SYNCH,
    CHAR_SEL_MAIL_SERVER_CHARACTER,
    CHAR_REP_MAIL_SERVER_CHARACTER,
    CHAR_SEL_CHARACTER_SOCIALLIST,
    CHAR_SEL_CHARACTER_HOMEBIND,
    CHAR_SEL_CHARACTER_SPELLCOOLDOWNS,
    CHAR_SEL_CHARACTER_DECLINEDNAMES,
    CHAR_SEL_CHARACTER_ACHIEVEMENTS,
    CHAR_SEL_CHARACTER_CRITERIAPROGRESS,
    CHAR_SEL_CHARACTER_EQUIPMENTSETS,
    CHAR_SEL_CHARACTER_ENTRY_POINT,
    CHAR_SEL_CHARACTER_GLYPHS,
    CHAR_SEL_CHARACTER_TALENTS,
    CHAR_SEL_CHARACTER_SKILLS,
    CHAR_SEL_CHARACTER_RANDOMBG,
    CHAR_SEL_CHARACTER_BANNED,
    CHAR_SEL_CHARACTER_QUESTSTATUSREW,
    CHAR_SEL_ACCOUNT_INSTANCELOCKTIMES,
    CHAR_SEL_MAILITEMS,
    CHAR_SEL_BREW_OF_THE_MONTH,
    CHAR_REP_BREW_OF_THE_MONTH,
    CHAR_SEL_AUCTION_ITEMS,
    CHAR_INS_AUCTION,
    CHAR_DEL_AUCTION,
    CHAR_UPD_AUCTION_BID,
    CHAR_SEL_AUCTIONS,
    CHAR_INS_MAIL,
    CHAR_DEL_MAIL_BY_ID,
    CHAR_INS_MAIL_ITEM,
    CHAR_DEL_MAIL_ITEM,
    CHAR_DEL_INVALID_MAIL_ITEM,
    CHAR_SEL_EXPIRED_MAIL,
    CHAR_SEL_EXPIRED_MAIL_ITEMS,
    CHAR_UPD_MAIL_RETURNED,
    CHAR_UPD_MAIL_ITEM_RECEIVER,
    CHAR_UPD_ITEM_OWNER,
    CHAR_SEL_ITEM_REFUNDS,
    CHAR_SEL_ITEM_BOP_TRADE,
    CHAR_DEL_ITEM_BOP_TRADE,
    CHAR_INS_ITEM_BOP_TRADE,
    CHAR_REP_INVENTORY_ITEM,
    CHAR_REP_ITEM_INSTANCE,
    CHAR_UPD_ITEM_INSTANCE,
    CHAR_UPD_ITEM_INSTANCE_ON_LOAD,
    CHAR_DEL_ITEM_INSTANCE,
    CHAR_DEL_ITEM_INSTANCE_BY_OWNER,
    CHAR_UPD_GIFT_OWNER,
    CHAR_DEL_GIFT,
    CHAR_SEL_CHARACTER_GIFT_BY_ITEM,
    CHAR_SEL_ACCOUNT_BY_NAME,
    CHAR_DEL_ACCOUNT_INSTANCE_LOCK_TIMES,
    CHAR_INS_ACCOUNT_INSTANCE_LOCK_TIMES,
    CHAR_SEL_MATCH_MAKER_RATING,
    CHAR_SEL_CHARACTER_COUNT,
    CHAR_UPD_NAME_BY_GUID,
    CHAR_DEL_DECLINED_NAME,

    CHAR_INS_GUILD,
    CHAR_DEL_GUILD,
    CHAR_UPD_GUILD_NAME,
    CHAR_INS_GUILD_MEMBER,
    CHAR_DEL_GUILD_MEMBER,
    CHAR_DEL_GUILD_MEMBERS,
    CHAR_SEL_GUILD_MEMBER_EXTENDED,
    CHAR_INS_GUILD_RANK,
    CHAR_DEL_GUILD_RANKS,
    CHAR_DEL_GUILD_LOWEST_RANK,
    CHAR_INS_GUILD_BANK_TAB,
    CHAR_DEL_GUILD_BANK_TAB,
    CHAR_DEL_GUILD_BANK_TABS,
    CHAR_INS_GUILD_BANK_ITEM,
    CHAR_DEL_GUILD_BANK_ITEM,
    CHAR_DEL_GUILD_BANK_ITEMS,
    CHAR_INS_GUILD_BANK_RIGHT,
    CHAR_DEL_GUILD_BANK_RIGHTS,
    CHAR_DEL_GUILD_BANK_RIGHTS_FOR_RANK,
    CHAR_INS_GUILD_BANK_EVENTLOG,
    CHAR_DEL_GUILD_BANK_EVENTLOG,
    CHAR_DEL_GUILD_BANK_EVENTLOGS,
    CHAR_INS_GUILD_EVENTLOG,
    CHAR_DEL_GUILD_EVENTLOG,
    CHAR_DEL_GUILD_EVENTLOGS,
    CHAR_UPD_GUILD_MEMBER_PNOTE,
    CHAR_UPD_GUILD_MEMBER_OFFNOTE,
    CHAR_UPD_GUILD_MEMBER_RANK,
    CHAR_UPD_GUILD_MOTD,
    CHAR_UPD_GUILD_INFO,
    CHAR_UPD_GUILD_LEADER,
    CHAR_UPD_GUILD_RANK_NAME,
    CHAR_UPD_GUILD_RANK_RIGHTS,
    CHAR_UPD_GUILD_EMBLEM_INFO,
    CHAR_UPD_GUILD_BANK_TAB_INFO,
    CHAR_UPD_GUILD_BANK_MONEY,
    CHAR_UPD_GUILD_BANK_EVENTLOG_TAB,
    CHAR_UPD_GUILD_RANK_BANK_MONEY,
    CHAR_UPD_GUILD_BANK_TAB_TEXT,
    CHAR_INS_GUILD_MEMBER_WITHDRAW,
    CHAR_DEL_GUILD_MEMBER_WITHDRAW,
    CHAR_SEL_CHAR_DATA_FOR_GUILD,

    CHAR_INS_CHANNEL,
    CHAR_UPD_CHANNEL,
    CHAR_DEL_CHANNEL,
    CHAR_UPD_CHANNEL_USAGE,
    CHAR_DEL_OLD_CHANNELS,
    CHAR_DEL_OLD_CHANNELS_BANS,
    CHAR_INS_CHANNEL_BAN,
    CHAR_DEL_CHANNEL_BAN,

    CHAR_UPD_EQUIP_SET,
    CHAR_INS_EQUIP_SET,
    CHAR_DEL_EQUIP_SET,

    CHAR_INS_AURA,

    CHAR_SEL_ACCOUNT_DATA,
    CHAR_REP_ACCOUNT_DATA,
    CHAR_DEL_ACCOUNT_DATA,
    CHAR_SEL_PLAYER_ACCOUNT_DATA,
    CHAR_REP_PLAYER_ACCOUNT_DATA,
    CHAR_DEL_PLAYER_ACCOUNT_DATA,

    CHAR_SEL_TUTORIALS,
    CHAR_SEL_HAS_TUTORIALS,
    CHAR_INS_TUTORIALS,
    CHAR_UPD_TUTORIALS,
    CHAR_DEL_TUTORIALS,

    CHAR_INS_INSTANCE_SAVE,
    CHAR_UPD_INSTANCE_SAVE_DATA,
    CHAR_UPD_INSTANCE_SAVE_ENCOUNTERMASK,

    CHAR_DEL_GAME_EVENT_SAVE,
    CHAR_INS_GAME_EVENT_SAVE,

    CHAR_DEL_ALL_GAME_EVENT_CONDITION_SAVE,
    CHAR_DEL_GAME_EVENT_CONDITION_SAVE,
    CHAR_INS_GAME_EVENT_CONDITION_SAVE,

    CHAR_INS_ARENA_TEAM,
    CHAR_INS_ARENA_TEAM_MEMBER,
    CHAR_DEL_ARENA_TEAM,
    CHAR_DEL_ARENA_TEAM_MEMBERS,
    CHAR_UPD_ARENA_TEAM_CAPTAIN,
    CHAR_DEL_ARENA_TEAM_MEMBER,
    CHAR_UPD_ARENA_TEAM_STATS,
    CHAR_UPD_ARENA_TEAM_MEMBER,
    CHAR_REP_CHARACTER_ARENA_STATS,
    CHAR_SEL_PLAYER_ARENA_TEAMS,
    CHAR_UPD_ARENA_TEAM_NAME,

    CHAR_DEL_ALL_PETITION_SIGNATURES,
    CHAR_DEL_PETITION_SIGNATURE,

    CHAR_INS_PLAYER_ENTRY_POINT,
    CHAR_DEL_PLAYER_ENTRY_POINT,

    CHAR_INS_PLAYER_HOMEBIND,
    CHAR_UPD_PLAYER_HOMEBIND,
    CHAR_DEL_PLAYER_HOMEBIND,

    CHAR_SEL_CORPSES,
    CHAR_INS_CORPSE,
    CHAR_DEL_CORPSE,
    CHAR_DEL_CORPSES_FROM_MAP,
    CHAR_SEL_CORPSE_LOCATION,

    CHAR_SEL_CREATURE_RESPAWNS,
    CHAR_REP_CREATURE_RESPAWN,
    CHAR_DEL_CREATURE_RESPAWN,
    CHAR_DEL_CREATURE_RESPAWN_BY_INSTANCE,

    CHAR_SEL_GO_RESPAWNS,
    CHAR_REP_GO_RESPAWN,
    CHAR_DEL_GO_RESPAWN,
    CHAR_DEL_GO_RESPAWN_BY_INSTANCE,

    CHAR_SEL_GM_TICKETS,
    CHAR_REP_GM_TICKET,
    CHAR_DEL_GM_TICKET,
    CHAR_DEL_ALL_GM_TICKETS,
    CHAR_DEL_PLAYER_GM_TICKETS,
    CHAR_UPD_PLAYER_GM_TICKETS_ON_CHAR_DELETION,

    CHAR_INS_GM_SURVEY,
    CHAR_INS_GM_SUBSURVEY,
    CHAR_INS_LAG_REPORT,

    CHAR_INS_CHARACTER,
    CHAR_UPD_CHARACTER,

    CHAR_UPD_ADD_AT_LOGIN_FLAG,
    CHAR_UPD_REM_AT_LOGIN_FLAG,
    CHAR_UPD_ALL_AT_LOGIN_FLAGS,
    CHAR_INS_BUG_REPORT,
    CHAR_UPD_PETITION_NAME,
    CHAR_INS_PETITION_SIGNATURE,
    CHAR_UPD_ACCOUNT_ONLINE,
    CHAR_INS_GROUP,
    CHAR_REP_GROUP_MEMBER,
    CHAR_DEL_GROUP_MEMBER,
    CHAR_UPD_GROUP_LEADER,
    CHAR_UPD_GROUP_TYPE,
    CHAR_UPD_GROUP_MEMBER_SUBGROUP,
    CHAR_UPD_GROUP_MEMBER_FLAG,
    CHAR_UPD_GROUP_DIFFICULTY,
    CHAR_UPD_GROUP_RAID_DIFFICULTY,
    CHAR_DEL_INVALID_SPELL_SPELLS,
    CHAR_DEL_INVALID_SPELL_TALENTS,
    CHAR_UPD_DELETE_INFO,
    CHAR_UDP_RESTORE_DELETE_INFO,
    CHAR_UPD_ZONE,
    CHAR_UPD_LEVEL,
    CHAR_UPD_XP_ACCUMULATIVE,
    CHAR_DEL_INVALID_ACHIEV_PROGRESS_CRITERIA,
    CHAR_DEL_INVALID_ACHIEVMENT,
    CHAR_INS_ADDON,
    CHAR_DEL_INVALID_PET_SPELL,
    CHAR_UPD_GLOBAL_INSTANCE_RESETTIME,
    CHAR_UPD_CHAR_ONLINE,
    CHAR_UPD_CHAR_NAME_AT_LOGIN,
    CHAR_UPD_WORLDSTATE,
    CHAR_INS_WORLDSTATE,
    CHAR_DEL_CHAR_INSTANCE_BY_INSTANCE,
    CHAR_DEL_CHAR_INSTANCE_BY_INSTANCE_NOT_EXTENDED,
    CHAR_UPD_CHAR_INSTANCE_SET_NOT_EXTENDED,
    CHAR_DEL_CHAR_INSTANCE_BY_INSTANCE_GUID,
    CHAR_UPD_CHAR_INSTANCE,
    CHAR_UPD_CHAR_INSTANCE_EXTENDED,
    CHAR_INS_CHAR_INSTANCE,
    CHAR_INS_ARENA_LOG_FIGHT,
    CHAR_INS_ARENA_LOG_MEMBERSTATS,
    CHAR_UPD_GENDER_AND_APPEARANCE,
    CHAR_DEL_CHARACTER_SKILL,
    CHAR_UPD_ADD_CHARACTER_SOCIAL_FLAGS,
    CHAR_UPD_REM_CHARACTER_SOCIAL_FLAGS,
    CHAR_INS_CHARACTER_SOCIAL,
    CHAR_DEL_CHARACTER_SOCIAL,
    CHAR_UPD_CHARACTER_SOCIAL_NOTE,
    CHAR_UPD_CHARACTER_POSITION,

    CHAR_REP_LFG_DATA,
    CHAR_DEL_LFG_DATA,

    CHAR_SEL_CHARACTER_AURA_FROZEN,
    CHAR_SEL_CHARACTER_ONLINE,

    CHAR_SEL_CHAR_DEL_INFO_BY_GUID,
    CHAR_SEL_CHAR_DEL_INFO_BY_NAME,
    CHAR_SEL_CHAR_DEL_INFO,

    CHAR_SEL_CHARS_BY_ACCOUNT_ID,
    CHAR_SEL_CHAR_PINFO,
    CHAR_SEL_PINFO_XP,
    CHAR_SEL_PINFO_MAILS,
    CHAR_SEL_PINFO_BANS,
    CHAR_SEL_CHAR_HOMEBIND,
    CHAR_SEL_CHAR_GUID_NAME_BY_ACC,
    CHAR_SEL_POOL_QUEST_SAVE,
    CHAR_SEL_CHARACTER_AT_LOGIN,
    CHAR_SEL_CHAR_CLASS_LVL_AT_LOGIN,
    CHAR_SEL_CHAR_CUSTOMIZE_INFO,
    CHAR_SEL_CHAR_RACE_OR_FACTION_CHANGE_INFOS,
    CHAR_SEL_CHAR_AT_LOGIN_TITLES_MONEY,
    CHAR_SEL_CHAR_COD_ITEM_MAIL,
    CHAR_SEL_CHAR_SOCIAL,
    CHAR_SEL_CHAR_OLD_CHARS,
    CHAR_SEL_ARENA_TEAM_ID_BY_PLAYER_GUID,
    CHAR_SEL_MAIL,
    CHAR_SEL_NEXT_MAIL_DELIVERYTIME,
    CHAR_DEL_CHAR_AURA_FROZEN,
    CHAR_SEL_CHAR_INVENTORY_COUNT_ITEM,
    CHAR_SEL_MAIL_COUNT_ITEM,
    CHAR_SEL_AUCTIONHOUSE_COUNT_ITEM,
    CHAR_SEL_GUILD_BANK_COUNT_ITEM,
    CHAR_SEL_CHAR_INVENTORY_ITEM_BY_ENTRY,
    CHAR_SEL_CHAR_INVENTORY_ITEM_BY_ENTRY_AND_OWNER,
    CHAR_SEL_MAIL_ITEMS_BY_ENTRY,
    CHAR_SEL_AUCTIONHOUSE_ITEM_BY_ENTRY,
    CHAR_SEL_GUILD_BANK_ITEM_BY_ENTRY,
    CHAR_DEL_CHAR_ACHIEVEMENT,
    CHAR_DEL_CHAR_ACHIEVEMENT_PROGRESS,
    CHAR_INS_CHAR_ACHIEVEMENT,
    CHAR_DEL_CHAR_ACHIEVEMENT_PROGRESS_BY_CRITERIA,
    CHAR_INS_CHAR_ACHIEVEMENT_PROGRESS,
    CHAR_DEL_CHAR_REPUTATION_BY_FACTION,
    CHAR_INS_CHAR_REPUTATION_BY_FACTION,
    CHAR_UPD_CHAR_ARENA_POINTS,
    CHAR_DEL_ITEM_REFUND_INSTANCE,
    CHAR_INS_ITEM_REFUND_INSTANCE,
    CHAR_DEL_GROUP,
    CHAR_DEL_GROUP_MEMBER_ALL,
    CHAR_INS_CHAR_GIFT,
    CHAR_DEL_INSTANCE_BY_INSTANCE,
    CHAR_DEL_MAIL_ITEM_BY_ID,
    CHAR_INS_PETITION,
    CHAR_DEL_PETITION_BY_GUID,
    CHAR_DEL_PETITION_SIGNATURE_BY_GUID,
    CHAR_DEL_CHAR_DECLINED_NAME,
    CHAR_INS_CHAR_DECLINED_NAME,
    CHAR_UPD_CHAR_RACE,
    CHAR_DEL_CHAR_SKILL_LANGUAGES,
    CHAR_INS_CHAR_SKILL_LANGUAGE,
    CHAR_UPD_CHAR_TAXI_PATH,
    CHAR_UPD_CHAR_TAXIMASK,
    CHAR_DEL_CHAR_QUESTSTATUS,
    CHAR_DEL_CHAR_SOCIAL_BY_GUID,
    CHAR_DEL_CHAR_SOCIAL_BY_FRIEND,
    CHAR_DEL_CHAR_ACHIEVEMENT_BY_ACHIEVEMENT,
    CHAR_UPD_CHAR_ACHIEVEMENT,
    CHAR_UPD_CHAR_INVENTORY_FACTION_CHANGE,
    CHAR_DEL_CHAR_SPELL_BY_SPELL,
    CHAR_UPD_CHAR_SPELL_FACTION_CHANGE,
    CHAR_SEL_CHAR_REP_BY_FACTION,
    CHAR_DEL_CHAR_REP_BY_FACTION,
    CHAR_UPD_CHAR_REP_FACTION_CHANGE,
    CHAR_UPD_CHAR_TITLES_FACTION_CHANGE,
    CHAR_RES_CHAR_TITLES_FACTION_CHANGE,
    CHAR_DEL_CHAR_SPELL_COOLDOWN,
    CHAR_DEL_CHARACTER,
    CHAR_DEL_CHAR_ACTION,
    CHAR_DEL_CHAR_AURA,
    CHAR_DEL_CHAR_GIFT,
    CHAR_DEL_CHAR_INSTANCE,
    CHAR_DEL_CHAR_INVENTORY,
    CHAR_DEL_CHAR_QUESTSTATUS_REWARDED,
    CHAR_DEL_CHAR_REPUTATION,
    CHAR_DEL_CHAR_SPELL,
    CHAR_DEL_MAIL,
    CHAR_DEL_MAIL_ITEMS,
    CHAR_DEL_CHAR_ACHIEVEMENTS,
    CHAR_DEL_CHAR_EQUIPMENTSETS,
    CHAR_DEL_GUILD_EVENTLOG_BY_PLAYER,
    CHAR_DEL_GUILD_BANK_EVENTLOG_BY_PLAYER,
    CHAR_DEL_CHAR_GLYPHS,
    CHAR_DEL_CHAR_TALENT,
    CHAR_DEL_CHAR_SKILLS,
    CHAR_UDP_CHAR_HONOR_POINTS,
    CHAR_UDP_CHAR_HONOR_POINTS_ACCUMULATIVE,
    CHAR_UDP_CHAR_ARENA_POINTS,
    CHAR_UDP_CHAR_ARENA_POINTS_ACCUMULATIVE,
    CHAR_UDP_CHAR_MONEY,
    CHAR_UDP_CHAR_MONEY_ACCUMULATIVE,
    CHAR_UPD_CHAR_REMOVE_GHOST, // pussywizard
    CHAR_INS_CHAR_ACTION,
    CHAR_UPD_CHAR_ACTION,
    CHAR_DEL_CHAR_ACTION_BY_BUTTON_SPEC,
    CHAR_DEL_CHAR_INVENTORY_BY_ITEM,
    CHAR_DEL_CHAR_INVENTORY_BY_BAG_SLOT,
    CHAR_UPD_MAIL,
    CHAR_REP_CHAR_QUESTSTATUS,
    CHAR_DEL_CHAR_QUESTSTATUS_BY_QUEST,
    CHAR_INS_CHAR_QUESTSTATUS_REWARDED,
    CHAR_DEL_CHAR_QUESTSTATUS_REWARDED_BY_QUEST,
    CHAR_UPD_CHAR_QUESTSTATUS_REWARDED_FACTION_CHANGE,
    CHAR_UPD_CHAR_QUESTSTATUS_REWARDED_ACTIVE,
    CHAR_UPD_CHAR_QUESTSTATUS_REWARDED_ACTIVE_BY_QUEST,
    CHAR_DEL_CHAR_SKILL_BY_SKILL,
    CHAR_INS_CHAR_SKILLS,
    CHAR_UDP_CHAR_SKILLS,
    CHAR_INS_CHAR_SPELL,
    CHAR_DEL_CHAR_STATS,
    CHAR_INS_CHAR_STATS,
    CHAR_SEL_CHAR_STATS,
    CHAR_DEL_PETITION_BY_OWNER,
    CHAR_DEL_PETITION_SIGNATURE_BY_OWNER,
    CHAR_DEL_PETITION_BY_OWNER_AND_TYPE,
    CHAR_DEL_PETITION_SIGNATURE_BY_OWNER_AND_TYPE,
    CHAR_INS_CHAR_GLYPHS,
    CHAR_DEL_CHAR_TALENT_BY_SPELL,
    CHAR_INS_CHAR_TALENT,
    CHAR_DEL_CHAR_ACTION_EXCEPT_SPEC,

    CHAR_REP_CALENDAR_EVENT,
    CHAR_DEL_CALENDAR_EVENT,
    CHAR_REP_CALENDAR_INVITE,
    CHAR_DEL_CALENDAR_INVITE,

    CHAR_SEL_PET_AURA,
    CHAR_SEL_PET_SPELL,
    CHAR_SEL_PET_SPELL_COOLDOWN,
    CHAR_DEL_PET_AURAS,
    CHAR_DEL_PET_SPELL_COOLDOWNS,
    CHAR_INS_PET_SPELL_COOLDOWN,
    CHAR_DEL_PET_SPELL_BY_SPELL,
    CHAR_INS_PET_SPELL,
    CHAR_INS_PET_AURA,

    CHAR_DEL_PET_SPELLS,
    CHAR_DEL_CHAR_PET_BY_OWNER,
    CHAR_DEL_CHAR_PET_DECLINEDNAME_BY_OWNER,
    CHAR_SEL_CHAR_PETS,
    CHAR_SEL_CHAR_PET_IDS,
    CHAR_DEL_CHAR_PET_DECLINEDNAME,
    CHAR_ADD_CHAR_PET_DECLINEDNAME,
    CHAR_SEL_PET_DECLINED_NAME,
    CHAR_UPD_CHAR_PET_NAME,
    CHAR_UPD_CHAR_PET_SLOT_BY_ID,
    CHAR_DEL_CHAR_PET_BY_ID,
    CHAR_DEL_CHAR_PET_BY_SLOT,
    CHAR_REP_CHAR_PET,

    CHAR_SEL_ITEMCONTAINER_ITEMS,
    CHAR_DEL_ITEMCONTAINER_SINGLE_ITEM,
    CHAR_INS_ITEMCONTAINER_SINGLE_ITEM,
    CHAR_DEL_ITEMCONTAINER_CONTAINER,

    CHAR_SEL_PVPSTATS_MAXID,
    CHAR_INS_PVPSTATS_BATTLEGROUND,
    CHAR_INS_PVPSTATS_PLAYER,
    CHAR_SEL_PVPSTATS_FACTIONS_OVERALL,
    CHAR_SEL_PVPSTATS_BRACKET_MONTH,

    CHAR_INS_DESERTER_TRACK,

    CHAR_INS_QUEST_TRACK,
    CHAR_UPD_QUEST_TRACK_GM_COMPLETE,
    CHAR_UPD_QUEST_TRACK_COMPLETE_TIME,
    CHAR_UPD_QUEST_TRACK_ABANDON_TIME,

    CHAR_INS_RECOVERY_ITEM,
    CHAR_SEL_RECOVERY_ITEM,
    CHAR_SEL_RECOVERY_ITEM_LIST,
    CHAR_DEL_RECOVERY_ITEM,
    CHAR_DEL_RECOVERY_ITEM_BY_RECOVERY_ID,

    CHAR_SEL_HONORPOINTS,
    CHAR_SEL_ARENAPOINTS,

    CHAR_INS_RESERVED_PLAYER_NAME,
    CHAR_INS_PROFANITY_PLAYER_NAME,

    CHAR_SEL_CHAR_SETTINGS,
    CHAR_REP_CHAR_SETTINGS,
    CHAR_DEL_CHAR_SETTINGS,

    CHAR_SELECT_INSTANCE_SAVED_DATA,
    CHAR_UPDATE_INSTANCE_SAVED_DATA,
    CHAR_INSERT_INSTANCE_SAVED_DATA,
    CHAR_DELETE_INSTANCE_SAVED_DATA,
    CHAR_SANITIZE_INSTANCE_SAVED_DATA,

    MAX_CHARACTERDATABASE_STATEMENTS
}
