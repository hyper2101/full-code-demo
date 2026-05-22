using System;

public class Smelter : CardData
{
	public override bool DetermineCanHaveCardsWhenIsRoot
	{
		get
		{
			return true;
		}
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == "iron_ore" || otherCard.Id == "wood" || otherCard.Id == "sand" || otherCard.Id == "gold_ore" || otherCard.Id == "gold" || otherCard.Id == "gold_bar" || otherCard.Id == "glass" || base.CanHaveCard(otherCard);
	}
}
