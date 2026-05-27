using Mewtations.Core;
using System;
using UnityEngine;

[Mewtations.Core.LegacySystem(Mewtations.Core.LegacyCategory.DeprecatedAutomation)]
    public class Battery : CardData, IEnergy
{
	public int EnergyAmount
	{
		get
		{
			return this.StoredEnergy;
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is Energy;
	}

	public override void UpdateCard()
	{
		this.MyGameCard.SpecialIcon.sprite = this.SpecialIcon;
		this.MyGameCard.SpecialValue = new int?(this.StoredEnergy);
		if (this.MyGameCard.HasChild)
		{
			foreach (GameCard gameCard in this.MyGameCard.GetChildCards())
			{
				if (this.StoredEnergy >= this.EnergyCapacity)
				{
					gameCard.RemoveFromParent();
					break;
				}
				this.StoredEnergy++;
				gameCard.DestroyCard(true, true);
			}
		}
		base.UpdateCard();
	}

	public void UseEnergy(int energyAmount)
	{
		this.StoredEnergy -= energyAmount;
		WorldManager.instance.CreateMinusElectricity(base.Position);
	}

	public CardData GetCardData()
	{
		return this;
	}

	public int EnergyCapacity = 50;

	[ExtraData("stored_energy")]
	public int StoredEnergy;

	public Sprite SpecialIcon;
}


