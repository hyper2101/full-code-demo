using System;
using System.Linq;
using UnityEngine;

public class EnergyHarvestable : Harvestable, IEnergyConsumer
{
	public override void OnInitialCreate()
	{
		if (this.MyGameCard.CardConnectorChildren.Count<CardConnector>((CardConnector x) => x.CardDirection == CardDirection.input && x.ConnectionType == ConnectionType.LV) > 0)
		{
			WorldManager.instance.Cutscene.QueueCutsceneIfNotPlayed("cities_first_energy");
		}
		base.OnInitialCreate();
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return base.CanHaveCard(otherCard);
	}

	public override void OnHarvestComplete()
	{
		if (this.PollutionPerHarvest > 0)
		{
			(WorldManager.instance.CreateCard(base.Position, "pollution", true, false, true) as Pollution).PollutionAmount = this.PollutionPerHarvest;
		}
		if (this.Id == "uranium_mine")
		{
			CardData cardData = WorldManager.instance.CreateCard(base.Position, "gravel", true, false, true);
			this.SendCard(cardData.MyGameCard);
		}
	}

	public override void SendCard(GameCard card)
	{
		WorldManager.instance.StackSendCheckTarget(this.MyGameCard, card, this.OutputDir, null, true, -1);
	}

	protected override bool CanSelectOutput()
	{
		return true;
	}

	protected override bool CanToggleOnOff()
	{
		return true;
	}

	protected virtual bool CanStartHarvesting()
	{
		return true;
	}

	public override void UpdateCard()
	{
		if (!base.WorkerAmountMet())
		{
			this.MyGameCard.CancelTimer(base.GetActionId("CompleteHarvest"));
		}
		else if (base.WorkerAmountMet() && this.RequiredVillagerCount <= 0 && !this.MyGameCard.TimerRunning && this.CanStartHarvesting())
		{
			this.MyGameCard.StartTimer(this.HarvestTime, new TimerAction(base.CompleteHarvest), base.StatusText, base.GetActionId("CompleteHarvest"), true, false, false);
		}
		if (!this.HasEnergyInput(null))
		{
			if (!base.HasStatusEffectOfType<StatusEffect_NoEnergy>())
			{
				base.AddStatusEffect(new StatusEffect_NoEnergy());
			}
			this.MyGameCard.CancelTimer(base.GetActionId("CompleteHarvest"));
		}
		else if (base.HasStatusEffectOfType<StatusEffect_NoEnergy>())
		{
			base.RemoveStatusEffect<StatusEffect_NoEnergy>();
		}
		if (!base.HasSewerConnected())
		{
			if (!base.HasStatusEffectOfType<StatusEffect_NoSewer>())
			{
				base.AddStatusEffect(new StatusEffect_NoSewer());
			}
			this.MyGameCard.CancelTimer(base.GetActionId("CompleteHarvest"));
		}
		else if (base.HasStatusEffectOfType<StatusEffect_NoSewer>())
		{
			base.RemoveStatusEffect<StatusEffect_NoSewer>();
		}
		base.UpdateCard();
	}

	string IEnergyConsumer.GetEnergyConsumptionString()
	{
		return base.GetEnergyInputString();
	}

	[Header("Cities options")]
	public int PollutionPerHarvest;
}
