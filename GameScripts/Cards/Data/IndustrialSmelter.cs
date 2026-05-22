using System;

public class IndustrialSmelter : EnergyConsumer
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == "iron_ore" || otherCard.Id == "gold_ore" || otherCard.Id == "iron_bar" || otherCard.Id == "copper_ore" || otherCard.Id == "lumber" || otherCard is Worker;
	}

	public override void UpdateCard()
	{
		base.UpdateCard();
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}
}
