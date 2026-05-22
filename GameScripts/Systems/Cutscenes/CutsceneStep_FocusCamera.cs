using System;
using System.Collections;
using System.Linq;

[Serializable]
public class CutsceneStep_FocusCamera : CutsceneStep
{
	public override IEnumerator Process()
	{
		if (!this.FocusType && !string.IsNullOrEmpty(this.CardId))
		{
			GameCamera.instance.TargetCardOverride = WorldManager.instance.GetCard(this.CardId);
		}
		else if (this.FocusType)
		{
			GameCamera.instance.TargetCardOverride = WorldManager.instance.CardQuery.GetAllCardsOnBoard(WorldManager.instance.CurrentBoard.Id).FirstOrDefault<GameCard>((GameCard x) => x.CardData.MyCardType == this.Type);
		}
		yield break;
	}

	[Card]
	public string CardId;

	public bool FocusType;

	public CardType Type;
}
