using System;
using UnityEngine;

public class Farmland : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is Worker || otherCard.Id == "water";
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanToggleOnOff()
	{
		return true;
	}

	public override void UpdateCard()
	{
		if (!this.IsDepleted)
		{
			base.RemoveStatusEffect<StatusEffect_Depleted>();
			if (!base.WorkerAmountMet() && this.MyGameCard.TimerRunning)
			{
				this.MyGameCard.CancelTimer(base.GetActionId("Harvest"));
			}
			if (!this.CanDeplete || this.AmountHarvested < this.HarvestAmount)
			{
				if (base.WorkerAmountMet() && !this.MyGameCard.TimerRunning)
				{
					this.MyGameCard.StartTimer(this.HarvestTime, new TimerAction(this.Harvest), MewtationsLoc.Translate("card_farmland_status"), base.GetActionId("Harvest"), true, false, false);
				}
			}
			else
			{
				if (this.CanDeplete)
				{
					this.IsDepleted = true;
				}
				this.AmountHarvested = 0;
			}
			this.MyGameCard.IconRenderer.sprite = this.NormalIcon;
		}
		else
		{
			base.AddStatusEffect(new StatusEffect_Depleted());
			if (this.MyGameCard.HasChild)
			{
				if (base.ChildrenMatchingPredicateCount((CardData x) => x.Id == "water") >= 1)
				{
					if (!this.MyGameCard.TimerRunning)
					{
						this.MyGameCard.StartTimer(this.DepletedTime, new TimerAction(this.WaterFarmland), MewtationsLoc.Translate("card_farmland_status_0"), base.GetActionId("WaterFarmland"), true, false, false);
						goto IL_0169;
					}
					goto IL_0169;
				}
			}
			this.MyGameCard.CancelTimer(base.GetActionId("WaterFarmland"));
			IL_0169:
			this.MyGameCard.IconRenderer.sprite = this.DepletedIcon;
		}
		this.MyGameCard.UpdateIcon();
		base.UpdateCard();
	}

	[TimedAction("harvest")]
	public void Harvest()
	{
		this.AmountHarvested++;
		CardData cardData = WorldManager.instance.CreateCard(base.Position, this.HarvestCardId, true, false, true);
		WorldManager.instance.StackSendCheckTarget(this.MyGameCard, cardData.MyGameCard, this.OutputDir, null, true, -1);
	}

	[TimedAction("water_farmland")]
	public void WaterFarmland()
	{
		base.DestroyChildrenMatchingPredicateAndRestack((CardData x) => x.Id == "water", 1);
		this.IsDepleted = false;
		AudioManager.me.PlaySound2D(this.WateringSound, 1f, 0.3f);
	}

	protected override bool CanSelectOutput()
	{
		return true;
	}

	public bool CanDeplete;

	[Card]
	public string HarvestCardId;

	public int HarvestAmount = 3;

	[HideInInspector]
	[ExtraData("amount_harvested")]
	public int AmountHarvested;

	public bool IsDepleted;

	public float DepletedTime = 30f;

	public float HarvestTime = 10f;

	public AudioClip WateringSound;

	public Sprite DepletedIcon;

	public Sprite NormalIcon;
}
