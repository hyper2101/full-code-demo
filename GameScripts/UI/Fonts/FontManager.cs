using System;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class FontManager : MonoBehaviour
{
	private void Awake()
	{
		FontManager.instance = this;
		this.UpdateWorldFontMaterial();
		if (SokLoc.instance != null)
		{
			SokLoc.instance.LanguageChanged += this.Instance_LanguageChanged;
		}
	}

	private void Start()
	{
		this.UpdateWorldFontMaterial();
	}

	private void OnDestroy()
	{
		if (SokLoc.instance != null)
		{
			SokLoc.instance.LanguageChanged -= this.Instance_LanguageChanged;
		}
	}

	private void Instance_LanguageChanged()
	{
		this.UpdateWorldFontMaterial();
	}

	private void Update()
	{
		FontManager.instance = this;
	}

	public void UpdateWorldFontMaterial()
	{
		if (SokLoc.instance != null)
		{
			TMP_FontAsset font = this.GetFont(FontType.World, SokLoc.instance.CurrentLanguage);
			this.WorldFontMaterial.CopyPropertiesFromMaterial(font.material);
			this.WorldFontMaterial.shader = this.WorldFontShader;
		}
	}

	public TMP_FontAsset GetFont(FontType fontType, string languageOverride = null)
	{
		if (SokLoc.instance != null)
		{
			string text = SokLoc.instance.CurrentLanguage;
			if (!string.IsNullOrEmpty(languageOverride))
			{
				text = languageOverride;
			}
			if (text == "Chinese (Traditional)")
			{
				return this.ChineseTraditionalFontAsset;
			}
			if (text == "Chinese (Simplified)")
			{
				return this.ChineseSimplifiedFontAsset;
			}
			if (text == "Japanese")
			{
				return this.JapaneseFontAsset;
			}
			if (text == "Korean")
			{
				return this.KoreanFontAsset;
			}
		}
		if (fontType == FontType.Regular)
		{
			return this.RegularFontAsset;
		}
		if (fontType == FontType.Rounded)
		{
			return this.TitleFontAsset;
		}
		if (fontType == FontType.World)
		{
			return this.WorldFontAsset;
		}
		throw new ArgumentException("Unknown fontType");
	}

	public static FontManager instance;

	public TMP_FontAsset RegularFontAsset;

	public TMP_FontAsset TitleFontAsset;

	public TMP_FontAsset WorldFontAsset;

	public TMP_FontAsset ChineseTraditionalFontAsset;

	public TMP_FontAsset ChineseSimplifiedFontAsset;

	public TMP_FontAsset JapaneseFontAsset;

	public TMP_FontAsset KoreanFontAsset;

	public Material WorldFontMaterial;

	public Shader WorldFontShader;
}
