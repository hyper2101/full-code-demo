using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Subprint
{
	public string StatusName
	{
		get
		{
			if (!string.IsNullOrEmpty(this.statusOverride))
			{
				return this.statusOverride;
			}
			return MewtationsLoc.Translate(this.StatusTerm);
		}
	}

	public static List<string> GetSpecialCardIds()
	{
		Subprint.UpdateAnyVillagerCardIds();
		Subprint.UpdateAnyWorkerCardIds();
		return Subprint.specialCardIds.Keys.ToList<string>();
	}

	public static void UpdateAnyVillagerCardIds()
	{
		Subprint.specialCardIds["any_villager_old"] = (Subprint.specialCardIds["any_villager_young"] = (Subprint.specialCardIds["any_villager"] = string.Join("|", Subprint.GetVillagerCardIds(null))));
		Subprint.specialCardIds["breedable_villager"] = string.Join("|", Subprint.GetVillagerCardIds((BaseVillager x) => x.CanBreed));
	}

	public static void UpdateAnyWorkerCardIds()
	{
		Subprint.specialCardIds["any_worker"] = string.Join("|", Subprint.GetWorkerCardIds(null));
		Subprint.specialCardIds["any_educated_worker"] = string.Join("|", Subprint.GetWorkerCardIds((Worker x) => x.WorkerType == WorkerType.Educated || x.WorkerType == WorkerType.Robot));
	}

	private static List<string> GetVillagerCardIds(Predicate<BaseVillager> pred = null)
	{
		List<string> list = new List<string>();
		WorldManager instance = WorldManager.instance;
		GameDataLoader gameDataLoader = ((instance != null) ? instance.GameDataLoader : null);
		if (gameDataLoader == null)
		{
			gameDataLoader = GameDataLoader.instance;
		}
		foreach (CardData cardData in gameDataLoader.CardDataPrefabs)
		{
			BaseVillager baseVillager = cardData as BaseVillager;
			if (baseVillager != null && (pred == null || pred(baseVillager)))
			{
				list.Add(cardData.Id);
			}
		}
		return list;
	}

	private static List<string> GetWorkerCardIds(Predicate<Worker> pred = null)
	{
		List<string> list = new List<string>();
		WorldManager instance = WorldManager.instance;
		GameDataLoader gameDataLoader = ((instance != null) ? instance.GameDataLoader : null);
		if (gameDataLoader == null)
		{
			gameDataLoader = GameDataLoader.instance;
		}
		foreach (CardData cardData in gameDataLoader.CardDataPrefabs)
		{
			Worker worker = cardData as Worker;
			if (worker != null && (pred == null || pred(worker)))
			{
				list.Add(cardData.Id);
			}
		}
		return list;
	}

	public bool StackMatchesSubprint(GameCard rootCard, out SubprintMatchInfo matchInfo)
	{
		matchInfo = default(SubprintMatchInfo);
		int num = rootCard.GetChildCount() + 1;
		if (rootCard.HasCardInStack((CardData x) => x.Id == "heavy_foundation"))
		{
			num--;
		}
		if (num < this.RequiredCards.Length)
		{
			return false;
		}
		if (this.ParentBlueprint.NeedsExactMatch && num != this.RequiredCards.Length)
		{
			return false;
		}
		this.missingCards.Clear();
		this.missingCards.AddRange(this.RequiredCards);
		GameCard gameCard = rootCard;
		int num2 = 0;
		while (gameCard != null)
		{
			for (int i = this.missingCards.Count - 1; i >= 0; i--)
			{
				string text = this.ParseCardId(this.missingCards[i]);
				if (CardStringSplitter.me.Split(text).Contains(gameCard.CardData.Id))
				{
					this.missingCards.RemoveAt(i);
					break;
				}
			}
			if (this.missingCards.Count == 0)
			{
				matchInfo = new SubprintMatchInfo(num2, this.RequiredCards.Length);
				return true;
			}
			gameCard = gameCard.Child;
			num2++;
		}
		return false;
	}

	public List<string> GetAllCardsToRemove()
	{
		List<string> list = new List<string>();
		if (this.CardsToRemove != null && this.CardsToRemove.Length != 0)
		{
			foreach (string text in this.CardsToRemove)
			{
				string text2 = this.ParseCardId(text);
				list.Add(text2);
			}
		}
		else
		{
			foreach (string text3 in this.RequiredCards)
			{
				string text4 = this.ParseCardId(text3);
				string text5 = CardStringSplitter.me.Split(text4)[0];
				CardData cardPrefab = WorldManager.instance.GetCardPrefab(text5, true);
				if (cardPrefab.MyCardType != CardType.Humans && cardPrefab.MyCardType != CardType.Structures && cardPrefab.Id != "worker")
				{
					list.Add(text4);
				}
			}
		}
		return list;
	}

	private string ParseCardId(string cardId)
	{
		if (Subprint.specialCardIds.ContainsKey(cardId))
		{
			return Subprint.specialCardIds[cardId];
		}
		return cardId;
	}

	public string DefaultText()
	{
		List<string> list = this.RequiredCards.ToList<string>();
		for (int i = 0; i < list.Count; i++)
		{
			string[] array = CardStringSplitter.me.Split(list[i]);
			CardData cardPrefab = WorldManager.instance.GetCardPrefab(array[0], true);
			cardPrefab.UpdateCardText();
			list[i] = cardPrefab.Name;
		}
		List<string> list2 = list.Distinct<string>().ToList<string>();
		string text = "";
		for (int j = 0; j < list2.Count; j++)
		{
			string card = list2[j];
			int num = list.Count<string>((string x) => x == card);
			text += string.Format("{0}x {1}", num, card);
			if (j < list2.Count - 1)
			{
				text += "\n";
			}
		}
		return text;
	}

	[HideInInspector]
	public int SubprintIndex;

	[HideInInspector]
	public Blueprint ParentBlueprint;

	[Card]
	public string[] RequiredCards;

	[Card]
	public string[] CardsToRemove;

	public int ResultPolution;

	public int ResultWellbeing;

	[Card]
	public string ResultCard;

	public string ResultAction;

	[Card]
	public string[] ExtraResultCards;

	public float Time = 10f;

	[Term]
	public string StatusTerm;

	[HideInInspector]
	public string statusOverride;

	private List<string> missingCards = new List<string>();

	private static Dictionary<string, string> specialCardIds = new Dictionary<string, string>
	{
		{ "any_villager", "" },
		{ "any_villager_young", "" },
		{ "any_villager_old", "" },
		{ "breedable_villager", "" },
		{ "stone", "stone|sandstone" },
		{ "cotton", "cotton" },
		{ "fish", "cod|eel|mackerel|tuna|shark" },
		{ "any_worker", "" },
		{ "any_educated_worker", "" }
	};
}
