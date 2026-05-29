using System;

[Serializable]
public class CardRequirement_NeedsHealing : CardRequirement
{
	public override string RequirementDescriptionNeed(int multiplier)
	{
		return MewtationsLoc.Translate("label_requirement_needs_healing");
	}

	public override string RequirementDescriptionNeedNegative(int multiplier)
	{
		return MewtationsLoc.Translate("label_requirement_needs_healing_negative");
	}

	public override bool Satisfied(GameCard card)
	{
		CitiesCombatable citiesCombatable = card.CardData as CitiesCombatable;
		return citiesCombatable != null && citiesCombatable.HealthPoints < citiesCombatable.ProcessedCombatStats.MaxHealth;
	}
}
