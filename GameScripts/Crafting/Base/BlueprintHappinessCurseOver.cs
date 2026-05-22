using System;
using System.Collections.Generic;

public class BlueprintHappinessCurseOver : Blueprint
{
	public override void BlueprintComplete(GameCard rootCard, List<GameCard> involvedCards, Subprint print)
	{
		base.BlueprintComplete(rootCard, involvedCards, print);
		if (WorldManager.instance.CurrentBoard.Id == "happiness")
		{
			WorldManager.instance.CurrentRunVariables.CompletedHappinessSpirit = true;
			WorldManager.instance.CurrentSave.FinishedHappiness = true;
			WorldManager.instance.Cutscene.QueueCutscene(Cutscenes.SpiritOutro(CurseType.Happiness));
		}
	}
}
