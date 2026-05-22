using System;

public class StablePortal : Portal
{
	public override void UpdateCard()
	{
		if (string.IsNullOrWhiteSpace(this.descriptionOverride))
		{
			this.descriptionOverride = SokLoc.Translate("card_stable_portal_description", new LocParam[] { LocParam.Create("amount", this.MaxVillagerCount.ToString()) });
		}
		if (!TransitionScreen.InTransition && !WorldManager.instance.InAnimation)
		{
			int num = base.ChildrenMatchingPredicateCount((CardData x) => x is BaseVillager);
			if (num > 0)
			{
				if (!WorldManager.instance.CurrentBoard.BoardOptions.CanTravelToForest)
				{
					GameCanvas.instance.ShowCantChangeBoardSpirit();
					base.Stay();
					return;
				}
				base.RemoveNonHuman();
				int cardCount = WorldManager.instance.GetCardCount((CardData x) => x is BaseVillager);
				if (base.ChildrenMatchingPredicateCount((CardData x) => x is BaseVillager) > this.MaxVillagerCount && !GameCanvas.instance.ModalIsOpen)
				{
					this.MyGameCard.CancelTimer(base.GetActionId("TakePortal"));
					GameCanvas.instance.MaxVillagerCountPrompt("label_taking_portal_title", this.MaxVillagerCount);
					base.RemoveExcessVillagersInPortal();
				}
				if (num == cardCount && !GameCanvas.instance.ModalIsOpen)
				{
					this.MyGameCard.CancelTimer(base.GetActionId("TakePortal"));
					GameCanvas.instance.OneVillagerNeedsToStayPrompt("label_taking_portal_title");
					base.RemoveLastVillagerInPortal();
				}
				else
				{
					this.MyGameCard.StartTimer(this.TravelTime, new TimerAction(base.TakePortal), SokLoc.Translate("card_stable_portal_status"), base.GetActionId("TakePortal"), true, false, false);
				}
			}
			else
			{
				this.MyGameCard.CancelTimer(base.GetActionId("TakePortal"));
			}
		}
		base.UpdateCard();
	}

	public float TravelTime = 30f;
}
