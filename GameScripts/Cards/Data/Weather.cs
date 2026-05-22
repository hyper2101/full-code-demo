using System;
using System.Collections.Generic;

public class Weather : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return this.AcceptedCards.Contains(otherCard.Id) || otherCard is Worker || otherCard.MyCardType == CardType.Weather;
	}

	public override bool DetermineCanHaveCardsWhenIsRoot
	{
		get
		{
			return true;
		}
	}

	[Card]
	public List<string> AcceptedCards = new List<string>();
}
