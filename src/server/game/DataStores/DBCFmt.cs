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

namespace AzerothCore.DataStores;

public static class DBCFmt
{
    public static readonly string Achievementfmt = "niixssssssssssssssssxxxxxxxxxxxxxxxxxxiixixxxxxxxxxxxxxxxxxxii";
    public static readonly string AchievementCategoryfmt = "nixxxxxxxxxxxxxxxxxx";
    public static readonly string AchievementCriteriafmt = "niiiiiiiixxxxxxxxxxxxxxxxxiiiix";
    public static readonly string AreaTableEntryfmt = "niiiixxxxxissssssssssssssssxiiiiixxx";
    public static readonly string AreaGroupEntryfmt = "niiiiiii";
    public static readonly string AreaPOIEntryfmt = "niiiiiiiiiiifffixixxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxix";
    public static readonly string AuctionHouseEntryfmt = "niiixxxxxxxxxxxxxxxxx";
    public static readonly string BankBagSlotPricesEntryfmt = "ni";
    public static readonly string BarberShopStyleEntryfmt = "nixxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxiii";
    public static readonly string BattlemasterListEntryfmt = "niiiiiiiiixssssssssssssssssxiixx";
    public static readonly string CharStartOutfitEntryfmt = "dbbbXiiiiiiiiiiiiiiiiiiiiiiiixxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
    public static readonly string CharTitlesEntryfmt = "nxssssssssssssssssxssssssssssssssssxi";
    public static readonly string ChatChannelsEntryfmt = "nixssssssssssssssssxxxxxxxxxxxxxxxxxx"; // ChatChannelsEntryfmt, index not used (more compact store)
    public static readonly string ChrClassesEntryfmt = "nxixssssssssssssssssxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxixii";
    public static readonly string ChrRacesEntryfmt = "niixiixixxxxixssssssssssssssssxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxi";
    public static readonly string CinematicCameraEntryfmt = "nsiffff";
    public static readonly string CinematicSequencesEntryfmt = "nxixxxxxxx";
    public static readonly string CreatureDisplayInfofmt = "nixifxxxxxxxxxxx";
    public static readonly string CreatureDisplayInfoExtrafmt = "diixxxxxxxxxxxxxxxxxx";
    public static readonly string CreatureFamilyfmt = "nfifiiiiixssssssssssssssssxx";
    public static readonly string CreatureModelDatafmt = "nixxfxxxxxxxxxfffxxxxxxxxxxx";
    public static readonly string CreatureSpellDatafmt = "niiiixxxx";
    public static readonly string CreatureTypefmt = "nxxxxxxxxxxxxxxxxxx";
    public static readonly string CurrencyTypesfmt = "xnxi";
    public static readonly string DestructibleModelDatafmt = "nxxixxxixxxixxxixxx";
    public static readonly string DungeonEncounterfmt = "niixissssssssssssssssxx";
    public static readonly string DurabilityCostsfmt = "niiiiiiiiiiiiiiiiiiiiiiiiiiiii";
    public static readonly string DurabilityQualityfmt = "nf";
    public static readonly string EmotesEntryfmt = "nxxiiix";
    public static readonly string EmotesTextEntryfmt = "nxixxxxxxxxxxxxxxxx";
    public static readonly string FactionEntryfmt = "niiiiiiiiiiiiiiiiiiffixssssssssssssssssxxxxxxxxxxxxxxxxxx";
    public static readonly string FactionTemplateEntryfmt = "niiiiiiiiiiiii";
    public static readonly string GameObjectArtKitfmt = "nxxxxxxx";
    public static readonly string GameObjectDisplayInfofmt = "nsxxxxxxxxxxffffffx";
    public static readonly string GemPropertiesEntryfmt = "nixxi";
    public static readonly string GlyphPropertiesfmt = "niii";
    public static readonly string GlyphSlotfmt = "nii";
    public static readonly string GtBarberShopCostBasefmt = "df";
    public static readonly string GtCombatRatingsfmt = "df";
    public static readonly string GtChanceToMeleeCritBasefmt = "df";
    public static readonly string GtChanceToMeleeCritfmt = "df";
    public static readonly string GtChanceToSpellCritBasefmt = "df";
    public static readonly string GtChanceToSpellCritfmt = "df";
    public static readonly string GtNPCManaCostScalerfmt = "df";
    public static readonly string GtOCTClassCombatRatingScalarfmt = "df";
    public static readonly string GtOCTRegenHPfmt = "df";
    public static readonly string GtRegenHPPerSptfmt = "df";
    public static readonly string GtRegenMPPerSptfmt = "df";
    public static readonly string Holidaysfmt = "niiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiixxsiix";
    public static readonly string Itemfmt = "niiiiiii";
    public static readonly string ItemBagFamilyfmt = "nxxxxxxxxxxxxxxxxx";
    public static readonly string ItemDisplayTemplateEntryfmt = "nxxxxsxxxxxxxxxxxxxxxxxxx";
    public static readonly string ItemExtendedCostEntryfmt = "niiiiiiiiiiiiiix";
    public static readonly string ItemLimitCategoryEntryfmt = "nxxxxxxxxxxxxxxxxxii";
    public static readonly string ItemRandomPropertiesfmt = "nxiiiiissssssssssssssssx";
    public static readonly string ItemRandomSuffixfmt = "nssssssssssssssssxxiiiiiiiiii";
    public static readonly string ItemSetEntryfmt = "dssssssssssssssssxiiiiiiiiiixxxxxxxiiiiiiiiiiiiiiiiii";
    public static readonly string LFGDungeonEntryfmt = "nssssssssssssssssxiiiiiiiiixxixixxxxxxxxxxxxxxxxx";
    public static readonly string LightEntryfmt = "nifffxxxxxxxxxx";
    public static readonly string LiquidTypefmt = "nxxixixxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
    public static readonly string LockEntryfmt = "niiiiiiiiiiiiiiiiiiiiiiiixxxxxxxx";
    public static readonly string MailTemplateEntryfmt = "nxxxxxxxxxxxxxxxxxssssssssssssssssx";
    public static readonly string MapEntryfmt = "nxiixssssssssssssssssxixxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxixiffxiii";
    public static readonly string MapDifficultyEntryfmt = "diisxxxxxxxxxxxxxxxxiix";
    public static readonly string MovieEntryfmt = "nxx";
    public static readonly string NamesReservedfmt = "xsx";
    public static readonly string NamesProfanityfmt = "xsx";
    public static readonly string OverrideSpellDatafmt = "niiiiiiiiiix";
    public static readonly string PowerDisplayfmt = "nixxxx";
    public static readonly string QuestSortEntryfmt = "nxxxxxxxxxxxxxxxxx";
    public static readonly string QuestXPfmt = "niiiiiiiiii";
    public static readonly string QuestFactionRewardfmt = "niiiiiiiiii";
    public static readonly string PvPDifficultyfmt = "diiiii";
    public static readonly string RandomPropertiesPointsfmt = "niiiiiiiiiiiiiii";
    public static readonly string ScalingStatDistributionfmt = "niiiiiiiiiiiiiiiiiiiii";
    public static readonly string ScalingStatValuesfmt = "iniiiiiiiiiiiiiiiiiiiiii";
    public static readonly string SkillLinefmt = "nixssssssssssssssssxxxxxxxxxxxxxxxxxxixxxxxxxxxxxxxxxxxi";
    public static readonly string SkillLineAbilityfmt = "niiiixxiiiiixx";
    public static readonly string SkillRaceClassInfofmt = "diiiixix";
    public static readonly string SkillTiersfmt = "nxxxxxxxxxxxxxxxxiiiiiiiiiiiiiiii";
    public static readonly string SoundEntriesfmt = "nxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
    public static readonly string SpellCastTimefmt = "nixx";
    public static readonly string SpellCategoryfmt = "ni";
    public static readonly string SpellDifficultyfmt = "niiii";
    public static readonly string SpellDurationfmt = "niii";
    public static readonly string SpellEntryfmt = "niiiiiiiiiiiixixiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiifxiiiiiiiiiiiiiiiiiiiiiiiiiiiifffiiiiiiiiiiiiiiiiiiiiifffiiiiiiiiiiiiiiifffiiiiiiiiiiiiiissssssssssssssssxssssssssssssssssxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxiiiiiiiiiiixfffxxxiiiiixxfffxx";
    public static readonly string SpellFocusObjectfmt = "nxxxxxxxxxxxxxxxxx";
    public static readonly string SpellItemEnchantmentfmt = "niiiiiiixxxiiissssssssssssssssxiiiiiii";
    public static readonly string SpellItemEnchantmentConditionfmt = "nbbbbbxxxxxbbbbbbbbbbiiiiiXXXXX";
    public static readonly string SpellRadiusfmt = "nfff";
    public static readonly string SpellRangefmt = "nffffixxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
    public static readonly string SpellRuneCostfmt = "niiii";
    public static readonly string SpellShapeshiftfmt = "nxxxxxxxxxxxxxxxxxxiixiiixxiiiiiiii";
    public static readonly string SpellVisualfmt = "dxxxxxxiixxxxxxxxxxxxxxxxxxxxxxx";
    public static readonly string StableSlotPricesfmt = "ni";
    public static readonly string SummonPropertiesfmt = "niiiii";
    public static readonly string TalentEntryfmt = "niiiiiiiixxxxixxixxixxx";
    public static readonly string TalentTabEntryfmt = "nxxxxxxxxxxxxxxxxxxxiiix";
    public static readonly string TaxiNodesEntryfmt = "nifffssssssssssssssssxii";
    public static readonly string TaxiPathEntryfmt = "niii";
    public static readonly string TaxiPathNodeEntryfmt = "diiifffiiii";
    public static readonly string TeamContributionPointsfmt = "df";
    public static readonly string TotemCategoryEntryfmt = "nxxxxxxxxxxxxxxxxxii";
    public static readonly string TransportAnimationfmt = "diifffx";
    public static readonly string TransportRotationfmt = "diiffff";
    public static readonly string VehicleEntryfmt = "niffffiiiiiiiifffffffffffffffssssfifiixx";
    public static readonly string VehicleSeatEntryfmt = "niiffffffffffiiiiiifffffffiiifffiiiiiiiffiiiiixxxxxxxxxxxx";
    public static readonly string WMOAreaTableEntryfmt = "niiixxxxxiixxxxxxxxxxxxxxxxx";
    public static readonly string WorldMapAreaEntryfmt = "xinxffffixx";
    public static readonly string WorldMapOverlayEntryfmt = "nxiiiixxxxxxxxxxx";
}
