using System;

public class PackSale : EventCard
{
	protected override void ExecuteEvent()
	{
		this.MyGameCard.StartTimer(WorldManager.instance.MonthTime / 2f, new TimerAction(this.StopEvent), MewtationsLoc.Translate("label_nice"), base.GetActionId("StopEvent"), true, false, false);
		WorldManager.instance.Cutscene.QueueCutscene("cities_pack_sale");
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
