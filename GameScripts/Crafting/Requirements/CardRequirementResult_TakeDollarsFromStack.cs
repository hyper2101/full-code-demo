using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class CardRequirementResult_TakeDollarsFromStack : CardRequirementResult
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
		List<GameCard> allCardsInStack = card.GetAllCardsInStack();
		List<ICurrency> currencyList = card.CardData.ChildrenMatchingPredicate((CardData x) => x is ICurrency).Cast<ICurrency>().ToList<ICurrency>();
		allCardsInStack.RemoveAll((GameCard x) => currencyList.Select<ICurrency, CardData>((ICurrency x) => x.Card).Contains(x.CardData));
		CitiesManager.instance.TryUseDollars(currencyList, this.Amount, true, false, false);
		allCardsInStack.AddRange(currencyList.Select<ICurrency, GameCard>((ICurrency x) => x.Card.MyGameCard));
		WorldManager.instance.Restack(allCardsInStack);
		card.CardData.UpdateRequirementResultsInStack(RequirementType.Dollar, -this.Amount, card);
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

	public int Amount;

	public bool IsNegative;
}
