using System;

public class HeavyFoundation : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return true;
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		if (this.MyGameCard.HasChild)
		{
			return this.MyGameCard.Child.CardData.CanHaveCardsWhileHasStatus();
		}
		return base.CanHaveCardsWhileHasStatus();
	}

	public override bool DetermineCanHaveCardsWhenIsRoot
	{
		get
		{
			if (!this.MyGameCard.HasChild)
			{
				return base.DetermineCanHaveCardsWhenIsRoot;
			}
			return this.MyGameCard.Child.CardData.DetermineCanHaveCardsWhenIsRoot;
		}
	}

	public override bool CanBePushedBy(CardData otherCard)
	{
		return otherCard.Id == this.Id;
	}
}
