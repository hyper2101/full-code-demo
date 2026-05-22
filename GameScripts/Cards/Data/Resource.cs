using System;

public class Resource : CardData
{
	public override bool CanHaveCardsWhileHasStatus()
	{
		return this.IsBuilding || base.CanHaveCardsWhileHasStatus();
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.MyCardType == CardType.Equipable || otherCard.MyCardType == CardType.Resources || otherCard.MyCardType == CardType.Humans || otherCard.MyCardType == CardType.Food || otherCard.Id == this.Id || (otherCard.MyCardType == CardType.Structures && !otherCard.IsBuilding) || otherCard.MyCardType == CardType.Weather;
	}
}
