using System;
using System.Collections.Generic;

public class Barrel : CardData
{
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

	protected override bool CanHaveCard(CardData otherCard)
	{
		return this.CanBottleIds.Contains(otherCard.Id);
	}

	public List<string> CanBottleIds;
}
