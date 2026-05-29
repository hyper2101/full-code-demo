using System;

public class SlaughterHouse : CardData
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
		int num = base.GetChildCount() + (1 + otherCard.GetChildCount());
		return otherCard is Animal && num <= 5;
	}

	public override void UpdateCard()
	{
		if (this.MyGameCard.HasChild && this.MyGameCard.Child.CardData is Animal)
		{
			this.MyGameCard.StartTimer(60f, new TimerAction(this.SlaughterAnimal), MewtationsLoc.Translate("action_slaughtering_status"), base.GetActionId("SlaughterAnimal"), true, false, false);
		}
		else
		{
			this.MyGameCard.CancelTimer(base.GetActionId("SlaughterAnimal"));
		}
		base.UpdateCard();
	}

	[TimedAction("slaughter_animal")]
	public void SlaughterAnimal()
	{
		if (this.MyGameCard.HasChild && this.MyGameCard.Child.CardData is Animal)
		{
			GameCard child = this.MyGameCard.Child;
			base.RemoveFirstChildFromStack();
			child.DestroyCard(false, true);
			CardData cardData;
			if (child.CardData.MyCardType == CardType.Fish)
			{
				cardData = WorldManager.instance.CreateCard(base.transform.position, "raw_fish", true, true, true);
			}
			else if (child.CardData.Id == "crab")
			{
				cardData = WorldManager.instance.CreateCard(base.transform.position, "raw_crab_meat", true, true, true);
			}
			else
			{
				cardData = WorldManager.instance.CreateCard(base.transform.position, "raw_meat", true, true, true);
			}
			WorldManager.instance.StackSendCheckTarget(this.MyGameCard, cardData.MyGameCard, this.OutputDir, null, true, -1);
			WorldManager.instance.CreateSmoke(base.transform.position);
		}
	}
}
