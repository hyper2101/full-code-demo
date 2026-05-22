using System;

public class Drought : EventCard
{
	protected override void ExecuteEvent()
	{
		this.EventIsActive = true;
		WorldManager.instance.Cutscene.QueueCutscene(CitiesCutscenes.CitiesDrought(this.MyGameCard));
	}
}
