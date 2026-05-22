using System;

public class Spring : Harvestable
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
		return otherCard.Id == "empty_bottle" || otherCard.Id == "magic_dust" || otherCard is BaseVillager || otherCard.Id == "brick" || base.CanHaveCard(otherCard);
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}
}
