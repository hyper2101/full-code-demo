using System;

public class Tornado : EventCard
{
	protected override void ExecuteEvent()
	{
		WorldManager.instance.Cutscene.QueueCutscene(CitiesCutscenes.CitiesTornado());
		this.EventIsActive = true;
		base.ExecuteEvent();
	}
}
