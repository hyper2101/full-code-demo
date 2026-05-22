using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class CutsceneStep_FocusCameraOnCardType : CutsceneStep
{
	public override IEnumerator Process()
	{
		List<GameCard> allCardsOnBoard = WorldManager.instance.CardQuery.GetAllCardsOnBoard(WorldManager.instance.CurrentBoard.Id);
		GameCamera.instance.TargetCardOverride = allCardsOnBoard.FirstOrDefault<GameCard>((GameCard x) => this.ClassNames.Contains(x.CardData.GetType().ToString()));
		yield break;
	}

	public List<string> ClassNames;
}
