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

public class LoginDatabase : MySqlBase<LoginStatements>
{
    public override void PreparedStatements()
    {
        PrepareStatement(LoginStatements.LOGIN_SEL_LOGONCHALLENGE,
            @"SELECT a.id, a.username, a.locked, a.lock_country, a.last_ip, a.failed_logins, 
            ab.unbandate > UNIX_TIMESTAMP() OR ab.unbandate = ab.bandate, ab.unbandate = ab.bandate, 
            ipb.unbandate > UNIX_TIMESTAMP() OR ipb.unbandate = ipb.bandate, ipb.unbandate = ipb.bandate, 
            aa.gmlevel, a.totp_secret, a.salt, a.verifier 
            FROM account a 
            LEFT JOIN account_access aa ON a.id = aa.id 
            LEFT JOIN account_banned ab ON ab.id = a.id AND ab.active = 1 
            LEFT JOIN ip_banned ipb ON ipb.ip = ? 
            WHERE a.username = ?");
        PrepareStatement(LoginStatements.LOGIN_SEL_RECONNECTCHALLENGE,
            @"SELECT a.id, a.username, a.locked, a.lock_country, a.last_ip, a.failed_logins, 
            ab.unbandate > UNIX_TIMESTAMP() OR ab.unbandate = ab.bandate, ab.unbandate = ab.bandate, 
            ipb.unbandate > UNIX_TIMESTAMP() OR ipb.unbandate = ipb.bandate, ipb.unbandate = ipb.bandate, 
            aa.gmlevel, a.session_key 
            FROM account a 
            LEFT JOIN account_access aa ON a.id = aa.id 
            LEFT JOIN account_banned ab ON ab.id = a.id AND ab.active = 1 
            LEFT JOIN ip_banned ipb ON ipb.ip = ? 
            WHERE a.username = ? AND a.session_key IS NOT NULL");
        PrepareStatement(LoginStatements.LOGIN_SEL_ACCOUNT_INFO_BY_NAME,
            @"SELECT a.id, a.session_key, a.last_ip, a.locked, a.lock_country, a.expansion, a.mutetime, a.locale, a.recruiter, a.os, a.totaltime, 
            aa.gmlevel, ab.unbandate > UNIX_TIMESTAMP() OR ab.unbandate = ab.bandate, r.id FROM account a LEFT JOIN account_access aa ON a.id = aa.id AND aa.RealmID IN (-1, ?) 
            LEFT JOIN account_banned ab ON a.id = ab.id AND ab.active = 1 LEFT JOIN account r ON a.id = r.recruiter WHERE a.username = ? 
            AND a.session_key IS NOT NULL ORDER BY aa.RealmID DESC LIMIT 1");
        PrepareStatement(LoginStatements.LOGIN_SEL_IP_INFO, "SELECT unbandate > UNIX_TIMESTAMP() OR unbandate = bandate AS banned, NULL as country FROM ip_banned WHERE ip = ?");
        PrepareStatement(LoginStatements.LOGIN_SEL_REALMLIST, "SELECT id, name, address, localAddress, localSubnetMask, port, icon, flag, timezone, allowedSecurityLevel, population, gamebuild FROM realmlist WHERE flag <> 3 ORDER BY name");
        PrepareStatement(LoginStatements.LOGIN_DEL_EXPIRED_IP_BANS, "DELETE FROM ip_banned WHERE unbandate<>bandate AND unbandate<=UNIX_TIMESTAMP()");
        PrepareStatement(LoginStatements.LOGIN_UPD_EXPIRED_ACCOUNT_BANS, "UPDATE account_banned SET active = 0 WHERE active = 1 AND unbandate<>bandate AND unbandate<=UNIX_TIMESTAMP()");
        PrepareStatement(LoginStatements.LOGIN_SEL_IP_BANNED, "SELECT * FROM ip_banned WHERE ip = ?");
        PrepareStatement(LoginStatements.LOGIN_INS_IP_AUTO_BANNED, "INSERT INTO ip_banned (ip, bandate, unbandate, bannedby, banreason) VALUES (?, UNIX_TIMESTAMP(), UNIX_TIMESTAMP()+?, 'Trinity realmd', 'Failed login autoban')");
        PrepareStatement(LoginStatements.LOGIN_SEL_IP_BANNED_ALL, "SELECT ip, bandate, unbandate, bannedby, banreason FROM ip_banned WHERE (bandate = unbandate OR unbandate > UNIX_TIMESTAMP()) ORDER BY unbandate");
        PrepareStatement(LoginStatements.LOGIN_SEL_IP_BANNED_BY_IP, "SELECT ip, bandate, unbandate, bannedby, banreason FROM ip_banned WHERE (bandate = unbandate OR unbandate > UNIX_TIMESTAMP()) AND ip LIKE CONCAT('%%', ?, '%%') ORDER BY unbandate");
        PrepareStatement(LoginStatements.LOGIN_SEL_ACCOUNT_BANNED, "SELECT bandate, unbandate FROM account_banned WHERE id = ? AND active = 1");
        PrepareStatement(LoginStatements.LOGIN_SEL_ACCOUNT_BANNED_ALL, "SELECT account.id, username FROM account, account_banned WHERE account.id = account_banned.id AND active = 1 GROUP BY account.id");
        PrepareStatement(LoginStatements.LOGIN_SEL_ACCOUNT_BANNED_BY_USERNAME, "SELECT account.id, username FROM account, account_banned WHERE account.id = account_banned.id AND active = 1 AND username LIKE CONCAT('%%', ?, '%%') GROUP BY account.id");
        PrepareStatement(LoginStatements.LOGIN_INS_ACCOUNT_AUTO_BANNED, "INSERT INTO account_banned VALUES (?, UNIX_TIMESTAMP(), UNIX_TIMESTAMP()+?, 'Trinity realmd', 'Failed login autoban', 1)");
        PrepareStatement(LoginStatements.LOGIN_DEL_ACCOUNT_BANNED, "DELETE FROM account_banned WHERE id = ?");
        PrepareStatement(LoginStatements.LOGIN_UPD_LOGON, "UPDATE account SET salt = ?, verifier = ? WHERE id = ?");
        PrepareStatement(LoginStatements.LOGIN_UPD_LOGONPROOF, "UPDATE account SET session_key = ?, last_ip = ?, last_login = NOW(), locale = ?, failed_logins = 0, os = ? WHERE username = ?");
        PrepareStatement(LoginStatements.LOGIN_UPD_FAILEDLOGINS, "UPDATE account SET failed_logins = failed_logins + 1 WHERE username = ?");
        PrepareStatement(LoginStatements.LOGIN_SEL_FAILEDLOGINS, "SELECT id, failed_logins FROM account WHERE username = ?");
        PrepareStatement(LoginStatements.LOGIN_SEL_ACCOUNT_ID_BY_NAME, "SELECT id FROM account WHERE username = ?");
        PrepareStatement(LoginStatements.LOGIN_SEL_ACCOUNT_LIST_BY_NAME, "SELECT id, username FROM account WHERE username = ?");
        PrepareStatement(LoginStatements.LOGIN_SEL_ACCOUNT_LIST_BY_EMAIL, "SELECT id, username FROM account WHERE email = ?");
        PrepareStatement(LoginStatements.LOGIN_SEL_NUM_CHARS_ON_REALM, "SELECT numchars FROM realmcharacters WHERE realmid = ? AND acctid= ?");
        PrepareStatement(LoginStatements.LOGIN_SEL_REALM_CHARACTER_COUNTS, "SELECT realmid, numchars FROM realmcharacters WHERE acctid = ?");
        PrepareStatement(LoginStatements.LOGIN_SEL_ACCOUNT_BY_IP, "SELECT id, username FROM account WHERE last_ip = ?");
        PrepareStatement(LoginStatements.LOGIN_SEL_ACCOUNT_BY_ID, "SELECT 1 FROM account WHERE id = ?");
        PrepareStatement(LoginStatements.LOGIN_INS_IP_BANNED, "INSERT INTO ip_banned (ip, bandate, unbandate, bannedby, banreason) VALUES (?, UNIX_TIMESTAMP(), UNIX_TIMESTAMP()+?, ?, ?)");
        PrepareStatement(LoginStatements.LOGIN_DEL_IP_NOT_BANNED, "DELETE FROM ip_banned WHERE ip = ?");
        PrepareStatement(LoginStatements.LOGIN_INS_ACCOUNT_BANNED, "INSERT INTO account_banned VALUES (?, UNIX_TIMESTAMP(), UNIX_TIMESTAMP()+?, ?, ?, 1)");
        PrepareStatement(LoginStatements.LOGIN_UPD_ACCOUNT_NOT_BANNED, "UPDATE account_banned SET active = 0 WHERE id = ? AND active != 0");
        PrepareStatement(LoginStatements.LOGIN_DEL_REALM_CHARACTERS, "DELETE FROM realmcharacters WHERE acctid = ?");
        PrepareStatement(LoginStatements.LOGIN_REP_REALM_CHARACTERS, "REPLACE INTO realmcharacters (numchars, acctid, realmid) VALUES (?, ?, ?)");
        PrepareStatement(LoginStatements.LOGIN_SEL_SUM_REALM_CHARACTERS, "SELECT SUM(numchars) FROM realmcharacters WHERE acctid = ?");
        PrepareStatement(LoginStatements.LOGIN_INS_ACCOUNT, "INSERT INTO account(username, salt, verifier, expansion, joindate) VALUES(?, ?, ?, ?, NOW())");
        PrepareStatement(LoginStatements.LOGIN_INS_REALM_CHARACTERS_INIT, "INSERT INTO realmcharacters (realmid, acctid, numchars) SELECT realmlist.id, account.id, 0 FROM realmlist, account LEFT JOIN realmcharacters ON acctid=account.id WHERE acctid IS NULL");
        PrepareStatement(LoginStatements.LOGIN_UPD_EXPANSION, "UPDATE account SET expansion = ? WHERE id = ?");
        PrepareStatement(LoginStatements.LOGIN_UPD_ACCOUNT_LOCK, "UPDATE account SET locked = ? WHERE id = ?");
        PrepareStatement(LoginStatements.LOGIN_UPD_ACCOUNT_LOCK_COUNTRY, "UPDATE account SET lock_country = ? WHERE id = ?");
        PrepareStatement(LoginStatements.LOGIN_UPD_USERNAME, "UPDATE account SET username = ? WHERE id = ?");
        PrepareStatement(LoginStatements.LOGIN_UPD_MUTE_TIME, "UPDATE account SET mutetime = ? , mutereason = ? , muteby = ? WHERE id = ?");
        PrepareStatement(LoginStatements.LOGIN_UPD_MUTE_TIME_LOGIN, "UPDATE account SET mutetime = ? WHERE id = ?");
        PrepareStatement(LoginStatements.LOGIN_UPD_LAST_IP, "UPDATE account SET last_ip = ? WHERE username = ?");
        PrepareStatement(LoginStatements.LOGIN_UPD_LAST_ATTEMPT_IP, "UPDATE account SET last_attempt_ip = ? WHERE username = ?");
        PrepareStatement(LoginStatements.LOGIN_UPD_ACCOUNT_ONLINE, "UPDATE account SET online = ? WHERE id = ?");
        PrepareStatement(LoginStatements.LOGIN_UPD_UPTIME_PLAYERS, "UPDATE uptime SET uptime = ?, maxplayers = ? WHERE realmid = ? AND starttime = ?");
        PrepareStatement(LoginStatements.LOGIN_DEL_OLD_LOGS, "DELETE FROM logs WHERE (time + ?) < ?");
        PrepareStatement(LoginStatements.LOGIN_DEL_ACCOUNT_ACCESS, "DELETE FROM account_access WHERE id = ?");
        PrepareStatement(LoginStatements.LOGIN_DEL_ACCOUNT_ACCESS_BY_REALM, "DELETE FROM account_access WHERE id = ? AND (RealmID = ? OR RealmID = -1)");
        PrepareStatement(LoginStatements.LOGIN_INS_ACCOUNT_ACCESS, "INSERT INTO account_access (id,gmlevel,RealmID) VALUES (?, ?, ?)");
        PrepareStatement(LoginStatements.LOGIN_GET_ACCOUNT_ID_BY_USERNAME, "SELECT id FROM account WHERE username = ?");
        PrepareStatement(LoginStatements.LOGIN_GET_ACCOUNT_ACCESS_GMLEVEL, "SELECT gmlevel FROM account_access WHERE id = ?");
        PrepareStatement(LoginStatements.LOGIN_GET_GMLEVEL_BY_REALMID, "SELECT gmlevel FROM account_access WHERE id = ? AND (RealmID = ? OR RealmID = -1)");
        PrepareStatement(LoginStatements.LOGIN_GET_USERNAME_BY_ID, "SELECT username FROM account WHERE id = ?");
        PrepareStatement(LoginStatements.LOGIN_SEL_CHECK_PASSWORD, "SELECT salt, verifier FROM account WHERE id = ?");
        PrepareStatement(LoginStatements.LOGIN_SEL_CHECK_PASSWORD_BY_NAME, "SELECT salt, verifier FROM account WHERE username = ?");
        PrepareStatement(LoginStatements.LOGIN_SEL_PINFO, "SELECT a.username, aa.gmlevel, a.email, a.reg_mail, a.last_ip, DATE_FORMAT(a.last_login, '%Y-%m-%d %T'), a.mutetime, a.mutereason, a.muteby, a.failed_logins, a.locked, a.OS FROM account a LEFT JOIN account_access aa ON (a.id = aa.id AND (aa.RealmID = ? OR aa.RealmID = -1)) WHERE a.id = ?");
        PrepareStatement(LoginStatements.LOGIN_SEL_PINFO_BANS, "SELECT unbandate, bandate = unbandate, bannedby, banreason FROM account_banned WHERE id = ? AND active ORDER BY bandate ASC LIMIT 1");
        PrepareStatement(LoginStatements.LOGIN_SEL_GM_ACCOUNTS, "SELECT a.username, aa.gmlevel FROM account a, account_access aa WHERE a.id=aa.id AND aa.gmlevel >= ? AND (aa.realmid = -1 OR aa.realmid = ?)");
        PrepareStatement(LoginStatements.LOGIN_SEL_ACCOUNT_INFO, "SELECT a.username, a.last_ip, aa.gmlevel, a.expansion FROM account a LEFT JOIN account_access aa ON (a.id = aa.id) WHERE a.id = ? ORDER BY a.last_ip"); // Only used in ".account onlinelist" command
        PrepareStatement(LoginStatements.LOGIN_SEL_ACCOUNT_ACCESS_GMLEVEL_TEST, "SELECT 1 FROM account_access WHERE id = ? AND gmlevel > ?");
        PrepareStatement(LoginStatements.LOGIN_SEL_ACCOUNT_ACCESS, "SELECT a.id, aa.gmlevel, aa.RealmID FROM account a LEFT JOIN account_access aa ON (a.id = aa.id) WHERE a.username = ?");
        PrepareStatement(LoginStatements.LOGIN_SEL_ACCOUNT_RECRUITER, "SELECT 1 FROM account WHERE recruiter = ?");
        PrepareStatement(LoginStatements.LOGIN_SEL_BANS, "SELECT 1 FROM account_banned WHERE id = ? AND active = 1 UNION SELECT 1 FROM ip_banned WHERE ip = ?");
        PrepareStatement(LoginStatements.LOGIN_SEL_ACCOUNT_WHOIS, "SELECT username, email, last_ip FROM account WHERE id = ?");
        PrepareStatement(LoginStatements.LOGIN_SEL_LAST_ATTEMPT_IP, "SELECT last_attempt_ip FROM account WHERE id = ?");
        PrepareStatement(LoginStatements.LOGIN_SEL_LAST_IP, "SELECT last_ip FROM account WHERE id = ?");
        PrepareStatement(LoginStatements.LOGIN_SEL_REALMLIST_SECURITY_LEVEL, "SELECT allowedSecurityLevel from realmlist WHERE id = ?");
        PrepareStatement(LoginStatements.LOGIN_DEL_ACCOUNT, "DELETE FROM account WHERE id = ?");
        PrepareStatement(LoginStatements.LOGIN_SEL_AUTOBROADCAST, "SELECT id, weight, text FROM autobroadcast WHERE realmid = ? OR realmid = -1");
        PrepareStatement(LoginStatements.LOGIN_SEL_MOTD, "SELECT text FROM motd WHERE realmid = ? OR realmid = -1 ORDER BY realmid DESC");
        PrepareStatement(LoginStatements.LOGIN_REP_MOTD, "REPLACE INTO motd (realmid, text) VALUES (?, ?)");
        PrepareStatement(LoginStatements.LOGIN_INS_ACCOUNT_MUTE, "INSERT INTO account_muted VALUES (?, UNIX_TIMESTAMP(), ?, ?, ?)");
        PrepareStatement(LoginStatements.LOGIN_SEL_ACCOUNT_MUTE_INFO, "SELECT mutedate, mutetime, mutereason, mutedby FROM account_muted WHERE guid = ? ORDER BY mutedate ASC");
        PrepareStatement(LoginStatements.LOGIN_DEL_ACCOUNT_MUTED, "DELETE FROM account_muted WHERE guid = ?");
        // 0: uint32, 1: uint32, 2: uint8, 3: uint32, 4: string // Complete name: "Login_Insert_AccountLoginDeLete_IP_Logging"
        PrepareStatement(LoginStatements.LOGIN_INS_ALDL_IP_LOGGING, "INSERT INTO logs_ip_actions (account_id,character_guid,type,ip,systemnote,unixtime,time) VALUES (?, ?, ?, (SELECT last_ip FROM account WHERE id = ?), ?, unix_timestamp(NOW()), NOW())");
        // 0: uint32, 1: uint32, 2: uint8, 3: uint32, 4: string // Complete name: "Login_Insert_FailedAccountLogin_IP_Logging"
        PrepareStatement(LoginStatements.LOGIN_INS_FACL_IP_LOGGING, "INSERT INTO logs_ip_actions (account_id,character_guid,type,ip,systemnote,unixtime,time) VALUES (?, ?, ?, (SELECT last_attempt_ip FROM account WHERE id = ?), ?, unix_timestamp(NOW()), NOW())");
        // 0: uint32, 1: uint32, 2: uint8, 3: string, 4: string // Complete name: "Login_Insert_CharacterDelete_IP_Logging"
        PrepareStatement(LoginStatements.LOGIN_INS_CHAR_IP_LOGGING, "INSERT INTO logs_ip_actions (account_id,character_guid,type,ip,systemnote,unixtime,time) VALUES (?, ?, ?, ?, ?, unix_timestamp(NOW()), NOW())");
        // 0: string, 1: string, 2: string                      // Complete name: "Login_Insert_Failed_Account_Login_due_password_IP_Logging"
        PrepareStatement(LoginStatements.LOGIN_INS_FALP_IP_LOGGING, "INSERT INTO logs_ip_actions (account_id,character_guid,type,ip,systemnote,unixtime,time) VALUES (?, 0, 1, ?, ?, unix_timestamp(NOW()), NOW())");

        // DB logging
        PrepareStatement(LoginStatements.LOGIN_INS_LOG, "INSERT INTO logs (time, realm, type, level, string) VALUES (?, ?, ?, ?, ?)");

        // TOTP
        PrepareStatement(LoginStatements.LOGIN_SEL_SECRET_DIGEST, "SELECT digest FROM secret_digest WHERE id = ?");
        PrepareStatement(LoginStatements.LOGIN_INS_SECRET_DIGEST, "INSERT INTO secret_digest (id, digest) VALUES (?,?)");
        PrepareStatement(LoginStatements.LOGIN_DEL_SECRET_DIGEST, "DELETE FROM secret_digest WHERE id = ?");

        PrepareStatement(LoginStatements.LOGIN_SEL_ACCOUNT_TOTP_SECRET, "SELECT totp_secret FROM account WHERE id = ?");
        PrepareStatement(LoginStatements.LOGIN_UPD_ACCOUNT_TOTP_SECRET, "UPDATE account SET totp_secret = ? WHERE id = ?");
    }
}

public enum LoginStatements
{
    LOGIN_SEL_REALMLIST,
    LOGIN_DEL_EXPIRED_IP_BANS,
    LOGIN_UPD_EXPIRED_ACCOUNT_BANS,
    LOGIN_SEL_IP_INFO,
    LOGIN_SEL_IP_BANNED,
    LOGIN_INS_IP_AUTO_BANNED,
    LOGIN_SEL_ACCOUNT_BANNED,
    LOGIN_SEL_ACCOUNT_BANNED_ALL,
    LOGIN_SEL_ACCOUNT_BANNED_BY_USERNAME,
    LOGIN_INS_ACCOUNT_AUTO_BANNED,
    LOGIN_DEL_ACCOUNT_BANNED,
    LOGIN_UPD_LOGON,
    LOGIN_UPD_LOGONPROOF,
    LOGIN_SEL_LOGONCHALLENGE,
    LOGIN_SEL_RECONNECTCHALLENGE,
    LOGIN_UPD_FAILEDLOGINS,
    LOGIN_SEL_FAILEDLOGINS,
    LOGIN_SEL_ACCOUNT_ID_BY_NAME,
    LOGIN_SEL_ACCOUNT_LIST_BY_NAME,
    LOGIN_SEL_ACCOUNT_INFO_BY_NAME,
    LOGIN_SEL_ACCOUNT_LIST_BY_EMAIL,
    LOGIN_SEL_NUM_CHARS_ON_REALM,
    LOGIN_SEL_REALM_CHARACTER_COUNTS,
    LOGIN_SEL_ACCOUNT_BY_IP,
    LOGIN_INS_IP_BANNED,
    LOGIN_DEL_IP_NOT_BANNED,
    LOGIN_SEL_IP_BANNED_ALL,
    LOGIN_SEL_IP_BANNED_BY_IP,
    LOGIN_SEL_ACCOUNT_BY_ID,
    LOGIN_INS_ACCOUNT_BANNED,
    LOGIN_UPD_ACCOUNT_NOT_BANNED,
    LOGIN_DEL_REALM_CHARACTERS,
    LOGIN_REP_REALM_CHARACTERS,
    LOGIN_SEL_SUM_REALM_CHARACTERS,
    LOGIN_INS_ACCOUNT,
    LOGIN_INS_REALM_CHARACTERS_INIT,
    LOGIN_UPD_EXPANSION,
    LOGIN_UPD_ACCOUNT_LOCK,
    LOGIN_UPD_ACCOUNT_LOCK_COUNTRY,
    LOGIN_UPD_USERNAME,
    LOGIN_UPD_MUTE_TIME,
    LOGIN_UPD_MUTE_TIME_LOGIN,
    LOGIN_UPD_LAST_IP,
    LOGIN_UPD_LAST_ATTEMPT_IP,
    LOGIN_UPD_ACCOUNT_ONLINE,
    LOGIN_UPD_UPTIME_PLAYERS,
    LOGIN_DEL_OLD_LOGS,
    LOGIN_DEL_ACCOUNT_ACCESS,
    LOGIN_DEL_ACCOUNT_ACCESS_BY_REALM,
    LOGIN_INS_ACCOUNT_ACCESS,
    LOGIN_GET_ACCOUNT_ID_BY_USERNAME,
    LOGIN_GET_ACCOUNT_ACCESS_GMLEVEL,
    LOGIN_GET_GMLEVEL_BY_REALMID,
    LOGIN_GET_USERNAME_BY_ID,
    LOGIN_SEL_CHECK_PASSWORD,
    LOGIN_SEL_CHECK_PASSWORD_BY_NAME,
    LOGIN_SEL_PINFO,
    LOGIN_SEL_PINFO_BANS,
    LOGIN_SEL_GM_ACCOUNTS,
    LOGIN_SEL_ACCOUNT_INFO,
    LOGIN_SEL_ACCOUNT_ACCESS_GMLEVEL_TEST,
    LOGIN_SEL_ACCOUNT_ACCESS,
    LOGIN_SEL_ACCOUNT_RECRUITER,
    LOGIN_SEL_BANS,
    LOGIN_SEL_ACCOUNT_WHOIS,
    LOGIN_SEL_REALMLIST_SECURITY_LEVEL,
    LOGIN_DEL_ACCOUNT,
    LOGIN_SEL_AUTOBROADCAST,
    LOGIN_SEL_MOTD,
    LOGIN_REP_MOTD,
    LOGIN_SEL_LAST_ATTEMPT_IP,
    LOGIN_SEL_LAST_IP,
    LOGIN_INS_ALDL_IP_LOGGING,
    LOGIN_INS_FACL_IP_LOGGING,
    LOGIN_INS_CHAR_IP_LOGGING,
    LOGIN_INS_FALP_IP_LOGGING,

    LOGIN_INS_ACCOUNT_MUTE,
    LOGIN_SEL_ACCOUNT_MUTE_INFO,
    LOGIN_DEL_ACCOUNT_MUTED,

    LOGIN_INS_LOG,

    LOGIN_SEL_SECRET_DIGEST,
    LOGIN_INS_SECRET_DIGEST,
    LOGIN_DEL_SECRET_DIGEST,

    LOGIN_SEL_ACCOUNT_TOTP_SECRET,
    LOGIN_UPD_ACCOUNT_TOTP_SECRET,

    MAX_LOGINDATABASE_STATEMENTS
}
