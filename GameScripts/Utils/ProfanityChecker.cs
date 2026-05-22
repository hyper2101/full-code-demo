using System;
using System.Collections.Generic;
using UnityEngine;

public class ProfanityChecker
{
	public ProfanityChecker()
	{
		this.profanitySheet = Resources.Load<SokSheet>("Sheets/ProfanitySheet");
		this.Load();
	}

	public bool IsProfanityInLanguage(string language, string word)
	{
		return !string.IsNullOrWhiteSpace(word) && this.languageProfanities.ContainsKey(language) && this.languageProfanities[language].Set.Contains(word);
	}

	public bool ContainsProfanityInLanguage(string language, string word)
	{
		if (!this.languageProfanities.ContainsKey(language))
		{
			return false;
		}
		ProfanityChecker.LanguageProfanities languageProfanities = this.languageProfanities[language];
		word = word.ToLower();
		for (int i = 0; i < languageProfanities.List.Count; i++)
		{
			if (word.Contains(languageProfanities.List[i]))
			{
				return true;
			}
		}
		return false;
	}

	private void Load()
	{
		for (int i = 0; i < this.profanitySheet.Table[0].GetLength(0); i++)
		{
			string text = this.profanitySheet.Table[0][i];
			ProfanityChecker.LanguageProfanities languageProfanities = new ProfanityChecker.LanguageProfanities(text);
			for (int j = 1; j < this.profanitySheet.Table.GetLength(0); j++)
			{
				foreach (string text2 in this.profanitySheet.Table[j][i].Split(',', StringSplitOptions.None))
				{
					languageProfanities.AddWord(text2);
				}
			}
			this.languageProfanities.Add(text, languageProfanities);
		}
	}

	private SokSheet profanitySheet;

	private Dictionary<string, ProfanityChecker.LanguageProfanities> languageProfanities = new Dictionary<string, ProfanityChecker.LanguageProfanities>();

	private class LanguageProfanities
	{
		public LanguageProfanities(string language)
		{
			this.Language = language;
		}

		public void AddWord(string s)
		{
			if (string.IsNullOrWhiteSpace(s))
			{
				return;
			}
			s = s.ToLower();
			this.List.Add(s);
			this.Set.Add(s);
		}

		public string Language;

		public List<string> List = new List<string>();

		public HashSet<string> Set = new HashSet<string>();
	}
}
