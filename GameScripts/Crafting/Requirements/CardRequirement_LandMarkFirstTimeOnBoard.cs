using System;

[Serializable]
public class CardRequirement_LandMarkFirstTimeOnBoard : CardRequirement
{
	public override string RequirementDescriptionNeed(int multiplier)
	{
		return MewtationsLoc.Translate("label_requirement_landmark_first_time");
	}

	public override string RequirementDescriptionNeedNegative(int multiplier)
	{
		return MewtationsLoc.Translate("label_requirement_landmark_first_time_negative");
	}

	public override bool Satisfied(GameCard card)
	{
		return !WorldManager.instance.CurrentRunVariables.BuiltLandmarks.Contains(card.CardData.Id);
	}
}
