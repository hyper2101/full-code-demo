using System;

public class Oven : CardData
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
		return otherCard.Id == "dough" || otherCard.Id == "cheese" || otherCard.Id == "tomato";
	}
}
