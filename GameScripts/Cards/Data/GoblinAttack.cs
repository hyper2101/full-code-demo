using System;
using System.Collections.Generic;
using UnityEngine;

public class GoblinAttack : CardData
{
	public override void UpdateCard()
	{
		if (!this.MyGameCard.TimerRunning)
		{
			this.MyGameCard.StartTimer(30f, new TimerAction(this.SpawnCreature), MewtationsLoc.Translate("card_event_goblin_attack_status_1"), base.GetActionId("SpawnCreature"), true, false, false);
		}
		base.UpdateCard();
	}

	[TimedAction("spawn_creature")]
	public void SpawnCreature()
	{
		List<EnemySetCardBag> list = new List<EnemySetCardBag>();
		if (CitiesManager.instance.Wellbeing >= 30)
		{
			list.Add(EnemySetCardBag.Cities_BasicEnemy);
			list.Add(EnemySetCardBag.Cities_AdvancedEnemy);
		}
		else
		{
			list.Add(EnemySetCardBag.Cities_BasicEnemy);
		}
		float num = Mathf.InverseLerp(20f, 80f, (float)CitiesManager.instance.Wellbeing);
		int num2 = Mathf.RoundToInt(Mathf.Lerp(20f, 180f, num));
		foreach (CardIdWithEquipment cardIdWithEquipment in SpawnHelper.GetEnemiesToSpawn(WorldManager.instance.GameDataLoader.GetSetCardBagForEnemyCardBagList(list), (float)num2, true))
		{
			Combatable combatable = WorldManager.instance.CreateCard(base.transform.position, cardIdWithEquipment, false, false, true) as Combatable;
			combatable.HealthPoints = combatable.ProcessedCombatStats.MaxHealth;
			combatable.MyGameCard.SendIt();
		}
		this.MyGameCard.DestroyCard(true, true);
	}
}
