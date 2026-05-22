using System;
using System.Collections.Generic;

public class StompingVat : CardData
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
		return otherCard is BaseVillager || this.CanHaveIds.Contains(otherCard.Id);
	}

	public List<string> CanHaveIds = new List<string> { "grape", "olive" };
}
