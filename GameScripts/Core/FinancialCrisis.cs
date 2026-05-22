using System;

public class FinancialCrisis : EventCard
{
	protected override void ExecuteEvent()
	{
		this.MyGameCard.StartTimer(WorldManager.instance.MonthTime, new TimerAction(this.StopEvent), SokLoc.Translate("label_uh_oh"), base.GetActionId("StopEvent"), true, false, false);
		this.EventIsActive = true;
		WorldManager.instance.Cutscene.QueueCutscene(CitiesCutscenes.CitiesFinancialCrisis());
	}

	[TimedAction("stop_event")]
	public void StopEvent()
	{
		base.EndEvent();
	}

	protected override void EndEvent()
	{
		WorldManager.instance.Cutscene.QueueCutscene(CitiesCutscenes.CitiesStopDisaster());
		base.EndEvent();
	}

	public override void UpdateCardText()
	{
		if (this.MyGameCard != null && this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == base.GetActionId("StopEvent"))
		{
			this.descriptionOverride = SokLoc.Translate(this.EventText);
		}
		base.UpdateCardText();
	}
}
