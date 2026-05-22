using System;
using System.Collections;

[Serializable]
public class CardRequirementResult_ConsumeFood : CardRequirementResult
{
	public override IEnumerator EndOfCutscenePerform(GameCard card)
	{
		return null;
	}

	public override RequirementType GetRequirementType()
	{
		return RequirementType.Food;
	}

	public override IEnumerator Perform(GameCard card)
	{
		yield return CitiesManager.instance.ConsumeFood(this.FoodAmount, card.Position);
		yield break;
	}

	public override string RequirementDescriptionNegative(int multiplier, GameCard card)
	{
		return "";
	}

	public override string RequirementDescriptionPositive(int multiplier, GameCard card)
	{
		return "";
	}

	public int FoodAmount;
}
