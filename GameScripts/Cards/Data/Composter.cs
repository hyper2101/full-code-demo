using System;

public class Composter : CardData
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
		Food food = otherCard as Food;
		return (food == null || food.FoodValue > 0) && otherCard.MyCardType == CardType.Food;
	}

	public override void UpdateCard()
	{
		if (base.ChildrenMatchingPredicateCount((CardData x) => this.CanHaveCard(x)) >= 5)
		{
			this.MyGameCard.StartTimer(60f, new TimerAction(this.Compost), MewtationsLoc.Translate("idea_composting_status"), "compost", true, false, false);
		}
		else
		{
			this.MyGameCard.CancelTimer("compost");
		}
		base.UpdateCard();
	}

	[TimedAction("compost")]
	public void Compost()
	{
		this.MyGameCard.GetRootCard().CardData.DestroyChildrenMatchingPredicateAndRestack((CardData x) => x.MyCardType == CardType.Food, 5);
		CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "soil", false, false, true);
		WorldManager.instance.StackSendCheckTarget(this.MyGameCard, cardData.MyGameCard, this.OutputDir, null, true, -1);
	}
}
