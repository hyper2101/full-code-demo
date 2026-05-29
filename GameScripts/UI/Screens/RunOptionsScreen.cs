using System;
using System.Collections.Generic;
using UnityEngine;

public class RunOptionsScreen : MewtationsScreen
{
	private void Start()
	{
		this.PlayButton.Clicked += delegate
		{
			TransitionScreen.instance.StartTransition(delegate
			{
				WorldManager.instance.CurrentRunOptions = new RunOptions
				{
					MoonLength = this.CurMoonLength,
					IsPeacefulMode = this.PeacefulMode,
					IsGreedEnabled = this.EnableGreed,
					IsDeathEnabled = this.EnableDeath,
					IsHappinessEnabled = this.EnableHappiness
				};
				SaveGame currentSave = WorldManager.instance.CurrentSave;
				if (currentSave != null)
				{
					SaveRound lastPlayedRound = currentSave.LastPlayedRound;
					if (lastPlayedRound != null)
					{
						List<SavedBoosterBox> savedBoosterBoxes = lastPlayedRound.SavedBoosterBoxes;
						if (savedBoosterBoxes != null)
						{
							savedBoosterBoxes.Clear();
						}
					}
				}
				foreach (BuyBoosterBox buyBoosterBox in WorldManager.instance.AllBoosterBoxes)
				{
					buyBoosterBox.StoredCostAmount = 0;
				}
				WorldManager.instance.CurrentRunVariables = new RunVariables();
				WorldManager.instance.RoundExtraKeyValues = new List<SerializedKeyValuePair>();
				CitiesManager.instance.Wellbeing = CitiesManager.instance.WellbeingStart;
				WorldManager.instance.StartNewRound();
				WorldManager.instance.Play();
			}, 0.2f);
		};
		this.BackButton.Clicked += delegate
		{
		};
		this.ShortMoon.Clicked += delegate
		{
			this.CurMoonLength = MoonLength.Short;
		};
		this.NormalMoon.Clicked += delegate
		{
			this.CurMoonLength = MoonLength.Normal;
		};
		this.LongMoon.Clicked += delegate
		{
			this.CurMoonLength = MoonLength.Long;
		};
		this.PeacefulModeOn.Clicked += delegate
		{
			this.PeacefulMode = true;
		};
		this.PeacefulModeOff.Clicked += delegate
		{
			this.PeacefulMode = false;
		};
		this.HappinessButton.Clicked += delegate
		{
			this.EnableHappiness = !this.EnableHappiness;
		};
		this.DeathButton.Clicked += delegate
		{
			this.EnableDeath = !this.EnableDeath;
		};
	}

	private void Update()
	{
		this.ShortMoon.Image.color = ((this.CurMoonLength != MoonLength.Short) ? ColorManager.instance.BackgroundColor : ColorManager.instance.InactiveBackgroundColor);
		this.NormalMoon.Image.color = ((this.CurMoonLength != MoonLength.Normal) ? ColorManager.instance.BackgroundColor : ColorManager.instance.InactiveBackgroundColor);
		this.LongMoon.Image.color = ((this.CurMoonLength != MoonLength.Long) ? ColorManager.instance.BackgroundColor : ColorManager.instance.InactiveBackgroundColor);
		this.PeacefulModeOn.Image.color = ((!this.PeacefulMode) ? ColorManager.instance.BackgroundColor : ColorManager.instance.InactiveBackgroundColor);
		this.PeacefulModeOff.Image.color = (this.PeacefulMode ? ColorManager.instance.BackgroundColor : ColorManager.instance.InactiveBackgroundColor);
		this.CurseOptions.gameObject.SetActive(WorldManager.instance.IsSpiritDlcActive());
		this.SetCurseButton(this.DeathButton, WorldManager.instance.CurrentSave.FinishedDeath, this.EnableDeath, "label_enable_death_curse");
		this.SetCurseButton(this.HappinessButton, WorldManager.instance.CurrentSave.FinishedHappiness, this.EnableHappiness, "label_enable_happiness_curse");
		if (InputController.instance.CancelTriggered())
		{
			GameCanvas.instance.SetScreen<MainMenu>();
		}
	}

	private void SetCurseButton(CustomButton but, bool curseUnlocked, bool curseEnabled, string mainTerm)
	{
		if (curseUnlocked)
		{
			but.ButtonEnabled = true;
			but.TooltipText = "";
			but.TextMeshPro.text = MewtationsLoc.Translate(mainTerm, new LocParam[] { LocParam.Create("on_off", RunOptionsScreen.YesNo(curseEnabled)) });
			return;
		}
		but.ButtonEnabled = false;
		but.TextMeshPro.text = MewtationsLoc.Translate("label_enable_curse_unknown");
		but.TooltipText = MewtationsLoc.Translate("label_beat_curse_to_unlock");
	}

	public static string YesNo(bool a)
	{
		if (!a)
		{
			return MewtationsLoc.Translate("label_off");
		}
		return MewtationsLoc.Translate("label_on");
	}

	public CustomButton ShortMoon;

	public CustomButton NormalMoon;

	public CustomButton LongMoon;

	public CustomButton PeacefulModeOn;

	public CustomButton PeacefulModeOff;

	public CustomButton PlayButton;

	public CustomButton BackButton;

	public CustomButton HappinessButton;

	public CustomButton DeathButton;

	public RectTransform CurseOptions;

	public MoonLength CurMoonLength = MoonLength.Normal;

	public bool PeacefulMode;

	public bool EnableGreed;

	public bool EnableHappiness;

	public bool EnableDeath;
}
