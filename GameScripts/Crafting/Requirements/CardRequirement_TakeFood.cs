using System;

[Serializable]
public class CardRequirement_TakeFood : CardRequirement
{
	public override string RequirementDescriptionNeed(int multiplier)
	{
		string text = string.Format("{0}", this.Amount * multiplier);
		return MewtationsLoc.Translate("label_requirement_take_food", new LocParam[] { LocParam.Create("amount", text) });
	}

	public override string RequirementDescriptionNeedNegative(int multiplier)
	{
		string text = string.Format("{0}", this.Amount * multiplier);
		return MewtationsLoc.Translate("label_requirement_take_food_negative", new LocParam[] { LocParam.Create("amount", text) });
	}

	public override bool Satisfied(GameCard card)
	{
		return CitiesManager.instance.GetFoodToUse(this.Amount).Count > 0;
	}

	public int Amount;
}
