using System;
using System.IO;
using System.Linq;
using Steamworks;
using UnityEngine;

public static class PlatformHelper
{
	public static bool UseSteam
	{
		get
		{
			return SteamManager.Initialized;
		}
	}

	public static bool HasModdingSupport
	{
		get
		{
			if (Application.platform == RuntimePlatform.Switch)
			{
				return false;
			}
			if (Application.isEditor && !DebugOptions.Default.ModdingSupportEnabled)
			{
				return false;
			}
			return !(from s in Environment.GetCommandLineArgs()
				select s.ToLower()).Contains("--no-mods");
		}
	}

	public static bool IsTestBuild
	{
		get
		{
			string text;
			return SteamManager.Initialized && SteamApps.GetCurrentBetaName(out text, 100);
		}
	}

	public static string CurrentSavesDirectory
	{
		get
		{
			string text;
			if (SteamManager.Initialized && SteamApps.GetCurrentBetaName(out text, 100))
			{
				string text2 = Path.Combine(Application.persistentDataPath, text + "_Saves");
				if (!Directory.Exists(text2))
				{
					Directory.CreateDirectory(text2);
				}
				return text2;
			}
			return Application.persistentDataPath;
		}
	}
}
