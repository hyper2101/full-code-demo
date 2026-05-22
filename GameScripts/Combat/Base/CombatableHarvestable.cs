using System;
using UnityEngine;

public class CombatableHarvestable : CardData
{
	public string StatusText
	{
		get
		{
			return SokLoc.Translate(this.StatusTerm);
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is BaseVillager;
	}

	public override void SetFoil()
	{
		base.SetFoil();
	}

	public override void UpdateCard()
	{
		BaseVillager baseVillager;
		if (base.HasCardOnTop<BaseVillager>(out baseVillager))
		{
			string actionId = base.GetActionId("CompleteHarvest");
			this.MyGameCard.StartTimer(baseVillager.GetActionTimeModifier(actionId, this) * this.HarvestTime, new TimerAction(this.CompleteHarvest), this.StatusText, actionId, true, false, false);
		}
		else
		{
			this.MyGameCard.CancelTimer(base.GetActionId("CompleteHarvest"));
		}
		base.UpdateCard();
	}

	[TimedAction("complete_harvest")]
	public void CompleteHarvest()
	{
		if (!this.IsUnlimited)
		{
			this.Amount--;
		}
		CardData cardData = WorldManager.instance.CreateCard(this.MyGameCard.transform.position, this.MyCardBag.GetCard(true), false, false, true);
		WorldManager.instance.StackSendCheckTarget(this.MyGameCard, cardData.MyGameCard, this.OutputDir, null, true, -1);
		BaseVillager baseVillager;
		if (base.HasCardOnTop<BaseVillager>(out baseVillager))
		{
			baseVillager.MyGameCard.RotWobble(0.5f);
		}
		if (!this.IsUnlimited && this.Amount <= 0)
		{
			this.MyGameCard.DestroyCard(true, true);
		}
	}

	[Header("Harvestable")]
	public string StatusTerm;

	[ExtraData("amount")]
	public int Amount = 3;

	public bool IsUnlimited;

	public float HarvestTime = 10f;

	public CardBag MyCardBag;
}
