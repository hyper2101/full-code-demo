using System;
using UnityEngine;

public class EventCard : CardData
{
	public override void OnInitialCreate()
	{
		if (!this.MyGameCard.TimerRunning)
		{
			this.MyGameCard.StartTimer(this.PreEventTime, new TimerAction(this.StartEvent), MewtationsLoc.Translate(this.PreEventText), base.GetActionId("StartEvent"), true, false, false);
		}
		if (this.IsPositiveEvent)
		{
			AudioManager.me.PlaySound((this.EventStartOverride != null) ? this.EventStartOverride : AudioManager.me.PositiveEventSpawn, base.transform, Random.Range(0.9f, 1.1f), 0.5f);
		}
		else
		{
			AudioManager.me.PlaySound((this.EventStartOverride != null) ? this.EventStartOverride : AudioManager.me.NegativeEventSpawn, base.transform, Random.Range(0.9f, 1.1f), 0.5f);
		}
		base.OnInitialCreate();
	}

	public override void UpdateCard()
	{
		if (this.ShouldStartEvent && !this.MyGameCard.TimerRunning)
		{
			this.ExecuteEvent();
		}
		this.ShouldStartEvent = false;
		base.UpdateCard();
	}

	[TimedAction("start_disaster")]
	public void StartEvent()
	{
		this.ShouldStartEvent = true;
		QuestManager.instance.SpecialActionComplete("event_disaster", this);
	}

	protected virtual void ExecuteEvent()
	{
	}

	protected virtual void EndEvent()
	{
		this.MyGameCard.DestroyCard(true, true);
	}

	public bool IsPositiveEvent;

	public float PreEventTime;

	[Term]
	public string PreEventText;

	[Term]
	public string EventText;

	[HideInInspector]
	public bool ShouldStartEvent;

	public CardEventType EventType;

	[ExtraData("event_is_active")]
	public bool EventIsActive;

	public AudioClip EventStartOverride;
}
