using System;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;

public class ModsScreen : SokScreen
{
	private void Awake()
	{
		if (!PlatformHelper.HasModdingSupport)
		{
			return;
		}
		this.BackButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<MainMenu>();
		};
		this.DisableModsButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<ModDisablingScreen>();
		};
		using (IEnumerator<Mod> enumerator = ModManager.LoadedMods.OrderBy<Mod, string>((Mod m) => m.Manifest.Name).GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Mod mod = enumerator.Current;
				CustomButton customButton = Object.Instantiate<CustomButton>(PrefabManager.instance.ButtonPrefab, this.ButtonsParent);
				customButton.transform.localScale = Vector3.one;
				customButton.transform.localPosition = Vector3.zero;
				customButton.transform.localRotation = Quaternion.identity;
				customButton.TextMeshPro.text = mod.Manifest.Name;
				customButton.Clicked += delegate
				{
					ModOptionsScreen.SelectedMod = mod;
					GameCanvas.instance.SetScreen<ModOptionsScreen>();
				};
			}
		}
		foreach (ModManifest modManifest in ModManager.DisabledModManifests.OrderBy<ModManifest, string>((ModManifest m) => m.Name))
		{
			CustomButton customButton2 = Object.Instantiate<CustomButton>(PrefabManager.instance.ButtonPrefab, this.ButtonsParent);
			customButton2.transform.localScale = Vector3.one;
			customButton2.transform.localPosition = Vector3.zero;
			customButton2.transform.localRotation = Quaternion.identity;
			customButton2.TextMeshPro.text = "<color=#A1A1A1><s>" + modManifest.Name + "</s>";
		}
		this.OpenWorkshopButton.Clicked += delegate
		{
			ModsScreen.OpenWorkshop();
		};
		this.OpenWorkshopButton.gameObject.SetActive(PlatformHelper.UseSteam);
	}

	private void Update()
	{
		if (InputController.instance.CancelTriggered())
		{
			GameCanvas.instance.SetScreen<MainMenu>();
		}
	}

	private static void OpenWorkshop()
	{
		if (PlatformHelper.UseSteam && !InputController.instance.GetKey(Key.LeftAlt))
		{
			SteamFriends.ActivateGameOverlayToWebPage("https://steamcommunity.com/app/1948280/workshop", EActivateGameOverlayToWebPageMode.k_EActivateGameOverlayToWebPageMode_Default);
		}
	}

	public CustomButton BackButton;

	public CustomButton DisableModsButton;

	public CustomButton OpenWorkshopButton;

	public RectTransform ButtonsParent;
}
