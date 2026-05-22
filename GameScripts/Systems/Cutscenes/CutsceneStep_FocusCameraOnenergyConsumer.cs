using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class CutsceneStep_FocusCameraOnenergyConsumer : CutsceneStep
{
	public override IEnumerator Process()
	{
		List<GameCard> allCardsOnBoard = WorldManager.instance.CardQuery.GetAllCardsOnBoard(WorldManager.instance.CurrentBoard.Id);
		GameCamera.instance.TargetCardOverride = allCardsOnBoard.FirstOrDefault<GameCard>((GameCard x) => x.CardConnectorChildren.Any<CardConnector>((CardConnector x) => (x.ConnectionType == ConnectionType.LV || x.ConnectionType == ConnectionType.HV) && x.CardDirection == CardDirection.input));
		yield break;
	}
}
