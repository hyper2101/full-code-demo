using System;

public class Landmark : EnergyConsumer
{
	protected override bool CanSelectOutput()
	{
		return false;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return (this.Id == "laboratory" && (otherCard.Id == "science" || otherCard.Id == "fossil")) || base.CanHaveCard(otherCard);
	}
}
