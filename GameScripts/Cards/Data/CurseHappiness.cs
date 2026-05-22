using System;

public class CurseHappiness : Curse
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == "euphoria";
	}

	public override void UpdateCard()
	{
		if (this.MyGameCard.IsDemoCard)
		{
			this.descriptionOverride = SokLoc.Translate("card_happiness_curse_description");
		}
		else
		{
			this.descriptionOverride = GameScreen.instance.HappinessSummaryText;
		}
		base.UpdateCard();
	}
}
