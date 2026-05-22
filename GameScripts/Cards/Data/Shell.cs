using System;

public class Shell : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.MyCardType == CardType.Resources || otherCard.MyCardType == CardType.Humans;
	}
}
