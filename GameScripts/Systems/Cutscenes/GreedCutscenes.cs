using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GreedCutscenes
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
		GreedCutscenes.Text = "";
		GreedCutscenes.Title = "";
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
		GreedCutscenes.currentAnimation = null;
	}

	public static IEnumerator FinalDemandStart(Demand demand)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		Vector3 randomPos = WorldManager.instance.GetRandomSpawnPosition();
		GameCamera.instance.TargetPositionOverride = new Vector3?(randomPos);
		yield return new WaitForSeconds(2f);
		GreedCutscenes.FindOrCreateGameCard("merchant", new Vector3?(randomPos));
		WorldManager.instance.CreateSmoke(randomPos);
		GreedCutscenes.Text = MewtationsLoc.Translate("label_final_demand_merchant");
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		Royal royal = GreedCutscenes.FindOrCreateGameCard("royal", null).CardData as Royal;
		GreedCutscenes.Title = "";
		GameCamera.instance.TargetPositionOverride = new Vector3?(royal.transform.position);
		GreedCutscenes.Text = MewtationsLoc.Translate("label_final_demand_text");
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		GreedCutscenes.Text = MewtationsLoc.Translate("label_final_demand_text_2");
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		GreedCutscenes.Text = MewtationsLoc.Translate("label_final_demand_text_3");
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		GreedCutscenes.Text = MewtationsLoc.Translate("label_final_demand_text_4");
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		DemandManager.instance.QuestStarted(demand);
		yield break;
	}

	public static IEnumerator FinalDemandEndSuccess(bool shouldStop)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		Royal royal = GreedCutscenes.FindOrCreateGameCard("royal", null).CardData as Royal;
		DragonEgg egg = GreedCutscenes.FindOrCreateGameCard("dragon_egg", null).CardData as DragonEgg;
		GreedCutscenes.Title = "";
		GameCamera.instance.TargetPositionOverride = new Vector3?(royal.transform.position);
		GreedCutscenes.Text = MewtationsLoc.Translate("label_final_demand_end_text");
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		GreedCutscenes.Text = "...";
		GameCamera.instance.TargetPositionOverride = new Vector3?(egg.transform.position);
		yield return new WaitForSeconds(0.5f);
		egg.CrackedState = 1;
		AudioManager.me.PlaySound2D(egg.CrackedSound, Random.Range(1.1f, 1.3f), 0.5f);
		yield return new WaitForSeconds(0.5f);
		GreedCutscenes.Text = MewtationsLoc.Translate("label_final_demand_end_text_2");
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		GameCamera.instance.TargetPositionOverride = new Vector3?(royal.transform.position);
		GreedCutscenes.Text = MewtationsLoc.Translate("label_final_demand_end_text_3");
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_uh_oh"));
		GreedCutscenes.Text = "...";
		GameCamera.instance.TargetPositionOverride = new Vector3?(egg.transform.position);
		yield return new WaitForSeconds(0.5f);
		egg.CrackedState = 2;
		AudioManager.me.PlaySound2D(egg.CrackedSound2, Random.Range(1.1f, 1.3f), 0.5f);
		yield return new WaitForSeconds(0.5f);
		GreedCutscenes.Text = MewtationsLoc.Translate("label_final_demand_end_text_4");
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		GameCamera.instance.TargetPositionOverride = new Vector3?(royal.transform.position);
		AudioManager.me.PlaySound2D(DemandManager.instance.FailedDemandSound, Random.Range(1.1f, 1.3f), 0.5f);
		AngryRoyal angryRoyal = WorldManager.instance.ChangeToCard(royal.MyGameCard, "angry_royal") as AngryRoyal;
		WorldManager.instance.CreateSmoke(royal.Position);
		GreedCutscenes.Text = MewtationsLoc.Translate("label_final_demand_end_text_5");
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		GameCamera.instance.TargetPositionOverride = new Vector3?(egg.transform.position);
		AudioManager.me.PlaySound2D(egg.CrackedSound2, Random.Range(1.1f, 1.3f), 0.5f);
		GreedCutscenes.Text = MewtationsLoc.Translate("label_final_demand_end_text_6");
		yield return new WaitForSeconds(1f);
		Combatable dragon = WorldManager.instance.ChangeToCard(egg.MyGameCard, "baby_dragon") as Combatable;
		WorldManager.instance.CreateSmoke(dragon.transform.position);
		AudioManager.me.PlaySound2D(dragon.PickupSound, Random.Range(1.1f, 1.3f), 0.4f);
		GreedCutscenes.Text = MewtationsLoc.Translate("label_final_demand_end_text_7");
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_uh_oh"));
		dragon.MyGameCard.CardAnimations.Add(new CardAnimation_FakeMeleeAttack(dragon.MyGameCard, angryRoyal.MyGameCard));
		AudioManager.me.PlaySound2D(AudioManager.me.Crit, Random.Range(0.8f, 1f), 0.1f);
		GreedCutscenes.Text = MewtationsLoc.Translate("label_final_demand_end_text_8");
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_nice"));
		yield return new WaitForSeconds(1f);
		AudioManager.me.PlaySound2D(AudioManager.me.Crit, Random.Range(0.8f, 1f), 0.1f);
		angryRoyal.DieInCutscene();
		GreedCutscenes.Text = "";
		yield return new WaitForSeconds(1f);
		yield return GreedCutscenes.FinalDemandLiftCurse(shouldStop);
		if (shouldStop)
		{
			GreedCutscenes.Stop(false);
		}
		yield break;
	}

	public static IEnumerator GreedWearCrown()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GreedCutscenes.Title = "";
		GameCard spirit = GreedCutscenes.FindOrCreateGameCard("greed_spirit", null);
		GameCamera.instance.TargetPositionOverride = new Vector3?(spirit.transform.position);
		GreedCutscenes.Text = MewtationsLoc.Translate("label_greed_outro_wear_crown");
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		spirit.DestroyCard(false, true);
		GreedCutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator NewVillager()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GreedCutscenes.Title = "";
		GameCard gameCard = GreedCutscenes.FindOrCreateGameCard("royal", null);
		GameCamera.instance.TargetCardOverride = gameCard;
		GreedCutscenes.Text = MewtationsLoc.Translate("label_greed_new_villager");
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		GreedCutscenes.Text = MewtationsLoc.Translate("label_greed_new_villager_2");
		CardData cardData = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "villager", true, false, true);
		GameCamera.instance.TargetCardOverride = cardData;
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_nice"));
		yield break;
	}

	public static IEnumerator FinalDemandLiftCurse(bool shouldStop)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GreedCutscenes.Title = "";
		CardData spirit = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "greed_spirit", true, true, true);
		GameCamera.instance.TargetPositionOverride = new Vector3?(spirit.transform.position);
		GreedCutscenes.Text = MewtationsLoc.Translate("label_greed_lift_curse");
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		GameCard gameCard = GreedCutscenes.FindOrCreateGameCard("royal_crown", null);
		GameCamera.instance.TargetCardOverride = gameCard;
		GreedCutscenes.Text = MewtationsLoc.Translate("label_greed_lift_curse_2");
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		GameCamera.instance.TargetPositionOverride = new Vector3?(WorldManager.instance.CardQuery.GetCard<Curse>().transform.position);
		GreedCutscenes.Text = MewtationsLoc.Translate("label_greed_lift_curse_3");
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		if (spirit != null)
		{
			spirit.MyGameCard.DestroyCard(false, true);
		}
		yield return new WaitForSeconds(0.5f);
		if (shouldStop)
		{
			GreedCutscenes.Stop(false);
		}
		yield break;
	}

	public static IEnumerator KillRoyalLiftCurse()
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GreedCutscenes.Title = "";
		CardData spirit = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "greed_spirit", true, true, true);
		GameCamera.instance.TargetPositionOverride = new Vector3?(spirit.transform.position);
		GreedCutscenes.Text = MewtationsLoc.Translate("label_greed_lift_curse_kill_royal");
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		GameCard gameCard = GreedCutscenes.FindOrCreateGameCard("royal_crown", null);
		GameCamera.instance.TargetCardOverride = gameCard;
		GreedCutscenes.Text = MewtationsLoc.Translate("label_greed_lift_curse_2");
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		GameCamera.instance.TargetPositionOverride = new Vector3?(WorldManager.instance.CardQuery.GetCard<BaseVillager>().transform.position);
		GreedCutscenes.Text = MewtationsLoc.Translate("label_greed_lift_curse_3");
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		if (spirit != null)
		{
			spirit.MyGameCard.DestroyCard(false, true);
		}
		yield return new WaitForSeconds(0.5f);
		GreedCutscenes.Stop(false);
		yield break;
	}

	public static IEnumerator StartDemand(Demand demand)
	{
		GreedCutscenes.Title = MewtationsLoc.Translate("greed_quest_demand_title");
		foreach (GreedAnimationState greedAnimationState in demand.QuestStartAnimationStates)
		{
			GreedCutscenes.Title = "";
			GreedCutscenes.Text = "";
			GameCard gameCard = GreedCutscenes.FindOrCreateGameCard(greedAnimationState.CameraTargetId, null);
			if (gameCard != null)
			{
				GameCamera.instance.TargetPositionOverride = new Vector3?(gameCard.transform.position);
			}
			if (!string.IsNullOrEmpty(greedAnimationState.TitleTerm))
			{
				GreedCutscenes.Title = MewtationsLoc.Translate(greedAnimationState.TitleTerm);
			}
			if (!string.IsNullOrEmpty(greedAnimationState.DescriptionTerm))
			{
				GreedCutscenes.Text = MewtationsLoc.Translate(greedAnimationState.DescriptionTerm);
			}
			yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate(greedAnimationState.ContinueTerm));
		}
		List<GreedAnimationState>.Enumerator enumerator = default(List<GreedAnimationState>.Enumerator);
		GreedCutscenes.Text = DemandManager.instance.GetDemandStartDescription(demand, null);
		GameCard royal = GreedCutscenes.FindOrCreateGameCard("royal", null);
		GameCamera.instance.TargetCardOverride = royal;
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		if (demand.BlueprintIds.Any<string>((string id) => !string.IsNullOrEmpty(id) && !WorldManager.instance.HasFoundCard(id)))
		{
			GreedCutscenes.Text = MewtationsLoc.Translate("greed_quest_demand_description_not_found");
			yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
			foreach (string text in demand.BlueprintIds)
			{
				CardData cardData = WorldManager.instance.CreateCard(royal.transform.position, text, true, true, true);
				GameCamera.instance.TargetCardOverride = cardData;
				cardData.MyGameCard.SendIt();
			}
		}
		DemandManager.instance.QuestStarted(demand);
		yield break;
		yield break;
	}

	public static IEnumerator FinishDemandSuccess(DemandEvent demandEvent)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GreedCutscenes.Title = MewtationsLoc.Translate("greed_quest_demand_title");
		GreedCutscenes.Text = MewtationsLoc.Translate("label_demand_complete_start");
		GameCamera.instance.TargetCardOverride = GreedCutscenes.FindOrCreateGameCard("royal", null);
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		foreach (GreedAnimationState greedAnimationState in demandEvent.Demand.QuestSuccessAnimationStates)
		{
			GameCard gameCard = GreedCutscenes.FindOrCreateGameCard(greedAnimationState.CameraTargetId, null);
			if (gameCard != null)
			{
				GameCamera.instance.TargetPositionOverride = new Vector3?(gameCard.transform.position);
			}
			GreedCutscenes.Title = "";
			GreedCutscenes.Text = "";
			if (!string.IsNullOrEmpty(greedAnimationState.TitleTerm))
			{
				GreedCutscenes.Title = MewtationsLoc.Translate(greedAnimationState.TitleTerm);
			}
			if (!string.IsNullOrEmpty(greedAnimationState.DescriptionTerm))
			{
				GreedCutscenes.Text = MewtationsLoc.Translate(greedAnimationState.DescriptionTerm);
			}
			yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		}
		List<GreedAnimationState>.Enumerator enumerator = default(List<GreedAnimationState>.Enumerator);
		GreedCutscenes.Text = DemandManager.instance.GetRandomSuccessDescription(demandEvent.Demand);
		GameCard gameCard2 = GreedCutscenes.FindOrCreateGameCard("royal", null);
		if (gameCard2 != null)
		{
			GameCamera.instance.TargetPositionOverride = new Vector3?(gameCard2.transform.position);
		}
		GameCamera.instance.CameraPositionDistanceOverride = null;
		if (demandEvent.Demand.ShouldDestroyOnComplete)
		{
			GreedCutscenes.Text = "";
			float speedup = 1f;
			int num;
			for (int i = 0; i < demandEvent.Demand.Amount - demandEvent.AmountGiven; i = num + 1)
			{
				CardData card = WorldManager.instance.GetCard(demandEvent.Demand.CardToGet);
				if (card != null)
				{
					GameCamera.instance.TargetPositionOverride = new Vector3?(card.Position);
					yield return new WaitForSeconds(0.2f * speedup);
					WorldManager.instance.CreateSmoke(card.Position);
					card.MyGameCard.DestroyCard(false, true);
					yield return new WaitForSeconds(0.3f * speedup);
					speedup -= 0.1f;
					speedup = Mathf.Max(0.4f, speedup);
				}
				card = null;
				num = i;
			}
			GameCamera.instance.TargetPositionOverride = null;
			yield return new WaitForSeconds(0.5f);
			GreedCutscenes.Text = MewtationsLoc.Translate("label_demand_collected");
			yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		}
		else
		{
			CardData card2 = WorldManager.instance.GetCard(demandEvent.Demand.CardToGet);
			if (card2 != null)
			{
				GameCamera.instance.TargetCardOverride = card2;
			}
			GreedCutscenes.Text = MewtationsLoc.Translate("label_demand_collected_2");
			yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		}
		DemandManager.instance.DemandFinishedSuccess(demandEvent.Demand);
		yield break;
		yield break;
	}

	public static IEnumerator FinishDemandSuccessPreMoon(Demand demand)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GreedCutscenes.Title = MewtationsLoc.Translate("greed_quest_demand_title");
		GreedCutscenes.Text = "";
		foreach (GreedAnimationState greedAnimationState in demand.QuestSuccessAnimationStates)
		{
			GameCard gameCard = GreedCutscenes.FindOrCreateGameCard(greedAnimationState.CameraTargetId, null);
			if (gameCard != null)
			{
				GameCamera.instance.TargetPositionOverride = new Vector3?(gameCard.transform.position);
			}
			GreedCutscenes.Title = "";
			GreedCutscenes.Text = "";
			if (!string.IsNullOrEmpty(greedAnimationState.TitleTerm))
			{
				GreedCutscenes.Title = MewtationsLoc.Translate(greedAnimationState.TitleTerm);
			}
			if (!string.IsNullOrEmpty(greedAnimationState.DescriptionTerm))
			{
				GreedCutscenes.Text = MewtationsLoc.Translate(greedAnimationState.DescriptionTerm);
			}
			yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		}
		List<GreedAnimationState>.Enumerator enumerator = default(List<GreedAnimationState>.Enumerator);
		GreedCutscenes.Title = MewtationsLoc.Translate("greed_quest_demand_title");
		GreedCutscenes.Text = DemandManager.instance.GetRandomSuccessDescription(demand);
		GameCard gameCard2 = GreedCutscenes.FindOrCreateGameCard("royal", null);
		if (gameCard2 != null)
		{
			GameCamera.instance.TargetPositionOverride = new Vector3?(gameCard2.transform.position);
		}
		GameCamera.instance.CameraPositionDistanceOverride = null;
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		DemandManager.instance.DemandFinishedSuccess(demand);
		GreedCutscenes.Stop(false);
		yield break;
		yield break;
	}

	public static IEnumerator FinishDemandFailed(Demand demand)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GreedCutscenes.Title = MewtationsLoc.Translate("greed_quest_demand_title");
		GreedCutscenes.Text = MewtationsLoc.Translate("label_demand_complete_start");
		GameCamera.instance.TargetCardOverride = GreedCutscenes.FindOrCreateGameCard("royal", null);
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_uh_oh"));
		int amountToTake = WorldManager.instance.GetCardCount((CardData x) => x.Id == demand.CardToGet);
		if (demand.ShouldDestroyOnComplete && amountToTake > 0)
		{
			GreedCutscenes.Text = "";
			float speedup = 1f;
			int num;
			for (int i = 0; i < amountToTake; i = num + 1)
			{
				CardData card = WorldManager.instance.GetCard(demand.CardToGet);
				GameCamera.instance.TargetPositionOverride = new Vector3?(card.Position);
				yield return new WaitForSeconds(0.2f * speedup);
				WorldManager.instance.CreateSmoke(card.Position);
				card.MyGameCard.DestroyCard(false, true);
				yield return new WaitForSeconds(0.3f * speedup);
				speedup -= 0.1f;
				speedup = Mathf.Max(0.4f, speedup);
				card = null;
				num = i;
			}
			GameCamera.instance.TargetPositionOverride = null;
			yield return new WaitForSeconds(0.5f);
		}
		foreach (GreedAnimationState greedAnimationState in demand.QuestFailedAnimationStates)
		{
			GameCard gameCard = GreedCutscenes.FindOrCreateGameCard(greedAnimationState.CameraTargetId, null);
			if (gameCard != null)
			{
				GameCamera.instance.TargetPositionOverride = new Vector3?(gameCard.transform.position);
			}
			GreedCutscenes.Title = "";
			GreedCutscenes.Text = "";
			if (!string.IsNullOrEmpty(greedAnimationState.TitleTerm))
			{
				GreedCutscenes.Title = MewtationsLoc.Translate(greedAnimationState.TitleTerm);
			}
			if (!string.IsNullOrEmpty(greedAnimationState.DescriptionTerm))
			{
				GreedCutscenes.Text = MewtationsLoc.Translate(greedAnimationState.DescriptionTerm);
			}
			yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		}
		List<GreedAnimationState>.Enumerator enumerator = default(List<GreedAnimationState>.Enumerator);
		GreedCutscenes.Title = MewtationsLoc.Translate("greed_quest_demand_title");
		GreedCutscenes.Text = DemandManager.instance.GetRandomFailedDescription(demand);
		GameCard gameCard2 = GreedCutscenes.FindOrCreateGameCard("royal", null);
		if (gameCard2 != null)
		{
			GameCamera.instance.TargetCardOverride = gameCard2;
		}
		GameCamera.instance.CameraPositionDistanceOverride = null;
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_uh_oh"));
		AudioManager.me.PlaySound2D(DemandManager.instance.FailedDemandSound, 0.9f, 0.3f);
		if (WorldManager.instance.CurrentRunVariables.PreviousDemandEvents.Count == 0)
		{
			GreedCutscenes.Title = "";
			GreedCutscenes.Text = MewtationsLoc.Translate("label_greed_demand_failed_first_time");
			yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		}
		else
		{
			GreedCutscenes.Title = "";
			if (WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
			{
				GreedCutscenes.Text = MewtationsLoc.Translate("label_greed_demand_failed_fight_peaceful");
				yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_uh_oh"));
				float speedup = 1f;
				int i = 0;
				int coinsToTake = 3 * DemandManager.instance.GetTimesDemandFailed();
				int num;
				for (int j = 0; j < coinsToTake; j = num + 1)
				{
					CardData card = WorldManager.instance.GetCard("gold");
					if (card != null)
					{
						GameCamera.instance.TargetPositionOverride = new Vector3?(card.Position);
						yield return new WaitForSeconds(0.2f * speedup);
						WorldManager.instance.CreateSmoke(card.Position);
						card.MyGameCard.DestroyCard(false, true);
						num = i;
						i = num + 1;
						yield return new WaitForSeconds(0.3f * speedup);
					}
					else
					{
						foreach (Chest chest in WorldManager.instance.CardQuery.GetCards<Chest>())
						{
							if (coinsToTake == i)
							{
								break;
							}
							if (chest != null && chest.CoinCount > 0)
							{
								int num2 = coinsToTake - i;
								int take = Mathf.Min(chest.CoinCount, num2);
								if (take > 0)
								{
									GameCamera.instance.TargetPositionOverride = new Vector3?(card.Position);
									yield return new WaitForSeconds(0.2f * speedup);
									WorldManager.instance.CreateSmoke(chest.Position);
									chest.CoinCount -= take;
									i += take;
									yield return new WaitForSeconds(0.3f * speedup);
								}
							}
							chest = null;
						}
						List<Chest>.Enumerator enumerator2 = default(List<Chest>.Enumerator);
					}
					if (coinsToTake == i)
					{
						break;
					}
					speedup -= 0.1f;
					speedup = Mathf.Max(0.4f, speedup);
					card = null;
					num = j;
				}
			}
			else
			{
				GreedCutscenes.Text = MewtationsLoc.Translate("label_greed_demand_failed_fight");
				List<Combatable> list = DemandManager.instance.SpawnEnemies();
				GameCamera.instance.TargetCardOverride = list.FirstOrDefault<Combatable>();
				yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_uh_oh"));
			}
		}
		yield break;
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

	public static IEnumerator TryAttackRoyal(Royal royal, int tries)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameCamera.instance.TargetPositionOverride = new Vector3?(royal.transform.position);
		GreedCutscenes.Title = MewtationsLoc.Translate("label_try_attack_royal_title");
		if (tries < 4)
		{
			GreedCutscenes.Text = MewtationsLoc.Translate("label_try_attack_royal_description");
		}
		if (tries >= 4 && tries < 8)
		{
			GreedCutscenes.Text = MewtationsLoc.Translate("label_try_attack_royal_description_4");
		}
		if (tries == 8)
		{
			GreedCutscenes.Text = MewtationsLoc.Translate("label_try_attack_royal_description_8");
		}
		yield return GreedCutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		GreedCutscenes.Stop(false);
		yield break;
	}

	public static GameCard FindOrCreateGameCard(string cardId, Vector3? position = null)
	{
		CardData cardData = WorldManager.instance.GetCard(cardId);
		if (cardData == null)
		{
			if (position != null)
			{
				cardData = WorldManager.instance.CreateCard(position.Value, cardId, true, false, true);
			}
			else
			{
				cardData = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), cardId, true, false, true);
			}
		}
		if (cardData == null)
		{
			return null;
		}
		return cardData.MyGameCard;
	}
}
