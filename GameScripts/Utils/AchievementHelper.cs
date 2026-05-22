using System;
using Steamworks;
using UnityEngine;

public class AchievementHelper
{
	public static void UnlockAchievement(string achName)
	{
		if (Application.isEditor)
		{
			return;
		}
		if (PlatformHelper.UseSteam)
		{
			SteamUserStats.SetAchievement(achName);
			SteamUserStats.StoreStats();
		}
	}
}
