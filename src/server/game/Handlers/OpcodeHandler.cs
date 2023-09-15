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

public interface IOpcodeHandler :
                    IMiscOpcodeHandler,
                    ICharacterHandler
{
    void HandleAcceptGrantLevel(WorldPacketData recvData);
    void HandleAcceptTradeOpcode(WorldPacketData recvData);
    void HandleActivateTaxiExpressOpcode(WorldPacketData recvData);
    void HandleActivateTaxiOpcode(WorldPacketData recvData);
    void HandleAddFriendOpcode(WorldPacketData recvData);
    void HandleAddIgnoreOpcode(WorldPacketData recvData);
    void HandleAlterAppearance(WorldPacketData recvData);
    void HandleAreaSpiritHealerQueryOpcode(WorldPacketData recvData);
    void HandleAreaSpiritHealerQueueOpcode(WorldPacketData recvData);
    void HandleAreaTriggerOpcode(WorldPacketData recvData);
    void HandleArenaTeamAcceptOpcode(WorldPacketData recvData);
    void HandleArenaTeamDeclineOpcode(WorldPacketData recvData);
    void HandleArenaTeamDisbandOpcode(WorldPacketData recvData);
    void HandleArenaTeamInviteOpcode(WorldPacketData recvData);
    void HandleArenaTeamLeaderOpcode(WorldPacketData recvData);
    void HandleArenaTeamLeaveOpcode(WorldPacketData recvData);
    void HandleArenaTeamQueryOpcode(WorldPacketData recvData);
    void HandleArenaTeamRemoveOpcode(WorldPacketData recvData);
    void HandleArenaTeamRosterOpcode(WorldPacketData recvData);
    void HandleAttackStopOpcode(WorldPacketData recvData);
    void HandleAttackSwingOpcode(WorldPacketData recvData);
    void HandleAuctionHelloOpcode(WorldPacketData recvData);
    void HandleAuctionListBidderItems(WorldPacketData recvData);
    void HandleAuctionListItems(WorldPacketData recvData);
    void HandleAuctionListOwnerItems(WorldPacketData recvData);
    void HandleAuctionListPendingSales(WorldPacketData recvData);
    void HandleAuctionPlaceBid(WorldPacketData recvData);
    void HandleAuctionRemoveItem(WorldPacketData recvData);
    void HandleAuctionSellItem(WorldPacketData recvData);
    void HandleAutoBankItemOpcode(WorldPacketData recvData);
    void HandleAutoEquipItemOpcode(WorldPacketData recvData);
    void HandleAutoEquipItemSlotOpcode(WorldPacketData recvData);
    void HandleAutoStoreBagItemOpcode(WorldPacketData recvData);
    void HandleAutoStoreBankItemOpcode(WorldPacketData recvData);
    void HandleAutostoreLootItemOpcode(WorldPacketData recvData);
    void HandleBankerActivateOpcode(WorldPacketData recvData);
    void HandleBattlefieldLeaveOpcode(WorldPacketData recvData);
    void HandleBattlefieldListOpcode(WorldPacketData recvData);
    void HandleBattleFieldPortOpcode(WorldPacketData recvData);
    void HandleBattlefieldStatusOpcode(WorldPacketData recvData);
    void HandleBattlegroundPlayerPositionsOpcode(WorldPacketData recvData);
    void HandleBattlemasterHelloOpcode(WorldPacketData recvData);
    void HandleBattlemasterJoinArena(WorldPacketData recvData);
    void HandleBattlemasterJoinOpcode(WorldPacketData recvData);
    void HandleBeginTradeOpcode(WorldPacketData recvData);
    void HandleBfEntryInviteResponse(WorldPacketData recvData);
    void HandleBfExitRequest(WorldPacketData recvData);
    void HandleBfQueueInviteResponse(WorldPacketData recvData);
    void HandleBinderActivateOpcode(WorldPacketData recvData);
    void HandleBusyTradeOpcode(WorldPacketData recvData);
    void HandleBuybackItem(WorldPacketData recvData);
    void HandleBuyBankSlotOpcode(WorldPacketData recvData);
    void HandleBuyItemInSlotOpcode(WorldPacketData recvData);
    void HandleBuyItemOpcode(WorldPacketData recvData);
    void HandleBuyStableSlot(WorldPacketData recvData);
    void HandleCalendarAddEvent(WorldPacketData recvData);
    void HandleCalendarArenaTeam(WorldPacketData recvData);
    void HandleCalendarComplain(WorldPacketData recvData);
    void HandleCalendarCopyEvent(WorldPacketData recvData);
    void HandleCalendarEventInvite(WorldPacketData recvData);
    void HandleCalendarEventModeratorStatus(WorldPacketData recvData);
    void HandleCalendarEventRemoveInvite(WorldPacketData recvData);
    void HandleCalendarEventRsvp(WorldPacketData recvData);
    void HandleCalendarEventSignup(WorldPacketData recvData);
    void HandleCalendarEventStatus(WorldPacketData recvData);
    void HandleCalendarGetCalendar(WorldPacketData recvData);
    void HandleCalendarGetEvent(WorldPacketData recvData);
    void HandleCalendarGetNumPending(WorldPacketData recvData);
    void HandleCalendarGuildFilter(WorldPacketData recvData);
    void HandleCalendarRemoveEvent(WorldPacketData recvData);
    void HandleCalendarUpdateEvent(WorldPacketData recvData);
    void HandleCancelAuraOpcode(WorldPacketData recvData);
    void HandleCancelAutoRepeatSpellOpcode(WorldPacketData recvData);
    void HandleCancelCastOpcode(WorldPacketData recvData);
    void HandleCancelChanneling(WorldPacketData recvData);
    void HandleCancelGrowthAuraOpcode(WorldPacketData recvData);
    void HandleCancelMountAuraOpcode(WorldPacketData recvData);
    void HandleCancelTempEnchantmentOpcode(WorldPacketData recvData);
    void HandleCancelTradeOpcode(WorldPacketData recvData);
    void HandleCastSpellOpcode(WorldPacketData recvData);
    void HandleChangeSeatsOnControlledVehicle(WorldPacketData recvData);
    void HandleChannelAnnouncements(WorldPacketData recvData);
    void HandleChannelBan(WorldPacketData recvData);
    void HandleChannelDeclineInvite(WorldPacketData recvData);
    void HandleChannelDisplayListQuery(WorldPacketData recvData);
    void HandleChannelInvite(WorldPacketData recvData);
    void HandleChannelKick(WorldPacketData recvData);
    void HandleChannelList(WorldPacketData recvData);
    void HandleChannelModerateOpcode(WorldPacketData recvData);
    void HandleChannelModerator(WorldPacketData recvData);
    void HandleChannelMute(WorldPacketData recvData);
    void HandleChannelOwner(WorldPacketData recvData);
    void HandleChannelPassword(WorldPacketData recvData);
    void HandleChannelSetOwner(WorldPacketData recvData);
    void HandleChannelUnban(WorldPacketData recvData);
    void HandleChannelUnmoderator(WorldPacketData recvData);
    void HandleChannelUnmute(WorldPacketData recvData);
    void HandleChannelVoiceOnOpcode(WorldPacketData recvData);
    void HandleChatIgnoredOpcode(WorldPacketData recvData);
    void HandleClearChannelWatch(WorldPacketData recvData);
    void HandleClearTradeItemOpcode(WorldPacketData recvData);
    void HandleComplainOpcode(WorldPacketData recvData);
    void HandleCompleteCinematic(WorldPacketData recvData);
    void HandleContactListOpcode(WorldPacketData recvData);
    void HandleCorpseMapPositionQuery(WorldPacketData recvData);
    void HandleCorpseQueryOpcode(WorldPacketData recvData);
    void HandleCreatureQueryOpcode(WorldPacketData recvData);
    void HandleDelFriendOpcode(WorldPacketData recvData);
    void HandleDelIgnoreOpcode(WorldPacketData recvData);
    void HandleDestroyItemOpcode(WorldPacketData recvData);
    void HandleDismissControlledVehicle(WorldPacketData recvData);
    void HandleDismissCritter(WorldPacketData recvData);
    void HandleDuelAcceptedOpcode(WorldPacketData recvData);
    void HandleDuelCancelledOpcode(WorldPacketData recvData);
    void HandleEjectPassenger(WorldPacketData recvData);
    void HandleEmoteOpcode(WorldPacketData recvData);
    void HandleEnterPlayerVehicle(WorldPacketData recvData);
    void HandleEquipmentSetDelete(WorldPacketData recvData);
    void HandleEquipmentSetSave(WorldPacketData recvData);
    void HandleEquipmentSetUse(WorldPacketData recvData);
    void HandleFarSightOpcode(WorldPacketData recvData);
    void HandleFeatherFallAck(WorldPacketData recvData);
    void HandleForceSpeedChangeAck(WorldPacketData recvData);
    void HandleGameObjectQueryOpcode(WorldPacketData recvData);
    void HandleGameobjectReportUse(WorldPacketData recvData);
    void HandleGameObjectUseOpcode(WorldPacketData recvData);
    void HandleGetChannelMemberCount(WorldPacketData recvData);
    void HandleGetMailList(WorldPacketData recvData);
    void HandleGMResponseResolve(WorldPacketData recvData);
    void HandleGMSurveySubmit(WorldPacketData recvData);
    void HandleGMTicketCreateOpcode(WorldPacketData recvData);
    void HandleGMTicketDeleteOpcode(WorldPacketData recvData);
    void HandleGMTicketGetTicketOpcode(WorldPacketData recvData);
    void HandleGMTicketSystemStatusOpcode(WorldPacketData recvData);
    void HandleGMTicketUpdateOpcode(WorldPacketData recvData);
    void HandleGossipHelloOpcode(WorldPacketData recvData);
    void HandleGrantLevel(WorldPacketData recvData);
    void HandleGroupAcceptOpcode(WorldPacketData recvData);
    void HandleGroupAssistantLeaderOpcode(WorldPacketData recvData);
    void HandleGroupChangeSubGroupOpcode(WorldPacketData recvData);
    void HandleGroupDeclineOpcode(WorldPacketData recvData);
    void HandleGroupDisbandOpcode(WorldPacketData recvData);
    void HandleGroupInviteOpcode(WorldPacketData recvData);
    void HandleGroupRaidConvertOpcode(WorldPacketData recvData);
    void HandleGroupSetLeaderOpcode(WorldPacketData recvData);
    void HandleGroupSwapSubGroupOpcode(WorldPacketData recvData);
    void HandleGroupUninviteGuidOpcode(WorldPacketData recvData);
    void HandleGroupUninviteOpcode(WorldPacketData recvData);
    void HandleGuildAcceptOpcode(WorldPacketData recvData);
    void HandleGuildAddRankOpcode(WorldPacketData recvData);
    void HandleGuildBankBuyTab(WorldPacketData recvData);
    void HandleGuildBankDepositMoney(WorldPacketData recvData);
    void HandleGuildBankerActivate(WorldPacketData recvData);
    void HandleGuildBankLogQuery(WorldPacketData recvData);
    void HandleGuildBankMoneyWithdrawn(WorldPacketData recvData);
    void HandleGuildBankQueryTab(WorldPacketData recvData);
    void HandleGuildBankSwapItems(WorldPacketData recvData);
    void HandleGuildBankUpdateTab(WorldPacketData recvData);
    void HandleGuildBankWithdrawMoney(WorldPacketData recvData);
    void HandleGuildChangeInfoTextOpcode(WorldPacketData recvData);
    void HandleGuildCreateOpcode(WorldPacketData recvData);
    void HandleGuildDeclineOpcode(WorldPacketData recvData);
    void HandleGuildDelRankOpcode(WorldPacketData recvData);
    void HandleGuildDemoteOpcode(WorldPacketData recvData);
    void HandleGuildDisbandOpcode(WorldPacketData recvData);
    void HandleGuildEventLogQueryOpcode(WorldPacketData recvData);
    void HandleGuildInfoOpcode(WorldPacketData recvData);
    void HandleGuildInviteOpcode(WorldPacketData recvData);
    void HandleGuildLeaderOpcode(WorldPacketData recvData);
    void HandleGuildLeaveOpcode(WorldPacketData recvData);
    void HandleGuildMOTDOpcode(WorldPacketData recvData);
    void HandleGuildPermissions(WorldPacketData recvData);
    void HandleGuildPromoteOpcode(WorldPacketData recvData);
    void HandleGuildQueryOpcode(WorldPacketData recvData);
    void HandleGuildRankOpcode(WorldPacketData recvData);
    void HandleGuildRemoveOpcode(WorldPacketData recvData);
    void HandleGuildRosterOpcode(WorldPacketData recvData);
    void HandleGuildSetOfficerNoteOpcode(WorldPacketData recvData);
    void HandleGuildSetPublicNoteOpcode(WorldPacketData recvData);
    void HandleHearthAndResurrect(WorldPacketData recvData);
    void HandleIgnoreTradeOpcode(WorldPacketData recvData);
    void HandleInitiateTradeOpcode(WorldPacketData recvData);
    void HandleInspectArenaTeamsOpcode(WorldPacketData recvData);
    void HandleInspectHonorStatsOpcode(WorldPacketData recvData);
    void HandleInspectOpcode(WorldPacketData recvData);
    void HandleInstanceLockResponse(WorldPacketData recvData);
    void HandleItemNameQueryOpcode(WorldPacketData recvData);
    void HandleItemQuerySingleOpcode(WorldPacketData recvData);
    void HandleItemRefund(WorldPacketData recvData);
    void HandleItemRefundInfoRequest(WorldPacketData recvData);
    void HandleItemTextQuery(WorldPacketData recvData);
    void HandleJoinChannel(WorldPacketData recvData);
    void HandleLearnPreviewTalents(WorldPacketData recvData);
    void HandleLearnPreviewTalentsPet(WorldPacketData recvData);
    void HandleLearnTalentOpcode(WorldPacketData recvData);
    void HandleLeaveChannel(WorldPacketData recvData);
    void HandleLfgGetStatus(WorldPacketData recvData);
    void HandleLfgJoinOpcode(WorldPacketData recvData);
    void HandleLfgLeaveOpcode(WorldPacketData recvData);
    void HandleLfgPartyLockInfoRequestOpcode(WorldPacketData recvData);
    void HandleLfgPlayerLockInfoRequestOpcode(WorldPacketData recvData);
    void HandleLfgProposalResultOpcode(WorldPacketData recvData);
    void HandleLfgSetBootVoteOpcode(WorldPacketData recvData);
    void HandleLfgSetCommentOpcode(WorldPacketData recvData);
    void HandleLfgSetRolesOpcode(WorldPacketData recvData);
    void HandleLfgTeleportOpcode(WorldPacketData recvData);
    void HandleLfrSearchJoinOpcode(WorldPacketData recvData);
    void HandleLfrSearchLeaveOpcode(WorldPacketData recvData);
    void HandleListInventoryOpcode(WorldPacketData recvData);
    void HandleListStabledPetsOpcode(WorldPacketData recvData);
    void HandleLootMasterGiveOpcode(WorldPacketData recvData);
    void HandleLootMethodOpcode(WorldPacketData recvData);
    void HandleLootMoneyOpcode(WorldPacketData recvData);
    void HandleLootOpcode(WorldPacketData recvData);
    void HandleLootReleaseOpcode(WorldPacketData recvData);
    void HandleLootRoll(WorldPacketData recvData);
    void HandleMailCreateTextItem(WorldPacketData recvData);
    void HandleMailDelete(WorldPacketData recvData);
    void HandleMailMarkAsRead(WorldPacketData recvData);
    void HandleMailReturnToSender(WorldPacketData recvData);
    void HandleMailTakeItem(WorldPacketData recvData);
    void HandleMailTakeMoney(WorldPacketData recvData);
    void HandleMessagechatOpcode(WorldPacketData recvData);
    void HandleMinimapPingOpcode(WorldPacketData recvData);
    void HandleMirrorImageDataRequest(WorldPacketData recvData);
    void HandleMountSpecialAnimOpcode(WorldPacketData recvData);
    void HandleMoveHoverAck(WorldPacketData recvData);
    void HandleMoveKnockBackAck(WorldPacketData recvData);
    void HandleMovementOpcodes(WorldPacketData recvData);
    void HandleMoveNotActiveMover(WorldPacketData recvData);
    void HandleMoveRootAck(WorldPacketData recvData);
    void HandleMoveSetCanFlyAckOpcode(WorldPacketData recvData);
    void HandleMoveSplineDoneOpcode(WorldPacketData recvData);
    void HandleMoveTeleportAck(WorldPacketData recvData);
    void HandleMoveTimeSkippedOpcode(WorldPacketData recvData);
    void HandleMoveUnRootAck(WorldPacketData recvData);
    void HandleMoveWaterWalkAck(WorldPacketData recvData);
    void HandleMoveWorldportAckOpcode(WorldPacketData recvData);
    void HandleNameQueryOpcode(WorldPacketData recvData);
    void HandleNextCinematicCamera(WorldPacketData recvData);
    void HandleNpcTextQueryOpcode(WorldPacketData recvData);
    void HandleOfferPetitionOpcode(WorldPacketData recvData);
    void HandleOpenItemOpcode(WorldPacketData recvData);
    void HandleOptOutOfLootOpcode(WorldPacketData recvData);
    void HandlePageTextQueryOpcode(WorldPacketData recvData);
    void HandlePartyAssignmentOpcode(WorldPacketData recvData);
    void HandlePetAbandon(WorldPacketData recvData);
    void HandlePetAction(WorldPacketData recvData);
    void HandlePetCancelAuraOpcode(WorldPacketData recvData);
    void HandlePetCastSpellOpcode(WorldPacketData recvData);
    void HandlePetitionBuyOpcode(WorldPacketData recvData);
    void HandlePetitionDeclineOpcode(WorldPacketData recvData);
    void HandlePetitionQueryOpcode(WorldPacketData recvData);
    void HandlePetitionRenameOpcode(WorldPacketData recvData);
    void HandlePetitionShowListOpcode(WorldPacketData recvData);
    void HandlePetitionShowSignOpcode(WorldPacketData recvData);
    void HandlePetitionSignOpcode(WorldPacketData recvData);
    void HandlePetLearnTalent(WorldPacketData recvData);
    void HandlePetNameQuery(WorldPacketData recvData);
    void HandlePetRename(WorldPacketData recvData);
    void HandlePetSetAction(WorldPacketData recvData);
    void HandlePetSpellAutocastOpcode(WorldPacketData recvData);
    void HandlePetStopAttack(WorldPacketData recvData);
    void HandlePlayedTime(WorldPacketData recvData);
    void HandlePushQuestToParty(WorldPacketData recvData);
    void HandlePVPLogDataOpcode(WorldPacketData recvData);
    void HandleQueryGuildBankTabText(WorldPacketData recvData);
    void HandleQueryInspectAchievements(WorldPacketData recvData);
    void HandleQueryNextMailTime(WorldPacketData recvData);
    void HandleQueryQuestsCompleted(WorldPacketData recvData);
    void HandleQueryTimeOpcode(WorldPacketData recvData);
    void HandleQuestConfirmAccept(WorldPacketData recvData);
    void HandleQuestgiverAcceptQuestOpcode(WorldPacketData recvData);
    void HandleQuestgiverCancel(WorldPacketData recvData);
    void HandleQuestgiverChooseRewardOpcode(WorldPacketData recvData);
    void HandleQuestgiverCompleteQuest(WorldPacketData recvData);
    void HandleQuestgiverHelloOpcode(WorldPacketData recvData);
    void HandleQuestgiverQueryQuestOpcode(WorldPacketData recvData);
    void HandleQuestgiverQuestAutoLaunch(WorldPacketData recvData);
    void HandleQuestgiverRequestRewardOpcode(WorldPacketData recvData);
    void HandleQuestgiverStatusMultipleQuery(WorldPacketData recvData);
    void HandleQuestgiverStatusQueryOpcode(WorldPacketData recvData);
    void HandleQuestLogRemoveQuest(WorldPacketData recvData);
    void HandleQuestLogSwapQuest(WorldPacketData recvData);
    void HandleQuestPOIQuery(WorldPacketData recvData);
    void HandleQuestPushResult(WorldPacketData recvData);
    void HandleQuestQueryOpcode(WorldPacketData recvData);
    void HandleRaidReadyCheckFinishedOpcode(WorldPacketData recvData);
    void HandleRaidReadyCheckOpcode(WorldPacketData recvData);
    void HandleRaidTargetUpdateOpcode(WorldPacketData recvData);
    void HandleRandomRollOpcode(WorldPacketData recvData);
    void HandleReadItem(WorldPacketData recvData);
    void HandleRemoveGlyph(WorldPacketData recvData);
    void HandleRepairItemOpcode(WorldPacketData recvData);
    void HandleReportLag(WorldPacketData recvData);
    void HandleReportPvPAFK(WorldPacketData recvData);
    void HandleRequestAccountData(WorldPacketData recvData);
    void HandleRequestPartyMemberStatsOpcode(WorldPacketData recvData);
    void HandleRequestPetInfo(WorldPacketData recvData);
    void HandleRequestRaidInfoOpcode(WorldPacketData recvData);
    void HandleRequestVehicleExit(WorldPacketData recvData);
    void HandleResetInstancesOpcode(WorldPacketData recvData);
    void HandleResurrectResponseOpcode(WorldPacketData recvData);
    void HandleSaveGuildEmblemOpcode(WorldPacketData recvData);
    void HandleSelfResOpcode(WorldPacketData recvData);
    void HandleSellItemOpcode(WorldPacketData recvData);
    void HandleSendMail(WorldPacketData recvData);
    void HandleSetActionBarToggles(WorldPacketData recvData);
    void HandleSetActionButtonOpcode(WorldPacketData recvData);
    void HandleSetActiveMoverOpcode(WorldPacketData recvData);
    void HandleSetActiveVoiceChannel(WorldPacketData recvData);
    void HandleSetAmmoOpcode(WorldPacketData recvData);
    void HandleSetChannelWatch(WorldPacketData recvData);
    void HandleSetContactNotesOpcode(WorldPacketData recvData);
    void HandleSetDungeonDifficultyOpcode(WorldPacketData recvData);
    void HandleSetFactionAtWar(WorldPacketData recvData);
    void HandleSetFactionCheat(WorldPacketData recvData);
    void HandleSetFactionInactiveOpcode(WorldPacketData recvData);
    void HandleSetGuildBankTabText(WorldPacketData recvData);
    void HandleSetPlayerDeclinedNames(WorldPacketData recvData);
    void HandleSetRaidDifficultyOpcode(WorldPacketData recvData);
    void HandleSetSavedInstanceExtend(WorldPacketData recvData);
    void HandleSetSheathedOpcode(WorldPacketData recvData);
    void HandleSetTaxiBenchmarkOpcode(WorldPacketData recvData);
    void HandleSetTitleOpcode(WorldPacketData recvData);
    void HandleSetTradeGoldOpcode(WorldPacketData recvData);
    void HandleSetTradeItemOpcode(WorldPacketData recvData);
    void HandleSetWatchedFactionOpcode(WorldPacketData recvData);
    void HandleShowingCloakOpcode(WorldPacketData recvData);
    void HandleShowingHelmOpcode(WorldPacketData recvData);
    void HandleSocketOpcode(WorldPacketData recvData);
    void HandleSpellClick(WorldPacketData recvData);
    void HandleSpiritHealerActivateOpcode(WorldPacketData recvData);
    void HandleSplitItemOpcode(WorldPacketData recvData);
    void HandleStablePet(WorldPacketData recvData);
    void HandleStableRevivePet(WorldPacketData recvData);
    void HandleStableSwapPet(WorldPacketData recvData);
    void HandleSummonResponseOpcode(WorldPacketData recvData);
    void HandleSwapInvItemOpcode(WorldPacketData recvData);
    void HandleSwapItem(WorldPacketData recvData);
    void HandleTabardVendorActivateOpcode(WorldPacketData recvData);
    void HandleTalentWipeConfirmOpcode(WorldPacketData recvData);
    void HandleTaxiNodeStatusQueryOpcode(WorldPacketData recvData);
    void HandleTaxiQueryAvailableNodes(WorldPacketData recvData);
    void HandleTextEmoteOpcode(WorldPacketData recvData);
    void HandleTimeSyncResp(WorldPacketData recvData);
    void HandleTotemDestroyed(WorldPacketData recvData);
    void HandleTrainerBuySpellOpcode(WorldPacketData recvData);
    void HandleTrainerListOpcode(WorldPacketData recvData);
    void HandleTurnInPetitionOpcode(WorldPacketData recvData);
    void HandleTutorialClear(WorldPacketData recvData);
    void HandleTutorialFlag(WorldPacketData recvData);
    void HandleTutorialReset(WorldPacketData recvData);
    void HandleUnacceptTradeOpcode(WorldPacketData recvData);
    void HandleUnlearnSkillOpcode(WorldPacketData recvData);
    void HandleUnstablePet(WorldPacketData recvData);
    void HandleUpdateAccountData(WorldPacketData recvData);
    void HandleUpdateMissileTrajectory(WorldPacketData recvData);
    void HandleUpdateProjectilePosition(WorldPacketData recvData);
    void HandleUseItemOpcode(WorldPacketData recvData);
    void HandleVoiceSessionEnableOpcode(WorldPacketData recvData);
    void HandleWardenDataOpcode(WorldPacketData recvData);
    void HandleWhoisOpcode(WorldPacketData recvData);
    void HandleWorldStateUITimerUpdate(WorldPacketData recvData);
    void HandleWorldTeleportOpcode(WorldPacketData recvData);
    void HandleWrapItemOpcode(WorldPacketData recvData);
    void Handle_EarlyProccess(WorldPacketData recvData);
    void Handle_NULL(WorldPacketData recvData);
    void Handle_ServerSide(WorldPacketData recvData);
}

public partial class WorldSession : IOpcodeHandler
{
    public void HandleAcceptGrantLevel(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAcceptTradeOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleActivateTaxiExpressOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleActivateTaxiOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAddFriendOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAddIgnoreOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAlterAppearance(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAreaSpiritHealerQueryOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAreaSpiritHealerQueueOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAreaTriggerOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleArenaTeamAcceptOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleArenaTeamDeclineOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleArenaTeamDisbandOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleArenaTeamInviteOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleArenaTeamLeaderOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleArenaTeamLeaveOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleArenaTeamQueryOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleArenaTeamRemoveOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleArenaTeamRosterOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAttackStopOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAttackSwingOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAuctionHelloOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAuctionListBidderItems(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAuctionListItems(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAuctionListOwnerItems(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAuctionListPendingSales(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAuctionPlaceBid(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAuctionRemoveItem(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAuctionSellItem(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAutoBankItemOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAutoEquipItemOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAutoEquipItemSlotOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAutoStoreBagItemOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAutoStoreBankItemOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleAutostoreLootItemOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleBankerActivateOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleBattlefieldLeaveOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleBattlefieldListOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleBattleFieldPortOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleBattlefieldStatusOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleBattlegroundPlayerPositionsOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleBattlemasterHelloOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleBattlemasterJoinArena(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleBattlemasterJoinOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleBeginTradeOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleBfEntryInviteResponse(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleBfExitRequest(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleBfQueueInviteResponse(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleBinderActivateOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleBusyTradeOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleBuybackItem(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleBuyBankSlotOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleBuyItemInSlotOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleBuyItemOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleBuyStableSlot(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCalendarAddEvent(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCalendarArenaTeam(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCalendarComplain(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCalendarCopyEvent(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCalendarEventInvite(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCalendarEventModeratorStatus(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCalendarEventRemoveInvite(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCalendarEventRsvp(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCalendarEventSignup(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCalendarEventStatus(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCalendarGetCalendar(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCalendarGetEvent(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCalendarGetNumPending(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCalendarGuildFilter(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCalendarRemoveEvent(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCalendarUpdateEvent(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCancelAuraOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCancelAutoRepeatSpellOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCancelCastOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCancelChanneling(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCancelGrowthAuraOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCancelMountAuraOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCancelTempEnchantmentOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCancelTradeOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCastSpellOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleChangeSeatsOnControlledVehicle(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleChannelAnnouncements(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleChannelBan(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleChannelDeclineInvite(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleChannelDisplayListQuery(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleChannelInvite(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleChannelKick(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleChannelList(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleChannelModerateOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleChannelModerator(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleChannelMute(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleChannelOwner(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleChannelPassword(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleChannelSetOwner(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleChannelUnban(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleChannelUnmoderator(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleChannelUnmute(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleChannelVoiceOnOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleChatIgnoredOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleClearChannelWatch(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleClearTradeItemOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleComplainOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCompleteCinematic(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleContactListOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCorpseMapPositionQuery(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCorpseQueryOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleCreatureQueryOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleDelFriendOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleDelIgnoreOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleDestroyItemOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleDismissControlledVehicle(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleDismissCritter(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleDuelAcceptedOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleDuelCancelledOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleEjectPassenger(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleEmoteOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleEnterPlayerVehicle(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleEquipmentSetDelete(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleEquipmentSetSave(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleEquipmentSetUse(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleFarSightOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleFeatherFallAck(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleForceSpeedChangeAck(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGameObjectQueryOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGameobjectReportUse(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGameObjectUseOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGetChannelMemberCount(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGetMailList(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGMResponseResolve(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGMSurveySubmit(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGMTicketCreateOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGMTicketDeleteOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGMTicketGetTicketOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGMTicketSystemStatusOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGMTicketUpdateOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGossipHelloOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGrantLevel(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGroupAcceptOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGroupAssistantLeaderOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGroupChangeSubGroupOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGroupDeclineOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGroupDisbandOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGroupInviteOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGroupRaidConvertOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGroupSetLeaderOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGroupSwapSubGroupOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGroupUninviteGuidOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGroupUninviteOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildAcceptOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildAddRankOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildBankBuyTab(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildBankDepositMoney(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildBankerActivate(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildBankLogQuery(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildBankMoneyWithdrawn(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildBankQueryTab(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildBankSwapItems(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildBankUpdateTab(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildBankWithdrawMoney(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildChangeInfoTextOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildCreateOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildDeclineOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildDelRankOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildDemoteOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildDisbandOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildEventLogQueryOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildInfoOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildInviteOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildLeaderOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildLeaveOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildMOTDOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildPermissions(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildPromoteOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildQueryOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildRankOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildRemoveOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildRosterOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildSetOfficerNoteOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleGuildSetPublicNoteOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleHearthAndResurrect(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleIgnoreTradeOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleInitiateTradeOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleInspectArenaTeamsOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleInspectHonorStatsOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleInspectOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleInstanceLockResponse(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleItemNameQueryOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleItemQuerySingleOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleItemRefund(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleItemRefundInfoRequest(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleItemTextQuery(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleJoinChannel(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLearnPreviewTalents(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLearnPreviewTalentsPet(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLearnTalentOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLeaveChannel(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLfgGetStatus(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLfgJoinOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLfgLeaveOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLfgPartyLockInfoRequestOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLfgPlayerLockInfoRequestOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLfgProposalResultOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLfgSetBootVoteOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLfgSetCommentOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLfgSetRolesOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLfgTeleportOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLfrSearchJoinOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLfrSearchLeaveOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleListInventoryOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleListStabledPetsOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLogoutRequestOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLootMasterGiveOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLootMethodOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLootMoneyOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLootOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLootReleaseOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleLootRoll(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMailCreateTextItem(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMailDelete(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMailMarkAsRead(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMailReturnToSender(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMailTakeItem(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMailTakeMoney(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMessagechatOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMinimapPingOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMirrorImageDataRequest(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMountSpecialAnimOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMoveHoverAck(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMoveKnockBackAck(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMovementOpcodes(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMoveNotActiveMover(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMoveRootAck(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMoveSetCanFlyAckOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMoveSplineDoneOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMoveTeleportAck(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMoveTimeSkippedOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMoveUnRootAck(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMoveWaterWalkAck(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMoveWorldportAckOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleMoveWorldportAck()
    {
        // TODO: game: WorldSession::HandleMoveWorldportAck()
    }

    public void HandleNameQueryOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleNextCinematicCamera(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleNpcTextQueryOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleOfferPetitionOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleOpenItemOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleOptOutOfLootOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePageTextQueryOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePartyAssignmentOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePetAbandon(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePetAction(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePetCancelAuraOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePetCastSpellOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePetitionBuyOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePetitionDeclineOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePetitionQueryOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePetitionRenameOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePetitionShowListOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePetitionShowSignOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePetitionSignOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePetLearnTalent(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePetNameQuery(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePetRename(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePetSetAction(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePetSpellAutocastOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePetStopAttack(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePlayedTime(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePushQuestToParty(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandlePVPLogDataOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleQueryGuildBankTabText(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleQueryInspectAchievements(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleQueryNextMailTime(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleQueryQuestsCompleted(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleQueryTimeOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleQuestConfirmAccept(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleQuestgiverAcceptQuestOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleQuestgiverCancel(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleQuestgiverChooseRewardOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleQuestgiverCompleteQuest(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleQuestgiverHelloOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleQuestgiverQueryQuestOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleQuestgiverQuestAutoLaunch(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleQuestgiverRequestRewardOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleQuestgiverStatusMultipleQuery(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleQuestgiverStatusQueryOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleQuestLogRemoveQuest(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleQuestLogSwapQuest(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleQuestPOIQuery(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleQuestPushResult(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleQuestQueryOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleRaidReadyCheckFinishedOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleRaidReadyCheckOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleRaidTargetUpdateOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleRandomRollOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleReadItem(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleRemoveGlyph(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleRepairItemOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleReportLag(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleReportPvPAFK(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleRequestAccountData(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleRequestPartyMemberStatsOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleRequestPetInfo(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleRequestRaidInfoOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleRequestVehicleExit(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleResetInstancesOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleResurrectResponseOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSaveGuildEmblemOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSelfResOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSellItemOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSendMail(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSetActionBarToggles(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSetActionButtonOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSetActiveMoverOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSetActiveVoiceChannel(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSetAmmoOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSetChannelWatch(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSetContactNotesOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSetDungeonDifficultyOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSetFactionAtWar(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSetFactionCheat(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSetFactionInactiveOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSetGuildBankTabText(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSetPlayerDeclinedNames(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSetRaidDifficultyOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSetSavedInstanceExtend(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSetSheathedOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSetTaxiBenchmarkOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSetTitleOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSetTradeGoldOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSetTradeItemOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSetWatchedFactionOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleShowingCloakOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleShowingHelmOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSocketOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSpellClick(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSpiritHealerActivateOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSplitItemOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleStablePet(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleStableRevivePet(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleStableSwapPet(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSummonResponseOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSwapInvItemOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleSwapItem(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleTabardVendorActivateOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleTalentWipeConfirmOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleTaxiNodeStatusQueryOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleTaxiQueryAvailableNodes(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleTextEmoteOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleTimeSyncResp(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleTotemDestroyed(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleTrainerBuySpellOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleTrainerListOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleTurnInPetitionOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleTutorialClear(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleTutorialFlag(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleTutorialReset(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleUnacceptTradeOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleUnlearnSkillOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleUnstablePet(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleUpdateAccountData(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleUpdateMissileTrajectory(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleUpdateProjectilePosition(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleUseItemOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleVoiceSessionEnableOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleWardenDataOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleWhoisOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleWorldStateUITimerUpdate(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleWorldTeleportOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void HandleWrapItemOpcode(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void Handle_EarlyProccess(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void Handle_NULL(WorldPacketData recvData)
    {
        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }

    public void Handle_ServerSide(WorldPacketData recvData)
    {
        logger.Debug(Logging.LogFilter.Network, "WORLD: CMSG_PLAYER_LOGIN");

        // TODO: game: WorldSession::Handle_XXXXXX(WorldPacketData recvData)
    }
}
