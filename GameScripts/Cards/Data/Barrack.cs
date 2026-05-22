using System;
using System.Collections.Generic;

public class Barrack : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return this.AcceptedCards.Contains(otherCard.Id) || otherCard is Worker || otherCard is CitiesCombatable;
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	public override bool DetermineCanHaveCardsWhenIsRoot
	{
		get
		{
			return true;
		}
	}

	protected override bool CanSelectOutput()
	{
		return true;
	}

	[Card]
	public List<string> AcceptedCards = new List<string>();
}
