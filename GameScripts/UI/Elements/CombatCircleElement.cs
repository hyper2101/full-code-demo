using System;

public class CombatCircleElement : Hoverable
{
	public override string GetTitle()
	{
		Combatable combatable = this.ParentCard.CardData as Combatable;
		if (combatable != null)
		{
			return combatable.GetCombatTypeTitle();
		}
		return "";
	}

	public override string GetDescription()
	{
		Combatable combatable = this.ParentCard.CardData as Combatable;
		if (combatable != null)
		{
			return "<i>" + combatable.GetCombatTypeLore() + "</i>\n\n" + combatable.GetCombatTypeDescription();
		}
		return "";
	}

	public GameCard ParentCard;
}
