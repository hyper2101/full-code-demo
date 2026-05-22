using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class CardBag
{
	private void RecalculateEnemiesIncluded()
	{
		List<string> cardsInBag = this.GetCardsInBag(new GameDataLoader(true, true));
		this.EnemiesIncluded = string.Join(", ", cardsInBag);
	}

	public void RecalculateOdds()
	{
		if (this.Chances != null && this.CardBagType == CardBagType.Chances)
		{
			float num = 0f;
			foreach (CardChance cardChance in this.Chances)
			{
				num += (float)cardChance.Chance;
			}
			foreach (CardChance cardChance2 in this.Chances)
			{
				cardChance2.PercentageChance = (cardChance2.PercentageChance = (float)cardChance2.Chance / num);
			}
		}
	}

	private float CalculatedExpectedValue(GameDataLoader loader, List<CardChance> chances)
	{
		float num = 0f;
		foreach (CardChance cardChance in chances)
		{
			num += (float)cardChance.Chance;
		}
		foreach (CardChance cardChance2 in chances)
		{
			cardChance2.PercentageChance = (float)cardChance2.Chance / num;
		}
		float num2 = 0f;
		foreach (CardChance cardChance3 in chances)
		{
			CardData cardFromId = loader.GetCardFromId(cardChance3.Id, true);
			float num3;
			if (!cardChance3.IsEnemy)
			{
				num3 = this.GetCardValue(loader, cardFromId);
			}
			else
			{
				num3 = 0f;
			}
			num2 += cardChance3.PercentageChance * num3;
		}
		return num2;
	}

	private float GetCardValue(GameDataLoader loader, CardData card)
	{
		Harvestable harvestable = card as Harvestable;
		float num;
		if (harvestable != null)
		{
			CardBag myCardBag = harvestable.MyCardBag;
			myCardBag.CalculateExpectedValueForBag(loader);
			num = Mathf.Max((float)card.GetValue(), myCardBag.ExpectedValue * (float)harvestable.Amount);
		}
		else
		{
			num = Mathf.Max(0f, (float)card.GetValue());
		}
		card.ExpectedValue = num;
		return card.ExpectedValue;
	}

	public void CalculateExpectedValueForBag(GameDataLoader loader)
	{
		if (this.CardBagType == CardBagType.SetCardBag)
		{
			this.ExpectedValue = this.CalculatedExpectedValue(loader, CardBag.GetChancesForSetCardBag(loader, this.SetCardBag, null));
			return;
		}
		if (this.CardBagType == CardBagType.SetPack)
		{
			this.ExpectedValue = this.CalculatedExpectedValue(loader, this.SetPackCards.Select<string, CardChance>((string x) => new CardChance(x, 1)).ToList<CardChance>());
			return;
		}
		if (this.CardBagType == CardBagType.Chances)
		{
			this.ExpectedValue = this.CalculatedExpectedValue(loader, this.Chances);
			return;
		}
		if (this.CardBagType == CardBagType.Enemies)
		{
			this.ExpectedValue = 0f;
		}
	}

	public ICardId GetCard(bool removeCard = true)
	{
		ICardId cardId;
		if (this.CardBagType == CardBagType.SetPack)
		{
			cardId = new CardId(this.SetPackCards[this.SetPackCards.Count - this.CardsInPack]);
		}
		else if (this.CardBagType == CardBagType.Chances)
		{
			cardId = WorldManager.instance.GetRandomCard(this.Chances, removeCard);
			if (cardId == null)
			{
				cardId = WorldManager.instance.GetRandomCard(CardBag.GetChancesForSetCardBag(WorldManager.instance.GameDataLoader, this.FallbackBag, null), removeCard);
			}
		}
		else if (this.CardBagType == CardBagType.SetCardBag)
		{
			List<CardChance> list;
			if (this.UseFallbackBag)
			{
				list = CardBag.GetChancesForSetCardBag(WorldManager.instance.GameDataLoader, this.SetCardBag, new SetCardBagType?(this.FallbackBag));
			}
			else
			{
				list = CardBag.GetChancesForSetCardBag(WorldManager.instance.GameDataLoader, this.SetCardBag, null);
			}
			cardId = WorldManager.instance.GetRandomCard(list, removeCard);
		}
		else if (this.CardBagType == CardBagType.Enemies)
		{
			if (WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
			{
				List<CardChance> chancesForSetCardBag = CardBag.GetChancesForSetCardBag(WorldManager.instance.GameDataLoader, CardBag.GetCurrentFallbackBag(), null);
				cardId = WorldManager.instance.GetRandomCard(chancesForSetCardBag, removeCard);
			}
			else
			{
				cardId = CardBag.GetCardIdForEnemyBag(this.EnemyCardBag, this.StrengthLevel);
			}
		}
		else
		{
			cardId = null;
		}
		if (removeCard)
		{
			this.CardsInPack--;
		}
		return cardId;
	}

	public ICardId GetCardFiltered(Predicate<string> filter, bool removeCard = true)
	{
		if (this.CardBagType != CardBagType.Chances)
		{
			throw new Exception("Can't get a filtered card for bag that is not using CardBagType.Chances");
		}
		if (removeCard)
		{
			this.CardsInPack--;
		}
		return WorldManager.instance.GetRandomCard(this.Chances.Where<CardChance>((CardChance x) => filter(x.Id)).ToList<CardChance>(), removeCard);
	}

	public static ICardId GetCardIdForEnemyBag(EnemySetCardBag enemyBag, float strengthLevel)
	{
		return SpawnHelper.GetEnemyToSpawn(WorldManager.instance.GameDataLoader.GetSetCardBagForEnemyCardBag(enemyBag).AsList<SetCardBagType>(), strengthLevel, true);
	}

	public List<string> GetCardsInBag(GameDataLoader loader)
	{
		if (this.CardBagType == CardBagType.SetPack)
		{
			return this.SetPackCards;
		}
		if (this.CardBagType == CardBagType.Chances)
		{
			return this.Chances.SelectMany<CardChance, string>((CardChance x) => CardBag.CardChanceToIds(x, loader)).ToList<string>();
		}
		if (this.CardBagType == CardBagType.SetCardBag)
		{
			return CardBag.GetChancesForSetCardBag(loader, this.SetCardBag, null).SelectMany<CardChance, string>((CardChance x) => CardBag.CardChanceToIds(x, loader)).ToList<string>();
		}
		if (this.CardBagType == CardBagType.Enemies)
		{
			SetCardBagType setCardBagForEnemyCardBag = loader.GetSetCardBagForEnemyCardBag(this.EnemyCardBag);
			List<CardChance> chancesForSetCardBag = CardBag.GetChancesForSetCardBag(loader, setCardBagForEnemyCardBag, null);
			chancesForSetCardBag.RemoveAll(delegate(CardChance x)
			{
				Combatable combatable = loader.GetCardFromId(x.Id, true) as Combatable;
				return combatable != null && combatable.ProcessedCombatStats.CombatLevel > this.StrengthLevel;
			});
			return chancesForSetCardBag.SelectMany<CardChance, string>((CardChance x) => CardBag.CardChanceToIds(x, loader)).ToList<string>();
		}
		throw new Exception();
	}

	private static List<string> CardChanceToIds(CardChance c, GameDataLoader loader)
	{
		if (c.IsEnemy)
		{
			SetCardBagType setCardBagForEnemyCardBag = loader.GetSetCardBagForEnemyCardBag(c.EnemyBag);
			List<CardChance> chancesForSetCardBag = CardBag.GetChancesForSetCardBag(loader, setCardBagForEnemyCardBag, null);
			chancesForSetCardBag.RemoveAll((CardChance x) => (loader.GetCardFromId(x.Id, true) as Combatable).ProcessedCombatStats.CombatLevel > x.Strength);
			return chancesForSetCardBag.Select<CardChance, string>((CardChance x) => x.Id).ToList<string>();
		}
		return c.Id.AsList<string>();
	}

	public List<string> GetCardsInBag()
	{
		return this.GetCardsInBag(WorldManager.instance.GameDataLoader);
	}

	private static List<CardChance> GetRawCardChanges(GameDataLoader loader, SetCardBagType bag)
	{
		return CardBag.ToCardChances(loader.SetCardBags.Where<SetCardBagData>((SetCardBagData x) => x.SetCardBagType == bag && x.IsActive()).SelectMany<SetCardBagData, SimpleCardChance>((SetCardBagData x) => x.Chances).ToList<SimpleCardChance>());
	}

	private static List<CardChance> GetChancesForBagNoFallback(GameDataLoader loader, SetCardBagType bag)
	{
		List<CardChance> rawCardChanges = CardBag.GetRawCardChanges(loader, bag);
		for (int i = rawCardChanges.Count - 1; i >= 0; i--)
		{
			if (string.IsNullOrWhiteSpace(rawCardChanges[i].Id))
			{
				Debug.LogError(string.Format("Error while processing {0}", bag));
			}
			else
			{
				CardData cardFromId = loader.GetCardFromId(rawCardChanges[i].Id, true);
				if (WorldManager.instance != null)
				{
					if ((cardFromId.MyCardType == CardType.Ideas || cardFromId.MyCardType == CardType.Rumors) && WorldManager.instance.CurrentSave.FoundCardIds.Contains(rawCardChanges[i].Id))
					{
						rawCardChanges.RemoveAt(i);
					}
					if (cardFromId is Enemy && WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
					{
						rawCardChanges.RemoveAt(i);
					}
				}
			}
		}
		return rawCardChanges;
	}

	public static List<CardChance> GetChancesForSetCardBag(GameDataLoader loader, SetCardBagType bag, SetCardBagType? fallbackBag = null)
	{
		List<CardChance> list = CardBag.GetChancesForBagNoFallback(loader, bag);
		if (list.Count == 0)
		{
			if (WorldManager.instance != null && WorldManager.instance.CurrentBoard != null)
			{
				SetCardBagType setCardBagType = CardBag.GetCurrentFallbackBag();
				if (fallbackBag != null)
				{
					setCardBagType = fallbackBag.Value;
				}
				list = CardBag.GetRawCardChanges(loader, setCardBagType);
			}
			else
			{
				list = CardBag.GetRawCardChanges(loader, SetCardBagType.BasicHarvestable);
			}
		}
		return list;
	}

	private static SetCardBagType GetCurrentFallbackBag()
	{
		return WorldManager.instance.CurrentBoard.BoardOptions.FallbackBag;
	}

	private static List<CardChance> ToCardChances(List<SimpleCardChance> sc)
	{
		List<CardChance> list = new List<CardChance>();
		foreach (SimpleCardChance simpleCardChance in sc)
		{
			list.Add(new CardChance(simpleCardChance.CardId, simpleCardChance.Chance));
		}
		return list;
	}

	public void SelectSetCardBag()
	{
	}

	public CardBagType CardBagType;

	public int CardsInPack = 3;

	public float ExpectedValue;

	public List<CardChance> Chances;

	[Card]
	public List<string> SetPackCards;

	public SetCardBagType SetCardBag;

	public bool UseFallbackBag;

	public EnemySetCardBag EnemyCardBag;

	public float StrengthLevel;

	public SetCardBagType FallbackBag;

	public string EnemiesIncluded;
}
