using System;

[Serializable]
public class CardRequirement_WorkerAmountMet : CardRequirement
{
	public override string RequirementDescriptionNeed(int multiplier)
	{
		return MewtationsLoc.Translate("label_requirement_worker_amount_met");
	}

	public override string RequirementDescriptionNeedNegative(int multiplier)
	{
		return MewtationsLoc.Translate("label_requirement_worker_amount_met_negative");
	}

	public override bool Satisfied(GameCard card)
	{
		return card.CardData.WorkerAmount > 0 && card.CardData.WorkerAmountMet();
	}
}
