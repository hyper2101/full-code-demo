using System;

public class IndustrialRevolution : CardData
{
	public override void OnInitialCreate()
	{
		AudioManager.me.PlaySound(AudioManager.me.IndustrialRevolutionCreate, base.transform, 1f, 0.5f);
		base.OnInitialCreate();
	}

	public override void UpdateCard()
	{
		if (this.MyGameCard.HasChild && this.MyGameCard.Child.CardData is BaseVillager)
		{
			if (WorldManager.instance.CardQuery.GetCardCount<TimeMachine>() > 0)
			{
				this.MyGameCard.Child.RemoveFromParent();
			}
			if (!this.MyGameCard.TimerRunning)
			{
				this.MyGameCard.StartTimer(10f, new TimerAction(this.ShowModal), MewtationsLoc.Translate("label_go_to_cities"), base.GetActionId("ShowModal"), true, false, false);
			}
		}
		else
		{
			this.MyGameCard.CancelTimer(base.GetActionId("ShowModal"));
		}
		base.UpdateCard();
	}

	[TimedAction("show_modal")]
	public void ShowModal()
	{
		GameCanvas.instance.GoToCityPrompt(new Action(this.GoToBoard), null);
		this.MyGameCard.RemoveFromStack();
	}

	public void GoToBoard()
	{
		WorldManager.instance.GoToBoard(WorldManager.instance.GetBoardWithId("cities"), delegate
		{
		}, "cities");
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is BaseVillager;
	}
}
