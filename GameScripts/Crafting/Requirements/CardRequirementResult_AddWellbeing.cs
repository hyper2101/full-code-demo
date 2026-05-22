using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class CardRequirementResult_AddWellbeing : CardRequirementResult
{
	public override IEnumerator EndOfCutscenePerform(GameCard card)
	{
		return null;
	}

	public override RequirementType GetRequirementType()
	{
		return RequirementType.WellBeing;
	}

	public override IEnumerator Perform(GameCard card)
	{
		CitiesManager.instance.AddWellbeing(this.Amount);
		card.CardData.UpdateRequirementResultsInStack(RequirementType.WellBeing, this.Amount, card);
		return null;
	}

	public override string RequirementDescriptionNegative(int multiplier, GameCard card)
	{
		if (this.IsNegative)
		{
			return string.Format("<color=#{0}><nobr>{1}{2}{3}</nobr></color>", new object[]
			{
				ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorFailed),
				CitiesManager.GetAmountPrefix(this.Amount),
				this.Amount * multiplier,
				Icons.Wellbeing
			});
		}
		return string.Format("<color=#{0}><nobr>{1}{2}{3}</nobr></color>", new object[]
		{
			ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorSuccess),
			CitiesManager.GetAmountPrefix(this.Amount),
			this.Amount * multiplier,
			Icons.Wellbeing
		});
	}

	public override string RequirementDescriptionPositive(int multiplier, GameCard card)
	{
		if (this.IsNegative)
		{
			return string.Format("<color=#{0}><nobr>{1}{2}{3}</nobr></color>", new object[]
			{
				ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorFailed),
				CitiesManager.GetAmountPrefix(this.Amount),
				this.Amount * multiplier,
				Icons.Wellbeing
			});
		}
		return string.Format("<color=#{0}><nobr>{1}{2}{3}</nobr></color>", new object[]
		{
			ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorSuccess),
			CitiesManager.GetAmountPrefix(this.Amount),
			this.Amount * multiplier,
			Icons.Wellbeing
		});
	}

	public int Amount;

	public bool IsNegative;
}
