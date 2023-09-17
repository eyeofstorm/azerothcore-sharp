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
using AzerothCore.Constants;
using AzerothCore.Database;
using AzerothCore.DataStores;
using AzerothCore.Logging;

namespace AzerothCore.Game;

[Flags]
public enum CharacterFlags : uint
{
    CHARACTER_FLAG_NONE = 0x00000000,
    CHARACTER_FLAG_UNK1 = 0x00000001,
    CHARACTER_FLAG_UNK2 = 0x00000002,
    CHARACTER_LOCKED_FOR_TRANSFER = 0x00000004,
    CHARACTER_FLAG_UNK4 = 0x00000008,
    CHARACTER_FLAG_UNK5 = 0x00000010,
    CHARACTER_FLAG_UNK6 = 0x00000020,
    CHARACTER_FLAG_UNK7 = 0x00000040,
    CHARACTER_FLAG_UNK8 = 0x00000080,
    CHARACTER_FLAG_UNK9 = 0x00000100,
    CHARACTER_FLAG_UNK10 = 0x00000200,
    CHARACTER_FLAG_HIDE_HELM = 0x00000400,
    CHARACTER_FLAG_HIDE_CLOAK = 0x00000800,
    CHARACTER_FLAG_UNK13 = 0x00001000,
    CHARACTER_FLAG_GHOST = 0x00002000,
    CHARACTER_FLAG_RENAME = 0x00004000,
    CHARACTER_FLAG_UNK16 = 0x00008000,
    CHARACTER_FLAG_UNK17 = 0x00010000,
    CHARACTER_FLAG_UNK18 = 0x00020000,
    CHARACTER_FLAG_UNK19 = 0x00040000,
    CHARACTER_FLAG_UNK20 = 0x00080000,
    CHARACTER_FLAG_UNK21 = 0x00100000,
    CHARACTER_FLAG_UNK22 = 0x00200000,
    CHARACTER_FLAG_UNK23 = 0x00400000,
    CHARACTER_FLAG_UNK24 = 0x00800000,
    CHARACTER_FLAG_LOCKED_BY_BILLING = 0x01000000,
    CHARACTER_FLAG_DECLINED = 0x02000000,
    CHARACTER_FLAG_UNK27 = 0x04000000,
    CHARACTER_FLAG_UNK28 = 0x08000000,
    CHARACTER_FLAG_UNK29 = 0x10000000,
    CHARACTER_FLAG_UNK30 = 0x20000000,
    CHARACTER_FLAG_UNK31 = 0x40000000,
    CHARACTER_FLAG_UNK32 = 0x80000000
}

public enum CharacterCustomizeFlags : uint
{
    CHAR_CUSTOMIZE_FLAG_NONE = 0x00000000,
    CHAR_CUSTOMIZE_FLAG_CUSTOMIZE = 0x00000001,       // name, gender, etc...
    CHAR_CUSTOMIZE_FLAG_FACTION = 0x00010000,       // name, gender, faction, etc...
    CHAR_CUSTOMIZE_FLAG_RACE = 0x00100000        // name, gender, race, etc...
}

public enum SpellModType
{
    SPELLMOD_FLAT = 107,                            // SPELL_AURA_ADD_FLAT_MODIFIER
    SPELLMOD_PCT = 108                             // SPELL_AURA_ADD_PCT_MODIFIER
}

[Flags]
public enum PlayerUnderwaterState
{
    UNDERWATER_NONE = 0x00,
    UNDERWATER_INWATER = 0x01,             // terrain type is water and player is afflicted by it
    UNDERWATER_INLAVA = 0x02,             // terrain type is lava and player is afflicted by it
    UNDERWATER_INSLIME = 0x04,             // terrain type is lava and player is afflicted by it
    UNDERWATER_INDARKWATER = 0x08,             // terrain type is dark water and player is afflicted by it

    UNDERWATER_EXIST_TIMERS = 0x10
}

public enum BuyBankSlotResult
{
    ERR_BANKSLOT_FAILED_TOO_MANY = 0,
    ERR_BANKSLOT_INSUFFICIENT_FUNDS = 1,
    ERR_BANKSLOT_NOTBANKER = 2,
    ERR_BANKSLOT_OK = 3
}

public enum PlayerSpellState
{
    PLAYERSPELL_UNCHANGED = 0,
    PLAYERSPELL_CHANGED = 1,
    PLAYERSPELL_NEW = 2,
    PLAYERSPELL_REMOVED = 3,
    PLAYERSPELL_TEMPORARY = 4
}

public enum TalentTree // talent tabs
{
    TALENT_TREE_WARRIOR_ARMS = 161,
    TALENT_TREE_WARRIOR_FURY = 164,
    TALENT_TREE_WARRIOR_PROTECTION = 163,
    TALENT_TREE_PALADIN_HOLY = 382,
    TALENT_TREE_PALADIN_PROTECTION = 383,
    TALENT_TREE_PALADIN_RETRIBUTION = 381,
    TALENT_TREE_HUNTER_BEAST_MASTERY = 361,
    TALENT_TREE_HUNTER_MARKSMANSHIP = 363,
    TALENT_TREE_HUNTER_SURVIVAL = 362,
    TALENT_TREE_ROGUE_ASSASSINATION = 182,
    TALENT_TREE_ROGUE_COMBAT = 181,
    TALENT_TREE_ROGUE_SUBTLETY = 183,
    TALENT_TREE_PRIEST_DISCIPLINE = 201,
    TALENT_TREE_PRIEST_HOLY = 202,
    TALENT_TREE_PRIEST_SHADOW = 203,
    TALENT_TREE_DEATH_KNIGHT_BLOOD = 398,
    TALENT_TREE_DEATH_KNIGHT_FROST = 399,
    TALENT_TREE_DEATH_KNIGHT_UNHOLY = 400,
    TALENT_TREE_SHAMAN_ELEMENTAL = 261,
    TALENT_TREE_SHAMAN_ENHANCEMENT = 263,
    TALENT_TREE_SHAMAN_RESTORATION = 262,
    TALENT_TREE_MAGE_ARCANE = 81,
    TALENT_TREE_MAGE_FIRE = 41,
    TALENT_TREE_MAGE_FROST = 61,
    TALENT_TREE_WARLOCK_AFFLICTION = 302,
    TALENT_TREE_WARLOCK_DEMONOLOGY = 303,
    TALENT_TREE_WARLOCK_DESTRUCTION = 301,
    TALENT_TREE_DRUID_BALANCE = 283,
    TALENT_TREE_DRUID_FERAL_COMBAT = 281,
    TALENT_TREE_DRUID_RESTORATION = 282
}

public enum TrainerSpellState
{
    TRAINER_SPELL_GREEN = 0,
    TRAINER_SPELL_RED = 1,
    TRAINER_SPELL_GRAY = 2,
    TRAINER_SPELL_GREEN_DISABLED = 10                       // custom value, not send to client: formally green but learn not allowed
}

public enum ActionButtonUpdateState
{
    ACTIONBUTTON_UNCHANGED = 0,
    ACTIONBUTTON_CHANGED = 1,
    ACTIONBUTTON_NEW = 2,
    ACTIONBUTTON_DELETED = 3
}

public enum ActionButtonType
{
    ACTION_BUTTON_SPELL = 0x00,
    ACTION_BUTTON_C = 0x01,                         // click?
    ACTION_BUTTON_EQSET = 0x20,
    ACTION_BUTTON_MACRO = 0x40,
    ACTION_BUTTON_CMACRO = ACTION_BUTTON_C | ACTION_BUTTON_MACRO,
    ACTION_BUTTON_ITEM = 0x80
};

public enum ReputationSource
{
    REPUTATION_SOURCE_KILL,
    REPUTATION_SOURCE_QUEST,
    REPUTATION_SOURCE_DAILY_QUEST,
    REPUTATION_SOURCE_WEEKLY_QUEST,
    REPUTATION_SOURCE_MONTHLY_QUEST,
    REPUTATION_SOURCE_REPEATABLE_QUEST,
    REPUTATION_SOURCE_SPELL
}

public enum DuelState
{
    DUEL_STATE_CHALLENGED,
    DUEL_STATE_COUNTDOWN,
    DUEL_STATE_IN_PROGRESS,
    DUEL_STATE_COMPLETED
}

public enum RuneCooldowns
{
    RUNE_BASE_COOLDOWN = 10000,
    RUNE_GRACE_PERIOD = 2500,     // xinef: maximum possible grace period
    RUNE_MISS_COOLDOWN = 1500,     // cooldown applied on runes when the spell misses
}

public enum RuneType
{
    RUNE_BLOOD = 0,
    RUNE_UNHOLY = 1,
    RUNE_FROST = 2,
    RUNE_DEATH = 3,
    NUM_RUNE_TYPES = 4
}

public enum PlayerMovementType
{
    MOVE_ROOT = 1,
    MOVE_UNROOT = 2,
    MOVE_WATER_WALK = 3,
    MOVE_LAND_WALK = 4
}

public enum DrunkenState
{
    DRUNKEN_SOBER = 0,
    DRUNKEN_TIPSY = 1,
    DRUNKEN_DRUNK = 2,
    DRUNKEN_SMASHED = 3
}

public enum PlayerFlags : uint
{
    PLAYER_FLAGS_GROUP_LEADER = 0x00000001,
    PLAYER_FLAGS_AFK = 0x00000002,
    PLAYER_FLAGS_DND = 0x00000004,
    PLAYER_FLAGS_GM = 0x00000008,
    PLAYER_FLAGS_GHOST = 0x00000010,
    PLAYER_FLAGS_RESTING = 0x00000020,
    PLAYER_FLAGS_UNK6 = 0x00000040,
    PLAYER_FLAGS_UNK7 = 0x00000080,               // pre-3.0.3 PLAYER_FLAGS_FFA_PVP flag for FFA PVP state
    PLAYER_FLAGS_CONTESTED_PVP = 0x00000100,               // Player has been involved in a PvP combat and will be attacked by contested guards
    PLAYER_FLAGS_IN_PVP = 0x00000200,
    PLAYER_FLAGS_HIDE_HELM = 0x00000400,
    PLAYER_FLAGS_HIDE_CLOAK = 0x00000800,
    PLAYER_FLAGS_PLAYED_LONG_TIME = 0x00001000,               // played long time
    PLAYER_FLAGS_PLAYED_TOO_LONG = 0x00002000,               // played too long time
    PLAYER_FLAGS_IS_OUT_OF_BOUNDS = 0x00004000,
    PLAYER_FLAGS_DEVELOPER = 0x00008000,               // <Dev> prefix for something?
    PLAYER_FLAGS_UNK16 = 0x00010000,               // pre-3.0.3 PLAYER_FLAGS_SANCTUARY flag for player entered sanctuary
    PLAYER_FLAGS_TAXI_BENCHMARK = 0x00020000,               // taxi benchmark mode (on/off) (2.0.1)
    PLAYER_FLAGS_PVP_TIMER = 0x00040000,               // 3.0.2, pvp timer active (after you disable pvp manually)
    PLAYER_FLAGS_UBER = 0x00080000,
    PLAYER_FLAGS_UNK20 = 0x00100000,
    PLAYER_FLAGS_UNK21 = 0x00200000,
    PLAYER_FLAGS_COMMENTATOR2 = 0x00400000,
    PLAYER_ALLOW_ONLY_ABILITY = 0x00800000,                // used by bladestorm and killing spree, allowed only spells with SPELL_ATTR0_USES_RANGED_SLOT, SPELL_EFFECT_ATTACK, checked only for active player
    PLAYER_FLAGS_UNK24 = 0x01000000,                // disabled all melee ability on tab include autoattack
    PLAYER_FLAGS_NO_XP_GAIN = 0x02000000,
    PLAYER_FLAGS_UNK26 = 0x04000000,
    PLAYER_FLAGS_UNK27 = 0x08000000,
    PLAYER_FLAGS_UNK28 = 0x10000000,
    PLAYER_FLAGS_UNK29 = 0x20000000,
    PLAYER_FLAGS_UNK30 = 0x40000000,
    PLAYER_FLAGS_UNK31 = 0x80000000,
}

public enum PlayerBytesOffsets //@todo: Implement
{
    PLAYER_BYTES_OFFSET_SKIN_ID = 0,
    PLAYER_BYTES_OFFSET_FACE_ID = 1,
    PLAYER_BYTES_OFFSET_HAIR_STYLE_ID = 2,
    PLAYER_BYTES_OFFSET_HAIR_COLOR_ID = 3
}

public enum PlayerBytes2Offsets //@todo: Implement
{
    PLAYER_BYTES_2_OFFSET_FACIAL_STYLE = 0,
    PLAYER_BYTES_2_OFFSET_PARTY_TYPE = 1,
    PLAYER_BYTES_2_OFFSET_BANK_BAG_SLOTS = 2,
    PLAYER_BYTES_2_OFFSET_REST_STATE = 3
}

public enum PlayerBytes3Offsets //@todo: Implement
{
    PLAYER_BYTES_3_OFFSET_GENDER = 0,
    PLAYER_BYTES_3_OFFSET_INEBRIATION = 1,
    PLAYER_BYTES_3_OFFSET_PVP_TITLE = 2,
    PLAYER_BYTES_3_OFFSET_ARENA_FACTION = 3
}

public enum PlayerFieldBytesOffsets //@todo: Implement
{
    PLAYER_FIELD_BYTES_OFFSET_FLAGS = 0,
    PLAYER_FIELD_BYTES_OFFSET_RAF_GRANTABLE_LEVEL = 1,
    PLAYER_FIELD_BYTES_OFFSET_ACTION_BAR_TOGGLES = 2,
    PLAYER_FIELD_BYTES_OFFSET_LIFETIME_MAX_PVP_RANK = 3
}

public enum PlayerFieldBytes2Offsets
{
    PLAYER_FIELD_BYTES_2_OFFSET_OVERRIDE_SPELLS_ID = 0,    // uint16!
    PLAYER_FIELD_BYTES_2_OFFSET_IGNORE_POWER_REGEN_PREDICTION_MASK = 2,
    PLAYER_FIELD_BYTES_2_OFFSET_AURA_VISION = 3
}

// used in PLAYER_FIELD_BYTES values
public enum PlayerFieldByteFlags
{
    PLAYER_FIELD_BYTE_TRACK_STEALTHED = 0x00000002,
    PLAYER_FIELD_BYTE_RELEASE_TIMER = 0x00000008,       // Display time till auto release spirit
    PLAYER_FIELD_BYTE_NO_RELEASE_WINDOW = 0x00000010        // Display no "release spirit" window at all
}

// used in PLAYER_FIELD_BYTES2 values
public enum PlayerFieldByte2Flags
{
    PLAYER_FIELD_BYTE2_NONE = 0x00,
    PLAYER_FIELD_BYTE2_STEALTH = 0x20,
    PLAYER_FIELD_BYTE2_INVISIBILITY_GLOW = 0x40
}

public enum MirrorTimerType
{
    FATIGUE_TIMER = 0,
    BREATH_TIMER = 1,
    FIRE_TIMER = 2
}

// 2^n values
[Flags]
public enum PlayerExtraFlags
{
    // gm abilities
    PLAYER_EXTRA_GM_ON = 0x0001,
    PLAYER_EXTRA_ACCEPT_WHISPERS = 0x0004,
    PLAYER_EXTRA_TAXICHEAT = 0x0008,
    PLAYER_EXTRA_GM_INVISIBLE = 0x0010,
    PLAYER_EXTRA_GM_CHAT = 0x0020,               // Show GM badge in chat messages
    PLAYER_EXTRA_HAS_310_FLYER = 0x0040,               // Marks if player already has 310% speed flying mount
    PLAYER_EXTRA_SPECTATOR_ON = 0x0080,               // Marks if player is spectactor
    PLAYER_EXTRA_PVP_DEATH = 0x0100,               // store PvP death status until corpse creating.
    PLAYER_EXTRA_SHOW_DK_PET = 0x0400,               // Marks if player should see ghoul on login screen
}

// 2^n values
[Flags]
public enum AtLoginFlags : ushort
{
    AT_LOGIN_NONE = 0x00,
    AT_LOGIN_RENAME = 0x01,
    AT_LOGIN_RESET_SPELLS = 0x02,
    AT_LOGIN_RESET_TALENTS = 0x04,
    AT_LOGIN_CUSTOMIZE = 0x08,
    AT_LOGIN_RESET_PET_TALENTS = 0x10,
    AT_LOGIN_FIRST = 0x20,
    AT_LOGIN_CHANGE_FACTION = 0x40,
    AT_LOGIN_CHANGE_RACE = 0x80,
    AT_LOGIN_RESET_AP = 0x100,
    AT_LOGIN_RESET_ARENA = 0x200,
    AT_LOGIN_CHECK_ACHIEVS = 0x400,
    AT_LOGIN_RESURRECT = 0x800
}

public enum QuestSlotOffsets
{
    QUEST_ID_OFFSET = 0,
    QUEST_STATE_OFFSET = 1,
    QUEST_COUNTS_OFFSET = 2,
    QUEST_TIME_OFFSET = 4
};

public enum QuestSlotStateMask
{
    QUEST_STATE_NONE = 0x0000,
    QUEST_STATE_COMPLETE = 0x0001,
    QUEST_STATE_FAIL = 0x0002
}

public enum SkillUpdateState
{
    SKILL_UNCHANGED = 0,
    SKILL_CHANGED = 1,
    SKILL_NEW = 2,
    SKILL_DELETED = 3
}

enum EquipmentSlots                                         // 19 slots
{
    EQUIPMENT_SLOT_START = 0,
    EQUIPMENT_SLOT_HEAD = 0,
    EQUIPMENT_SLOT_NECK = 1,
    EQUIPMENT_SLOT_SHOULDERS = 2,
    EQUIPMENT_SLOT_BODY = 3,
    EQUIPMENT_SLOT_CHEST = 4,
    EQUIPMENT_SLOT_WAIST = 5,
    EQUIPMENT_SLOT_LEGS = 6,
    EQUIPMENT_SLOT_FEET = 7,
    EQUIPMENT_SLOT_WRISTS = 8,
    EQUIPMENT_SLOT_HANDS = 9,
    EQUIPMENT_SLOT_FINGER1 = 10,
    EQUIPMENT_SLOT_FINGER2 = 11,
    EQUIPMENT_SLOT_TRINKET1 = 12,
    EQUIPMENT_SLOT_TRINKET2 = 13,
    EQUIPMENT_SLOT_BACK = 14,
    EQUIPMENT_SLOT_MAINHAND = 15,
    EQUIPMENT_SLOT_OFFHAND = 16,
    EQUIPMENT_SLOT_RANGED = 17,
    EQUIPMENT_SLOT_TABARD = 18,
    EQUIPMENT_SLOT_END = 19
};

public enum InventorySlots                                         // 4 slots
{
    INVENTORY_SLOT_BAG_START = 19,
    INVENTORY_SLOT_BAG_END = 23
}

public enum InventoryPackSlots                                     // 16 slots
{
    INVENTORY_SLOT_ITEM_START = 23,
    INVENTORY_SLOT_ITEM_END = 39
}

public enum BankItemSlots                                          // 28 slots
{
    BANK_SLOT_ITEM_START = 39,
    BANK_SLOT_ITEM_END = 67
}

public enum BankBagSlots                                           // 7 slots
{
    BANK_SLOT_BAG_START = 67,
    BANK_SLOT_BAG_END = 74
}

public enum BuyBackSlots                                           // 12 slots
{
    // stored in m_buybackitems
    BUYBACK_SLOT_START = 74,
    BUYBACK_SLOT_END = 86
}

public enum KeyRingSlots                                           // 32 slots
{
    KEYRING_SLOT_START = 86,
    KEYRING_SLOT_END = 118
}

public enum CurrencyTokenSlots                                     // 32 slots
{
    CURRENCYTOKEN_SLOT_START = 118,
    CURRENCYTOKEN_SLOT_END = 150
}

public enum EquipmentSetUpdateState
{
    EQUIPMENT_SET_UNCHANGED = 0,
    EQUIPMENT_SET_CHANGED = 1,
    EQUIPMENT_SET_NEW = 2,
    EQUIPMENT_SET_DELETED = 3
}

public enum TransferAbortReason
{
    TRANSFER_ABORT_NONE = 0x00,
    TRANSFER_ABORT_ERROR = 0x01,
    TRANSFER_ABORT_MAX_PLAYERS = 0x02,         // Transfer Aborted: instance is full
    TRANSFER_ABORT_NOT_FOUND = 0x03,         // Transfer Aborted: instance not found
    TRANSFER_ABORT_TOO_MANY_INSTANCES = 0x04,         // You have entered too many instances recently.
    TRANSFER_ABORT_ZONE_IN_COMBAT = 0x06,         // Unable to zone in while an encounter is in progress.
    TRANSFER_ABORT_INSUF_EXPAN_LVL = 0x07,         // You must have <TBC, WotLK> expansion installed to access this area.
    TRANSFER_ABORT_DIFFICULTY = 0x08,         // <Normal, Heroic, Epic> difficulty mode is not available for %s.
    TRANSFER_ABORT_UNIQUE_MESSAGE = 0x09,         // Until you've escaped TLK's grasp, you cannot leave this place!
    TRANSFER_ABORT_TOO_MANY_REALM_INSTANCES = 0x0A,         // Additional instances cannot be launched, please try again later.
    TRANSFER_ABORT_NEED_GROUP = 0x0B,         // 3.1
    TRANSFER_ABORT_NOT_FOUND1 = 0x0C,         // 3.1
    TRANSFER_ABORT_NOT_FOUND2 = 0x0D,         // 3.1
    TRANSFER_ABORT_NOT_FOUND3 = 0x0E,         // 3.2
    TRANSFER_ABORT_REALM_ONLY = 0x0F,         // All players on party must be from the same realm.
    TRANSFER_ABORT_MAP_NOT_ALLOWED = 0x10,         // Map can't be entered at this time.
}

public enum InstanceResetWarningType
{
    RAID_INSTANCE_WARNING_HOURS = 1,                    // WARNING! %s is scheduled to reset in %d hour(s).
    RAID_INSTANCE_WARNING_MIN = 2,                    // WARNING! %s is scheduled to reset in %d minute(s)!
    RAID_INSTANCE_WARNING_MIN_SOON = 3,                    // WARNING! %s is scheduled to reset in %d minute(s). Please exit the zone or you will be returned to your bind location!
    RAID_INSTANCE_WELCOME = 4,                    // Welcome to %s. This raid instance is scheduled to reset in %s.
    RAID_INSTANCE_EXPIRED = 5
}

public enum RestFlag
{
    REST_FLAG_IN_TAVERN = 0x1,
    REST_FLAG_IN_CITY = 0x2,
    REST_FLAG_IN_FACTION_AREA = 0x4, // used with AREA_FLAG_REST_ZONE_*
}

public enum TeleportToOptions
{
    TELE_TO_GM_MODE = 0x01,
    TELE_TO_NOT_LEAVE_TRANSPORT = 0x02,
    TELE_TO_NOT_LEAVE_COMBAT = 0x04,
    TELE_TO_NOT_UNSUMMON_PET = 0x08,
    TELE_TO_SPELL = 0x10,
    TELE_TO_NOT_LEAVE_VEHICLE = 0x20,
    TELE_TO_WITH_PET = 0x40,
    TELE_TO_NOT_LEAVE_TAXI = 0x80
}

/// Type of environmental damages
public enum EnviromentalDamage
{
    DAMAGE_EXHAUSTED = 0,
    DAMAGE_DROWNING = 1,
    DAMAGE_FALL = 2,
    DAMAGE_LAVA = 3,
    DAMAGE_SLIME = 4,
    DAMAGE_FIRE = 5,
    DAMAGE_FALL_TO_VOID = 6                                 // custom case for fall without durability loss
}

public enum PlayerChatTag
{
    CHAT_TAG_NONE = 0x00,
    CHAT_TAG_AFK = 0x01,
    CHAT_TAG_DND = 0x02,
    CHAT_TAG_GM = 0x04,
    CHAT_TAG_COM = 0x08, // Commentator
    CHAT_TAG_DEV = 0x10,
}

public enum PlayedTimeIndex
{
    PLAYED_TIME_TOTAL = 0,
    PLAYED_TIME_LEVEL = 1
}

// used at player loading query list preparing, and later result selection
public enum PlayerLoginQueryIndex
{
    PLAYER_LOGIN_QUERY_LOAD_FROM = 0,
    PLAYER_LOGIN_QUERY_LOAD_AURAS = 3,
    PLAYER_LOGIN_QUERY_LOAD_SPELLS = 4,
    PLAYER_LOGIN_QUERY_LOAD_QUEST_STATUS = 5,
    PLAYER_LOGIN_QUERY_LOAD_DAILY_QUEST_STATUS = 6,
    PLAYER_LOGIN_QUERY_LOAD_REPUTATION = 7,
    PLAYER_LOGIN_QUERY_LOAD_INVENTORY = 8,
    PLAYER_LOGIN_QUERY_LOAD_ACTIONS = 9,
    PLAYER_LOGIN_QUERY_LOAD_MAILS = 10,
    PLAYER_LOGIN_QUERY_LOAD_MAIL_ITEMS = 11,
    PLAYER_LOGIN_QUERY_LOAD_SOCIAL_LIST = 13,
    PLAYER_LOGIN_QUERY_LOAD_HOME_BIND = 14,
    PLAYER_LOGIN_QUERY_LOAD_SPELL_COOLDOWNS = 15,
    PLAYER_LOGIN_QUERY_LOAD_DECLINED_NAMES = 16,
    PLAYER_LOGIN_QUERY_LOAD_ACHIEVEMENTS = 18,
    PLAYER_LOGIN_QUERY_LOAD_CRITERIA_PROGRESS = 19,
    PLAYER_LOGIN_QUERY_LOAD_EQUIPMENT_SETS = 20,
    PLAYER_LOGIN_QUERY_LOAD_ENTRY_POINT = 21,
    PLAYER_LOGIN_QUERY_LOAD_GLYPHS = 22,
    PLAYER_LOGIN_QUERY_LOAD_TALENTS = 23,
    PLAYER_LOGIN_QUERY_LOAD_ACCOUNT_DATA = 24,
    PLAYER_LOGIN_QUERY_LOAD_SKILLS = 25,
    PLAYER_LOGIN_QUERY_LOAD_WEEKLY_QUEST_STATUS = 26,
    PLAYER_LOGIN_QUERY_LOAD_RANDOM_BG = 27,
    PLAYER_LOGIN_QUERY_LOAD_BANNED = 28,
    PLAYER_LOGIN_QUERY_LOAD_QUEST_STATUS_REW = 29,
    PLAYER_LOGIN_QUERY_LOAD_INSTANCE_LOCK_TIMES = 30,
    PLAYER_LOGIN_QUERY_LOAD_SEASONAL_QUEST_STATUS = 31,
    PLAYER_LOGIN_QUERY_LOAD_MONTHLY_QUEST_STATUS = 32,
    PLAYER_LOGIN_QUERY_LOAD_BREW_OF_THE_MONTH = 34,
    PLAYER_LOGIN_QUERY_LOAD_CORPSE_LOCATION = 35,
    PLAYER_LOGIN_QUERY_LOAD_CHARACTER_SETTINGS = 36,
    PLAYER_LOGIN_QUERY_LOAD_PET_SLOTS = 37,
    MAX_PLAYER_LOGIN_QUERY
}

public enum PlayerDelayedOperations
{
    DELAYED_SAVE_PLAYER = 0x01,
    DELAYED_RESURRECT_PLAYER = 0x02,
    DELAYED_SPELL_CAST_DESERTER = 0x04,
    DELAYED_BG_MOUNT_RESTORE = 0x08,                     ///< Flag to restore mount state after teleport from BG
    DELAYED_BG_TAXI_RESTORE = 0x10,                     ///< Flag to restore taxi state after teleport from BG
    DELAYED_BG_GROUP_RESTORE = 0x20,                     ///< Flag to restore group state after teleport from BG
    DELAYED_VEHICLE_TELEPORT = 0x40,
    DELAYED_END
}

public enum PlayerCharmedAISpells
{
    SPELL_T_STUN,
    SPELL_ROOT_OR_FEAR,
    SPELL_INSTANT_DAMAGE,
    SPELL_INSTANT_DAMAGE2,
    SPELL_HIGH_DAMAGE1,
    SPELL_HIGH_DAMAGE2,
    SPELL_DOT_DAMAGE,
    SPELL_T_CHARGE,
    SPELL_IMMUNITY,
    SPELL_FAST_RUN,
    NUM_CAI_SPELLS
}

public enum CharDeleteMethod
{
    CHAR_DELETE_REMOVE = 0,                      // Completely remove from the database
    CHAR_DELETE_UNLINK = 1                       // The character gets unlinked from the account,
                                                 // the name gets freed up and appears as deleted ingame
}

public enum CurrencyItems
{
    ITEM_HONOR_POINTS_ID = 43308,
    ITEM_ARENA_POINTS_ID = 43307
}

public enum ReferAFriendError
{
    ERR_REFER_A_FRIEND_NONE = 0x00,
    ERR_REFER_A_FRIEND_NOT_REFERRED_BY = 0x01,
    ERR_REFER_A_FRIEND_TARGET_TOO_HIGH = 0x02,
    ERR_REFER_A_FRIEND_INSUFFICIENT_GRANTABLE_LEVELS = 0x03,
    ERR_REFER_A_FRIEND_TOO_FAR = 0x04,
    ERR_REFER_A_FRIEND_DIFFERENT_FACTION = 0x05,
    ERR_REFER_A_FRIEND_NOT_NOW = 0x06,
    ERR_REFER_A_FRIEND_GRANT_LEVEL_MAX_I = 0x07,
    ERR_REFER_A_FRIEND_NO_TARGET = 0x08,
    ERR_REFER_A_FRIEND_NOT_IN_GROUP = 0x09,
    ERR_REFER_A_FRIEND_SUMMON_LEVEL_MAX_I = 0x0A,
    ERR_REFER_A_FRIEND_SUMMON_COOLDOWN = 0x0B,
    ERR_REFER_A_FRIEND_INSUF_EXPAN_LVL = 0x0C,
    ERR_REFER_A_FRIEND_SUMMON_OFFLINE_S = 0x0D
}

public enum PlayerRestState
{
    REST_STATE_RESTED = 0x01,
    REST_STATE_NOT_RAF_LINKED = 0x02,
    REST_STATE_RAF_LINKED = 0x06
}

public enum AdditionalSaving
{
    ADDITIONAL_SAVING_NONE = 0x00,
    ADDITIONAL_SAVING_INVENTORY_AND_GOLD = 0x01,
    ADDITIONAL_SAVING_QUEST_STATUS = 0x02,
}

public enum PlayerCommandStates
{
    CHEAT_NONE = 0x00,
    CHEAT_GOD = 0x01,
    CHEAT_CASTTIME = 0x02,
    CHEAT_COOLDOWN = 0x04,
    CHEAT_POWER = 0x08,
    CHEAT_WATERWALK = 0x10
}

// Used for OnGiveXP PlayerScript hook
public enum PlayerXPSource
{
    XPSOURCE_KILL = 0,
    XPSOURCE_QUEST = 1,
    XPSOURCE_QUEST_DF = 2,
    XPSOURCE_EXPLORE = 3,
    XPSOURCE_BATTLEGROUND = 4
}

public enum InstantFlightGossipAction
{
    GOSSIP_ACTION_TOGGLE_INSTANT_FLIGHT = 500
}

public enum EmoteBroadcastTextID
{
    EMOTE_BROADCAST_TEXT_ID_STRANGE_GESTURES = 91243
}

public struct PlayerCreateInfoItems
{
    public uint ItemId;
    public uint ItemAmount;

    public PlayerCreateInfoItems() { }

    public PlayerCreateInfoItems(uint id, uint amount)
    {
        ItemId = id;
        ItemAmount = amount;
    }
}

public struct PlayerCreateInfoSkills
{

}

public struct PlayerCreateInfoActions
{

}

public struct PlayerCreateInfoSpells
{

}

public struct PlayerLevelInfo
{
    public uint[] Stats = new uint[SharedConst.MAX_STATS];

    public PlayerLevelInfo() { }
}

public struct PlayerInfo
{
    public uint MapId;
    public uint AreaId;
    public float PositionX;
    public float PositionY;
    public float PositionZ;
    public float Orientation;
    public ushort DisplayId_M;
    public ushort DisplayId_F;
    public PlayerCreateInfoItems Item;
    public PlayerCreateInfoSpells CustomSpells;
    public PlayerCreateInfoSpells CastSpells;
    public PlayerCreateInfoActions Action;
    public PlayerCreateInfoSkills Skills;
    public PlayerLevelInfo LevelInfo;                             // [level-1] 0..MaxPlayerLevel - 1

    public PlayerInfo() { }
}

public static class PlayerConst
{
    // Player summoning auto-decline time (in secs)
    public static readonly int MAX_PLAYER_SUMMON_DELAY      = 2 * SharedConst.MINUTE;
    public static readonly int MAX_MONEY_AMOUNT             = 0x7FFFFFFF - 1;

    public static readonly int PLAYER_MAX_SKILLS            = 127;
    public static readonly int PLAYER_MAX_DAILY_QUESTS      = 25;
    public static readonly int PLAYER_EXPLORED_ZONES_SIZE   = 128;

    public static readonly int KNOWN_TITLES_SIZE            = 3;
    public static readonly int MAX_TITLE_INDEX              = KNOWN_TITLES_SIZE * 64;
}

public partial class Player : Unit
{
    private long _semaphoreTeleportNear;
    private long _semaphoreTeleportFar;
    private Unit _mover;
    private WorldSession _worldSession;

    public Unit Mover
    {
        get
        {
            return _mover;
        }
    }

    public Player(WorldSession worldSession)
    {
        _semaphoreTeleportNear = 0;
        _semaphoreTeleportFar = 0;
        _worldSession = worldSession;
        _mover = this;
    }

    public static bool BuildEnumData(QueryResult result, ref WorldPacketData data)
    {
        //             0               1                2                3                 4                  5                 6               7
        //    "SELECT characters.guid, characters.name, characters.race, characters.class, characters.gender, characters.skin, characters.face, characters.hairStyle,
        //     8                     9                       10              11               12              13                     14                     15
        //    characters.hairColor, characters.facialStyle, character.level, characters.zone, characters.map, characters.position_x, characters.position_y, characters.position_z,
        //    16                    17                      18                   19                   20                     21                   22               23
        //    guild_member.guildid, characters.playerFlags, characters.at_login, character_pet.entry, character_pet.modelid, character_pet.level, characters.equipmentCache, character_banned.guid,
        //    24                      25
        //    characters.extra_flags, character_declinedname.genitive

        Fields fields = result.Fetch();

        uint guidLow = fields[0].Get<uint>();
        string? plrName = fields[1].Get<string>();
        byte plrRace = fields[2].Get<byte>();
        byte plrClass = fields[3].Get<byte>();
        byte gender = fields[4].Get<byte>();

        ObjectGuid guid = ObjectGuid.Create(HighGuid.Player, guidLow);

        PlayerInfo? info = Global.sObjectMgr.GetPlayerInfo(plrRace, plrClass);

        if (info == null)
        {
            logger.Error(LogFilter.Player, $"Player {guid} has incorrect race/class pair. Don't build enum.");
            return false;
        }
        else if (!IsValidGender(gender))
        {
            logger.Error(LogFilter.PlayerLoading, $"Player ({guid}) has incorrect gender ({gender}), don't build enum.");
            return false;
        }

        data.WriteObjectGuid(guid);
        data.WriteCString(plrName ?? string.Empty);              // name
        data.WriteByte(plrRace);                                 // race
        data.WriteByte(plrClass);                                // class
        data.WriteByte(gender);                                  // gender

        byte skin = fields[5].Get<byte>();
        byte face = fields[6].Get<byte>();
        byte hairStyle = fields[7].Get<byte>();
        byte hairColor = fields[8].Get<byte>();
        byte facialStyle = fields[9].Get<byte>();

        uint charFlags = 0;
        uint playerFlags = fields[17].Get<uint>();
        ushort atLoginFlags = fields[18].Get<ushort>();
        uint zone = (atLoginFlags & (ushort)AtLoginFlags.AT_LOGIN_FIRST) != 0 ? (ushort)0 : fields[11].Get<ushort>(); // if first login do not show the zone

        data.WriteByte(skin);
        data.WriteByte(face);
        data.WriteByte(hairStyle);
        data.WriteByte(hairColor);
        data.WriteByte(facialStyle);

        data.WriteByte(fields[10].Get<byte>());                      // level
        data.WriteUInt(zone);                                       // zone
        data.WriteUInt(fields[12].Get<ushort>());                    // map

        data.WriteFloat(fields[13].Get<float>());                    // x
        data.WriteFloat(fields[14].Get<float>());                    // y
        data.WriteFloat(fields[15].Get<float>());                    // z

        data.WriteUInt(fields[16].Get<uint>());                      // guild id

        if ((atLoginFlags & (ushort)AtLoginFlags.AT_LOGIN_RESURRECT) != 0)
        {
            playerFlags &= ~(uint)PlayerFlags.PLAYER_FLAGS_GHOST;
        }

        if ((playerFlags & (uint)PlayerFlags.PLAYER_FLAGS_HIDE_HELM) != 0)
        {
            charFlags |= (uint)CharacterFlags.CHARACTER_FLAG_HIDE_HELM;
        }

        if ((playerFlags & (uint)PlayerFlags.PLAYER_FLAGS_HIDE_CLOAK) != 0)
        {
            charFlags |= (uint)CharacterFlags.CHARACTER_FLAG_HIDE_CLOAK;
        }

        if ((playerFlags & (uint)PlayerFlags.PLAYER_FLAGS_GHOST) != 0)
        {
            charFlags |= (uint)CharacterFlags.CHARACTER_FLAG_GHOST;
        }

        if ((atLoginFlags & (ushort)AtLoginFlags.AT_LOGIN_RENAME) != 0)
        {
            charFlags |= (uint)CharacterFlags.CHARACTER_FLAG_RENAME;
        }

        if (fields[23].Get<uint>() != 0)
        {
            charFlags |= (uint)CharacterFlags.CHARACTER_FLAG_LOCKED_BY_BILLING;
        }

        if (ConfigMgr.GetOption("RealmZone", RealmZone.REALM_ZONE_DEVELOPMENT) == RealmZone.REALM_ZONE_RUSSIAN ||
            ConfigMgr.GetOption("DeclinedNames", false))
        {
            if (!fields[25].Get<string>().IsEmpty())
            {
                charFlags |= (uint)CharacterFlags.CHARACTER_FLAG_DECLINED;
            }
        }
        else
        {
            charFlags |= (uint)CharacterFlags.CHARACTER_FLAG_DECLINED;
        }

        data.WriteUInt(charFlags);                              // character flags

        // character customize flags
        if ((atLoginFlags & (ushort)AtLoginFlags.AT_LOGIN_CUSTOMIZE) != 0)
        {
            data.WriteUInt((uint)CharacterCustomizeFlags.CHAR_CUSTOMIZE_FLAG_CUSTOMIZE);
        }
        else if ((atLoginFlags & (ushort)AtLoginFlags.AT_LOGIN_CHANGE_FACTION) != 0)
        {
            data.WriteUInt((uint)CharacterCustomizeFlags.CHAR_CUSTOMIZE_FLAG_FACTION);
        }
        else if ((atLoginFlags & (ushort)AtLoginFlags.AT_LOGIN_CHANGE_RACE) != 0)
        {
            data.WriteUInt((uint)CharacterCustomizeFlags.CHAR_CUSTOMIZE_FLAG_RACE);
        }
        else
        {
            data.WriteUInt((uint)CharacterCustomizeFlags.CHAR_CUSTOMIZE_FLAG_NONE);
        }

        // First login
        data.WriteByte((atLoginFlags & (ushort)AtLoginFlags.AT_LOGIN_FIRST) != 0 ? (byte)0x01 : (byte)0x00);

        // Pets info
        uint petDisplayId = 0;
        uint petLevel = 0;
        uint petFamily = 0;

        // show pet at selection character in character list only for non-ghost character
        if (!result.IsEmpty() &&
            (playerFlags & (uint)PlayerFlags.PLAYER_FLAGS_GHOST) == 0 &&
            (plrClass == (byte)Classes.CLASS_WARLOCK ||
            plrClass == (byte)Classes.CLASS_HUNTER ||
            (plrClass == (byte)Classes.CLASS_DEATH_KNIGHT && ((fields[21].Get<uint>() & (uint)PlayerExtraFlags.PLAYER_EXTRA_SHOW_DK_PET) != 0))))
        {
            uint entry = fields[19].Get<uint>();
            CreatureTemplate? creatureInfo = Global.sObjectMgr.GetCreatureTemplate(entry);

            if (creatureInfo.HasValue)
            {
                petDisplayId = fields[20].Get<uint>();
                petLevel = fields[21].Get<ushort>();
                petFamily = creatureInfo.Value.Family;
            }
        }

        data.WriteUInt(petDisplayId);
        data.WriteUInt(petLevel);
        data.WriteUInt(petFamily);

        string[] equipment = (fields[22].Get<string>() ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);

        for (byte slot = 0; slot < (byte)InventorySlots.INVENTORY_SLOT_BAG_END; ++slot)
        {
            uint visualBase = (uint)slot * 2;
            uint? itemId = null;

            if (visualBase < equipment.Length)
            {
                itemId = Convert.ToUInt32(equipment[visualBase]);
            }

            ItemTemplate? proto = null;

            if (itemId != null)
            {
                proto = Global.sObjectMgr.GetItemTemplate(itemId.Value);
            }

            if (proto == null)
            {
                if (itemId == null || itemId == 0)
                {
                    string equipmentName = (visualBase < equipment.Length) ? equipment[visualBase] : " < none > ";
                    logger.Warn(LogFilter.PlayerLoading, $"Player {guid} has invalid equipment '{equipmentName}' in `equipmentcache` at index {visualBase}. Skipped.");
                }

                data.WriteUInt(0);
                data.WriteByte(0);
                data.WriteUInt(0);

                continue;
            }

            SpellItemEnchantmentEntry? enchant = null;

            uint? enchants = null;

            if ((visualBase + 1) < equipment.Length)
            {
                enchants = Convert.ToUInt32(equipment[visualBase + 1]);
            }

            if (enchants == null)
            {
                string equipmentName = ((visualBase + 1) < equipment.Length) ? equipment[visualBase + 1] : "<none>";

                logger.Warn(LogFilter.PlayerLoading,
                            $"Player {guid} has invalid enchantment info '{equipmentName}' in `equipmentcache` at index {visualBase + 1}. Skipped.");

                enchants = 0;
            }

            for (byte enchantSlot = (byte)EnchantmentSlot.PERM_ENCHANTMENT_SLOT;
                 enchantSlot <= (byte)EnchantmentSlot.TEMP_ENCHANTMENT_SLOT;
                 ++enchantSlot)
            {
                // values stored in 2 uint16
                uint enchantId = 0x0000FFFF & ((enchants.Value) >> enchantSlot * 16);

                if (enchantId == 0)
                {
                    continue;
                }

                enchant = Global.sSpellItemEnchantmentStore.LookupEntry(enchantId);

                if (enchant != null)
                {
                    break;
                }
            }

            data.WriteUInt(proto.Value.DisplayInfoID);
            data.WriteByte((byte)proto.Value.InventoryType);
            data.WriteUInt(enchant != null ? enchant.AuraId : 0);
        }

        return true;
    }

    public static bool IsValidGender(ushort gender)
    {
        return gender <= (ushort)Gender.GENDER_FEMALE;
    }

    public bool IsInWorld()
    {
        return _inWorld;
    }

    public bool IsBeingTeleported()
    {
        return _semaphoreTeleportNear != 0 || _semaphoreTeleportFar != 0;
    }

    public bool IsBeingTeleportedNear()
    {
        return _semaphoreTeleportNear != 0;
    }

    public bool IsBeingTeleportedFar()
    {
        return _semaphoreTeleportFar != 0;
    }

    public WorldSession GetSession()
    {
        return _worldSession;
    }

    override public void SetObjectScale(float scale)
    {
        base.SetObjectScale(scale);

        SetFloatValue((ushort)EUnitFields.UNIT_FIELD_BOUNDINGRADIUS, scale * ObjectDefines.DEFAULT_WORLD_OBJECT_SIZE);
        SetFloatValue((ushort)EUnitFields.UNIT_FIELD_COMBATREACH, scale * ObjectDefines.DEFAULT_COMBAT_REACH);
    }

    public void SetMoney(uint value)
    {
        SetUInt32Value((ushort)EUnitFields.PLAYER_FIELD_COINAGE, value);
        MoneyChanged(value);
        UpdateAchievementCriteria(AchievementCriteriaTypes.ACHIEVEMENT_CRITERIA_TYPE_HIGHEST_GOLD_VALUE_OWNED);
    }

    public PlayerFlags  GetPlayerFlags()
    {
        return (PlayerFlags)GetUInt32Value((ushort)EUnitFields.PLAYER_FLAGS);
    }

    public bool HasPlayerFlag(PlayerFlags flags)
    {
        return HasFlag((ushort)EUnitFields.PLAYER_FLAGS, (uint)flags);
    }

    public void SetPlayerFlag(PlayerFlags flags)
    {
        SetFlag((ushort)EUnitFields.PLAYER_FLAGS, (uint)flags);
    }

    public void RemovePlayerFlag(PlayerFlags flags)
    {
        RemoveFlag((ushort)EUnitFields.PLAYER_FLAGS, (uint)flags);
    }

    public void ReplaceAllPlayerFlags(PlayerFlags flags)
    {
        SetUInt32Value((ushort)EUnitFields.PLAYER_FLAGS, (uint)flags);
    }

    public void SetFlag(ushort index, uint newFlag)
    {
        if ((index + 1) < _valuesCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (_values == null)
        {
            throw new NullReferenceException(nameof(_values));
        }

        uint oldval = _values[index].UInt32Value;
        uint newval = oldval | newFlag;

        if (oldval != newval)
        {
            var objVal = _values[index];

            objVal.UInt32Value = newval;

            _changesMask.SetBit(index);

            AddToObjectUpdateIfNeeded();
        }
    }

    public void RemoveFlag(ushort index, uint oldFlag)
    {
        if ((index + 1) < _valuesCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (_values == null)
        {
            throw new NullReferenceException(nameof(_values));
        }

        uint oldval = _values[index].UInt32Value;
        uint newval = oldval & ~oldFlag;

        if (oldval != newval)
        {
            var objVal = _values[index];

            objVal.UInt32Value = newval;

            _changesMask.SetBit(index);

            AddToObjectUpdateIfNeeded();
        }
    }

    public void ToggleFlag(ushort index, uint flag)
    {
        if (HasFlag(index, flag))
        {
            RemoveFlag(index, flag);
        }
        else
        {
            SetFlag(index, flag);
        }
    }

    public bool HasFlag(ushort index, uint flag)
    {
        if (index >= _valuesCount)
        {
            return false;
        }

        if (_values == null)
        {
            return false;
        }

        return (_values[index].UInt32Value & flag) != 0;
    }

    public void InitDisplayIds()
    {
        PlayerInfo? info = Global.sObjectMgr.GetPlayerInfo(GetRace(true), GetClass());

        if (info == null)
        {
            logger.Error(LogFilter.Player, $"Player {GetGUID()} has incorrect race/class pair. Can't init display ids.");

            return;
        }

        byte gender = GetGender();

        switch (gender)
        {
        case (byte)Gender.GENDER_FEMALE:
            SetDisplayId(info.Value.DisplayId_F);
            SetNativeDisplayId(info.Value.DisplayId_F);
            break;
        case (byte)Gender.GENDER_MALE:
            SetDisplayId(info.Value.DisplayId_M);
            SetNativeDisplayId(info.Value.DisplayId_M);
            break;
        default:
            logger.Error(LogFilter.Player, $"Invalid gender {gender} for player");
            return;
        }
    }
}
