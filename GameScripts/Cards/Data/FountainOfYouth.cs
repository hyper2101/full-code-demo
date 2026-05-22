using System;

public class FountainOfYouth : Building
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		Curse curse = otherCard as Curse;
		return (curse != null && curse.CurseType == CurseType.Death) || base.CanHaveCard(otherCard);
	}
}
