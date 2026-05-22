using System;
using System.Collections.Generic;
using UnityEngine;

public class StrangePortal : Portal
{
	public override bool CanBeDragged
	{
		get
		{
			return false;
		}
	}

	public override void UpdateCard()
	{
		if (!TransitionScreen.InTransition && !WorldManager.instance.InAnimation)
		{
			int num = base.ChildrenMatchingPredicateCount((CardData x) => x is BaseVillager);
			if (!this.MyGameCard.TimerRunning && num == 0)
			{
				this.MyGameCard.StartTimer(this.SpawnTime, new TimerAction(this.SpawnCreature), SokLoc.Translate("new_portal_shaking"), base.GetActionId("SpawnCreature"), true, false, false);
				if (this.SpawnTimer > 0f)
				{
					this.MyGameCard.CurrentTimerTime = this.SpawnTimer;
				}
			}
			if (num > 0)
			{
				if (!WorldManager.instance.CurrentBoard.BoardOptions.CanTravelToForest)
				{
					GameCanvas.instance.ShowCantChangeBoardSpirit();
					base.Stay();
					return;
				}
				base.RemoveNonHuman();
				int cardCount = WorldManager.instance.GetCardCount((CardData x) => x is BaseVillager);
				if (base.ChildrenMatchingPredicateCount((CardData x) => x is BaseVillager) > this.MaxVillagerCount)
				{
					if (this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == base.GetActionId("TakePortal"))
					{
						this.TravelTimer = this.MyGameCard.CurrentTimerTime;
					}
					this.MyGameCard.CancelTimer(base.GetActionId("TakePortal"));
					GameCanvas.instance.MaxVillagerCountPrompt("label_taking_portal_title", this.MaxVillagerCount);
					base.RemoveExcessVillagersInPortal();
				}
				if (num == cardCount)
				{
					if (this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == base.GetActionId("TakePortal"))
					{
						this.TravelTimer = this.MyGameCard.CurrentTimerTime;
					}
					this.MyGameCard.CancelTimer(base.GetActionId("TakePortal"));
					GameCanvas.instance.OneVillagerNeedsToStayPrompt("label_taking_portal_title");
					base.RemoveLastVillagerInPortal();
				}
				else if (this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId != base.GetActionId("TakePortal"))
				{
					this.SpawnTimer = this.MyGameCard.CurrentTimerTime;
					this.MyGameCard.CancelTimer(base.GetActionId("SpawnCreature"));
					this.MyGameCard.StartTimer(this.TravelTime, new TimerAction(base.TakePortal), SokLoc.Translate("card_stable_portal_status"), base.GetActionId("TakePortal"), true, false, false);
					if (this.TravelTimer > 0f)
					{
						this.MyGameCard.CurrentTimerTime = this.TravelTimer;
					}
				}
				else if (!this.MyGameCard.TimerRunning)
				{
					this.MyGameCard.StartTimer(this.TravelTime, new TimerAction(base.TakePortal), SokLoc.Translate("card_stable_portal_status"), base.GetActionId("TakePortal"), true, false, false);
					if (this.TravelTimer > 0f)
					{
						this.MyGameCard.CurrentTimerTime = this.TravelTimer;
					}
				}
			}
			else
			{
				if (this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == base.GetActionId("TakePortal"))
				{
					this.TravelTimer = this.MyGameCard.CurrentTimerTime;
				}
				this.MyGameCard.CancelTimer(base.GetActionId("TakePortal"));
			}
		}
		base.UpdateCard();
	}

	[TimedAction("spawn_creature")]
	public void SpawnCreature()
	{
		List<EnemySetCardBag> list = new List<EnemySetCardBag>();
		if (WorldManager.instance.Time.CurrentMonth >= 24)
		{
			list.Add(EnemySetCardBag.BasicEnemy);
			list.Add(EnemySetCardBag.AdvancedEnemy);
			list.Add(EnemySetCardBag.Forest_BasicEnemy);
		}
		else if (WorldManager.instance.Time.CurrentMonth >= 16)
		{
			list.Add(EnemySetCardBag.BasicEnemy);
			list.Add(EnemySetCardBag.AdvancedEnemy);
		}
		else
		{
			list.Add(EnemySetCardBag.BasicEnemy);
		}
		int num = Mathf.RoundToInt((float)Mathf.Max(12, WorldManager.instance.Time.CurrentMonth) * 1.5f);
		num = Mathf.Clamp(num, 0, 70);
		if (this.IsRarePortal)
		{
			num = Mathf.RoundToInt((float)num * 1.5f);
		}
		foreach (CardIdWithEquipment cardIdWithEquipment in SpawnHelper.GetEnemiesToSpawn(WorldManager.instance.GameDataLoader.GetSetCardBagForEnemyCardBagList(list), (float)num, true))
		{
			Combatable combatable = WorldManager.instance.CreateCard(base.transform.position, cardIdWithEquipment, false, false, true) as Combatable;
			combatable.HealthPoints = combatable.ProcessedCombatStats.MaxHealth;
			combatable.MyGameCard.SendIt();
		}
		this.MyGameCard.DestroyCard(true, true);
	}

	public float SpawnTime = 10f;

	public float TravelTime = 5f;

	public bool IsRarePortal;

	private float SpawnTimer;

	private float TravelTimer;

	[ExtraData("spawns_remaining")]
	public int SpawnsRemaining = 3;
}
