using System;

public class Cesspool : CardData
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
		return otherCard is Poop || base.CanHaveCard(otherCard);
	}
}
