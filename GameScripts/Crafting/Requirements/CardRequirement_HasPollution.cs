using System;

[Serializable]
public class CardRequirement_HasPollution : CardRequirement
{
	public override string RequirementDescriptionNeed(int multiplier)
	{
		string text = string.Format("{0}", this.Amount * multiplier);
		return SokLoc.Translate("label_requirement_has_pollution", new LocParam[] { LocParam.Create("amount", text) });
	}

	public override string RequirementDescriptionNeedNegative(int multiplier)
	{
		string text = string.Format("{0}", this.Amount * multiplier);
		return SokLoc.Translate("label_requirement_has_pollution_negative", new LocParam[] { LocParam.Create("amount", text) });
	}

	public override bool Satisfied(GameCard card)
	{
		Pollution pollution = card.CardData as Pollution;
		return pollution != null && pollution.PollutionAmount >= this.Amount;
	}

	public int Amount;
}
