using System;
using System.Collections.Generic;

public class BlueprintFountainOfYouth : Blueprint
{
	public override void BlueprintComplete(GameCard rootCard, List<GameCard> involvedCards, Subprint print)
	{
		base.BlueprintComplete(rootCard, involvedCards, print);
		foreach (GameCard gameCard in involvedCards)
		{
			if (gameCard != null)
			{
				gameCard.RemoveFromStack();
				gameCard.SendIt();
			}
		}
	}

	public override Subprint GetMatchingSubprint(GameCard card, out SubprintMatchInfo matchInfo)
	{
		Subprint matchingSubprint = base.GetMatchingSubprint(card, out matchInfo);
		if (matchingSubprint == null)
		{
			return null;
		}
		bool flag;
		if (card.CardData.AnyChildMatchesPredicate(delegate(CardData x)
		{
			BaseVillager baseVillager = x as BaseVillager;
			return baseVillager != null && baseVillager.MyLifeStage == LifeStage.Teenager;
		}))
		{
			if (card.CardData.AnyChildMatchesPredicate(delegate(CardData x)
			{
				BaseVillager baseVillager2 = x as BaseVillager;
				return baseVillager2 != null && baseVillager2.MyLifeStage == LifeStage.Adult;
			}))
			{
				flag = card.CardData.AnyChildMatchesPredicate(delegate(CardData x)
				{
					BaseVillager baseVillager3 = x as BaseVillager;
					return baseVillager3 != null && baseVillager3.MyLifeStage == LifeStage.Elderly;
				});
				goto IL_0093;
			}
		}
		flag = false;
		IL_0093:
		if (!flag)
		{
			return null;
		}
		return matchingSubprint;
	}
}
