using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OptionsScreen : SokScreen
{
	public override bool IsFrameRateUncapped
	{
		get
		{
			return true;
		}
	}

	private void Awake()
	{
		this.SelectSaveButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<SelectSaveScreen>();
		};
		this.ResolutionButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<SelectResolutionScreen>();
		};
		this.ControlsButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<ControlsScreen>();
		};
		this.AccessibilityButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<AccessibilityScreen>();
		};
		this.AdvancedSettingsButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<AdvancedSettingsScreen>();
		};
		this.FullscreenButton.Clicked += OptionsScreen.ToggleFullScreen;
		this.FrameRateCapButton.Clicked += this.ToggleFrameRateCap;
		this.UIScaleButton.Clicked += OptionsScreen.ToggleUIScale;
		this.ClearSaveButton.Clicked += delegate
		{
			GameCanvas.instance.ShowClearSaveModal();
		};
		this.LanguageButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<SelectLanguageScreen>();
		};
		this.MusicSlider.onValueChanged.AddListener(new UnityAction<float>(this.OnMusicVolumeChange));
		this.SfxSlider.onValueChanged.AddListener(new UnityAction<float>(this.OnSFXVolumeChange));
		this.CreditsButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<CreditsScreen>();
		};
		this.BackButton.Clicked += delegate
		{
			this.GoBack();
		};
		OptionsScreen.LoadSettings();
		this.MusicSlider.value = OptionsScreen.MusicVol;
		this.SfxSlider.value = OptionsScreen.SfxVol;
		SokLoc.instance.LanguageChanged += this.Instance_LanguageChanged;
	}

	public void OnMusicVolumeChange(float sliderValue)
	{
		OptionsScreen.MusicVol = sliderValue;
	}

	public void OnSFXVolumeChange(float sliderValue)
	{
		OptionsScreen.SfxVol = sliderValue;
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
		this.SetTexts();
	}

	private void GoBack()
	{
		if (WorldManager.instance.CurrentGameState == WorldManager.GameState.Paused)
		{
			GameCanvas.instance.SetScreen<PauseScreen>();
		}
		else
		{
			GameCanvas.instance.SetScreen<MainMenu>();
		}
		OptionsScreen.SaveSettings();
	}

	private void Start()
	{
		this.SetTexts();
	}

	private static void ToggleUIScale()
	{
		int num = OptionsScreen.UIScale.IndexOf(OptionsScreen.CurrentUIScale) + 1;
		if (num == OptionsScreen.UIScale.Count)
		{
			num = 0;
		}
		OptionsScreen.CurrentUIScale = OptionsScreen.UIScale[num];
		OptionsScreen.SetUIScale();
	}

	private static void ToggleFullScreen()
	{
		OptionsScreen.CurrentFullScreen = !OptionsScreen.CurrentFullScreen;
		OptionsScreen.SetResolution();
	}

	private void ToggleFrameRateCap()
	{
		int num = OptionsScreen.frameRates.IndexOf(OptionsScreen.CurrentFrameRate) + 1;
		if (num == OptionsScreen.frameRates.Count)
		{
			num = 0;
		}
		OptionsScreen.CurrentFrameRate = OptionsScreen.frameRates[num];
	}

	private void Update()
	{
		this.VersionText.text = "v" + Application.version;
		if (InputController.instance.CancelTriggered() && !GameCanvas.instance.ModalIsOpen)
		{
			this.GoBack();
		}
		this.SetTexts();
	}

	private void OnDisable()
	{
		OptionsScreen.SaveSettings();
	}

	private void SetTexts()
	{
		this.ResolutionButton.TextMeshPro.text = SokLoc.Translate("label_resolution", new LocParam[]
		{
			LocParam.Create("width", OptionsScreen.CurrentWidth.ToString()),
			LocParam.Create("height", OptionsScreen.CurrentHeight.ToString())
		});
		this.FullscreenButton.TextMeshPro.text = SokLoc.Translate("label_fullscreen", new LocParam[] { LocParam.Create("on_off", OptionsScreen.YesNo(OptionsScreen.CurrentFullScreen)) });
		this.FrameRateCapButton.TextMeshPro.text = SokLoc.Translate("label_framerate_cap", new LocParam[] { LocParam.Create("fps_cap", OptionsScreen.FramerateLabel(OptionsScreen.CurrentFrameRate)) });
		this.UIScaleButton.TextMeshPro.text = SokLoc.Translate("label_ui_scale", new LocParam[] { LocParam.Create("scale", OptionsScreen.UIScaleLabel(OptionsScreen.UIScale.IndexOf(OptionsScreen.CurrentUIScale))) });
		this.BackButton.TextMeshPro.text = SokLoc.Translate("label_back");
		this.MusicVolumeText.text = string.Format("{0}%", Mathf.RoundToInt(OptionsScreen.MusicVol * 100f));
		this.SfxVolumeText.text = string.Format("{0}%", Mathf.RoundToInt(OptionsScreen.SfxVol * 100f));
	}

	public static string YesNo(bool a)
	{
		if (!a)
		{
			return SokLoc.Translate("label_off");
		}
		return SokLoc.Translate("label_on");
	}

	public static string FramerateLabel(int i)
	{
		if (i <= -1)
		{
			if (i == -2)
			{
				return SokLoc.Translate("label_framerate_unlimited");
			}
			if (i == -1)
			{
				return SokLoc.Translate("label_framerate_vsync");
			}
		}
		else
		{
			if (i == 30)
			{
				return SokLoc.Translate("label_framerate_30");
			}
			if (i == 60)
			{
				return SokLoc.Translate("label_framerate_60");
			}
			if (i == 120)
			{
				return SokLoc.Translate("label_framerate_120");
			}
		}
		return SokLoc.Translate("label_framerate_unlimited");
	}

	public static string UIScaleLabel(int i)
	{
		string text;
		switch (i)
		{
		case 0:
			text = SokLoc.Translate("label_ui_scale_100");
			break;
		case 1:
			text = SokLoc.Translate("label_ui_scale_80");
			break;
		case 2:
			text = SokLoc.Translate("label_ui_scale_60");
			break;
		default:
			text = SokLoc.Translate("label_ui_scale_100");
			break;
		}
		return text;
	}

	public static void LoadSettings()
	{
		OptionsScreen.MusicOn = PlayerPrefs.GetInt("musicOn", 1) == 1;
		OptionsScreen.SfxOn = PlayerPrefs.GetInt("sfxOn", 1) == 1;
		OptionsScreen.MusicVol = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
		OptionsScreen.SfxVol = PlayerPrefs.GetFloat("SfxVolume", 0.8f);
		string languageName = SokLoc.DetermineSystemLanguage().LanguageName;
		string @string = PlayerPrefs.GetString("language", languageName);
		SokLoc.instance.SetLanguage(@string);
		int @int = PlayerPrefs.GetInt("width", -1);
		int int2 = PlayerPrefs.GetInt("height", -1);
		OptionsScreen.CurrentFullScreen = PlayerPrefs.GetInt("fullscreen", 1) == 1;
		OptionsScreen.CurrentFrameRate = PlayerPrefs.GetInt("framerate", -1);
		int int3 = PlayerPrefs.GetInt("uiScaleIndex", 0);
		if (int3 >= 0 && int3 < OptionsScreen.UIScale.Count)
		{
			OptionsScreen.CurrentUIScale = OptionsScreen.UIScale[int3];
		}
		else
		{
			OptionsScreen.CurrentUIScale = OptionsScreen.UIScale[0];
		}
		OptionsScreen.SetUIScale();
		if (@int != -1 && int2 != -1)
		{
			OptionsScreen.CurrentWidth = @int;
			OptionsScreen.CurrentHeight = int2;
			OptionsScreen.SetResolution();
			Debug.Log(string.Format("Loaded resolution {0}x{1}", OptionsScreen.CurrentWidth, OptionsScreen.CurrentHeight));
			return;
		}
		OptionsScreen.CurrentWidth = Screen.currentResolution.width;
		OptionsScreen.CurrentHeight = Screen.currentResolution.height;
		Debug.Log(string.Format("Set current resolution to {0}x{1}", OptionsScreen.CurrentWidth, OptionsScreen.CurrentHeight));
	}

	public static void SaveSettings()
	{
		PlayerPrefs.SetInt("musicOn", OptionsScreen.MusicOn ? 1 : 0);
		PlayerPrefs.SetInt("sfxOn", OptionsScreen.SfxOn ? 1 : 0);
		PlayerPrefs.SetFloat("MusicVolume", OptionsScreen.MusicVol);
		PlayerPrefs.SetFloat("SfxVolume", OptionsScreen.SfxVol);
		PlayerPrefs.SetInt("fullscreen", OptionsScreen.CurrentFullScreen ? 1 : 0);
		PlayerPrefs.SetInt("framerate", OptionsScreen.CurrentFrameRate);
		PlayerPrefs.SetInt("uiScaleIndex", OptionsScreen.UIScale.IndexOf(OptionsScreen.CurrentUIScale));
		PlayerPrefs.SetInt("width", OptionsScreen.CurrentWidth);
		PlayerPrefs.SetInt("height", OptionsScreen.CurrentHeight);
		PlayerPrefs.SetString("language", SokLoc.instance.CurrentLanguage);
	}

	private static bool IsSameResolution(Resolution a, Resolution b)
	{
		return a.width == b.width && a.height == b.height;
	}

	public static List<Resolution> PossibleResolutions()
	{
		List<Resolution> list = new List<Resolution>();
		List<Resolution> list2 = Screen.resolutions.ToList<Resolution>();
		list2.Sort((Resolution a, Resolution b) => a.width - b.width);
		for (int i = 0; i < list2.Count; i++)
		{
			Resolution resolution = list2[i];
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				if (OptionsScreen.IsSameResolution(resolution, list[j]))
				{
					flag = true;
					break;
				}
			}
			if (!flag && resolution.height != 0 && resolution.width != 0)
			{
				list.Add(resolution);
			}
		}
		return list;
	}

	public static void SetUIScale()
	{
		GameCanvas.instance.Canvas.GetComponent<CanvasScaler>().referenceResolution = OptionsScreen.CurrentUIScale;
	}

	public static void SetResolution()
	{
		OptionsScreen.CurrentWidth = ((OptionsScreen.CurrentWidth < 100) ? OptionsScreen.PossibleResolutions()[0].width : OptionsScreen.CurrentWidth);
		OptionsScreen.CurrentHeight = ((OptionsScreen.CurrentHeight < 100) ? OptionsScreen.PossibleResolutions()[0].height : OptionsScreen.CurrentHeight);
		Screen.SetResolution(OptionsScreen.CurrentWidth, OptionsScreen.CurrentHeight, OptionsScreen.CurrentFullScreen);
	}

	public static int CurrentWidth;

	public static int CurrentHeight;

	public static bool CurrentFullScreen;

	public static int CurrentFrameRate;

	public static Vector2 CurrentUIScale;

	public const bool DEBUG_RESOLUTION = false;

	public CustomButton ResolutionButton;

	public CustomButton FullscreenButton;

	public CustomButton UIScaleButton;

	public CustomButton FrameRateCapButton;

	public CustomButton LanguageButton;

	public CustomButton ClearSaveButton;

	public CustomButton CreditsButton;

	public CustomButton ControlsButton;

	public CustomButton AccessibilityButton;

	public CustomButton BackButton;

	public CustomButton AdvancedSettingsButton;

	public Slider MusicSlider;

	public Slider SfxSlider;

	public CanvasScaler CanvasScaler;

	public TextMeshProUGUI VersionText;

	public TextMeshProUGUI MusicVolumeText;

	public TextMeshProUGUI SfxVolumeText;

	public CustomButton SelectSaveButton;

	private static List<int> frameRates = new List<int> { -2, -1, 30, 60, 120 };

	private static List<Vector2> UIScale = new List<Vector2>
	{
		new Vector2(1920f, 1080f),
		new Vector2(2500f, 1080f),
		new Vector2(3440f, 2169f)
	};

	public static bool MusicOn;

	public static bool SfxOn;

	public static float MusicVol;

	public static float SfxVol;
}
