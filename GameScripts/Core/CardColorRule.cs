using System;

public class CardColorRule
{
	public CardColorRule(CardPalette palette, Predicate<CardData> pred)
	{
		this.Palette = palette;
		this.Predicate = pred;
	}

	public Predicate<CardData> Predicate;

	public CardPalette Palette;
}
