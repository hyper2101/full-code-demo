using System;

public class WellbeingGenerator : Landmark, IEnergyConsumer
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is Worker;
	}

	protected override bool CanToggleOnOff()
	{
		return true;
	}

	public override void UpdateCard()
	{
		if (base.WorkerAmountMet() && !this.MyGameCard.TimerRunning)
		{
			this.MyGameCard.StartTimer((float)this.HarvestTime, new TimerAction(this.Complete), MewtationsLoc.Translate(this.StatusTerm), base.GetActionId("Complete"), true, false, false);
		}
		else if (!base.WorkerAmountMet())
		{
			this.MyGameCard.CancelAnyTimer();
		}
		if (!this.MyGameCard.TimerRunning && !this.HasEnergyInput(null))
		{
			if (!base.HasStatusEffectOfType<StatusEffect_NoEnergy>())
			{
				base.AddStatusEffect(new StatusEffect_NoEnergy());
			}
		}
		else
		{
			base.RemoveStatusEffect<StatusEffect_NoEnergy>();
		}
		base.UpdateCard();
	}

	[TimedAction("complete")]
	public void Complete()
	{
		CitiesManager.instance.AddWellbeing(this.WellbeingAmountPerCycle);
		WorldManager.instance.CreateFloatingText(this.MyGameCard, this.WellbeingAmountPerCycle > 0, this.WellbeingAmountPerCycle, MewtationsLoc.Translate(this.StatusResultTerm), Icons.Wellbeing, true, 0, 0f, true);
	}

	string IEnergyConsumer.GetEnergyConsumptionString()
	{
		return base.GetEnergyInputString();
	}

	public int WellbeingAmountPerCycle;

	public int HarvestTime = 10;

	[Term]
	public string StatusResultTerm;

	[Term]
	public string StatusTerm;
}
