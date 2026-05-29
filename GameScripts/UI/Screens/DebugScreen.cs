using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class DebugScreen : MonoBehaviour
{
	private void InitializeDebugScreen()
	{
		this.cardData = WorldManager.instance.CardDataPrefabs.OrderBy<CardData, string>(delegate(CardData x)
		{
			if (x.MyCardType == CardType.Ideas)
			{
				return "Idea: " + x.Name;
			}
			return x.Name;
		}).ToList<CardData>();
		this.cards.Clear();
		for (int i = 0; i < this.cardData.Count; i++)
		{
			CardData prefab = this.cardData[i];
			CustomButton customButton = Object.Instantiate<CustomButton>(PrefabManager.instance.DebugButtonPrefab);
			customButton.transform.SetParent(this.CardContent);
			customButton.transform.localPosition = Vector3.zero;
			customButton.transform.localScale = Vector3.one;
			customButton.transform.localRotation = Quaternion.identity;
			customButton.TextMeshPro.text = prefab.FullName;
			customButton.Clicked += delegate
			{
				CardData cardData = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), prefab, true, false, true, true);
				WorldManager.instance.StackSend(cardData.MyGameCard, Vector3.zero, null, true);
			};
			this.cards.Add(customButton);
		}
		this.UpdateSaveElements();
	}

	private void Start()
	{
		this.SwitchTab(DebugTab.General);
		this.InitializeDebugScreen();
	}

	private void Update()
	{
		this.CheckDebugInput();
		this.GeneralButton.Image.color = (this.GeneralRect.gameObject.activeInHierarchy ? ColorManager.instance.BackgroundColor : ColorManager.instance.InactiveBackgroundColor);
		this.CardsButton.Image.color = (this.CardRect.gameObject.activeInHierarchy ? ColorManager.instance.BackgroundColor : ColorManager.instance.InactiveBackgroundColor);
		this.ShortcutButton.Image.color = (this.ShortcutRect.gameObject.activeInHierarchy ? ColorManager.instance.BackgroundColor : ColorManager.instance.InactiveBackgroundColor);
		this.EffectButton.Image.color = (this.EffectRect.gameObject.activeInHierarchy ? ColorManager.instance.BackgroundColor : ColorManager.instance.InactiveBackgroundColor);
		this.SavesButton.Image.color = (this.SavesRect.gameObject.activeInHierarchy ? ColorManager.instance.BackgroundColor : ColorManager.instance.InactiveBackgroundColor);
		this.CutscenesButton.Image.color = (this.CutscenesRect.gameObject.activeInHierarchy ? ColorManager.instance.BackgroundColor : ColorManager.instance.InactiveBackgroundColor);
		this.EndlessMoonButton.TextMeshPro.text = MewtationsLoc.Translate("label_debug_endless_moon", new LocParam[] { LocParam.Create("on_off", DebugScreen.YesNo(WorldManager.instance.DebugEndlessMoonEnabled)) });
		this.PeacefulModeButton.TextMeshPro.text = MewtationsLoc.Translate("label_debug_toggle_peaceful_mode", new LocParam[] { LocParam.Create("on_off", DebugScreen.YesNo(WorldManager.instance.CurrentRunOptions.IsPeacefulMode)) });
		this.NoFoodButton.TextMeshPro.text = MewtationsLoc.Translate("label_debug_toggle_no_food", new LocParam[] { LocParam.Create("on_off", DebugScreen.YesNo(WorldManager.instance.DebugNoFoodEnabled)) });
		this.NeedVillagersButton.TextMeshPro.text = MewtationsLoc.Translate("label_debug_need_villagers", new LocParam[] { LocParam.Create("on_off", DebugScreen.YesNo(WorldManager.instance.DebugDontNeedVillagers)) });
		this.NoEnergyButton.TextMeshPro.text = MewtationsLoc.Translate("label_debug_no_energy", new LocParam[] { LocParam.Create("on_off", DebugScreen.YesNo(WorldManager.instance.DebugNoEnergyEnabled)) });
	}

	private void Awake()
	{
		DebugScreen.instance = this;
		this.SetHandlers();
	}

	private void CheckDebugInput()
	{
		if (this.SelectedStatusEffect != null && InputController.instance.GetInputBegan(0))
		{
			GameCard hoveredCard = WorldManager.instance.HoveredCard;
			if (hoveredCard != null)
			{
				CardData cardData = hoveredCard.CardData;
				if (cardData != null)
				{
					cardData.AddStatusEffect(this.SelectedStatusEffect);
				}
			}
			this.SelectedStatusEffect = null;
		}
		if (!Application.isEditor)
		{
			WorldManager.instance.CheckDebugInput();
		}
	}

	private void SwitchTab(DebugTab tab)
	{
		this.GeneralRect.gameObject.SetActive(tab == DebugTab.General);
		this.CardRect.gameObject.SetActive(tab == DebugTab.Cards);
		this.ShortcutRect.gameObject.SetActive(tab == DebugTab.Shortcuts);
		this.EffectRect.gameObject.SetActive(tab == DebugTab.Effects);
		this.SavesRect.gameObject.SetActive(tab == DebugTab.Saves);
		this.PresetRect.gameObject.SetActive(tab == DebugTab.Presets);
		this.CutscenesRect.gameObject.SetActive(tab == DebugTab.Cutscenes);
	}

	private void SetHandlers()
	{
		this.GeneralButton.Clicked += delegate
		{
			this.OpenTab = DebugTab.General;
			this.SwitchTab(this.OpenTab);
		};
		this.CardsButton.Clicked += delegate
		{
			this.OpenTab = DebugTab.Cards;
			this.SwitchTab(this.OpenTab);
		};
		this.ShortcutButton.Clicked += delegate
		{
			this.OpenTab = DebugTab.Shortcuts;
			this.SwitchTab(this.OpenTab);
		};
		this.EffectButton.Clicked += delegate
		{
			this.OpenTab = DebugTab.Effects;
			this.SwitchTab(this.OpenTab);
		};
		this.SavesButton.Clicked += delegate
		{
			this.OpenTab = DebugTab.Saves;
			this.SwitchTab(this.OpenTab);
		};
		this.CutscenesButton.Clicked += delegate
		{
			this.OpenTab = DebugTab.Cutscenes;
			this.SwitchTab(this.OpenTab);
		};
		this.PresetButton.Clicked += delegate
		{
			if (Application.isEditor)
			{
				this.OpenTab = DebugTab.Presets;
				this.SwitchTab(this.OpenTab);
				return;
			}
			GameCanvas.instance.ShowSimpleModal("This option is only available in editor", "Not available!");
		};
		this.SearchField.onValueChanged.AddListener(delegate(string value)
		{
			foreach (CustomButton customButton3 in this.cards)
			{
				customButton3.gameObject.SetActive(false);
			}
			foreach (CustomButton customButton4 in this.cards.Where<CustomButton>((CustomButton card) => card.TextMeshPro.text.ToLower().Replace(" ", "").Contains(value.ToLower().Replace(" ", ""))).ToList<CustomButton>())
			{
				customButton4.gameObject.SetActive(true);
			}
		});
		this.EndlessMoonButton.Clicked += delegate
		{
			this.ToggleEndlessMoon();
		};
		this.NeedVillagersButton.Clicked += delegate
		{
			this.ToggleNeedVillagers();
		};
		this.EndCurrentMoonButton.Clicked += delegate
		{
			this.EndCurrentMoon();
		};
		this.StartCitiesRunButton.Clicked += delegate
		{
			this.StartCitiesRun();
		};
		this.UnlockBoostersButton.Clicked += delegate
		{
			this.UnlockBoosters();
		};
		this.UnlockIdeasButton.Clicked += delegate
		{
			this.UnlockIdeas();
		};
		this.UnlockBaseGameIdeasButton.Clicked += delegate
		{
			this.UnlockBaseGameIdeas();
		};
		this.UnlockQuestsButton.Clicked += delegate
		{
			this.UnlockQuests();
		};
		this.PeacefulModeButton.Clicked += delegate
		{
			this.TogglePeacefulMode();
		};
		this.NoFoodButton.Clicked += delegate
		{
			this.ToggleNoFood();
		};
		this.NoEnergyButton.Clicked += delegate
		{
			this.ToggleNoEnergy();
		};
		this.CoinChestButton.Clicked += delegate
		{
			this.SpawnFullCoinChest();
		};
		this.ResetRunButton.Clicked += delegate
		{
			this.ResetRunVariables();
		};
		this.SpawnEnemiesButton.Clicked += delegate
		{
			this.SpawnEnemies();
		};
		this.DemonScenarioButton.Clicked += delegate
		{
			this.ScenarioDemon();
		};
		this.KrakenScenarioButton.Clicked += delegate
		{
			this.ScenarioKraken();
		};
		this.IslandScenarioButton.Clicked += delegate
		{
			this.ScenarioIsland();
		};
		this.DemonLordScenarioButton.Clicked += delegate
		{
			this.ScenarioDemonLord();
		};
		this.WitchForestButton.Clicked += delegate
		{
			this.ScenarioWitchForest();
		};
		using (List<Type>.Enumerator enumerator = (from type in typeof(StatusEffect).Assembly.GetTypes()
			where type.IsSubclassOf(typeof(StatusEffect))
			select type).ToList<Type>().GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Type statusEffect = enumerator.Current;
				StatusEffect statusEffect2 = Activator.CreateInstance(statusEffect) as StatusEffect;
				CustomButton customButton = Object.Instantiate<CustomButton>(PrefabManager.instance.DebugButtonPrefab);
				customButton.TextMeshPro.text = statusEffect2.Name;
				customButton.transform.SetParentClean(this.StatusEffectButtonParent);
				customButton.Clicked += delegate
				{
					this.SelectedStatusEffect = Activator.CreateInstance(statusEffect) as StatusEffect;
				};
			}
		}
		using (List<ScriptableCutscene>.Enumerator enumerator2 = WorldManager.instance.GameDataLoader.Cutscenes.GetEnumerator())
		{
			while (enumerator2.MoveNext())
			{
				ScriptableCutscene cutscenes = enumerator2.Current;
				CustomButton customButton2 = Object.Instantiate<CustomButton>(PrefabManager.instance.DebugButtonPrefab);
				customButton2.TextMeshPro.text = cutscenes.CutsceneId;
				customButton2.transform.SetParentClean(this.CutsceneButtonParent);
				customButton2.Clicked += delegate
				{
					WorldManager.instance.Cutscene.QueueCutscene(cutscenes);
				};
			}
		}
		this.SaveButton.Clicked += delegate
		{
			SaveManager.instance.CreateDebugSaveWithId(DateTime.Now.ToString("dd_MM_HHmm_ss"));
			this.UpdateSaveElements();
		};
		this.OpenSavesDirectoryButton.Clicked += delegate
		{
			SaveManager.OpenSavesDirectory();
		};
		this.PresetSaveButton.Clicked += delegate
		{
			this.SavePreset();
			this.UpdatePresetsElements();
		};
		this.OpenPresetsDirectoryButton.Clicked += delegate
		{
			this.openPresetsDirectory();
		};
	}

	public void SavePreset()
	{
		SavedPreset savedPreset = new SavedPreset();
		savedPreset.SaveId = WorldManager.instance.CurrentBoard.Id + "_" + DateTime.Now.ToString("dd_MM_HHmm_ss");
		savedPreset.SavedCards = new List<SavedCard>();
		foreach (GameCard gameCard in WorldManager.instance.CardQuery.GetAllCardsOnBoard(WorldManager.instance.CurrentBoard.Id))
		{
			savedPreset.SavedCards.Add(gameCard.ToSavedCard());
		}
		WorldManager.instance.SavePreset(savedPreset);
	}

	private void openPresetsDirectory()
	{
		string text = Application.dataPath + "/PresetSaves";
		text = text.Replace("/", "\\");
		Process.Start("explorer.exe", text);
	}

	public void SpawnEnemies()
	{
		foreach (CardIdWithEquipment cardIdWithEquipment in SpawnHelper.GetEnemiesToSpawn(new List<SetCardBagType> { SetCardBagType.BasicEnemy }, 50f, true))
		{
			Combatable combatable = WorldManager.instance.CreateCard(WorldManager.instance.GetRandomSpawnPosition(), cardIdWithEquipment, true, true, true) as Combatable;
			combatable.HealthPoints = combatable.ProcessedCombatStats.MaxHealth;
			combatable.MyGameCard.SendIt();
		}
	}

	public void ResetRunVariables()
	{
		WorldManager.instance.CurrentRunVariables.VisitedForest = false;
		WorldManager.instance.CurrentRunVariables.VisitedIsland = false;
		WorldManager.instance.CurrentRunVariables.ForestWave = 1;
	}

	public void ToggleEndlessMoon()
	{
		WorldManager.instance.DebugEndlessMoonEnabled = !WorldManager.instance.DebugEndlessMoonEnabled;
	}

	public void ToggleNeedVillagers()
	{
		WorldManager.instance.DebugDontNeedVillagers = !WorldManager.instance.DebugDontNeedVillagers;
	}

	public void ToggleNoEnergy()
	{
		WorldManager.instance.DebugNoEnergyEnabled = !WorldManager.instance.DebugNoEnergyEnabled;
	}

	public void EndCurrentMoon()
	{
		WorldManager.instance.Time.MonthTimer = WorldManager.instance.MonthTime - 1f;
	}

	public void StartCitiesRun()
	{
		CardData cardData = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "villager", true, true, true);
		WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "event_industrial_revolution", true, true, true).MyGameCard.SetChild(cardData.MyGameCard);
	}

	public void UnlockBoosters()
	{
	}

	public void UnlockIdeas()
	{
		WorldManager.instance.DebugUnlockIdeas(false);
		GameScreen.instance.UpdateIdeasLog();
	}

	public void UnlockBaseGameIdeas()
	{
		WorldManager.instance.DebugUnlockIdeas(true);
		GameScreen.instance.UpdateIdeasLog();
	}

	public void UnlockQuests()
	{
		QuestManager.instance.DebugUnlockAllQuests();
	}

	public void TogglePeacefulMode()
	{
		WorldManager.instance.CurrentRunOptions.IsPeacefulMode = !WorldManager.instance.CurrentRunOptions.IsPeacefulMode;
	}

	public void ToggleNoFood()
	{
		WorldManager.instance.DebugNoFoodEnabled = !WorldManager.instance.DebugNoFoodEnabled;
	}

	public void SpawnFullCoinChest()
	{
		if (WorldManager.instance.CurrentBoard.BoardOptions.UsesShells)
		{
			(WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "shell_chest", true, false, true) as Chest).CoinCount = 100;
			return;
		}
		(WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "coin_chest", true, false, true) as Chest).CoinCount = 100;
	}

	public void ScenarioDemon()
	{
		CardData cardData = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "goblet", true, false, true);
		CardData cardData2 = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "temple", true, false, true);
		WorldManager.instance.StackSendTo(cardData.MyGameCard, cardData2.MyGameCard);
	}

	public void ScenarioKraken()
	{
		CardData cardData = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "sacred_key", true, false, true);
		CardData cardData2 = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "sacred_chest", true, false, true);
		WorldManager.instance.StackSendTo(cardData.MyGameCard, cardData2.MyGameCard);
	}

	public void ScenarioIsland()
	{
		CardData cardData = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "villager", true, false, true);
		CardData cardData2 = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "rowboat", true, false, true);
		WorldManager.instance.StackSendTo(cardData.MyGameCard, cardData2.MyGameCard);
	}

	public void ScenarioDemonLord()
	{
		CardData cardData = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "island_relic", true, false, true);
		CardData cardData2 = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "cathedral", true, false, true);
		WorldManager.instance.StackSendTo(cardData.MyGameCard, cardData2.MyGameCard);
	}

	public void ScenarioWitchForest()
	{
		CardData cardData = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "stable_portal", true, false, true);
		CardData cardData2 = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "villager", true, false, true);
		CardData cardData3 = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "sword", true, false, true);
		cardData2.MyGameCard.SendIt();
		cardData.MyGameCard.SendIt();
		WorldManager.instance.StackSendTo(cardData3.MyGameCard, cardData2.MyGameCard);
		WorldManager.instance.StackSendTo(cardData.MyGameCard, cardData2.MyGameCard);
	}

	public static string YesNo(bool a)
	{
		if (!a)
		{
			return MewtationsLoc.Translate("label_off");
		}
		return MewtationsLoc.Translate("label_on");
	}

	private void UpdateSaveElements()
	{
		foreach (object obj in this.SavesParent)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		using (List<FileInfo>.Enumerator enumerator2 = SaveManager.GetDebugFiles().GetEnumerator())
		{
			while (enumerator2.MoveNext())
			{
				FileInfo file = enumerator2.Current;
				CustomButton cb = Object.Instantiate<CustomButton>(PrefabManager.instance.ButtonPrefab);
				cb.HardSetText(file.Name.Replace("_", " ").Replace(".sav", ""));
				cb.TextMeshPro.fontSize = 20f;
				cb.Clicked += delegate
				{
					if (cb.WasRightClick)
					{
						FileHelper.ArchiveFile(file.FullName);
						this.UpdateSaveElements();
						return;
					}
					SaveManager.ForceReload(SaveManager.GetSaveFromFileInfo(file));
					WorldManager.RestartGame();
				};
				cb.transform.SetParentClean(this.SavesParent);
			}
		}
	}

	private void UpdatePresetsElements()
	{
		foreach (object obj in this.PresetsParent)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		foreach (SavedPreset savedPreset in this.GetSavedPresets())
		{
			CustomButton customButton = Object.Instantiate<CustomButton>(PrefabManager.instance.ButtonPrefab);
			customButton.HardSetText(savedPreset.SaveId.Replace("_", " "));
			customButton.TextMeshPro.fontSize = 20f;
			customButton.Clicked += delegate
			{
			};
			customButton.transform.SetParentClean(this.PresetsParent);
		}
	}

	private List<SavedPreset> GetSavedPresets()
	{
		List<SavedPreset> list = new List<SavedPreset>();
		foreach (FileInfo fileInfo in DebugScreen.GetPresetFiles())
		{
			if (!(fileInfo.Extension == ".meta"))
			{
				SavedPreset savedPreset = JsonUtility.FromJson<SavedPreset>(File.ReadAllText(fileInfo.FullName));
				savedPreset.FullPath = fileInfo.FullName;
				list.Add(savedPreset);
			}
		}
		return list;
	}

	public void AutoSave()
	{
		SaveGame currentSave = WorldManager.instance.CurrentSave;
		string saveId = currentSave.SaveId;
		string text = DateTime.Now.ToString("dd_MM_HHmm");
		currentSave.SaveId = string.Format("auto_{0}_moon_{1}", text, WorldManager.instance.Time.CurrentMonth);
		currentSave.LastPlayedRound = WorldManager.instance.Save.GetSaveRound();
		string text2 = JsonUtility.ToJson(currentSave);
		FileHelper.SaveFile(currentSave.SaveId, text2, "AutoSave");
		Debug.Log("Auto saved! (" + currentSave.SaveId + ")");
		currentSave.SaveId = saveId;
		this.UpdateSaveElements();
	}

	private static List<FileInfo> GetPresetFiles()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(Application.dataPath + "/PresetSaves");
		List<FileInfo> list = new List<FileInfo>();
		list.AddRange(directoryInfo.GetFiles());
		List<FileInfo> list2 = list.OrderBy<FileInfo, string>((FileInfo x) => x.Name).ToList<FileInfo>();
		List<FileInfo> list3 = new List<FileInfo>();
		foreach (FileInfo fileInfo in list2)
		{
			if (fileInfo.Name.StartsWith("preset"))
			{
				list3.Add(fileInfo);
			}
		}
		return list3;
	}

	public TMP_InputField SearchField;

	public RectTransform GeneralRect;

	public RectTransform CardRect;

	public RectTransform ShortcutRect;

	public RectTransform EffectRect;

	public RectTransform SavesRect;

	public RectTransform PresetRect;

	public RectTransform CutscenesRect;

	public RectTransform GeneralContent;

	public RectTransform CardContent;

	public RectTransform ShortcutContent;

	public RectTransform EffectContent;

	public CustomButton GeneralButton;

	public CustomButton CardsButton;

	public CustomButton ShortcutButton;

	public CustomButton EffectButton;

	public CustomButton SavesButton;

	public CustomButton PresetButton;

	public CustomButton CutscenesButton;

	public CustomButton EndlessMoonButton;

	public CustomButton NeedVillagersButton;

	public CustomButton EndCurrentMoonButton;

	public CustomButton UnlockBaseGameIdeasButton;

	public CustomButton UnlockIdeasButton;

	public CustomButton UnlockBoostersButton;

	public CustomButton UnlockQuestsButton;

	public CustomButton PeacefulModeButton;

	public CustomButton NoFoodButton;

	public CustomButton NoEnergyButton;

	public CustomButton CoinChestButton;

	public CustomButton ResetRunButton;

	public CustomButton SpawnEnemiesButton;

	public CustomButton StartCitiesRunButton;

	public CustomButton DemonScenarioButton;

	public CustomButton KrakenScenarioButton;

	public CustomButton IslandScenarioButton;

	public CustomButton DemonLordScenarioButton;

	public CustomButton WitchForestButton;

	public RectTransform StatusEffectButtonParent;

	public RectTransform CutsceneButtonParent;

	public RectTransform SavesParent;

	public CustomButton SaveButton;

	public CustomButton OpenSavesDirectoryButton;

	public RectTransform PresetsParent;

	public CustomButton PresetSaveButton;

	public CustomButton OpenPresetsDirectoryButton;

	private DebugTab OpenTab;

	private StatusEffect SelectedStatusEffect;

	private List<CardData> cardData = new List<CardData>();

	private List<CustomButton> cards = new List<CustomButton>();

	public static DebugScreen instance;
}
