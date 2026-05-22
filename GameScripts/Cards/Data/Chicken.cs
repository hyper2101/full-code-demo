using System;

public class Chicken : Animal
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return (otherCard.Id == "egg" && !otherCard.MyGameCard.HasChild) || base.CanHaveCard(otherCard);
	}

	public override bool CanCreate
	{
		get
		{
			return !this.IsBrooding && base.CanCreate;
		}
	}

	protected bool IsBrooding
	{
		get
		{
			GameCard cardWithStatusInStack = this.MyGameCard.GetCardWithStatusInStack();
			return cardWithStatusInStack != null && cardWithStatusInStack.TimerBlueprintId == "blueprint_chicken";
		}
	}

	public override bool CanMove
	{
		get
		{
			return !this.IsBrooding && base.CanMove;
		}
	}
}
