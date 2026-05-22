using System;
using System.Collections.Generic;

public class FactoryParts : Resource
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return this.AcceptedCards.Contains(otherCard.Id) || base.CanHaveCard(otherCard);
	}

	[Card]
	public List<string> AcceptedCards = new List<string>();
}
