using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
	public void Awake()
	{
		SaveManager.instance = this;
		Debug.Log("Current save directory is: '" + PlatformHelper.CurrentSavesDirectory + "'");
		this.Load();
	}

	private void ConvertLegacySave()
	{
		string @string = PlayerPrefs.GetString("save", "");
		if (!string.IsNullOrEmpty(@string))
		{
			Debug.Log("Found a legacy save - converting!");
			FileHelper.SaveFile("0", @string);
			PlayerPrefs.SetString("legacy_save", @string);
			PlayerPrefs.DeleteKey("save");
		}
	}

	internal void Load()
	{
		this.ConvertLegacySave();
		this.CurrentSave = this.DetermineCurrentSave();
	}

	public void Save(SaveGame saveGame)
	{
		saveGame.LastSavedUtc = DateTime.UtcNow;
		string text = JsonUtility.ToJson(saveGame);
		FileHelper.SaveFile(saveGame.SaveId, text);
	}

	private SaveGame DetermineCurrentSave()
	{
		SaveGame saveGame = (from x in this.GetAllSaves()
			where x != null
			orderby x.LastSavedUtc descending
			select x).ToArray<SaveGame>()[0];
		if (SaveManager.forceReloadSave != null)
		{
			Debug.Log("ForceReloading the game");
			SaveGame saveGame2 = SaveManager.forceReloadSave;
			SaveManager.forceReloadSave = null;
			SaveManager.IsForceReload = true;
			saveGame2.SaveId = saveGame.SaveId;
			return saveGame2;
		}
		return saveGame;
	}

	public List<SaveGame> GetAllSaves()
	{
		SaveGame[] array = new SaveGame[5];
		for (int i = 0; i < 5; i++)
		{
			SaveGame saveGame = SaveManager.LoadSaveFromFile(i.ToString());
			array[i] = saveGame;
		}
		if (array[0] == null)
		{
			array[0] = this.GetDefaultSaveGame();
		}
		return array.ToList<SaveGame>();
	}

	public SaveGame GetDefaultSaveGame()
	{
		return new SaveGame
		{
			SaveId = "0"
		};
	}

	public SaveGame GetSaveFromResources(string path)
	{
		return SaveGame.LoadFromString(Resources.Load<TextAsset>(path).text, "0");
	}

	public static SaveGame LoadSaveFromFile(string saveId)
	{
		string text = FileHelper.LoadFile(saveId);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		return SaveGame.LoadFromString(text, saveId);
	}

	public static void ForceReload(SaveGame saveGame)
	{
		SaveManager.forceReloadSave = saveGame;
	}

	public void Save(bool saveRound)
	{
		if (saveRound)
		{
			this.CurrentSave.LastPlayedRound = WorldManager.instance.Save.GetSaveRound();
		}
		this.Save(this.CurrentSave);
	}

	public static SaveGame GetSaveFromFileInfo(FileInfo info)
	{
		string text = File.ReadAllText(info.FullName);
		string text2 = info.Name;
		text2 = text2.Replace("save_", "").Replace(".sav", "");
		SaveGame saveGame = SaveGame.LoadFromString(text, text2);
		saveGame.FullPath = info.FullName;
		return saveGame;
	}

	public static List<FileInfo> GetDebugFiles()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(PlatformHelper.CurrentSavesDirectory);
		DirectoryInfo directoryInfo2 = new DirectoryInfo(Path.Combine(PlatformHelper.CurrentSavesDirectory, "AutoSave"));
		List<FileInfo> list = new List<FileInfo>();
		list.AddRange(directoryInfo.GetFiles());
		if (directoryInfo2.Exists)
		{
			list.AddRange(directoryInfo2.GetFiles());
		}
		list = list.OrderByDescending<FileInfo, string>((FileInfo x) => x.Name).ToList<FileInfo>();
		List<FileInfo> list2 = new List<FileInfo>();
		foreach (FileInfo fileInfo in list)
		{
			if (fileInfo.Name.StartsWith("save_debug") || fileInfo.Name.StartsWith("save_auto"))
			{
				list2.Add(fileInfo);
			}
		}
		return list2;
	}

	public string GetSaveSummary(SaveGame saveGame)
	{
		int count = QuestManager.instance.AllQuests.Count;
		int count2 = saveGame.CompletedAchievementIds.Count;
		int num = WorldManager.instance.CardDataPrefabs.Count<CardData>((CardData x) => !x.HideFromCardopedia);
		string text = Mathf.FloorToInt((float)(saveGame.FoundCardIds.Count + count2) / (float)(num + count) * 100f).ToString() + "%";
		string text2 = saveGame.LastSavedUtc.ToString("yyyy-MM-dd");
		return MewtationsLoc.Translate("label_save_game", new LocParam[]
		{
			LocParam.Create("save_index", (int.Parse(saveGame.SaveId) + 1).ToString()),
			LocParam.Create("percentage", text),
			LocParam.Create("date", text2)
		});
	}

	public static void OpenSavesDirectory()
	{
		Application.OpenURL("file:///" + PlatformHelper.CurrentSavesDirectory);
	}

	public string CreateDebugSaveWithId(string id)
	{
		string saveId = WorldManager.instance.CurrentSave.SaveId;
		string text = "debug_" + id;
		WorldManager.instance.CurrentSave.SaveId = text;
		WorldManager.instance.CurrentSave.LastPlayedRound = WorldManager.instance.Save.GetSaveRound();
		this.Save(WorldManager.instance.CurrentSave);
		WorldManager.instance.CurrentSave.SaveId = saveId;
		return text;
	}

	public static SaveManager instance;

	public const int CurrentSaveRoundVersion = 3;

	public SaveGame CurrentSave;

	private static SaveGame forceReloadSave;

	public static bool IsForceReload;
}
