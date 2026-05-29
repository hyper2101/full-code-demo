using System;
using System.Collections.Generic;

public class Charity : CardData
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
		return otherCard.Id == "gold";
	}

	public override void UpdateCard()
	{
		base.GetChildrenMatchingPredicate((CardData x) => x is Gold, this.golds);
		if (this.golds.Count >= this.RequiredCoins)
		{
			this.MyGameCard.StartTimer(this.GenerationTime, new TimerAction(this.CompleteCharity), MewtationsLoc.Translate("card_charity_status_active"), "complete_charity", true, false, false);
		}
		else
		{
			this.MyGameCard.CancelTimer("complete_charity");
		}
		base.UpdateCard();
	}

	public override void UpdateCardText()
	{
		this.descriptionOverride = MewtationsLoc.Translate("card_charity_description", new LocParam[] { LocParam.Create("amount", this.RequiredCoins.ToString()) });
		base.UpdateCardText();
	}

	[TimedAction("complete_charity")]
	public void CompleteCharity()
	{
		base.GetChildrenMatchingPredicate((CardData x) => x is Gold, this.golds);
		if (this.golds.Count >= this.RequiredCoins)
		{
			base.DestroyChildrenMatchingPredicateAndRestack((CardData x) => this.golds.Contains(x), this.RequiredCoins);
			WorldManager.instance.TryCreateHappiness(base.transform.position, 1);
		}
	}

	public float GenerationTime = 5f;

	public int RequiredCoins = 3;

	private List<CardData> golds = new List<CardData>();
}
