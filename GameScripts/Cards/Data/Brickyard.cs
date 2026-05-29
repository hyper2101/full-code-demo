using System;

public class Brickyard : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == "stone" || otherCard.Id == "sandstone";
	}

	public override void UpdateCard()
	{
		if (base.ChildrenMatchingPredicateCount((CardData c) => c.Id == "stone" || c.Id == "sandstone") >= 2)
		{
			this.MyGameCard.StartTimer(10f, new TimerAction(this.CompleteMaking), MewtationsLoc.Translate("card_brickyard_status"), base.GetActionId("CompleteMaking"), true, false, false);
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
		this.MyGameCard.GetRootCard().CardData.DestroyChildrenMatchingPredicateAndRestack((CardData c) => c.Id == "stone" || c.Id == "sandstone", 2);
		CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "brick", false, false, true);
		WorldManager.instance.StackSendCheckTarget(this.MyGameCard, cardData.MyGameCard, this.OutputDir, this.MyGameCard, true, -1);
	}
}
