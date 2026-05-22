using System;

public class PettingZoo : CardData
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
		if (otherCard.Id == "soil")
		{
			return true;
		}
		int num = base.GetChildCount() + (1 + otherCard.GetChildCount());
		return otherCard is Animal && otherCard.MyCardType != CardType.Fish && num <= 5;
	}

	public override void UpdateCard()
	{
		if (base.ChildrenMatchingPredicateCount((CardData x) => x is Animal) > 0)
		{
			this.MyGameCard.StartTimer(this.GenerationTime, new TimerAction(this.CompletePetting), SokLoc.Translate("card_petting_zoo_status_active"), "complete_petting", true, false, false);
		}
		else
		{
			this.MyGameCard.CancelTimer("complete_petting");
		}
		base.UpdateCard();
	}

	[TimedAction("complete_petting")]
	public void CompletePetting()
	{
		int num = base.ChildrenMatchingPredicateCount((CardData x) => x is Animal);
		WorldManager.instance.TryCreateHappiness(this.MyGameCard.transform.position, num);
	}

	public float GenerationTime;
}
