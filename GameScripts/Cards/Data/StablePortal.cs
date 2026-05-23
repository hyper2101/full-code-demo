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
			bool hasCats = base.ChildrenMatchingPredicateCount((CardData x) => x is CatCardData) > 0;
			int num = base.ChildrenMatchingPredicateCount((CardData x) => x is BaseVillager || x is CatCardData);
			if (num > 0)
			{
				if (!hasCats && !WorldManager.instance.CurrentBoard.BoardOptions.CanTravelToForest)
				{
					GameCanvas.instance.ShowCantChangeBoardSpirit();
					base.Stay();
					return;
				}
				base.RemoveNonHuman();
				
				int currentOnPortal = base.ChildrenMatchingPredicateCount((CardData x) => x is BaseVillager || x is CatCardData);
				if (currentOnPortal > this.MaxVillagerCount && !GameCanvas.instance.ModalIsOpen)
				{
					this.MyGameCard.CancelTimer(base.GetActionId("TakePortal"));
					GameCanvas.instance.MaxVillagerCountPrompt("label_taking_portal_title", this.MaxVillagerCount);
					base.RemoveExcessVillagersInPortal();
				}
				
				if (!hasCats)
				{
					int humanCount = WorldManager.instance.GetCardCount((CardData x) => x is BaseVillager);
					int humansOnPortal = base.ChildrenMatchingPredicateCount((CardData x) => x is BaseVillager);
					if (humansOnPortal == humanCount && !GameCanvas.instance.ModalIsOpen)
					{
						this.MyGameCard.CancelTimer(base.GetActionId("TakePortal"));
						GameCanvas.instance.OneVillagerNeedsToStayPrompt("label_taking_portal_title");
						base.RemoveLastVillagerInPortal();
						return;
					}
				}
				
				if (!GameCanvas.instance.ModalIsOpen)
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
