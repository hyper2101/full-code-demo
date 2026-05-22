using System;
using System.Collections.Generic;
using UnityEngine;

public class SewerCard : CardData
{
	private bool HasSewerConnector()
	{
		using (List<CardConnectorData>.Enumerator enumerator = this.EnergyConnectors.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.EnergyConnectionStrength == ConnectionType.Sewer)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void UpdateCard()
	{
		if (this.HasSewerConnector())
		{
			if (!base.HasSewerConnected())
			{
				if (!base.HasStatusEffectOfType<StatusEffect_NoSewer>())
				{
					base.AddStatusEffect(new StatusEffect_NoSewer());
				}
				this.shouldRunTimer = true;
			}
			else
			{
				base.RemoveStatusEffect<StatusEffect_NoSewer>();
				this.shouldRunTimer = false;
			}
		}
		this.CheckSpawnPoop();
		base.UpdateCard();
	}

	[TimedAction("check_spawn_poop")]
	public void CheckSpawnPoop()
	{
		if (this.shouldRunTimer)
		{
			this.PoopTimer += Time.deltaTime * WorldManager.instance.TimeScale;
		}
		if (this.PoopTimer >= 30f && (double)Random.value > 0.5)
		{
			CardData cardData = WorldManager.instance.CreateCard(base.Position, "poop", true, true, true);
			WorldManager.instance.StackSend(cardData.MyGameCard, this.OutputDir, null, true);
			this.PoopTimer = 0f;
		}
	}

	[ExtraData("poop_timer")]
	[HideInInspector]
	public float PoopTimer;

	private bool shouldRunTimer;
}
