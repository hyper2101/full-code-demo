using System;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ModOptionsScreen : SokScreen
{
	private void Awake()
	{
		ModOptionsScreen.instance = this;
		this.BackButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<ModsScreen>();
			Mod selectedMod = ModOptionsScreen.SelectedMod;
			if (selectedMod != null)
			{
				selectedMod.Config.Save();
			}
			if (this.ShouldRestart)
			{
				WorldManager.RebootGame();
			}
		};
		this.OpenFolderButton.Clicked += delegate
		{
			Application.OpenURL("file:///" + ModOptionsScreen.SelectedMod.Path);
		};
		this.UploadButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<ModUploadScreen>();
		};
	}

	private void Update()
	{
		if (!InputController.instance.DisableAllInput && InputController.instance.GetKeyDown(Key.U) && PlatformHelper.UseSteam)
		{
			GameCanvas.instance.SetScreen<ModUploadScreen>();
		}
		if (InputController.instance.CancelTriggered())
		{
			GameCanvas.instance.SetScreen<ModsScreen>();
			Mod selectedMod = ModOptionsScreen.SelectedMod;
			if (selectedMod != null)
			{
				selectedMod.Config.Save();
			}
			if (this.ShouldRestart)
			{
				WorldManager.RebootGame();
			}
		}
	}

	private void OnEnable()
	{
		foreach (object obj in this.ButtonsParent)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		if (ModOptionsScreen.SelectedMod == null)
		{
			return;
		}
		if (ModUploadScreen.GetWorkshopId(ModOptionsScreen.SelectedMod) != 0UL)
		{
			this.OpenWorkshopButton.gameObject.SetActive(true);
			this.OpenWorkshopButton.Clicked += delegate
			{
				ModOptionsScreen.OpenWorkshop();
			};
		}
		else
		{
			this.OpenWorkshopButton.gameObject.SetActive(false);
		}
		this.Title.GetComponent<TextMeshProUGUI>().text = ModOptionsScreen.SelectedMod.Manifest.Name;
		this.Version.GetComponent<TextMeshProUGUI>().text = "v" + ModOptionsScreen.SelectedMod.Manifest.Version;
		foreach (ConfigEntryBase configEntryBase in ModOptionsScreen.SelectedMod.Config.Entries)
		{
			Action<ConfigEntryBase> onUI = configEntryBase.UI.OnUI;
			if (onUI != null)
			{
				onUI(configEntryBase);
			}
			if (!configEntryBase.UI.Hidden)
			{
				Debug.Log(string.Format("creating ui for {0} ({1})", configEntryBase.Name, configEntryBase.ValueType));
				if (configEntryBase.ValueType == typeof(bool))
				{
					this.CreateBoolConfig(configEntryBase);
				}
				else if (configEntryBase.ValueType == typeof(string))
				{
					this.CreateTextConfig(configEntryBase);
				}
				else if (configEntryBase.ValueType == typeof(int) || configEntryBase.ValueType == typeof(float))
				{
					this.CreateNumberConfig(configEntryBase, configEntryBase.ValueType);
				}
			}
		}
		this.UploadButton.gameObject.SetActive(false);
		if (ModManager.LocalModPaths.Contains(ModOptionsScreen.SelectedMod.Path))
		{
			this.UploadButton.gameObject.SetActive(true);
			return;
		}
		this.crQueryCompleted = CallResult<SteamUGCQueryCompleted_t>.Create(new CallResult<SteamUGCQueryCompleted_t>.APIDispatchDelegate(this.OnQueryCompleted));
		ulong workshopId = ModUploadScreen.GetWorkshopId(ModOptionsScreen.SelectedMod);
		if (workshopId != 0UL)
		{
			this.queryHandle = SteamUGC.CreateQueryUGCDetailsRequest(new PublishedFileId_t[] { (PublishedFileId_t)workshopId }, 1U);
			SteamAPICall_t steamAPICall_t = SteamUGC.SendQueryUGCRequest(this.queryHandle);
			this.crQueryCompleted.Set(steamAPICall_t, null);
		}
	}

	private void OnQueryCompleted(SteamUGCQueryCompleted_t result, bool failed)
	{
		if (result.m_eResult != EResult.k_EResultOK)
		{
			return;
		}
		SteamUGCDetails_t steamUGCDetails_t;
		if (SteamUGC.GetQueryUGCResult(this.queryHandle, 0U, out steamUGCDetails_t))
		{
			Debug.Log(string.Format("owner of mod is {0}", steamUGCDetails_t.m_ulSteamIDOwner));
			if (steamUGCDetails_t.m_ulSteamIDOwner == (ulong)SteamUser.GetSteamID())
			{
				this.UploadButton.gameObject.SetActive(true);
			}
		}
		SteamUGC.ReleaseQueryUGCRequest(this.queryHandle);
	}

	private static void OpenWorkshop()
	{
		if (PlatformHelper.UseSteam && !InputController.instance.GetKey(Key.LeftAlt))
		{
			SteamFriends.ActivateGameOverlayToWebPage(string.Format("https://steamcommunity.com/sharedfiles/filedetails/?id={0}", ModUploadScreen.GetWorkshopId(ModOptionsScreen.SelectedMod)), EActivateGameOverlayToWebPageMode.k_EActivateGameOverlayToWebPageMode_Default);
			return;
		}
		Application.OpenURL(string.Format("https://steamcommunity.com/sharedfiles/filedetails/?id={0}", ModUploadScreen.GetWorkshopId(ModOptionsScreen.SelectedMod)));
	}

	private void CreateTextConfig(ConfigEntryBase entry)
	{
		CustomButton customButton = Object.Instantiate<CustomButton>(PrefabManager.instance.ButtonPrefab, this.ButtonsParent);
		customButton.transform.localScale = Vector3.one;
		customButton.transform.localPosition = Vector3.zero;
		customButton.transform.localRotation = Quaternion.identity;
		string name = entry.UI.GetName();
		string tooltip = entry.UI.GetTooltip();
		customButton.TextMeshPro.text = ((!string.IsNullOrEmpty(name)) ? name : entry.Name);
		if (!string.IsNullOrEmpty(tooltip))
		{
			customButton.TooltipText = tooltip;
		}
		TMP_InputField component = Object.Instantiate<RectTransform>(this.InputPrefab, this.ButtonsParent).GetComponent<TMP_InputField>();
		component.text = (string)entry.BoxedValue;
		component.characterLimit = 0;
		((TMP_Text)component.placeholder).text = entry.UI.PlaceholderText;
		component.onValueChanged.AddListener(delegate(string newValue)
		{
			entry.BoxedValue = newValue;
			if (entry.UI.RestartAfterChange)
			{
				this.ShouldRestart = true;
			}
		});
		Object.Instantiate<RectTransform>(this.SpacerPrefab, this.ButtonsParent);
	}

	private void CreateNumberConfig(ConfigEntryBase entry, Type inputType)
	{
		CustomButton customButton = Object.Instantiate<CustomButton>(PrefabManager.instance.ButtonPrefab, this.ButtonsParent);
		customButton.transform.localScale = Vector3.one;
		customButton.transform.localPosition = Vector3.zero;
		customButton.transform.localRotation = Quaternion.identity;
		string name = entry.UI.GetName();
		string tooltip = entry.UI.GetTooltip();
		customButton.TextMeshPro.text = ((!string.IsNullOrEmpty(name)) ? name : entry.Name);
		if (!string.IsNullOrEmpty(tooltip))
		{
			customButton.TooltipText = tooltip;
		}
		TMP_InputField component = Object.Instantiate<RectTransform>(this.InputPrefab, this.ButtonsParent).GetComponent<TMP_InputField>();
		component.characterValidation = ((inputType == typeof(int)) ? TMP_InputField.CharacterValidation.Integer : TMP_InputField.CharacterValidation.Decimal);
		component.text = ((inputType == typeof(int)) ? ((int)entry.BoxedValue).ToString() : ((float)entry.BoxedValue).ToString());
		component.onValueChanged.AddListener(delegate(string newValue)
		{
			entry.BoxedValue = ((inputType == typeof(int)) ? int.Parse(newValue) : float.Parse(newValue));
			if (entry.UI.RestartAfterChange)
			{
				this.ShouldRestart = true;
			}
		});
		Object.Instantiate<RectTransform>(this.SpacerPrefab, this.ButtonsParent);
	}

	private void CreateBoolConfig(ConfigEntryBase entry)
	{
		CustomButton btn = Object.Instantiate<CustomButton>(PrefabManager.instance.ButtonPrefab, this.ButtonsParent);
		btn.transform.localScale = Vector3.one;
		btn.transform.localPosition = Vector3.zero;
		btn.transform.localRotation = Quaternion.identity;
		string name = entry.UI.GetName();
		string tooltip = entry.UI.GetTooltip();
		btn.TextMeshPro.text = ((!string.IsNullOrEmpty(name)) ? name : entry.Name) + ": " + this.BoolToLabel((bool)entry.BoxedValue);
		if (!string.IsNullOrEmpty(tooltip))
		{
			btn.TooltipText = tooltip;
		}
		btn.Clicked += delegate
		{
			entry.BoxedValue = !(bool)entry.BoxedValue;
			btn.TextMeshPro.text = ((!string.IsNullOrEmpty(name)) ? name : entry.Name) + ": " + this.BoolToLabel((bool)entry.BoxedValue);
			if (entry.UI.RestartAfterChange)
			{
				this.ShouldRestart = true;
			}
		};
	}

	private string BoolToLabel(bool b)
	{
		if (!b)
		{
			return SokLoc.Translate("label_off");
		}
		return SokLoc.Translate("label_on");
	}

	public static ModOptionsScreen instance;

	public CustomButton BackButton;

	public CustomButton OpenFolderButton;

	public CustomButton OpenWorkshopButton;

	public CustomButton UploadButton;

	public RectTransform ButtonsParent;

	public static Mod SelectedMod;

	public bool ShouldRestart;

	public RectTransform SpacerPrefab;

	public RectTransform InputPrefab;

	public RectTransform Title;

	public RectTransform Version;

	private CallResult<SteamUGCQueryCompleted_t> crQueryCompleted;

	private UGCQueryHandle_t queryHandle;
}
