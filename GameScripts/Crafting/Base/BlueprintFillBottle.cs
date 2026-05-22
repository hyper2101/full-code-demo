using System;
using System.Collections.Generic;
using System.Linq;

public class BlueprintFillBottle : Blueprint
{
	public override void BlueprintComplete(GameCard rootCard, List<GameCard> involvedCards, Subprint print)
	{
		if (print.RequiredCards.Contains("empty_bottle"))
		{
			GameCard gameCard = involvedCards.Find((GameCard x) => x.CardData.Id == "spring");
			Harvestable harvestable = ((gameCard != null) ? gameCard.CardData : null) as Harvestable;
			if (harvestable != null)
			{
				harvestable.Amount--;
				if (harvestable.Amount <= 0)
				{
					harvestable.MyGameCard.DestroyCard(true, true);
				}
			}
		}
		base.BlueprintComplete(rootCard, involvedCards, print);
	}
}
