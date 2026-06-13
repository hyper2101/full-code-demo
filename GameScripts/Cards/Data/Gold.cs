using System;

[Obsolete("Legacy currency. Use standardized economy definitions instead.")]
public class Gold : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.MyCardType == CardType.Resources || otherCard.MyCardType == CardType.Humans;
	}
}
