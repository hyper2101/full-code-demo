using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TreasureChest : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == "key" || otherCard.Id == "treasure_chest";
	}

	public override void UpdateCard()
	{
		CardData cardData;
		if (base.HasCardOnTop("key", out cardData))
		{
			for (int i = 0; i < this.Amount; i++)
			{
				WorldManager.instance.CreateCard(base.transform.position, this.GetCard(), false, false, true, true).MyGameCard.SendIt();
			}
			QuestManager.instance.SpecialActionComplete("treasure_chest_opened", this);
			cardData.MyGameCard.DestroyCard(false, true);
			this.MyGameCard.DestroyCard(false, true);
		}
		base.UpdateCard();
	}

	private CardData GetCard()
	{
		List<CardData> list = WorldManager.instance.CardDataPrefabs.Where<CardData>((CardData x) => (x.MyCardType == CardType.Resources || x.MyCardType == CardType.Food) && x.CardUpdateType == CardUpdateType.Main).ToList<CardData>();
		list.RemoveAll((CardData x) => x.Id == "goblet");
		return list[Random.Range(0, list.Count)];
	}

	public int Amount = 3;
}
