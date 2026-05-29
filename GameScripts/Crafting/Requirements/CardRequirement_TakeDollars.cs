using System;

[Serializable]
public class CardRequirement_TakeDollars : CardRequirement
{
	public override string RequirementDescriptionNeed(int multiplier)
	{
		string text = string.Format("{0}", this.Amount * multiplier);
		return MewtationsLoc.Translate("label_requirement_take_dollars", new LocParam[] { LocParam.Create("amount", text) });
	}

	public override string RequirementDescriptionNeedNegative(int multiplier)
	{
		string text = string.Format("{0}", this.Amount * multiplier);
		return MewtationsLoc.Translate("label_requirement_take_dollars_negative", new LocParam[] { LocParam.Create("amount", text) });
	}

	public override bool Satisfied(GameCard card)
	{
		return WorldManager.instance.Economy.GetDollarCount(true) >= this.Amount;
	}

	public int Amount;
}
