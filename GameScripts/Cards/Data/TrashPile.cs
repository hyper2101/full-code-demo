using System;
using UnityEngine;

public class TrashPile : Harvestable
{
	public override ICardId GetCardToGive()
	{
		// 70% item_rotten_food, 30% item_spirit_ore
		if (UnityEngine.Random.value <= 0.70f)
		{
			return new CardId("item_rotten_food");
		}
		else
		{
			return new CardId("item_spirit_ore");
		}
	}

	public override void OnHarvestComplete()
	{
		WorldManager.instance.CurrentRunVariables.OpenedFirstTrash = true;
	}
}
