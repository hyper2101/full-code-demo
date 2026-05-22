using System;
using System.Collections.Generic;

public class CardStringSplitter
{
	public static CardStringSplitter me
	{
		get
		{
			if (CardStringSplitter._instance == null)
			{
				CardStringSplitter._instance = new CardStringSplitter();
			}
			return CardStringSplitter._instance;
		}
	}

	public string[] Split(string s)
	{
		string[] array;
		if (this.stringToSplit.TryGetValue(s, out array))
		{
			return array;
		}
		string[] array2 = s.Split('|', StringSplitOptions.None);
		this.stringToSplit[s] = array2;
		return array2;
	}

	private static CardStringSplitter _instance;

	private Dictionary<string, string[]> stringToSplit = new Dictionary<string, string[]>();
}
