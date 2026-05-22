using System;

[Serializable]
public class CardRequirement_HasPollutionLandfill : CardRequirement
{
	public override string RequirementDescriptionNeed(int multiplier)
	{
		string text = string.Format("{0}", this.Amount * multiplier);
		return SokLoc.Translate("label_requirement_has_pollution_landfill", new LocParam[] { LocParam.Create("amount", text) });
	}

	public override string RequirementDescriptionNeedNegative(int multiplier)
	{
		string text = string.Format("{0}", this.Amount * multiplier);
		return SokLoc.Translate("label_requirement_has_pollution_landfill_negative", new LocParam[] { LocParam.Create("amount", text) });
	}

	public override bool Satisfied(GameCard card)
	{
		Landfill landfill = card.CardData as Landfill;
		RecyclingCenter recyclingCenter = card.CardData as RecyclingCenter;
		return (landfill != null && landfill.StoredPollution >= this.Amount) || (recyclingCenter != null && recyclingCenter.StoredPollution >= this.Amount);
	}

	public int Amount;
}
