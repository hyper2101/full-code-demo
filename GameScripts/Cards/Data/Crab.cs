using System;

public class Crab : Animal
{
	public override void Die()
	{
		if (!WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
		{
			WorldManager.instance.CurrentRunVariables.CrabsKilled++;
			if (WorldManager.instance.CurrentRunVariables.CrabsKilled % 3 == 0)
			{
				CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "momma_crab", false, false, true);
				WorldManager.instance.Cutscene.QueueCutscene(Cutscenes.MommaCrab(cardData));
			}
		}
		base.Die();
	}
}
