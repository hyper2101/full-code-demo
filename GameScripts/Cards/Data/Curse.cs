using System;

public class Curse : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == "royal_crown" || otherCard.Id == "euphoria" || otherCard.Id == "fountain_of_youth";
	}

	public CurseType CurseType;
}
