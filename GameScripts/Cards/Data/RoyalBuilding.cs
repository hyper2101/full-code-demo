using System;

public class RoyalBuilding : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == this.Id || otherCard is Royal || base.CanHaveCard(otherCard);
	}
}
