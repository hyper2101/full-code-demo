using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class CutsceneStep_WaitForEnergyConnectionMade : CutsceneStep
{
	public override IEnumerator Process()
	{
		WorldManager.instance.ConnectConnectors = true;
		List<GameCard> cards = (from x in WorldManager.instance.CardQuery.GetAllCardsOnBoard(WorldManager.instance.CurrentBoard.Id)
			where x.CardConnectorChildren.Count > 0
			select x).ToList<GameCard>();
		WorldManager.instance.ContinueClicked = false;
		WorldManager.instance.ContinueClicked = false;
		WorldManager.instance.ContinueButtonText = SokLoc.Translate("label_skip");
		WorldManager.instance.ShowContinueButton = true;
		for (;;)
		{
			if (!cards.All<GameCard>((GameCard x) => x.CardConnectorChildren.Count<CardConnector>((CardConnector x) => x.ConnectedNode != null && (x.ConnectionType == ConnectionType.LV || x.ConnectionType == ConnectionType.HV)) <= 0) || WorldManager.instance.ContinueClicked)
			{
				break;
			}
			yield return null;
			if (!(GameCanvas.instance.CurrentScreen is CutsceneScreen))
			{
				GameCanvas.instance.SetScreen<CutsceneScreen>();
			}
		}
		WorldManager.instance.ShowContinueButton = false;
		WorldManager.instance.ConnectConnectors = false;
		yield break;
	}
}
