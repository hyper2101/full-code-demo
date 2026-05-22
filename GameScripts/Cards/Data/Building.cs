using System;

public class Building : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == this.Id || base.CanHaveCard(otherCard);
	}
}
