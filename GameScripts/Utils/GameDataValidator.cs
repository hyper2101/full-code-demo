using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameDataValidator
{
	public GameDataValidator(GameDataLoader gameDataLoader)
	{
		this.GameDataLoader = gameDataLoader;
	}

	public ValidationResult Validate()
	{
		ValidationResult validationResult = new ValidationResult();
		this.VerifyBlueprints(validationResult);
		this.CheckDuplicateBlueprints(validationResult);
		this.CheckSetCardBags(validationResult);
		this.CheckCardBags(validationResult);
		this.VerifyBoosterPacks(validationResult);
		this.CheckCardTerms(validationResult);
		this.VerifyAllCardsReferenced(validationResult);
		this.VerifyQuests(validationResult);
		this.CalculateExpectedValues(validationResult);
		this.CheckCardDataUsage(validationResult);
		this.CheckDefaultAudio(validationResult);
		this.VerifyLegacyPurge(validationResult);
		return validationResult;
	}

	private void CalculateExpectedValues(ValidationResult validationResult)
	{
		foreach (BoosterpackData boosterpackData in this.GameDataLoader.BoosterpackDatas)
		{
			for (int i = 0; i < boosterpackData.CardBags.Count; i++)
			{
				CardBag cardBag = boosterpackData.CardBags[i];
				try
				{
					cardBag.CalculateExpectedValueForBag(this.GameDataLoader);
				}
				catch (Exception ex)
				{
					validationResult.AddError(null, string.Format("Error when processing bag {0} in booster {1}", i, boosterpackData.BoosterId), ValidationCategory.ExpectedValues);
					Debug.LogException(ex);
				}
			}
			boosterpackData.ExpectedValue = boosterpackData.CardBags.Sum<CardBag>((CardBag x) => x.ExpectedValue * (float)((x.CardBagType == CardBagType.SetPack) ? x.SetPackCards.Count : x.CardsInPack));
		}
	}

	private void VerifyQuests(ValidationResult validationResult)
	{
		foreach (Quest quest in QuestManager.GetAllQuests())
		{
			string text = quest.DescriptionTerm;
			if (quest.DescriptionTermOverride != null)
			{
				text = quest.DescriptionTermOverride;
			}
			if (!SokLoc.FallbackSet.ContainsTerm(text))
			{
				validationResult.AddError(null, string.Concat(new string[] { "Quest ", quest.Id, " has an invalid DescriptionTerm (", quest.DescriptionTerm, ")" }), ValidationCategory.Quests);
			}
		}
	}

	private void CheckCardTerms(ValidationResult validationResult)
	{
		foreach (CardData cardData in this.GameDataLoader.CardDataPrefabs)
		{
			if (!SokLoc.FallbackSet.ContainsTerm(cardData.NameTerm))
			{
				validationResult.AddError(cardData, cardData.Id + " has an invalid NameTerm", ValidationCategory.CardTerms);
			}
			if (!string.IsNullOrWhiteSpace(cardData.DescriptionTerm) && !SokLoc.FallbackSet.ContainsTerm(cardData.DescriptionTerm))
			{
				validationResult.AddError(cardData, cardData.Id + " has an invalid DescriptionTerm", ValidationCategory.CardTerms);
			}
		}
	}

	private void VerifyAllCardsReferenced(ValidationResult validationResult)
	{
		List<ICardReference> list = this.GameDataLoader.DetermineCardReferences();
		using (List<CardData>.Enumerator enumerator = this.GameDataLoader.CardDataPrefabs.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				CardData cd = enumerator.Current;
				Blueprint blueprint = cd as Blueprint;
				if ((blueprint == null || !blueprint.HideFromCardopedia) && !list.Any<ICardReference>((ICardReference x) => x.ReferencedCardId == cd.Id))
				{
					validationResult.AddError(cd, "Card " + cd.Id + " is never referenced", ValidationCategory.CardReferences);
				}
			}
		}
	}

	private void CheckCardDataUsage(ValidationResult validationResult)
	{
		foreach (CardData cardData in this.GameDataLoader.CardDataPrefabs)
		{
			Blueprint blueprint = cardData as Blueprint;
			if ((blueprint == null || !blueprint.HideFromCardopedia) && cardData.GetType() == typeof(CardData))
			{
				validationResult.AddError(cardData, "Card " + cardData.Id + " has base CardData class", ValidationCategory.CardClasses);
			}
		}
	}

	public void CheckStackOrders()
	{
		foreach (Blueprint blueprint in this.GameDataLoader.BlueprintPrefabs)
		{
			foreach (Subprint subprint in blueprint.Subprints)
			{
				this.CheckSubprintStackOrder(subprint);
			}
		}
	}

	private void CheckSubprintStackOrder(Subprint sp)
	{
		if (sp.RequiredCards.Length >= 7)
		{
			Debug.Log(string.Format("Did not check blueprint {0} subprint {1}", sp.ParentBlueprint.Name, sp.SubprintIndex));
			return;
		}
		List<CardData> list = new List<CardData>();
		foreach (string text in sp.RequiredCards)
		{
			string text2 = text;
			if (text2 == "any_villager" || text2 == "breedable_villager" || text2 == "any_villager_old" || text2 == "any_villager_young")
			{
				text2 = "villager";
			}
			if (text2 == "any_worker")
			{
				text2 = "worker";
			}
			if (text2.Contains('|'))
			{
				text2 = text.Split('|', StringSplitOptions.None)[0];
			}
			list.Add(WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), text2, true, false, true));
		}
		foreach (IEnumerable<CardData> enumerable in list.Permute<CardData>())
		{
			List<CardData> list2 = enumerable.ToList<CardData>();
			for (int j = 0; j < list2.Count - 1; j++)
			{
				if (list2[j].CanHaveCardOnTop(list2[j + 1], false))
				{
					list2[j].MyGameCard.SetChild(list2[j + 1].MyGameCard);
				}
				else if (list2[j + 1].MyCardType != CardType.Structures)
				{
					Debug.LogError(string.Concat(new string[]
					{
						list2[j + 1].Id,
						" can not go on top of ",
						list2[j].Id,
						" for blueprint ",
						sp.ParentBlueprint.Id
					}));
					break;
				}
			}
			foreach (CardData cardData in list2)
			{
				cardData.MyGameCard.RemoveFromStack();
			}
		}
		foreach (CardData cardData2 in list)
		{
			cardData2.MyGameCard.DestroyCard(false, true);
		}
	}

	private void CheckCardBags(ValidationResult validationResult)
	{
		foreach (CardData cardData in this.GameDataLoader.CardDataPrefabs)
		{
			foreach (CardBag cardBag in cardData.GetCardBags())
			{
				foreach (string text in cardBag.GetCardsInBag(this.GameDataLoader))
				{
					if (this.GameDataLoader.GetCardFromId(text, false) == null)
					{
						validationResult.AddError(cardData, "Invalid card id in " + cardData.Id + " " + text, ValidationCategory.CardBag);
					}
				}
			}
		}
	}

	private void VerifyBoosterPacks(ValidationResult validationResult)
	{
		foreach (BoosterpackData boosterpackData in this.GameDataLoader.BoosterpackDatas)
		{
			for (int i = 0; i < boosterpackData.CardBags.Count; i++)
			{
				foreach (string text in boosterpackData.CardBags[i].GetCardsInBag(this.GameDataLoader))
				{
					if (this.GameDataLoader.GetCardFromId(text, false) == null)
					{
						validationResult.AddError(null, string.Format("Invalid card id in {0} - bag {1}: {2}", boosterpackData.BoosterId, i, text), ValidationCategory.BoosterPacks);
					}
				}
			}
		}
	}

	private void VerifyBlueprints(ValidationResult validationResult)
	{
		foreach (Blueprint blueprint in this.GameDataLoader.BlueprintPrefabs)
		{
			if (blueprint.MyCardType != CardType.Ideas)
			{
				validationResult.AddError(blueprint, "Blueprint " + blueprint.Id + " is not set to card type Ideas", ValidationCategory.Blueprints);
			}
			for (int i = 0; i < blueprint.Subprints.Count; i++)
			{
				Subprint subprint = blueprint.Subprints[i];
				if (string.IsNullOrEmpty(subprint.ResultAction))
				{
					List<string> list = new List<string>(subprint.ExtraResultCards);
					if (subprint.ExtraResultCards.Length == 0 && subprint.ResultCard != "")
					{
						list.Add(subprint.ResultCard);
					}
					if (subprint.RequiredCards.Length == 0)
					{
						validationResult.AddError(blueprint, string.Format("Blueprint {0} has no required cards in subprint {1}", blueprint.Id, i), ValidationCategory.Blueprints);
					}
					string[] array = subprint.RequiredCards;
					for (int j = 0; j < array.Length; j++)
					{
						string[] array2 = array[j].Split('|', StringSplitOptions.None);
						list.AddRange(array2);
					}
					if (subprint.CardsToRemove != null)
					{
						array = subprint.CardsToRemove;
						for (int j = 0; j < array.Length; j++)
						{
							string[] array3 = array[j].Split('|', StringSplitOptions.None);
							list.AddRange(array3);
						}
					}
					foreach (string text in list)
					{
						if (this.GameDataLoader.GetCardFromId(text, false) == null)
						{
							validationResult.AddError(blueprint, string.Format("Blueprint {0} has an invalid card id (id: {1}) (subprint {2})", blueprint.Id, text, subprint.SubprintIndex), ValidationCategory.Blueprints);
						}
					}
					if (!SokLoc.FallbackSet.ContainsTerm(subprint.StatusTerm))
					{
						validationResult.AddError(blueprint, "Blueprint " + blueprint.Id + " has an invalid status term", ValidationCategory.Blueprints);
					}
				}
			}
		}
	}

	private void CheckDuplicateBlueprints(ValidationResult validationResult)
	{
		new List<ValidationError>();
		Dictionary<string, Blueprint> dictionary = new Dictionary<string, Blueprint>();
		foreach (Blueprint blueprint in this.GameDataLoader.BlueprintPrefabs)
		{
			foreach (Subprint subprint in blueprint.Subprints)
			{
				string text = string.Join("-", subprint.RequiredCards.OrderBy<string, string>((string x) => x));
				if (dictionary.ContainsKey(text))
				{
					validationResult.AddError(blueprint, "Blueprint " + blueprint.gameObject.name + " has a clashing subprint with " + dictionary[text].gameObject.name, ValidationCategory.BlueprintDuplicates);
				}
				else
				{
					dictionary.Add(text, blueprint);
				}
			}
		}
	}

	public void CheckSetCardBags(ValidationResult validationResult)
	{
		foreach (SetCardBagData setCardBagData in this.GameDataLoader.SetCardBags)
		{
			SetCardBagType setCardBagType = setCardBagData.SetCardBagType;
			foreach (CardChance cardChance in CardBag.GetChancesForSetCardBag(this.GameDataLoader, setCardBagType, null))
			{
				if (this.GameDataLoader.GetCardFromId(cardChance.Id, true) == null)
				{
					validationResult.AddError(setCardBagData, cardChance.Id + " doesn't exist", ValidationCategory.SetCardBag);
				}
			}
		}
	}

	public void CheckDefaultAudio(ValidationResult validationResult)
	{
		foreach (CardData cardData in this.GameDataLoader.CardDataPrefabs)
		{
			if (!cardData.name.StartsWith("Misc_"))
			{
				if (cardData.PickupSoundGroup == PickupSoundGroup.Custom)
				{
					if (cardData.PickupSound == null)
					{
						validationResult.AddError(cardData, "Card " + cardData.Id + " has custom sound without an audio source", ValidationCategory.CardAudio);
					}
				}
				else if (!this.HasCorrectAudio(cardData))
				{
					validationResult.AddError(cardData, "Card " + cardData.Id + " doesn't have the correct audio", ValidationCategory.CardAudio);
				}
			}
		}
	}

	private bool HasCorrectAudio(CardData card)
	{
		return (card.MyCardType != CardType.Structures || card.PickupSoundGroup == PickupSoundGroup.Heavy || card.PickupSoundGroup == PickupSoundGroup.Medium) && (card.MyCardType != CardType.Food || card.PickupSoundGroup == PickupSoundGroup.Medium) && (card.MyCardType != CardType.Resources || card.PickupSoundGroup == PickupSoundGroup.Heavy || card.PickupSoundGroup == PickupSoundGroup.Medium) && (card.MyCardType != CardType.Mobs || card.PickupSoundGroup == PickupSoundGroup.Default) && (card.MyCardType != CardType.Ideas || card.PickupSoundGroup == PickupSoundGroup.Default) && (card.MyCardType != CardType.Equipable || card.PickupSoundGroup == PickupSoundGroup.Default) && (card.MyCardType != CardType.Locations || card.PickupSoundGroup == PickupSoundGroup.Heavy);
	}

	private void VerifyLegacyPurge(ValidationResult validationResult)
	{
		foreach (CardData cardData in this.GameDataLoader.CardDataPrefabs)
		{
			if (cardData != null)
			{
				var attribute = System.Attribute.GetCustomAttribute(cardData.GetType(), typeof(Mewtations.Core.LegacySystemAttribute)) as Mewtations.Core.LegacySystemAttribute;
				if (attribute != null || cardData is Mewtations.Core.ILegacySystemMarker)
				{
					string category = attribute != null ? attribute.Category.ToString() : "Unknown";
					validationResult.AddError(cardData, $"Card '{cardData.Id}' uses legacy quarantined class '{cardData.GetType().Name}' (Category: {category})!", ValidationCategory.CardClasses);
				}
			}
		}

		foreach (BoosterpackData boosterpackData in this.GameDataLoader.BoosterpackDatas)
		{
			for (int i = 0; i < boosterpackData.CardBags.Count; i++)
			{
				foreach (string cardId in boosterpackData.CardBags[i].GetCardsInBag(this.GameDataLoader))
				{
					CardData cardPrefab = this.GameDataLoader.GetCardFromId(cardId, false);
					if (cardPrefab != null)
					{
						var attribute = System.Attribute.GetCustomAttribute(cardPrefab.GetType(), typeof(Mewtations.Core.LegacySystemAttribute)) as Mewtations.Core.LegacySystemAttribute;
						if (attribute != null || cardPrefab is Mewtations.Core.ILegacySystemMarker)
						{
							validationResult.AddError(null, $"Booster '{boosterpackData.BoosterId}' bag {i} references legacy quarantined card '{cardId}'!", ValidationCategory.BoosterPacks);
						}
					}
				}
			}
		}
	}

	public GameDataLoader GameDataLoader;
}
