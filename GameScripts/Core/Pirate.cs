using System;

public class Pirate : Enemy
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == "parrot" || base.CanHaveCard(otherCard);
	}
}
