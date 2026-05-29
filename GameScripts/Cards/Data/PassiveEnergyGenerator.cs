using System;
using System.Collections.Generic;

public class PassiveEnergyGenerator : EnergyGenerator
{
	public override void UpdateCard()
	{
		if (!this.MyGameCard.TimerRunning && !this.IsDamaged)
		{
			this.MyGameCard.StartTimer(this.CycleTime, new TimerAction(this.EndCycle), MewtationsLoc.Translate("card_energy_status_0"), base.GetActionId("EndCycle"), true, false, false);
		}
		base.UpdateCard();
	}

	public override bool HasEnergyOutput(CardConnector connectedNode, List<CardConnector> nodeTracker)
	{
		if (base.WorkerAmountMet())
		{
			this.hasEnergy = true;
		}
		else
		{
			this.hasEnergy = false;
		}
		if (this.hasEnergy != this.prevHasEnergy)
		{
			base.NotifyEnergyConsumers();
		}
		this.prevHasEnergy = this.hasEnergy;
		return this.hasEnergy;
	}

	[TimedAction("end_cycle")]
	public void EndCycle()
	{
	}

	public int EnergyGeneratedPerCycle = 1;

	public float CycleTime = 30f;

	[ExtraData("has_energy")]
	public bool hasEnergy;

	private bool prevHasEnergy;
}
