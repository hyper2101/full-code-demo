using System;

[Serializable]
public class CardRequirement_InConflict : CardRequirement
{
	public override string RequirementDescriptionNeed(int multiplier)
	{
		return SokLoc.Translate("label_requirement_in_conflict");
	}

	public override string RequirementDescriptionNeedNegative(int multiplier)
	{
		return SokLoc.Translate("label_requirement_in_conflict_negative");
	}

	public override bool Satisfied(GameCard card)
	{
		Enemy enemy = card.CardData as Enemy;
		return enemy != null && enemy.InConflict;
	}
}
