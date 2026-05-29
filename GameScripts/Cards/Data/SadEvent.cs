using System;
using UnityEngine;

public class SadEvent : CardData
{
	public override void UpdateCard()
	{
		if (!TransitionScreen.InTransition && !WorldManager.instance.InAnimation && !this.MyGameCard.TimerRunning)
		{
			this.MyGameCard.StartTimer(this.EventTime, new TimerAction(this.StartSadEvent), MewtationsLoc.Translate("new_portal_shaking"), base.GetActionId("StartSadEvent"), true, false, false);
		}
		base.UpdateCard();
	}

	[TimedAction("start_sad_event")]
	public void StartSadEvent()
	{
		int cardCount = WorldManager.instance.CardQuery.GetCardCount<BaseVillager>();
		WorldManager.instance.TryCreateUnhappiness(base.Position, Mathf.FloorToInt((float)(cardCount / 2)));
		this.MyGameCard.DestroyCard(true, true);
	}

	public float EventTime = 60f;
}
