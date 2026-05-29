using System;

public class Market : CardData
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
		if (otherCard.MyGameCard == null)
		{
			return otherCard.Value > 0;
		}
		return WorldManager.instance.CardCanBeSold(otherCard.MyGameCard, true, false);
	}

	public override void UpdateCard()
	{
		if (this.MyGameCard.HasChild && WorldManager.instance.CardCanBeSold(this.MyGameCard.Child, false, false) && (!this.MyGameCard.HasParent || this.MyGameCard.Parent.CardData is HeavyFoundation))
		{
			string text = MewtationsLoc.Translate("new_selling_card", new LocParam[] { LocParam.Create("card", this.MyGameCard.Child.CardData.FullName) });
			this.MyGameCard.StartTimer(60f, new TimerAction(this.SellWithMarket), text, base.GetActionId("SellWithMarket"), true, false, false);
		}
		else
		{
			this.MyGameCard.CancelTimer(base.GetActionId("SellWithMarket"));
		}
		base.UpdateCard();
	}

	[TimedAction("sell_with_market")]
	public void SellWithMarket()
	{
		GameCard child = this.MyGameCard.Child;
		if (child == null)
		{
			return;
		}
		GameCard gameCard = null;
		if (child.HasChild && WorldManager.instance.CardCanBeSold(child.Child, true, false))
		{
			gameCard = child.Child;
		}
		child.RemoveFromStack();
		if (gameCard != null)
		{
			this.MyGameCard.SetChild(gameCard);
		}
		QuestManager.instance.SpecialActionComplete("sell_at_market", this);
		GameCard gameCard2 = WorldManager.instance.SellCard(base.transform.position, child, 2f, false);
		if (gameCard2 != null)
		{
			WorldManager.instance.StackSendCheckTarget(this.MyGameCard, gameCard2.GetRootCard(), this.OutputDir, this.MyGameCard, true, -1);
		}
	}
}
