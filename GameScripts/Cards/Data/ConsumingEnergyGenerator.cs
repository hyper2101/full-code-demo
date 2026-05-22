using System;
using System.Collections.Generic;
using System.Linq;

public class ConsumingEnergyGenerator : EnergyGenerator
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return this.CardsToConsume.Select<CardAmountPair, string>((CardAmountPair x) => x.CardId).Contains(otherCard.Id) || base.CanHaveCard(otherCard);
	}

	protected override bool CanToggleOnOff()
	{
		return true;
	}

	public override void UpdateCard()
	{
		if (this.MyGameCard.HasChild && (!this.MyGameCard.TimerRunning || this.MyGameCard.TimerActionId == base.GetActionId("StopEnergy")) && this.CardsToConsume.All<CardAmountPair>((CardAmountPair pair) => base.CardsInStackMatchingPredicate((CardData x) => x.Id == pair.CardId).Count >= pair.Amount))
		{
			if (!this.IsDamaged)
			{
				this.MyGameCard.StartTimer(this.CycleTime, new TimerAction(this.GenerateEnergy), SokLoc.Translate("card_energy_status_0"), base.GetActionId("GenerateEnergy"), true, false, false);
			}
			if (this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == base.GetActionId("GenerateEnergy"))
			{
				this.MyGameCard.CancelTimer(base.GetActionId("StopEnergy"));
				AudioManager.me.PlaySound2D(AudioManager.me.CardDestroy, 1f, 0.4f);
				using (List<CardAmountPair>.Enumerator enumerator = this.CardsToConsume.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						CardAmountPair pair = enumerator.Current;
						base.DestroyChildrenMatchingPredicateAndRestack((CardData x) => x.Id == pair.CardId, pair.Amount);
					}
				}
			}
		}
		if (this.MyGameCard.TimerRunning)
		{
			this.hasEnergy = true;
		}
		if (!this.MyGameCard.TimerRunning && this.hasEnergy)
		{
			this.MyGameCard.StartTimer(5f, new TimerAction(this.StopEnergy), SokLoc.Translate("card_energy_status_0"), base.GetActionId("StopEnergy"), false, true, true);
		}
		if (this.hasEnergy != this.prevHasEnergy)
		{
			base.NotifyEnergyConsumers();
		}
		this.prevHasEnergy = this.hasEnergy;
		base.UpdateCard();
	}

	public override bool HasEnergyOutput(CardConnector connectedNode, List<CardConnector> nodeTracker)
	{
		return this.hasEnergy;
	}

	[TimedAction("generate_energy")]
	public void GenerateEnergy()
	{
		if (this.PollutionAmount > 0)
		{
			WorldManager.instance.CreateCardStack(base.Position, this.PollutionAmount, "pollution", false);
		}
	}

	[TimedAction("stop_energy")]
	public void StopEnergy()
	{
		this.hasEnergy = false;
		AudioManager.me.PlaySound2D(AudioManager.me.PowerOutage, 1f, 0.4f);
	}

	public List<CardAmountPair> CardsToConsume = new List<CardAmountPair>();

	public int PollutionAmount;

	public float CycleTime = 15f;

	[ExtraData("has_energy")]
	public bool hasEnergy;

	private bool prevHasEnergy;
}
