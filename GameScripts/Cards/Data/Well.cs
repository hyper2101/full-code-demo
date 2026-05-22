using System;

public class Well : Harvestable
{
	public override bool DetermineCanHaveCardsWhenIsRoot
	{
		get
		{
			return true;
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == "empty_bottle" || otherCard is BaseVillager || otherCard.Id == "magic_dust" || otherCard.Id == "brick" || base.CanHaveCard(otherCard);
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}
}
