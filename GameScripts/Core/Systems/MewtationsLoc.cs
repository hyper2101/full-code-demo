using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public class MewtationsLoc : MonoBehaviour
{
	// (get) Token: 0x06000030 RID: 48 RVA: 0x00002C33 File Offset: 0x00000E33
	public static MewtationsLanguage DefaultLanguage
	{
		get
		{
			return MewtationsLoc.Languages[0];
		}
	}
	// (get) Token: 0x06000031 RID: 49 RVA: 0x00002C3C File Offset: 0x00000E3C
	public LoadedLocSet CurrentLocSet
	{
		get
		{
			if (this.currentLocSet == null || this.currentLocSet.Language != this.CurrentLanguage)
			{
				this.currentLocSet = new LoadedLocSet(LocResources.Default, this.CurrentLanguage);
			}
			return this.currentLocSet;
		}
	}
	// (get) Token: 0x06000032 RID: 50 RVA: 0x00002C7A File Offset: 0x00000E7A
	public static LoadedLocSet FallbackSet
	{
		get
		{
			if (MewtationsLoc.fallbackLocSet == null)
			{
				MewtationsLoc.fallbackLocSet = new LoadedLocSet(LocResources.Default, "English");
			}
			return MewtationsLoc.fallbackLocSet;
		}
	}
	// (add) Token: 0x06000033 RID: 51 RVA: 0x00002C9C File Offset: 0x00000E9C
	// (remove) Token: 0x06000034 RID: 52 RVA: 0x00002CD4 File Offset: 0x00000ED4
	public event Action LanguageChanged;
	private void Awake()
	{
		MewtationsLoc.instance = this;
	}
	public void SetLanguage(string language)
	{
		if (this.GetLanguageWithName(language) == null)
		{
			throw new ArgumentException(language + " is not a valid language");
		}
		this.CurrentLanguage = language;
		Action languageChanged = this.LanguageChanged;
		if (languageChanged == null)
		{
			return;
		}
		languageChanged();
	}
	public MewtationsLanguage GetLanguageWithName(string languageName)
	{
		foreach (MewtationsLanguage MewtationsLanguage in MewtationsLoc.Languages)
		{
			if (MewtationsLanguage.LanguageName == languageName)
			{
				return MewtationsLanguage;
			}
		}
		return null;
	}
	public static string Translate(string termId)
	{
		if (MewtationsLoc.instance != null)
		{
			string text = MewtationsLoc.instance.CurrentLocSet.TranslateTerm(termId);
			if (!string.IsNullOrWhiteSpace(text))
			{
				return text;
			}
		}
		return MewtationsLoc.FallbackSet.TranslateTerm(termId);
	}
	public static string Translate(string termId, params LocParam[] locParams)
	{
		if (MewtationsLoc.instance != null)
		{
			string text = MewtationsLoc.instance.CurrentLocSet.TranslateTerm(termId, locParams);
			if (!string.IsNullOrWhiteSpace(text))
			{
				return text;
			}
		}
		return MewtationsLoc.FallbackSet.TranslateTerm(termId, locParams);
	}
	public static MewtationsLanguage DetermineSystemLanguage()
	{
		foreach (MewtationsLanguage MewtationsLanguage in MewtationsLoc.Languages)
		{
			if (MewtationsLanguage.UnitySystemLanguage == Application.systemLanguage)
			{
				return MewtationsLanguage;
			}
		}
		return MewtationsLoc.DefaultLanguage;
	}
	public static string GetLocalLanguageName(string languageName)
	{
		return MewtationsLoc.localLanguageNames[languageName];
	}
	public void LoadTermsFromFile(string path, bool disableWarning = false)
	{
		string[][] array = MewtationsLoc.ParseTableFromTsv(File.ReadAllText(path));
		int languageColumnIndex = MewtationsLoc.GetLanguageColumnIndex(array, this.CurrentLanguage);
		if (languageColumnIndex == -1)
		{
			return;
		}
		for (int i = 1; i < array.Length; i++)
		{
			string term = array[i][0];
			string text = array[i][languageColumnIndex];
			term = term.Trim().ToLower();
			if (!string.IsNullOrEmpty(term))
			{
				MewtationsTerm MewtationsTerm = new MewtationsTerm(this.CurrentLocSet, term, text);
				if (this.CurrentLocSet.TermLookup.ContainsKey(term))
				{
					if (!disableWarning)
					{
						Debug.LogError("Term " + term + " has been found more than once in the localisation sheet. Using last item in sheet.");
					}
					this.CurrentLocSet.TermLookup[term] = MewtationsTerm;
					this.CurrentLocSet.AllTerms.RemoveAll((MewtationsTerm x) => x.Id == term);
					this.CurrentLocSet.AllTerms.Add(MewtationsTerm);
				}
				else
				{
					this.CurrentLocSet.AllTerms.Add(MewtationsTerm);
					this.CurrentLocSet.TermLookup.Add(term, MewtationsTerm);
				}
			}
		}
	}
	public static string[][] ParseTableFromTsv(string tsv)
	{
		string[] array = tsv.Trim().Split('\n', StringSplitOptions.None);
		string[][] array2 = new string[array.Length][];
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i];
			text = text.Replace("\r", "");
			text = text.Replace("#", "\n");
			array2[i] = text.Split('\t', StringSplitOptions.None);
		}
		Debug.Log(string.Format("Parsed {0} rows", array.Length - 1));
		return array2;
	}
	public static int GetLanguageColumnIndex(string[][] table, string language)
	{
		for (int i = 0; i < table[0].Length; i++)
		{
			if (table[0][i] == language)
			{
				return i;
			}
		}
		Debug.LogError("No column exists for " + language);
		return -1;
	}
	public static MewtationsLoc instance;
	public static MewtationsLanguage[] Languages = new MewtationsLanguage[]
	{
		new MewtationsLanguage
		{
			LanguageName = "English",
			UnitySystemLanguage = SystemLanguage.English
		},
		new MewtationsLanguage
		{
			LanguageName = "Dutch",
			UnitySystemLanguage = SystemLanguage.Dutch
		},
		new MewtationsLanguage
		{
			LanguageName = "French",
			UnitySystemLanguage = SystemLanguage.French
		},
		new MewtationsLanguage
		{
			LanguageName = "Italian",
			UnitySystemLanguage = SystemLanguage.Italian
		},
		new MewtationsLanguage
		{
			LanguageName = "German",
			UnitySystemLanguage = SystemLanguage.German
		},
		new MewtationsLanguage
		{
			LanguageName = "Spanish",
			UnitySystemLanguage = SystemLanguage.Spanish
		},
		new MewtationsLanguage
		{
			LanguageName = "Polish",
			UnitySystemLanguage = SystemLanguage.Polish
		},
		new MewtationsLanguage
		{
			LanguageName = "Brazilian Portuguese",
			UnitySystemLanguage = SystemLanguage.Portuguese
		},
		new MewtationsLanguage
		{
			LanguageName = "Chinese (Traditional)",
			UnitySystemLanguage = SystemLanguage.ChineseTraditional
		},
		new MewtationsLanguage
		{
			LanguageName = "Chinese (Simplified)",
			UnitySystemLanguage = SystemLanguage.ChineseSimplified
		},
		new MewtationsLanguage
		{
			LanguageName = "Japanese",
			UnitySystemLanguage = SystemLanguage.Japanese
		},
		new MewtationsLanguage
		{
			LanguageName = "Korean",
			UnitySystemLanguage = SystemLanguage.Korean
		}
	};
	private static Dictionary<string, string> localLanguageNames = new Dictionary<string, string>
	{
		{ "English", "English" },
		{ "Dutch", "Nederlands" },
		{ "French", "Français" },
		{ "Italian", "Italiano" },
		{ "German", "Deutsch" },
		{ "Spanish", "Español" },
		{ "Polish", "Polski" },
		{ "Brazilian Portuguese", "Português (Brasil)" },
		{ "Chinese (Traditional)", "????" },
		{ "Chinese (Simplified)", "????" },
		{ "Japanese", "???" },
		{ "Korean", "???" }
	};
	private LoadedLocSet currentLocSet;
	private static LoadedLocSet fallbackLocSet;
	public string CurrentLanguage = "Dutch";
}

