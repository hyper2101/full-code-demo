using System;

public class Artwork : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return this.Id == otherCard.Id;
	}
}
