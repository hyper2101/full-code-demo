using System;
using UnityEngine;

public class AccessibilityScreen : MewtationsScreen
{
	private void Awake()
	{
		this.AutoPauseWhenUsingControllerButton.Clicked += delegate
		{
			AccessibilityScreen.AutoPauseWhenUsingController = !AccessibilityScreen.AutoPauseWhenUsingController;
			this.SaveSettings();
		};
		this.AutoPauseWhenUsingKeyboardMouseButton.Clicked += delegate
		{
			AccessibilityScreen.AutoPauseWhenUsingKeyboardMouse = !AccessibilityScreen.AutoPauseWhenUsingKeyboardMouse;
			this.SaveSettings();
		};
		this.ScreenShakeButton.Clicked += delegate
		{
			AccessibilityScreen.ScreenshakeEnabled = !AccessibilityScreen.ScreenshakeEnabled;
			this.SaveSettings();
		};
		this.ClickToDragButton.Clicked += delegate
		{
			AccessibilityScreen.ClickToDragEnabled = !AccessibilityScreen.ClickToDragEnabled;
			this.SaveSettings();
		};
		this.DisablePausedTextButton.Clicked += delegate
		{
			AccessibilityScreen.FlashingPausedEnabled = !AccessibilityScreen.FlashingPausedEnabled;
			this.SaveSettings();
		};
		this.BackButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<OptionsScreen>();
		};
		this.LoadSettings();
	}

	private void OnApplicationQuit()
	{
		this.SaveSettings();
	}

	private void LoadSettings()
	{
		AccessibilityScreen.AutoPauseWhenUsingController = PlayerPrefs.GetInt("AutoPauseWithController", 1) == 1;
		AccessibilityScreen.AutoPauseWhenUsingKeyboardMouse = PlayerPrefs.GetInt("AutoPauseWithKeyboardMouse", 0) == 1;
		AccessibilityScreen.ScreenshakeEnabled = PlayerPrefs.GetInt("ScreenshakeEnabled", 1) == 1;
		AccessibilityScreen.ClickToDragEnabled = PlayerPrefs.GetInt("ClickToDragEnabled", 0) == 1;
		AccessibilityScreen.FlashingPausedEnabled = PlayerPrefs.GetInt("FlashingPausedEnabled", 0) == 1;
	}

	private void SaveSettings()
	{
		PlayerPrefs.SetInt("AutoPauseWithController", AccessibilityScreen.AutoPauseWhenUsingController ? 1 : 0);
		PlayerPrefs.SetInt("AutoPauseWithKeyboardMouse", AccessibilityScreen.AutoPauseWhenUsingKeyboardMouse ? 1 : 0);
		PlayerPrefs.SetInt("ScreenshakeEnabled", AccessibilityScreen.ScreenshakeEnabled ? 1 : 0);
		PlayerPrefs.SetInt("ClickToDragEnabled", AccessibilityScreen.ClickToDragEnabled ? 1 : 0);
		PlayerPrefs.SetInt("FlashingPausedEnabled", AccessibilityScreen.FlashingPausedEnabled ? 1 : 0);
	}

	private void Update()
	{
		this.AutoPauseWhenUsingControllerButton.TextMeshPro.text = MewtationsLoc.Translate("label_auto_pause_controller") + " " + OptionsScreen.YesNo(AccessibilityScreen.AutoPauseWhenUsingController);
		this.AutoPauseWhenUsingKeyboardMouseButton.TextMeshPro.text = MewtationsLoc.Translate("label_auto_pause_keyboardmouse") + " " + OptionsScreen.YesNo(AccessibilityScreen.AutoPauseWhenUsingKeyboardMouse);
		this.ScreenShakeButton.TextMeshPro.text = MewtationsLoc.Translate("label_screenshake_enabled") + ": " + OptionsScreen.YesNo(AccessibilityScreen.ScreenshakeEnabled);
		this.ClickToDragButton.TextMeshPro.text = MewtationsLoc.Translate("label_clicktodrag_enabled") + ": " + OptionsScreen.YesNo(AccessibilityScreen.ClickToDragEnabled);
		this.DisablePausedTextButton.TextMeshPro.text = MewtationsLoc.Translate("label_disable_paused_text", new LocParam[] { LocParam.Create("on_off", OptionsScreen.YesNo(AccessibilityScreen.FlashingPausedEnabled)) });
		this.ClickToDragButton.TooltipText = MewtationsLoc.Translate("label_clicktodrag_tooltip");
	}

	public CustomButton BackButton;

	public CustomButton AutoPauseWhenUsingControllerButton;

	public CustomButton AutoPauseWhenUsingKeyboardMouseButton;

	public CustomButton ScreenShakeButton;

	public CustomButton ClickToDragButton;

	public CustomButton DisablePausedTextButton;

	public static bool AutoPauseWhenUsingController = true;

	public static bool AutoPauseWhenUsingKeyboardMouse = false;

	public static bool ScreenshakeEnabled = true;

	public static bool ClickToDragEnabled = false;

	public static bool FlashingPausedEnabled = true;
}
