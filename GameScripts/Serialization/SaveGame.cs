using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveGame
{
	public DateTime LastSavedUtc
	{
		get
		{
			return new DateTime(this.LastSavedUtcTicks);
		}
		set
		{
			this.LastSavedUtcTicks = value.Ticks;
		}
	}

	public static SaveGame LoadFromString(string json, string saveId)
	{
		SaveGame saveGame;
		if (!string.IsNullOrEmpty(json))
		{
			try
			{
				saveGame = JsonUtility.FromJson<SaveGame>(json);
				if (saveGame.SaveFormatIdentity != "Mewtations_Dogma")
				{
					Debug.LogWarning($"[Save Game Reset] Resetting save {saveId} due to SaveFormatIdentity mismatch.");
					saveGame = new SaveGame { SaveFormatIdentity = "Mewtations_Dogma", SaveDataVersion = 1 };
				}
				else if (saveGame.SaveDataVersion < 1)
				{
					saveGame = SaveMigrationManager.ExecuteMigration(saveGame, 1);
				}
				if (saveGame.LastPlayedRound != null && saveGame.LastPlayedRound.SavedCards.Count == 0 && saveGame.LastPlayedRound.SavedBoosters.Count == 0)
				{
					saveGame.LastPlayedRound = null;
				}
				goto IL_0053;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				saveGame = new SaveGame { SaveFormatIdentity = "Mewtations_Dogma", SaveDataVersion = 1 };
				goto IL_0053;
			}
		}
		saveGame = new SaveGame { SaveFormatIdentity = "Mewtations_Dogma", SaveDataVersion = 1 };
		IL_0053:
		saveGame.SaveId = saveId;
		return saveGame;
	}

	[NonSerialized]
	public string SaveId = "";

	[NonSerialized]
	public string FullPath = "";

	public SaveRound LastPlayedRound;

	public List<string> CompletedAchievementIds = new List<string>();

	public List<string> FoundCardIds = new List<string>();

	public List<string> FoundBoosterIds = new List<string>();

	public List<string> NewCardopediaIds = new List<string>();

	public List<string> NewKnowledgeIds = new List<string>();

	public List<string> SeenQuestIds = new List<string>();

	public List<SerializedKeyValuePair> ExtraKeyValues = new List<SerializedKeyValuePair>();

	public List<string> DisabledMods = new List<string>();

	public bool GotIslandIntroPack;

	public int ExpeditionSpecialMapPityCounter = 0;
	public List<string> CompletedSpecialMaps = new List<string>();
	public int ExpeditionBossRespawnTimer = 0;

	public long LastSavedUtcTicks;

	public bool FinishedGreed;

	public bool FinishedDeath;

	public bool FinishedHappiness;

	public string SaveFormatIdentity = "Mewtations_Dogma";
	public int SaveDataVersion = 1;

	// --- Codex / Recipe Book System ---
	public List<string> UnlockedRecipeIds = new List<string>();
	public List<string> UnreadRecipeIds = new List<string>();

	public void UnlockRecipe(string blueprintId)
	{
		if (!UnlockedRecipeIds.Contains(blueprintId))
		{
			UnlockedRecipeIds.Add(blueprintId);
			if (!UnreadRecipeIds.Contains(blueprintId))
			{
				UnreadRecipeIds.Add(blueprintId);
			}
		}
	}

	public void MarkRecipeRead(string blueprintId)
	{
		if (UnreadRecipeIds.Contains(blueprintId))
		{
			UnreadRecipeIds.Remove(blueprintId);
		}
	}
}

