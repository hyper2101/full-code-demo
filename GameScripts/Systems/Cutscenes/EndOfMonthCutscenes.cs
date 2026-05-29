using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EndOfMonthCutscenes
{
	public static string CutsceneTitle
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

	public static string CutsceneText
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

	public static int CurrentMonth
	{
		get
		{
			return WorldManager.instance.Time.CurrentMonth;
		}
	}

	private static float CalculateWaitFromSpeedup(float f)
	{
		return Mathf.Max(0.01f, 0.12f - f * 0.04f);
	}

	private static Food GetFoodToUseUp()
	{
		List<Food> cards = WorldManager.instance.CardQuery.GetCards<Food>();
		if (cards.Count == 0)
		{
			return null;
		}
		Demand currentDemand = ((WorldManager.instance.CurrentRunVariables.ActiveDemand != null) ? DemandManager.instance.GetDemandById(WorldManager.instance.CurrentRunVariables.ActiveDemand.DemandId) : null);
		return cards.OrderBy<Food, int>(delegate(Food c)
		{
			bool flag = c.MyGameCard.GetCardWithStatusInStack() != null;
			if (c.MyGameCard.HasCardInStack((CardData x) => x is MessHall))
			{
				return -1000 + c.MyGameCard.GetCardIndex();
			}
			if (c is Hotpot)
			{
				return -100 + c.FoodValue;
			}
			if (c.IsSpoiling && !flag)
			{
				return -5;
			}
			if (currentDemand != null)
			{
				if (currentDemand.CardToGet == "royal_banquet" && (c.Id == "fruit_salad" || c.Id == "wine" || c.Id == "roasted_meat" || c.Id == "olive_oil"))
				{
					return 5;
				}
				if (currentDemand.CardToGet == c.Id)
				{
					return 5;
				}
			}
			if (flag)
			{
				return 4;
			}
			if (WorldManager.instance.GetCardCount(c.Id) == 1 && !c.IsCookedFood)
			{
				return 3;
			}
			if (!c.IsCookedFood)
			{
				return 2;
			}
			return 0;
		}).ThenBy<Food, int>((Food x) => x.FoodValue).ThenBy<Food, int>((Food x) => x.GetValue())
			.FirstOrDefault<Food>((Food x) => x.FoodValue > 0);
	}

	public static List<BaseVillager> GetVillagersToAge()
	{
		List<BaseVillager> list = new List<BaseVillager>();
		foreach (GameCard gameCard in WorldManager.instance.AllCards)
		{
			if (gameCard.MyBoard.IsCurrent)
			{
				BaseVillager baseVillager = gameCard.CardData as BaseVillager;
				if (baseVillager != null)
				{
					list.Add(baseVillager);
				}
			}
		}
		list = list.OrderBy<BaseVillager, int>(delegate(BaseVillager x)
		{
			if (x is TeenageVillager)
			{
				return 0;
			}
			if (x is Villager)
			{
				return 1;
			}
			if (x is OldVillager)
			{
				return 2;
			}
			return 3;
		}).ToList<BaseVillager>();
		return list;
	}

	public static bool AnyVillagerWillChangeLifeStage(List<BaseVillager> villagers)
	{
		foreach (BaseVillager baseVillager in villagers)
		{
			if (baseVillager.WillChangeLifeStage() || baseVillager.MyLifeStage == LifeStage.Dead)
			{
				return true;
			}
		}
		return false;
	}

	public static bool AnyAnimalWillDie(List<Animal> animals)
	{
		foreach (Animal animal in animals)
		{
			if (WorldManager.instance.Time.CurrentMonth - animal.CreationMonth >= 5)
			{
				return true;
			}
		}
		return false;
	}

	public static IEnumerator CheckMakeSick()
	{
		List<BaseVillager> list = (from x in WorldManager.instance.CardQuery.GetCards<BaseVillager>()
			where !x.HasStatusEffectOfType<StatusEffect_Sick>() && !x.HasEquipableWithId("plague_mask")
			select x).ToList<BaseVillager>();
		List<Poop> cards = WorldManager.instance.CardQuery.GetCards<Poop>();
		List<BaseVillager> list2 = new List<BaseVillager>();
		foreach (Poop poop in cards)
		{
			if (poop.CanMakeSick)
			{
				if (list.Count <= 0)
				{
					break;
				}
				if (Random.value * 100f < poop.SickChance)
				{
					BaseVillager baseVillager = list.Choose<BaseVillager>();
					baseVillager.AddStatusEffect(new StatusEffect_Sick());
					AudioManager.me.PlaySound2D(AudioManager.me.GetSick, 1f, 0.5f);
					list2.Add(baseVillager);
				}
			}
		}
		if (list2.Count == 0)
		{
			yield break;
		}
		foreach (BaseVillager baseVillager2 in list2)
		{
			EndOfMonthCutscenes.CutsceneTitle = MewtationsLoc.Translate("label_uh_oh");
			EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_villager_sick", new LocParam[] { LocParam.Create("villager", baseVillager2.Name) });
			GameCamera.instance.TargetCardOverride = baseVillager2;
			yield return Cutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		}
		List<BaseVillager>.Enumerator enumerator2 = default(List<BaseVillager>.Enumerator);
		yield break;
		yield break;
	}

	public static IEnumerator AgeVillagers(List<BaseVillager> villagersToAge)
	{
		if (!WorldManager.LegacyFoodTaxEnabled) yield break;
		EndOfMonthCutscenes.CutsceneTitle = MewtationsLoc.Translate("label_villager_age_birthday");
		EndOfMonthCutscenes.CutsceneText = "";
		yield return Cutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_age_villagers"));
		WorldManager.instance.EndOfMonthSpeedup = 0f;
		int num;
		for (int i = 0; i < villagersToAge.Count; i = num + 1)
		{
			BaseVillager baseVill = villagersToAge[i];
			WorldManager.instance.EndOfMonthSpeedup += 1f;
			LifeStage lifeStage = baseVill.DetermineLifeStageFromAge(baseVill.Age);
			baseVill.Age++;
			LifeStage newLifeStage = baseVill.DetermineLifeStageFromAge(baseVill.Age);
			if (lifeStage != newLifeStage || newLifeStage == LifeStage.Dead)
			{
				if (newLifeStage == LifeStage.Dead)
				{
					QuestManager.instance.SpecialActionComplete("villager_old_age_dead", null);
					EndOfMonthCutscenes.CutsceneTitle = MewtationsLoc.Translate("label_villager_old_age_death", new LocParam[] { LocParam.Create("villager", baseVill.Name) });
					yield return WorldManager.instance.KillVillagerCoroutine(baseVill, null, null, true);
					if (!WorldManager.instance.CheckAllVillagersDead())
					{
						if (WorldManager.instance.CardQuery.GetCardCount<BaseVillager>() > 1)
						{
							WorldManager.instance.Cutscene.QueueCutsceneIfNotPlayed("death_middle");
						}
						else
						{
							WorldManager.instance.Cutscene.QueueCutsceneIfNotPlayed("death_middle_villager");
						}
					}
				}
				else if (baseVill.ChangesCardOnStage)
				{
					string nextCardId = baseVill.DetermineCardFromStage(newLifeStage);
					GameCamera.instance.TargetPositionOverride = new Vector3?(baseVill.MyGameCard.transform.position);
					yield return new WaitForSeconds(1f);
					if (newLifeStage == LifeStage.Teenager)
					{
						AudioManager.me.PlaySound2D(AudioManager.me.BecomeTeenager, 1f, 0.3f);
					}
					if (newLifeStage == LifeStage.Adult)
					{
						AudioManager.me.PlaySound2D(AudioManager.me.BecomeAdult, 1f, 0.3f);
					}
					if (newLifeStage == LifeStage.Elderly)
					{
						AudioManager.me.PlaySound2D(AudioManager.me.BecomeOld, 1f, 0.3f);
						QuestManager.instance.SpecialActionComplete("villager_old", null);
					}
					WorldManager.instance.CreateSmoke(baseVill.transform.position);
					WorldManager.instance.ChangeToCard(baseVill.MyGameCard, nextCardId);
					yield return new WaitForSeconds(1f);
					nextCardId = null;
				}
				yield return new WaitForSeconds(EndOfMonthCutscenes.CalculateWaitFromSpeedup(WorldManager.instance.EndOfMonthSpeedup));
				baseVill = null;
			}
			num = i;
		}
		if (WorldManager.instance.CheckAllVillagersDead())
		{
			WorldManager.instance.VillagersStarvedAtEndOfMoon = true;
			yield return Cutscenes.EveryoneInSpiritWorldDead(WorldManager.instance.CurrentBoard.Id);
		}
		yield break;
	}

	public static IEnumerator KillAnimals(List<Animal> AnimalsToAge)
	{
		if (!WorldManager.LegacyFoodTaxEnabled) yield break;
		EndOfMonthCutscenes.CutsceneTitle = MewtationsLoc.Translate("label_animal_die");
		EndOfMonthCutscenes.CutsceneText = "";
		yield return Cutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		WorldManager.instance.EndOfMonthSpeedup = 4f;
		int num;
		for (int i = 0; i < AnimalsToAge.Count; i = num + 1)
		{
			Animal animal = AnimalsToAge[i];
			WorldManager.instance.EndOfMonthSpeedup += 1f;
			if (WorldManager.instance.Time.CurrentMonth - animal.CreationMonth >= 5)
			{
				EndOfMonthCutscenes.CutsceneTitle = MewtationsLoc.Translate("label_villager_old_age_death", new LocParam[] { LocParam.Create("villager", animal.Name) });
				yield return WorldManager.instance.KillVillagerCoroutine(animal, null, null, false);
			}
			yield return new WaitForSeconds(EndOfMonthCutscenes.CalculateWaitFromSpeedup(WorldManager.instance.EndOfMonthSpeedup));
			num = i;
		}
		GameCamera.instance.TargetPositionOverride = null;
		yield break;
	}

	public static List<CardData> GetCardsToFeed()
	{
		List<CardData> list = new List<CardData>();
		foreach (GameCard gameCard in WorldManager.instance.AllCards)
		{
			if (gameCard.MyBoard.IsCurrent)
			{
				CardData cardData = gameCard.CardData;
				if (cardData is BaseVillager)
				{
					list.Add(cardData);
				}
				if (cardData is Kid)
				{
					list.Add(cardData);
				}
			}
		}
		list = list.OrderBy<CardData, int>(delegate(CardData x)
		{
			if (x is Kid)
			{
				return 0;
			}
			if (x.Id == "dog" || x.Id == "cat")
			{
				return 1;
			}
			if (x is TeenageVillager)
			{
				return 2;
			}
			if (x is OldVillager)
			{
				return 4;
			}
			return 3;
		}).ToList<CardData>();
		return list;
	}

	public static IEnumerator FeedVillagers()
	{
		if (!WorldManager.LegacyFoodTaxEnabled) yield break;
		EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_time_to_eat");
		yield return Cutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_feed_villagers"));
		WorldManager.instance.InEatingAnimation = true;
		EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_eating");
		int requiredFoodCount = WorldManager.instance.GetRequiredFoodCount();
		List<CardData> cardsToFeed = EndOfMonthCutscenes.GetCardsToFeed();
		List<CardData> fedCards = new List<CardData>();
		yield return new WaitForSeconds(1f);
		WorldManager.instance.EndOfMonthSpeedup = 0f;
		int num;
		for (int i = 0; i < cardsToFeed.Count; i = num + 1)
		{
			CardData cardToFeed = cardsToFeed[i];
			BaseVillager baseVillager = cardToFeed as BaseVillager;
			if (baseVillager != null)
			{
				baseVillager.AteUncookedFood = false;
			}
			WorldManager.instance.EndOfMonthSpeedup += 1f;
			int foodForVillager = WorldManager.instance.GetCardRequiredFoodCount(cardToFeed.MyGameCard);
			for (int j = 0; j < foodForVillager; j = num + 1)
			{
				Food food = EndOfMonthCutscenes.GetFoodToUseUp();
				if (food == null)
				{
					break;
				}
				GameCard foodCard = food.MyGameCard;
				foodCard.PushEnabled = false;
				foodCard.SetY = false;
				foodCard.Velocity = null;
				GameCamera.instance.TargetPositionOverride = new Vector3?(cardToFeed.transform.position);
				GameCard originalParent = foodCard.Parent;
				GameCard originalChild = foodCard.Child;
				Vector3 originalPosition = foodCard.TargetPosition;
				List<GameCard> originalStack = foodCard.GetAllCardsInStack();
				foodCard.RemoveFromStack();
				foodCard.TargetPosition = cardToFeed.transform.position + new Vector3(0f, 0.1f, 0f);
				Vector3 diff;
				do
				{
					diff = foodCard.TargetPosition - foodCard.transform.position;
					yield return null;
				}
				while (diff.magnitude > 0.001f);
				AudioManager.me.PlaySound2D(AudioManager.me.Eat, Random.Range(0.8f, 1.2f), 0.3f);
				food.FoodValue--;
				num = requiredFoodCount;
				requiredFoodCount = num - 1;
				foodCard.SetHitEffect(null);
				foodCard.transform.localScale *= 0.9f;
				yield return new WaitForSeconds(EndOfMonthCutscenes.CalculateWaitFromSpeedup(WorldManager.instance.EndOfMonthSpeedup));
				BaseVillager baseVillager2 = cardToFeed as BaseVillager;
				if (baseVillager2 != null)
				{
					baseVillager2.HealthPoints = Mathf.Min(baseVillager2.HealthPoints + 3, baseVillager2.ProcessedCombatStats.MaxHealth);
					food.ConsumedBy(baseVillager2);
					EndOfMonthCutscenes.TryCreatePoop(baseVillager2);
					if (!food.IsCookedFood)
					{
						baseVillager2.AteUncookedFood = true;
					}
				}
				if (food.FoodValue <= 0 && !(food is Hotpot))
				{
					food.FullyConsumed(cardToFeed);
					originalStack.Remove(foodCard);
					WorldManager.instance.Restack(originalStack);
					foodCard.DestroyCard(true, true);
				}
				else
				{
					foodCard.PushEnabled = true;
					foodCard.SetY = true;
					if (originalParent != null)
					{
						foodCard.SetParent(originalParent);
					}
					if (originalChild != null)
					{
						foodCard.SetChild(originalChild);
					}
					foodCard.TargetPosition = originalPosition;
				}
				if (j == foodForVillager - 1)
				{
					fedCards.Add(cardToFeed);
				}
				foodCard = null;
				food = null;
				originalParent = null;
				originalChild = null;
				originalPosition = default(Vector3);
				originalStack = null;
				diff = default(Vector3);
				num = j;
			}
			cardToFeed = null;
			num = i;
		}
		yield return new WaitForSeconds(1f);
		WorldManager.instance.InEatingAnimation = false;
		int num2 = requiredFoodCount;
		List<CardData> unfedVillagers = new List<CardData>();
		foreach (CardData cardData in cardsToFeed)
		{
			if (!fedCards.Contains(cardData) && !(cardData is Kid))
			{
				unfedVillagers.Add(cardData);
			}
		}
		int humansToDie = unfedVillagers.Count;
		if (num2 <= 0)
		{
			EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_everyone_fed");
		}
		else
		{
			EndOfMonthCutscenes.SetStarvingHumanStatus(humansToDie);
			yield return Cutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_uh_oh"));
			for (int i = 0; i < unfedVillagers.Count; i = num + 1)
			{
				CardData cardData2 = unfedVillagers[i];
				if (!(cardData2 is Kid))
				{
					yield return WorldManager.instance.KillVillagerCoroutine(cardData2 as BaseVillager, null, null, true);
					EndOfMonthCutscenes.SetStarvingHumanStatus(humansToDie - i);
				}
				num = i;
			}
			if (WorldManager.instance.CheckAllVillagersDead())
			{
				WorldManager.instance.VillagersStarvedAtEndOfMoon = true;
				if (WorldManager.instance.CurrentBoard.Id == "main")
				{
					EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_everyone_starved");
					yield return Cutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_game_over"));
					GameCanvas.instance.SetScreen<GameOverScreen>();
					WorldManager.instance.currentAnimationRoutine = null;
				}
				else if (WorldManager.instance.CurrentBoard.Id == "island")
				{
					yield return Cutscenes.EveryoneOnIslandDead();
				}
				else if (WorldManager.instance.CurrentBoard.Id == "forest")
				{
					yield return Cutscenes.EveryoneInForestDead();
				}
				else if (WorldManager.instance.CurrentBoard.BoardOptions.IsSpiritWorld)
				{
					yield return Cutscenes.EveryoneInSpiritWorldDead(WorldManager.instance.CurrentBoard.Id);
				}
				else if (!(WorldManager.instance.CurrentBoard.Id == "cities"))
				{
					yield return Cutscenes.EveryoneOnIslandDead();
				}
			}
		}
		yield break;
	}

	private static void TryCreatePoop(CardData vill)
	{
		if (WorldManager.instance.CurseIsActive(CurseType.Death))
		{
			Poop poop = WorldManager.instance.CreateCard(vill.transform.position, "human_poop", true, false, true) as Poop;
			AudioManager.me.PlaySound2D(poop.PoopSound, 1f, 0.5f);
			WorldManager.instance.StackSend(poop.MyGameCard, vill.OutputDir, null, true);
		}
	}

	private static List<CardData> CardsThatNeedHappiness()
	{
		List<CardData> list = new List<CardData>();
		foreach (GameCard gameCard in WorldManager.instance.AllCards)
		{
			if (gameCard.MyBoard.IsCurrent && WorldManager.instance.GetCardRequiredHappinessCount(gameCard) > 0)
			{
				list.Add(gameCard.CardData);
			}
		}
		list = list.OrderBy<CardData, int>(delegate(CardData x)
		{
			if (!(x is Unhappiness))
			{
				ResourceChest resourceChest = x as ResourceChest;
				if (resourceChest == null || !(resourceChest.HeldCardId == "unhappiness"))
				{
					if (x is Kid)
					{
						return 0;
					}
					if (x.Id == "dog")
					{
						return 1;
					}
					return 3;
				}
			}
			return -1;
		}).ToList<CardData>();
		return list;
	}

	public static IEnumerator HappinessWarning()
	{
		EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_happiness_warning", new LocParam[] { LocParam.Create("count", WorldManager.instance.GetRequiredHappinessCount().ToString()) });
		yield return Cutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		yield break;
	}

	public static IEnumerator NewVillagerBecauseOfHappiness()
	{
		CardData cardData = WorldManager.instance.CreateCard(WorldManager.instance.GetRandomSpawnPosition(), "villager", true, false, true);
		GameCamera.instance.TargetPositionOverride = new Vector3?(cardData.transform.position);
		EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_new_villager_happiness");
		yield return Cutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		yield break;
	}

	public static IEnumerator NewVillagerSpawnsInDeath()
	{
		CardData cardData = WorldManager.instance.CreateCard(WorldManager.instance.GetRandomSpawnPosition(), "villager", true, false, true);
		GameCamera.instance.TargetPositionOverride = new Vector3?(cardData.transform.position);
		EndOfMonthCutscenes.CutsceneTitle = MewtationsLoc.Translate("label_new_villager");
		EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_new_villager_death");
		yield return Cutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		yield break;
	}

	public static IEnumerator IndustrialRevolutionEvent()
	{
		EndOfMonthCutscenes.CutsceneTitle = MewtationsLoc.Translate("label_cutscene_industrial_revolution_title");
		EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_cutscene_industrial_revolution_text");
		yield return Cutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_nice"));
		CardData cardData = WorldManager.instance.CreateCard(WorldManager.instance.GetRandomSpawnPosition(), "event_industrial_revolution", true, false, true);
		WorldManager.instance.CreateSmoke(cardData.Position);
		GameCamera.instance.TargetPositionOverride = new Vector3?(cardData.transform.position);
		EndOfMonthCutscenes.CutsceneTitle = MewtationsLoc.Translate("label_cutscene_industrial_revolution_title");
		EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_cutscene_industrial_revolution_text_1");
		yield return Cutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		yield break;
	}

	private static List<GameCard> GetHappinessProviders()
	{
		List<GameCard> list = (from x in WorldManager.instance.CardQuery.GetCards<Happiness>()
			select x.MyGameCard).ToList<GameCard>();
		IEnumerable<GameCard> enumerable = WorldManager.instance.CardQuery.GetAllCardsOnBoard(WorldManager.instance.CurrentBoard.Id).Where<GameCard>(delegate(GameCard x)
		{
			ResourceChest resourceChest = x.CardData as ResourceChest;
			return resourceChest != null && resourceChest.HeldCardId == "happiness" && resourceChest.ResourceCount > 0;
		});
		list.AddRange(enumerable);
		return list.OrderBy<GameCard, int>(delegate(GameCard x)
		{
			if (x.CardData is ResourceChest)
			{
				return 1;
			}
			GameCard rootCard = x.GetRootCard();
			if (rootCard != null && rootCard.TimerRunning)
			{
				return -1;
			}
			return 0;
		}).ToList<GameCard>();
	}

	public static IEnumerator UseHappiness()
	{
		if (!WorldManager.LegacyFoodTaxEnabled) yield break;
		EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_giving_happiness");
		List<CardData> list = EndOfMonthCutscenes.CardsThatNeedHappiness();
		List<GameCard> happinessProviders = EndOfMonthCutscenes.GetHappinessProviders();
		List<ValueTuple<CardData, int>> unhappyCards = new List<ValueTuple<CardData, int>>();
		WorldManager.instance.EndOfMonthSpeedup = 0f;
		int missingUnhappiness = 0;
		foreach (CardData needsHappiness in list)
		{
			int takenHappiness = 0;
			int requiredHappinessForCard = WorldManager.instance.GetCardRequiredHappinessCount(needsHappiness.MyGameCard);
			int num;
			for (int i = 0; i < requiredHappinessForCard; i = num + 1)
			{
				for (int j = happinessProviders.Count - 1; j >= 0; j = num - 1)
				{
					GameCard happinessCard = happinessProviders[j];
					happinessCard.PushEnabled = false;
					happinessCard.SetY = false;
					happinessCard.Velocity = null;
					GameCamera.instance.TargetPositionOverride = new Vector3?(needsHappiness.transform.position);
					GameCard originalParent = happinessCard.Parent;
					GameCard originalChild = happinessCard.Child;
					Vector3 originalPosition = happinessCard.TargetPosition;
					happinessCard.GetAllCardsInStack();
					happinessCard.RemoveFromStack();
					happinessCard.TargetPosition = needsHappiness.transform.position + new Vector3(0f, 0.1f, 0f);
					Vector3 diff;
					do
					{
						diff = happinessCard.TargetPosition - happinessCard.transform.position;
						yield return null;
					}
					while (diff.magnitude > 0.001f);
					AudioManager.me.PlaySound2D(AudioManager.me.ConsumeHappiness, Random.Range(0.8f, 1.2f), 0.1f);
					happinessCard.SetHitEffect(null);
					happinessCard.transform.localScale *= 0.9f;
					yield return new WaitForSeconds(EndOfMonthCutscenes.CalculateWaitFromSpeedup(WorldManager.instance.EndOfMonthSpeedup));
					num = takenHappiness;
					takenHappiness = num + 1;
					if (happinessCard.CardData is Happiness)
					{
						happinessProviders.RemoveAt(j);
						happinessCard.DestroyCard(true, true);
						WorldManager.instance.EndOfMonthSpeedup += 1f;
						break;
					}
					ResourceChest resourceChest = happinessCard.CardData as ResourceChest;
					if (resourceChest != null)
					{
						resourceChest.ResourceCount--;
						if (resourceChest.ResourceCount <= 0)
						{
							happinessProviders.RemoveAt(j);
						}
						happinessCard.PushEnabled = true;
						happinessCard.SetY = true;
						if (originalParent != null)
						{
							happinessCard.SetParent(originalParent);
						}
						if (originalChild != null)
						{
							happinessCard.SetChild(originalChild);
						}
						happinessCard.TargetPosition = originalPosition;
						WorldManager.instance.EndOfMonthSpeedup += 1f;
						break;
					}
					happinessCard = null;
					originalParent = null;
					originalChild = null;
					originalPosition = default(Vector3);
					diff = default(Vector3);
					num = j;
				}
				num = i;
			}
			if (needsHappiness is BaseVillager && takenHappiness < requiredHappinessForCard)
			{
				GameCamera.instance.TargetPositionOverride = new Vector3?(needsHappiness.transform.position);
				unhappyCards.Add(new ValueTuple<CardData, int>(needsHappiness, requiredHappinessForCard - takenHappiness));
			}
			if (needsHappiness is Unhappiness)
			{
				if (takenHappiness >= requiredHappinessForCard)
				{
					needsHappiness.MyGameCard.DestroyCard(true, true);
					AudioManager.me.PlaySound2D(AudioManager.me.CancelSadness, 1f, 0.5f);
				}
				else
				{
					missingUnhappiness += requiredHappinessForCard - takenHappiness;
				}
			}
			needsHappiness = null;
		}
		List<CardData>.Enumerator enumerator = default(List<CardData>.Enumerator);
		if (unhappyCards.Count > 0)
		{
			WorldManager.instance.CurrentRunVariables.VillagersHappyMonthCount = 0;
			WorldManager.instance.CurrentRunVariables.VillagersUnhappyMonthCount++;
			EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_not_everyone_happy");
			yield return Cutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_uh_oh"));
			foreach (ValueTuple<CardData, int> valueTuple in unhappyCards)
			{
				if (valueTuple.Item1 is BaseVillager)
				{
					if (WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
					{
						BaseVillager baseVillager = valueTuple.Item1 as BaseVillager;
						if (baseVillager != null)
						{
							baseVillager.Damage(3);
						}
						GameCamera.instance.TargetCardOverride = valueTuple.Item1;
						yield return new WaitForSeconds(0.5f);
					}
					else
					{
						float num2 = (float)valueTuple.Item2 * 2.5f;
						num2 += (float)missingUnhappiness / (float)unhappyCards.Count * 3f;
						num2 += (float)WorldManager.instance.CurrentRunVariables.VillagersUnhappyMonthCount * 10f;
						foreach (CardIdWithEquipment cardIdWithEquipment in SpawnHelper.GetEnemiesToSpawn(SetCardBagType.Happiness_Enemy.AsList<SetCardBagType>(), num2, true))
						{
							WorldManager.instance.CreateCard(valueTuple.Item1.transform.position, cardIdWithEquipment, true, false, true).MyGameCard.SendIt();
						}
						GameCamera.instance.TargetCardOverride = valueTuple.Item1;
						yield return new WaitForSeconds(0.5f);
					}
				}
			}
			List<ValueTuple<CardData, int>>.Enumerator enumerator2 = default(List<ValueTuple<CardData, int>>.Enumerator);
		}
		else
		{
			WorldManager.instance.CurrentRunVariables.VillagersHappyMonthCount++;
			if (WorldManager.instance.CurrentRunVariables.VillagersHappyMonthCount >= 4 && WorldManager.instance.CardQuery.GetCardCount<BaseVillager>() < 3)
			{
				WorldManager.instance.CurrentRunVariables.VillagersHappyMonthCount = 0;
				yield return EndOfMonthCutscenes.NewVillagerBecauseOfHappiness();
				EndOfMonthCutscenes.CutsceneText = "";
			}
		}
		if (WorldManager.instance.CheckAllVillagersDead())
		{
			WorldManager.instance.VillagersAngryAtEndOfMoon = true;
			if (WorldManager.instance.CurrentBoard.Id == "main")
			{
				EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_everyone_angry");
				yield return Cutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_game_over"));
				GameCanvas.instance.SetScreen<GameOverScreen>();
				WorldManager.instance.currentAnimationRoutine = null;
			}
			else if (WorldManager.instance.CurrentBoard.Id == "island")
			{
				yield return Cutscenes.EveryoneOnIslandDead();
			}
			else if (WorldManager.instance.CurrentBoard.Id == "forest")
			{
				yield return Cutscenes.EveryoneInForestDead();
			}
			else if (WorldManager.instance.CurrentBoard.BoardOptions.IsSpiritWorld)
			{
				yield return Cutscenes.EveryoneInSpiritWorldDead(WorldManager.instance.CurrentBoard.Id);
			}
			else if (!(WorldManager.instance.CurrentBoard.Id == "cities"))
			{
				yield return Cutscenes.EveryoneOnIslandDead();
			}
		}
		yield break;
		yield break;
	}

	private static void SetStarvingHumanStatus(int deathCount)
	{
		EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_starving_humans", new LocParam[] { LocParam.Plural("count", deathCount) });
	}

	private static bool AnyCardCanBeSold()
	{
		foreach (GameCard gameCard in WorldManager.instance.AllCards)
		{
			if (gameCard.MyBoard.IsCurrent && !gameCard.IsEquipped)
			{
				MonthlyRequirementResult monthlyRequirementResult = gameCard.CardData.MonthlyRequirementResult;
				bool flag;
				if (monthlyRequirementResult == null)
				{
					flag = false;
				}
				else
				{
					Dictionary<string, MonthlyResult> results = monthlyRequirementResult.results;
					int? num = ((results != null) ? new int?(results.Count) : null);
					int num2 = 0;
					flag = (num.GetValueOrDefault() > num2) & (num != null);
				}
				if (!flag && gameCard.CardData.GetValue() != -1)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static IEnumerator MaxCardCount()
	{
		int cardCount = WorldManager.instance.GetCardCount();
		int maxCardCount = WorldManager.instance.GetMaxCardCount(WorldManager.instance.CurrentBoard);
		int num = cardCount - maxCardCount;
		if (!EndOfMonthCutscenes.AnyCardCanBeSold())
		{
			num = 0;
		}
		if (num > 0)
		{
			EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_too_many_cards", new LocParam[] { LocParam.Plural("count", num) });
			string text = MewtationsLoc.Translate("label_sell_x_cards", new LocParam[] { LocParam.Plural("count", num) });
			yield return Cutscenes.WaitForContinueClicked(text);
			WorldManager.instance.RemovingCards = true;
			while (WorldManager.instance.GetCardCount() > WorldManager.instance.GetMaxCardCount(WorldManager.instance.CurrentBoard))
			{
				GameCamera.instance.TargetPositionOverride = null;
				int num2 = WorldManager.instance.GetCardCount() - WorldManager.instance.GetMaxCardCount(WorldManager.instance.CurrentBoard);
				if (!EndOfMonthCutscenes.AnyCardCanBeSold())
				{
					break;
				}
				EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_too_many_cards", new LocParam[] { LocParam.Plural("count", num2) });
				EndOfMonthCutscenes.CutsceneTitle = MewtationsLoc.Translate("label_sell_cards_to_continue");
				yield return null;
			}
			int num3 = Mathf.Max(0, WorldManager.instance.GetCardCount() - WorldManager.instance.GetMaxCardCount(WorldManager.instance.CurrentBoard));
			EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_too_many_cards", new LocParam[] { LocParam.Plural("count", num3) });
			EndOfMonthCutscenes.CutsceneTitle = MewtationsLoc.Translate("label_sell_cards_to_continue");
			WorldManager.instance.RemovingCards = false;
		}
		yield break;
	}

	public static IEnumerator SpecialEvents()
	{
		EndOfMonthCutscenes.CutsceneTitle = "";
		EndOfMonthCutscenes.CutsceneText = "";
		bool flag = EndOfMonthCutscenes.CurrentMonth > 8 && EndOfMonthCutscenes.CurrentMonth % 4 == 0;
		bool spawnTravellingCart = (Random.value <= 0.1f && EndOfMonthCutscenes.CurrentMonth >= 8 && EndOfMonthCutscenes.CurrentMonth % 2 == 1) || EndOfMonthCutscenes.CurrentMonth == 19;
		bool spawnPirateBoat = WorldManager.instance.BoardMonths.IslandMonth % 7 == 0 && WorldManager.instance.CurrentBoard.BoardOptions.CanSpawnPirateBoat;
		bool spawnShaman = (WorldManager.instance.CurrentRunVariables.FinishedDemon || QuestManager.instance.QuestIsComplete("kill_demon")) && WorldManager.instance.IsSpiritDlcActive() && !WorldManager.instance.CurrentRunVariables.ShamanVisited;
		bool spawnSadEvent = WorldManager.instance.CurrentBoard.Id == "happiness" && EndOfMonthCutscenes.CurrentMonth > 4 && EndOfMonthCutscenes.CurrentMonth % 4 == 0;
		if (WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
		{
			flag = false;
			spawnPirateBoat = false;
		}
		if (!WorldManager.instance.CurrentBoard.BoardOptions.CanSpawnPirateBoat)
		{
			spawnPirateBoat = false;
		}
		if (!WorldManager.instance.CurrentBoard.BoardOptions.CanSpawnPortals)
		{
			flag = false;
		}
		if (!WorldManager.instance.CurrentBoard.BoardOptions.CanSpawnTravellingCart)
		{
			spawnTravellingCart = false;
		}
		if (!WorldManager.instance.CurrentBoard.BoardOptions.CanSpawnShaman || (WorldManager.instance.HasFoundCard("blueprint_altar") && WorldManager.instance.HasFoundCard("greed_recipe") && WorldManager.instance.HasFoundCard("happiness_recipe") && WorldManager.instance.HasFoundCard("death_recipe")))
		{
			spawnShaman = false;
		}
		if (flag)
		{
			WorldManager.instance.CurrentRunVariables.StrangePortalSpawns++;
			Vector3 randomSpawnPosition = WorldManager.instance.GetRandomSpawnPosition();
			CardData cardData;
			if (WorldManager.instance.CurrentRunVariables.StrangePortalSpawns % 4 == 0)
			{
				EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_strange_portal_appeared_strong");
				cardData = WorldManager.instance.CreateCard(randomSpawnPosition, "rare_portal", true, false, true);
			}
			else
			{
				cardData = WorldManager.instance.CreateCard(randomSpawnPosition, "strange_portal", true, false, true);
			}
			EndOfMonthCutscenes.CutsceneTitle = MewtationsLoc.Translate("label_strange_portal_appeared");
			if (cardData != null)
			{
				GameCamera.instance.TargetPositionOverride = new Vector3?(cardData.transform.position);
			}
			yield return new WaitForSeconds(2f);
			GameCamera.instance.TargetPositionOverride = null;
			yield return Cutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_uh_oh"));
		}
		if (spawnPirateBoat)
		{
			WorldManager.instance.ShowContinueButton = false;
			Vector3 randomSpawnPosition2 = WorldManager.instance.GetRandomSpawnPosition();
			CardData cardData2 = WorldManager.instance.CreateCard(randomSpawnPosition2, "pirate_boat", true, false, true);
			EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_pirate_boat_appeared");
			GameCamera.instance.TargetPositionOverride = new Vector3?(cardData2.transform.position);
			yield return new WaitForSeconds(2f);
			GameCamera.instance.TargetPositionOverride = null;
			WorldManager.instance.CurrentRunVariables.PirateBoatsSpawned++;
			yield return Cutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_uh_oh"));
		}
		if (spawnTravellingCart)
		{
			WorldManager.instance.ShowContinueButton = false;
			Vector3 randomSpawnPosition3 = WorldManager.instance.GetRandomSpawnPosition();
			CardData cardData3 = WorldManager.instance.CreateCard(randomSpawnPosition3, "travelling_cart", true, false, true);
			EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_travelling_cart_appeared");
			GameCamera.instance.TargetPositionOverride = new Vector3?(cardData3.transform.position);
			yield return new WaitForSeconds(2f);
			GameCamera.instance.TargetPositionOverride = null;
			yield return Cutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		}
		if (spawnShaman)
		{
			EndOfMonthCutscenes.CutsceneTitle = MewtationsLoc.Translate("label_shaman_intro_title");
			WorldManager.instance.CurrentRunVariables.ShamanVisited = true;
			WorldManager.instance.ShowContinueButton = false;
			Vector3 randomPos = WorldManager.instance.GetRandomSpawnPosition();
			GameCamera.instance.TargetPositionOverride = new Vector3?(randomPos);
			EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_shaman_intro");
			yield return new WaitForSeconds(2f);
			CardData cardData4 = WorldManager.instance.CreateCard(randomPos, "shaman", true, false, true);
			AudioManager.me.PlaySound2D(AudioManager.me.ShamanSpawn, 1f, 0.2f);
			WorldManager.instance.CreateSmoke(randomPos);
			GameCamera.instance.TargetPositionOverride = new Vector3?(cardData4.transform.position);
			EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_shaman_appeared");
			yield return Cutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_wow"));
			if (!WorldManager.instance.CurrentRunVariables.VisitedIsland || !QuestManager.instance.AnyIslandQuestComplete())
			{
				if (WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
				{
					EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_shaman_intro_peaceful");
				}
				else
				{
					EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_shaman_intro_demon");
				}
				yield return Cutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_nice"));
				EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_shaman_intro_island");
				yield return Cutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
			}
			EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_shaman_intro_cursed");
			yield return Cutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
			GameCamera.instance.TargetPositionOverride = null;
			randomPos = default(Vector3);
		}
		if (spawnSadEvent)
		{
			WorldManager.instance.ShowContinueButton = false;
			Vector3 randomSpawnPosition4 = WorldManager.instance.GetRandomSpawnPosition();
			CardData cardData5 = WorldManager.instance.CreateCard(randomSpawnPosition4, "sad_event", true, false, true);
			EndOfMonthCutscenes.CutsceneText = MewtationsLoc.Translate("label_sad_event_appeared");
			GameCamera.instance.TargetPositionOverride = new Vector3?(cardData5.transform.position);
			yield return new WaitForSeconds(2f);
			GameCamera.instance.TargetPositionOverride = null;
			yield return Cutscenes.WaitForContinueClicked(MewtationsLoc.Translate("label_okay"));
		}
		yield break;
	}
}
