using System;

public class Festival : EventCard
{
	protected override void ExecuteEvent()
	{
		this.MyGameCard.StartTimer(5f, new TimerAction(this.StopEvent), MewtationsLoc.Translate(this.EventText), base.GetActionId("StopEvent"), true, false, false);
		WorldManager.instance.Cutscene.QueueCutscene("cities_festival");
		CardData cardData = WorldManager.instance.CreateCard(base.Position, "merch", true, false, true);
		WorldManager.instance.CreateWellbeingPlus(base.Position);
		cardData.MyGameCard.SendIt();
		this.EventIsActive = true;
	}

	[TimedAction("stop_event")]
	public void StopEvent()
	{
		base.EndEvent();
	}

	protected override void EndEvent()
	{
		base.EndEvent();
	}

	public override void UpdateCardText()
	{
		if (this.MyGameCard != null && this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == base.GetActionId("StopEvent"))
		{
			this.descriptionOverride = MewtationsLoc.Translate(this.EventText);
		}
		base.UpdateCardText();
	}
}
