using System;

[Serializable]
public class SimpleCardChance
{
	public SimpleCardChance(string cardId, int chance)
	{
		this.CardId = cardId;
		this.Chance = chance;
	}

	public SimpleCardChance()
	{
	}

	[Card]
	public string CardId;

	public int Chance = 1;
}
