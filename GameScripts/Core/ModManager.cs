using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json;
using Steamworks;
using UnityEngine;

public class ModManager : MonoBehaviour
{
	private void Awake()
	{
		if (!ModdingConfig.Enabled)
		{
			UnityEngine.Debug.LogWarning("[Modding] Disabled - Harmony stub active.");
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}

		if (!PlatformHelper.HasModdingSupport)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		ModManager.instance = this;
		ModManager.LoadedMods = new List<Mod>();
		ModManager.DisabledModManifests = new List<ModManifest>();
		List<string> modPaths = ModManager.GetModPaths();
		Debug.Log(string.Format("found {0} mods", modPaths.Count));
		List<ModManifest> list = new List<ModManifest>();
		foreach (string text in modPaths)
		{
			string text2 = Path.Combine(text, "manifest.json");
			if (!File.Exists(text2))
			{
				Debug.LogError("Could not find manifest.json in \"" + text + "\"; skipping!");
			}
			else
			{
				ModManifest manifest = JsonConvert.DeserializeObject<ModManifest>(File.ReadAllText(text2));
				if (list.Any<ModManifest>((ModManifest m) => m.Id == manifest.Id) || ModManager.DisabledModManifests.Any<ModManifest>((ModManifest m) => m.Id == manifest.Id))
				{
					Debug.LogError("Already loaded a mod with id " + manifest.Id + "; skipping!");
				}
				else if (SaveManager.instance.CurrentSave.DisabledMods.Contains(manifest.Id))
				{
					ModManager.DisabledModManifests.Add(manifest);
					Debug.LogWarning("Skipping " + manifest.Id + " because it is disabled!");
				}
				else
				{
					manifest.Folder = text;
					list.Add(manifest);
				}
			}
		}
		try
		{
			list = DependencyHelper.GetValidModLoadOrder(list);
			Debug.Log("Found valid mod load order: " + string.Join(", ", list.Select<ModManifest, string>((ModManifest m) => m.Id)));
		}
		catch (Exception ex)
		{
			Debug.LogError("Could not find valid mod load order!");
			Debug.LogError(ex.Message);
			return;
		}
		foreach (ModManifest modManifest in list)
		{
			this.LoadModFromDir(new DirectoryInfo(modManifest.Folder));
		}
		this.FindClassesInAssemblies();
	}

	public static List<string> GetModPaths()
	{
		List<string> list = new List<string>();
		string text = Path.Combine(Application.persistentDataPath, "Mods");
		FileHelper.MakeOrCreatePath(text);
		foreach (DirectoryInfo directoryInfo in new DirectoryInfo(text).GetDirectories())
		{
			ModManager.LocalModPaths.Add(directoryInfo.FullName);
			list.Add(directoryInfo.FullName);
		}
		if (PlatformHelper.UseSteam)
		{
			list.AddRange(ModManager.GetSteamWorkshopModPaths());
		}
		return list;
	}

	private static List<string> GetSteamWorkshopModPaths()
	{
		List<string> list = new List<string>();
		uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
		PublishedFileId_t[] array = new PublishedFileId_t[numSubscribedItems];
		SteamUGC.GetSubscribedItems(array, numSubscribedItems);
		foreach (PublishedFileId_t publishedFileId_t in array)
		{
			uint num = 1024U;
			ulong num2;
			string text;
			uint num3;
			bool itemInstallInfo = SteamUGC.GetItemInstallInfo(publishedFileId_t, out num2, out text, num, out num3);
			if (!string.IsNullOrEmpty(text) && itemInstallInfo)
			{
				list.Add(text);
			}
		}
		return list;
	}

	private void LoadModFromDir(DirectoryInfo dir)
	{
		string text = Path.Combine(dir.FullName, "manifest.json");
		if (!File.Exists(text))
		{
			Debug.LogError("Could not find manifest.json in \"" + dir.FullName + "\"; skipping!");
			return;
		}
		ModManifest modManifest = JsonConvert.DeserializeObject<ModManifest>(File.ReadAllText(text));
		Mod mod;
		if (ModManager.TryGetMod(modManifest.Id, out mod))
		{
			Debug.LogError("Already loaded a mod with id " + modManifest.Id + "; skipping!");
			return;
		}
		Debug.Log(string.Concat(new string[] { "loading mod :) ", modManifest.Name, " (", modManifest.Id, ") v", modManifest.Version }));
		string text2 = "";
		Type type = null;
		string[] files = Directory.GetFiles(dir.FullName, "*.dll");
		if (files.Length > 1)
		{
			if (files.Contains(modManifest.Id + ".dll"))
			{
				text2 = Path.Combine(dir.FullName, modManifest.Id + ".dll");
			}
			else
			{
				if (string.IsNullOrEmpty(modManifest.Assembly))
				{
					Debug.LogError("Found more than 1 assemblies in the mod folder. Please specify the main one with the \"assembly\" property in your manifest.json");
					return;
				}
				text2 = Path.Combine(dir.FullName, modManifest.Assembly);
			}
		}
		if (files.Length == 1)
		{
			text2 = Path.Combine(dir.FullName, files[0]);
		}
		if (File.Exists(text2))
		{
			foreach (Type type2 in Assembly.LoadFrom(text2).GetTypes())
			{
				if (typeof(Mod).IsAssignableFrom(type2))
				{
					if (type != null)
					{
						Debug.LogWarning(string.Format("Found more than 1 Mod class! Keeping {0}, skipping {1}!", type, type2));
					}
					else
					{
						type = type2;
					}
				}
			}
		}
		else
		{
			Debug.LogWarning(string.Concat(new string[] { "Could not find Mods/", dir.Name, "/", modManifest.Id, ".dll; loading as codeless mod!" }));
		}
		if (type == null)
		{
			type = typeof(Mod);
		}
		base.gameObject.SetActive(false);
		Mod mod2 = base.gameObject.AddComponent(type) as Mod;
		mod2.Manifest = modManifest;
		mod2.Harmony = new Harmony(modManifest.Id);
		mod2.Path = dir.FullName;
		mod2.Logger = new ModLogger(modManifest);
		string text3 = Path.Combine(dir.FullName, "config.json");
		if (!File.Exists(text3))
		{
			File.WriteAllText(text3, "{}");
		}
		mod2.Config = new ConfigFile(mod2, text3);
		ModManager.LoadedMods.Add(mod2);
		base.gameObject.SetActive(true);
	}

	internal void FindClassesInAssemblies()
	{
		ModManager.CardClasses = new Dictionary<string, Type>();
		foreach (Type type in typeof(CardData).Assembly.GetSafeTypes())
		{
			if (typeof(CardData).IsAssignableFrom(type))
			{
				ModManager.CardClasses[type.ToString()] = type;
			}
		}
		foreach (Mod mod in ModManager.LoadedMods)
		{
			if (!(mod.GetType() == typeof(Mod)))
			{
				foreach (Type type2 in mod.GetType().Assembly.GetSafeTypes())
				{
					if (typeof(CardData).IsAssignableFrom(type2))
					{
						ModManager.CardClasses[type2.ToString()] = type2;
					}
				}
			}
		}
	}

	internal void ReadyUpMods()
	{
		Debug.Log("(ModManager) readying up mods");
		using (List<Mod>.Enumerator enumerator = ModManager.LoadedMods.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Mod mod = enumerator.Current;
				try
				{
					string locPath = Path.Combine(mod.Path, "localization.tsv");
					if (File.Exists(locPath))
					{
						MewtationsLoc.instance.LoadTermsFromFile(locPath, false);
						MewtationsLoc.instance.LanguageChanged += delegate
						{
							Debug.Log("Loading localization.tsv for " + mod.Manifest.Id);
							MewtationsLoc.instance.LoadTermsFromFile(locPath, true);
						};
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("Failed to load localization.tsv for " + mod.Manifest.Id);
					Debug.LogException(ex);
				}
				try
				{
					mod.Ready();
				}
				catch (Exception ex2)
				{
					Debug.LogException(ex2);
				}
			}
		}
	}

	public static bool TryGetMod(string id, out Mod mod)
	{
		Mod mod2 = ModManager.LoadedMods.FirstOrDefault<Mod>((Mod m) => m.Manifest.Id == id);
		if (mod2 == null)
		{
			mod = null;
			return false;
		}
		mod = mod2;
		return true;
	}

	public static ModManager instance;

	public static List<Mod> LoadedMods;

	public static Dictionary<string, Type> CardClasses;

	public static List<ModManifest> DisabledModManifests;

	public static List<string> LocalModPaths = new List<string>();
}
