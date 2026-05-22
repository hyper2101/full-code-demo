using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class CardRequirementResult_HealCombatable : CardRequirementResult
{
	public override IEnumerator EndOfCutscenePerform(GameCard card)
	{
		return null;
	}

	public override RequirementType GetRequirementType()
	{
		return RequirementType.Health;
	}

	public override IEnumerator Perform(GameCard card)
	{
		CitiesCombatable citiesCombatable = card.CardData as CitiesCombatable;
		if (citiesCombatable != null)
		{
			citiesCombatable.HealthPoints = Mathf.Min(citiesCombatable.HealthPoints + this.HealthPerMonth, citiesCombatable.ProcessedCombatStats.MaxHealth);
		}
		return null;
	}

	public override string RequirementDescriptionNegative(int multiplier, GameCard card)
	{
		return "";
	}

	public override string RequirementDescriptionPositive(int multiplier, GameCard card)
	{
		return "";
	}

	public bool IsNegative;

	public int HealthPerMonth;
}
