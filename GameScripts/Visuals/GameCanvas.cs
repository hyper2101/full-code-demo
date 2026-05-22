using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameCanvas : MonoBehaviour
{
	private RectTransform rectTransform
	{
		get
		{
			return base.transform as RectTransform;
		}
	}

	public bool NothingSelected
	{
		get
		{
			return EventSystem.current.currentSelectedGameObject == null || !EventSystem.current.currentSelectedGameObject.activeInHierarchy;
		}
	}

	public GameObject SelectedObject
	{
		get
		{
			return EventSystem.current.currentSelectedGameObject;
		}
	}

	private void Awake()
	{
		GameCanvas.instance = this;
		this.screens = base.GetComponentsInChildren<SokScreen>(true).ToList<SokScreen>();
		this.screenPositions = new List<GameCanvas.ScreenPosition>();
		this.screenInTransition = new List<bool>();
		foreach (SokScreen sokScreen in this.screens)
		{
			this.screenPositions.Add(GameCanvas.ScreenPosition.Bottom);
			this.screenInTransition.Add(false);
		}
		foreach (SokScreen sokScreen2 in this.screens)
		{
			sokScreen2.gameObject.SetActive(true);
			sokScreen2.gameObject.SetActive(false);
		}
		this.Modal.gameObject.SetActive(true);
		this.Modal.gameObject.SetActive(false);
		this.FreeMoveParent.gameObject.SetActive(true);
		this.Transition.gameObject.SetActive(true);
		this.SetScreen<MainMenu>();
	}

	private void Update()
	{
		if ((!(this.CurrentScreen is GameScreen) || GameScreen.instance.ControllerIsInUI) && InputController.instance.CurrentSchemeIsController && this.NothingSelected)
		{
			if (EventSystem.current.currentSelectedGameObject != null)
			{
				bool flag = !EventSystem.current.currentSelectedGameObject.activeInHierarchy;
			}
			this.SelectFirstSelectable(this.CurrentScreen);
		}
		if (InputController.instance.CancelTriggered() && !this.ModalIsOpen)
		{
			SokScreen previousScreen = this.GetPreviousScreen(this.CurrentScreen);
			if (previousScreen != null)
			{
				this.SetScreen(previousScreen);
			}
		}
		this.PerformMouseRaycast();
		this.UpdateMouseOverObject();
		this.UpdateCustomButtonHovered();
	}

	private void UpdateMouseOverObject()
	{
		if (EventSystem.current == null)
		{
			return;
		}
		if (this.raycastResults.Count > 0)
		{
			this.MouseOverObject = this.raycastResults[0].gameObject;
			return;
		}
		this.MouseOverObject = null;
	}

	private void PerformMouseRaycast()
	{
		this.eventDataCurrentPosition.Reset();
		this.eventDataCurrentPosition.position = InputController.instance.ClampedMousePosition();
		this.raycastResults.Clear();
		EventSystem.current.RaycastAll(this.eventDataCurrentPosition, this.raycastResults);
	}

	private void UpdateCustomButtonHovered()
	{
		if (this.previousHovered != null)
		{
			this.previousHovered.IsHovered = false;
		}
		if (!InputController.instance.IsUsingMouse)
		{
			return;
		}
		if (TransitionScreen.InTransition)
		{
			return;
		}
		if (InputController.instance.MouseIsDragging)
		{
			return;
		}
		if (EventSystem.current == null)
		{
			return;
		}
		foreach (RaycastResult raycastResult in this.raycastResults)
		{
			CustomButton component = this.getCustomButtonCacher.GetComponent(raycastResult.gameObject);
			if (!(component == null) && (!(component.parentScreen != null) || !this.ModalIsOpen) && RectTransformUtility.RectangleContainsScreenPoint(component.RectTransform, InputController.instance.ClampedMousePosition(), null) && this.IsSelected(component.RectTransform, this.raycastResults))
			{
				component.IsHovered = true;
				this.previousHovered = component;
				break;
			}
		}
	}

	private SokScreen GetPreviousScreen(SokScreen screen)
	{
		if (screen is CardopediaScreen)
		{
			if (WorldManager.instance.CurrentGameState == WorldManager.GameState.Paused)
			{
				return this.GetScreen<PauseScreen>();
			}
			return this.GetScreen<MainMenu>();
		}
		else
		{
			if (screen is CreditsScreen || screen is SelectResolutionScreen || screen is SelectLanguageScreen || screen is SelectSaveScreen || screen is ControlsScreen || screen is AccessibilityScreen || screen is AdvancedSettingsScreen)
			{
				return this.GetScreen<OptionsScreen>();
			}
			return null;
		}
	}

	public static string FormatTimeLeft(float timeLeft)
	{
		return SokLoc.Translate("label_seconds_left_format", new LocParam[] { LocParam.Create("seconds", timeLeft.ToString("0.0")) });
	}

	public static string FormatTime(float time)
	{
		return SokLoc.Translate("label_seconds_format", new LocParam[] { LocParam.Create("seconds", time.ToString("0.0")) });
	}

	public static string FormatTimeShort(float time)
	{
		return SokLoc.Translate("label_seconds_format", new LocParam[] { LocParam.Create("seconds", time.ToString("0")) });
	}

	public void SetScreen(SokScreen nextScreen)
	{
		if (nextScreen == this.CurrentScreen)
		{
			return;
		}
		SokScreen currentScreen = this.CurrentScreen;
		if (currentScreen != null)
		{
			this.lastSelectedObject[currentScreen.Rect] = EventSystem.current.currentSelectedGameObject;
		}
		this.HandleTransition(this.CurrentScreen, nextScreen);
		this.CurrentScreen = nextScreen;
		foreach (SokScreen sokScreen in this.screens)
		{
			if (!(sokScreen == currentScreen))
			{
				sokScreen.gameObject.SetActive(nextScreen == sokScreen);
			}
		}
		if (this.lastSelectedObject.ContainsKey(nextScreen.Rect))
		{
			GameObject gameObject = this.lastSelectedObject[nextScreen.Rect];
			if (gameObject != null && gameObject.gameObject.activeInHierarchy)
			{
				EventSystem.current.SetSelectedGameObject(gameObject);
			}
		}
		this.SetFrameRateCap(this.CurrentScreen);
	}

	public SokScreen GetScreen<T>() where T : SokScreen
	{
		return this.screens.Find((SokScreen x) => x is T);
	}

	public void SetScreen<T>() where T : SokScreen
	{
		SokScreen screen = this.GetScreen<T>();
		if (screen == this.CurrentScreen)
		{
			return;
		}
		SokScreen currentScreen = this.CurrentScreen;
		if (currentScreen != null)
		{
			this.lastSelectedObject[currentScreen.Rect] = EventSystem.current.currentSelectedGameObject;
		}
		this.HandleTransition(this.CurrentScreen, screen);
		this.CurrentScreen = screen;
		foreach (SokScreen sokScreen in this.screens)
		{
			if (!(sokScreen == currentScreen))
			{
				sokScreen.gameObject.SetActive(screen == sokScreen);
			}
		}
		if (this.lastSelectedObject.ContainsKey(screen.Rect))
		{
			GameObject gameObject = this.lastSelectedObject[screen.Rect];
			if (gameObject != null && gameObject.activeInHierarchy)
			{
				EventSystem.current.SetSelectedGameObject(gameObject);
			}
		}
		this.SetFrameRateCap(this.CurrentScreen);
	}

	private void SetFrameRateCap(SokScreen currentScreen)
	{
		if (!currentScreen.IsFrameRateUncapped)
		{
			Application.targetFrameRate = -1;
			QualitySettings.vSyncCount = 1;
			return;
		}
		int @int = PlayerPrefs.GetInt("framerate", -1);
		Application.targetFrameRate = Mathf.Max(-1, @int);
		if (@int == -1)
		{
			QualitySettings.vSyncCount = 1;
			return;
		}
		QualitySettings.vSyncCount = 0;
	}

	private void HandleTransition(SokScreen prevScreen, SokScreen nextScreen)
	{
		if (!this.HasTransition(prevScreen, nextScreen))
		{
			if (prevScreen != null)
			{
				prevScreen.Rect.DOKill(true);
			}
			nextScreen.Rect.DOKill(true);
			if (prevScreen != null)
			{
				prevScreen.gameObject.SetActive(false);
			}
			nextScreen.gameObject.SetActive(true);
			nextScreen.Rect.anchoredPosition = Vector2.zero;
			return;
		}
		GameCanvas.ScreenPosition screenPosition = GameCanvas.ScreenPosition.Left;
		GameCanvas.ScreenPosition screenPosition2 = GameCanvas.ScreenPosition.Left;
		this.LeaveTo(prevScreen, screenPosition);
		this.EnterFrom(nextScreen, screenPosition2);
	}

	public bool MousePositionIsOverUI()
	{
		return this.raycastResults.Count > 0;
	}

	public bool PositionIsOverUI(Vector2 screenPos)
	{
		if (EventSystem.current == null)
		{
			return false;
		}
		this.eventDataCurrentPosition.Reset();
		this.eventDataCurrentPosition.position = screenPos;
		this.raycastResults.Clear();
		EventSystem.current.RaycastAll(this.eventDataCurrentPosition, this.raycastResults);
		return this.raycastResults.Count > 0;
	}

	public Vector3 ScreenPosToLocalPos(Vector3 pos)
	{
		Vector2 vector;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(this.FreeMoveParent, pos, null, out vector);
		return vector;
	}

	public bool AboveMeOrMyChildrenRaycast(Transform t, Vector2 position)
	{
		if (EventSystem.current == null)
		{
			return false;
		}
		this.eventDataCurrentPosition.Reset();
		this.eventDataCurrentPosition.position = position;
		this.raycastResults.Clear();
		EventSystem.current.RaycastAll(this.eventDataCurrentPosition, this.raycastResults);
		foreach (RaycastResult raycastResult in this.raycastResults)
		{
			if (raycastResult.gameObject.transform.IsChildOf(t) || raycastResult.gameObject.transform == t)
			{
				return true;
			}
		}
		return false;
	}

	public bool AboveMeOrMyChildren(Transform t, GameObject mouseOverObject)
	{
		return !(mouseOverObject == null) && (mouseOverObject.transform.IsChildOf(t) || mouseOverObject.transform == t);
	}

	public static Rect GetWorldRect2(RectTransform rt)
	{
		Vector3[] array = new Vector3[4];
		rt.GetWorldCorners(array);
		Vector3 vector = array[0];
		Vector2 vector2 = array[2] - array[0];
		return new Rect(vector, vector2);
	}

	public Rect GetWorldRect(RectTransform rt)
	{
		return GameCanvas.GetWorldRect2(rt);
	}

	public static void SetScrollRectPosition(ScrollRect scrollRect, RectTransform rectTransform, bool centerInView = false)
	{
		if (centerInView)
		{
			scrollRect.ScrollToCenter(rectTransform, true);
			return;
		}
		Vector2 vector = scrollRect.transform.InverseTransformPoint(scrollRect.content.position);
		Rect worldRect = GameCanvas.instance.GetWorldRect(rectTransform);
		Vector3 vector2 = new Vector3(worldRect.xMin, worldRect.yMax);
		Vector2 vector3 = scrollRect.transform.InverseTransformPoint(vector2);
		Vector2 vector4 = vector - vector3;
		vector4.x = scrollRect.content.anchoredPosition.x;
		scrollRect.content.anchoredPosition = vector4;
		scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
	}

	public void OpenModal()
	{
		this.Modal.gameObject.SetActive(true);
		this.ModalIsOpen = true;
	}

	public void CloseModal()
	{
		this.Modal.gameObject.SetActive(false);
		this.ModalIsOpen = false;
	}

	public void ShowDlcNotInstalledModal()
	{
		ModalScreen.instance.Clear();
		ModalScreen.instance.SetTexts(SokLoc.Translate("label_spirit_dlc_not_installed_title"), SokLoc.Translate("label_spirit_dlc_not_installed_text"));
		ModalScreen.instance.AddOption(SokLoc.Translate("label_okay"), delegate
		{
			this.CloseModal();
		});
		this.OpenModal();
	}

	public void ShowCantChangeBoardSpirit()
	{
		ModalScreen.instance.Clear();
		ModalScreen.instance.SetTexts(SokLoc.Translate("label_change_board_disabled_title"), SokLoc.Translate("label_change_board_disabled_text"));
		ModalScreen.instance.AddOption(SokLoc.Translate("label_okay"), delegate
		{
			this.CloseModal();
		});
		this.OpenModal();
	}

	public void ShowNameCombatableModal(CardData cb, Action onDone)
	{
		ModalScreen.instance.Clear();
		ModalScreen.instance.SetTexts(SokLoc.Translate("label_name_villager_title"), SokLoc.Translate("label_name_villager_text"));
		TMP_InputField input = ModalScreen.instance.AddInputNoButton();
		if (!string.IsNullOrEmpty(cb.CustomName))
		{
			input.text = cb.CustomName;
		}
		else
		{
			input.text = cb.Name;
		}
		input.characterLimit = 12;
		ModalScreen.instance.AddOption(SokLoc.Translate("label_random_name"), delegate
		{
			input.text = this.GetRandomName();
		});
		ModalScreen.instance.AddOption(SokLoc.Translate("label_okay"), delegate
		{
			ProfanityChecker profanityChecker = WorldManager.instance.GameDataLoader.ProfanityChecker;
			string text = input.text;
			if (profanityChecker.IsProfanityInLanguage(SokLoc.instance.CurrentLanguage, text))
			{
				text = "Bobba";
			}
			text = StringUtils.RemoveRichText(text);
			cb.CustomName = text;
			this.CloseModal();
			onDone();
		});
		this.OpenModal();
	}

	public string GetRandomName()
	{
		return WorldManager.instance.GameDataLoader.VillagerNames.Choose<string>();
	}

	public void ShowSimpleModal(string textTerm, string titleTerm)
	{
		ModalScreen.instance.Clear();
		ModalScreen.instance.SetTexts(titleTerm, textTerm);
		ModalScreen.instance.AddOption(SokLoc.Translate("label_okay"), delegate
		{
			this.CloseModal();
		});
		this.OpenModal();
	}

	public void ShowClearSaveModal()
	{
		ModalScreen.instance.Clear();
		ModalScreen.instance.SetTexts(SokLoc.Translate("label_clear_save"), SokLoc.Translate("label_clear_save_confirm"));
		ModalScreen.instance.AddOption(SokLoc.Translate("label_yes"), delegate
		{
			TransitionScreen.instance.StartTransition(delegate
			{
				WorldManager.instance.ClearSaveAndRestart();
				this.CloseModal();
			}, 0.2f);
		});
		ModalScreen.instance.AddOption(SokLoc.Translate("label_no"), delegate
		{
			this.CloseModal();
		});
		this.OpenModal();
	}

	public void ShowEarlyAccessModal()
	{
		ModalScreen.instance.Clear();
		string text = SokLoc.Translate("label_youtube_cities_dlc_text", new LocParam[] { LocParam.Create("saveDirectory", PlatformHelper.CurrentSavesDirectory) });
		ModalScreen.instance.SetTexts(SokLoc.Translate("label_youtube_cities_dlc_title"), text);
		ModalScreen.instance.AddOption(SokLoc.Translate("label_load_run"), delegate
		{
			SaveGame saveFromResources = SaveManager.instance.GetSaveFromResources("Saves/2000_BetaSave");
			saveFromResources.SaveId = "4";
			SaveManager.instance.Save(saveFromResources);
			WorldManager.RestartGame();
			this.CloseModal();
		});
		ModalScreen.instance.AddOption(SokLoc.Translate("label_exit"), delegate
		{
			this.CloseModal();
		});
		this.OpenModal();
	}

	public void ShowStartNewRunModal(Action onYes)
	{
		ModalScreen.instance.Clear();
		ModalScreen.instance.SetTexts(SokLoc.Translate("label_start_new_run"), SokLoc.Translate("label_new_run_confirm"));
		ModalScreen.instance.AddOption(SokLoc.Translate("label_yes"), delegate
		{
			this.CloseModal();
			onYes();
		});
		ModalScreen.instance.AddOption(SokLoc.Translate("label_no"), delegate
		{
			this.CloseModal();
		});
		this.OpenModal();
	}

	public void OneVillagerNeedsToStayPrompt(string term)
	{
		ModalScreen.instance.Clear();
		if (WorldManager.instance.CurrentBoard.Id == "main")
		{
			ModalScreen.instance.SetTexts(SokLoc.Translate(term), SokLoc.Translate("label_one_villager_needs_to_stay"));
		}
		else if (WorldManager.instance.CurrentBoard.Id == "island")
		{
			ModalScreen.instance.SetTexts(SokLoc.Translate(term), SokLoc.Translate("label_one_villager_needs_to_stay_island"));
		}
		else
		{
			ModalScreen.instance.SetTexts(SokLoc.Translate(term), SokLoc.Translate("label_one_villager_needs_to_stay"));
		}
		ModalScreen.instance.AddOption(SokLoc.Translate("label_okay"), delegate
		{
			this.CloseModal();
		});
		this.OpenModal();
	}

	public void MaxVillagerCountPrompt(string term, int amount)
	{
		ModalScreen.instance.Clear();
		ModalScreen.instance.SetTexts(SokLoc.Translate(term), SokLoc.Translate("label_max_villager_in_portal", new LocParam[] { LocParam.Create("amount", amount.ToString()) }));
		ModalScreen.instance.AddOption(SokLoc.Translate("label_okay"), delegate
		{
			this.CloseModal();
		});
		this.OpenModal();
	}

	public void NotEnoughFoodToUsePortalPrompt()
	{
		ModalScreen.instance.Clear();
		ModalScreen.instance.SetTexts(SokLoc.Translate("label_taking_portal_title"), SokLoc.Translate("label_not_enough_food_before_portal"));
		ModalScreen.instance.AddOption(SokLoc.Translate("label_okay"), delegate
		{
			this.CloseModal();
		});
		this.OpenModal();
	}

	public void NotEnoughFoodToSailOffPrompt()
	{
		ModalScreen.instance.Clear();
		ModalScreen.instance.SetTexts(SokLoc.Translate("label_sailing_off_title"), SokLoc.Translate("label_not_enough_food_before_sailing"));
		ModalScreen.instance.AddOption(SokLoc.Translate("label_okay"), delegate
		{
			this.CloseModal();
		});
		this.OpenModal();
	}

	public void LeaveSpiritWorldPrompt(Action onYes, Action onNo)
	{
		ModalScreen.instance.Clear();
		ModalScreen.instance.SetTexts(SokLoc.Translate("label_return_from_spirit_world_title"), SokLoc.Translate("label_return_from_spirit_world_text"));
		ModalScreen.instance.AddOption(SokLoc.Translate("label_yes"), delegate
		{
			this.CloseModal();
			onYes();
		});
		ModalScreen.instance.AddOption(SokLoc.Translate("label_no"), delegate
		{
			this.CloseModal();
			onNo();
		});
		this.OpenModal();
	}

	public void AbandonCityPrompt(Action onYes, Action onNo)
	{
		ModalScreen.instance.Clear();
		ModalScreen.instance.SetTexts(SokLoc.Translate("label_abandon_city_title"), SokLoc.Translate("label_abandon_city_text"));
		ModalScreen.instance.AddOption(SokLoc.Translate("label_yes"), delegate
		{
			this.CloseModal();
			onYes();
		});
		ModalScreen.instance.AddOption(SokLoc.Translate("label_no"), delegate
		{
			this.CloseModal();
			Action onNo2 = onNo;
			if (onNo2 == null)
			{
				return;
			}
			onNo2();
		});
		this.OpenModal();
	}

	public void GoToCityPrompt(Action onYes, Action onNo)
	{
		ModalScreen.instance.Clear();
		ModalScreen.instance.SetTexts(SokLoc.Translate("label_go_to_city_title"), SokLoc.Translate("label_go_to_city_text"));
		ModalScreen.instance.AddOption(SokLoc.Translate("label_yes"), delegate
		{
			this.CloseModal();
			onYes();
		});
		ModalScreen.instance.AddOption(SokLoc.Translate("label_no"), delegate
		{
			this.CloseModal();
			Action onNo2 = onNo;
			if (onNo2 == null)
			{
				return;
			}
			onNo2();
		});
		this.OpenModal();
	}

	public void MissingCardsInSavePrompt(Action onYes, HashSet<string> missingCardIds)
	{
		ModalScreen.instance.Clear();
		string text = string.Join(", ", missingCardIds.Take<string>(3));
		if (missingCardIds.Count > 3)
		{
			text = text + " " + SokLoc.Translate("label_missing_cards_more_text", new LocParam[] { LocParam.Create("amount", (missingCardIds.Count - 3).ToString()) });
		}
		ModalScreen.instance.SetTexts(SokLoc.Translate("label_missing_cards_title"), SokLoc.Translate("label_missing_cards_text", new LocParam[] { LocParam.Create("missing", text) }));
		ModalScreen.instance.AddOption(SokLoc.Translate("label_yes"), delegate
		{
			this.CloseModal();
			onYes();
		});
		ModalScreen.instance.AddOption(SokLoc.Translate("label_no"), delegate
		{
			this.CloseModal();
		});
		this.OpenModal();
	}

	public void ChangeLocationPrompt(Action onYes, Action onNo, string destinationBoard)
	{
		string text = "label_return_to_mainland_prompt";
		string text2 = "label_sailing_off_title";
		if (WorldManager.instance.CurrentBoard.Id == "main")
		{
			if (destinationBoard == "island")
			{
				text = "label_go_to_island_prompt";
			}
			else if (destinationBoard == "forest")
			{
				text2 = "label_taking_portal_title";
				text = "label_go_to_forest_prompt";
			}
			else if (destinationBoard == "cities")
			{
				text2 = "label_take_time_machine";
				text = "label_go_to_cities_mainland";
			}
		}
		else if (WorldManager.instance.CurrentBoard.Id == "island")
		{
			if (destinationBoard == "main")
			{
				text = "label_return_to_mainland_prompt";
			}
			else if (destinationBoard == "forest")
			{
				text2 = "label_taking_portal_title";
				text = "label_go_to_forest_prompt";
			}
		}
		else if (WorldManager.instance.CurrentBoard.Id == "cities" && destinationBoard == "main")
		{
			text2 = "label_take_time_machine";
			text = "label_go_to_mainland_cities";
		}
		ModalScreen.instance.Clear();
		ModalScreen.instance.SetTexts(SokLoc.Translate(text2), SokLoc.Translate(text));
		ModalScreen.instance.AddOption(SokLoc.Translate("label_yes"), delegate
		{
			this.CloseModal();
			onYes();
		});
		ModalScreen.instance.AddOption(SokLoc.Translate("label_no"), delegate
		{
			this.CloseModal();
			onNo();
		});
		this.OpenModal();
	}

	private bool HasTransition(SokScreen prevScreen, SokScreen nextScreen)
	{
		return !(prevScreen == null) && !(nextScreen is GameScreen) && !(prevScreen is GameScreen) && !(prevScreen is CreditsScreen) && !(nextScreen is CreditsScreen);
	}

	private float GetEaseDuration(SokScreen screen, bool leaving)
	{
		return 0.25f;
	}

	private static Vector2 GetPosition(GameCanvas.ScreenPosition pos, Vector2 size)
	{
		Vector2 vector;
		if (pos == GameCanvas.ScreenPosition.Left)
		{
			vector = Vector2.left;
		}
		else if (pos == GameCanvas.ScreenPosition.Right)
		{
			vector = Vector2.right;
		}
		else if (pos == GameCanvas.ScreenPosition.Bottom)
		{
			vector = Vector2.down;
		}
		else if (pos == GameCanvas.ScreenPosition.Top)
		{
			vector = Vector2.up;
		}
		else
		{
			if (pos != GameCanvas.ScreenPosition.None)
			{
				throw new Exception(string.Format("Can't get screen position for {0}", pos));
			}
			vector = new Vector2(0f, 0f);
		}
		return new Vector2(vector.x * size.x, vector.y * size.y);
	}

	private void EnterFrom(SokScreen screen, GameCanvas.ScreenPosition pos)
	{
		if (screen == null)
		{
			return;
		}
		RectTransform rect = screen.Rect;
		this.TrackScreenPosition(screen, GameCanvas.ScreenPosition.None);
		float easeDuration = this.GetEaseDuration(screen, false);
		this.screenInTransition[this.ScreenIndex(screen)] = true;
		rect.DOKill(false);
		rect.DOAnchorPos(Vector2.zero, easeDuration, true).From(GameCanvas.GetPosition(pos, this.rectTransform.sizeDelta) * 1.2f, true, false).SetEase(this.easeType)
			.OnComplete(delegate
			{
				this.screenInTransition[this.ScreenIndex(screen)] = false;
			});
	}

	private int ScreenIndex(SokScreen s)
	{
		return this.screens.IndexOf(s);
	}

	private void TrackScreenPosition(SokScreen screen, GameCanvas.ScreenPosition pos)
	{
		if (screen == null)
		{
			return;
		}
		this.screenPositions[this.screens.IndexOf(screen)] = pos;
	}

	private void LeaveTo(SokScreen screen, GameCanvas.ScreenPosition pos)
	{
		RectTransform rect = screen.Rect;
		if (rect == null)
		{
			return;
		}
		this.TrackScreenPosition(screen, pos);
		float easeDuration = this.GetEaseDuration(screen, true);
		this.screenInTransition[this.ScreenIndex(screen)] = true;
		rect.DOKill(false);
		rect.DOAnchorPos(GameCanvas.GetPosition(pos, this.rectTransform.sizeDelta), easeDuration, true).SetEase(this.easeType).OnComplete(delegate
		{
			rect.gameObject.SetActive(false);
			this.screenInTransition[this.ScreenIndex(screen)] = false;
		});
	}

	public void EaseRectTransformPosition(RectTransform rectTransform, GameCanvas.ScreenPosition target, Vector2 size, Action onComplete = null)
	{
		rectTransform.DOKill(false);
		Vector2 position = GameCanvas.GetPosition(target, size);
		rectTransform.DOAnchorPos(position, 0.4f, false).SetEase(this.easeType).OnComplete(delegate
		{
			Action onComplete2 = onComplete;
			if (onComplete2 == null)
			{
				return;
			}
			onComplete2();
		});
	}

	public void SetRectTransformPosition(RectTransform rectTransform, GameCanvas.ScreenPosition target, Vector2 size)
	{
		Vector2 position = GameCanvas.GetPosition(target, size);
		rectTransform.anchoredPosition = position;
	}

	public SokScreen GetParentScreen(RectTransform obj)
	{
		return obj.gameObject.GetComponentInParent<SokScreen>(true);
	}

	private bool ScreenInTransition(SokScreen screen)
	{
		return this.screenInTransition[this.ScreenIndex(screen)];
	}

	public bool AnyScreenInTransition()
	{
		for (int i = 0; i < this.screenInTransition.Count; i++)
		{
			if (this.screenInTransition[i])
			{
				return true;
			}
		}
		return false;
	}

	public bool ScreenIsInteractable(SokScreen screen)
	{
		return this.CurrentScreen == screen && !this.ScreenInTransition(screen);
	}

	public bool ScreenIsInteractable<T>() where T : SokScreen
	{
		SokScreen screen = this.GetScreen<T>();
		return this.CurrentScreen == screen && !this.ScreenInTransition(screen);
	}

	public bool IsSelected(Transform t, Vector2 position)
	{
		if (EventSystem.current == null)
		{
			return false;
		}
		this.eventDataCurrentPosition.Reset();
		this.eventDataCurrentPosition.position = position;
		this.raycastResults.Clear();
		EventSystem.current.RaycastAll(this.eventDataCurrentPosition, this.raycastResults);
		if (this.raycastResults.Count == 0)
		{
			return false;
		}
		RaycastResult raycastResult = this.raycastResults.OrderBy<RaycastResult, int>((RaycastResult x) => x.displayIndex).First<RaycastResult>();
		return raycastResult.gameObject.transform == t || this.IsDeepChildOf(raycastResult.gameObject.transform, t);
	}

	public bool IsSelected(Transform t, List<RaycastResult> raycastResults)
	{
		RaycastResult raycastResult = raycastResults.OrderBy<RaycastResult, int>((RaycastResult x) => x.displayIndex).First<RaycastResult>();
		return raycastResult.gameObject.transform == t || this.IsDeepChildOf(raycastResult.gameObject.transform, t);
	}

	private bool IsDeepChildOf(Transform a, Transform parent)
	{
		Transform transform = a;
		while (transform.parent != null)
		{
			if (transform.parent == parent)
			{
				return true;
			}
			transform = transform.parent;
		}
		return false;
	}

	public void ReselectFirstSelectable()
	{
		if (this.CurrentScreen != null)
		{
			this.SelectFirstSelectable(this.CurrentScreen);
		}
	}

	private void SelectFirstSelectable(SokScreen screen)
	{
		RectTransform rectTransform = screen.Rect;
		if (this.ModalIsOpen)
		{
			rectTransform = ModalScreen.instance.transform as RectTransform;
		}
		if (rectTransform == null)
		{
			return;
		}
		Selectable selectable = rectTransform.GetComponentInChildren<Selectable>();
		if (selectable == null)
		{
			if (Application.isEditor)
			{
				Debug.Log("Could not find a Selectable child in " + rectTransform.name);
			}
			return;
		}
		int num = 0;
		while (selectable.FindSelectableOnUp() != null && num < 100)
		{
			selectable = selectable.FindSelectableOnUp();
			num++;
		}
		if (selectable is CustomButton && !(selectable as CustomButton).SelectableWithController)
		{
			foreach (CustomButton customButton in rectTransform.GetComponentsInChildren<CustomButton>())
			{
				if (customButton.SelectableWithController)
				{
					selectable = customButton;
					break;
				}
			}
		}
		if (selectable.interactable)
		{
			selectable.Select();
			EventSystem.current.SetSelectedGameObject(selectable.gameObject);
			selectable.Select();
		}
	}

	public void SetUIToggle(bool enabled)
	{
		this.Canvas.enabled = enabled;
	}

	public static GameCanvas instance;

	public RectTransform FreeMoveParent;

	public SokScreen CurrentScreen;

	public RectTransform Modal;

	public Canvas Canvas;

	private List<GameCanvas.ScreenPosition> screenPositions;

	private List<bool> screenInTransition;

	private Dictionary<RectTransform, GameObject> lastSelectedObject = new Dictionary<RectTransform, GameObject>();

	private List<SokScreen> screens;

	private List<RaycastResult> raycastResults = new List<RaycastResult>();

	public GameObject Transition;

	public bool ModalIsOpen;

	public GameObject MouseOverObject;

	private CustomButton previousHovered;

	private PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);

	private GetComponentCacher<CustomButton> getCustomButtonCacher = new GetComponentCacher<CustomButton>();

	public Ease easeType = Ease.InOutCubic;

	public enum ScreenPosition
	{
		Left,
		Right,
		Bottom,
		Top,
		None
	}
}
