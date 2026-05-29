using System;

[Serializable]
public class CardRequirement_HasEnergyWorkers : CardRequirement
{
	public override string RequirementDescriptionNeed(int multiplier)
	{
		return MewtationsLoc.Translate("label_requirement_energy_workers");
	}

	public override string RequirementDescriptionNeedNegative(int multiplier)
	{
		return MewtationsLoc.Translate("label_requirement_energy_workers_negative");
	}

	public override bool Satisfied(GameCard card)
	{
		return card.CardData.HasEnergyInput(null) && card.CardData.WorkerAmount > 0 && card.CardData.WorkerAmountMet();
	}
}
