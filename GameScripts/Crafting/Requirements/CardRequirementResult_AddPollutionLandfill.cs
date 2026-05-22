using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class CardRequirementResult_AddPollutionLandfill : CardRequirementResult
{
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
		Landfill landfill = card.CardData as Landfill;
		if (landfill != null && landfill.StoredPollution >= this.Amount)
		{
			landfill.StoredPollution += this.Amount;
		}
		card.CardData.UpdateRequirementResultsInStack(RequirementType.Pollution, -this.Amount, card);
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
				Icons.Pollution
			});
		}
		return string.Format("<color=#{0}><nobr>{1}{2}{3}</nobr></color>", new object[]
		{
			ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorSuccess),
			CitiesManager.GetAmountPrefix(this.Amount),
			this.Amount * multiplier,
			Icons.Pollution
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
				Icons.Pollution
			});
		}
		return string.Format("<color=#{0}><nobr>{1}{2}{3}</nobr></color>", new object[]
		{
			ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorSuccess),
			CitiesManager.GetAmountPrefix(this.Amount),
			this.Amount * multiplier,
			Icons.Pollution
		});
	}

	public int Amount;

	public bool IsNegative;
}
