using System;
using System.Collections.Generic;
using UnityEngine;

public class PirateBoat : CardData
{
	protected override void Awake()
	{
		base.Awake();
	}

	private void Start()
	{
		if (!this.MyGameCard.IsDemoCard)
		{
			this.Demand = Mathf.Min(100, 3 + WorldManager.instance.CurrentRunVariables.PirateBoatsBribed * 3);
		}
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard.MyGameCard == null)
		{
			return otherCard.Id == "gold";
		}
		return WorldManager.instance.BoughtWithGoldChest(otherCard.MyGameCard, this.Demand) || WorldManager.instance.BoughtWithGold(otherCard.MyGameCard, this.Demand, false);
	}

	public override bool CanBeDragged
	{
		get
		{
			return false;
		}
	}

	public void Buy()
	{
		this.MyGameCard.DestroyCard(true, false);
		QuestManager.instance.SpecialActionComplete("bribe_pirate_boat", null);
		WorldManager.instance.CurrentRunVariables.PirateBoatsBribed++;
	}

	public override void UpdateCard()
	{
		if (this.MyGameCard.HasChild)
		{
			GameCard child = this.MyGameCard.Child;
			if (WorldManager.instance.BoughtWithGold(child, this.Demand, false))
			{
				WorldManager.instance.RemoveCardsFromStackPred(child, this.Demand, (GameCard x) => x.CardData.Id == "gold");
				this.Buy();
			}
			else if (WorldManager.instance.BoughtWithGoldChest(child, this.Demand))
			{
				WorldManager.instance.BuyWithChest(child, this.Demand);
				this.Buy();
			}
		}
		if (!this.MyGameCard.TimerRunning)
		{
			this.MyGameCard.StartTimer(this.SpawnTime, new TimerAction(this.SpawnPirates), MewtationsLoc.Translate("card_pirate_boat_name"), base.GetActionId("SpawnPirates"), true, false, false);
		}
		base.UpdateCard();
	}

	public override void UpdateCardText()
	{
		this.descriptionOverride = MewtationsLoc.Translate("card_pirate_boat_status", new LocParam[] { LocParam.Create("count", this.Demand.ToString()) });
	}

	[TimedAction("spawn_pirates")]
	public void SpawnPirates()
	{
		float num = (float)(1 + WorldManager.instance.CurrentRunVariables.PirateBoatsBribed * (2 + Mathf.Min(2, WorldManager.instance.CurrentRunVariables.PirateBoatsSpawned - 1))) * 30f;
		Combatable combatable = WorldManager.instance.GetCardPrefab("pirate", true) as Combatable;
		foreach (CardIdWithEquipment cardIdWithEquipment in SpawnHelper.GetEnemiesToSpawn(new List<Combatable> { combatable }, num))
		{
			WorldManager.instance.CreateCard(base.transform.position, cardIdWithEquipment, false, false, true).MyGameCard.SendIt();
		}
		this.MyGameCard.DestroyCard(true, true);
	}

	public float SpawnTime = 20f;

	[ExtraData("spawns_remaining")]
	public int SpawnsRemaining = 3;

	public int Demand = 20;
}
