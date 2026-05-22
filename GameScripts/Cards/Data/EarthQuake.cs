using System;

public class EarthQuake : EventCard
{
	protected override void ExecuteEvent()
	{
		this.EventIsActive = true;
		WorldManager.instance.Cutscene.QueueCutscene(CitiesCutscenes.CitiesEarthQuake(this.MyGameCard));
	}
}
