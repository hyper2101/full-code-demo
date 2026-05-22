using System;
using UnityEngine;

public class CitiesCombatable : Combatable, HousingConsumer
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is CitiesCombatable || base.CanHaveCard(otherCard);
	}

	[HideInInspector]
	public Apartment Housing
	{
		get
		{
			if (this.HousingUniqueId != null && WorldManager.instance.GetCardWithUniqueId(this.HousingUniqueId) != null)
			{
				return WorldManager.instance.GetCardWithUniqueId(this.HousingUniqueId).CardData as Apartment;
			}
			return null;
		}
		set
		{
			this.HousingUniqueId = ((value != null) ? value.UniqueId : "");
		}
	}

	public string HousingId
	{
		get
		{
			return this.HousingUniqueId;
		}
	}

	public override void OnInitialCreate()
	{
		this.Housing = null;
		base.OnInitialCreate();
	}

	public override void UpdateCard()
	{
		Apartment housing = this.Housing;
		bool flag = housing != null && !housing.IsDamaged && housing.HasEnergyInput(null);
		if (this.GetHousingSpaceRequired() == 0)
		{
			flag = true;
		}
		if (!flag && !base.HasStatusEffectOfType<StatusEffect_Homeless>())
		{
			base.AddStatusEffect(new StatusEffect_Homeless());
		}
		if (flag && base.HasStatusEffectOfType<StatusEffect_Homeless>())
		{
			base.RemoveStatusEffect<StatusEffect_Homeless>();
		}
		base.UpdateCard();
	}

	public override void UpdateCardText()
	{
		if (!string.IsNullOrEmpty(this.CustomName))
		{
			this.nameOverride = this.CustomName;
		}
		base.UpdateCardText();
	}

	public GameCard GetGameCard()
	{
		return this.MyGameCard;
	}

	public int GetHousingSpaceRequired()
	{
		return this.HousingSpaceRequired;
	}

	public WorkerType GetWorkerType()
	{
		if (this.Id == "robot_soldier")
		{
			return WorkerType.Robot;
		}
		return WorkerType.Normal;
	}

	public override void OnSellCard()
	{
		if (this.Housing != null)
		{
			this.Housing.UsedSpace -= this.GetHousingSpaceRequired();
			this.Housing = null;
		}
		base.OnSellCard();
	}

	public override void OnDestroyCard()
	{
		if (this.Housing != null)
		{
			this.Housing.UsedSpace -= this.GetHousingSpaceRequired();
			this.Housing = null;
		}
		base.OnDestroyCard();
	}

	public int HousingSpaceRequired = 1;

	[HideInInspector]
	[ExtraData("housingUniqueId")]
	public string HousingUniqueId;
}
