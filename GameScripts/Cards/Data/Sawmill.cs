using System;

public class Sawmill : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == "wood";
	}

	public override void UpdateCard()
	{
		if (base.ChildrenMatchingPredicateCount((CardData c) => c.Id == "wood") >= 2)
		{
			this.MyGameCard.StartTimer(10f, new TimerAction(this.CompleteMaking), SokLoc.Translate("card_sawmill_status"), base.GetActionId("CompleteMaking"), true, false, false);
		}
		else
		{
			this.MyGameCard.CancelTimer(base.GetActionId("CompleteMaking"));
		}
		base.UpdateCard();
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	[TimedAction("complete_making")]
	public void CompleteMaking()
	{
		this.MyGameCard.GetRootCard().CardData.DestroyChildrenMatchingPredicateAndRestack((CardData c) => c.Id == "wood", 2);
		CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "plank", false, false, true);
		WorldManager.instance.StackSendCheckTarget(this.MyGameCard, cardData.MyGameCard, this.OutputDir, this.MyGameCard, true, -1);
	}
}
