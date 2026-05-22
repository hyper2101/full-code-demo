using System;

public class EnergyDemand
{
	public EnergyDemand(CardData consumer, int energyAmount)
	{
		this.Consumer = consumer;
		this.EnergyAmount = energyAmount;
	}

	public CardData Consumer;

	public int EnergyAmount;
}
