using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Boosterpack", menuName = "ScriptableObjects/Boosterpack", order = 1)]
public class BoosterpackData : ScriptableObject
{
	public string Name
	{
		get
		{
			if (!string.IsNullOrEmpty(this.nameOverride))
			{
				return this.nameOverride;
			}
			return MewtationsLoc.Translate(this.NameTerm);
		}
	}

	public bool IsUnlocked
	{
		get
		{
			return QuestManager.instance.BoosterIsUnlocked(this, true);
		}
	}

	public int RemainingAchievementCountToUnlock
	{
		get
		{
			return QuestManager.instance.RemainingQuestCountToComplete(this);
		}
	}

	public int UndiscoveredCardCount
	{
		get
		{
			List<string> list = new List<string>();
			foreach (CardBag cardBag in this.CardBags)
			{
				list.AddRange(cardBag.GetCardsInBag());
			}
			return BoosterpackData.GetUndiscoveredCardCount(list);
		}
	}

	public string GetSummary()
	{
		List<string> list = new List<string>();
		foreach (CardBag cardBag in this.CardBags)
		{
			list.AddRange(cardBag.GetCardsInBag());
		}
		return BoosterpackData.GetSummaryFromAllCards(list, "label_may_contain");
	}

	public static int GetUndiscoveredCardCount(List<string> allCards)
	{
		List<string> list = allCards.Distinct<string>().ToList<string>();
		int num = 0;
		foreach (string text in list)
		{
			if (!WorldManager.instance.CurrentSave.FoundCardIds.Contains(text))
			{
				num++;
			}
		}
		return num;
	}

	public static string GetSummaryFromAllCards(List<string> allCards, string prefix = "label_may_contain")
	{
		if (allCards.Count == 0)
		{
			return "";
		}
		List<string> list = allCards.Distinct<string>().ToList<string>();
		List<string> list2 = new List<string>();
		int num = 0;
		foreach (string text in list)
		{
			CardData cardPrefab = WorldManager.instance.GetCardPrefab(text, true);
			string text2 = cardPrefab.FullName;
			if (cardPrefab.MyCardType == CardType.Ideas)
			{
				text2 = MewtationsLoc.Translate("label_an_idea");
			}
			if (cardPrefab.MyCardType == CardType.Rumors)
			{
				text2 = MewtationsLoc.Translate("label_a_rumor");
			}
			if (!WorldManager.instance.CurrentSave.FoundCardIds.Contains(text))
			{
				num++;
			}
			else if (!list2.Contains(text2))
			{
				list2.Add(text2);
			}
		}
		list2 = (from x in list2
			orderby x
			select "  " + Icons.Circle + " " + x).ToList<string>();
		string text3 = string.Join("\n", list2);
		string text4 = "";
		if (!string.IsNullOrEmpty(prefix))
		{
			text4 = MewtationsLoc.Translate(prefix) + "\n";
		}
		if (num > 0)
		{
			text4 = string.Concat(new string[]
			{
				text4,
				"  ",
				Icons.Circle,
				" ",
				MewtationsLoc.Translate("label_undiscovered_cards", new LocParam[] { LocParam.Plural("count", num) }),
				"\n"
			});
		}
		return text4 + text3;
	}

	public void LogAllCardsEditor()
	{
		GameDataLoader gameDataLoader = new GameDataLoader(true, true);
		List<string> list = new List<string>();
		foreach (CardBag cardBag in this.CardBags)
		{
			list.AddRange(cardBag.GetCardsInBag(gameDataLoader));
		}
		Debug.Log(BoosterpackData.GetSummaryEditor(list, gameDataLoader));
	}

	public static string GetSummaryEditor(List<string> allCards, GameDataLoader loader)
	{
		if (allCards.Count == 0)
		{
			return "";
		}
		List<string> list = allCards.Distinct<string>().ToList<string>();
		List<string> list2 = new List<string>();
		foreach (string text in list)
		{
			CardData cardFromId = loader.GetCardFromId(text, true);
			string text2 = cardFromId.FullName;
			if (cardFromId.MyCardType == CardType.Ideas)
			{
				text2 = MewtationsLoc.Translate("label_an_idea");
			}
			if (cardFromId.MyCardType == CardType.Rumors)
			{
				text2 = MewtationsLoc.Translate("label_a_rumor");
			}
			if (!list2.Contains(text2))
			{
				list2.Add(text2);
			}
		}
		return string.Join(", ", list2);
	}

	public string BoosterId;

	public string NameTerm;

	[HideInInspector]
	public string nameOverride;

	public int MinAchievementCount = 3;

	public bool IsIntroPack;

	public int Cost = 3;

	public Sprite Icon;

	public Location BoosterLocation;

	public List<CardBag> CardBags;

	public List<BoosterAddition> BoosterAdditions;

	public float ExpectedValue;
}
