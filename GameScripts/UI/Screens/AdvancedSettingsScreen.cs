using System;
using UnityEngine;

public class AdvancedSettingsScreen : SokScreen
{
	private void Awake()
	{
		this.AdvancedCombatStatsButton.Clicked += delegate
		{
			AdvancedSettingsScreen.AdvancedCombatStatsEnabled = !AdvancedSettingsScreen.AdvancedCombatStatsEnabled;
			this.SaveSettings();
		};
		this.BackButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<OptionsScreen>();
		};
		this.LoadSettings();
	}

	private void Update()
	{
		this.AdvancedCombatStatsButton.TextMeshPro.text = SokLoc.Translate("label_advanced_combat", new LocParam[] { LocParam.Create("on_off", OptionsScreen.YesNo(AdvancedSettingsScreen.AdvancedCombatStatsEnabled)) });
	}

	private void OnApplicationQuit()
	{
		this.SaveSettings();
	}

	private void LoadSettings()
	{
		AdvancedSettingsScreen.AdvancedCombatStatsEnabled = PlayerPrefs.GetInt("AdvancedCombatStatsEnabled", 0) == 1;
	}

	private void SaveSettings()
	{
		PlayerPrefs.SetInt("AdvancedCombatStatsEnabled", AdvancedSettingsScreen.AdvancedCombatStatsEnabled ? 1 : 0);
	}

	public CustomButton BackButton;

	public CustomButton AdvancedCombatStatsButton;

	public static bool AdvancedCombatStatsEnabled;
}
