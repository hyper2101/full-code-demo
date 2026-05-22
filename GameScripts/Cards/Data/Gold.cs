using System;

public class Gold : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.MyCardType == CardType.Resources || otherCard.MyCardType == CardType.Humans;
	}
}
