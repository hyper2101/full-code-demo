using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class CardRequirementResult_UsePollution : CardRequirementResult
{
	public int WellbeingAmount
	{
		get
		{
			Pollution card = WorldManager.instance.CardQuery.GetCard<Pollution>();
			if (card != null && card.PollutionAmount > 0)
			{
				return -Mathf.RoundToInt((float)(card.PollutionAmount / this.PollutionPerWellbeing));
			}
			return 0;
		}
	}

	public override IEnumerator EndOfCutscenePerform(GameCard card)
	{
		return null;
	}

	public override RequirementType GetRequirementType()
	{
		return RequirementType.Pollution;
	}

	public override IEnumerator Perform(GameCard card)
	{
		Pollution pollution = card.CardData as Pollution;
		if (pollution != null)
		{
			int num = -Mathf.RoundToInt((float)(pollution.PollutionAmount / this.PollutionPerWellbeing));
			CitiesManager.instance.AddWellbeing(num);
			card.CardData.UpdateRequirementResultsInStack(RequirementType.Pollution, num, card);
		}
		return null;
	}

	public override string RequirementDescriptionNegative(int multiplier, GameCard card)
	{
		return string.Format("<color=#{0}><nobr>{1}{2}</nobr></color>", ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorSuccess), -1 * multiplier, Icons.Wellbeing);
	}

	public override string RequirementDescriptionPositive(int multiplier, GameCard card)
	{
		return string.Format("<color=#{0}><nobr>{1}{2}</nobr></color>", ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorFailed), -1 * multiplier, Icons.Wellbeing);
	}

	public int PollutionPerWellbeing;
}
