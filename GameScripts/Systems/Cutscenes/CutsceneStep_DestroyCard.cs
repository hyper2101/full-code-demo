using System;
using System.Collections;
using System.Collections.Generic;

public class CutsceneStep_DestroyCard : CutsceneStep
{
	public override IEnumerator Process()
	{
		List<CardData> cards = WorldManager.instance.GetCards(this.CardId);
		if (cards.Count > 0)
		{
			cards[cards.Count - 1].MyGameCard.DestroyCard(true, true);
		}
		yield break;
	}

	[Card]
	public string CardId;
}
