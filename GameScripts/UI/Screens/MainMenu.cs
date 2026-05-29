using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenu : MewtationsScreen
{
	private void StartNewRun()
	{
		GameCanvas.instance.SetScreen<RunOptionsScreen>();
	}

	private void Awake()
	{
		this.JoinDiscordButton.Clicked += delegate
		{
			Application.OpenURL("https://discord.gg/sokpop");
		};
		this.NewGameButton.Clicked += delegate
		{
			if (WorldManager.instance.CurrentSave.LastPlayedRound != null)
			{
				GameCanvas.instance.ShowStartNewRunModal(delegate
				{
					this.StartNewRun();
				});
				return;
			}
			this.StartNewRun();
		};
		this.ContinueButton.Clicked += delegate
		{
			HashSet<string> hashSet = WorldManager.instance.FindMissingCardsInSave();
			if (hashSet.Count == 0)
			{
				TransitionScreen.instance.StartTransition(delegate
				{
					WorldManager.instance.LoadPreviousRound();
					WorldManager.instance.Play();
				}, 1f);
				return;
			}
			GameCanvas.instance.MissingCardsInSavePrompt(delegate
			{
				TransitionScreen.instance.StartTransition(delegate
				{
					WorldManager.instance.LoadPreviousRound();
					WorldManager.instance.Play();
				}, 1f);
			}, hashSet);
		};
		this.OptionsButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<OptionsScreen>();
		};
		if (!PlatformHelper.HasModdingSupport)
		{
			this.ModsButton.gameObject.SetActive(false);
		}
		this.ModsButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<ModsScreen>();
		};
		this.CardopediaButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<CardopediaScreen>();
		};
		this.QuitButton.Clicked += delegate
		{
			Application.Quit();
		};
		this.UpdateInfoButton.Clicked += delegate
		{
			this.UpdatePopup.gameObject.SetActive(true);
		};
		this.UpdatePopup.gameObject.SetActive(false);
		if (PlayerPrefs.GetInt("showUpdatePopup", 4) <= 4)
		{
			this.UpdatePopup.gameObject.SetActive(true);
			PlayerPrefs.SetInt("showUpdatePopup", 5);
		}
		this.TitleText2000.gameObject.SetActive(WorldManager.instance.IsCitiesDlcActive());
	}

	private void Update()
	{
		this.timer += Time.deltaTime;
		if (InputController.instance.GetKeyDown(Key.Space) || InputController.instance.GetKeyDown(Key.Escape))
		{
			this.UpdatePopup.gameObject.SetActive(false);
		}
		this.JoinDiscordButton.gameObject.SetActive(GameCanvas.instance.ScreenIsInteractable<MainMenu>());
		this.CardopediaNewElement.gameObject.SetActive(WorldManager.instance.CurrentSave.NewCardopediaIds.Count > 0);
		if (WorldManager.instance.CurrentSave.LastPlayedRound != null)
		{
			string text;
			if (WorldManager.instance.CurrentSave.LastPlayedRound.SaveVersion <= 1)
			{
				text = WorldManager.instance.CurrentSave.LastPlayedRound.CurrentMonth.ToString();
			}
			else if (WorldManager.instance.CurrentSave.LastPlayedRound.CurrentBoardId == "cities")
			{
				text = WorldManager.instance.CurrentSave.LastPlayedRound.BoardMonths.CitiesMonth.ToString();
			}
			else
			{
				text = (WorldManager.instance.CurrentSave.LastPlayedRound.BoardMonths.MainMonth + WorldManager.instance.CurrentSave.LastPlayedRound.BoardMonths.IslandMonth).ToString();
			}
			this.ContinueButton.TextMeshPro.text = MewtationsLoc.Translate("label_continue_run", new LocParam[] { LocParam.Create("moon", text) });
		}
		if (WorldManager.instance.IsCitiesDlcActive())
		{
			this.UpdateText.text = MewtationsLoc.Translate("label_menu_cities_title");
		}
		else
		{
			this.UpdateText.text = MewtationsLoc.Translate("label_menu_cities_title_locked");
		}
		this.ContinueButton.gameObject.SetActive(WorldManager.instance.CurrentSave.LastPlayedRound != null);
		Vector3 vector;
		if (this.UpdateInfoButton.IsHovered)
		{
			vector = new Vector3(1.2f, 1.2f, 1.2f);
		}
		else
		{
			vector = Vector3.one * (1f + Mathf.Sin(this.timer * 2f) * 0.1f);
		}
		this.UpdateInfoButton.transform.localScale = Vector3.Lerp(this.UpdateInfoButton.transform.localScale, vector, Time.deltaTime * 18f);
	}

	public CustomButton NewGameButton;

	public CustomButton ContinueButton;

	public CustomButton CardopediaButton;

	public CustomButton OptionsButton;

	public CustomButton ModsButton;

	public CustomButton QuitButton;

	public CustomButton JoinDiscordButton;

	public CustomButton UpdateInfoButton;

	public TextMeshProUGUI UpdateText;

	public RectTransform CardopediaNewElement;

	public UpdatePopup UpdatePopup;

	public TextMeshProUGUI TitleText2000;

	private float timer;
}
