using System;

[Serializable]
public class CardRequirement_HasHousing : CardRequirement
{
	public override string RequirementDescriptionNeed(int multiplier)
	{
		string text = string.Format("{0}", this.Amount * multiplier);
		return MewtationsLoc.Translate("label_requirement_has_housing", new LocParam[] { LocParam.Create("amount", text) });
	}

	public override string RequirementDescriptionNeedNegative(int multiplier)
	{
		string text = string.Format("{0}", this.Amount * multiplier);
		return MewtationsLoc.Translate("label_requirement_has_housing_negative", new LocParam[] { LocParam.Create("amount", text) });
	}

	public override bool Satisfied(GameCard card)
	{
		HousingConsumer housingConsumer = card.CardData as HousingConsumer;
		return housingConsumer == null || (!(housingConsumer.Housing == null) && !housingConsumer.Housing.IsDamaged && housingConsumer.Housing.HasEnergyInput(null));
	}

	public int Amount;
}
