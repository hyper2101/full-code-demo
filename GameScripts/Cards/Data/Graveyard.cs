using System;

public class Graveyard : Harvestable
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == "corpse" || base.CanHaveCard(otherCard);
	}
}
