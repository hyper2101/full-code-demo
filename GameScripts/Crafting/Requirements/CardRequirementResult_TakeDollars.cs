using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class CardRequirementResult_TakeDollars : CardRequirementResult
{
	public override IEnumerator EndOfCutscenePerform(GameCard card)
	{
		return null;
	}

	public override RequirementType GetRequirementType()
	{
		return RequirementType.Dollar;
	}

	public override IEnumerator Perform(GameCard card)
	{
		AudioManager.me.PlaySound2D(AudioManager.me.Dollar, 1f, 0.5f);
		CitiesManager.instance.TryUseDollars(WorldManager.instance.CardQuery.GetCardsImplementingInterface<ICurrency>(), this.Amount, true, false, false);
		card.CardData.UpdateRequirementResultsInStack(RequirementType.Dollar, -this.Amount, card);
		return null;
	}

	public override string RequirementDescriptionNegative(int multiplier, GameCard card)
	{
		if (this.IsNegative)
		{
			return string.Format("<color=#{0}>-{1}{2}</nobr></color>", ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorFailed), this.Amount * multiplier, Icons.Dollar);
		}
		return string.Format("<color=#{0}>-{1}{2}</nobr></color>", ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorSuccess), this.Amount * multiplier, Icons.Dollar);
	}

	public override string RequirementDescriptionPositive(int multiplier, GameCard card)
	{
		if (this.IsNegative)
		{
			return string.Format("<color=#{0}>-{1}{2}</nobr></color>", ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorFailed), this.Amount * multiplier, Icons.Dollar);
		}
		return string.Format("<color=#{0}>-{1}{2}</nobr></color>", ColorUtility.ToHtmlStringRGB(ColorManager.instance.FloatingTextColorSuccess), this.Amount * multiplier, Icons.Dollar);
	}

	public int Amount;

	public bool IsNegative;
}
