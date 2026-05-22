using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class CardRequirementResult_AddPollution : CardRequirementResult
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
		Pollution pollution = WorldManager.instance.CreateCard(card.Position, "pollution", true, true, true) as Pollution;
		pollution.PollutionAmount = this.Amount;
		pollution.MyGameCard.SendIt();
		card.CardData.UpdateRequirementResultsInStack(RequirementType.Pollution, -this.Amount, card);
		return null;
	}

	public override string RequirementDescriptionNegative(int multiplier, GameCard card)
	{
		return string.Format("<color=#{0}><nobr>{1}{2}{3}</nobr></color>", new object[]
		{
			ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorFailed),
			CitiesManager.GetAmountPrefix(this.Amount),
			this.Amount * multiplier,
			Icons.Pollution
		});
	}

	public override string RequirementDescriptionPositive(int multiplier, GameCard card)
	{
		return string.Format("<color=#{0}><nobr>{1}{2}{3}</nobr></color>", new object[]
		{
			ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorFailed),
			CitiesManager.GetAmountPrefix(this.Amount),
			this.Amount * multiplier,
			Icons.Pollution
		});
	}

	public int Amount;
}
