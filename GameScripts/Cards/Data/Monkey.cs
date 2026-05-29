using System;

public class Monkey : Animal
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == "banana" || base.CanHaveCard(otherCard);
	}

	public override void UpdateCard()
	{
		if (this.MyGameCard.HasChild && this.MyGameCard.Child.CardData.Id == "banana")
		{
			this.MyGameCard.StartTimer(1f, new TimerAction(this.TrainMonkey), MewtationsLoc.Translate("idea_training_monkey_status"), base.GetActionId("TrainMonkey"), true, false, false);
		}
		else
		{
			this.MyGameCard.CancelTimer(base.GetActionId("TrainMonkey"));
		}
		base.UpdateCard();
	}

	[TimedAction("train_monkey")]
	public void TrainMonkey()
	{
		this.MyGameCard.Child.DestroyCard(false, true);
		WorldManager.instance.ChangeToCard(this.MyGameCard, "trained_monkey");
	}
}
