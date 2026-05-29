using System;
using System.Collections.Generic;
using UnityEngine;

public class SelectSaveScreen : MewtationsScreen
{
	private void Start()
	{
		this.BackButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<OptionsScreen>();
		};
		this.UpdateButtonText();
	}

	private void OnEnable()
	{
		this.UpdateButtonText();
	}

	private void TryCreateSaveButtons()
	{
		if (this.saveButtons.Count > 0)
		{
			return;
		}
		this.saveButtons.Clear();
		for (int i = 0; i < 5; i++)
		{
			CustomButton customButton = Object.Instantiate<CustomButton>(PrefabManager.instance.ButtonPrefab);
			customButton.transform.SetParentClean(this.ButtonsParent);
			this.saveButtons.Add(customButton);
		}
	}

	private void UpdateButtonText()
	{
		this.TryCreateSaveButtons();
		List<SaveGame> allSaves = SaveManager.instance.GetAllSaves();
		for (int i = 0; i < 5; i++)
		{
			int index = i;
			SaveGame save = allSaves[index];
			CustomButton customButton = this.saveButtons[index];
			if (save != null)
			{
				customButton.TextMeshPro.text = SaveManager.instance.GetSaveSummary(save);
			}
			else
			{
				customButton.TextMeshPro.text = MewtationsLoc.Translate("label_start_new_save", new LocParam[] { LocParam.Create("save_index", (index + 1).ToString()) });
			}
			Action <>9__1;
			customButton.Clicked += delegate
			{
				if (save == null)
				{
					SaveManager.instance.Save(new SaveGame
					{
						SaveId = index.ToString()
					});
					this.Restart();
					return;
				}
				if (PlatformHelper.HasModdingSupport && !new HashSet<string>(save.DisabledMods).SetEquals(new HashSet<string>(WorldManager.instance.CurrentSave.DisabledMods)))
				{
					ModalScreen.instance.Clear();
					ModalScreen.instance.SetTexts(MewtationsLoc.Translate("label_restart_required"), MewtationsLoc.Translate("label_different_mods"));
					ModalScreen instance = ModalScreen.instance;
					string text = MewtationsLoc.Translate("label_restart");
					Action action;
					if ((action = <>9__1) == null)
					{
						action = (<>9__1 = delegate
						{
							SaveManager.instance.Save(save);
							WorldManager.RebootGame();
						});
					}
					instance.AddOption(text, action);
					GameCanvas.instance.OpenModal();
					return;
				}
				this.SetSave(save);
			};
		}
	}

	private void Restart()
	{
		TransitionScreen.instance.StartTransition(delegate
		{
			WorldManager.RestartGame();
		}, 0.2f);
	}

	private void SetSave(SaveGame save)
	{
		SaveManager.instance.Save(save);
		this.Restart();
	}

	public RectTransform ButtonsParent;

	public CustomButton BackButton;

	private List<CustomButton> saveButtons = new List<CustomButton>();
}
