using System;

[Serializable]
public class CardRequirement_TakeCard : CardRequirement
{
	public override string RequirementDescriptionNeed(int multiplier)
	{
		return "";
	}

	public override string RequirementDescriptionNeedNegative(int multiplier)
	{
		return "";
	}

	public override bool Satisfied(GameCard card)
	{
		return WorldManager.instance.GetCards(this.CardId).Count >= this.Amount;
	}

	[Card]
	public string CardId;

	public int Amount;
}
