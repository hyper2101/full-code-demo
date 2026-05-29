using System;
using System.Collections.Generic;
using System.IO;
using Steamworks;
using TMPro;
using UnityEngine;

public class ModUploadScreen : MewtationsScreen
{
	private void Awake()
	{
		this.TogglePreviewButton.Clicked += delegate
		{
			this.SetPreviewImage = !this.SetPreviewImage;
			this.TogglePreviewButton.TextMeshPro.text = "Set preview image: " + OptionsScreen.YesNo(this.SetPreviewImage);
		};
		this.SelectTagsButton.Clicked += delegate
		{
			ModalScreen instance = ModalScreen.instance;
			this.Tags.Clear();
			instance.Clear();
			instance.SetTexts("Select Tags", "Click tags below to enable them.\n\n<color=#0000ff><u><link=\"https://modding.stacklands.co/en/latest/guides/publishing.html#adding-tags\">What are tags?</link></u></color>");
			this.CreateTagButton("Cards");
			this.CreateTagButton("Gameplay");
			this.CreateTagButton("Language");
			this.CreateTagButton("Quality of Life");
			this.CreateTagButton("Content");
			this.CreateTagButton("Development");
			instance.AddOption("Back", delegate
			{
				GameCanvas.instance.CloseModal();
			});
			GameCanvas.instance.OpenModal();
		};
		this.BackButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<ModOptionsScreen>();
		};
	}

	private void OnEnable()
	{
		Mod mod = ModOptionsScreen.SelectedMod;
		if (mod == null)
		{
			return;
		}
		this.crCreateItemResult = CallResult<CreateItemResult_t>.Create(new CallResult<CreateItemResult_t>.APIDispatchDelegate(this.OnCreateItem));
		this.crSubmitItemUpdateResult = CallResult<SubmitItemUpdateResult_t>.Create(new CallResult<SubmitItemUpdateResult_t>.APIDispatchDelegate(this.OnUpdateItem));
		if (ModUploadScreen.GetWorkshopId(mod) == 0UL)
		{
			ModalScreen instance = ModalScreen.instance;
			instance.Clear();
			instance.SetTexts("Create Workshop Item?", "It appears this mod has not been uploaded to the Steam Workshop yet. Would you like to create a new Workshop Item now?\n\nBy submitting this item, you agree to the <color=#0000ff><u><link=\"https://steamcommunity.com/workshop/workshoplegalagreement/\">Steam Workshop Terms Of Service</link></u></color>");
			instance.AddOption("Create Item", delegate
			{
				if (!this.crCreateItemResult.IsActive())
				{
					this.CreateWorkshopItem();
				}
			});
			instance.AddOption("Back", delegate
			{
				GameCanvas.instance.SetScreen<ModOptionsScreen>();
				GameCanvas.instance.CloseModal();
			});
			GameCanvas.instance.OpenModal();
		}
		Action <>9__4;
		this.UploadButton.Clicked += delegate
		{
			if (this.ugcUpdateHandle != UGCUpdateHandle_t.Invalid)
			{
				return;
			}
			ModalScreen modal = ModalScreen.instance;
			modal.Clear();
			modal.SetTexts("Upload mod?", "You are about to upload the contents of the following folder to the Steam Workshop: " + mod.Path);
			modal.AddOption("Upload", delegate
			{
				modal.Clear();
				this.UploadWorkshopItem();
			});
			ModalScreen modal2 = modal;
			string text = "Open Folder";
			Action action;
			if ((action = <>9__4) == null)
			{
				action = (<>9__4 = delegate
				{
					Application.OpenURL("file:///" + mod.Path);
				});
			}
			modal2.AddOption(text, action);
			modal.AddOption("Back", delegate
			{
				GameCanvas.instance.CloseModal();
			});
			GameCanvas.instance.OpenModal();
		};
	}

	private void Update()
	{
		this.UpdateUploading();
		if (InputController.instance.CancelTriggered())
		{
			GameCanvas.instance.SetScreen<ModOptionsScreen>();
		}
	}

	private void UpdateUploading()
	{
		if (this.ugcUpdateHandle == UGCUpdateHandle_t.Invalid)
		{
			return;
		}
		ModalScreen instance = ModalScreen.instance;
		ulong num;
		ulong num2;
		EItemUpdateStatus itemUpdateProgress = SteamUGC.GetItemUpdateProgress(this.ugcUpdateHandle, out num, out num2);
		if (num2 != 0UL)
		{
			instance.SetTexts("Uploading...", string.Format("Status: {0} ({1}/{2})", itemUpdateProgress, ModUploadScreen.FormatBytes(num), ModUploadScreen.FormatBytes(num2)));
		}
		else
		{
			instance.SetTexts("Uploading...", string.Format("Status: {0}", itemUpdateProgress));
		}
		if (itemUpdateProgress != this.lastUpdateStatus)
		{
			Debug.Log(itemUpdateProgress);
			this.lastUpdateStatus = itemUpdateProgress;
		}
	}

	private void CheckForConfig()
	{
		string configPath = Path.Combine(ModOptionsScreen.SelectedMod.Path, "config.json");
		ModalScreen modal = ModalScreen.instance;
		if (File.Exists(configPath))
		{
			modal.Clear();
			modal.SetTexts("config.json detected", "A config.json file has been detected in the mod folder. If this file is uploaded, all players will have your current settings instead of the default ones. Would you like to delete the file and proceed with the upload?");
			modal.AddOption("Delete config.json & Upload", delegate
			{
				File.Delete(configPath);
				this.UploadWorkshopItem();
				modal.Clear();
			});
			modal.AddOption("Keep config.json & Upload (Not Recommended)", delegate
			{
				this.UploadWorkshopItem();
				modal.Clear();
			});
			modal.AddOption("Open Folder", delegate
			{
				Application.OpenURL("file:///" + ModOptionsScreen.SelectedMod.Path);
			});
			modal.AddOption("Cancel", delegate
			{
				GameCanvas.instance.CloseModal();
			});
			return;
		}
		this.UploadWorkshopItem();
		modal.Clear();
	}

	private void CreateWorkshopItem()
	{
		Debug.Log("Creating workshop item..");
		SteamAPICall_t steamAPICall_t = SteamUGC.CreateItem(new AppId_t(1948280U), EWorkshopFileType.k_EWorkshopFileTypeFirst);
		this.crCreateItemResult.Set(steamAPICall_t, null);
	}

	private void UploadWorkshopItem()
	{
		Mod selectedMod = ModOptionsScreen.SelectedMod;
		string text = Path.Combine(selectedMod.Path, "config.json");
		if (File.Exists(text))
		{
			File.Move(text, Path.Combine(Application.persistentDataPath, "Mods", "config.json"));
		}
		this.ugcUpdateHandle = SteamUGC.StartItemUpdate(new AppId_t(1948280U), new PublishedFileId_t(ModUploadScreen.GetWorkshopId(selectedMod)));
		SteamUGC.SetItemTitle(this.ugcUpdateHandle, selectedMod.Manifest.Name);
		SteamUGC.SetItemContent(this.ugcUpdateHandle, selectedMod.Path);
		if (this.Tags.Count > 0)
		{
			SteamUGC.SetItemTags(this.ugcUpdateHandle, this.Tags);
		}
		if (this.SetPreviewImage && File.Exists(Path.Combine(selectedMod.Path, "icon.png")))
		{
			SteamUGC.SetItemPreview(this.ugcUpdateHandle, Path.Combine(selectedMod.Path, "icon.png"));
		}
		SteamAPICall_t steamAPICall_t = SteamUGC.SubmitItemUpdate(this.ugcUpdateHandle, this.ChangeNotesText.text);
		this.crSubmitItemUpdateResult.Set(steamAPICall_t, null);
	}

	private void OnCreateItem(CreateItemResult_t result, bool failed)
	{
		if (result.m_eResult != EResult.k_EResultOK)
		{
			Debug.LogError(string.Format("uh oh, result of CreateItem is {0}", result.m_eResult));
			return;
		}
		Debug.Log(string.Format("Item has been created: {0}", result.m_nPublishedFileId));
		if (result.m_bUserNeedsToAcceptWorkshopLegalAgreement)
		{
			SteamFriends.ActivateGameOverlayToWebPage(string.Format("steam://url/CommunityFilePage/{0}", result.m_nPublishedFileId), EActivateGameOverlayToWebPageMode.k_EActivateGameOverlayToWebPageMode_Default);
		}
		Mod mod = ModOptionsScreen.SelectedMod;
		ModUploadScreen.SetWorkshopId(mod, (ulong)result.m_nPublishedFileId);
		ModalScreen instance = ModalScreen.instance;
		instance.Clear();
		instance.SetTexts("Item created", "A workshop.txt file has been created in the mod folder, be sure to copy this to your source folder!");
		instance.AddOption("Open folder", delegate
		{
			Application.OpenURL("file:///" + mod.Path);
		});
		instance.AddOption("Back", delegate
		{
			GameCanvas.instance.CloseModal();
		});
	}

	private void OnUpdateItem(SubmitItemUpdateResult_t result, bool failed)
	{
		this.ugcUpdateHandle = UGCUpdateHandle_t.Invalid;
		this.lastUpdateStatus = EItemUpdateStatus.k_EItemUpdateStatusInvalid;
		ModalScreen instance = ModalScreen.instance;
		instance.Clear();
		if (result.m_eResult == EResult.k_EResultOK)
		{
			instance.SetTexts("Upload finished", "The files have been successfully uploaded");
			instance.AddOption(MewtationsLoc.Translate("label_okay"), delegate
			{
				GameCanvas.instance.CloseModal();
			});
		}
		else
		{
			instance.SetTexts("uh oh", string.Format("something went wrong :( {0}", result.m_eResult));
			instance.AddOption(MewtationsLoc.Translate("label_okay"), delegate
			{
				GameCanvas.instance.CloseModal();
			});
		}
		string text = Path.Combine(Application.persistentDataPath, "Mods", "config.json");
		if (File.Exists(text))
		{
			File.Move(text, Path.Combine(ModOptionsScreen.SelectedMod.Path, "config.json"));
		}
		SteamFriends.ActivateGameOverlayToWebPage(string.Format("https://steamcommunity.com/sharedfiles/filedetails/?id={0}", result.m_nPublishedFileId), EActivateGameOverlayToWebPageMode.k_EActivateGameOverlayToWebPageMode_Default);
	}

	private void CreateTagButton(string tag)
	{
		CustomButton but = Object.Instantiate<CustomButton>(ModalScreen.instance.ButtonPrefab);
		but.transform.SetParentClean(ModalScreen.instance.ButtonParent);
		but.TextMeshPro.text = "<color=#A1A1A1><s>" + tag + "</s>";
		but.Clicked += delegate
		{
			if (this.Tags.Contains(tag))
			{
				this.Tags.Remove(tag);
				but.TextMeshPro.text = "<color=#A1A1A1><s>" + tag + "</s>";
				return;
			}
			this.Tags.Add(tag);
			but.TextMeshPro.text = tag;
		};
	}

	public static string FormatBytes(ulong bytes)
	{
		if (bytes < 1024UL)
		{
			return string.Format("{0} B", bytes);
		}
		int num = (int)(Math.Log(bytes) / Math.Log(1024.0));
		return string.Format("{0:F2} {1}B", bytes / Math.Pow(1024.0, (double)num), "KMGT"[num - 1]);
	}

	public static ulong GetWorkshopId(Mod mod)
	{
		string text = Path.Combine(mod.Path, "workshop.txt");
		if (!File.Exists(text))
		{
			return 0UL;
		}
		ulong num;
		if (ulong.TryParse(File.ReadAllText(text), out num))
		{
			return num;
		}
		return 0UL;
	}

	public static void SetWorkshopId(Mod mod, ulong id)
	{
		File.WriteAllText(Path.Combine(mod.Path, "workshop.txt"), id.ToString());
	}

	public CustomButton BackButton;

	public CustomButton UploadButton;

	public CustomButton TogglePreviewButton;

	public CustomButton SelectTagsButton;

	public bool SetPreviewImage = true;

	public TextMeshProUGUI ChangeNotesText;

	public List<string> Tags = new List<string>();

	private CallResult<CreateItemResult_t> crCreateItemResult;

	private CallResult<SubmitItemUpdateResult_t> crSubmitItemUpdateResult;

	private UGCUpdateHandle_t ugcUpdateHandle = UGCUpdateHandle_t.Invalid;

	private EItemUpdateStatus lastUpdateStatus;
}
