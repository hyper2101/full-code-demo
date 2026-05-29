using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Apartment : EnergyConsumer
{
	public void UpdateUsedSpace()
	{
		int num = 0;
		foreach (HousingConsumer housingConsumer in CitiesManager.instance.HousingConsumers)
		{
			if (housingConsumer.Housing == this)
			{
				num += housingConsumer.GetHousingSpaceRequired();
			}
		}
		this.UsedSpace = num;
	}

	public override void UpdateCard()
	{
		this.updateTimer -= Time.deltaTime;
		if (this.updateTimer <= 0f)
		{
			this.updateTimer = Random.Range(0.5f, 1f);
			this.UpdateUsedSpace();
		}
		this.FreeSpace = this.HousingSpace - this.UsedSpace;
		if (this.FreeSpace > 0)
		{
			if (!base.HasStatusEffectOfType<StatusEffect_Space>())
			{
				base.AddStatusEffect(new StatusEffect_Space());
			}
		}
		else
		{
			base.RemoveStatusEffect<StatusEffect_Space>();
		}
		if (CitiesManager.instance.HomelessHousingConsumers.Count > 0 && this.FreeSpace > 0)
		{
			int i = 0;
			while (i < CitiesManager.instance.HomelessHousingConsumers.Count)
			{
				HousingConsumer housingConsumer = CitiesManager.instance.HomelessHousingConsumers[i];
				if (housingConsumer == null)
				{
					goto IL_0127;
				}
				if (!housingConsumer.GetGameCard().Destroyed && (housingConsumer.GetWorkerType() != WorkerType.Robot || this.CanHouseRobotWorkers) && (!this.CanHouseRobotWorkers || housingConsumer.GetWorkerType() == WorkerType.Robot))
				{
					if (housingConsumer.GetHousingSpaceRequired() <= this.FreeSpace)
					{
						housingConsumer.Housing = this;
						this.UsedSpace += housingConsumer.GetHousingSpaceRequired();
						this.FreeSpace = this.HousingSpace - this.UsedSpace;
						CitiesManager.instance.HomelessHousingConsumers.RemoveAt(i);
						goto IL_0127;
					}
					goto IL_0127;
				}
				IL_0130:
				i++;
				continue;
				IL_0127:
				if (this.FreeSpace > 0)
				{
					goto IL_0130;
				}
				break;
			}
		}
		if (this.MyGameCard.HasChild && this.MyGameCard.Child.CardData is ICurrency)
		{
			if (base.ChildrenMatchingPredicate((CardData x) => x is ICurrency).Cast<ICurrency>().ToList<ICurrency>()
				.Sum<ICurrency>((ICurrency x) => x.CurrencyValue) >= 20)
			{
				if (!this.MyGameCard.TimerRunning)
				{
					this.MyGameCard.StartTimer(5f, new TimerAction(this.NewWorker), MewtationsLoc.Translate("label_recruiting_worker"), base.GetActionId("NewWorker"), true, false, false);
				}
			}
			else
			{
				this.MyGameCard.CancelTimer(base.GetActionId("NewWorker"));
			}
		}
		else
		{
			this.MyGameCard.CancelTimer(base.GetActionId("NewWorker"));
		}
		base.UpdateCard();
	}

	public override void UpdateCardText()
	{
		this.descriptionOverride = MewtationsLoc.Translate(this.DescriptionTerm, new LocParam[] { LocParam.Create("amount", this.HousingSpace.ToString()) });
		if (this.FreeSpace != 0 && this.MyGameCard != null && !this.MyGameCard.IsDemoCard)
		{
			this.descriptionOverride = this.descriptionOverride + ". " + MewtationsLoc.Translate("card_apartment_free_space", new LocParam[] { LocParam.Create("free", this.FreeSpace.ToString()) });
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is Apartment || otherCard is ICurrency || otherCard.Id == "copper_bar" || base.CanHaveCard(otherCard);
	}

	[TimedAction("new_worker")]
	public void NewWorker()
	{
		List<ICurrency> list = base.ChildrenMatchingPredicate((CardData x) => x is ICurrency).Cast<ICurrency>().ToList<ICurrency>();
		if (list.Sum<ICurrency>((ICurrency x) => x.CurrencyValue) >= 20)
		{
			CitiesManager.instance.TryUseDollars(list, 20, true, true, false);
			CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "worker", true, true, true);
			cardData.MyGameCard.RemoveFromStack();
			WorldManager.instance.StackSend(cardData.MyGameCard, this.OutputDir, null, true);
		}
	}

	public AudioClip SpawnWorkerSound;

	public int HousingSpace = 2;

	[HideInInspector]
	public int FreeSpace;

	[HideInInspector]
	[ExtraData("used_space")]
	public int UsedSpace;

	public bool CanHouseRobotWorkers;

	private float updateTimer = 1f;
}
