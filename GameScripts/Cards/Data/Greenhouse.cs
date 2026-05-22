using System;

public class Greenhouse : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.MyCardType == CardType.Resources || otherCard.MyCardType == CardType.Humans || otherCard.MyCardType == CardType.Food;
	}
}
