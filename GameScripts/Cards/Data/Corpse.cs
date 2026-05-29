using System;

public class Corpse : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is Corpse;
	}

	public override void UpdateCardText()
	{
		string text = MewtationsLoc.Translate(this.NameTerm);
		if (!string.IsNullOrEmpty(this.CustomName))
		{
			text = MewtationsLoc.Translate("card_corpse_name_long", new LocParam[] { LocParam.Create("name", this.CustomName) });
		}
		this.nameOverride = text;
	}
}
