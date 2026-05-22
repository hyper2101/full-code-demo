using System;

public class CardValue
{
	public int TotalValue
	{
		get
		{
			return this.BaseValue + this.ExtraValue;
		}
	}

	public CardValue(int baseValue)
	{
		this.BaseValue = baseValue;
	}

	public string ToValueString(GameBoard currentBoard)
	{
		string text = "";
		string text2 = Icons.Gold;
		if (currentBoard.Location == Location.Island)
		{
			text2 = Icons.Shell;
		}
		else if (currentBoard.Location == Location.Cities)
		{
			text2 = Icons.Dollar;
		}
		return text + string.Format("{0} {1}", this.BaseValue, text2);
	}

	public int BaseValue;

	public int ExtraValue;
}
