using System;
using System.Linq;

public class EnergyConsumer : SewerCard, IEnergyConsumer
{
	public override void OnInitialCreate()
	{
		if (this.MyGameCard.CardConnectorChildren.Count<CardConnector>((CardConnector x) => x.CardDirection == CardDirection.input && x.ConnectionType == ConnectionType.LV) > 0)
		{
			WorldManager.instance.Cutscene.QueueCutsceneIfNotPlayed("cities_first_energy");
		}
		base.OnInitialCreate();
	}

	protected override bool CanToggleOnOff()
	{
		return true;
	}

	protected override bool CanSelectOutput()
	{
		return true;
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	public override void UpdateCard()
	{
		if (!base.WorkerAmountMet() && this.MyGameCard.TimerRunning && !this.MyGameCard.SkipCitiesChecks)
		{
			this.MyGameCard.CancelAnyTimer();
		}
		if (!this.HasEnergyInput(null))
		{
			if (!base.HasStatusEffectOfType<StatusEffect_NoEnergy>())
			{
				base.AddStatusEffect(new StatusEffect_NoEnergy());
			}
			if (this.MyGameCard.TimerRunning && !this.MyGameCard.SkipCitiesChecks)
			{
				this.MyGameCard.CancelAnyTimer();
			}
		}
		else
		{
			base.RemoveStatusEffect<StatusEffect_NoEnergy>();
		}
		base.UpdateCard();
	}

	string IEnergyConsumer.GetEnergyConsumptionString()
	{
		return base.GetEnergyInputString();
	}
}
