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

using System.Text;

using AzerothCore.Configuration;
using AzerothCore.Constants;
using AzerothCore.Database;
using AzerothCore.DataStores;
using AzerothCore.Logging;
using AzerothCore.Singleton;
using AzerothCore.Utilities;

namespace AzerothCore.Game;

internal struct RaceStats
{
    public short[] StatModifier = new short[SharedConst.MAX_STATS];

    public RaceStats()
    {
    }
}

public class ObjectMgr : Singleton<ObjectMgr>
{
    public const int MAX_PLAYER_NAME          = 12;                         // max allowed by client name length
    public const int MAX_INTERNAL_PLAYER_NAME = 15;                         // max server internal player name length (> MAX_PLAYER_NAME for support declined names)
    public const int MAX_PET_NAME             = 12 ;                        // max allowed by client name length
    public const int MAX_CHARTER_NAME         = 24;                         // max allowed by client name length
    public const int MAX_CHANNEL_NAME         = 50;

    private static readonly ILogger logger = LoggerFactory.GetLogger();

    private readonly PlayerInfo[,]                          _playerInfo;
    private readonly Dictionary<short, InstanceTemplate>    _instanceTemplateStore;
    private readonly List<string>                           _scriptNamesStore;
    private readonly Dictionary<uint, CreatureTemplate>     _creatureTemplateStore;
    private readonly Dictionary<uint, ItemTemplate>         _itemTemplateStore;

    private ObjectMgr()
	{
        _playerInfo = new PlayerInfo[SharedConst.MAX_RACES, SharedConst.MAX_CLASSES];
        _instanceTemplateStore = new Dictionary<short, InstanceTemplate>();
        _scriptNamesStore = new List<string>();
        _creatureTemplateStore = new Dictionary<uint, CreatureTemplate>();
        _itemTemplateStore = new Dictionary<uint, ItemTemplate>();
    }

    public void LoadPlayerInfo()
    {
        // Load playercreate
        logger.Info(LogFilter.ServerLoading, "Loading Player Create Info Data...");

        {
            uint oldMSTime = TimeHelper.GetMSTime();

            //                                                0     1      2    3        4          5           6
            QueryResult result = DB.World.Query("SELECT race, class, map, zone, position_x, position_y, position_z, orientation FROM playercreateinfo");

            if (result.IsEmpty())
            {
                logger.Info(LogFilter.ServerLoading, " ");
                logger.Warn(LogFilter.ServerLoading, ">> Loaded 0 player create definitions. DB table `playercreateinfo` is empty.");

                Environment.Exit(1);
            }
            else
            {
                uint count = 0;

                do
                {
                    Fields fields = result.Fetch();

                    uint currentRace = fields[0].Get<byte>();
                    uint currentClass = fields[1].Get<byte>();
                    uint mapId = fields[2].Get<ushort>();
                    uint areaId = fields[3].Get<uint>(); // zone
                    float positionX = fields[4].Get<float>();
                    float positionY = fields[5].Get<float>();
                    float positionZ = fields[6].Get<float>();
                    float orientation = fields[7].Get<float>();

                    if (currentRace >= SharedConst.MAX_RACES)
                    {
                        logger.Error(LogFilter.Sql, $"Wrong race {currentRace} in `playercreateinfo` table, ignoring.");
                        continue;
                    }

                    ChrRacesEntry? rEntry = Global.sChrRacesStore.LookupEntry(currentRace);

                    if (rEntry == null)
                    {
                        logger.Error(LogFilter.Sql, $"Wrong race {currentRace} in `playercreateinfo` table, ignoring.");
                        continue;
                    }

                    if (currentClass >= SharedConst.MAX_CLASSES)
                    {
                        logger.Error(LogFilter.Sql, $"Wrong class {currentClass} in `playercreateinfo` table, ignoring.");
                        continue;
                    }

                    if (Global.sChrClassesStore.LookupEntry(currentClass) == null)
                    {
                        logger.Error(LogFilter.Sql, $"Wrong class {currentClass} in `playercreateinfo` table, ignoring.");
                        continue;
                    }

                    // accept DB data only for valid position (and non instanceable)
                    if (!MapMgr.IsValidMapCoord(mapId, positionX, positionY, positionZ, orientation))
                    {
                        logger.Error(LogFilter.Sql, $"Wrong home position for class {currentClass} race {currentRace} pair in `playercreateinfo` table, ignoring.");
                        continue;
                    }

                    MapEntry? mapEntry = Global.sMapStore.LookupEntry(mapId);

                    if (mapEntry != null && mapEntry.Instanceable())
                    {
                        logger.Error(LogFilter.Sql, $"Home position in instanceable map for class {currentClass} race {currentRace} pair in `playercreateinfo` table, ignoring.");
                        continue;
                    }

                    PlayerInfo info = new()
                    {
                        MapId = mapId,
                        AreaId = areaId,
                        PositionX = positionX,
                        PositionY = positionY,
                        PositionZ = positionZ,
                        Orientation = orientation,
                        #pragma warning disable
                        DisplayId_M = (ushort)rEntry.ModelMale,
                        DisplayId_F = (ushort)rEntry.ModelFemale
                        #pragma warning restore
                    };

                    _playerInfo[currentRace, currentClass] = info;

                    ++count;
                }
                while (result.NextRow());

                logger.Info(LogFilter.ServerLoading, $">> Loaded {count} Player Create Definitions in {TimeHelper.GetMSTimeDiffToNow(oldMSTime)} ms");
                logger.Info(LogFilter.ServerLoading, $" ");
            }
        }

// TODO: game: ObjectMgr::LoadPlayerInfo()
//    // Load playercreate items
//    logger.Info(LogFilter.ServerLoading, "Loading Player Create Items Data...");

//    {
//        uint32 oldMSTime = getMSTime();

//        //                                                0     1      2       3
//        QueryResult result = WorldDatabase.Query("SELECT race, class, itemid, amount FROM playercreateinfo_item");

//        if (!result)
//        {
//            LOG_WARN("server.loading", ">> Loaded 0 Custom Player Create Items. DB Table `playercreateinfo_item` Is Empty.");
//            LOG_INFO("server.loading", " ");
//        }
//        else
//        {
//            uint32 count = 0;

//            do
//            {
//                Field* fields = result->Fetch();

//                uint32 current_race = fields[0].Get<uint8>();
//                if (current_race >= MAX_RACES)
//                {
//                    LOG_ERROR("sql.sql", "Wrong race {} in `playercreateinfo_item` table, ignoring.", current_race);
//                    continue;
//                }

//                uint32 current_class = fields[1].Get<uint8>();
//                if (current_class >= MAX_CLASSES)
//                {
//                    LOG_ERROR("sql.sql", "Wrong class {} in `playercreateinfo_item` table, ignoring.", current_class);
//                    continue;
//                }

//                uint32 item_id = fields[2].Get<uint32>();

//                if (!GetItemTemplate(item_id))
//                {
//                    LOG_ERROR("sql.sql", "Item id {} (race {} class {}) in `playercreateinfo_item` table but not listed in `item_template`, ignoring.", item_id, current_race, current_class);
//                    continue;
//                }

//                int32 amount = fields[3].Get<int32>();

//                if (!amount)
//                {
//                    LOG_ERROR("sql.sql", "Item id {} (class {} race {}) have amount == 0 in `playercreateinfo_item` table, ignoring.", item_id, current_race, current_class);
//                    continue;
//                }

//                if (!current_race || !current_class)
//                {
//                    uint32 min_race = current_race ? current_race : 1;
//                    uint32 max_race = current_race ? current_race + 1 : MAX_RACES;
//                    uint32 min_class = current_class ? current_class : 1;
//                    uint32 max_class = current_class ? current_class + 1 : MAX_CLASSES;
//                    for (uint32 r = min_race; r < max_race; ++r)
//                        for (uint32 c = min_class; c < max_class; ++c)
//                            PlayerCreateInfoAddItemHelper(r, c, item_id, amount);
//                }
//                else
//                    PlayerCreateInfoAddItemHelper(current_race, current_class, item_id, amount);

//                ++count;
//            } while (result->NextRow());

//            LOG_INFO("server.loading", ">> Loaded {} Custom Player Create Items in {} ms", count, GetMSTimeDiffToNow(oldMSTime));
//            LOG_INFO("server.loading", " ");
//        }
//    }

//    // Load playercreate skills
//    logger.Info(LogFilter.ServerLoading, "Loading Player Create Skill Data...");

//    {
//        uint32 oldMSTime = getMSTime();

//        QueryResult result = WorldDatabase.Query("SELECT raceMask, classMask, skill, `rank` FROM playercreateinfo_skills");

//        if (!result)
//        {
//            LOG_WARN("server.loading", ">> Loaded 0 Player Create Skills. DB Table `playercreateinfo_skills` Is Empty.");
//        }
//        else
//        {
//            uint32 count = 0;

//            do
//            {
//                Field* fields = result->Fetch();
//                uint32 raceMask = fields[0].Get<uint32>();
//                uint32 classMask = fields[1].Get<uint32>();
//                PlayerCreateInfoSkill skill;
//                skill.SkillId = fields[2].Get<uint16>();
//                skill.Rank = fields[3].Get<uint16>();

//                if (skill.Rank >= MAX_SKILL_STEP)
//                {
//                    LOG_ERROR("sql.sql", "Skill rank value {} set for skill {} raceMask {} classMask {} is too high, max allowed value is {}", skill.Rank, skill.SkillId, raceMask, classMask, MAX_SKILL_STEP);
//                    continue;
//                }

//                if (raceMask != 0 && !(raceMask & RACEMASK_ALL_PLAYABLE))
//                {
//                    LOG_ERROR("sql.sql", "Wrong race mask {} in `playercreateinfo_skills` table, ignoring.", raceMask);
//                    continue;
//                }

//                if (classMask != 0 && !(classMask & CLASSMASK_ALL_PLAYABLE))
//                {
//                    LOG_ERROR("sql.sql", "Wrong class mask {} in `playercreateinfo_skills` table, ignoring.", classMask);
//                    continue;
//                }

//                if (!sSkillLineStore.LookupEntry(skill.SkillId))
//                {
//                    LOG_ERROR("sql.sql", "Wrong skill id {} in `playercreateinfo_skills` table, ignoring.", skill.SkillId);
//                    continue;
//                }

//                for (uint32 raceIndex = RACE_HUMAN; raceIndex < MAX_RACES; ++raceIndex)
//                {
//                    if (raceMask == 0 || ((1 << (raceIndex - 1)) & raceMask))
//                    {
//                        for (uint32 classIndex = CLASS_WARRIOR; classIndex < MAX_CLASSES; ++classIndex)
//                        {
//                            if (classMask == 0 || ((1 << (classIndex - 1)) & classMask))
//                            {
//                                if (!GetSkillRaceClassInfo(skill.SkillId, raceIndex, classIndex))
//                                    continue;

//                                if (PlayerInfo * info = _playerInfo[raceIndex][classIndex])
//                                {
//                                    info->skills.push_back(skill);
//                                    ++count;
//                                }
//                            }
//                        }
//                    }
//                }
//            } while (result->NextRow());

//            LOG_INFO("server.loading", ">> Loaded {} Player Create Skills in {} ms", count, GetMSTimeDiffToNow(oldMSTime));
//            LOG_INFO("server.loading", " ");
//        }
//    }

//    // Load playercreate spells
//    logger.Info(LogFilter.ServerLoading, "Loading Player Create Spell Data...");

//    {
//        uint32 oldMSTime = getMSTime();

//        QueryResult result = WorldDatabase.Query("SELECT racemask, classmask, Spell FROM playercreateinfo_spell_custom");

//        if (!result)
//        {
//            LOG_WARN("server.loading", ">> Loaded 0 player create spells. DB table `playercreateinfo_spell_custom` is empty.");
//        }
//        else
//        {
//            uint32 count = 0;

//            do
//            {
//                Field* fields = result->Fetch();
//                uint32 raceMask = fields[0].Get<uint32>();
//                uint32 classMask = fields[1].Get<uint32>();
//                uint32 spellId = fields[2].Get<uint32>();

//                if (raceMask != 0 && !(raceMask & RACEMASK_ALL_PLAYABLE))
//                {
//                    LOG_ERROR("sql.sql", "Wrong race mask {} in `playercreateinfo_spell_custom` table, ignoring.", raceMask);
//                    continue;
//                }

//                if (classMask != 0 && !(classMask & CLASSMASK_ALL_PLAYABLE))
//                {
//                    LOG_ERROR("sql.sql", "Wrong class mask {} in `playercreateinfo_spell_custom` table, ignoring.", classMask);
//                    continue;
//                }

//                for (uint32 raceIndex = RACE_HUMAN; raceIndex < MAX_RACES; ++raceIndex)
//                {
//                    if (raceMask == 0 || ((1 << (raceIndex - 1)) & raceMask))
//                    {
//                        for (uint32 classIndex = CLASS_WARRIOR; classIndex < MAX_CLASSES; ++classIndex)
//                        {
//                            if (classMask == 0 || ((1 << (classIndex - 1)) & classMask))
//                            {
//                                if (PlayerInfo * info = _playerInfo[raceIndex][classIndex])
//                                {
//                                    info->customSpells.push_back(spellId);
//                                    ++count;
//                                }
//                            }
//                        }
//                    }
//                }
//            } while (result->NextRow());

//            LOG_INFO("server.loading", ">> Loaded {} Custom Player Create Spells in {} ms", count, GetMSTimeDiffToNow(oldMSTime));
//            LOG_INFO("server.loading", " ");
//        }
//    }

//    // Load playercreate cast spell
//    logger.Info(LogFilter.ServerLoading, "Loading Player Create Cast Spell Data...");

//    {
//        uint32 oldMSTime = getMSTime();

//        QueryResult result = WorldDatabase.Query("SELECT raceMask, classMask, spell FROM playercreateinfo_cast_spell");

//        if (!result)
//        {
//            LOG_WARN("server.loading", ">> Loaded 0 Player Create Cast Spells. DB Table `playercreateinfo_cast_spell` Is Empty.");
//        }
//        else
//        {
//            uint32 count = 0;

//            do
//            {
//                Field* fields = result->Fetch();
//                uint32 raceMask = fields[0].Get<uint32>();
//                uint32 classMask = fields[1].Get<uint32>();
//                uint32 spellId = fields[2].Get<uint32>();

//                if (raceMask != 0 && !(raceMask & RACEMASK_ALL_PLAYABLE))
//                {
//                    LOG_ERROR("sql.sql", "Wrong race mask {} in `playercreateinfo_cast_spell` table, ignoring.", raceMask);
//                    continue;
//                }

//                if (classMask != 0 && !(classMask & CLASSMASK_ALL_PLAYABLE))
//                {
//                    LOG_ERROR("sql.sql", "Wrong class mask {} in `playercreateinfo_cast_spell` table, ignoring.", classMask);
//                    continue;
//                }

//                for (uint32 raceIndex = RACE_HUMAN; raceIndex < MAX_RACES; ++raceIndex)
//                {
//                    if (raceMask == 0 || ((1 << (raceIndex - 1)) & raceMask))
//                    {
//                        for (uint32 classIndex = CLASS_WARRIOR; classIndex < MAX_CLASSES; ++classIndex)
//                        {
//                            if (classMask == 0 || ((1 << (classIndex - 1)) & classMask))
//                            {
//                                if (PlayerInfo * info = _playerInfo[raceIndex][classIndex])
//                                {
//                                    info->castSpells.push_back(spellId);
//                                    ++count;
//                                }
//                            }
//                        }
//                    }
//                }
//            } while (result->NextRow());

//            LOG_INFO("server.loading", ">> Loaded {} Player Create Cast Spells in {} ms", count, GetMSTimeDiffToNow(oldMSTime));
//            LOG_INFO("server.loading", " ");
//        }
//    }

//    // Load playercreate actions
//    logger.Info(LogFilter.ServerLoading, "Loading Player Create Action Data...");

//    {
//        uint32 oldMSTime = getMSTime();

//        //                                                0     1      2       3       4
//        QueryResult result = WorldDatabase.Query("SELECT race, class, button, action, type FROM playercreateinfo_action");

//        if (!result)
//        {
//            LOG_WARN("server.loading", ">> Loaded 0 Player Create Actions. DB Table `playercreateinfo_action` Is Empty.");
//            LOG_INFO("server.loading", " ");
//        }
//        else
//        {
//            uint32 count = 0;

//            do
//            {
//                Field* fields = result->Fetch();

//                uint32 current_race = fields[0].Get<uint8>();
//                if (current_race >= MAX_RACES)
//                {
//                    LOG_ERROR("sql.sql", "Wrong race {} in `playercreateinfo_action` table, ignoring.", current_race);
//                    continue;
//                }

//                uint32 current_class = fields[1].Get<uint8>();
//                if (current_class >= MAX_CLASSES)
//                {
//                    LOG_ERROR("sql.sql", "Wrong class {} in `playercreateinfo_action` table, ignoring.", current_class);
//                    continue;
//                }
//                if (PlayerInfo * info = _playerInfo[current_race][current_class])
//                    info->action.push_back(PlayerCreateInfoAction(fields[2].Get<uint16>(), fields[3].Get<uint32>(), fields[4].Get<uint16>()));
//                ++count;
//            } while (result->NextRow());

//            LOG_INFO("server.loading", ">> Loaded {} Player Create Actions in {} ms", count, GetMSTimeDiffToNow(oldMSTime));
//            LOG_INFO("server.loading", " ");
//        }
//    }

//    // Loading levels data (class only dependent)
//    logger.Info(LogFilter.ServerLoading, "Loading Player Create Level HP/Mana Data...");

//    {
//        uint32 oldMSTime = getMSTime();

//        //                                                0      1      2       3
//        QueryResult result = WorldDatabase.Query("SELECT class, level, basehp, basemana FROM player_classlevelstats");

//        if (!result)
//        {
//            LOG_FATAL("server.loading", ">> Loaded 0 level health/mana definitions. DB table `player_classlevelstats` is empty.");
//            exit(1);
//        }
//        uint32 count = 0;

//        do
//        {
//            Field* fields = result->Fetch();

//            uint32 current_class = fields[0].Get<uint8>();
//            if (current_class >= MAX_CLASSES)
//            {
//                LOG_ERROR("sql.sql", "Wrong class {} in `player_classlevelstats` table, ignoring.", current_class);
//                continue;
//            }

//            uint8 current_level = fields[1].Get<uint8>();      // Can't be > than STRONG_MAX_LEVEL (hardcoded level maximum) due to var type
//            if (current_level > sWorld->getIntConfig(CONFIG_MAX_PLAYER_LEVEL))
//            {
//                LOG_INFO("sql.sql", "Unused (> MaxPlayerLevel in worldserver.conf) level {} in `player_classlevelstats` table, ignoring.", current_level);
//                ++count;                                    // make result loading percent "expected" correct in case disabled detail mode for example.
//                continue;
//            }

//            PlayerClassInfo* info = _playerClassInfo[current_class];
//            if (!info)
//            {
//                info = new PlayerClassInfo();
//                info->levelInfo = new PlayerClassLevelInfo[sWorld->getIntConfig(CONFIG_MAX_PLAYER_LEVEL)];
//                _playerClassInfo[current_class] = info;
//            }

//            PlayerClassLevelInfo & levelInfo = info->levelInfo[current_level - 1];

//            levelInfo.basehealth = fields[2].Get<uint32>();
//            levelInfo.basemana = fields[3].Get<uint32>();

//            ++count;
//        } while (result->NextRow());

//        // Fill gaps and check integrity
//        for (int class_ = 0; class_ < MAX_CLASSES; ++class_)
//        {
//            // skip non existed classes
//            if (!sChrClassesStore.LookupEntry(class_))
//                continue;

//            PlayerClassInfo* pClassInfo = _playerClassInfo[class_];

//            // fatal error if no initial level data
//            if (!pClassInfo->levelInfo || (pClassInfo->levelInfo[sWorld->getIntConfig(CONFIG_START_PLAYER_LEVEL) - 1].basehealth == 0 && class_ != CLASS_DEATH_KNIGHT) || (pClassInfo->levelInfo[sWorld->getIntConfig(CONFIG_START_HEROIC_PLAYER_LEVEL) - 1].basehealth == 0 && class_ == CLASS_DEATH_KNIGHT))
//            {
//                LOG_ERROR("sql.sql", "Class {} initial level does not have health/mana data!", class_);
//                exit(1);
//            }

//            // fill level gaps
//            for (uint8 level = 1; level < sWorld->getIntConfig(CONFIG_MAX_PLAYER_LEVEL); ++level)
//            {
//                if ((pClassInfo->levelInfo[level].basehealth == 0 && class_ != CLASS_DEATH_KNIGHT) || (level >= sWorld->getIntConfig(CONFIG_START_HEROIC_PLAYER_LEVEL) && pClassInfo->levelInfo[level].basehealth == 0 && class_ == CLASS_DEATH_KNIGHT))
//                {
//                    LOG_ERROR("sql.sql", "Class {} level {} does not have health/mana data. Using stats data of level {}.", class_, level + 1, level);
//                    pClassInfo->levelInfo[level] = pClassInfo->levelInfo[level - 1];
//                }
//            }
//        }

//        LOG_INFO("server.loading", ">> Loaded {} Level Health/Mana Definitions in {} ms", count, GetMSTimeDiffToNow(oldMSTime));
//        LOG_INFO("server.loading", " ");
//    }

//    // Loading levels data (class/race dependent)
//    logger.Info(LogFilter.ServerLoading, "Loading Player Create Level Stats Data...");
//    {
//        std::array<RaceStats, MAX_RACES> raceStatModifiers;

//        uint32 oldMSTime = getMSTime();

//        //                                                          0       1         2        3         4        5
//        QueryResult raceStatsResult = WorldDatabase.Query("SELECT Race, Strength, Agility, Stamina, Intellect, Spirit FROM player_race_stats");

//        if (!raceStatsResult)
//        {
//            LOG_WARN("server.loading", ">> Loaded 0 race stats definitions. DB table `player_race_stats` is empty.");
//            LOG_INFO("server.loading", " ");

//            exit(1);
//        }

//        do
//        {
//            Field* fields = raceStatsResult->Fetch();

//            uint32 current_race = fields[0].Get<uint8>();

//            if (current_race >= MAX_RACES)
//            {
//                LOG_ERROR("sql.sql", "Wrong race {} in `player_race_stats` table, ignoring.", current_race);
//                continue;
//            }

//            for (uint32 i = 0; i < MAX_STATS; ++i)
//                raceStatModifiers[current_race].StatModifier[i] = fields[i + 1].Get<int16>();

//        }
//        while (raceStatsResult->NextRow()) ;

//        //                                                 0      1       2         3        4         5        6
//        QueryResult result = WorldDatabase.Query("SELECT Class, Level, Strength, Agility, Stamina, Intellect, Spirit FROM player_class_stats");

//        if (!result)
//        {
//            LOG_ERROR("server.loading", ">> Loaded 0 level stats definitions. DB table `player_class_stats` is empty.");
//            exit(1);
//        }

//        uint32 count = 0;

//        do
//        {
//            Field* fields = result->Fetch();

//            uint32 current_class = fields[0].Get<uint8>();

//            if (current_class >= MAX_CLASSES)
//            {
//                LOG_ERROR("sql.sql", "Wrong class {} in `player_class_stats` table, ignoring.", current_class);
//                continue;
//            }

//            uint32 current_level = fields[1].Get<uint8>();

//            if (current_level > sWorld->getIntConfig(CONFIG_MAX_PLAYER_LEVEL))
//            {
//                if (current_level > STRONG_MAX_LEVEL)        // hardcoded level maximum
//                    LOG_ERROR("sql.sql", "Wrong (> {}) level {} in `player_class_stats` table, ignoring.", STRONG_MAX_LEVEL, current_level);
//                else
//                    LOG_DEBUG("sql.sql", "Unused (> MaxPlayerLevel in worldserver.conf) level {} in `player_class_stats` table, ignoring.", current_level);

//                continue;
//            }

//            for (std::size_t race = 0; race < raceStatModifiers.size(); ++race)
//            {
//                if (PlayerInfo * info = _playerInfo[race][current_class])
//                {
//                    if (!info->levelInfo)
//                        info->levelInfo = new PlayerLevelInfo[sWorld->getIntConfig(CONFIG_MAX_PLAYER_LEVEL)];

//                    PlayerLevelInfo & levelInfo = info->levelInfo[current_level - 1];

//                    for (int i = 0; i < MAX_STATS; ++i)
//                        levelInfo.stats[i] = fields[i + 2].Get<uint16>() + raceStatModifiers[race].StatModifier[i];
//                }
//            }

//            ++count;
//        } while (result->NextRow());

//        // Fill gaps and check integrity
//        for (int race = 0; race < MAX_RACES; ++race)
//        {
//            // skip non existed races
//            if (!sChrRacesStore.LookupEntry(race))
//                continue;

//            for (int class_ = 0; class_ < MAX_CLASSES; ++class_)
//            {
//                // skip non existed classes
//                if (!sChrClassesStore.LookupEntry(class_))
//                    continue;

//                PlayerInfo* info = _playerInfo[race][class_];
//                if (!info)
//                    continue;

//                // skip expansion races if not playing with expansion
//                if (sWorld->getIntConfig(CONFIG_EXPANSION) < EXPANSION_THE_BURNING_CRUSADE && (race == RACE_BLOODELF || race == RACE_DRAENEI))
//                    continue;

//                // skip expansion classes if not playing with expansion
//                if (sWorld->getIntConfig(CONFIG_EXPANSION) < EXPANSION_WRATH_OF_THE_LICH_KING && class_ == CLASS_DEATH_KNIGHT)
//                    continue;

//                // fatal error if no initial level data
//                if (!info->levelInfo || (info->levelInfo[sWorld->getIntConfig(CONFIG_START_PLAYER_LEVEL) - 1].stats[0] == 0 && class_ != CLASS_DEATH_KNIGHT) || (info->levelInfo[sWorld->getIntConfig(CONFIG_START_HEROIC_PLAYER_LEVEL) - 1].stats[0] == 0 && class_ == CLASS_DEATH_KNIGHT))
//                {
//                    LOG_ERROR("sql.sql", "Race {} class {} initial level does not have stats data!", race, class_);
//                    exit(1);
//                }

//                // fill level gaps
//                for (uint8 level = 1; level < sWorld->getIntConfig(CONFIG_MAX_PLAYER_LEVEL); ++level)
//                {
//                    if ((info->levelInfo[level].stats[0] == 0 && class_ != CLASS_DEATH_KNIGHT) || (level >= sWorld->getIntConfig(CONFIG_START_HEROIC_PLAYER_LEVEL) && info->levelInfo[level].stats[0] == 0 && class_ == CLASS_DEATH_KNIGHT))
//                    {
//                        LOG_ERROR("sql.sql", "Race {} class {} level {} does not have stats data. Using stats data of level {}.", race, class_, level + 1, level);
//                        info->levelInfo[level] = info->levelInfo[level - 1];
//                    }
//                }
//            }
//        }

//        logger.Info(LogFilter.ServerLoading, ">> Loaded {} Level Stats Definitions in {} ms", count, GetMSTimeDiffToNow(oldMSTime));
//        logger.Info(LogFilter.ServerLoading, " ");
//    }

//    // Loading xp per level data
//    logger.Info(LogFilter.ServerLoading, "Loading Player Create XP Data...");

//    {
//        uint32 oldMSTime = getMSTime();

//        _playerXPperLevel.resize(sWorld->getIntConfig(CONFIG_MAX_PLAYER_LEVEL));
//        for (uint8 level = 0; level < sWorld->getIntConfig(CONFIG_MAX_PLAYER_LEVEL); ++level)
//            _playerXPperLevel[level] = 0;

//        //                                                 0    1
//        QueryResult result = WorldDatabase.Query("SELECT Level, Experience FROM player_xp_for_level");

//        if (!result)
//        {
//            LOG_WARN("server.loading", ">> Loaded 0 xp for level definitions. DB table `player_xp_for_level` is empty.");
//            LOG_INFO("server.loading", " ");
//            exit(1);
//        }
//        uint32 count = 0;

//        do
//        {
//            Field* fields = result->Fetch();

//            uint32 current_level = fields[0].Get<uint8>();
//            uint32 current_xp = fields[1].Get<uint32>();

//            if (current_level >= sWorld->getIntConfig(CONFIG_MAX_PLAYER_LEVEL))
//            {
//                if (current_level > STRONG_MAX_LEVEL)        // hardcoded level maximum
//                {
//                    LOG_ERROR("sql.sql", "Wrong (> {}) level {} in `player_xp_for_level` table, ignoring.", STRONG_MAX_LEVEL, current_level);
//                }
//                else
//                {
//                    LOG_DEBUG("sql.sql", "Unused (> MaxPlayerLevel in worldserver.conf) level {} in `player_xp_for_levels` table, ignoring.", current_level);
//                    ++count;                                // make result loading percent "expected" correct in case disabled detail mode for example.
//                }

//                continue;
//            }

//            //PlayerXPperLevel
//            _playerXPperLevel[current_level] = current_xp;
//            ++count;
//        }
//        while (result->NextRow());

//        // fill level gaps
//        for (uint8 level = 1; level < sWorld->getIntConfig(CONFIG_MAX_PLAYER_LEVEL); ++level)
//        {
//            if (_playerXPperLevel[level] == 0)
//            {
//                LOG_ERROR("sql.sql", "Level {} does not have XP for level data. Using data of level [{}] + 100.", level + 1, level);
//                _playerXPperLevel[level] = _playerXPperLevel[level - 1] + 100;
//            }
//        }

//        LOG_INFO("server.loading", ">> Loaded {} XP For Level Definitions in {} ms", count, GetMSTimeDiffToNow(oldMSTime));
//        LOG_INFO("server.loading", " ");
//    }
    }

    public PlayerInfo? GetPlayerInfo(byte race, byte @class)
    {
        if (race >= SharedConst.MAX_RACES)
        {
            return null;
        }

        if (@class >= SharedConst.MAX_CLASSES)
        {
            return null;
        }

        PlayerInfo? info = _playerInfo[race, @class];

        if (info == null)
        {
            return null;
        }

        return info;
    }

    public void LoadInstanceTemplate()
    {
        uint oldMSTime = TimeHelper.GetMSTime();

        //                                                0     1       2        4
        QueryResult result = DB.World.Query("SELECT map, parent, script, allowMount FROM instance_template");

        if (result.IsEmpty())
        {
            logger.Warn(LogFilter.ServerLoading, ">> Loaded 0 instance templates. DB table `page_text` is empty!");
            logger.Warn(LogFilter.ServerLoading, " ");

            return;
        }

        uint count = 0;

        do
        {
            Fields fields = result.Fetch();

            short mapID = fields[0].Get<short>();

            if (!MapMgr.IsValidMAP((uint)mapID, true))
            {
                logger.Error(LogFilter.ServerLoading, $"ObjectMgr::LoadInstanceTemplate: bad mapid {mapID} for template!");
                continue;
            }

            InstanceTemplate instanceTemplate = new InstanceTemplate();

            instanceTemplate.AllowMount = fields[3].Get<bool>();
            instanceTemplate.Parent = (uint)fields[1].Get<short>();
            instanceTemplate.ScriptId = Global.sObjectMgr.GetScriptId(fields[2].Get<string>());

            _instanceTemplateStore[mapID] = instanceTemplate;

            ++count;
        }
        while (result.NextRow());

        logger.Info(LogFilter.ServerLoading, $">> Loaded {count} Instance Templates in {TimeHelper.GetMSTimeDiffToNow(oldMSTime)} ms");
        logger.Info(LogFilter.ServerLoading, $" ");
    }

    public void LoadScriptNames()
    {
        uint oldMSTime = TimeHelper.GetMSTime();

        // We insert an empty placeholder here so we can use the
        // script id 0 as dummy for "no script found".
        _scriptNamesStore.Add("");

        QueryResult result = DB.World.Query(
            @"SELECT DISTINCT(ScriptName) FROM achievement_criteria_data WHERE ScriptName <> '' AND type = 11 
              UNION 
              SELECT DISTINCT(ScriptName) FROM battleground_template WHERE ScriptName <> '' 
              UNION 
              SELECT DISTINCT(ScriptName) FROM creature WHERE ScriptName <> '' 
              UNION 
              SELECT DISTINCT(ScriptName) FROM creature_template WHERE ScriptName <> '' 
              UNION 
              SELECT DISTINCT(ScriptName) FROM gameobject WHERE ScriptName <> '' 
              UNION 
              SELECT DISTINCT(ScriptName) FROM gameobject_template WHERE ScriptName <> '' 
              UNION 
              SELECT DISTINCT(ScriptName) FROM item_template WHERE ScriptName <> '' 
              UNION 
              SELECT DISTINCT(ScriptName) FROM areatrigger_scripts WHERE ScriptName <> '' 
              UNION 
              SELECT DISTINCT(ScriptName) FROM spell_script_names WHERE ScriptName <> '' 
              UNION 
              SELECT DISTINCT(ScriptName) FROM transports WHERE ScriptName <> '' 
              UNION 
              SELECT DISTINCT(ScriptName) FROM game_weather WHERE ScriptName <> '' 
              UNION 
              SELECT DISTINCT(ScriptName) FROM conditions WHERE ScriptName <> '' 
              UNION 
              SELECT DISTINCT(ScriptName) FROM outdoorpvp_template WHERE ScriptName <> '' 
              UNION 
              SELECT DISTINCT(script) FROM instance_template WHERE script <> ''");

        if (result.IsEmpty())
        {
            logger.Info(LogFilter.ServerLoading, " ");
            logger.Error(LogFilter.Sql, ">> Loaded empty set of Script Names!");

            return;
        }

        do
        {
            string? name = result.Read<string>(0);

            if (name != null)
            {
                _scriptNamesStore.Add(name);
            }
        }
        while (result.NextRow());

        _scriptNamesStore.Sort();

        logger.Info(LogFilter.ServerLoading, $">> Loaded {_scriptNamesStore.Count} ScriptNames in {TimeHelper.GetMSTimeDiffToNow(oldMSTime)} ms");
        logger.Info(LogFilter.ServerLoading, $" ");
    }

    private uint GetScriptId(string? name)
    {
        // use binary search to find the script name in the sorted vector
        // assume "" is the first element
        if (string.IsNullOrEmpty(name))
        {
            return 0;
        }

        int firstIndex = _scriptNamesStore.IndexOf(name);

        if (firstIndex < 0)
        {
            return 0;
        }

        return (uint)firstIndex;
    }

    public InstanceTemplate? GetInstanceTemplate(uint mapid)
    {
        if (_instanceTemplateStore.TryGetValue((short)mapid, out InstanceTemplate instanceTemplate))
        {
            return instanceTemplate;
        }
        else
        {
            return null;
        }
    }

    public CreatureTemplate? GetCreatureTemplate(uint entry)
    {
        return _creatureTemplateStore.ContainsKey(entry) ? _creatureTemplateStore[entry] : null;
    }

    public void LoadItemTemplates()
    {
        uint oldMSTime = TimeHelper.GetMSTime();

        //                                            0      1       2               3              4        5        6       7          8         9        10        11           12
        QueryResult result = DB.World.Query("SELECT entry, class, subclass, SoundOverrideSubclass, name, displayid, Quality, Flags, FlagsExtra, BuyCount, BuyPrice, SellPrice, InventoryType, " +
                             //                                              13              14           15          16             17               18                19              20
                             "AllowableClass, AllowableRace, ItemLevel, RequiredLevel, RequiredSkill, RequiredSkillRank, requiredspell, requiredhonorrank, " +
                             //                                              21                      22                       23               24        25          26             27           28
                             "RequiredCityRank, RequiredReputationFaction, RequiredReputationRank, maxcount, stackable, ContainerSlots, StatsCount, stat_type1, " +
                             //                                            29           30          31           32          33           34          35           36          37           38
                             "stat_value1, stat_type2, stat_value2, stat_type3, stat_value3, stat_type4, stat_value4, stat_type5, stat_value5, stat_type6, " +
                             //                                            39           40          41           42           43          44           45           46           47
                             "stat_value6, stat_type7, stat_value7, stat_type8, stat_value8, stat_type9, stat_value9, stat_type10, stat_value10, " +
                             //                                                   48                    49           50        51        52         53        54         55      56      57        58
                             "ScalingStatDistribution, ScalingStatValue, dmg_min1, dmg_max1, dmg_type1, dmg_min2, dmg_max2, dmg_type2, armor, holy_res, fire_res, " +
                             //                                            59          60         61          62       63       64            65            66          67               68
                             "nature_res, frost_res, shadow_res, arcane_res, delay, ammo_type, RangedModRange, spellid_1, spelltrigger_1, spellcharges_1, " +
                             //                                              69              70                71                 72                 73           74               75
                             "spellppmRate_1, spellcooldown_1, spellcategory_1, spellcategorycooldown_1, spellid_2, spelltrigger_2, spellcharges_2, " +
                             //                                              76               77              78                  79                 80           81               82
                             "spellppmRate_2, spellcooldown_2, spellcategory_2, spellcategorycooldown_2, spellid_3, spelltrigger_3, spellcharges_3, " +
                             //                                              83               84              85                  86                 87           88               89
                             "spellppmRate_3, spellcooldown_3, spellcategory_3, spellcategorycooldown_3, spellid_4, spelltrigger_4, spellcharges_4, " +
                             //                                              90               91              92                  93                  94          95               96
                             "spellppmRate_4, spellcooldown_4, spellcategory_4, spellcategorycooldown_4, spellid_5, spelltrigger_5, spellcharges_5, " +
                             //                                              97               98              99                  100                 101        102         103       104          105
                             "spellppmRate_5, spellcooldown_5, spellcategory_5, spellcategorycooldown_5, bonding, description, PageText, LanguageID, PageMaterial, " +
                             //                                            106       107     108      109          110            111       112     113         114       115   116     117
                             "startquest, lockid, Material, sheath, RandomProperty, RandomSuffix, block, itemset, MaxDurability, area, Map, BagFamily, " +
                             //                                            118             119             120             121             122            123              124            125
                             "TotemCategory, socketColor_1, socketContent_1, socketColor_2, socketContent_2, socketColor_3, socketContent_3, socketBonus, " +
                             //                                            126                 127                     128            129            130            131         132         133
                             "GemProperties, RequiredDisenchantSkill, ArmorDamageModifier, duration, ItemLimitCategory, HolidayId, ScriptName, DisenchantID, " +
                             //                                           134        135            136
                             "FoodType, minMoneyLoot, maxMoneyLoot, flagsCustom FROM item_template");

        if (result.IsEmpty())
        {
            logger.Warn(LogFilter.ServerLoading, ">> Loaded 0 item templates. DB table `item_template` is empty.");
            logger.Info(LogFilter.ServerLoading, " ");

            return;
        }

        uint count = 0;

        // original inspiration https://github.com/TrinityCore/TrinityCore/commit/0c44bd33ee7b42c924859139a9f4b04cf2b91261
        bool enforceDBCAttributes = Global.sWorld.GetBoolConfig(WorldBoolConfigs.CONFIG_DBC_ENFORCE_ITEM_ATTRIBUTES);

        do
        {
            List<Field> fields = result.Fetch();

            uint entry = fields[0].Get<uint>();

            ItemTemplate itemTemplate = new();
            _itemTemplateStore[entry] = itemTemplate;

            itemTemplate.ItemId = entry;
            itemTemplate.Class = fields[1].Get<byte>();
            itemTemplate.SubClass = fields[2].Get<byte>();
            itemTemplate.SoundOverrideSubclass = fields[3].Get<sbyte>();
            itemTemplate.Name1 = fields[4].Get<string>() ?? string.Empty;
            itemTemplate.DisplayInfoID = fields[5].Get<uint>();
            itemTemplate.Quality = fields[6].Get<byte>();
            itemTemplate.Flags = fields[7].Get<uint>();
            itemTemplate.Flags2 = fields[8].Get<uint>();
            itemTemplate.BuyCount = fields[9].Get<byte>();
            itemTemplate.BuyPrice = (int)(fields[10].Get<long>() * Global.sWorld.GetRate(Rates.RATE_BUYVALUE_ITEM_POOR + itemTemplate.Quality));
            itemTemplate.SellPrice = (uint)(fields[11].Get<uint>() * Global.sWorld.GetRate(Rates.RATE_SELLVALUE_ITEM_POOR + itemTemplate.Quality));
            itemTemplate.InventoryType = fields[12].Get<byte>();
            itemTemplate.AllowableClass = (uint)fields[13].Get<int>();
            itemTemplate.AllowableRace = (uint)fields[14].Get<int>();
            itemTemplate.ItemLevel = fields[15].Get<ushort>();
            itemTemplate.RequiredLevel = fields[16].Get<byte>();
            itemTemplate.RequiredSkill = fields[17].Get<ushort>();
            itemTemplate.RequiredSkillRank = fields[18].Get<ushort>();
            itemTemplate.RequiredSpell = fields[19].Get<uint>();
            itemTemplate.RequiredHonorRank = fields[20].Get<uint>();
            itemTemplate.RequiredCityRank = fields[21].Get<uint>();
            itemTemplate.RequiredReputationFaction = fields[22].Get<ushort>();
            itemTemplate.RequiredReputationRank = fields[23].Get<ushort>();
            itemTemplate.MaxCount = fields[24].Get<int>();
            itemTemplate.Stackable = fields[25].Get<int>();
            itemTemplate.ContainerSlots = fields[26].Get<byte>();
            itemTemplate.StatsCount = fields[27].Get<byte>();

            for (byte i = 0; i < itemTemplate.StatsCount; ++i)
            {
                itemTemplate.ItemStat[i].ItemStatType = fields[28 + i * 2].Get<byte>();
                itemTemplate.ItemStat[i].ItemStatValue = fields[29 + i * 2].Get<int>();
            }

            itemTemplate.ScalingStatDistribution = fields[48].Get<ushort>();
            itemTemplate.ScalingStatValue = fields[49].Get<uint>();

            for (byte i = 0; i < ItemTemplateConst.MAX_ITEM_PROTO_DAMAGES; ++i)
            {
                itemTemplate.Damage[i].DamageMin = fields[50 + i * 3].Get<float>();
                itemTemplate.Damage[i].DamageMax = fields[51 + i * 3].Get<float>();
                itemTemplate.Damage[i].DamageType = fields[52 + i * 3].Get<byte>();
            }

            itemTemplate.Armor = fields[56].Get<uint>();
            itemTemplate.HolyRes = fields[57].Get<int>();
            itemTemplate.FireRes = fields[58].Get<int>();
            itemTemplate.NatureRes = fields[59].Get<int>();
            itemTemplate.FrostRes = fields[60].Get<int>();
            itemTemplate.ShadowRes = fields[61].Get<int>();
            itemTemplate.ArcaneRes = fields[62].Get<int>();
            itemTemplate.Delay = fields[63].Get<ushort>();
            itemTemplate.AmmoType = fields[64].Get<byte>();
            itemTemplate.RangedModRange = fields[65].Get<float>();

            for (byte i = 0; i < ItemTemplateConst.MAX_ITEM_PROTO_SPELLS; ++i)
            {
                itemTemplate.Spells[i].SpellId = fields[66 + i * 7].Get<int>();
                itemTemplate.Spells[i].SpellTrigger = fields[67 + i * 7].Get<byte>();
                itemTemplate.Spells[i].SpellCharges = fields[68 + i * 7].Get<short>();
                itemTemplate.Spells[i].SpellPPMRate = fields[69 + i * 7].Get<float>();
                itemTemplate.Spells[i].SpellCooldown = fields[70 + i * 7].Get<int>();
                itemTemplate.Spells[i].SpellCategory = fields[71 + i * 7].Get<ushort>();
                itemTemplate.Spells[i].SpellCategoryCooldown = fields[72 + i * 7].Get<int>();
            }

            itemTemplate.Bonding = fields[101].Get<byte>();
            itemTemplate.Description = fields[102].Get <string>() ?? string.Empty;
            itemTemplate.PageText = fields[103].Get<uint>();
            itemTemplate.LanguageID = fields[104].Get<byte>();
            itemTemplate.PageMaterial = fields[105].Get<byte>();
            itemTemplate.StartQuest = fields[106].Get<uint>();
            itemTemplate.LockID = fields[107].Get<uint>();
            itemTemplate.Material = fields[108].Get<sbyte>();
            itemTemplate.Sheath = fields[109].Get<byte>();
            itemTemplate.RandomProperty = fields[110].Get<int>();
            itemTemplate.RandomSuffix = fields[111].Get<int>();
            itemTemplate.Block = fields[112].Get<uint>();
            itemTemplate.ItemSet = fields[113].Get<uint>();
            itemTemplate.MaxDurability = fields[114].Get<ushort>();
            itemTemplate.Area = fields[115].Get<uint>();
            itemTemplate.Map = fields[116].Get<ushort>();
            itemTemplate.BagFamily = fields[117].Get<uint>();
            itemTemplate.TotemCategory = fields[118].Get<uint>();

            for (byte i = 0; i < ItemTemplateConst.MAX_ITEM_PROTO_SOCKETS; ++i)
            {
                itemTemplate.Socket[i].Color = fields[119 + i * 2].Get<byte>();
                itemTemplate.Socket[i].Content = fields[120 + i * 2].Get<uint>();
            }

            itemTemplate.socketBonus = fields[125].Get<uint>();
            itemTemplate.GemProperties = fields[126].Get<uint>();
            itemTemplate.RequiredDisenchantSkill = (uint)fields[127].Get<short>();
            itemTemplate.ArmorDamageModifier = fields[128].Get<float>();
            itemTemplate.Duration = fields[129].Get<uint>();
            itemTemplate.ItemLimitCategory = (uint)fields[130].Get<short>();
            itemTemplate.HolidayId = fields[131].Get<uint>();
            itemTemplate.ScriptId = Global.sObjectMgr.GetScriptId(fields[132].Get <string> ());
            itemTemplate.DisenchantID = fields[133].Get<uint>();
            itemTemplate.FoodType = fields[134].Get<byte>();
            itemTemplate.MinMoneyLoot = fields[135].Get<uint>();
            itemTemplate.MaxMoneyLoot = fields[136].Get<uint>();
            itemTemplate.FlagsCu = fields[137].Get<uint>();

            // Checks
            ItemEntry? dbcitem = Global.sItemStore.LookupEntry(entry);

            if (dbcitem != null)
            {

                // TODO: game: ObjectMgr::LoadItemTemplates()
            }

            // TODO: game: ObjectMgr::LoadItemTemplates()

            count++;
        }
        while (result.NextRow());



        // TODO: game: ObjectMgr::LoadItemTemplates()

        logger.Info(LogFilter.ServerLoading, $">> Loaded {count} Item Templates in {TimeHelper.GetMSTimeDiffToNow(oldMSTime)} ms");
        logger.Info(LogFilter.ServerLoading, " ");
    }

    public ItemTemplate? GetItemTemplate(uint itemId)
    { 
        return _itemTemplateStore.ContainsKey(itemId) ? _itemTemplateStore[itemId] : null;
    }

    public static ResponseCodes CheckPlayerName(string name, bool create = false)
    {
        // Check for invalid characters
        string wname;

        try
        {
            wname = Encoding.Unicode.GetString(Encoding.UTF8.GetBytes(name));
        }
        catch
        {
            return ResponseCodes.CHAR_NAME_INVALID_CHARACTER;
        }

        // Check for too long name
        if (wname.Length > MAX_PLAYER_NAME)
        {
            return ResponseCodes.CHAR_NAME_TOO_LONG;
        }

        // Check for too short name
        uint minName = ConfigMgr.GetOption("MinPlayerName", 2U);

        if (minName < 1 || minName > MAX_PLAYER_NAME)
        {
            minName = 2;
        }

        if (wname.Length < minName)
        {
            return ResponseCodes.CHAR_NAME_TOO_SHORT;
        }

        return ResponseCodes.CHAR_NAME_SUCCESS;
    }

    internal bool IsReservedName(string name)
    {
        // TODO: game: ObjectManager::IsReservedName(string name)
        return false;
    }

    internal bool IsProfanityName(string name)
    {
        // TODO: game: ObjectManager::IsProfanityName(string name)
        return false;
    }
}
