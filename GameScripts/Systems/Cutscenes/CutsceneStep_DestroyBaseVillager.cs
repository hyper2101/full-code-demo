using System;
using System.Collections;
using System.Collections.Generic;

public class CutsceneStep_DestroyBaseVillager : CutsceneStep
{
	public override IEnumerator Process()
	{
		List<BaseVillager> cards = WorldManager.instance.CardQuery.GetCards<BaseVillager>();
		if (cards.Count > 0)
		{
			cards[cards.Count - 1].MyGameCard.DestroyCard(true, true);
		}
		yield break;
	}
}
