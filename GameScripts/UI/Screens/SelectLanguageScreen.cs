using System;
using UnityEngine;

public class SelectLanguageScreen : SokScreen
{
	private void Start()
	{
		SokLanguage[] languages = SokLoc.Languages;
		for (int i = 0; i < languages.Length; i++)
		{
			SokLanguage language = languages[i];
			CustomButton customButton = Object.Instantiate<CustomButton>(PrefabManager.instance.ButtonPrefab);
			customButton.transform.SetParent(this.ButtonsParent);
			customButton.transform.localScale = Vector3.one;
			customButton.transform.localPosition = Vector3.zero;
			customButton.transform.localRotation = Quaternion.identity;
			customButton.name = language.LanguageName;
			customButton.TextMeshPro.text = SokLoc.GetLocalLanguageName(language.LanguageName);
			customButton.GetComponentInChildren<FontSetter>().LanguageOverride = language.LanguageName;
			customButton.Clicked += delegate
			{
				SokLoc.instance.SetLanguage(language.LanguageName);
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
