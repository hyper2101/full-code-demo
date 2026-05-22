using System;

public class Demon : Enemy
{
	public override void Die()
	{
		if (this.Id == "demon")
		{
			WorldManager.instance.Cutscene.QueueCutscene(Cutscenes.BossFightComplete(this));
			WorldManager.instance.CurrentRunVariables.FinishedDemon = true;
			GameScreen.instance.UpdateQuestLog();
		}
		else
		{
			WorldManager.instance.Cutscene.QueueCutscene(Cutscenes.BossFight2Complete(this));
			WorldManager.instance.CurrentRunVariables.FinishedDemonLord = true;
		}
		base.TryDropItems();
	}
}
