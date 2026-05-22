using System;

public class ExportCenter : Factory
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		if (WorldManager.instance.CardCanBeSold(otherCard.MyGameCard, true, false))
		{
			return otherCard.AllChildrenMatchPredicate((CardData x) => WorldManager.instance.CardCanBeSold(x.MyGameCard, true, false));
		}
		return false;
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanToggleOnOff()
	{
		return true;
	}

	protected override bool CanSelectOutput()
	{
		return true;
	}

	public override void UpdateCard()
	{
		if (this.MyGameCard.HasChild && WorldManager.instance.CardCanBeSold(this.MyGameCard.GetLeafCard(), true, false) && !this.MyGameCard.TimerRunning)
		{
			this.MyGameCard.StartTimer(this.ExportTime, new TimerAction(this.SellCard), SokLoc.Translate("card_export_center_status_1"), base.GetActionId("SellCard"), true, false, false);
		}
		else if (!this.MyGameCard.HasChild)
		{
			this.MyGameCard.CancelTimer(base.GetActionId("SellCard"));
		}
		base.UpdateCard();
	}

	[TimedAction("sell_card")]
	public void SellCard()
	{
		GameCard leafCard = this.MyGameCard.GetLeafCard();
		leafCard.RemoveFromStack();
		GameCard gameCard = WorldManager.instance.SellCard(base.Position, leafCard, 1f, false);
		gameCard.RemoveFromParent();
		WorldManager.instance.StackSendCheckTarget(this.MyGameCard, gameCard, this.OutputDir, null, true, -1);
	}

	public float ExportTime;
}
