using System;

public class Kid : CardData
{
	public override void UpdateCardText()
	{
		string text = MewtationsLoc.Translate(this.NameTerm);
		if (!string.IsNullOrEmpty(this.CustomName))
		{
			text = text + " " + this.CustomName;
		}
		this.nameOverride = text;
	}
}
