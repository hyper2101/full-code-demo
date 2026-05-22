using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WorldManager : MonoBehaviour
{
	public GameCard DraggingCard
	{
		get
		{
			return this.DraggingDraggable as GameCard;
		}
	}

	public GameCard HoveredCard
	{
		get
		{
			return this.HoveredDraggable as GameCard;
		}
	}

	public List<CardData> CardDataPrefabs
	{
		get
		{
			return this.GameDataLoader.CardDataPrefabs;
		}
	}

	public List<Blueprint> BlueprintPrefabs
	{
		get
		{
			return this.GameDataLoader.BlueprintPrefabs;
		}
	}

	public List<BoosterpackData> BoosterPackDatas
	{
		get
		{
			return this.GameDataLoader.BoosterpackDatas;
		}
	}

	public SaveGame CurrentSave
	{
		get
		{
			return SaveManager.instance.CurrentSave;
		}
	}

	public int CurrentMonth => Time.CurrentMonth;

	public float MonthTime
	{
		get
		{
			if (this.CurrentRunOptions != null)
			{
				if (this.CurrentRunOptions.MoonLength == MoonLength.Short)
				{
					return 90f;
				}
				if (this.CurrentRunOptions.MoonLength == MoonLength.Normal)
				{
					return 120f;
				}
				if (this.CurrentRunOptions.MoonLength == MoonLength.Long)
				{
					return 200f;
				}
			}
			return 120f;
		}
	}

	public bool IsPlaying
	{
		get
		{
			return this.CurrentGameState == WorldManager.GameState.Playing;
		}
	}

	public bool InAnimation
	{
		get
		{
			return this.currentAnimationRoutine != null || this.currentAnimation != null;
		}
	}

	public bool CanInteract
	{
		get
		{
			return this.IsPlaying && ((this.currentAnimationRoutine == null && this.currentAnimation == null) || this.RemovingCards) && !GameScreen.instance.ControllerIsInUI && !GameCanvas.instance.ModalIsOpen;
		}
	}

	public List<SerializedKeyValuePair> SaveExtraKeyValues
	{
		get
		{
			return this.CurrentSave.ExtraKeyValues;
		}
	}

	private void Awake()
	{
		WorldManager.instance = this;
		this.Time = new TimeSystem(this);
		this.Economy = new EconomySystem(this);
		this.Save = new SaveSystem(this);
		this.CardQuery = new CardQuerySystem(this);
		this.Cutscene = new CutsceneSystem(this);
		this.Input = new InputSystem(this);
		this.DayEvent = new DayEventSystem(this);
		base.gameObject.AddComponent<Mewtations.Combat.TurnBasedCombatManager>();
		base.gameObject.AddComponent<Mewtations.Combat.CombatOverlayUI>();
		base.gameObject.AddComponent<Mewtations.Expedition.ExpeditionManager>();
		base.gameObject.AddComponent<Mewtations.Expedition.ExpeditionMapUI>();
		base.gameObject.AddComponent<Mewtations.Dialogue.DialogueSystem>();
		this.AllCards = new List<GameCard>();
		this.Boards = Object.FindObjectsOfType<GameBoard>().ToList<GameBoard>();
		this.CurrentGameState = WorldManager.GameState.InMenu;
		this.GameDataLoader = new GameDataLoader(this.SpiritDLCInstalled(), this.CitiesDLCInstalled());
		Subprint.UpdateAnyVillagerCardIds();
		Subprint.UpdateAnyWorkerCardIds();
		if (Application.isEditor)
		{
			this.DebugEndlessMoonEnabled = DebugOptions.Default.EndlessMoonEnabled;
			this.DebugNoFoodEnabled = DebugOptions.Default.NoFoodEnabled;
			this.DebugDontNeedVillagers = DebugOptions.Default.DontNeedVillagers;
			this.DebugNoEnergyEnabled = DebugOptions.Default.NoEnergyEnabled;
			this.CurrentSave.FinishedDeath = DebugOptions.Default.CursedFinished;
			this.CurrentSave.FinishedGreed = DebugOptions.Default.CursedFinished;
			this.CurrentSave.FinishedHappiness = DebugOptions.Default.CursedFinished;
			base.gameObject.AddComponent<Screenshotter>();
		}
		Shader.SetGlobalFloat("_WorldSizeIncrease", 0f);
		Shader.SetGlobalFloat("_WorldSizeIncreaseNormalized", 0f);
		SokLoc.instance.LanguageChanged += this.OnLanguageChange;
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		this.InitializeBaseVillagerSpeedRules();
	}

	private void Start()
	{
		OptionsScreen.LoadSettings();
		QuestManager.instance.CheckSteamAchievements();
		WorldManager.CheckForceReloadSave();
	}

	private void InitActionTimeBases()
	{
		this.actionTimeBases.Add(new ActionTimeBase((ActionTimeParams p) => p.villager.Id == "dog" || p.villager.Id == "cat", 2f));
		this.actionTimeBases.Add(new ActionTimeBase((ActionTimeParams p) => p.villager.Id == "fisher" && p.actionId == "complete_harvest" && p.baseCard.Id == "fishing_spot", 0.5f));
		this.actionTimeBases.Add(new ActionTimeBase((ActionTimeParams p) => p.villager.Id == "explorer", 1.25f));
		this.actionTimeBases.Add(new ActionTimeBase((ActionTimeParams p) => p.villager.Id == "explorer" && p.actionId == "complete_harvest" && p.baseCard.MyCardType == CardType.Locations, 0.5f));
		this.actionTimeBases.Add(new ActionTimeBase((ActionTimeParams p) => p.villager.Id == "lumberjack" && p.actionId == "complete_harvest" && (p.baseCard.Id == "lumbercamp" || p.baseCard.Id == "apple_tree" || p.baseCard.Id == "tree" || p.baseCard.Id == "olive_tree"), 0.5f));
		this.actionTimeBases.Add(new ActionTimeBase((ActionTimeParams p) => p.villager.Id == "builder" && p.actionId == "finish_blueprint", 0.5f));
		this.actionTimeBases.Add(new ActionTimeBase((ActionTimeParams p) => p.villager.Id == "miner" && p.actionId == "complete_harvest" && (p.baseCard.Id == "gold_mine" || p.baseCard.Id == "mine" || p.baseCard.Id == "rock" || p.baseCard.Id == "quarry" || p.baseCard.Id == "iron_deposit"), 0.5f));
		this.actionTimeBases.Add(new ActionTimeBase((ActionTimeParams p) => p.villager.HasEquipableWithId("scythe") && p.actionId == "complete_harvest" && (p.baseCard.Id == "berrybush" || p.baseCard.Id == "olive_tree" || p.baseCard.Id == "apple_tree" || p.baseCard.Id == "grape_vine" || p.baseCard.Id == "tomato_plant"), 0.5f));
	}

	private void InitActionTimeModifiers()
	{
		this.actionTimeModifiers.Add(new ActionTimeModifier((ActionTimeParams p) => p.villager.HasStatusEffectOfType<StatusEffect_Drunk>(), 2f));
		this.actionTimeModifiers.Add(new ActionTimeModifier((ActionTimeParams p) => p.villager.HasStatusEffectOfType<StatusEffect_Anxious>(), 2.5f));
		this.actionTimeModifiers.Add(new ActionTimeModifier((ActionTimeParams p) => p.villager.HasStatusEffectOfType<StatusEffect_WellFed>(), 0.5f));
		this.actionTimeModifiers.Add(new ActionTimeModifier((ActionTimeParams p) => p.villager.MyLifeStage == LifeStage.Teenager, 0.75f));
		this.actionTimeModifiers.Add(new ActionTimeModifier((ActionTimeParams p) => p.villager.MyLifeStage == LifeStage.Elderly, 1.25f));
	}

	public void InitializeBaseVillagerSpeedRules()
	{
		this.InitActionTimeBases();
		this.InitActionTimeModifiers();
	}

	public HashSet<string> FindMissingCardsInSave()
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (SavedCard savedCard in this.CurrentSave.LastPlayedRound.SavedCards)
		{
			if (this.GameDataLoader.GetCardFromId(savedCard.CardPrefabId, true) == null)
			{
				hashSet.Add(savedCard.CardPrefabId);
			}
		}
		return hashSet;
	}

	public void LoadPreviousRound()
	{
		this.LoadSaveRound(this.CurrentSave.LastPlayedRound);
		foreach (GameBoard gameBoard in this.Boards)
		{
			gameBoard.WorldSizeIncrease = this.DetermineTargetWorldSize(gameBoard);
		}
		if (QuestManager.instance.QuestIsComplete(AllQuests.KillDemon) && !this.CurrentSave.GotIslandIntroPack)
		{
			this.QueueCutscene(Cutscenes.IslandIntroPack());
		}
	}

	public SaveRound GetSaveRound()
	{
		SaveRound saveRound = new SaveRound();
		saveRound.SaveVersion = 3;
		saveRound.SavedCards = new List<SavedCard>();
		saveRound.SavedBoosters = new List<SavedBooster>();
		saveRound.SavedBoosterBoxes = new List<SavedBoosterBox>();
		saveRound.SavedConflicts = new List<SavedConflict>();
		saveRound.RunVariables = this.CurrentRunVariables;
		saveRound.RunOptions = this.CurrentRunOptions;
		saveRound.BoughtBoosterIds = this.BoughtBoosterIds;
		saveRound.CurrentBoardId = this.CurrentBoard.Id;
		saveRound.ExtraKeyValues = this.RoundExtraKeyValues;
		foreach (GameCard gameCard in this.AllCards)
		{
			saveRound.SavedCards.Add(gameCard.ToSavedCard());
		}
		foreach (Boosterpack boosterpack in this.AllBoosters)
		{
			saveRound.SavedBoosters.Add(new SavedBooster
			{
				BoosterId = boosterpack.BoosterId,
				TimesOpened = boosterpack.TimesOpened,
				BoardId = boosterpack.MyBoard.Id,
				Position = boosterpack.TargetPosition
			});
		}
		foreach (BuyBoosterBox buyBoosterBox in this.AllBoosterBoxes)
		{
			saveRound.SavedBoosterBoxes.Add(new SavedBoosterBox
			{
				BoosterId = buyBoosterBox.BoosterId,
				StoredCostAmount = buyBoosterBox.StoredCostAmount
			});
		}
		saveRound.MonthTimer = this.MonthTimer;
		saveRound.CurrentMonth = this.CurrentMonth;
		saveRound.OldCurrentMonth = this.OldCurrentMonth;
		saveRound.BoardMonths = this.BoardMonths.ToSavedMonth();
		saveRound.NewCardsFound = this.NewCardsFound;
		saveRound.QuestsCompleted = this.QuestsCompleted;
		saveRound.CitiesWellbeing = CitiesManager.instance.Wellbeing;
		saveRound.CitiesConflictMonth = CitiesManager.instance.NextConflictMonth;
		saveRound.CitiesDisaster = CitiesManager.instance.ActiveEvent;
		foreach (Conflict conflict in this.GetAllConflicts())
		{
			List<SavedConflict> savedConflicts = saveRound.SavedConflicts;
			SavedConflict savedConflict = new SavedConflict();
			savedConflict.Id = conflict.Id;
			savedConflict.InitiatorCardId = conflict.Initiator.UniqueId;
			savedConflict.InvolvedCards = conflict.Participants.Select<Combatable, string>((Combatable x) => x.UniqueId).ToList<string>();
			savedConflict.StartPosition = conflict.ConflictStartPosition;
			savedConflicts.Add(savedConflict);
		}
		return saveRound;
	}

	private void OnApplicationQuit()
	{
		Shader.SetGlobalFloat("_WorldSizeIncrease", 0f);
		Shader.SetGlobalFloat("_WorldSizeIncreaseNormalized", 0f);
		if (this.CurrentGameState == WorldManager.GameState.Playing && this.currentAnimationRoutine == null && this.currentAnimation == null)
		{
			SaveManager.instance.Save(true);
		}
	}

	private void OnDestroy()
	{
		if (SokLoc.instance != null)
		{
			SokLoc.instance.LanguageChanged -= this.OnLanguageChange;
		}
	}

	public void SaveAndGoBackToMenu()
	{
		SaveManager.instance.Save(true);
		WorldManager.RestartGame();
	}

	public void UpdateCardTargets()
	{
		foreach (CardTarget cardTarget in this.CardTargets)
		{
			BuyBoosterBox buyBoosterBox = cardTarget as BuyBoosterBox;
			if (buyBoosterBox != null)
			{
				buyBoosterBox.UpdateUndiscoveredCards();
			}
		}
	}

	public void Play()
	{
		GameCanvas.instance.SetScreen<GameScreen>();
		this.CurrentGameState = WorldManager.GameState.Playing;
		GameCamera.instance.CenterOnBoard(this.CurrentBoard);
		QuestManager.instance.CheckPacksUnlocked();
		GameScreen.instance.OnBoardChange();
		this.UpdateCardTargets();
	}

	public GameBoard CurrentBoard { get; private set; }

	public Vector3 MiddleOfBoard()
	{
		return this.CurrentBoard.MiddleOfBoard();
	}

	public void SetViewType(ViewType type)
	{
		this.CurrentView = type;
		foreach (GameCard gameCard in this.AllCards)
		{
			gameCard.UpdateCardPalette();
		}
		CitiesManager.instance.StopDrawCable(null);
	}

	public void SavePreset(SavedPreset preset)
	{
		string text = JsonUtility.ToJson(preset);
		FileHelper.SavePresetFile(preset.SaveId, text);
	}

	public Boosterpack IntroPack
	{
		get
		{
			return this.AllBoosters.FirstOrDefault<Boosterpack>((Boosterpack x) => x.IsIntroPack && x.MyBoard.IsCurrent);
		}
	}

	public void StartNewRound()
	{
		this.ClearRound();
		this.CurrentBoard = this.GetBoardWithId("main");
		this.CreateBoosterpack(this.MiddleOfBoard(), "starter");
	}

	public void ClearSaveAndRestart()
	{
		SaveGame saveGame = new SaveGame();
		saveGame.LastSavedUtc = DateTime.UtcNow;
		saveGame.SaveId = this.CurrentSave.SaveId;
		string text = JsonUtility.ToJson(saveGame);
		FileHelper.SaveFile(this.CurrentSave.SaveId, text);
		SaveManager.instance.Save(saveGame);
		WorldManager.RestartGame();
	}

	public float TimeScale
	{
		get
		{
			if (!this.IsPlaying)
			{
				return 0f;
			}
			if (this.currentAnimationRoutine != null || this.currentAnimation != null)
			{
				return 0f;
			}
			if (GameCanvas.instance.ModalIsOpen)
			{
				return 0f;
			}
			if (TransitionScreen.InTransition)
			{
				return 0f;
			}
			if (this.DayEvent != null && this.DayEvent.HasActiveEvent())
			{
				return 0f;
			}
			if (Mewtations.Expedition.ExpeditionManager.Instance != null && Mewtations.Expedition.ExpeditionManager.Instance.IsExpeditionActive)
			{
				return 0f;
			}
			return this.SpeedUp;
		}
	}

	public float PhysicsTimeScale
	{
		get
		{
			if (!this.IsPlaying)
			{
				return 0f;
			}
			if (this.SpeedUp == 0f || (this.DayEvent != null && this.DayEvent.HasActiveEvent()))
			{
				return 1f;
			}
			if (Mewtations.Expedition.ExpeditionManager.Instance != null && Mewtations.Expedition.ExpeditionManager.Instance.IsExpeditionActive)
			{
				return 1f;
			}
			return this.SpeedUp;
		}
	}

	public void QuestCompleted(Quest quest)
	{
		Debug.Log("Completed quest " + quest.Id);
		if (GameScreen.instance != null && quest.QuestLocation == this.CurrentBoard.Location)
		{
			GameScreen.instance.AddNotification(SokLoc.Translate("label_quest_completed"), quest.Description, delegate
			{
				GameScreen.instance.ScrollToQuest(quest);
			});
		}
		AudioManager.me.PlaySound2D(AudioManager.me.QuestComplete, 1f, 0.1f);
		BoosterpackData boosterpackData = QuestManager.instance.JustUnlockedPack();
		if (boosterpackData != null)
		{
			bool flag = boosterpackData.BoosterLocation == quest.QuestLocation;
			if (boosterpackData.BoosterLocation != this.CurrentBoard.Location)
			{
				flag = false;
			}
			if (TransitionScreen.InTransition)
			{
				flag = false;
			}
			if (flag)
			{
				this.QueueCutscene(Cutscenes.JustUnlockedPack(boosterpackData));
			}
		}
	}

	public void OnLanguageChange()
	{
		foreach (GameCard gameCard in this.AllCards)
		{
			gameCard.CardData.OnLanguageChange();
		}
		foreach (CardData cardData in this.GameDataLoader.CardDataPrefabs)
		{
			cardData.OnLanguageChange();
		}
	}

	private void QueueAnimation(QueuedAnimation anim)
	{
		this.queuedAnimations.Add(anim);
	}

	private void CheckQueuedAnimations()
	{
		if (this.InAnimation)
		{
			return;
		}
		if (GameCanvas.instance.ModalIsOpen)
		{
			return;
		}
		if (!this.IsPlaying)
		{
			return;
		}
		if (this.currentAnimation == null && this.queuedAnimations.Count > 0)
		{
			this.CloseOpenInventories();
			this.SetViewType(ViewType.Default);
			this.currentAnimation = this.queuedAnimations[0];
			this.currentAnimation.OnActivate();
			this.queuedAnimations.RemoveAt(0);
		}
	}

	public void QueueCutscene(IEnumerator coroutine)
	{
		this.QueueAnimation(new QueuedAnimation(delegate
		{
			this.StartCoroutine(coroutine);
		}, null));
	}

	public void QueueCutsceneIfNotQueued(IEnumerator coroutine, string id)
	{
		if (this.queuedAnimations.Any<QueuedAnimation>((QueuedAnimation x) => x.Id == id))
		{
			return;
		}
		this.QueueAnimation(new QueuedAnimation(delegate
		{
			this.StartCoroutine(coroutine);
		}, id));
	}

	public void QueueCutsceneIfNotPlayed(string cutsceneId)
	{
		if (this.CurrentRunVariables.PlayedCutsceneIds.Contains(cutsceneId))
		{
			return;
		}
		this.CurrentRunVariables.PlayedCutsceneIds.Add(cutsceneId);
		this.QueueCutscene(cutsceneId);
	}

	public void QueueCutscene(string cutsceneId)
	{
		ScriptableCutscene cutsceneWithId = this.GameDataLoader.GetCutsceneWithId(cutsceneId);
		this.QueueCutscene(cutsceneWithId);
	}

	public void QueueCutscene(ScriptableCutscene cutscene)
	{
		this.QueueAnimation(new QueuedAnimation(delegate
		{
			if (!this.CurrentRunVariables.PlayedCutsceneIds.Contains(cutscene.CutsceneId))
			{
				this.CurrentRunVariables.PlayedCutsceneIds.Add(cutscene.CutsceneId);
			}
			this.StartCoroutine(Cutscenes.RunScriptableCutscene(cutscene));
		}, null));
	}

	public void ModalAbandonCity()
	{
		GameCanvas.instance.AbandonCityPrompt(new Action(this.AbandonCity), null);
	}

	public void AbandonCity()
	{
		GameBoard citiesBoard = this.GetCurrentBoardSafe();
		this.GoToBoard(this.GetBoardWithId("main"), delegate
		{
			this.RemoveAllCardsFromBoard(citiesBoard.Id, true);
			this.ResetBoughtBoostersOnLocation(citiesBoard.Location);
			this.ResetCityVariables();
			if (this.CurrentGameState == WorldManager.GameState.Paused)
			{
				this.TogglePause();
			}
		}, "cities");
	}

	private void ResetCityVariables()
	{
		this.CurrentRunVariables.HasCitiesBoard = false;
		this.CurrentRunVariables.BuiltLandmarks = new List<string>();
		this.CurrentRunVariables.OpenedFirstTrash = false;
		this.BoardMonths.CitiesMonth = 1;
		CitiesManager.instance.Wellbeing = 15;
	}

	public void TogglePause()
	{
		if (this.currentAnimationRoutine != null)
		{
			return;
		}
		if (this.currentAnimation != null)
		{
			return;
		}
		if (this.CurrentGameState != WorldManager.GameState.Paused && this.CurrentGameState != WorldManager.GameState.Playing)
		{
			return;
		}
		if (GameCanvas.instance.ModalIsOpen)
		{
			return;
		}
		if (this.CurrentGameState == WorldManager.GameState.Playing)
		{
			this.CurrentGameState = WorldManager.GameState.Paused;
		}
		else if (this.CurrentGameState == WorldManager.GameState.Paused)
		{
			this.CurrentGameState = WorldManager.GameState.Playing;
		}
		bool flag = this.CurrentGameState == WorldManager.GameState.Paused;
		this.SetViewType(ViewType.Default);
		if (flag)
		{
			GameCanvas.instance.SetScreen<PauseScreen>();
			return;
		}
		GameCanvas.instance.SetScreen<GameScreen>();
	}

	private void CheckDisableInput()
	{
		if (EventSystem.current.currentSelectedGameObject != null)
		{
			TMP_InputField component = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
			if (component != null)
			{
				this.currentSelectedInput = component;
				InputController.instance.DisableAllInput = true;
				return;
			}
		}
		InputController.instance.DisableAllInput = false;
		this.currentSelectedInput = null;
	}

	public float DetermineTargetWorldSize(GameBoard board)
	{
		float num = Mathf.Max(0.15f, board.PackLineWidth - 8.86f);
		float num2 = 4.5f;
		num = Mathf.Max(num, board.BoardOptions.BaseBoardSize);
		if (board.Id == "cities")
		{
			num2 = 10f;
		}
		return Mathf.Clamp(Mathf.Clamp(num + (float)this.CardCapIncrease(board) * 0.05f, num, num2) + (float)this.BoardSizeIncrease(board) * 0.05f, num, num2 + 3f);
	}

	public Vector3 ScreenPosToWorldPos(Vector3 screenPos, out Ray ray)
	{
		ray = Camera.main.ScreenPointToRay(screenPos);
		Plane plane = new Plane(Vector3.up, Vector3.zero);
		float num;
		plane.Raycast(ray, out num);
		return ray.origin + ray.direction * num;
	}

	public void IncrementMonth()
	{
		this.BoardMonths.IncrementMonth();
		if (this.DayEvent != null)
		{
			this.DayEvent.TriggerDayEvent(this.CurrentMonth);
		}
	}

	private void Update()
	{
		if (Application.isEditor)
		{
			this.CheckDebugInput();
		}
		if (this.CurrentBoard != null && this.CurrentBoard.Id == "forest")
		{
			this.ForestMoonEnabled = true;
		}
		else
		{
			this.ForestMoonEnabled = false;
		}
		Shader.SetGlobalFloat("_GridWidth", this.GridWidth);
		Shader.SetGlobalFloat("_GridHeight", this.GridHeight);
		Shader.SetGlobalFloat("_AnimationTime", this.AnimationTime);
		if (this.CurrentBoard != null)
		{
			Shader.SetGlobalColor("_BoardBackgroundA", this.CurrentBoard.BoardOptions.CardBackgroundPallete.Color.linear);
			Shader.SetGlobalColor("_BoardBackgroundB", this.CurrentBoard.BoardOptions.CardBackgroundPallete.Color2.linear);
		}
		this.gridAlpha = Mathf.Lerp(this.gridAlpha, 0f, Time.deltaTime * 3f);
		Shader.SetGlobalFloat("_GridAlpha", this.gridAlpha);
		this.UpdatePhysics();
		this.CheckQueuedAnimations();
		this.CheckResetCanDropItem();
		this.clickStartedGrabbing = false;
		bool flag = true;
		if (this.IntroPack != null && !this.IntroPack.WasClicked)
		{
			flag = false;
		}
		if (this.currentAnimationRoutine != null || this.currentAnimation != null)
		{
			flag = false;
		}
		if (this.ForestMoonEnabled)
		{
			flag = false;
		}
		if (flag && !this.DebugEndlessMoonEnabled)
		{
			this.MonthTimer += Time.deltaTime * this.TimeScale;
		}
		this.AnimationTime += Time.deltaTime * this.TimeScale;
		if (!this.DebugEndlessMoonEnabled && this.MonthTimer >= this.MonthTime && this.currentAnimationRoutine == null)
		{
			this.MonthTimer -= this.MonthTime;
			this.IncrementMonth();
			this.EndOfMonth(null);
		}
		Ray ray;
		if (InputController.instance.CurrentSchemeIsController)
		{
			this.mouseWorldPosition = this.ScreenPosToWorldPos(new Vector2((float)Screen.width, (float)Screen.height) * 0.5f, out ray);
		}
		else if (InputController.instance.CurrentSchemeIsTouch)
		{
			this.mouseWorldPosition = this.ScreenPosToWorldPos(InputController.instance.GetSafeTouchPosition(0), out ray);
		}
		else
		{
			this.mouseWorldPosition = this.ScreenPosToWorldPos(InputController.instance.ClampedMousePosition(), out ray);
		}
		this.CheckDisableInput();
		this.HoveredDraggable = null;
		this.HoveredInteractable = null;
		this.CurrentHoverable = null;
		this.CanUseTransport = this.GetCardCount<RoadBuilder>() > 0;
		if (InputController.instance.ToggleViewTriggered() && !this.InAnimation && !TransitionScreen.InTransition && (this.CurrentGameState == WorldManager.GameState.Playing || this.CurrentGameState == WorldManager.GameState.Paused))
		{
			if (this.CurrentBoard.Id == "cities")
			{
				int num;
				if (this.CurrentView == ViewType.Calamity)
				{
					num = 1;
				}
				else
				{
					num = (int)(this.CurrentView + 1);
				}
				this.SetViewType((ViewType)num);
			}
			else if (this.CanUseTransport)
			{
				if (this.CurrentView == ViewType.Transport)
				{
					this.SetViewType(ViewType.Default);
				}
				else
				{
					this.SetViewType(ViewType.Transport);
				}
			}
		}
		if (this.GetCurrentBoardSafe().Id != "cities" && !this.CanUseTransport)
		{
			this.SetViewType(ViewType.Default);
		}
		if (InputController.instance.PauseTriggered())
		{
			this.TogglePause();
		}
		if (this.IsPlaying && InputController.instance.SnapCardsTriggered())
		{
			this.SnapCardsToGrid();
		}
		bool flag2 = InputController.instance.GetInput(0) && GameCanvas.instance.PositionIsOverUI(InputController.instance.GetInputPosition(0));
		bool flag3 = AccessibilityScreen.ClickToDragEnabled;
		if (InputController.instance.CurrentSchemeIsController || InputController.instance.CurrentSchemeIsTouch)
		{
			flag3 = false;
		}
		if (InputController.instance.CurrentScheme == ControlScheme.KeyboardMouse)
		{
			flag2 = GameCanvas.instance.MousePositionIsOverUI();
		}
		if (InputController.instance.CurrentSchemeIsController)
		{
			flag2 = false;
		}
		if (!flag2 && (InputController.instance.CurrentSchemeIsMouseKeyboard || InputController.instance.CurrentSchemeIsController))
		{
			int num2 = Physics.RaycastNonAlloc(ray, this.hits);
			float num3 = float.MaxValue;
			for (int i = 0; i < num2; i++)
			{
				RaycastHit raycastHit = this.hits[i];
				if (raycastHit.collider.gameObject.activeInHierarchy)
				{
					Draggable component = this.DraggableLookup.GetComponent(raycastHit.collider.gameObject);
					if (component != null)
					{
						float distance = raycastHit.distance;
						if (distance < num3)
						{
							num3 = distance;
							if (this.DraggingDraggable == null)
							{
								this.HoveredDraggable = component;
							}
						}
					}
					Interactable component2 = this.InteractableLookup.GetComponent(raycastHit.collider.gameObject);
					if (component2 != null)
					{
						this.HoveredInteractable = component2;
					}
					Hoverable component3 = this.HoverableLookup.GetComponent(raycastHit.collider.gameObject);
					if (component3 != null)
					{
						this.CurrentHoverable = component3;
					}
				}
			}
		}
		if (InputController.instance.CurrentSchemeIsTouch)
		{
			int num4 = Physics.RaycastNonAlloc(ray, this.hits);
			float num5 = float.MaxValue;
			for (int j = 0; j < num4; j++)
			{
				RaycastHit raycastHit2 = this.hits[j];
				if (raycastHit2.collider.gameObject.activeInHierarchy)
				{
					Draggable component4 = this.DraggableLookup.GetComponent(raycastHit2.collider.gameObject);
					if (component4 != null)
					{
						float distance2 = raycastHit2.distance;
						if (distance2 < num5)
						{
							num5 = distance2;
							if (this.DraggingDraggable == null)
							{
								this.HoveredDraggable = component4;
							}
						}
					}
					Interactable component5 = this.InteractableLookup.GetComponent(raycastHit2.collider.gameObject);
					if (component5 != null)
					{
						this.HoveredInteractable = component5;
					}
					Hoverable component6 = this.HoverableLookup.GetComponent(raycastHit2.collider.gameObject);
					if (component6 != null)
					{
						this.CurrentHoverable = component6;
					}
				}
			}
		}
		if (this.HoveredInteractable != null)
		{
			GameScreen.InfoBoxTitle = this.HoveredInteractable.name;
			GameScreen.InfoBoxText = this.HoveredInteractable.GetTooltipText();
		}
		if (this.CurrentHoverable != null)
		{
			GameScreen.InfoBoxTitle = this.CurrentHoverable.GetTitle();
			GameScreen.InfoBoxText = this.CurrentHoverable.GetDescription();
		}
		if (this.CanInteract || this.ConnectConnectors)
		{
			if (this.ConnectConnectors && this.HoveredDraggable != null && !(this.HoveredDraggable is CardConnector))
			{
				return;
			}
			bool flag4 = InputController.instance.StartedGrabbing();
			if ((InputController.instance.GetInputBegan(0) || flag4) && !flag2)
			{
				if (this.HoveredDraggable != null)
				{
					if (this.HoveredDraggable.CanBeDragged())
					{
						Draggable draggable = this.HoveredDraggable;
						draggable.DragTag = null;
						draggable.ClickedObject = null;
						GameCard gameCard = this.HoveredDraggable as GameCard;
						if (gameCard != null && InputController.instance.GetKey(Key.LeftShift))
						{
							this.IsShiftDragging = true;
							draggable = gameCard.GetRootCard();
							draggable.ClickedObject = gameCard;
						}
						else
						{
							this.IsShiftDragging = false;
						}
						this.DraggingDraggable = draggable;
						this.DraggingDraggable.DragStartPosition = this.DraggingDraggable.transform.position;
						this.grabOffset = this.mouseWorldPosition - this.DraggingDraggable.transform.position;
						this.DraggingDraggable.StartDragging();
						if (flag3)
						{
							this.clickStartedGrabbing = true;
						}
					}
					else
					{
						this.HoveredDraggable.Clicked();
						GameCard gameCard2 = this.HoveredDraggable as GameCard;
						if (gameCard2 != null)
						{
							gameCard2.RotWobble(1f);
						}
					}
				}
				else if (this.HoveredInteractable != null)
				{
					this.HoveredInteractable.Click();
				}
				else if ((InputController.instance.CurrentSchemeIsMouseKeyboard || InputController.instance.CurrentSchemeIsTouch) && !InputController.instance.GetRightMouseBegan())
				{
					GameCamera.instance.StartDragging();
				}
				else
				{
					GameCamera.instance.Clicked();
				}
			}
			if (InputController.instance.ToggleInventoryTriggered() && !flag2 && this.HoveredCard != null && this.HoveredCard.CardData.HasInventory)
			{
				this.HoveredCard.ShowInventory = !this.HoveredCard.ShowInventory;
			}
			if (InputController.instance.SellTriggered() && !flag2 && this.HoveredCard != null && this.CardCanBeSold(this.HoveredCard, true, false))
			{
				this.SellCard(this.HoveredCard.transform.position, this.HoveredCard.GetRootCard(), 1f, true);
			}
		}
		else if (this.CutsceneBoardView)
		{
			bool flag5 = InputController.instance.StartedGrabbing();
			if ((InputController.instance.GetInputBegan(0) || flag5) && !flag2 && this.HoveredDraggable == null)
			{
				if ((InputController.instance.CurrentSchemeIsMouseKeyboard || InputController.instance.CurrentSchemeIsTouch) && !InputController.instance.GetRightMouseBegan())
				{
					GameCamera.instance.StartDragging();
				}
				else
				{
					GameCamera.instance.Clicked();
				}
			}
		}
		else if ((InputController.instance.CurrentSchemeIsMouseKeyboard || InputController.instance.CurrentSchemeIsTouch) && InputController.instance.GetInputBegan(0) && !flag2 && !this.InAnimation)
		{
			GameCamera.instance.StartDragging();
		}
		bool flag6 = false;
		if (InputController.instance.CurrentSchemeIsController && AccessibilityScreen.AutoPauseWhenUsingController)
		{
			flag6 = true;
		}
		if (InputController.instance.CurrentScheme == ControlScheme.KeyboardMouse && AccessibilityScreen.AutoPauseWhenUsingKeyboardMouse)
		{
			flag6 = true;
		}
		if (flag6)
		{
			bool flag7 = this.DraggingDraggable != null;
			if (flag7 && !this.isAutoPaused)
			{
				this.isAutoPaused = true;
				this.preAutoPauseSpeed = this.SpeedUp;
				this.SpeedUp = 0f;
			}
			if (this.isAutoPaused && !flag7)
			{
				this.isAutoPaused = false;
				this.SpeedUp = this.preAutoPauseSpeed;
			}
		}
		this.NearbyCardTarget = null;
		float maxValue = float.MaxValue;
		if (this.DraggingDraggable != null)
		{
			this.DraggingDraggable.TargetPosition = this.mouseWorldPosition - this.grabOffset;
			if (this.DraggingCard != null)
			{
				this.DraggingCard.Clampieee();
			}
		}
		if (this.DraggingCard != null)
		{
			foreach (CardTarget cardTarget in this.CardTargets)
			{
				if (cardTarget.CanHaveCard(this.DraggingCard))
				{
					float num6 = Vector3.Distance(this.DraggingCard.TargetPosition + this.grabOffset, cardTarget.transform.position);
					num6 = Mathf.Min(Vector3.Distance(this.DraggingCard.TargetPosition, cardTarget.transform.position), num6);
					if (num6 < this.CardTargetSnapDistance && num6 < maxValue)
					{
						this.NearbyCardTarget = cardTarget;
						this.DraggingCard.TargetPosition = cardTarget.transform.position;
					}
				}
			}
		}
		if (!this.CanInteract && this.DraggingDraggable != null)
		{
			this.DraggingDraggable.StopDragging();
			this.SetDraggingDraggableToNull();
		}
		bool flag8 = InputController.instance.StoppedGrabbing();
		if (InputController.instance.GetInputEnded(0) || flag8 || !this.CanInteract)
		{
			if (flag3)
			{
				if (InputController.instance.GetLeftMouseEnded())
				{
					this.DropCard();
				}
			}
			else
			{
				this.DropCard();
			}
		}
		if (flag3 && InputController.instance.GetRightMouseBegan() && !this.clickStartedGrabbing)
		{
			this.DropCard();
		}
		if (this.CurrentGameState == WorldManager.GameState.Playing && this.currentAnimationRoutine == null && this.currentAnimation == null && this.IntroPack == null && !TransitionScreen.InTransition)
		{
			if (this.CheckAllVillagersDead())
			{
				if (this.CurrentBoard.Id == "main")
				{
					this.CurrentGameState = WorldManager.GameState.GameOver;
					GameCanvas.instance.SetScreen<GameOverScreen>();
				}
				else if (this.CurrentBoard.Id == "island")
				{
					this.QueueCutscene(Cutscenes.EveryoneOnIslandDead());
				}
				else if (this.CurrentBoard.BoardOptions.IsSpiritWorld)
				{
					this.QueueCutscene(Cutscenes.EveryoneInSpiritWorldDead(this.CurrentBoard.Id));
				}
				else if (!(this.CurrentBoard.Id == "forest") && !(this.CurrentBoard.Id == "cities"))
				{
					this.QueueCutscene(Cutscenes.EveryoneOnIslandDead());
				}
			}
			CitiesManager.instance.CheckCityHealth();
		}
		if (this.TimeScale > 0f)
		{
			this.CheckSpiritCutscenes();
		}
		this.DebugUpdate();
	}

	public void SetDraggingDraggableToNull()
	{
		this.DraggingDraggable = null;
		foreach (Draggable draggable in this.AllDraggables)
		{
			draggable.BeingDragged = false;
		}
	}

	public Sprite GetCurrencyIcon(BoardCurrency? currency)
	{
		if (currency != null)
		{
			BoardCurrency? boardCurrency = currency;
			BoardCurrency boardCurrency2 = BoardCurrency.Gold;
			if ((boardCurrency.GetValueOrDefault() == boardCurrency2) & (boardCurrency != null))
			{
				return SpriteManager.instance.CoinIcon;
			}
			boardCurrency = currency;
			boardCurrency2 = BoardCurrency.Shell;
			if ((boardCurrency.GetValueOrDefault() == boardCurrency2) & (boardCurrency != null))
			{
				return SpriteManager.instance.ShellIcon;
			}
			boardCurrency = currency;
			boardCurrency2 = BoardCurrency.Dollar;
			if ((boardCurrency.GetValueOrDefault() == boardCurrency2) & (boardCurrency != null))
			{
				return SpriteManager.instance.DollarIcon;
			}
		}
		return SpriteManager.instance.CoinIcon;
	}

	private void CheckResetCanDropItem()
	{
		if (this.CurrentBoard == null)
		{
			return;
		}
		if (this.CurrentBoard.BoardOptions.ResetItemDrops && !this.CurrentRunVariables.CanDropItem)
		{
			if (this.GetCardCount((CardData x) => x is Enemy) == 0)
			{
				this.CurrentRunVariables.CanDropItem = true;
			}
		}
	}

	private void DropCard()
	{
		if (this.DraggingCard != null)
		{
			if (this.NearbyCardTarget != null)
			{
				this.NearbyCardTarget.CardDropped(this.DraggingCard);
			}
			else
			{
				this.CheckIfCanAddOnStack(this.DraggingCard);
			}
		}
		if (this.DraggingDraggable != null)
		{
			this.DraggingDraggable.StopDragging();
			this.SetDraggingDraggableToNull();
		}
	}

	public void OnBoosterOpened(string boosterId)
	{
		if (this.CurrentBoard.Id == "greed" && boosterId == "greed_intro")
		{
			this.QueueCutscene(Cutscenes.GreedIntro());
			return;
		}
		if (this.CurrentBoard.Id == "happiness" && boosterId == "happiness_intro")
		{
			this.QueueCutscene(Cutscenes.HappinessIntro());
			return;
		}
		if (this.CurrentBoard.Id == "death" && boosterId == "death_intro")
		{
			this.QueueCutscene(Cutscenes.DeathIntro());
			return;
		}
		if (this.CurrentBoard.Id == "cities" && boosterId == "cities_intro")
		{
			this.QueueCutscene("cities_start");
		}
	}

	public void CloseOpenInventories()
	{
		foreach (GameCard gameCard in this.AllCards)
		{
			gameCard.ShowInventory = false;
		}
	}

	private void UpdatePhysics()
	{
		this.physicsTimer += Time.deltaTime * this.PhysicsTimeScale;
		while (this.physicsTimer >= 0.02f)
		{
			this.physicsTimer -= 0.02f;
			foreach (Draggable draggable in this.PhysicsDraggables)
			{
				draggable.UpdatePhysics(0.02f);
			}
		}
	}

	public void SnapCardsToGrid()
	{
		this.gridAlpha = 1f;
		foreach (GameCard gameCard in this.AllCards)
		{
			if (!gameCard.HasParent && !(gameCard.CardData is Mob))
			{
				Vector3 position = gameCard.transform.position;
				position.x = (float)Mathf.RoundToInt(position.x / this.GridWidth) * this.GridWidth;
				position.z = (float)Mathf.RoundToInt(position.z / this.GridHeight) * this.GridHeight;
				gameCard.TargetPosition = position;
			}
		}
	}

	public void DissolveStack(GameCard card)
	{
		GameCard gameCard = card.GetLeafCard();
		while (gameCard != null)
		{
			GameCard parent = gameCard.Parent;
			gameCard.RemoveFromStack();
			gameCard = parent;
		}
	}

	public bool CardCanBeSold(GameCard card, bool checkStatus = true, bool checkSpeedup = false)
	{
		if (checkSpeedup && this.SpeedUp == 0f)
		{
			return false;
		}
		if (card.IsEquipped || card.IsWorking)
		{
			return false;
		}
		if (card.MyBoard.Id == "forest")
		{
			return false;
		}
		if (card.WorkerChildren.Count > 0)
		{
			return false;
		}
		if (card.InConflict)
		{
			return false;
		}
		List<GameCard> allCardsInStack = card.GetAllCardsInStack();
		if (allCardsInStack.Any<GameCard>(delegate(GameCard x)
		{
			ResourceChest resourceChest = x.CardData as ResourceChest;
			return resourceChest != null && resourceChest.ResourceCount > 0;
		}))
		{
			return false;
		}
		if (allCardsInStack.Any<GameCard>(delegate(GameCard x)
		{
			Chest chest = x.CardData as Chest;
			return chest != null && chest.CoinCount > 0;
		}))
		{
			return false;
		}
		if (allCardsInStack.Any<GameCard>(delegate(GameCard x)
		{
			Creditcard creditcard = x.CardData as Creditcard;
			return creditcard != null && creditcard.DollarCount > 0;
		}))
		{
			return false;
		}
		if (allCardsInStack.Any<GameCard>(delegate(GameCard x)
		{
			FoodWarehouse foodWarehouse = x.CardData as FoodWarehouse;
			return foodWarehouse != null && foodWarehouse.FoodValue > 0;
		}))
		{
			return false;
		}
		if (checkStatus)
		{
			GameCard cardWithStatusInStack = card.GetCardWithStatusInStack();
			if (cardWithStatusInStack != null && !(cardWithStatusInStack.CardData is EnergyGenerator))
			{
				return false;
			}
		}
		return !allCardsInStack.Any<GameCard>((GameCard x) => x.CardData.GetValue() == -1);
	}

	public void ClearRoundAndRestart()
	{
		this.CurrentSave.LastPlayedRound = null;
		SaveManager.instance.Save(false);
		WorldManager.RestartGame();
	}

	public static void RestartGame()
	{
		SaveManager.instance.Load();
		WorldManager.instance.CurrentGameState = WorldManager.GameState.InMenu;
		WorldManager.instance.Time.SpeedUp = 1f;
		GameCamera.instance.PauseVolume.enabled = false;
		GameCamera.instance.PauseVolume.gameObject.SetActive(false);
		GameCanvas.instance.SetScreen<MainMenu>();
		GameCamera.instance.OnRestartGame();
		CardopediaScreen.instance.RefreshCardopedia();
		QuestManager.instance.UpdateCurrentQuests();
		foreach (BuyBoosterBox buyBoosterBox in WorldManager.instance.AllBoosterBoxes)
		{
			buyBoosterBox.UpdateUndiscoveredCards();
		}
		WorldManager.instance.ClearRound();
		Shader.SetGlobalFloat("_WorldSizeIncrease", 0f);
		Shader.SetGlobalFloat("_WorldSizeIncreaseNormalized", 0f);
		WorldManager.CheckForceReloadSave();
	}

	private static void CheckForceReloadSave()
	{
		if (SaveManager.IsForceReload)
		{
			SaveManager.IsForceReload = false;
			WorldManager.instance.LoadPreviousRound();
			WorldManager.instance.Play();
		}
	}

	public static void RebootGame()
	{
		Debug.Log("attempting to reboot game..");
		if (!PlatformHelper.UseSteam)
		{
			Application.quitting += WorldManager.RelaunchProcess;
			Application.Quit();
			return;
		}
		if (SteamManager.Initialized)
		{
			Application.quitting += WorldManager.RelaunchSteam;
			Application.Quit();
			return;
		}
		Debug.Log("cant figure out how to reboot game");
	}

	private static void RelaunchProcess()
	{
		string text;
		if (Application.platform == RuntimePlatform.OSXPlayer)
		{
			text = Path.Combine(Application.dataPath, "MacOS/Stacklands");
		}
		else
		{
			text = Path.Combine(Application.dataPath, "..", "Stacklands.exe");
		}
		Process.Start(text);
	}

	private static void RelaunchSteam()
	{
		Application.OpenURL("steam://rungameid/1948280");
	}

	public GameCard CreateCardStack(Vector3 pos, int amount, string cardId, bool checkAddToStack = true)
	{
		if (amount == 0)
		{
			return null;
		}
		GameCard gameCard = null;
		while (amount > 0)
		{
			int num = Mathf.Min(amount, 30);
			gameCard = null;
			for (int i = 0; i < num; i++)
			{
				GameCard myGameCard = this.CreateCard(pos, cardId, true, checkAddToStack, true).MyGameCard;
				if (gameCard != null)
				{
					myGameCard.SetParent(gameCard);
				}
				gameCard = myGameCard;
			}
			amount -= num;
		}
		return gameCard;
	}

	public void StartCursePlaythrough(CurseType curse, Action onArrive)
	{
		GameBoard gameBoard = null;
		if (curse == CurseType.Happiness)
		{
			gameBoard = this.GetBoardWithId("happiness");
		}
		else if (curse == CurseType.Death)
		{
			gameBoard = this.GetBoardWithId("death");
		}
		else if (curse == CurseType.Greed)
		{
			gameBoard = this.GetBoardWithId("greed");
		}
		GameCanvas.instance.SetScreen<EmptyScreen>();
		this.GoToBoard(gameBoard, delegate
		{
			GameCanvas.instance.SetScreen<GameScreen>();
			onArrive();
		}, "spirit");
	}

	public List<GameCard> CreateDollarsFromValue(int value, Vector3 pos, bool checkAddToStack = true)
	{
		if (value <= 0)
		{
			return new List<GameCard>();
		}
		int num = 0;
		int num2 = Math.DivRem(value, 100, out num);
		GameCard gameCard = this.CreateCardStack(pos, num2, "hundred_dollar", false);
		num2 = Math.DivRem(num, 50, out num);
		GameCard gameCard2 = this.CreateCardStack(pos, num2, "fifty_dollar", false);
		num2 = Math.DivRem(num, 20, out num);
		GameCard gameCard3 = this.CreateCardStack(pos, num2, "twenty_dollar", false);
		num2 = Math.DivRem(num, 10, out num);
		GameCard gameCard4 = this.CreateCardStack(pos, num2, "ten_dollar", false);
		List<GameCard> list = new List<GameCard>();
		if (gameCard != null)
		{
			list.AddRange(gameCard.GetAllCardsInStack());
		}
		if (gameCard2 != null)
		{
			list.AddRange(gameCard2.GetAllCardsInStack());
		}
		if (gameCard3 != null)
		{
			list.AddRange(gameCard3.GetAllCardsInStack());
		}
		if (gameCard4 != null)
		{
			list.AddRange(gameCard4.GetAllCardsInStack());
		}
		this.Restack(list);
		if (checkAddToStack)
		{
			this.CheckIfCanAddOnStack(list.First<GameCard>());
		}
		else
		{
			list.First<GameCard>().SendIt();
		}
		return list;
	}

	public GameCard SellCard(Vector3 pos, GameCard card, float multiplier = 1f, bool checkAddToStack = true)
	{
		if (card.CardData.Id == "coin_chest" || card.CardData.Id == "shell_chest")
		{
			multiplier = 1f;
		}
		CardValue stackValue = this.GetStackValue(card);
		bool flag = false;
		foreach (GameCard gameCard in card.GetAllCardsInStack())
		{
			if (gameCard.CardData != null)
			{
				gameCard.CardData.OnSellCard();
			}
			if (gameCard.CardData is Worker)
			{
				flag = true;
			}
			this.CreateSmoke(gameCard.transform.position);
		}
		this.DestroyStack(card);
		GameCard gameCard2 = null;
		if (flag)
		{
			QuestManager.instance.SpecialActionComplete("worker_removed", null);
		}
		if (this.CurrentBoard.Id == "cities")
		{
			if (stackValue.TotalValue >= 10)
			{
				List<GameCard> list = this.CreateDollarsFromValue(stackValue.TotalValue, pos, true);
				gameCard2 = ((list != null) ? list.FirstOrDefault<GameCard>() : null);
				if (list != null)
				{
					Dollar dollar = null;
					for (int i = 0; i < list.Count; i++)
					{
						Dollar dollar2 = list[i].CardData as Dollar;
						if (dollar == null || dollar2.DollarValue > dollar.DollarValue)
						{
							dollar = dollar2;
						}
					}
					if (dollar != null)
					{
						AudioManager.me.PlaySound2D(dollar.PickupSound, Random.Range(0.8f, 1.2f), 0.8f);
					}
				}
			}
		}
		else
		{
			string text = (this.CurrentBoard.BoardOptions.UsesShells ? "shell" : "gold");
			gameCard2 = this.CreateCardStack(pos, Mathf.CeilToInt(multiplier * (float)stackValue.TotalValue), text, checkAddToStack);
			if (gameCard2 != null)
			{
				QuestManager.instance.SpecialActionComplete("sell_card", gameCard2.CardData);
				AudioManager.me.PlaySound2D(AudioManager.me.Coin, Random.Range(0.8f, 1.2f), 0.8f);
			}
		}
		return gameCard2;
	}

	public void DestroyStack(GameCard card)
	{
		GameCard gameCard = card;
		while (gameCard != null)
		{
			gameCard.Destroyed = true;
			this.AllCards.Remove(card);
			this.UniqueIdToCard.Remove(card.CardData.UniqueId);
			Object.Destroy(gameCard.gameObject);
			gameCard = gameCard.Child;
		}
	}

	public Vector3 GetRandomSpawnPosition()
	{
		Bounds worldBounds = this.CurrentBoard.WorldBounds;
		float num = Mathf.Lerp(worldBounds.min.x, worldBounds.max.x, Random.Range(0.1f, 0.9f));
		float num2 = Mathf.Lerp(worldBounds.min.z, worldBounds.max.z, Random.Range(0.1f, 0.9f));
		return new Vector3(num, 0f, num2);
	}

	private void DebugUpdate()
	{
		bool flag = false;
		if (Application.isEditor && InputController.instance.GetKeyDown(Key.F1))
		{
			flag = true;
		}
		if (!Application.isEditor && InputController.instance.GetKeyDown(Key.F1) && InputController.instance.GetKey(Key.K) && InputController.instance.GetKey(Key.O))
		{
			flag = true;
		}
		if (flag)
		{
			GameScreen gameScreen = GameScreen.instance;
			if (gameScreen != null)
			{
				Image debugScreen = gameScreen.DebugScreen;
				if (debugScreen != null)
				{
					debugScreen.gameObject.SetActive(!GameScreen.instance.DebugScreen.gameObject.activeInHierarchy);
				}
			}
			this.DebugScreenOpened = true;
		}
		if (this.DebugScreenOpened && InputController.instance.GetKeyDown(Key.F5))
		{
			GameCanvas.instance.gameObject.SetActive(!GameCanvas.instance.gameObject.activeInHierarchy);
		}
	}

	public void CheckStackOrders()
	{
		if (this.validator == null)
		{
			this.validator = new GameDataValidator(this.GameDataLoader);
		}
		this.validator.CheckStackOrders();
	}

	public void SpawnAndDestroyEveryCard()
	{
		base.StartCoroutine(this.SpawnAndDestroyCards());
	}

	private IEnumerator SpawnAndDestroyCards()
	{
		foreach (CardData cardData in this.CardDataPrefabs)
		{
			CardData card = this.CreateCard(this.GetRandomSpawnPosition(), cardData, true, true, true, true);
			yield return null;
			card.MyGameCard.DestroyCard(false, true);
			card = null;
		}
		List<CardData>.Enumerator enumerator = default(List<CardData>.Enumerator);
		yield break;
		yield break;
	}

	public void GoToBoard(GameBoard newBoard, Action onComplete = null, string transitionId = "default")
	{
		if (newBoard.BoardOptions.IsSpiritWorld || this.CurrentBoard.BoardOptions.IsSpiritWorld)
		{
			AudioManager.me.PlaySound2D(AudioManager.me.SpiritTransitionEnter, 1f, 0.5f);
		}
		else if (newBoard.Id == "cities" || this.CurrentBoard.Id == "cities")
		{
			AudioManager.me.PlaySound2D(AudioManager.me.CitiesTransitionEnter, 1f, 0.5f);
		}
		TransitionScreen.instance.StartTransition(delegate
		{
			if (this.DraggingDraggable != null)
			{
				this.DraggingDraggable.StopDragging();
				this.SetDraggingDraggableToNull();
			}
			GameBoard currentBoard = this.CurrentBoard;
			this.CurrentRunVariables.PreviouseBoard = currentBoard.Id;
			this.CurrentBoard = newBoard;
			QuestManager.instance.SpecialActionComplete("board_" + this.CurrentBoard.Id, null);
			if (this.CurrentBoard.Id == "island")
			{
				if (!this.CurrentRunVariables.VisitedIsland)
				{
					this.QueueCutscene(Cutscenes.IslandIntro());
				}
				this.CurrentRunVariables.VisitedIsland = true;
				this.CheckSpawnIslandBooster();
			}
			if (this.CurrentBoard.Id == "cities" && !this.CurrentRunVariables.HasCitiesBoard)
			{
				this.CreateBoosterIfNotExists(this.CurrentBoard.MiddleOfBoard(), "cities_intro");
				this.CurrentRunVariables.HasCitiesBoard = true;
				CitiesManager.instance.Wellbeing = CitiesManager.instance.WellbeingStart;
			}
			if (this.CurrentBoard.Id == "greed")
			{
				this.CreateBoosterIfNotExists(this.CurrentBoard.MiddleOfBoard(), "greed_intro");
			}
			if (this.CurrentBoard.Id == "happiness")
			{
				this.CreateBoosterIfNotExists(this.CurrentBoard.MiddleOfBoard(), "happiness_intro");
			}
			if (this.CurrentBoard.Id == "death")
			{
				this.CreateBoosterIfNotExists(this.CurrentBoard.MiddleOfBoard(), "death_intro");
			}
			Action onComplete2 = onComplete;
			if (onComplete2 != null)
			{
				onComplete2();
			}
			if (newBoard.BoardOptions.IsSpiritWorld || this.CurrentBoard.BoardOptions.IsSpiritWorld)
			{
				AudioManager.me.PlaySound2D(AudioManager.me.SpiritTransitionExit, 1f, 0.5f);
			}
			else if (newBoard.Id == "cities" || this.CurrentBoard.Id == "cities")
			{
				AudioManager.me.PlaySound2D(AudioManager.me.CitiesTransitionExit, 1f, 0.5f);
			}
			GameScreen.instance.OnBoardChange();
			GameScreen.instance.UpdateQuestLog();
			GameScreen.instance.UpdateIdeasLog();
			GameScreen.instance.SetQuestTab();
			this.CurrentRunVariables.LastGoToBoardMonth = this.CurrentMonth;
			if (currentBoard.Id != "forest" && this.CurrentBoard.Id != "forest")
			{
				this.MonthTimer = 0f;
			}
			this.SpeedUp = 1f;
			if (currentBoard.Id == "cities" && !this.HasFoundCard("blueprint_road_builder"))
			{
				this.CreateCard(this.CurrentBoard.MiddleOfBoard(), "blueprint_road_builder", true, true, true);
			}
			GameCamera.instance.CenterOnBoard(this.CurrentBoard);
			QuestManager.instance.CheckPacksUnlocked();
			this.SetViewType(ViewType.Default);
			SaveManager.instance.Save(true);
		}, transitionId, 2f);
	}

	private void CreateBoosterIfNotExists(Vector3 pos, string boosterId)
	{
		if (this.AllBoosters.Count<Boosterpack>((Boosterpack x) => x.BoosterId == boosterId && x.MyBoard.IsCurrent) > 0)
		{
			return;
		}
		this.CreateBoosterpack(pos, boosterId);
	}

	private void CheckSpawnIslandBooster()
	{
		if (this.CurrentBoard.Id != "island")
		{
			return;
		}
		if (this.GetCardCount() == 0)
		{
			if (this.AllBoosters.Count<Boosterpack>((Boosterpack x) => x.MyBoard.Id == "island") == 0)
			{
				this.CreateBoosterpack(this.CurrentBoard.NormalizedPosToWorldPos(new Vector2(0.6f, 0.5f)), "island1");
			}
		}
	}

	public void SendStackToBoard(GameCard rootCard, GameBoard newBoard, Vector2 normalizedPos)
	{
		rootCard = rootCard.GetRootCard();
		this.SendToBoard(rootCard, newBoard, normalizedPos);
	}

	public void SendToBoard(GameCard rootCard, GameBoard newBoard, Vector2 normalizedPos)
	{
		rootCard.MyBoard = newBoard;
		foreach (GameCard gameCard in rootCard.GetChildCards())
		{
			gameCard.MyBoard = newBoard;
		}
		foreach (GameCard gameCard2 in rootCard.GetAllCardsInStack())
		{
			foreach (GameCard gameCard3 in gameCard2.EquipmentChildren)
			{
				gameCard3.MyBoard = newBoard;
			}
		}
		rootCard.transform.position = (rootCard.TargetPosition = newBoard.NormalizedPosToWorldPos(normalizedPos));
		rootCard.UpdateChildPositions(true);
	}

	public void Restack(List<GameCard> cards)
	{
		foreach (GameCard gameCard in cards)
		{
			gameCard.RemoveFromStack();
		}
		for (int i = 0; i < cards.Count; i++)
		{
			if (i > 0)
			{
				cards[i].SetParent(cards[i - 1]);
			}
		}
	}

	public bool CheckIfCanAddOnStack(GameCard topCard)
	{
		List<GameCard> overlappingCards = topCard.GetOverlappingCards();
		float num = float.MaxValue;
		GameCard gameCard = null;
		foreach (GameCard gameCard2 in overlappingCards)
		{
			if (!(gameCard2 == topCard) && !gameCard2.IsChildOf(topCard))
			{
				bool flag = topCard == gameCard2.removedChild;
				GameCard leafCard = gameCard2.GetLeafCard();
				if (!flag)
				{
					GameCard cardWithStatusInStack = leafCard.GetCardWithStatusInStack();
					if (cardWithStatusInStack != null && !cardWithStatusInStack.CardData.CanHaveCardsWhileHasStatus())
					{
						continue;
					}
				}
				if (leafCard.CardData.CanHaveCardOnTop(topCard.CardData, false))
				{
					Vector3 vector = topCard.transform.position - gameCard2.transform.position;
					vector.y = 0f;
					if (vector.magnitude < num)
					{
						gameCard = leafCard;
						num = vector.magnitude;
					}
				}
			}
		}
		if (gameCard != null)
		{
			topCard.SetParent(gameCard);
			return true;
		}
		return false;
	}

	public GameBoard GetCurrentBoardSafe()
	{
		if (this.CurrentBoard != null)
		{
			return this.CurrentBoard;
		}
		GameBoard gameBoard;
		if (this.IsCitiesDlcActive())
		{
			gameBoard = this.GetBoardWithId("cities");
		}
		else if (this.IsSpiritDlcActive())
		{
			gameBoard = this.GetBoardWithId("death");
		}
		else
		{
			gameBoard = this.GetBoardWithId("main");
		}
		return gameBoard;
	}

	public int GetCardCount(string id)
	{
		return this.GetCardCount(id, this.CurrentBoard);
	}

	public int GetCardCount(string id, GameBoard board)
	{
		int num = 0;
		for (int i = this.AllCards.Count - 1; i >= 0; i--)
		{
			GameCard gameCard = this.AllCards[i];
			if (!(gameCard.MyBoard != board) && gameCard.CardData.Id == id)
			{
				num++;
			}
		}
		return num;
	}

	public int GetCardCountWithChest(string id)
	{
		return this.GetCardCountWithChest(id, this.CurrentBoard);
	}

	public int GetCardCountWithChest(string id, GameBoard board)
	{
		int num = 0;
		foreach (GameCard gameCard in this.AllCards)
		{
			if (!(gameCard.MyBoard != board))
			{
				if (gameCard.CardData.Id == id)
				{
					num++;
				}
				ResourceChest resourceChest = gameCard.CardData as ResourceChest;
				if (resourceChest != null && resourceChest.HeldCardId == id)
				{
					num += resourceChest.ResourceCount;
				}
			}
		}
		return num;
	}

	public int GetCardCount(Predicate<CardData> pred)
	{
		int num = 0;
		foreach (GameCard gameCard in this.AllCards)
		{
			if (!(gameCard.MyBoard != this.CurrentBoard) && pred(gameCard.CardData))
			{
				num++;
			}
		}
		return num;
	}

	public int GetCardCountInStack(GameCard card, Predicate<CardData> pred)
	{
		int num = 0;
		foreach (GameCard gameCard in card.GetAllCardsInStack())
		{
			if (pred(gameCard.CardData))
			{
				num++;
			}
		}
		return num;
	}

	public CardData GetCardPrefab(string id, bool showError = true)
	{
		return this.GameDataLoader.GetCardFromId(id, showError);
	}

	public T GetCardPrefab<T>(string id, bool showError = true) where T : CardData
	{
		CardData cardFromId = this.GameDataLoader.GetCardFromId(id, showError);
		if (!(cardFromId is T))
		{
			Debug.LogError(string.Format("Card {0} is not of type {1}", id, typeof(T)));
			return default(T);
		}
		return cardFromId as T;
	}

	public BoosterpackData GetBoosterData(string boosterId)
	{
		return this.GameDataLoader.GetBoosterData(boosterId);
	}

	public Boosterpack CreateBoosterpack(Vector3 position, string boosterId)
	{
		Boosterpack boosterpack = Object.Instantiate<Boosterpack>(PrefabManager.instance.BoosterpackPrefab);
		BoosterpackData boosterpackData = Object.Instantiate<BoosterpackData>(this.GetBoosterData(boosterId));
		boosterpack.PackData = boosterpackData;
		boosterpack.transform.position = position;
		boosterpack.MyBoard = this.CurrentBoard;
		if (!this.CurrentSave.FoundBoosterIds.Contains(boosterId))
		{
			this.CurrentSave.FoundBoosterIds.Add(boosterId);
		}
		foreach (BoosterAddition boosterAddition in boosterpackData.BoosterAdditions)
		{
			if (boosterAddition.Filter.IsMet())
			{
				boosterpackData.CardBags.AddRange(boosterAddition.CardBags);
			}
		}
		boosterpack.TotalCardsInPack = boosterpackData.CardBags.Sum<CardBag>((CardBag x) => x.CardsInPack);
		return boosterpack;
	}

	public void StackSend(GameCard myCard, Vector3 outputDirection, GameCard initialParent = null, bool sendToChest = true)
	{
		if (this.TrySendToMagnet(myCard))
		{
			return;
		}
		if (sendToChest && this.TrySendToChest(myCard))
		{
			return;
		}
		if (myCard.BounceTarget != null)
		{
			return;
		}
		GameCard gameCard = null;
		float num = float.MaxValue;
		Vector3 vector = Vector3.zero;
		foreach (GameCard gameCard2 in this.AllCards)
		{
			if (gameCard2.MyBoard.IsCurrent && !(gameCard2 == myCard))
			{
				GameCard cardWithStatusInStack = gameCard2.GetCardWithStatusInStack();
				if ((!(cardWithStatusInStack != null) || cardWithStatusInStack.CardData.CanHaveCardsWhileHasStatus()) && !(gameCard2.GetCardInCombatInStack() != null) && !gameCard2.BeingDragged && !gameCard2.IsChildOf(myCard) && !gameCard2.IsParentOf(myCard) && (!(initialParent != null) || (!gameCard2.IsChildOf(initialParent) && !(gameCard2 == initialParent))) && !gameCard2.HasChild && gameCard2.CardData.CanHaveCardOnTop(myCard.CardData, false) && gameCard2.CardData.Id == myCard.CardData.Id)
				{
					Vector3 vector2 = gameCard2.transform.position - myCard.transform.position;
					vector2.y = 0f;
					if (vector2.magnitude <= 2f && vector2.magnitude <= num)
					{
						gameCard = gameCard2;
						num = vector2.magnitude;
						vector = new Vector3(vector2.x * 4f, 7f, vector2.z * 4f);
					}
				}
			}
		}
		if (gameCard != null)
		{
			myCard.BounceTarget = gameCard;
			myCard.Velocity = new Vector3?(vector);
			return;
		}
		myCard.SendIt();
	}

	public void StackSendCheckTarget(GameCard origin, GameCard myCard, Vector3 outputDirection, GameCard initialParent = null, bool sendToChest = true, int outputIndex = -1)
	{
		if (this.TrySendWithPipe(myCard, origin, outputIndex))
		{
			return;
		}
		if (this.TrySendToMagnet(myCard))
		{
			return;
		}
		if (myCard.BounceTarget != null)
		{
			return;
		}
		this.StackSend(myCard, outputDirection, initialParent, sendToChest);
	}

	public GameCard GetTargetCard(GameCard origin, CardData inputCardDataPrefab, Vector3 direction, bool allowDraggedCards, GameCard inputCard = null)
	{
		return this.GetBestCardInDirection(origin, direction, allowDraggedCards, (GameCard gameCard) => (!(inputCard != null) || !(gameCard == inputCard)) && !(gameCard == inputCardDataPrefab) && this.OutputCardAllowed(gameCard, inputCardDataPrefab) && (!(inputCardDataPrefab.MyGameCard != null) || !(inputCardDataPrefab.MyGameCard == gameCard)) && !gameCard.IsPartOfSameStack(origin));
	}

	public GameCard GetBestCardInDirection(GameCard origin, Vector3 direction, bool allowDraggedCards, Func<GameCard, bool> pred)
	{
		Vector3 position = origin.transform.position;
		float num = float.MinValue;
		GameCard gameCard = null;
		float num2 = float.MaxValue;
		foreach (GameCard gameCard2 in this.AllCards.Where<GameCard>((GameCard x) => x.MyBoard == this.CurrentBoard))
		{
			if (!(gameCard2 == origin) && (allowDraggedCards || !gameCard2.BeingDragged) && pred(gameCard2))
			{
				Vector3 vector = gameCard2.transform.position - position;
				float num3 = Vector3.Dot(direction, vector);
				if (num3 > 0f)
				{
					float num4 = num3 / vector.sqrMagnitude;
					if (num4 > 0.5f && num4 > num)
					{
						num = num4;
						Vector3 vector2 = gameCard2.transform.position - position;
						vector2.y = 0f;
						if (vector2.magnitude <= 2f && vector2.magnitude <= num2)
						{
							gameCard = gameCard2;
							num2 = vector2.magnitude;
						}
					}
				}
			}
		}
		return gameCard;
	}

	private bool OutputCardAllowed(GameCard gameCard, CardData inputCardDataPrefab)
	{
		if (gameCard.CardData.Id == "heavy_foundation")
		{
			return false;
		}
		if (gameCard.Velocity != null || gameCard.BounceTarget != null)
		{
			return false;
		}
		if (gameCard.HasChild)
		{
			return false;
		}
		if (!gameCard.gameObject.activeInHierarchy)
		{
			return false;
		}
		if (gameCard.MyBoard == null)
		{
			Debug.Log(((gameCard != null) ? gameCard.ToString() : null) + " does not have a board");
			return false;
		}
		if (!gameCard.MyBoard.IsCurrent)
		{
			return false;
		}
		try
		{
			if (!gameCard.CardData.CanHaveCardOnTop(inputCardDataPrefab, true))
			{
				return false;
			}
		}
		catch (Exception ex)
		{
			if (Application.isEditor)
			{
				Debug.LogError(ex);
			}
			return false;
		}
		return true;
	}

	public void StackSendTo(GameCard myCard, GameCard target)
	{
		Vector3 vector = target.transform.position - myCard.transform.position;
		vector.y = 0f;
		Vector3 vector2 = new Vector3(vector.x * 4f, 7f, vector.z * 4f);
		if (target.GetChildCount() > 0)
		{
			target = target.GetChildCards().Last<GameCard>();
		}
		if (target != null && target.CardData.CanHaveCardOnTop(myCard.CardData, false))
		{
			myCard.BounceTarget = target.GetRootCard();
			myCard.Velocity = new Vector3?(vector2);
			return;
		}
		myCard.SendIt();
	}

	public CardData CreateCard(Vector3 position, ICardId cardId, bool faceUp = true, bool checkAddToStack = true, bool playSound = true)
	{
		CardIdWithEquipment cardIdWithEquipment = cardId as CardIdWithEquipment;
		if (cardIdWithEquipment != null)
		{
			Combatable combatable = (Combatable)this.CreateCard(position, cardId.Id, faceUp, checkAddToStack, playSound);
			foreach (string text in cardIdWithEquipment.Equipment)
			{
				combatable.CreateAndEquipCard(text, false);
			}
			return combatable;
		}
		return this.CreateCard(position, cardId.Id, faceUp, checkAddToStack, playSound);
	}

	public CardData CreateCard(Vector3 position, string cardId, bool faceUp = true, bool checkAddToStack = true, bool playSound = true)
	{
		CardData cardPrefab = this.GetCardPrefab(cardId, true);
		Vector2 vector = Random.insideUnitCircle * 0.001f;
		position += new Vector3(vector.x, 0f, vector.y);
		return this.CreateCard(position, cardPrefab, faceUp, checkAddToStack, playSound, true);
	}

	private string GetUniqueId()
	{
		return Guid.NewGuid().ToString().Substring(0, 12);
	}

	public bool HasFoundCard(string cardId)
	{
		return this.CurrentSave.FoundCardIds.Contains(cardId);
	}

	public void FoundCard(CardData card)
	{
		if (this.CurrentSave.FoundCardIds.Contains(card.Id))
		{
			return;
		}
		if (card.MyCardType == CardType.Ideas || card.MyCardType == CardType.Rumors)
		{
			this.CurrentSave.NewKnowledgeIds.Add(card.Id);
		}
		this.CurrentSave.NewCardopediaIds.Add(card.Id);
		this.CurrentSave.FoundCardIds.Add(card.Id);
		this.NewCardsFound++;
		this.UpdateCardTargets();
		card.MyGameCard.IsNew = true;
	}

	public void DebugUnlockIdeas(bool justBasegame)
	{
		foreach (CardData cardData in this.CardDataPrefabs.Where<CardData>((CardData x) => x.MyCardType == CardType.Ideas && !x.HideFromCardopedia).ToList<CardData>())
		{
			if ((!justBasegame || cardData.CardUpdateType != CardUpdateType.Spirit) && !this.HasFoundCard(cardData.Id))
			{
				if (cardData.MyCardType == CardType.Ideas || cardData.MyCardType == CardType.Rumors)
				{
					this.CurrentSave.NewKnowledgeIds.Add(cardData.Id);
				}
				this.CurrentSave.NewCardopediaIds.Add(cardData.Id);
				this.CurrentSave.FoundCardIds.Add(cardData.Id);
				this.NewCardsFound++;
			}
		}
	}

	public CardData ChangeToCard(GameCard card, string cardId)
	{
		CardData cardPrefab = this.GetCardPrefab(cardId, true);
		CardData cardData = card.CardData;
		CardData cardData2 = Object.Instantiate<CardData>(cardPrefab);
		cardData2.StatusEffects = cardData.StatusEffects;
		foreach (StatusEffect statusEffect in cardData2.StatusEffects)
		{
			statusEffect.ParentCard = cardData2;
		}
		card.StatusEffectsChanged();
		cardData2.SetExtraCardData(card.CardData.GetExtraCardData());
		cardData2.UniqueId = card.CardData.UniqueId;
		cardData2.transform.SetParent(card.transform);
		cardData2.transform.localPosition = Vector3.zero;
		cardData2.MyGameCard = card;
		card.CardData = cardData2;
		if (cardData.IsFoil)
		{
			cardData2.SetFoil();
		}
		card.gameObject.name = cardPrefab.gameObject.name;
		if (!this.IsLoadingSaveRound)
		{
			QuestManager.instance.CardCreated(cardData2);
		}
		cardData2.MyGameCard.IsNew = false;
		this.FoundCard(cardData2);
		if (GameScreen.instance != null && (cardData2.MyCardType == CardType.Ideas || cardData2.MyCardType == CardType.Rumors))
		{
			GameScreen.instance.UpdateIdeasLog();
		}
		Combatable combatable = cardData as Combatable;
		if (combatable != null)
		{
			Combatable combatable2 = cardData2 as Combatable;
			if (combatable2 != null)
			{
				if (combatable.InConflict)
				{
					combatable.MyConflict.SwapParticipant(combatable, combatable2);
				}
				combatable2.HealthPoints = Mathf.Min(combatable2.HealthPoints, combatable2.ProcessedCombatStats.MaxHealth);
			}
		}
		Object.Destroy(cardData.gameObject);
		card.UpdateIcon();
		card.UpdateCardPalette();
		this.CreateSmoke(card.transform.position + Vector3.up * 0.05f);
		return cardData2;
	}

	public CardData CreateCard(Vector3 position, CardData cardDataPrefab, bool faceUp, bool checkAddToStack = true, bool playSound = true, bool markAsFound = true)
	{
		if (cardDataPrefab == null)
		{
			return null;
		}
		if (playSound)
		{
			AudioManager.me.PlaySound2D(AudioManager.me.CardCreate, 1f, 0.1f);
		}
		GameCard gameCard = Object.Instantiate<GameCard>(PrefabManager.instance.GameCardPrefab);
		gameCard.transform.position = position;
		gameCard.MyBoard = this.CurrentBoard;
		CardData cardData = Object.Instantiate<CardData>(cardDataPrefab);
		cardData.transform.SetParent(gameCard.transform);
		cardData.transform.localPosition = Vector3.zero;
		cardData.CreationMonth = this.CurrentMonth;
		cardData.UniqueId = this.GetUniqueId();
		gameCard.CardData = cardData;
		cardData.MyGameCard = gameCard;
		gameCard.gameObject.name = cardDataPrefab.gameObject.name;
		gameCard.SetFaceUp(faceUp);
		if (checkAddToStack)
		{
			this.CheckIfCanAddOnStack(gameCard);
		}
		gameCard.transform.position = position;
		this.AllCards.Add(gameCard);
		if (!this.IsLoadingSaveRound)
		{
			this.UniqueIdToCard[cardData.UniqueId] = gameCard;
		}
		Curse curse = gameCard.CardData as Curse;
		if (curse != null)
		{
			this.ActiveCurses.Add(curse);
		}
		if (!this.IsLoadingSaveRound)
		{
			QuestManager.instance.CardCreated(cardData);
		}
		if (markAsFound)
		{
			this.FoundCard(cardData);
		}
		if (GameScreen.instance != null && (cardData.MyCardType == CardType.Ideas || cardData.MyCardType == CardType.Rumors))
		{
			GameScreen.instance.UpdateIdeasLog();
		}
		if (cardData.WorkerAmount > 0)
		{
			gameCard.WorkerTransformHolder.UpdateWorkerAmount(cardData.WorkerAmount);
		}
		if (cardData.EnergyConnectors.Count > 0)
		{
			gameCard.CreateCardConnectors();
		}
		if (!this.IsLoadingSaveRound)
		{
			this.TrySendToMagnet(gameCard);
			cardData.OnInitialCreate();
		}
		cardData.gameObject.SetActive(true);
		return cardData;
	}

	public CardData GetNearestCardMatchingPred(GameCard card, Predicate<GameCard> pred)
	{
		CardData cardData = null;
		float num = float.MaxValue;
		foreach (GameCard gameCard in from x in this.AllCards.FindAll(pred)
			where WorldManager.instance.CurrentBoard.Id == x.MyBoard.Id
			select x)
		{
			Vector3 vector = gameCard.transform.position - card.transform.position;
			vector.y = 0f;
			if (vector.sqrMagnitude < num)
			{
				num = vector.sqrMagnitude;
				cardData = gameCard.CardData;
			}
		}
		return cardData;
	}

	public bool TrySendWithPipe(GameCard card, GameCard origin, int outputIndex = -1)
	{
		if (origin.CardConnectorChildren.Any<CardConnector>((CardConnector x) => x.ConnectionType == ConnectionType.Transport && x.CardDirection == CardDirection.output && x.ConnectedNode != null))
		{
			List<CardConnector> list = origin.CardConnectorChildren.FindAll((CardConnector x) => x.ConnectionType == ConnectionType.Transport && x.CardDirection == CardDirection.output && x.ConnectedNode != null);
			CardConnector cardConnector = null;
			if (outputIndex >= 0 && outputIndex < list.Count)
			{
				cardConnector = list[outputIndex];
			}
			if (cardConnector == null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					int num = (origin.ConnectorOutputIndex + i) % list.Count;
					cardConnector = list[num];
					if (cardConnector != null && cardConnector.ConnectedNode != null)
					{
						break;
					}
				}
			}
			if (cardConnector != null)
			{
				origin.ConnectorOutputIndex = list.IndexOf(cardConnector) + 1;
				GameCard parent = cardConnector.ConnectedNode.Parent;
				this.StackSendTo(card, parent.GetLeafCard());
				return true;
			}
		}
		return false;
	}

	public bool TrySendToMagnet(GameCard card)
	{
		CardData nearestCardMatchingPred = this.GetNearestCardMatchingPred(card, delegate(GameCard x)
		{
			ResourceMagnet resourceMagnet = x.CardData as ResourceMagnet;
			return resourceMagnet != null && resourceMagnet.PullCardId == card.CardData.Id && x.MyBoard == card.MyBoard && resourceMagnet.MyGameCard.GetStackCount() < 30;
		});
		if (nearestCardMatchingPred == null)
		{
			return false;
		}
		this.StackSendTo(card, nearestCardMatchingPred.MyGameCard.GetLeafCard());
		QuestManager.instance.SpecialActionComplete("use_magnet", null);
		return true;
	}

	public bool TrySendToChest(GameCard card)
	{
		GameCard gameCard = null;
		float num = float.MaxValue;
		List<GameCard> list = new List<GameCard>();
		if (card.CardData.Id == "gold" || card.CardData.Id == "shell")
		{
			list = this.AllCards.FindAll(delegate(GameCard x)
			{
				Chest chest = x.CardData as Chest;
				return chest != null && chest.HeldCardId == card.CardData.Id && chest.CoinCount < chest.MaxCoinCount;
			});
		}
		else
		{
			list = this.AllCards.FindAll(delegate(GameCard x)
			{
				ResourceChest resourceChest = x.CardData as ResourceChest;
				return resourceChest != null && resourceChest.HeldCardId == card.CardData.Id && resourceChest.ResourceCount < resourceChest.MaxResourceCount;
			});
		}
		foreach (GameCard gameCard2 in list)
		{
			Vector3 vector = gameCard2.transform.position - card.transform.position;
			vector.y = 0f;
			if (vector.magnitude <= 2f && vector.magnitude <= num)
			{
				gameCard = gameCard2;
				num = vector.magnitude;
			}
		}
		if (gameCard != null)
		{
			this.StackSendTo(card, gameCard.GetLeafCard());
			return true;
		}
		return false;
	}

	public CardValue GetStackValue(GameCard card)
	{
		CardValue cardValue = new CardValue(card.CardData.GetValue());
		if (card.IsPartOfStack())
		{
			foreach (GameCard gameCard in card.GetAllCardsInStack())
			{
				if (gameCard != card)
				{
					cardValue.BaseValue += gameCard.CardData.GetValue();
				}
			}
		}
		cardValue.BaseValue = Mathf.Max(0, cardValue.BaseValue);
		return cardValue;
	}

	public bool StackAllSame(GameCard card)
	{
		List<GameCard> allCardsInStack = card.GetAllCardsInStack();
		return this.AllCardsSame(allCardsInStack);
	}

	public bool AllCardsSame(List<GameCard> cards)
	{
		return cards.Select<GameCard, string>((GameCard x) => x.CardData.Id).Distinct<string>().Count<string>() == 1;
	}

	public bool AllCardsInStackMatchPred(GameCard card, Predicate<GameCard> pred)
	{
		return card.GetAllCardsInStack().All<GameCard>((GameCard x) => pred(x));
	}

	private bool CountsTowardCardCount(GameCard card)
	{
		CardData cardData = card.CardData;
		return (!(cardData is Poop) || !this.CurseIsActive(CurseType.Death)) && !this.doesntCountTowardsCount.Contains(cardData.Id) && (!(cardData is Dollar) && !(cardData is Energy) && cardData.MyCardType != CardType.Weather) && !(cardData is Worker);
	}

	public int GetCardCount()
	{
		int num = 0;
		bool canTravelToIsland = this.CurrentBoard.BoardOptions.CanTravelToIsland;
		for (int i = 0; i < this.AllCards.Count; i++)
		{
			GameCard gameCard = this.AllCards[i];
			if (!(gameCard.MyBoard != this.CurrentBoard) && !gameCard.IsEquipped && this.CountsTowardCardCount(gameCard))
			{
				Food food = gameCard.CardData as Food;
				if (food == null || !food.IsConsumed)
				{
					GameCard rootCard = gameCard.GetRootCard();
					if (canTravelToIsland)
					{
						if (rootCard.CardData.AnyChildMatchesPredicate(delegate(CardData x)
						{
							Boat boat2 = x as Boat;
							return boat2 != null && boat2.InSailOff;
						}))
						{
							goto IL_00FD;
						}
					}
					Boat boat = rootCard.CardData as Boat;
					if (boat == null || !boat.InSailOff)
					{
						ResourceChest resourceChest = gameCard.CardData as ResourceChest;
						if (resourceChest != null)
						{
							num += ((this.CurrentBoard.Id == "cities") ? 1 : resourceChest.ResourceCount);
						}
						num++;
					}
				}
			}
			IL_00FD:;
		}
		return num;
	}

	public int GetMaxCardCount()
	{
		return this.GetMaxCardCount(this.CurrentBoard);
	}

	public int GetMaxCardCount(GameBoard board)
	{
		return board.BoardOptions.BaseCardCount + this.CardCapIncrease(board);
	}

	public int CardCapIncrease(GameBoard board)
	{
		if (board.Id == "cities")
		{
			return this.GetTownHallIncrease(board);
		}
		int num = 0;
		for (int i = this.AllCards.Count - 1; i >= 0; i--)
		{
			GameCard gameCard = this.AllCards[i];
			if (!(gameCard.MyBoard != board))
			{
				if (gameCard.CardData.Id == "shed")
				{
					num += 4;
				}
				else if (gameCard.CardData.Id == "warehouse")
				{
					num += 14;
				}
				else if (gameCard.CardData.Id == "lighthouse")
				{
					num += 14;
				}
			}
		}
		return num;
	}

	public int GetTownHallIncrease(GameBoard board)
	{
		this.GetCardsNonAlloc<CityHall>(board, this.townhalls);
		if (this.townhalls.Count <= 0)
		{
			return 0;
		}
		int num = 0;
		foreach (CityHall cityHall in this.townhalls)
		{
			num += cityHall.DollarAmount;
		}
		return Mathf.FloorToInt((float)(num / CityHall.DollarPerCardcap));
	}

	public int BoardSizeIncrease(GameBoard board)
	{
		return this.GetCardCount("lighthouse", board) * 10;
	}

	public int GetFoodCount(bool allowDebug = true)
	{
		if (this.DebugNoFoodEnabled && allowDebug)
		{
			return 99;
		}
		int num = 0;
		foreach (GameCard gameCard in this.AllCards)
		{
			if (gameCard.MyBoard.IsCurrent)
			{
				Food food = gameCard.CardData as Food;
				if (food != null)
				{
					num += food.FoodValue;
				}
			}
		}
		return num;
	}

	public int GetHappinessCount(bool allowDebug = true, bool includeInChest = true)
	{
		if (this.DebugNoFoodEnabled && allowDebug)
		{
			return 99;
		}
		int num = 0;
		if (includeInChest)
		{
			num = this.GetCountInChests("happiness");
		}
		return this.GetCardCount("happiness") + num;
	}

	private int GetCountInChests(string cardId)
	{
		int num = 0;
		foreach (GameCard gameCard in this.AllCards)
		{
			if (gameCard.MyBoard.IsCurrent)
			{
				ResourceChest resourceChest = gameCard.CardData as ResourceChest;
				if (resourceChest != null && resourceChest.HeldCardId == cardId)
				{
					num += resourceChest.ResourceCount;
				}
				Chest chest = gameCard.CardData as Chest;
				if (chest != null && chest.HeldCardId == cardId)
				{
					num += chest.CoinCount;
				}
			}
		}
		return num;
	}

	public int GetShellCount(bool includeInChest)
	{
		int num = 0;
		if (includeInChest)
		{
			num = this.GetCountInChests("shell");
		}
		return this.GetCardCount<Shell>() + num;
	}

	public int GetGoldCount(bool includeInChest)
	{
		int num = 0;
		if (includeInChest)
		{
			num = this.GetCountInChests("gold");
		}
		return this.GetCardCount<Gold>() + num;
	}

	public int GetDollarCount(bool includeInChest)
	{
		int num = 0;
		if (includeInChest)
		{
			num = this.GetDollarInBank();
		}
		this.GetCardsNonAlloc<Dollar>(this.dollars);
		int num2 = 0;
		for (int i = 0; i < this.dollars.Count; i++)
		{
			num2 += this.dollars[i].DollarValue;
		}
		return num2 + num;
	}

	public int GetDollarInBank()
	{
		this.GetCardsNonAlloc<Creditcard>(this.creditcards);
		int num = 0;
		for (int i = 0; i < this.creditcards.Count; i++)
		{
			num += this.creditcards[i].DollarCount;
		}
		return num;
	}

	public int GetIdeaCount()
	{
		int num = 0;
		using (List<string>.Enumerator enumerator = this.CurrentSave.FoundCardIds.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.StartsWith("blueprint"))
				{
					num++;
				}
			}
		}
		return num;
	}

	public int GetCardCount<T>(Predicate<T> pred) where T : CardData
	{
		int num = 0;
		GameBoard currentBoard = this.CurrentBoard;
		for (int i = this.AllCards.Count - 1; i >= 0; i--)
		{
			GameCard gameCard = this.AllCards[i];
			if (!(gameCard.MyBoard != currentBoard))
			{
				T t = gameCard.CardData as T;
				if (t != null && (pred == null || pred(t)))
				{
					num++;
				}
			}
		}
		return num;
	}

	public int GetCardCount<T>() where T : CardData
	{
		return this.GetCardCount<T>(null);
	}

	public T GetCard<T>() where T : CardData
	{
		for (int i = 0; i < this.AllCards.Count; i++)
		{
			GameCard gameCard = this.AllCards[i];
			if (gameCard.MyBoard.IsCurrent && gameCard.CardData is T)
			{
				return (T)((object)gameCard.CardData);
			}
		}
		return default(T);
	}

	public T GetCard<T>(GameBoard board) where T : CardData
	{
		foreach (GameCard gameCard in this.AllCards)
		{
			if (!(gameCard.MyBoard.Id != board.Id) && gameCard.CardData is T)
			{
				return (T)((object)gameCard.CardData);
			}
		}
		return default(T);
	}

	public CardData GetCard(string cardId)
	{
		foreach (GameCard gameCard in this.AllCards)
		{
			if (gameCard.MyBoard.IsCurrent && gameCard.CardData.Id == cardId)
			{
				return gameCard.CardData;
			}
		}
		return null;
	}

	public List<GameCard> GetAllCardsOnBoard(string board)
	{
		return this.AllCards.Where<GameCard>((GameCard card) => card.MyBoard.Id == board).ToList<GameCard>();
	}

	public List<CardData> GetCards(string cardId)
	{
		List<CardData> list = new List<CardData>();
		foreach (GameCard gameCard in this.AllCards)
		{
			if (gameCard.MyBoard.IsCurrent && gameCard.CardData.Id == cardId)
			{
				list.Add(gameCard.CardData);
			}
		}
		return list;
	}

	public List<T> GetCardsImplementingInterface<T>()
	{
		if (!typeof(T).IsInterface)
		{
			throw new ArgumentException();
		}
		List<T> list = new List<T>();
		GameBoard currentBoard = this.CurrentBoard;
		foreach (GameCard gameCard in this.AllCards)
		{
			if (!(gameCard.MyBoard != currentBoard))
			{
				CardData cardData = gameCard.CardData;
				if (cardData is T)
				{
					T t = cardData as T;
					list.Add(t);
				}
			}
		}
		return list;
	}

	public List<T> GetCardsImplementingInterfaceNonAlloc<T>(List<T> list)
	{
		if (!typeof(T).IsInterface)
		{
			throw new ArgumentException();
		}
		list.Clear();
		GameBoard currentBoard = this.CurrentBoard;
		for (int i = this.AllCards.Count - 1; i >= 0; i--)
		{
			GameCard gameCard = this.AllCards[i];
			if (!(gameCard.MyBoard != currentBoard))
			{
				CardData cardData = gameCard.CardData;
				if (cardData is T)
				{
					T t = cardData as T;
					list.Add(t);
				}
			}
		}
		return list;
	}

	public List<T> GetCards<T>() where T : CardData
	{
		List<T> list = new List<T>();
		for (int i = 0; i < this.AllCards.Count; i++)
		{
			GameCard gameCard = this.AllCards[i];
			if (gameCard.MyBoard.IsCurrent)
			{
				T t = gameCard.CardData as T;
				if (t != null)
				{
					list.Add(t);
				}
			}
		}
		return list;
	}

	public void GetCardsNonAlloc<T>(List<T> list) where T : CardData
	{
		list.Clear();
		GameBoard currentBoard = this.CurrentBoard;
		foreach (GameCard gameCard in this.AllCards)
		{
			if (!(gameCard.MyBoard != currentBoard))
			{
				T t = gameCard.CardData as T;
				if (t != null)
				{
					list.Add(t);
				}
			}
		}
	}

	public List<T> GetCards<T>(GameBoard board) where T : CardData
	{
		List<T> list = new List<T>();
		foreach (GameCard gameCard in this.AllCards)
		{
			if (!(gameCard.MyBoard.Id != board.Id))
			{
				T t = gameCard.CardData as T;
				if (t != null)
				{
					list.Add(t);
				}
			}
		}
		return list;
	}

	public void GetCardsNonAlloc<T>(GameBoard board, List<T> list) where T : CardData
	{
		list.Clear();
		foreach (GameCard gameCard in this.AllCards)
		{
			if (!(gameCard.MyBoard.Id != board.Id))
			{
				T t = gameCard.CardData as T;
				if (t != null)
				{
					list.Add(t);
				}
			}
		}
	}

	public List<Boosterpack> GetAllBoostersOnBoard(string board)
	{
		return this.AllBoosters.Where<Boosterpack>((Boosterpack booster) => booster.MyBoard.Id == board).ToList<Boosterpack>();
	}

	public int GetRequiredFoodCount()
	{
		if (this.DebugNoFoodEnabled)
		{
			return 0;
		}
		int num = 0;
		foreach (GameCard gameCard in this.AllCards)
		{
			if (gameCard.MyBoard.IsCurrent)
			{
				num += this.GetCardRequiredFoodCount(gameCard);
			}
		}
		return num;
	}

	public int GetCardRequiredFoodCount(GameCard c)
	{
		BaseVillager baseVillager = c.CardData as BaseVillager;
		if (baseVillager != null)
		{
			return baseVillager.GetRequiredFoodCount();
		}
		if (c.CardData is Kid)
		{
			if (!(this.CurrentBoard.Id == "cities"))
			{
				return 1;
			}
			return 0;
		}
		else
		{
			Apartment apartment = c.CardData as Apartment;
			if (apartment != null)
			{
				return apartment.RequirementHolders.Sum<RequirementHolder>((RequirementHolder x) => x.CardRequirements.Sum<CardRequirement>(delegate(CardRequirement x)
				{
					CardRequirement_TakeFood cardRequirement_TakeFood = x as CardRequirement_TakeFood;
					if (cardRequirement_TakeFood != null)
					{
						return cardRequirement_TakeFood.Amount;
					}
					return 0;
				}));
			}
			return 0;
		}
	}

	public int GetRequiredHappinessCount()
	{
		if (this.DebugNoFoodEnabled)
		{
			return 0;
		}
		int num = 0;
		foreach (GameCard gameCard in this.AllCards)
		{
			if (gameCard.MyBoard.IsCurrent)
			{
				num += this.GetCardRequiredHappinessCount(gameCard);
			}
		}
		return num;
	}

	public int GetCardRequiredHappinessCount(GameCard c)
	{
		if (c.CardData is BaseVillager)
		{
			return 1;
		}
		if (c.CardData is Kid)
		{
			return 1;
		}
		if (c.CardData is Unhappiness)
		{
			return 1;
		}
		ResourceChest resourceChest = c.CardData as ResourceChest;
		if (resourceChest != null && resourceChest.HeldCardId == "unhappiness")
		{
			return resourceChest.ResourceCount;
		}
		return 0;
	}

	public Blueprint GetBlueprintWithId(string id)
	{
		foreach (Blueprint blueprint in this.BlueprintPrefabs)
		{
			if (blueprint.Id == id)
			{
				return blueprint;
			}
		}
		return null;
	}

	public string GetStackSummary(List<GameCard> cards)
	{
		List<string> list = cards.Select<GameCard, string>((GameCard x) => x.CardData.FullName).Distinct<string>().ToList<string>();
		string text = "";
		for (int i = 0; i < list.Count; i++)
		{
			string card = list[i];
			int num = cards.Count<GameCard>((GameCard x) => x.CardData.FullName == card);
			text += string.Format("{0}x {1}", num, card);
			if (i < list.Count - 1)
			{
				text += ", ";
			}
		}
		return text;
	}

	private void EndOfMonth(EndOfMonthParameters param = null)
	{
		if (param == null)
		{
			param = new EndOfMonthParameters();
		}
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		this.CloseOpenInventories();
		if (this.CurrentBoard.Id == "cities")
		{
			this.currentAnimationRoutine = base.StartCoroutine(this.EndOfMonthCitiesRoutine(param));
		}
		else
		{
			this.currentAnimationRoutine = base.StartCoroutine(this.EndOfMonthRoutine(param));
		}
		if (GameScreen.instance.ControllerIsInUI)
		{
			GameScreen.instance.SetControllerInUI(false);
		}
		this.SpeedUp = 1f;
		QuestManager.instance.SpecialActionComplete("month_end", null);
	}

	public IEnumerator FinishDemand(Demand demand, DemandEvent demandEvent)
	{
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		this.CloseOpenInventories();
		yield return this.FinishDemandRoutine(demand, demandEvent);
		if (GameScreen.instance.ControllerIsInUI)
		{
			GameScreen.instance.SetControllerInUI(false);
		}
		this.SpeedUp = 1f;
		QuestManager.instance.SpecialActionComplete("first_demand", null);
		yield break;
	}

	public void ForceEndOfMoon(EndOfMonthParameters param)
	{
		this.MonthTimer = 0f;
		this.IncrementMonth();
		this.EndOfMonth(param);
	}

	private IEnumerator WaitForContinueClicked(string text)
	{
		this.ContinueClicked = false;
		this.ContinueButtonText = text;
		this.ShowContinueButton = true;
		while (!this.ContinueClicked)
		{
			yield return null;
		}
		this.ShowContinueButton = false;
		yield break;
	}

	private IEnumerator EndOfMonthRoutine(EndOfMonthParameters param)
	{
		if (this.CurrentView != ViewType.Default)
		{
			this.SetViewType(ViewType.Default);
		}
		foreach (TravellingCart travellingCart in this.GetCards<TravellingCart>())
		{
			travellingCart.MyGameCard.DestroyCard(true, true);
		}
		foreach (CardData cardData in this.GetCards("trained_monkey"))
		{
			this.ChangeToCard(cardData.MyGameCard, "monkey").UpdateCardText();
		}
		AudioManager.me.PlaySound2D(AudioManager.me.EndOfMoon, 0.8f, 0.2f);
		if (param.CutsceneTitle == null)
		{
			this.CutsceneTitle = SokLoc.Translate("label_end_of_moon", new LocParam[] { LocParam.Create("moon", (this.CurrentMonth - 1).ToString()) });
		}
		else
		{
			this.CutsceneTitle = param.CutsceneTitle;
		}
		if (!this.DebugNoFoodEnabled)
		{
			this.VillagersStarvedAtEndOfMoon = false;
			yield return EndOfMonthCutscenes.FeedVillagers();
			if (this.VillagersStarvedAtEndOfMoon)
			{
				yield break;
			}
			if (this.CurseIsActive(CurseType.Happiness))
			{
				this.VillagersAngryAtEndOfMoon = false;
				yield return EndOfMonthCutscenes.UseHappiness();
				if (this.VillagersAngryAtEndOfMoon)
				{
					yield break;
				}
			}
			else
			{
				this.CurrentRunVariables.VillagersUnhappyMonthCount = 0;
			}
		}
		if (this.CurseIsActive(CurseType.Death))
		{
			CardData fountain = this.GetCard("fountain_of_youth");
			if (fountain != null)
			{
				this.CutsceneTitle = SokLoc.Translate("label_fountain_title");
				this.CutsceneText = SokLoc.Translate("label_fountain_text");
				yield return new WaitForSeconds(1f);
				GameCamera.instance.TargetPositionOverride = new Vector3?(fountain.MyGameCard.transform.position);
				yield return new WaitForSeconds(2f);
			}
			else
			{
				List<BaseVillager> villagersToAge = EndOfMonthCutscenes.GetVillagersToAge();
				if (villagersToAge.Any<BaseVillager>((BaseVillager x) => x.DetermineLifeStageFromAge(x.Age) == LifeStage.Elderly))
				{
					QuestManager.instance.SpecialActionComplete("villager_old", null);
				}
				if (EndOfMonthCutscenes.AnyVillagerWillChangeLifeStage(villagersToAge))
				{
					yield return EndOfMonthCutscenes.AgeVillagers(villagersToAge);
				}
				else
				{
					foreach (BaseVillager baseVillager in villagersToAge)
					{
						baseVillager.Age++;
					}
				}
				if (!this.VillagersStarvedAtEndOfMoon)
				{
					bool flag = false;
					if (this.CurrentBoard.Id == "death" && this.BoardMonths.DeathMonth >= 6)
					{
						flag = true;
					}
					else if ((this.CurrentBoard.Id == "main" || this.CurrentBoard.Id == "island") && this.CurrentMonth > 6)
					{
						flag = true;
					}
					if (flag)
					{
						yield return EndOfMonthCutscenes.CheckMakeSick();
					}
					if (this.BoardMonths.DeathMonth == 4 && this.CurrentBoard.Id == "death")
					{
						yield return EndOfMonthCutscenes.NewVillagerSpawnsInDeath();
					}
				}
			}
			if (!this.VillagersStarvedAtEndOfMoon)
			{
				List<Animal> cards = this.GetCards<Animal>();
				foreach (Animal animal in cards)
				{
					if (this.CurrentMonth - animal.CreationMonth >= 3)
					{
						animal.IsOld = true;
					}
					else
					{
						animal.IsOld = false;
					}
				}
				if (EndOfMonthCutscenes.AnyAnimalWillDie(cards))
				{
					yield return EndOfMonthCutscenes.KillAnimals((from x in this.GetCards<Animal>()
						where this.CurrentMonth - x.CreationMonth >= 5
						select x).ToList<Animal>());
				}
			}
			fountain = null;
		}
		yield return new WaitForSeconds(1.5f);
		yield return EndOfMonthCutscenes.MaxCardCount();
		if (this.CurseIsActive(CurseType.Greed))
		{
			yield return DemandManager.instance.CheckDemands(this.CurrentMonth);
		}
		if (this.IsCitiesDlcActive() && this.CurrentBoard.Id == "main")
		{
			if (this.GetCards<Food>().Sum<Food>((Food x) => x.FoodValue) >= 25 && this.GetCard("event_industrial_revolution") == null)
			{
				yield return EndOfMonthCutscenes.IndustrialRevolutionEvent();
			}
		}
		if (param.CutsceneTitle == null)
		{
			this.CutsceneTitle = SokLoc.Translate("label_start_of_moon", new LocParam[] { LocParam.Create("moon", this.CurrentMonth.ToString()) });
		}
		else
		{
			this.CutsceneTitle = param.CutsceneTitle;
		}
		if (!param.SkipSpecialEvents)
		{
			yield return EndOfMonthCutscenes.SpecialEvents();
		}
		if (param.CutsceneTitle == null)
		{
			this.CutsceneTitle = SokLoc.Translate("label_start_of_moon", new LocParam[] { LocParam.Create("moon", this.CurrentMonth.ToString()) });
		}
		else
		{
			this.CutsceneTitle = param.CutsceneTitle;
		}
		this.CutsceneText = "";
		if (!param.SkipEndConfirmation)
		{
			yield return this.WaitForContinueClicked(SokLoc.Translate("label_start_new_moon"));
		}
		GameCanvas.instance.SetScreen<GameScreen>();
		if (param.OnDone != null)
		{
			Action onDone = param.OnDone;
			if (onDone != null)
			{
				onDone();
			}
		}
		GameCamera.instance.TargetPositionOverride = null;
		this.currentAnimationRoutine = null;
		SaveManager.instance.Save(true);
		if (DebugScreen.instance != null)
		{
			DebugScreen.instance.AutoSave();
		}
		yield break;
	}

	public IEnumerator EndOfMonthCitiesRoutine(EndOfMonthParameters param)
	{
		AudioManager.me.PlaySound2D(AudioManager.me.EndOfMoon, 0.8f, 0.2f);
		if (this.CurrentView != ViewType.Default)
		{
			this.SetViewType(ViewType.Default);
		}
		if (param.CutsceneTitle == null)
		{
			this.CutsceneTitle = SokLoc.Translate("label_end_of_moon", new LocParam[] { LocParam.Create("moon", (this.CurrentMonth - 1).ToString()) });
		}
		else
		{
			this.CutsceneTitle = param.CutsceneTitle;
		}
		CutsceneScreen.instance.EnableWellbeingBar(CitiesManager.instance.Wellbeing);
		yield return new WaitForSeconds(1f);
		List<CardData> requirementsCards = (from x in this.GetCards<CardData>()
			where x.RequirementHolders != null && x.RequirementHolders.Count > 0 && (!(x.MyGameCard.GetCardWithStatusInStack() != null) || !(x.MyGameCard.GetCardWithStatusInStack().TimerActionId == "finish_blueprint"))
			select x).ToList<CardData>();
		int previousWellbeing = CitiesManager.instance.Wellbeing;
		(from x in this.GetCards<Enemy>()
			where x.InConflict
			select x).ToList<Enemy>();
		this.GetCardCount<CitiesCombatable>();
		foreach (CardData requirementCard in requirementsCards)
		{
			foreach (RequirementHolder requirementHolder in requirementCard.RequirementHolders)
			{
				bool flag = true;
				foreach (CardRequirement cardRequirement in requirementHolder.CardRequirements)
				{
					flag = cardRequirement.Satisfied(requirementCard.MyGameCard);
					if (!flag)
					{
						break;
					}
				}
				if (flag)
				{
					foreach (CardRequirementResult cardRequirementResult in requirementHolder.PositiveResults)
					{
						if (requirementCard.MyGameCard != null)
						{
							yield return cardRequirementResult.Perform(requirementCard.MyGameCard);
						}
					}
					List<CardRequirementResult>.Enumerator enumerator4 = default(List<CardRequirementResult>.Enumerator);
				}
				else
				{
					foreach (CardRequirementResult cardRequirementResult2 in requirementHolder.NegativeResults)
					{
						if (requirementCard.MyGameCard != null)
						{
							yield return cardRequirementResult2.Perform(requirementCard.MyGameCard);
						}
					}
					List<CardRequirementResult>.Enumerator enumerator4 = default(List<CardRequirementResult>.Enumerator);
				}
			}
			List<RequirementHolder>.Enumerator enumerator2 = default(List<RequirementHolder>.Enumerator);
			requirementCard = null;
		}
		List<CardData>.Enumerator enumerator = default(List<CardData>.Enumerator);
		List<CardData> rootWithResults = (from x in this.GetCards<CardData>()
			where x.MonthlyRequirementResult != null
			select x).ToList<CardData>();
		CutsceneScreen.instance.WellbeingAmount = CitiesManager.instance.Wellbeing;
		if (CitiesManager.instance.Wellbeing - previousWellbeing > 0)
		{
			AudioManager.me.PlaySound2D(AudioManager.me.AddWellbeing, 1f, 0.5f);
		}
		else if (CitiesManager.instance.Wellbeing - previousWellbeing < 0)
		{
			AudioManager.me.PlaySound2D(AudioManager.me.LostWellbeing, 1f, 0.5f);
		}
		if (CitiesManager.instance.Wellbeing - previousWellbeing >= 5)
		{
			QuestManager.instance.SpecialActionComplete("cities_wellbeing_gained_5", null);
		}
		else if (CitiesManager.instance.Wellbeing - previousWellbeing <= -5)
		{
			QuestManager.instance.SpecialActionComplete("cities_wellbeing_lost_5", null);
		}
		bool lostGame = false;
		if (CitiesManager.instance.Wellbeing > 0)
		{
			foreach (CardData cardData in rootWithResults)
			{
				int num = 0;
				foreach (KeyValuePair<string, MonthlyResult> keyValuePair in cardData.MonthlyRequirementResult.results)
				{
					if (keyValuePair.Value.Amount != 0)
					{
						this.CreateFloatingText(cardData.MyGameCard, keyValuePair.Value.Amount > 0, keyValuePair.Value.Amount, keyValuePair.Value.Card.CardData.GetRequirementDescription(keyValuePair.Value.Card, keyValuePair.Value.CardAmount, true), this.GetIconStringFromRequirementType(keyValuePair.Value.Type), keyValuePair.Value.Amount > 0, num, 0f, false);
					}
					num++;
				}
			}
			this.CutsceneBoardView = true;
			yield return EndOfMonthCutscenes.MaxCardCount();
			this.CutsceneBoardView = false;
			if (CitiesManager.instance.Wellbeing > 25 && !this.CurrentRunOptions.IsPeacefulMode && this.CurrentMonth == CitiesManager.instance.NextConflictMonth && CitiesManager.instance.NextConflictMonth != -1)
			{
				Vector3 randomSpawnPosition = this.GetRandomSpawnPosition();
				GameCamera.instance.TargetPositionOverride = new Vector3?(randomSpawnPosition);
				this.CutsceneTitle = SokLoc.Translate("label_goblin_conflict_title");
				this.CutsceneText = SokLoc.Translate("label_goblin_conflict_text");
				this.CreateCard(randomSpawnPosition, "event_goblin_attack", true, true, true);
				yield return this.WaitForContinueClicked(SokLoc.Translate("label_uh_oh"));
				GameCamera.instance.TargetPositionOverride = null;
			}
			this.CutsceneBoardView = true;
			CutsceneScreen.instance.CanMoveScreen = true;
			int num2 = CitiesManager.instance.Wellbeing - previousWellbeing;
			this.CutsceneTitle = SokLoc.Translate("label_end_of_moon_cities_wellbeing");
			string text = Mathf.Abs(num2).ToString();
			if (num2 > 0)
			{
				this.CutsceneText = SokLoc.Translate("label_end_of_moon_cities_gained", new LocParam[]
				{
					LocParam.Create("amount", text),
					LocParam.Create("icon", Icons.Wellbeing)
				});
			}
			else if (num2 == 0)
			{
				this.CutsceneText = SokLoc.Translate("label_end_of_moon_cities_same", new LocParam[] { LocParam.Create("icon", Icons.Wellbeing) });
			}
			else
			{
				this.CutsceneText = SokLoc.Translate("label_end_of_moon_cities_lost", new LocParam[]
				{
					LocParam.Create("amount", text),
					LocParam.Create("icon", Icons.Wellbeing)
				});
			}
			if (num2 != 0)
			{
				this.CutsceneText = this.CutsceneText + ", " + SokLoc.Translate("label_hover_status_wellbeing");
			}
			yield return this.WaitForContinueClicked(SokLoc.Translate((num2 >= 0) ? "label_nice" : "label_uh_oh"));
			if (param.CutsceneTitle == null)
			{
				this.CutsceneTitle = SokLoc.Translate("label_start_of_moon", new LocParam[] { LocParam.Create("moon", this.CurrentMonth.ToString()) });
			}
			else
			{
				this.CutsceneTitle = param.CutsceneTitle;
			}
			this.CutsceneText = "";
			if (!param.SkipEndConfirmation)
			{
				yield return this.WaitForContinueClicked(SokLoc.Translate("label_start_new_moon"));
			}
			GameCamera.instance.TargetPositionOverride = null;
		}
		else
		{
			lostGame = true;
			this.CutsceneTitle = SokLoc.Translate("label_cities_game_over_title");
			this.CutsceneText = SokLoc.Translate("label_cities_game_over_text");
			yield return this.WaitForContinueClicked(SokLoc.Translate("label_uh_oh"));
			this.CutsceneText = SokLoc.Translate("label_cities_game_over_text_1");
			yield return this.WaitForContinueClicked(SokLoc.Translate("label_okay"));
			GameCamera.instance.TargetPositionOverride = null;
		}
		CutsceneScreen.instance.CanMoveScreen = false;
		if (!lostGame)
		{
			this.CloseAllFloatingTextObjects();
			foreach (CardData requirementCard in requirementsCards.Where<CardData>((CardData x) => x != null))
			{
				foreach (RequirementHolder requirementHolder2 in requirementCard.RequirementHolders)
				{
					bool flag2 = true;
					foreach (CardRequirement cardRequirement2 in requirementHolder2.CardRequirements)
					{
						flag2 = cardRequirement2.Satisfied(requirementCard.MyGameCard);
						if (!flag2)
						{
							break;
						}
					}
					if (flag2)
					{
						foreach (CardRequirementResult cardRequirementResult3 in requirementHolder2.PositiveResults)
						{
							yield return cardRequirementResult3.EndOfCutscenePerform(requirementCard.MyGameCard);
						}
						List<CardRequirementResult>.Enumerator enumerator4 = default(List<CardRequirementResult>.Enumerator);
					}
					else
					{
						foreach (CardRequirementResult cardRequirementResult4 in requirementHolder2.NegativeResults)
						{
							yield return cardRequirementResult4.EndOfCutscenePerform(requirementCard.MyGameCard);
						}
						List<CardRequirementResult>.Enumerator enumerator4 = default(List<CardRequirementResult>.Enumerator);
					}
				}
				List<RequirementHolder>.Enumerator enumerator2 = default(List<RequirementHolder>.Enumerator);
				requirementCard = null;
			}
			IEnumerator<CardData> enumerator7 = null;
			foreach (CardData cardData2 in rootWithResults)
			{
				cardData2.MonthlyRequirementResult = null;
			}
			if (this.CurrentMonth % 4 == 0)
			{
				AudioManager.me.PlaySound2D(AudioManager.me.EndOfMoon, 0.5f, 0.5f);
				this.CutsceneTitle = SokLoc.Translate("label_weather_report_title");
				this.CutsceneText = SokLoc.Translate("label_weather_report_text");
				GameCamera.instance.TargetPositionOverride = new Vector3?(this.MiddleOfBoard());
				yield return new WaitForSeconds(0.5f);
				Boosterpack pack = this.CreateBoosterpack(this.MiddleOfBoard(), "cities_weather");
				AudioManager.me.PlaySound2D(AudioManager.me.CardCreate, 1f, 0.5f);
				yield return new WaitForSeconds(0.5f);
				int num3;
				for (int i = 0; i < pack.TotalCardsInPack; i = num3 + 1)
				{
					pack.Clicked();
					yield return new WaitForSeconds(0.2f);
					num3 = i;
				}
				yield return this.WaitForContinueClicked(SokLoc.Translate("label_nice"));
				GameCamera.instance.TargetPositionOverride = null;
				pack = null;
			}
		}
		QuestManager.instance.SpecialActionComplete("cities_wellbeing_changed", null);
		GameCanvas.instance.SetScreen<GameScreen>();
		if (param.OnDone != null)
		{
			Action onDone = param.OnDone;
			if (onDone != null)
			{
				onDone();
			}
		}
		this.currentAnimationRoutine = null;
		this.CutsceneBoardView = false;
		GameCamera.instance.TargetPositionOverride = null;
		this.CutsceneTitle = "";
		this.CutsceneText = "";
		if (lostGame)
		{
			GameBoard citiesBoard = this.GetCurrentBoardSafe();
			this.GoToBoard(this.GetBoardWithId("main"), delegate
			{
				this.RemoveAllCardsFromBoard(citiesBoard.Id, true);
				this.ResetBoughtBoostersOnLocation(citiesBoard.Location);
				this.ResetCityVariables();
			}, "cities");
		}
		else
		{
			SaveManager.instance.Save(true);
			if (DebugScreen.instance != null)
			{
				DebugScreen.instance.AutoSave();
			}
			this.QueueCutsceneIfNotPlayed("cities_first_moon");
		}
		yield break;
		yield break;
	}

	public string GetIconStringFromRequirementType(RequirementType type)
	{
		if (type == RequirementType.Food)
		{
			return Icons.Food;
		}
		if (type == RequirementType.WellBeing)
		{
			return Icons.Wellbeing;
		}
		if (type == RequirementType.Energy)
		{
			return Icons.Energy;
		}
		if (type == RequirementType.Dollar)
		{
			return Icons.Dollar;
		}
		if (type == RequirementType.Pollution)
		{
			return Icons.Pollution;
		}
		if (type == RequirementType.Health)
		{
			return Icons.Health;
		}
		throw new NotImplementedException("Icon is not implemented");
	}

	private IEnumerator FinishDemandRoutine(Demand demand, DemandEvent demandEvent)
	{
		if (demand.Amount == demandEvent.AmountGiven)
		{
			yield return GreedCutscenes.FinishDemandSuccessPreMoon(demand);
			demandEvent.Successful = true;
			this.CurrentRunVariables.LastDemandMonth = this.CurrentMonth + 1;
		}
		else if (this.GetCardCount((CardData x) => x.Id == demand.CardToGet) >= demand.Amount - demandEvent.AmountGiven)
		{
			yield return GreedCutscenes.FinishDemandSuccess(demandEvent);
			demandEvent.Successful = true;
			this.CurrentRunVariables.LastDemandMonth = this.CurrentMonth;
		}
		else
		{
			yield return GreedCutscenes.FinishDemandFailed(demand);
			demandEvent.Successful = false;
			this.CurrentRunVariables.LastDemandMonth = this.CurrentMonth;
		}
		this.CurrentRunVariables.PreviousDemandEvents.Add(demandEvent);
		this.CurrentRunVariables.ActiveDemand = null;
		if (demandEvent.Successful)
		{
			QuestManager.instance.SpecialActionComplete("demand_success", null);
		}
		else
		{
			QuestManager.instance.SpecialActionComplete("demand_failed", null);
		}
		yield break;
	}

	private void LateUpdate()
	{
		if (!this.ShowContinueButton)
		{
			this.ContinueClicked = false;
		}
	}

	public void KillVillager(Combatable combatable, Action onComplete = null, Action onCreateCorpse = null)
	{
		this.currentAnimationRoutine = base.StartCoroutine(this.KillVillagerCoroutine(combatable, delegate
		{
			this.currentAnimationRoutine = null;
			Action onComplete2 = onComplete;
			if (onComplete2 == null)
			{
				return;
			}
			onComplete2();
		}, onCreateCorpse, true));
	}

	public IEnumerator KillVillagerCoroutine(Combatable combatable, Action onComplete, Action onCreateCorpse, bool resetTargetOnDeath = true)
	{
		GameCamera.instance.TargetPositionOverride = new Vector3?(combatable.MyGameCard.transform.position);
		yield return new WaitForSeconds(1f);
		List<Equipable> allEquipables = combatable.GetAllEquipables();
		List<ExtraCardData> extraCardData = combatable.GetExtraCardData();
		if (combatable.MyGameCard.HasParent && combatable.MyGameCard.HasChild)
		{
			GameCard parent = combatable.MyGameCard.Parent;
			GameCard child = combatable.MyGameCard.Child;
			combatable.MyGameCard.RemoveFromStack();
			child.SetParent(parent);
		}
		combatable.MyGameCard.DestroyCard(true, true);
		if (combatable is Animal)
		{
			combatable.Die();
		}
		else if (combatable.Id != "trained_monkey")
		{
			this.CreateCard(combatable.MyGameCard.transform.position, "corpse", true, false, true).SetExtraCardData(extraCardData);
			if (onCreateCorpse != null)
			{
				onCreateCorpse();
			}
			this.TryCreateUnhappiness(combatable.MyGameCard.transform.position, 2);
		}
		foreach (Equipable equipable in allEquipables)
		{
			if (equipable != null && !string.IsNullOrEmpty(equipable.Id))
			{
				this.CreateCard(combatable.transform.position, equipable.Id, true, false, false).MyGameCard.SendIt();
			}
		}
		yield return new WaitForSeconds(1f);
		if (resetTargetOnDeath)
		{
			GameCamera.instance.TargetPositionOverride = null;
		}
		if (onComplete != null)
		{
			onComplete();
		}
		yield break;
	}

	public bool CheckAllVillagersDead()
	{
		return !this.DebugDontNeedVillagers && this.GetCardCount<BaseVillager>() <= 0;
	}

	public void CreateSmoke(Vector3 pos)
	{
		ParticleSystem particleSystem = null;
		foreach (ParticleSystem particleSystem2 in this.smokeBuffer)
		{
			if (!particleSystem2.isPlaying)
			{
				particleSystem = particleSystem2;
			}
		}
		if (particleSystem == null)
		{
			particleSystem = Object.Instantiate<GameObject>(PrefabManager.instance.SmokeParticlePrefab).GetComponentInChildren<ParticleSystem>();
			this.smokeBuffer.Add(particleSystem);
		}
		particleSystem.Play();
		particleSystem.transform.position = pos + Vector3.up * 0.05f;
	}

	public void CreateMinusElectricity(Vector3 pos)
	{
		ParticleSystem particleSystem = null;
		foreach (ParticleSystem particleSystem2 in this.energyMinusBuffer)
		{
			if (!particleSystem2.isPlaying)
			{
				particleSystem = particleSystem2;
			}
		}
		if (particleSystem == null)
		{
			particleSystem = Object.Instantiate<GameObject>(PrefabManager.instance.ElectricityMinusParticlePrefab).GetComponentInChildren<ParticleSystem>();
			this.energyMinusBuffer.Add(particleSystem);
		}
		particleSystem.Play();
		particleSystem.transform.position = pos + Vector3.up * 0.05f;
	}

	public void CreateWellbeingPlus(Vector3 pos)
	{
		ParticleSystem particleSystem = null;
		foreach (ParticleSystem particleSystem2 in this.wellbeingPlusBuffer)
		{
			if (!particleSystem2.isPlaying)
			{
				particleSystem = particleSystem2;
			}
		}
		if (particleSystem == null)
		{
			particleSystem = Object.Instantiate<GameObject>(PrefabManager.instance.WellbeingPlusParticlePrefab).GetComponentInChildren<ParticleSystem>();
			this.wellbeingPlusBuffer.Add(particleSystem);
		}
		particleSystem.Play();
		particleSystem.transform.position = pos + Vector3.up * 0.05f;
	}

	public void CloseAllFloatingTextObjects()
	{
		foreach (FloatingStatus floatingStatus in this.floatingTextBuffer)
		{
			if (floatingStatus != null && floatingStatus.ParentCard)
			{
				floatingStatus.StopAnimation();
			}
		}
	}

	public void CreateFloatingText(GameCard parent, bool isPositive, int amount, string descriptionText, string iconTag, bool desiredBehaviour, int index = 1, float timer = 1f, bool closeOnHover = false)
	{
		FloatingStatus floatingStatus = null;
		foreach (FloatingStatus floatingStatus2 in this.floatingTextBuffer)
		{
			if (!floatingStatus2.InAnimation)
			{
				floatingStatus = floatingStatus2;
			}
		}
		if (floatingStatus == null)
		{
			floatingStatus = Object.Instantiate<GameObject>(PrefabManager.instance.FloatingTextPrefab).GetComponentInChildren<FloatingStatus>();
			this.floatingTextBuffer.Add(floatingStatus);
		}
		floatingStatus.StartAnimation(parent, isPositive, amount, descriptionText, iconTag, desiredBehaviour, index, timer, closeOnHover);
	}

	public void TryCreateHappiness(Vector3 position, int amount)
	{
		if (!this.CurseIsActive(CurseType.Happiness))
		{
			return;
		}
		for (int i = 0; i < amount; i++)
		{
			CardData cardData = this.CreateCard(position, "happiness", true, false, true);
			this.CreateSmoke(position);
			this.StackSend(cardData.MyGameCard, Vector3.zero, null, true);
		}
	}

	public void TryCreateUnhappiness(Vector3 position, int amount)
	{
		if (!this.CurseIsActive(CurseType.Happiness))
		{
			return;
		}
		for (int i = 0; i < amount; i++)
		{
			CardData cardData = this.CreateCard(position, "unhappiness", true, false, true);
			this.CreateSmoke(position);
			this.StackSend(cardData.MyGameCard, Vector3.zero, null, true);
		}
	}

	public ICardId GetRandomCard(List<CardChance> chances, bool removeCard)
	{
		this.chanceBag.Clear();
		foreach (CardChance cardChance in chances)
		{
			if ((!cardChance.HasMaxCount || this.GetCurrentCardCount(cardChance.Id) < cardChance.MaxCountToGive) && (!cardChance.HasPrerequisiteCard || this.GivenCards.Contains(cardChance.PrerequisiteCardId)))
			{
				if (cardChance.IsEnemy)
				{
					if (cardChance.Chance != 0)
					{
						this.chanceBag.AddEntry(cardChance, (float)cardChance.Chance);
					}
				}
				else
				{
					CardData cardPrefab = this.GetCardPrefab(cardChance.Id, true);
					if ((!this.CurrentRunOptions.IsPeacefulMode || !(cardPrefab is Enemy)) && (!this.CurrentRunOptions.IsPeacefulMode || !(cardPrefab.Id == "catacombs")) && ((cardPrefab.MyCardType != CardType.Ideas && cardPrefab.MyCardType != CardType.Rumors) || !this.CurrentSave.FoundCardIds.Contains(cardChance.Id)) && cardChance.Chance != 0)
					{
						this.chanceBag.AddEntry(cardChance, (float)cardChance.Chance);
					}
				}
			}
		}
		CardChance cardChance2 = this.chanceBag.Choose();
		if (cardChance2 == null)
		{
			return null;
		}
		if (cardChance2.IsEnemy && !this.CurrentRunOptions.IsPeacefulMode)
		{
			return CardBag.GetCardIdForEnemyBag(cardChance2.EnemyBag, cardChance2.Strength);
		}
		return (CardId)cardChance2.Id;
	}

	private int GetCurrentCardCount(string cardId)
	{
		return this.AllCards.Count<GameCard>((GameCard c) => c.CardData.Id == cardId && c.MyBoard.IsCurrent);
	}

	public GameCard GetCardWithUniqueId(string uniqueId)
	{
		GameCard gameCard;
		if (!this.UniqueIdToCard.TryGetValue(uniqueId, out gameCard))
		{
			return null;
		}
		return gameCard;
	}

	public int GetTimesAnyBoosterWasBought()
	{
		return this.BoughtBoosterIds.Count;
	}

	public int GetTimesBoosterWasBoughtOnLocation(Location location)
	{
		int num = 0;
		foreach (string text in this.BoughtBoosterIds)
		{
			BoosterpackData boosterData = this.GetBoosterData(text);
			if (boosterData != null && boosterData.BoosterLocation == location)
			{
				num++;
			}
		}
		return num;
	}

	public void ResetBoughtBoostersOnLocation(Location location)
	{
		this.BoughtBoosterIds.RemoveAll(delegate(string boosterId)
		{
			BoosterpackData boosterData = this.GetBoosterData(boosterId);
			return boosterData != null && boosterData.BoosterLocation == location;
		});
	}

	public List<Conflict> GetAllConflicts()
	{
		List<Conflict> list = new List<Conflict>();
		foreach (GameCard gameCard in this.AllCards)
		{
			if (gameCard.InConflict && !list.Contains(gameCard.Combatable.MyConflict))
			{
				list.Add(gameCard.Combatable.MyConflict);
			}
		}
		return list;
	}

	public void LoadSaveRound(SaveRound saveRound)
	{
		this.IsLoadingSaveRound = true;
		this.ClearRound();
		this.AllCards.Clear();
		this.UniqueIdToCard.Clear();
		if (Application.isEditor)
		{
			Debug.Log(string.Format("Loading Run with {0} moon length and peaceful mode: {1}", saveRound.RunOptions.MoonLength, saveRound.RunOptions.IsPeacefulMode));
		}
		this.MonthTimer = saveRound.MonthTimer;
		this.OldCurrentMonth = saveRound.CurrentMonth;
		this.BoardMonths = new BoardMonths(saveRound.BoardMonths);
		this.GivenCards = saveRound.GivenCards;
		this.BoughtBoosterIds = saveRound.BoughtBoosterIds;
		this.CurrentBoard = this.GetBoardWithId(saveRound.CurrentBoardId);
		this.CurrentRunOptions = saveRound.RunOptions;
		this.CurrentRunVariables = saveRound.RunVariables;
		this.RoundExtraKeyValues = saveRound.ExtraKeyValues;
		if (CitiesManager.instance == null)
		{
			new Exception("CitiesManager should be active before loading the saveRound");
		}
		CitiesManager.instance.Wellbeing = saveRound.CitiesWellbeing;
		CitiesManager.instance.NextConflictMonth = saveRound.CitiesConflictMonth;
		CitiesManager.instance.ActiveEvent = saveRound.CitiesDisaster;
		if (this.CurrentRunVariables.ActiveDemand != null && string.IsNullOrEmpty(this.CurrentRunVariables.ActiveDemand.DemandId))
		{
			this.CurrentRunVariables.ActiveDemand = null;
		}
		foreach (SavedCard savedCard in saveRound.SavedCards)
		{
			CardData cardData = this.CreateCard(savedCard.CardPosition, savedCard.CardPrefabId, savedCard.FaceUp, false, false);
			if (!(cardData == null))
			{
				cardData.MyGameCard.MyBoard = this.GetBoardWithId(savedCard.BoardId);
				cardData.UniqueId = savedCard.UniqueId;
				this.UniqueIdToCard[cardData.UniqueId] = cardData.MyGameCard;
				cardData.ParentUniqueId = savedCard.ParentUniqueId;
				cardData.EquipmentHolderUniqueId = savedCard.EquipmentHolderUniqueId;
				cardData.WorkerHolderUniqueId = savedCard.WorkerHolderUniqueId;
				cardData.WorkerIndex = savedCard.WorkerIndex;
				cardData.SetExtraCardData(savedCard.ExtraCardData);
				if (savedCard.IsFoil)
				{
					cardData.SetFoil();
				}
				cardData.IsDamaged = savedCard.IsDamaged;
				cardData.DamageType = savedCard.DamageType;
				if (savedCard.StatusEffects != null && savedCard.StatusEffects.Count > 0)
				{
					List<StatusEffect> list = savedCard.StatusEffects.Select<SavedStatusEffect, StatusEffect>((SavedStatusEffect x) => StatusEffect.FromSavedStatusEffect(x)).ToList<StatusEffect>();
					list.RemoveAll((StatusEffect x) => x == null);
					foreach (StatusEffect statusEffect in list)
					{
						statusEffect.ParentCard = cardData;
					}
					cardData.StatusEffects = list;
					cardData.MyGameCard.StatusEffectsChanged();
				}
				else
				{
					cardData.StatusEffects = new List<StatusEffect>();
				}
				if (savedCard.CardConnectors != null && savedCard.CardConnectors.Count > 0)
				{
					List<SavedCardConnector> cardConnectors = savedCard.CardConnectors;
					cardConnectors.RemoveAll((SavedCardConnector x) => x == null || string.IsNullOrEmpty(x.ConnectedNodeUniqueId));
					for (int i = 0; i < cardData.MyGameCard.CardConnectorChildren.Count; i++)
					{
						CardConnector cardConnector = cardData.MyGameCard.CardConnectorChildren[i];
						string myUniqueId = cardConnector.GetConnectorUniqueId();
						SavedCardConnector savedCardConnector = cardConnectors.Find((SavedCardConnector x) => x.UniqueId == myUniqueId);
						if (savedCardConnector != null)
						{
							cardConnector.UniqueId = savedCardConnector.UniqueId;
							cardConnector.ConnectedNodeUniqueId = savedCardConnector.ConnectedNodeUniqueId;
						}
					}
				}
				if (savedCard.TimerRunning)
				{
					TimerAction delegateForActionId = cardData.GetDelegateForActionId(savedCard.TimerActionId);
					if (delegateForActionId != null)
					{
						cardData.MyGameCard.StartTimer(savedCard.TargetTimerTime, delegateForActionId, savedCard.Status, savedCard.TimerActionId, savedCard.WithStatusBar, true, false);
						cardData.MyGameCard.CurrentTimerTime = savedCard.CurrentTimerTime;
						cardData.MyGameCard.TimerBlueprintId = savedCard.TimerBlueprintId;
						cardData.MyGameCard.TimerSubprintIndex = savedCard.SubprintIndex;
						cardData.MyGameCard.SkipCitiesChecks = savedCard.SkipCitiesChecks;
					}
				}
			}
		}
		foreach (GameCard gameCard in this.AllCards)
		{
			using (List<CardConnector>.Enumerator enumerator4 = gameCard.CardConnectorChildren.GetEnumerator())
			{
				while (enumerator4.MoveNext())
				{
					CardConnector connector = enumerator4.Current;
					if (!string.IsNullOrEmpty(connector.ConnectedNodeUniqueId))
					{
						CardConnector cardConnector2 = (from x in this.AllCards.SelectMany<GameCard, CardConnector>((GameCard x) => x.CardConnectorChildren)
							where x.UniqueId == connector.ConnectedNodeUniqueId
							select x).FirstOrDefault<CardConnector>();
						if (cardConnector2 != null)
						{
							connector.ConnectedNode = cardConnector2;
						}
					}
				}
			}
		}
		foreach (GameCard gameCard2 in this.AllCards)
		{
			if (!string.IsNullOrEmpty(gameCard2.CardData.ParentUniqueId))
			{
				GameCard cardWithUniqueId = this.GetCardWithUniqueId(gameCard2.CardData.ParentUniqueId);
				if (cardWithUniqueId != null)
				{
					gameCard2.SetParent(cardWithUniqueId);
				}
			}
		}
		foreach (GameCard gameCard3 in this.AllCards)
		{
			if (!string.IsNullOrEmpty(gameCard3.CardData.EquipmentHolderUniqueId))
			{
				GameCard cardWithUniqueId2 = this.GetCardWithUniqueId(gameCard3.CardData.EquipmentHolderUniqueId);
				if (cardWithUniqueId2 != null)
				{
					cardWithUniqueId2.EquipmentChildren.Add(gameCard3);
					gameCard3.EquipmentHolder = cardWithUniqueId2;
					gameCard3.IsEquipped = true;
				}
			}
		}
		foreach (GameCard gameCard4 in this.AllCards)
		{
			if (gameCard4.CardData.WorkerAmount > 0)
			{
				gameCard4.WorkerTransformHolder.UpdateWorkerAmount(gameCard4.CardData.WorkerAmount);
			}
			if (!string.IsNullOrEmpty(gameCard4.CardData.WorkerHolderUniqueId))
			{
				GameCard cardWithUniqueId3 = this.GetCardWithUniqueId(gameCard4.CardData.WorkerHolderUniqueId);
				if (cardWithUniqueId3 != null)
				{
					if (cardWithUniqueId3.WorkerChildren.Count < cardWithUniqueId3.CardData.WorkerAmount)
					{
						cardWithUniqueId3.WorkerChildren.Add(gameCard4);
						gameCard4.WorkerHolder = cardWithUniqueId3;
						gameCard4.IsWorking = true;
					}
					else
					{
						gameCard4.CardData.WorkerHolderUniqueId = null;
						gameCard4.IsWorking = false;
					}
				}
			}
		}
		foreach (GameCard gameCard5 in this.AllCards)
		{
			gameCard5.StatusEffectsChanged();
		}
		foreach (SavedConflict savedConflict in saveRound.SavedConflicts)
		{
			Conflict.CreateFromSavedConflict(savedConflict);
		}
		foreach (SavedBooster savedBooster2 in saveRound.SavedBoosters)
		{
			Boosterpack boosterpack = this.CreateBoosterpack(savedBooster2.Position, savedBooster2.BoosterId);
			if (boosterpack != null)
			{
				boosterpack.MyBoard = this.GetBoardWithId(savedBooster2.BoardId);
				int num = savedBooster2.TimesOpened;
				boosterpack.TimesOpened = savedBooster2.TimesOpened;
				for (int j = 0; j < boosterpack.CardBags.Count; j++)
				{
					CardBag cardBag = boosterpack.CardBags[j];
					int num2 = Mathf.Min(num, cardBag.CardsInPack);
					cardBag.CardsInPack -= num2;
					num -= num2;
					if (num <= 0)
					{
						break;
					}
				}
			}
		}
		using (List<SavedBoosterBox>.Enumerator enumerator7 = saveRound.SavedBoosterBoxes.GetEnumerator())
		{
			while (enumerator7.MoveNext())
			{
				SavedBoosterBox savedBooster = enumerator7.Current;
				BuyBoosterBox buyBoosterBox = this.AllBoosterBoxes.Find((BuyBoosterBox x) => x.BoosterId == savedBooster.BoosterId);
				if (buyBoosterBox != null)
				{
					buyBoosterBox.StoredCostAmount = savedBooster.StoredCostAmount;
				}
			}
		}
		if (saveRound.SaveVersion != 3)
		{
			this.PerformSaveRoundMigration(saveRound.SaveVersion, 3);
		}
		this.IsLoadingSaveRound = false;
	}

	public void PerformSaveRoundMigration(int oldVersion, int newVersion)
	{
		if (oldVersion == 0 && newVersion == 1)
		{
			Debug.Log(string.Format("Performing save round migration from v{0} to v{1}", oldVersion, newVersion));
			foreach (GameCard gameCard in this.AllCards)
			{
				BaseVillager baseVillager = gameCard.CardData as BaseVillager;
				if (baseVillager != null)
				{
					baseVillager.HealthPoints = Mathf.Min(baseVillager.ProcessedCombatStats.MaxHealth, baseVillager.HealthPoints * 3);
				}
			}
			for (int i = this.AllCards.Count - 1; i >= 0; i--)
			{
				Combatable combatable = this.AllCards[i].CardData as Combatable;
				if (combatable != null)
				{
					if (combatable.Id == "swordsman")
					{
						combatable.CreateAndEquipCard("sword", true);
					}
					if (combatable.Id == "explorer")
					{
						combatable.CreateAndEquipCard("map", true);
					}
					if (combatable.Id == "militia")
					{
						combatable.CreateAndEquipCard("spear", true);
					}
					if (combatable.Id == "fisher")
					{
						combatable.CreateAndEquipCard("fishing_rod", true);
					}
				}
			}
		}
		if (oldVersion == 1 && newVersion == 2 && this.BoardMonths.IsEmpty && this.MonthTimer > 0f)
		{
			this.BoardMonths = new BoardMonths();
			this.BoardMonths.MainMonth = this.OldCurrentMonth - this.CurrentRunVariables.IslandMonths;
			this.BoardMonths.IslandMonth = this.CurrentRunVariables.IslandMonths;
			this.BoardMonths.DeathMonth = Mathf.Max(1, this.CurrentRunVariables.DeathMonths);
		}
		if (oldVersion == 2 && newVersion == 3)
		{
			List<GameCard> list = this.AllCards.Where<GameCard>((GameCard x) => x.CardData.Id == "strange_portal").ToList<GameCard>();
			for (int j = 0; j < list.Count - 1; j++)
			{
				list[j].DestroyCard(false, true);
			}
		}
	}

	public GameBoard GetBoardWithId(string id)
	{
		foreach (GameBoard gameBoard in this.Boards)
		{
			if (gameBoard.Id == id)
			{
				return gameBoard;
			}
		}
		return null;
	}

	public GameBoard GetBoardWithLocation(Location loc)
	{
		foreach (GameBoard gameBoard in this.Boards)
		{
			if (gameBoard.Location == loc)
			{
				return gameBoard;
			}
		}
		return null;
	}

	public bool BoughtWithGoldChest(GameCard card, int count)
	{
		return this.BoughtWithChest(card, count, "gold");
	}

	public bool BoughtWithShellChest(GameCard card, int count)
	{
		return this.BoughtWithChest(card, count, "shell");
	}

	private bool BoughtWithChest(GameCard card, int count, string heldCardId)
	{
		return card.GetAllCardsInStack().Sum<GameCard>(delegate(GameCard x)
		{
			Chest chest = x.CardData as Chest;
			if (chest == null || !(chest.HeldCardId == heldCardId))
			{
				return 0;
			}
			return chest.CoinCount;
		}) >= count;
	}

	public int GetAmountInChest(GameCard card, string heldCardId)
	{
		return card.GetAllCardsInStack().Sum<GameCard>(delegate(GameCard x)
		{
			Chest chest = x.CardData as Chest;
			if (chest == null || !(chest.HeldCardId == heldCardId))
			{
				return 0;
			}
			return chest.CoinCount;
		});
	}

	public void BuyWithChest(GameCard childCard, int toUse)
	{
		List<Chest> list = (from x in childCard.GetAllCardsInStack()
			where x.CardData is Chest
			select x.CardData as Chest).ToList<Chest>();
		for (int i = 0; i < list.Count; i++)
		{
			Chest chest = list[i];
			int num = Mathf.Min(toUse, chest.CoinCount);
			chest.CoinCount -= num;
			toUse -= num;
			if (toUse <= 0)
			{
				break;
			}
		}
		if (childCard.HasParent)
		{
			childCard.RemoveFromStack();
			childCard.SendIt();
		}
	}

	public bool BoughtWithGold(GameCard card, int count, bool checkStackAllSame = false)
	{
		return this.GetCardCountInStack(card, (CardData x) => x.Id == "gold") >= count;
	}

	public bool BoughtWithShells(GameCard card, int count, bool checkStackAllSame = false)
	{
		return this.GetCardCountInStack(card, (CardData x) => x.Id == "shell") >= count;
	}

	public int GetDollarsInCreditcard(GameCard card)
	{
		return card.GetAllCardsInStack().Sum<GameCard>(delegate(GameCard x)
		{
			Creditcard creditcard = x.CardData as Creditcard;
			if (creditcard == null)
			{
				return 0;
			}
			return creditcard.DollarCount;
		});
	}

	public void BuyWithCreditcard(GameCard childCard, int toUse)
	{
		List<Creditcard> list = (from x in childCard.GetAllCardsInStack()
			where x.CardData is Creditcard
			select x.CardData as Creditcard).ToList<Creditcard>();
		for (int i = 0; i < list.Count; i++)
		{
			Creditcard creditcard = list[i];
			int num = Mathf.Min(toUse, creditcard.DollarCount);
			creditcard.DollarCount -= num;
			toUse -= num;
			if (toUse <= 0)
			{
				break;
			}
		}
		if (childCard.HasParent)
		{
			childCard.RemoveFromStack();
			childCard.SendIt();
		}
	}

	public void RemoveCardsFromStack(GameCard childCard, int toRemove)
	{
		for (int i = 0; i < toRemove; i++)
		{
			childCard.GetLeafCard().DestroyCard(true, true);
		}
		if (childCard != null && childCard.HasParent)
		{
			childCard.RemoveFromParent();
		}
	}

	public void RemoveCardsFromStackPred(GameCard card, int toRemove, Predicate<GameCard> pred)
	{
		List<GameCard> list = card.GetAllCardsInStack().FindAll(pred);
		List<GameCard> allCardsInStack = card.GetAllCardsInStack();
		int num = 0;
		foreach (GameCard gameCard in list)
		{
			if (num == toRemove)
			{
				break;
			}
			allCardsInStack.Remove(gameCard);
			gameCard.RemoveFromStack();
			gameCard.DestroyCard(true, true);
			num++;
		}
		this.Restack(allCardsInStack);
	}

	private void ClearRound()
	{
		this.QuestsCompleted = 0;
		this.NewCardsFound = 0;
		this.MonthTimer = 0f;
		this.BoardMonths = new BoardMonths();
		if (CitiesManager.instance != null)
		{
			CitiesManager.instance.Wellbeing = CitiesManager.instance.WellbeingStart;
			CitiesManager.instance.NextConflictMonth = 0;
		}
		this.GivenCards.Clear();
		this.BoughtBoosterIds.Clear();
		for (int i = this.AllCards.Count - 1; i >= 0; i--)
		{
			if (i <= 0 || i < this.AllCards.Count)
			{
				this.AllCards[i].DestroyCard(false, false);
			}
		}
		for (int j = this.AllBoosters.Count - 1; j >= 0; j--)
		{
			Object.Destroy(this.AllBoosters[j].gameObject);
		}
	}

	public HitText CreateHitText(Vector3 pos, string text, HitText prefab)
	{
		HitText hitText = Object.Instantiate<HitText>(prefab);
		hitText.transform.position = pos;
		hitText.TextMesh.text = text;
		return hitText;
	}

	public void CheckDebugInput()
	{
		if (InputController.instance.GetKeyDown(Key.Period))
		{
			CitiesManager.instance.AddWellbeing(5);
		}
		if (InputController.instance.GetKeyDown(Key.Comma))
		{
			CitiesManager.instance.AddWellbeing(-5);
		}
		if (this.HoveredCard != null && InputController.instance.GetKeyDown(Key.Z))
		{
			GameCard cardWithStatusInStack = this.HoveredCard.GetCardWithStatusInStack();
			if (cardWithStatusInStack != null)
			{
				cardWithStatusInStack.CurrentTimerTime = cardWithStatusInStack.TargetTimerTime;
			}
		}
		if (InputController.instance.GetKeyDown(Key.M))
		{
			AudioManager.me.SkipSong();
		}
		if (InputController.instance.GetKeyDown(Key.U) && this.HoveredCard != null)
		{
			this.HoveredCard.CardData.StatusEffects.Clear();
			this.HoveredCard.CardData.IsDamaged = false;
			this.HoveredCard.CardData.DamageType = CardDamageType.None;
			this.HoveredCard.StatusEffectsChanged();
		}
		if (InputController.instance.GetKeyDown(Key.L) && this.HoveredCard != null)
		{
			BaseVillager card = this.GetCard<BaseVillager>();
			this.HoveredCard.CardAnimations.Add(new CardAnimation_FakeMeleeAttack(this.HoveredCard, card.MyGameCard));
		}
		if (InputController.instance.GetKeyDown(Key.H))
		{
			GameCard hoveredCard = this.HoveredCard;
			Combatable combatable = ((hoveredCard != null) ? hoveredCard.CardData : null) as Combatable;
			if (combatable != null)
			{
				combatable.HealthPoints = combatable.ProcessedCombatStats.MaxHealth;
			}
		}
		if (InputController.instance.GetKeyDown(Key.R) && (this.HoveredCard != null || this.HoveredDraggable != null))
		{
			if (this.HoveredCard != null)
			{
				Combatable combatable2 = this.HoveredCard.CardData as Combatable;
				if (combatable2 != null)
				{
					combatable2.Damage(100);
				}
				else
				{
					this.HoveredCard.DestroyCard(false, true);
				}
			}
			else if (this.HoveredDraggable != null && this.HoveredDraggable is Boosterpack)
			{
				Object.Destroy(this.HoveredDraggable.gameObject);
			}
		}
		if (InputController.instance.GetKeyDown(Key.G) && this.HoveredCard != null)
		{
			if (!this.HoveredCard.CardData.IsFoil)
			{
				this.HoveredCard.CardData.SetFoil();
			}
			else
			{
				this.HoveredCard.CardData.IsFoil = false;
				if (this.HoveredCard.CardData.Value != -1)
				{
					this.HoveredCard.CardData.Value /= 5;
				}
				if (this.HoveredCard.CardData.CitiesValue != -1)
				{
					this.HoveredCard.CardData.CitiesValue /= 5;
				}
			}
		}
		if (InputController.instance.GetKeyDown(Key.F) && this.HoveredCard != null)
		{
			this.CreateFloatingText(this.HoveredCard, true, 5, "Test hovered", Icons.Wellbeing, true, 0, 0f, true);
		}
		if (InputController.instance.GetKeyDown(Key.C) && this.HoveredCard != null)
		{
			CardData cardData = this.CreateCard(this.HoveredCard.transform.position, this.HoveredCard.CardData, true, false, true, true);
			Worker worker = cardData as Worker;
			if (worker != null)
			{
				worker.Housing = null;
			}
			this.StackSendTo(cardData.MyGameCard, this.HoveredCard);
		}
		if (InputController.instance.GetKeyDown(Key.O) && this.HoveredCard != null)
		{
			BaseVillager baseVillager = this.HoveredCard.CardData as BaseVillager;
			if (baseVillager != null)
			{
				baseVillager.Age++;
			}
		}
	}

	private bool SpiritDLCInstalled()
	{
		if (Application.isEditor)
		{
			return DebugOptions.Default.SpiritDlcEnabled;
		}
		if (SteamManager.Initialized && SteamApps.BIsDlcInstalled(new AppId_t(2446110U)))
		{
			Debug.Log("Load Spirit DLC");
			return true;
		}
		return false;
	}

	private bool CitiesDLCInstalled()
	{
		if (Application.isEditor)
		{
			return DebugOptions.Default.CitiesDlcEnabled;
		}
		if (SteamManager.Initialized && SteamApps.BIsDlcInstalled(new AppId_t(2867570U)))
		{
			Debug.Log("Load Cities DLC");
			return true;
		}
		return false;
	}

	public bool IsSpiritDlcActive()
	{
		return this.GameDataLoader.SpiritDlcLoaded;
	}

	public bool IsCitiesDlcActive()
	{
		return this.GameDataLoader.CitiesDlcLoaded;
	}

	public bool CurseIsActive(CurseType curseType)
	{
		foreach (Curse curse in this.ActiveCurses)
		{
			if (curse.MyGameCard.MyBoard.IsCurrent && curse.CurseType == curseType)
			{
				return true;
			}
		}
		return false;
	}

	public void RemoveAllCardsFromBoard(string boardId, bool removeBoosters = true)
	{
		foreach (GameCard gameCard in this.GetAllCardsOnBoard(boardId))
		{
			gameCard.DestroyCard(false, true);
		}
		if (removeBoosters)
		{
			this.RemoveAllBoostersFromBoard(boardId);
		}
	}

	public void RemoveAllBoostersFromBoard(string boardId)
	{
		foreach (Boosterpack boosterpack in this.GetAllBoostersOnBoard(boardId))
		{
			Object.Destroy(boosterpack.gameObject);
		}
	}

	public void CheckSpiritCutscenes()
	{
		if (this.CurseIsActive(CurseType.Happiness) && this.CurrentBoard.Id == "happiness")
		{
			int happinessCount = this.GetHappinessCount(true, true);
			if (happinessCount >= 5)
			{
				this.QueueCutsceneIfNotPlayed("happiness_middle");
			}
			if (happinessCount >= 10)
			{
				this.QueueCutsceneIfNotPlayed("happiness_end");
			}
		}
		if (this.CurseIsActive(CurseType.Death) && this.CurrentBoard.Id == "death")
		{
			if (this.AllBoosterBoxes.Any<BuyBoosterBox>((BuyBoosterBox x) => x.BoosterId == "death_locations" && x.Booster.IsUnlocked))
			{
				this.QueueCutsceneIfNotPlayed("death_end");
			}
		}
	}

	public static WorldManager instance;

	public float CardOverlayOffset = 0.1f;

	public float CollapsedCardOverlayOffset = 0.02f;

	public float CombatOffset = 1f;

	public float HorizonalCombatOffset = 0.2f;

	public float CombatMissOffset = 0.2f;

	public float CardOverlayHeightOffset = 0.001f;

	public float ConflictWidthIncrease;

	public float ConflictHeightIncrease;

	public float ConflictArrowLengthDecrease = 0.2f;

	private List<ParticleSystem> smokeBuffer = new List<ParticleSystem>();

	private List<ParticleSystem> energyMinusBuffer = new List<ParticleSystem>();

	private List<ParticleSystem> wellbeingPlusBuffer = new List<ParticleSystem>();

	private List<FloatingStatus> floatingTextBuffer = new List<FloatingStatus>();

	public AnimationCurve CombatYPosition;

	public AnimationCurve CombatFlatPositionCurve;

	public AnimationCurve CombatKnockbackCurve;

	public float CombatSpeed = 5f;

	public List<Draggable> AllDraggables = new List<Draggable>();

	public List<Draggable> PhysicsDraggables = new List<Draggable>();

	public GetComponentCacher<Draggable> DraggableLookup = new GetComponentCacher<Draggable>();

	public GetComponentCacher<Interactable> InteractableLookup = new GetComponentCacher<Interactable>();

	public GetComponentCacher<Hoverable> HoverableLookup = new GetComponentCacher<Hoverable>();

	public InputSystem Input { get; private set; }
	public Draggable HoveredDraggable { get => Input.HoveredDraggable; set => Input.HoveredDraggable = value; }
	public Draggable DraggingDraggable { get => Input.DraggingDraggable; set => Input.DraggingDraggable = value; }
	public Interactable HoveredInteractable { get => Input.HoveredInteractable; set => Input.HoveredInteractable = value; }
	public Hoverable CurrentHoverable { get => Input.CurrentHoverable; set => Input.CurrentHoverable = value; }

	[HideInInspector]
	public List<GameCard> AllCards = new List<GameCard>();

	[HideInInspector]
	public Dictionary<string, GameCard> UniqueIdToCard = new Dictionary<string, GameCard>();

	[HideInInspector]
	public List<CardTarget> CardTargets = new List<CardTarget>();

	public List<Boosterpack> AllBoosters = new List<Boosterpack>();

	public List<string> BoughtBoosterIds = new List<string>();

	public List<BuyBoosterBox> AllBoosterBoxes = new List<BuyBoosterBox>();

	public Material HitMaterial;

	public ViewType CurrentView = ViewType.Default;

	public List<GameBoard> Boards;

	public TimeSystem Time { get; private set; }
	public EconomySystem Economy { get; private set; }
	public SaveSystem Save { get; private set; }
	public CardQuerySystem CardQuery { get; private set; }
	public CutsceneSystem Cutscene { get; private set; }
	public DayEventSystem DayEvent { get; private set; }
	public float MonthTimer { get => Time.MonthTimer; set => Time.MonthTimer = value; }

	public float AnimationTime;

	public BoardMonths BoardMonths;

	public int OldCurrentMonth { get => Time.OldCurrentMonth; set => Time.OldCurrentMonth = value; }

	public bool DebugScreenOpened;

	public float CardTargetSnapDistance = 0.2f;

	public WorldManager.GameState CurrentGameState;

	public bool CanUseTransport;

	[HideInInspector]
	public Vector3 mouseWorldPosition { get => Input.mouseWorldPosition; set => Input.mouseWorldPosition = value; }

	private RaycastHit[] hits = new RaycastHit[40];

	[HideInInspector]
	public Vector3 grabOffset { get => Input.grabOffset; set => Input.grabOffset = value; }

	public GameDataLoader GameDataLoader;

	private GameDataValidator validator;

	[HideInInspector]
	public bool DebugEndlessMoonEnabled;

	[HideInInspector]
	public bool DebugNoFoodEnabled;

	[HideInInspector]
	public bool ForestMoonEnabled;

	[HideInInspector]
	public bool DebugDontNeedVillagers;

	[HideInInspector]
	public bool DebugNoEnergyEnabled;

	public List<SerializedKeyValuePair> RoundExtraKeyValues = new List<SerializedKeyValuePair>();

	public bool IsLoadingSaveRound { get => Save.IsLoadingSaveRound; set => Save.IsLoadingSaveRound = value; }

	private bool clickStartedGrabbing;

	[HideInInspector]
	public bool CutsceneBoardView;

	private TMP_InputField currentSelectedInput;

	public List<Curse> ActiveCurses = new List<Curse>();

	public List<ActionTimeBase> actionTimeBases = new List<ActionTimeBase>();

	public List<ActionTimeModifier> actionTimeModifiers = new List<ActionTimeModifier>();

	public CardTarget NearbyCardTarget;

	public float SpeedUp { get => Time.SpeedUp; set => Time.SpeedUp = value; }

	public QueuedAnimation currentAnimation { get => Cutscene.currentAnimation; set => Cutscene.currentAnimation = value; }

	private List<QueuedAnimation> queuedAnimations => Cutscene.queuedAnimations;

	private float physicsTimer;

	private float preAutoPauseSpeed;

	private bool isAutoPaused;

	public bool IsShiftDragging;

	public float GridWidth = 0.75f;

	public float GridHeight = 0.85f;

	public float gridAlpha;

	private HashSet<string> doesntCountTowardsCount = new HashSet<string> { "gold", "shell", "happiness", "unhappiness", "pollution" };

	private List<CityHall> townhalls = new List<CityHall>();

	private List<Dollar> dollars = new List<Dollar>();

	private List<Creditcard> creditcards = new List<Creditcard>();

	public bool ShowContinueButton;

	public string ContinueButtonText = "";

	public string CutsceneText = "";

	public string CutsceneTitle = "";

	public bool RemovingCards;

	public bool ConnectConnectors;

	[HideInInspector]
	public bool ContinueClicked;

	[HideInInspector]
	public int ContinueButtonIndex;

	public bool InEatingAnimation;

	public float EndOfMonthSpeedup;

	public bool VillagersStarvedAtEndOfMoon;

	public bool VillagersAngryAtEndOfMoon;

	public Coroutine currentAnimationRoutine { get => Cutscene.currentAnimationRoutine; set => Cutscene.currentAnimationRoutine = value; }

	private WeightedRandomBag<CardChance> chanceBag = new WeightedRandomBag<CardChance>();

	public int QuestsCompleted;

	public int NewCardsFound;

	public RunOptions CurrentRunOptions;

	public RunVariables CurrentRunVariables;

	public List<string> GivenCards = new List<string>();

	public enum GameState
	{
		Playing,
		Paused,
		GameOver,
		InMenu
	}
}

