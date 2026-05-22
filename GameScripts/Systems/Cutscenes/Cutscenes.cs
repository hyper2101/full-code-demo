using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Cutscenes
{
	public static string Title
	{
		get
		{
			return WorldManager.instance.CutsceneTitle;
		}
		set
		{
			WorldManager.instance.CutsceneTitle = value;
		}
	}

	public static string Text
	{
		get
		{
			return WorldManager.instance.CutsceneText;
		}
		set
		{
			WorldManager.instance.CutsceneText = value;
		}
	}

	private static QueuedAnimation currentAnimation
	{
		set
		{
			WorldManager.instance.currentAnimation = value;
		}
	}

	private static void Stop(bool keepCameraPosition = false)
	{
		Cutscenes.Text = "";
		Cutscenes.Title = "";
		GameCamera.instance.TargetPositionOverride = null;
		GameCamera.instance.CameraPositionDistanceOverride = null;
		GameCamera.instance.TargetCardOverride = null;
		CutsceneScreen.instance.IsAdvisorCutscene = false;
		CutsceneScreen.instance.IsEndOfMonthCutscene = false;
		CutsceneScreen.instance.CheckAdvisorCutscene();
		if (keepCameraPosition)
		{
			GameCamera.instance.KeepCameraAtCurrentPos();
		}
		GameCanvas.instance.SetScreen<GameScreen>();
		Cutscenes.currentAnimation = null;
	}

	public static IEnumerator SpawnTentacles()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		Cutscenes.Title = "";
		Cutscenes.Text = SokLoc.Translate("label_awakened_something");
		List<Tentacle> tentacles = new List<Tentacle>();
		int num;
		for (int i = 0; i < 5; i = num + 1)
		{
			Tentacle tentacle = WorldManager.instance.CreateCard(WorldManager.instance.GetRandomSpawnPosition(), "tentacle", true, false, true) as Tentacle;
			GameCamera.instance.TargetPositionOverride = new Vector3?(tentacle.transform.position);
			tentacles.Add(tentacle);
			yield return new WaitForSeconds(1f);
			num = i;
		}
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_uh_oh"));
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator SpawnKraken()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		Cutscenes.Title = "";
		Cutscenes.Text = SokLoc.Translate("label_awakened_the_kraken");
		CardData cardData = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "kraken", false, false, true);
		GameCamera.instance.TargetPositionOverride = new Vector3?(cardData.transform.position);
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_uh_oh"));
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator JustUnlockedPack(BoosterpackData justUnlockedPack)
	{
		BuyBoosterBox sellbox = Object.FindObjectsOfType<BuyBoosterBox>().FirstOrDefault<BuyBoosterBox>((BuyBoosterBox x) => x.BoosterId == justUnlockedPack.BoosterId);
		if (sellbox == null)
		{
			Debug.LogError("No sellbox found for booster pack " + justUnlockedPack.BoosterId);
			yield break;
		}
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		if (justUnlockedPack != null)
		{
			Cutscenes.Title = SokLoc.Translate("label_new_pack_unlocked");
			Cutscenes.Text = SokLoc.Translate("label_pack_now_available", new LocParam[] { LocParam.Create("pack", justUnlockedPack.Name) });
			yield return new WaitForSeconds(0.5f);
			GameCamera.instance.TargetPositionOverride = new Vector3?(sellbox.transform.position);
			yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_continue"));
			if (justUnlockedPack.BoosterId == "structures")
			{
				QuestManager.instance.SpecialActionComplete("unlocked_all_packs", null);
			}
			if (justUnlockedPack.BoosterId == "island_locations")
			{
				QuestManager.instance.SpecialActionComplete("unlocked_all_island_packs", null);
			}
			if (justUnlockedPack.BoosterId == "cities_ideas_2")
			{
				QuestManager.instance.SpecialActionComplete("unlocked_all_cities_packs", null);
			}
			QuestManager.instance.SpecialActionComplete("unlocked_" + justUnlockedPack.BoosterId + "_pack", null);
			if (justUnlockedPack.BoosterId == "basic")
			{
				GameCamera.instance.CenterOnBoard(WorldManager.instance.GetBoardWithId("main"));
			}
		}
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator EveryoneInForestDead()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		Cutscenes.Title = SokLoc.Translate("label_everyone_in_forest_dead");
		Cutscenes.Text = SokLoc.Translate("label_everyone_in_forest_dead_text");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_return_to_mainland"));
		WorldManager.instance.GoToBoard(WorldManager.instance.GetBoardWithId("main"), delegate
		{
			Cutscenes.RemoveEnemiesFromBoard("forest");
			Cutscenes.Stop(false);
			WorldManager.instance.currentAnimationRoutine = null;
		}, "default");
		yield break;
	}

	public static IEnumerator EveryoneOnIslandDead()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		Cutscenes.Title = SokLoc.Translate("label_everyone_on_island_dead");
		Cutscenes.Text = SokLoc.Translate("label_everyone_on_island_dead_text");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_return_to_mainland"));
		WorldManager.instance.GoToBoard(WorldManager.instance.GetBoardWithId("main"), delegate
		{
			Cutscenes.RemoveEnemiesFromBoard("island");
			Cutscenes.Stop(false);
			WorldManager.instance.currentAnimationRoutine = null;
		}, "default");
		yield break;
	}

	public static IEnumerator EveryoneInSpiritWorldDead(string boardId)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		Cutscenes.Title = SokLoc.Translate("label_everyone_spirit_world_dead");
		Cutscenes.Text = SokLoc.Translate("label_everyone_spirit_world_dead_text");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Cutscenes.Title = SokLoc.Translate("label_everyone_spirit_world_dead");
		Cutscenes.Text = SokLoc.Translate("label_everyone_spirit_world_dead_text_retry");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		GameBoard currentBoard = WorldManager.instance.GetBoardWithId(boardId);
		GameBoard boardWithId = WorldManager.instance.GetBoardWithId(WorldManager.instance.CurrentRunVariables.PreviouseBoard);
		WorldManager.instance.GoToBoard(boardWithId, delegate
		{
			WorldManager.instance.RemoveAllCardsFromBoard(boardId, true);
			WorldManager.instance.ResetBoughtBoostersOnLocation(currentBoard.Location);
			if (currentBoard.Id == "greed")
			{
				DemandManager.instance.ResetDemands();
				WorldManager.instance.BoardMonths.GreedMonth = 1;
			}
			if (currentBoard.Id == "happiness")
			{
				WorldManager.instance.BoardMonths.HappinessMonth = 1;
				WorldManager.instance.CurrentRunVariables.VillagersUnhappyMonthCount = 0;
				WorldManager.instance.CurrentRunVariables.VillagersHappyMonthCount = 0;
			}
			if (currentBoard.Id == "death")
			{
				WorldManager.instance.BoardMonths.DeathMonth = 1;
			}
			Cutscenes.Stop(false);
			WorldManager.instance.currentAnimationRoutine = null;
		}, "spirit");
		yield break;
	}

	private static void RemoveEnemiesFromBoard(string board)
	{
		List<GameCard> list = new List<GameCard>();
		foreach (GameCard gameCard in WorldManager.instance.AllCards)
		{
			if (gameCard.MyBoard.Id == board)
			{
				if (!(gameCard.CardData is Enemy))
				{
					Mob mob = gameCard.CardData as Mob;
					if (mob == null || !mob.IsAggressive)
					{
						continue;
					}
				}
				list.Add(gameCard);
			}
		}
		foreach (GameCard gameCard2 in list)
		{
			gameCard2.DestroyCard(false, true);
		}
	}

	public static IEnumerator MommaCrab(CardData mommaCrab)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.TargetPositionOverride = new Vector3?(mommaCrab.transform.position);
		Cutscenes.Title = SokLoc.Translate("label_momma_crab_appeared_title");
		Cutscenes.Text = SokLoc.Translate("label_momma_crab_appeared_text");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_uh_oh"));
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator IslandIntroPack()
	{
		Boosterpack boosterpack = WorldManager.instance.CreateBoosterpack(WorldManager.instance.GetRandomSpawnPosition(), "island_intro");
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.TargetPositionOverride = new Vector3?(boosterpack.transform.position);
		Cutscenes.Title = SokLoc.Translate("label_island_intro_pack_title");
		Cutscenes.Text = SokLoc.Translate("label_island_intro_pack_text");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_nice"));
		WorldManager.instance.CurrentSave.GotIslandIntroPack = true;
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator IslandIntro()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.CenterOnBoard(WorldManager.instance.GetBoardWithId("island"));
		Cutscenes.Title = "";
		SokLoc.Translate("label_island_intro_title");
		Cutscenes.Text = SokLoc.Translate("label_island_intro_1");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Cutscenes.Text = SokLoc.Translate("label_island_intro_2");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		CardData card = WorldManager.instance.GetCard("rowboat");
		if (card != null)
		{
			GameCamera.instance.TargetPositionOverride = new Vector3?(card.transform.position);
		}
		Cutscenes.Text = SokLoc.Translate("label_island_intro_3");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Cutscenes.Text = SokLoc.Translate("label_island_intro_4");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		GameCamera.instance.TargetPositionOverride = new Vector3?(WorldManager.instance.CardQuery.GetCard<BaseVillager>().transform.position);
		Cutscenes.Text = SokLoc.Translate("label_island_intro_5");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		GameCamera.instance.TargetPositionOverride = null;
		GameCamera.instance.CenterOnBoard(WorldManager.instance.GetBoardWithId("island"));
		Cutscenes.Text = SokLoc.Translate("label_island_intro_6");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator ForestIntro()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.CenterOnBoard(WorldManager.instance.GetBoardWithId("forest"));
		Cutscenes.Title = SokLoc.Translate("label_forest_intro_title");
		Cutscenes.Text = SokLoc.Translate("label_forest_intro_1");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Cutscenes.Text = SokLoc.Translate("label_forest_intro_2");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		WickedWitch witch = Cutscenes.FindOrCreateWitch();
		witch.IsOldLady = true;
		WorldManager.instance.CreateSmoke(witch.MyGameCard.transform.position);
		GameCamera.instance.TargetPositionOverride = new Vector3?(witch.transform.position);
		Cutscenes.Text = SokLoc.Translate("label_forest_intro_3");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Cutscenes.Text = SokLoc.Translate("label_forest_intro_4");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		AudioManager.me.PlaySound2D(ForestCombatManager.instance.WitchSounds, Random.Range(1.1f, 1.3f), 0.5f);
		WorldManager.instance.CreateSmoke(witch.MyGameCard.transform.position);
		GameCamera.instance.Screenshake = 0.3f;
		witch.IsOldLady = false;
		Cutscenes.Text = SokLoc.Translate("label_forest_intro_5");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Cutscenes.Text = SokLoc.Translate("label_forest_intro_6");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		WorldManager.instance.CreateSmoke(witch.MyGameCard.transform.position);
		witch.MyGameCard.DestroyCard(false, true);
		AudioManager.me.PlaySound2D(ForestCombatManager.instance.WitchSounds, Random.Range(1.1f, 1.3f), 0.5f);
		GameCamera.instance.TargetPositionOverride = null;
		GameCamera.instance.CenterOnBoard(WorldManager.instance.GetBoardWithId("forest"));
		ForestCombatManager.instance.PrepareWave();
		Cutscenes.Text = SokLoc.Translate("label_forest_fight");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Cutscenes.Stop(false);
		ForestCombatManager.instance.StartWave();
		yield break;
	}

	public static IEnumerator GreedIntro()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		Cutscenes.Title = "";
		Spirit spirit = Cutscenes.FindOrCreateSpirit(CurseType.Greed, null);
		GameCamera.instance.TargetCardOverride = spirit;
		Cutscenes.Text = SokLoc.Translate("label_greed_intro_1");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Royal royal = Cutscenes.FindOrCreateGameCard("royal", WorldManager.instance.MiddleOfBoard()).CardData as Royal;
		GameCamera.instance.TargetCardOverride = royal;
		Cutscenes.Text = SokLoc.Translate("label_greed_intro_2");
		AudioManager.me.PlaySound2D(DemandManager.instance.StartDemandSound, 1f, 0.4f);
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Cutscenes.Text = SokLoc.Translate("label_greed_intro_3");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Cutscenes.Text = SokLoc.Translate("label_greed_intro_4");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		GameCamera.instance.TargetCardOverride = spirit;
		Cutscenes.Text = SokLoc.Translate("label_greed_intro_5");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		GameCamera.instance.CenterOnBoard(WorldManager.instance.GetBoardWithId("greed"));
		spirit.MyGameCard.DestroyCard(false, true);
		yield return new WaitForSeconds(0.5f);
		WorldManager.instance.CurrentRunVariables.VisitedGreed = true;
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator HappinessIntro()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.CenterOnBoard(WorldManager.instance.GetBoardWithId("happiness"));
		Cutscenes.Title = "";
		Spirit spirit = Cutscenes.FindOrCreateSpirit(CurseType.Happiness, null);
		GameCamera.instance.TargetCardOverride = spirit;
		Cutscenes.Text = SokLoc.Translate("label_happiness_intro_1");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Happiness card = WorldManager.instance.CardQuery.GetCard<Happiness>();
		if (card != null)
		{
			GameCamera.instance.TargetPositionOverride = new Vector3?(card.transform.position);
		}
		Cutscenes.Text = SokLoc.Translate("label_happiness_intro_2");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Cutscenes.Text = SokLoc.Translate("label_happiness_intro_3");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		GameCamera.instance.TargetCardOverride = WorldManager.instance.CardQuery.GetCard<BaseVillager>();
		Cutscenes.Text = SokLoc.Translate("label_happiness_intro_4");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		GameCamera.instance.TargetCardOverride = spirit;
		Cutscenes.Text = SokLoc.Translate("label_happiness_intro_5");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		spirit.MyGameCard.DestroyCard(false, true);
		yield return new WaitForSeconds(0.5f);
		WorldManager.instance.CurrentRunVariables.VisitedHappiness = true;
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator DeathIntro()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.CenterOnBoard(WorldManager.instance.GetBoardWithId("death"));
		Cutscenes.Title = "";
		SokLoc.Translate("label_death_intro_title");
		Spirit spirit = Cutscenes.FindOrCreateSpirit(CurseType.Death, null);
		GameCamera.instance.TargetCardOverride = spirit;
		Cutscenes.Text = SokLoc.Translate("label_death_intro_1");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Cutscenes.Text = SokLoc.Translate("label_death_intro_2");
		GameCamera.instance.TargetCardOverride = WorldManager.instance.CardQuery.GetCard<BaseVillager>();
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Cutscenes.Text = SokLoc.Translate("label_death_intro_3");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Cutscenes.Text = SokLoc.Translate("label_death_intro_4");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		GameCamera.instance.TargetCardOverride = spirit;
		Cutscenes.Text = SokLoc.Translate("label_death_intro_5");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		spirit.MyGameCard.DestroyCard(false, true);
		yield return new WaitForSeconds(0.5f);
		WorldManager.instance.CurrentRunVariables.VisitedDeath = true;
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator ForestResumeIntro()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.CenterOnBoard(WorldManager.instance.GetBoardWithId("forest"));
		Cutscenes.Title = SokLoc.Translate("label_forest_resume_title");
		if (WorldManager.instance.CurrentRunVariables.ForestWave <= ForestCombatManager.instance.WickedWitchWave)
		{
			AudioManager.me.PlaySound2D(ForestCombatManager.instance.WitchSounds, Random.Range(1.1f, 1.3f), 0.5f);
			WickedWitch witch = Cutscenes.FindOrCreateWitch();
			WorldManager.instance.CreateSmoke(witch.MyGameCard.transform.position);
			Cutscenes.FocusCameraOnWitchAndVillagers();
			Cutscenes.Text = SokLoc.Translate("label_forest_resume");
			yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
			ForestCombatManager.instance.PrepareWave();
			Cutscenes.Text = SokLoc.Translate("label_forest_fight");
			yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
			WorldManager.instance.CreateSmoke(witch.MyGameCard.transform.position);
			witch.MyGameCard.DestroyCard(false, true);
			AudioManager.me.PlaySound2D(ForestCombatManager.instance.WitchSounds, Random.Range(1.1f, 1.3f), 0.5f);
			witch = null;
		}
		else
		{
			ForestCombatManager.instance.PrepareWave();
			Cutscenes.Text = SokLoc.Translate("label_forest_fight");
			yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		}
		GameCamera.instance.TargetPositionOverride = null;
		ForestCombatManager.instance.StartWave();
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator ForestWaveEnd()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.CenterOnBoard(WorldManager.instance.GetBoardWithId("forest"));
		Cutscenes.Title = SokLoc.Translate("label_forest_wave_title");
		AudioManager.me.PlaySound2D(ForestCombatManager.instance.WitchSounds, Random.Range(1.1f, 1.3f), 0.5f);
		WickedWitch witch = Cutscenes.FindOrCreateWitch();
		WorldManager.instance.CreateSmoke(witch.MyGameCard.transform.position);
		Cutscenes.FocusCameraOnWitchAndVillagers();
		Cutscenes.Text = SokLoc.Translate("label_forest_wave_end");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Cutscenes.Text = SokLoc.Translate("label_forest_wave_end2");
		yield return Cutscenes.WaitForAnswer(new string[]
		{
			SokLoc.Translate("label_forest_wave_end_next_wave", new LocParam[] { LocParam.Create("wave", WorldManager.instance.CurrentRunVariables.ForestWave.ToString()) }),
			SokLoc.Translate("label_forest_wave_end_leave")
		});
		if (WorldManager.instance.ContinueButtonIndex == 0)
		{
			ForestCombatManager.instance.PrepareWave();
			Cutscenes.Text = SokLoc.Translate("label_forest_fight");
			yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
			WorldManager.instance.CreateSmoke(witch.MyGameCard.transform.position);
			witch.MyGameCard.DestroyCard(false, true);
			AudioManager.me.PlaySound2D(ForestCombatManager.instance.WitchSounds, Random.Range(1.1f, 1.3f), 0.5f);
			GameCamera.instance.TargetPositionOverride = null;
			ForestCombatManager.instance.StartWave();
		}
		else
		{
			Cutscenes.Text = SokLoc.Translate("label_forest_leave");
			yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
			WorldManager.instance.CreateSmoke(witch.MyGameCard.transform.position);
			witch.MyGameCard.DestroyCard(false, true);
			AudioManager.me.PlaySound2D(ForestCombatManager.instance.WitchSounds, Random.Range(1.1f, 1.3f), 0.5f);
			GameCamera.instance.TargetPositionOverride = null;
			ForestCombatManager.instance.LeaveForest();
		}
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator ForestLastWaveEnd()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.CenterOnBoard(WorldManager.instance.GetBoardWithId("forest"));
		Cutscenes.Title = SokLoc.Translate("label_forest_wave_title");
		AudioManager.me.PlaySound2D(ForestCombatManager.instance.WitchSounds, Random.Range(1.1f, 1.3f), 0.5f);
		WickedWitch witch = Cutscenes.FindOrCreateWitch();
		WorldManager.instance.CreateSmoke(witch.MyGameCard.transform.position);
		Cutscenes.FocusCameraOnWitchAndVillagers();
		Cutscenes.Text = SokLoc.Translate("label_forest_wave_10_end");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		GameCamera.instance.TargetPositionOverride = new Vector3?(WorldManager.instance.CardQuery.GetCard<BaseVillager>().transform.position);
		Cutscenes.Text = SokLoc.Translate("label_forest_wave_10_end2");
		yield return Cutscenes.WaitForAnswer(new string[]
		{
			SokLoc.Translate("label_forest_wave_end_wave_10"),
			SokLoc.Translate("label_forest_wave_end_leave")
		});
		if (WorldManager.instance.ContinueButtonIndex == 0)
		{
			WorldManager.instance.CreateSmoke(witch.MyGameCard.transform.position);
			witch.MyGameCard.DestroyCard(false, true);
			ForestCombatManager.instance.PrepareWave();
			Cutscenes.Text = SokLoc.Translate("label_forest_fight_wave_10");
			yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
			AudioManager.me.PlaySound2D(ForestCombatManager.instance.WitchSounds, Random.Range(1.1f, 1.3f), 0.5f);
			GameCamera.instance.TargetPositionOverride = null;
			ForestCombatManager.instance.StartWave();
		}
		else
		{
			Cutscenes.Text = SokLoc.Translate("label_forest_leave");
			yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
			WorldManager.instance.CreateSmoke(witch.MyGameCard.transform.position);
			witch.MyGameCard.DestroyCard(false, true);
			AudioManager.me.PlaySound2D(ForestCombatManager.instance.WitchSounds, Random.Range(1.1f, 1.3f), 0.5f);
			GameCamera.instance.TargetPositionOverride = null;
			ForestCombatManager.instance.LeaveForest();
		}
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator ForestEndlessWaveEnd()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.CenterOnBoard(WorldManager.instance.GetBoardWithId("forest"));
		Cutscenes.Title = SokLoc.Translate("label_forest_wave_title");
		Cutscenes.Text = SokLoc.Translate("label_forest_wave_endless_end");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		if (WorldManager.instance.CurrentRunVariables.ForestWave == ForestCombatManager.instance.WickedWitchWave + 1)
		{
			Cutscenes.Text = SokLoc.Translate("label_forest_wave_endless_end2");
			yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		}
		GameCamera.instance.TargetPositionOverride = new Vector3?(WorldManager.instance.CardQuery.GetCard<BaseVillager>().transform.position);
		Cutscenes.Text = SokLoc.Translate("label_forest_wave_endless_end3");
		yield return Cutscenes.WaitForAnswer(new string[]
		{
			SokLoc.Translate("label_forest_wave_end_next_wave", new LocParam[] { LocParam.Create("wave", WorldManager.instance.CurrentRunVariables.ForestWave.ToString()) }),
			SokLoc.Translate("label_forest_wave_end_leave")
		});
		if (WorldManager.instance.ContinueButtonIndex == 0)
		{
			ForestCombatManager.instance.PrepareWave();
			Cutscenes.Text = SokLoc.Translate("label_forest_fight");
			yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
			GameCamera.instance.TargetPositionOverride = null;
			ForestCombatManager.instance.StartWave();
		}
		else
		{
			Cutscenes.Text = SokLoc.Translate("label_forest_leave");
			yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
			GameCamera.instance.TargetPositionOverride = null;
			ForestCombatManager.instance.LeaveForest();
		}
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator ForestWaveLost()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.CenterOnBoard(WorldManager.instance.GetBoardWithId("forest"));
		Cutscenes.Title = SokLoc.Translate("label_forest_lost_title");
		if (WorldManager.instance.CurrentRunVariables.ForestWave <= ForestCombatManager.instance.WickedWitchWave)
		{
			AudioManager.me.PlaySound2D(ForestCombatManager.instance.WitchSounds, Random.Range(1.1f, 1.3f), 0.5f);
			WickedWitch witch = Cutscenes.FindOrCreateWitch();
			WorldManager.instance.CreateSmoke(witch.MyGameCard.transform.position);
			Cutscenes.FocusCameraOnWitchAndVillagers();
			Cutscenes.Text = SokLoc.Translate("label_forest_lost");
			yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
			Cutscenes.Text = SokLoc.Translate("label_forest_lost_2");
			yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
			WorldManager.instance.CreateSmoke(witch.MyGameCard.transform.position);
			witch.MyGameCard.DestroyCard(false, true);
			AudioManager.me.PlaySound2D(ForestCombatManager.instance.WitchSounds, Random.Range(1.1f, 1.3f), 0.5f);
			witch = null;
		}
		else
		{
			Cutscenes.Text = SokLoc.Translate("label_forest_lost_2");
			yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		}
		GameCamera.instance.TargetPositionOverride = null;
		ForestCombatManager.instance.ForestWaveLost();
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator BossFight(Temple temple, CardData goblet)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.TargetPositionOverride = new Vector3?(temple.transform.position);
		Cutscenes.Title = SokLoc.Translate("label_goblet_brought_to_temple");
		Cutscenes.Text = "";
		if (!WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
		{
			string text = SokLoc.Translate("label_start_the_ritual");
			string text2 = SokLoc.Translate("label_cancel_the_ritual");
			yield return Cutscenes.WaitForAnswer(new string[] { text, text2 });
		}
		else
		{
			string text = SokLoc.Translate("label_well_done");
			yield return Cutscenes.WaitForAnswer(new string[] { text });
		}
		Cutscenes.Title = "";
		yield return new WaitForSeconds(0.5f);
		if (WorldManager.instance.ContinueButtonIndex == 0)
		{
			temple.MyGameCard.DestroyCard(true, true);
			goblet.MyGameCard.DestroyCard(true, true);
			if (!WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
			{
				WorldManager.instance.CreateCard(temple.transform.position, "demon", true, false, true);
			}
			else
			{
				WorldManager.instance.CurrentRunVariables.FinishedDemon = true;
			}
		}
		else
		{
			goblet.MyGameCard.RemoveFromStack();
		}
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator BossFight2(Cathedral cathedral, CardData goblet)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.TargetPositionOverride = new Vector3?(cathedral.transform.position);
		Cutscenes.Title = SokLoc.Translate("label_relic_brought_to_cathedral");
		Cutscenes.Text = "";
		if (!WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
		{
			string text = SokLoc.Translate("label_start_the_ritual");
			string text2 = SokLoc.Translate("label_cancel_the_ritual");
			yield return Cutscenes.WaitForAnswer(new string[] { text, text2 });
		}
		else
		{
			string text = SokLoc.Translate("label_well_done");
			yield return Cutscenes.WaitForAnswer(new string[] { text });
		}
		Cutscenes.Title = "";
		yield return new WaitForSeconds(0.5f);
		if (WorldManager.instance.ContinueButtonIndex == 0)
		{
			cathedral.MyGameCard.DestroyCard(true, true);
			goblet.MyGameCard.DestroyCard(true, true);
			if (!WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
			{
				WorldManager.instance.CreateCard(cathedral.transform.position, "demon_lord", true, false, true);
			}
			else
			{
				WorldManager.instance.CurrentRunVariables.FinishedDemonLord = true;
			}
		}
		else
		{
			goblet.MyGameCard.RemoveFromStack();
		}
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator BossFightComplete(Demon demon)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.TargetPositionOverride = new Vector3?(demon.transform.position);
		Cutscenes.Title = SokLoc.Translate("label_you_killed_the_demon");
		Cutscenes.Text = "";
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_nice"));
		yield return new WaitForSeconds(1f);
		demon.MyGameCard.DestroyCard(true, true);
		if (!WorldManager.instance.CurrentSave.GotIslandIntroPack)
		{
			WorldManager.instance.Cutscene.QueueCutscene(Cutscenes.IslandIntroPack());
		}
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator BossFight2Complete(Demon demon)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.TargetPositionOverride = new Vector3?(demon.transform.position);
		Cutscenes.Title = SokLoc.Translate("label_you_killed_the_demon_lord");
		Cutscenes.Text = "";
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_nice"));
		yield return new WaitForSeconds(1f);
		demon.MyGameCard.DestroyCard(true, true);
		yield return new WaitForSeconds(1f);
		Cutscenes.Title = SokLoc.Translate("label_completed_main_quest");
		Cutscenes.Text = SokLoc.Translate("label_keep_playing");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_nice"));
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator Wish1(WishingWell well)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.TargetPositionOverride = new Vector3?(well.transform.position);
		Cutscenes.Title = SokLoc.Translate("card_wishing_well_name");
		Cutscenes.Text = SokLoc.Translate("card_wishing_well_wish_1");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_nice"));
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator Wish2(WishingWell well)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.TargetPositionOverride = new Vector3?(well.transform.position);
		Cutscenes.Title = SokLoc.Translate("card_wishing_well_name");
		Cutscenes.Text = SokLoc.Translate("card_wishing_well_wish_2");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_nice"));
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator Wish5(WishingWell well)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.TargetPositionOverride = new Vector3?(well.transform.position);
		Cutscenes.Title = SokLoc.Translate("card_wishing_well_name");
		Cutscenes.Text = SokLoc.Translate("card_wishing_well_wish_5");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_nice"));
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator Wish10(WishingWell well)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.TargetPositionOverride = new Vector3?(well.transform.position);
		Cutscenes.Title = SokLoc.Translate("card_wishing_well_name");
		Cutscenes.Text = SokLoc.Translate("card_wishing_well_wish_10");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_nice"));
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator Wish20(WishingWell well)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.TargetPositionOverride = new Vector3?(well.transform.position);
		Cutscenes.Title = SokLoc.Translate("card_wishing_well_name");
		Cutscenes.Text = SokLoc.Translate("card_wishing_well_wish_20");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_nice"));
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator Wish50(WishingWell well)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.TargetPositionOverride = new Vector3?(well.transform.position);
		Cutscenes.Title = SokLoc.Translate("card_wishing_well_name");
		Cutscenes.Text = SokLoc.Translate("card_wishing_well_wish_50");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_nice"));
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator WaitForAnswer(params string[] answers)
	{
		CutsceneScreen.instance.CreateMultipleOptions(answers);
		WorldManager.instance.ContinueClicked = false;
		while (!WorldManager.instance.ContinueClicked)
		{
			yield return null;
			if (!(GameCanvas.instance.CurrentScreen is CutsceneScreen))
			{
				GameCanvas.instance.SetScreen<CutsceneScreen>();
			}
		}
		CutsceneScreen.instance.ClearMultipleOptions();
		WorldManager.instance.ShowContinueButton = false;
		yield break;
	}

	public static IEnumerator WaitForContinueClicked(string text)
	{
		WorldManager.instance.ContinueClicked = false;
		WorldManager.instance.ContinueButtonText = text;
		WorldManager.instance.ShowContinueButton = true;
		while (!WorldManager.instance.ContinueClicked)
		{
			yield return null;
			if (!(GameCanvas.instance.CurrentScreen is CutsceneScreen))
			{
				GameCanvas.instance.SetScreen<CutsceneScreen>();
			}
		}
		WorldManager.instance.ShowContinueButton = false;
		yield break;
	}

	public static IEnumerator RunScriptableCutscene(ScriptableCutscene scriptableCutscene)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		CutsceneScreen.instance.IsAdvisorCutscene = scriptableCutscene.IsAdvisorCutscene;
		if (scriptableCutscene.IsAdvisorCutscene)
		{
			if (scriptableCutscene.AdvisorWarning)
			{
				AudioManager.me.PlaySound2D(AudioManager.me.AdvisorWarning, Random.Range(0.9f, 1.1f), 0.3f);
			}
			else
			{
				AudioManager.me.PlaySound2D(AudioManager.me.AdvisorAppears, Random.Range(0.9f, 1.1f), 0.3f);
			}
		}
		int num;
		for (int i = 0; i < scriptableCutscene.CutsceneSteps.Count; i = num + 1)
		{
			CutsceneStep cutsceneStep = scriptableCutscene.CutsceneSteps[i];
			cutsceneStep.StepIndex = i;
			yield return cutsceneStep.Process();
			num = i;
		}
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator RunScriptableCutsceneInCutscene(ScriptableCutscene scriptableCutscene)
	{
		CutsceneScreen.instance.IsAdvisorCutscene = scriptableCutscene.IsAdvisorCutscene;
		if (scriptableCutscene.IsAdvisorCutscene)
		{
			if (scriptableCutscene.AdvisorWarning)
			{
				AudioManager.me.PlaySound2D(AudioManager.me.AdvisorWarning, Random.Range(0.9f, 1.1f), 0.3f);
			}
			else
			{
				AudioManager.me.PlaySound2D(AudioManager.me.AdvisorAppears, Random.Range(0.9f, 1.1f), 0.3f);
			}
		}
		foreach (CutsceneStep cutsceneStep in scriptableCutscene.CutsceneSteps)
		{
			yield return cutsceneStep.Process();
		}
		List<CutsceneStep>.Enumerator enumerator = default(List<CutsceneStep>.Enumerator);
		yield break;
		yield break;
	}

	public static WickedWitch FindOrCreateWitch()
	{
		GameCard gameCard = WorldManager.instance.CardQuery.GetAllCardsOnBoard("forest").FirstOrDefault<GameCard>((GameCard x) => x.CardData.Id == "wicked_witch");
		CardData cardData = ((gameCard != null) ? gameCard.CardData : null);
		if (cardData == null)
		{
			cardData = WorldManager.instance.CreateCard(ForestCombatManager.GetWitchPosition(), "wicked_witch", true, false, true);
		}
		return cardData as WickedWitch;
	}

	public static Spirit FindOrCreateSpirit(CurseType curse, Vector3? spawnPos = null)
	{
		string text = "";
		if (curse == CurseType.Happiness)
		{
			text = "happiness_spirit";
		}
		else if (curse == CurseType.Greed)
		{
			text = "greed_spirit";
		}
		else if (curse == CurseType.Death)
		{
			text = "death_spirit";
		}
		CardData cardData = WorldManager.instance.GetCard(text);
		if (cardData == null)
		{
			Vector3 vector = ((spawnPos != null) ? spawnPos.Value : WorldManager.instance.MiddleOfBoard());
			cardData = WorldManager.instance.CreateCard(vector, text, true, false, true);
			cardData.MyGameCard.SendIt();
		}
		return cardData as Spirit;
	}

	public static GameCard FindOrCreateGameCard(string cardId, Vector3 position)
	{
		CardData cardData = WorldManager.instance.GetCard(cardId);
		if (cardData == null)
		{
			cardData = WorldManager.instance.CreateCard(position, cardId, true, true, true);
		}
		if (cardData == null)
		{
			return null;
		}
		return cardData.MyGameCard;
	}

	private static void FocusCameraOnWitchAndVillagers()
	{
		List<BaseVillager> cards = WorldManager.instance.CardQuery.GetCards<BaseVillager>();
		WickedWitch wickedWitch = Cutscenes.FindOrCreateWitch();
		if (cards.Count == 0)
		{
			GameCamera.instance.TargetPositionOverride = new Vector3?(wickedWitch.transform.position);
			return;
		}
		Vector3 vector = new Vector3(wickedWitch.transform.position.x, wickedWitch.transform.position.y, Mathf.Lerp(wickedWitch.transform.position.z, cards[0].transform.position.z, 0.5f));
		GameCamera.instance.TargetPositionOverride = new Vector3?(vector);
	}

	public static IEnumerator SpiritOutro(CurseType curseType)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		Cutscenes.Title = "";
		if (curseType == CurseType.Happiness)
		{
			Cutscenes.Text = SokLoc.Translate("label_spirit_outro_0_happiness");
		}
		if (curseType == CurseType.Greed)
		{
			Cutscenes.Text = SokLoc.Translate("label_spirit_outro_0_greed");
		}
		if (curseType == CurseType.Death)
		{
			Cutscenes.Text = SokLoc.Translate("label_spirit_outro_0_death");
		}
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Cutscenes.Text = SokLoc.Translate("label_spirit_outro_1");
		Spirit spirit = Cutscenes.FindOrCreateSpirit(curseType, null);
		GameCamera.instance.TargetCardOverride = spirit;
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Cutscenes.Text = SokLoc.Translate("label_spirit_outro_2");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Cutscenes.Text = SokLoc.Translate("label_spirit_outro_3");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Cutscenes.Text = SokLoc.Translate("label_spirit_outro_4");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator ReturnToBoardCutscene()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		Cutscenes.Text = SokLoc.Translate("label_happiness_return_0");
		yield return Cutscenes.WaitForAnswer(new string[]
		{
			SokLoc.Translate("label_spirit_outro_choice_0"),
			SokLoc.Translate("label_spirit_outro_choice_1")
		});
		if (WorldManager.instance.ContinueButtonIndex == 1)
		{
			WorldManager.instance.GoToBoard(WorldManager.instance.GetBoardWithId("main"), null, "default");
		}
		Cutscenes.Stop(false);
		yield break;
	}

	private static string GetCurseName(CurseType curseType)
	{
		if (curseType == CurseType.Death)
		{
			return SokLoc.Translate("card_death_curse_name");
		}
		if (curseType == CurseType.Greed)
		{
			return SokLoc.Translate("card_greed_curse_name");
		}
		if (curseType == CurseType.Happiness)
		{
			return SokLoc.Translate("card_happiness_curse_name");
		}
		throw new ArgumentException("curseType");
	}

	public static IEnumerator AltarIntro(Altar altar, CurseType curseType)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		Cutscenes.Title = SokLoc.Translate("label_altar_cutscene_title");
		GameCamera.instance.TargetCardOverride = altar;
		GameCamera.instance.Screenshake = 0.5f;
		yield return new WaitForSeconds(2f);
		Cutscenes.Title = "...";
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		WorldManager.instance.CreateSmoke(altar.transform.position);
		altar.MyGameCard.Child.DestroyCard(false, true);
		yield return new WaitForSeconds(1f);
		bool doMainRun = false;
		if ((curseType == CurseType.Death && WorldManager.instance.CurrentSave.FinishedDeath) || (curseType == CurseType.Happiness && WorldManager.instance.CurrentSave.FinishedHappiness))
		{
			Cutscenes.Title = "";
			Cutscenes.Text = SokLoc.Translate("label_altar_cutscene_mainland");
			yield return Cutscenes.WaitForAnswer(new string[]
			{
				SokLoc.Translate("label_altar_run_board", new LocParam[] { LocParam.Create("curse", Cutscenes.GetCurseName(curseType)) }),
				SokLoc.Translate("label_altar_run_redo")
			});
			if (WorldManager.instance.ContinueButtonIndex == 0)
			{
				doMainRun = true;
			}
		}
		if (doMainRun)
		{
			GameCard gameCard = null;
			if (curseType == CurseType.Greed)
			{
				gameCard = Cutscenes.FindOrCreateGameCard("greed_curse", altar.transform.position);
			}
			if (curseType == CurseType.Death)
			{
				gameCard = Cutscenes.FindOrCreateGameCard("death_curse", altar.transform.position);
			}
			if (curseType == CurseType.Happiness)
			{
				gameCard = Cutscenes.FindOrCreateGameCard("happiness_curse", altar.transform.position);
			}
			if (gameCard != null)
			{
				gameCard.SendIt();
				yield return Cutscenes.CurseActivatedOnBoard(gameCard, curseType);
			}
		}
		else
		{
			Cutscenes.<>c__DisplayClass49_0 CS$<>8__locals1 = new Cutscenes.<>c__DisplayClass49_0();
			Cutscenes.Title = "";
			Cutscenes.Text = "...";
			CS$<>8__locals1.spirit = Cutscenes.FindOrCreateSpirit(curseType, new Vector3?(altar.transform.position));
			CS$<>8__locals1.spirit.CreateBackgroundPlane();
			GameCamera.instance.TargetCardOverride = CS$<>8__locals1.spirit;
			yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_wow"));
			Cutscenes.Title = "";
			Cutscenes.Text = SokLoc.Translate("label_altar_cutscene_text", new LocParam[] { LocParam.Create("spirit", CS$<>8__locals1.spirit.FullName) });
			yield return Cutscenes.WaitForAnswer(new string[]
			{
				SokLoc.Translate("label_altar_cutscene_help_true"),
				SokLoc.Translate("label_altar_cutscene_help_false")
			});
			if (WorldManager.instance.ContinueButtonIndex == 0)
			{
				Cutscenes.Text = SokLoc.Translate("label_altar_cutscene_help_spirit_true");
				yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
				Cutscenes.Stop(true);
				WorldManager.instance.StartCursePlaythrough(curseType, delegate
				{
					GameCamera.instance.TargetCardOverride = null;
					CS$<>8__locals1.spirit.MyGameCard.DestroyCard(false, true);
				});
			}
			else
			{
				Cutscenes.Text = SokLoc.Translate("label_altar_cutscene_help_spirit_false");
				yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
				GameCamera.instance.TargetCardOverride = null;
				CS$<>8__locals1.spirit.MyGameCard.DestroyCard(false, true);
				yield return new WaitForSeconds(0.5f);
				Cutscenes.Stop(false);
			}
			CS$<>8__locals1 = null;
		}
		altar.inCutscene = false;
		yield break;
	}

	public static IEnumerator DemonOfSadness()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		Cutscenes.Title = "..";
		Cutscenes.Text = "";
		GameCamera.instance.TargetCardOverride = WorldManager.instance.CardQuery.GetCard<Unhappiness>();
		yield return new WaitForSeconds(1f);
		foreach (Unhappiness unhappiness in WorldManager.instance.CardQuery.GetCards<Unhappiness>())
		{
			unhappiness.MyGameCard.DestroyCard(false, true);
		}
		Cutscenes.Title = SokLoc.Translate("label_sadness_demon_title");
		Cutscenes.Text = SokLoc.Translate("label_sadness_demon_title");
		CardData cardData = WorldManager.instance.CreateCard(WorldManager.instance.GetRandomSpawnPosition(), "sadness_demon", true, false, true);
		GameCamera.instance.TargetCardOverride = cardData;
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_uh_oh"));
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator ShamanLeaving(Shaman shaman)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.TargetPositionOverride = new Vector3?(shaman.transform.position);
		Cutscenes.Title = SokLoc.Translate("label_shaman_leaving_title");
		Cutscenes.Text = SokLoc.Translate("label_shaman_leaving_text");
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_okay"));
		Cutscenes.Text = "";
		WorldManager.instance.CreateSmoke(shaman.transform.position);
		shaman.MyGameCard.DestroyCard(false, true);
		yield return new WaitForSeconds(2f);
		Cutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator CurseActivatedOnBoard(GameCard curse, CurseType curseType)
	{
		Cutscenes.Title = "...";
		Cutscenes.Text = "";
		Spirit spirit = Cutscenes.FindOrCreateSpirit(curseType, new Vector3?(curse.transform.position));
		spirit.CreateBackgroundPlane();
		GameCamera.instance.TargetCardOverride = spirit;
		yield return new WaitForSeconds(1f);
		Cutscenes.Title = "";
		Cutscenes.Text = SokLoc.Translate("label_altar_cutscene_good_luck", new LocParam[] { LocParam.Create("spirit", spirit.FullName) });
		yield return Cutscenes.WaitForContinueClicked(SokLoc.Translate("label_uh_oh"));
		spirit.MyGameCard.DestroyCard(false, true);
		yield return new WaitForSeconds(0.5f);
		Cutscenes.Stop(false);
		yield break;
	}
}
