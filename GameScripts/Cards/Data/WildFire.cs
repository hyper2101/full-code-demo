using System;

public class WildFire : EventCard
{
	protected override void ExecuteEvent()
	{
		this.EventIsActive = true;
		WorldManager.instance.Cutscene.QueueCutscene(CitiesCutscenes.CitiesWildFire(this.MyGameCard));
	}
}
