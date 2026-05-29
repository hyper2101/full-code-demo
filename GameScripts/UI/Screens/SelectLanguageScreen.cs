using System;
using UnityEngine;

public class SelectLanguageScreen : MewtationsScreen
{
	private void Start()
	{
		MewtationsLanguage[] languages = MewtationsLoc.Languages;
		for (int i = 0; i < languages.Length; i++)
		{
			MewtationsLanguage language = languages[i];
			CustomButton customButton = Object.Instantiate<CustomButton>(PrefabManager.instance.ButtonPrefab);
			customButton.transform.SetParent(this.ButtonsParent);
			customButton.transform.localScale = Vector3.one;
			customButton.transform.localPosition = Vector3.zero;
			customButton.transform.localRotation = Quaternion.identity;
			customButton.name = language.LanguageName;
			customButton.TextMeshPro.text = MewtationsLoc.GetLocalLanguageName(language.LanguageName);
			customButton.GetComponentInChildren<FontSetter>().LanguageOverride = language.LanguageName;
			customButton.Clicked += delegate
			{
				MewtationsLoc.instance.SetLanguage(language.LanguageName);
				OptionsScreen.SaveSettings();
			};
		}
		this.BackButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<OptionsScreen>();
		};
	}

	public RectTransform ButtonsParent;

	public CustomButton BackButton;
}
