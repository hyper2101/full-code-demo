using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardopediaScreen : SokScreen
{
	private void Awake()
	{
		CardopediaScreen.instance = this;
		this.tabButtons = new List<CustomButton> { this.All, this.Main, this.Island, this.Forest, this.Order, this.Spirit, this.Cities, this.Modded };
		this.BackButton.Clicked += delegate
		{
			this.CardopediaBackground.gameObject.SetActive(false);
			this.ClearScreen();
			if (WorldManager.instance.CurrentGameState == WorldManager.GameState.InMenu)
			{
				GameCanvas.instance.SetScreen<MainMenu>();
				return;
			}
			GameCanvas.instance.SetScreen<PauseScreen>();
		};
		this.SearchField.onValueChanged.AddListener(delegate(string value)
		{
			this.FilterEntries();
			foreach (ExpandableLabelCardopedia expandableLabelCardopedia in this.labels)
			{
				if (this.GetActiveLabelChildrenCount(expandableLabelCardopedia) > 0 && !string.IsNullOrEmpty(value))
				{
					expandableLabelCardopedia.SetExpanded(true);
					expandableLabelCardopedia.ShowChildrenCardopedia();
				}
			}
		});
		SokLoc.instance.LanguageChanged += this.Instance_LanguageChanged;
		this.AddTabListeners();
		this.CardopediaBackground = GameCamera.instance.transform.Find("CardopediaBackground");
		this.TargetCardPos = GameCamera.instance.transform.Find("TargetCardPos");
		this.CardopediaBackground.gameObject.SetActive(false);
		this.CreateEntries();
		if (!PlatformHelper.HasModdingSupport)
		{
			this.Modded.gameObject.SetActive(false);
		}
	}

	private void OnDestroy()
	{
		if (SokLoc.instance != null)
		{
			SokLoc.instance.LanguageChanged -= this.Instance_LanguageChanged;
		}
	}

	private void Instance_LanguageChanged()
	{
		foreach (CardopediaEntryElement cardopediaEntryElement in this.entries)
		{
			cardopediaEntryElement.UpdateText();
		}
		if (this.demoCard != null)
		{
			this.demoCard.CardData.OnLanguageChange();
		}
		this.UpdateLabels();
	}

	public void RefreshCardopedia()
	{
		foreach (CardopediaEntryElement cardopediaEntryElement in this.entries)
		{
			cardopediaEntryElement.SetCardData(cardopediaEntryElement.MyCardData);
			cardopediaEntryElement.UpdateText();
		}
		this.UpdateLabels();
	}

	private void OnEnable()
	{
		this.RefreshCardopedia();
		this.CardDescription.transform.parent.gameObject.SetActive(false);
		this.CardopediaBackground.gameObject.SetActive(true);
		this.totalFoundCount = this.DetermineFoundCount(null);
		this.SwitchActiveTab(this.All);
		this.ScrollRect.verticalNormalizedPosition = 1f;
	}

	private int DetermineFoundCount(CardUpdateType? updateType = null)
	{
		List<string> foundCardIds = WorldManager.instance.CurrentSave.FoundCardIds;
		HashSet<string> hashSet = new HashSet<string>();
		foreach (string text in foundCardIds)
		{
			if (!hashSet.Contains(text))
			{
				hashSet.Add(text);
			}
		}
		int num = 0;
		List<CardData> list = WorldManager.instance.CardDataPrefabs;
		if (updateType != null)
		{
			list = list.Where<CardData>(delegate(CardData x)
			{
				CardUpdateType cardUpdateType = x.CardUpdateType;
				CardUpdateType? updateType2 = updateType;
				return (cardUpdateType == updateType2.GetValueOrDefault()) & (updateType2 != null);
			}).ToList<CardData>();
		}
		foreach (CardData cardData in list)
		{
			if (!cardData.HideFromCardopedia && hashSet.Contains(cardData.Id))
			{
				num++;
			}
		}
		return num;
	}

	public bool IsSearching
	{
		get
		{
			return !string.IsNullOrEmpty(this.SearchField.text);
		}
	}

	private void AddTabListeners()
	{
		if (this.activeTab == null)
		{
			this.SwitchActiveTab(this.All);
		}
		using (List<CustomButton>.Enumerator enumerator = this.tabButtons.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				CustomButton tab = enumerator.Current;
				tab.Clicked += delegate
				{
					this.SwitchActiveTab(tab);
				};
				tab.ExplicitNavigationChanged += delegate(CustomButton but, Navigation nav)
				{
					nav.selectOnUp = null;
					nav.selectOnDown = this.GetFirstSelectableInList();
					return nav;
				};
			}
		}
	}

	private Selectable GetFirstSelectableInList()
	{
		return this.labels.FirstOrDefault<ExpandableLabelCardopedia>((ExpandableLabelCardopedia x) => x.gameObject.activeInHierarchy).MyButton;
	}

	private void SwitchActiveTab(CustomButton tab)
	{
		this.activeTab = tab;
		this.ScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
		this.SearchDisabled = false;
		this.CardFoundAmount.gameObject.SetActive(true);
		if (this.activeTab == this.All)
		{
			this.activeCardUpdateType = null;
			this.FilterEntriesCardUpdateType(null);
		}
		else if (this.activeTab == this.Main)
		{
			this.activeCardUpdateType = new CardUpdateType?(CardUpdateType.Main);
			this.FilterEntriesCardUpdateType(new CardUpdateType?(CardUpdateType.Main));
		}
		else if (this.activeTab == this.Island)
		{
			this.activeCardUpdateType = new CardUpdateType?(CardUpdateType.Island);
			this.FilterEntriesCardUpdateType(new CardUpdateType?(CardUpdateType.Island));
		}
		else if (this.activeTab == this.Forest)
		{
			this.activeCardUpdateType = new CardUpdateType?(CardUpdateType.Forest);
			this.FilterEntriesCardUpdateType(new CardUpdateType?(CardUpdateType.Forest));
		}
		else if (this.activeTab == this.Order)
		{
			this.activeCardUpdateType = new CardUpdateType?(CardUpdateType.Order);
			this.FilterEntriesCardUpdateType(new CardUpdateType?(CardUpdateType.Order));
		}
		else if (this.activeTab == this.Spirit)
		{
			this.activeCardUpdateType = new CardUpdateType?(CardUpdateType.Spirit);
			this.FilterEntriesCardUpdateType(new CardUpdateType?(CardUpdateType.Spirit));
			if (!WorldManager.instance.IsSpiritDlcActive())
			{
				this.SetTempDemoCard(WorldManager.instance.CardDataPrefabs.Find((CardData x) => x.Id == "card_display_spirit_dlc"));
				this.ScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
				this.SearchDisabled = true;
				this.CardFoundAmount.gameObject.SetActive(false);
			}
		}
		else if (this.activeTab == this.Cities)
		{
			this.activeCardUpdateType = new CardUpdateType?(CardUpdateType.Cities);
			this.FilterEntriesCardUpdateType(new CardUpdateType?(CardUpdateType.Cities));
			if (!WorldManager.instance.IsCitiesDlcActive())
			{
				this.SetTempDemoCard(WorldManager.instance.CardDataPrefabs.Find((CardData x) => x.Id == "display_2000_dlc"));
				this.ScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
				this.SearchDisabled = true;
				this.CardFoundAmount.gameObject.SetActive(false);
			}
		}
		else if (this.activeTab == this.Modded)
		{
			this.activeCardUpdateType = new CardUpdateType?(CardUpdateType.Mod);
			this.FilterEntriesCardUpdateType(new CardUpdateType?(CardUpdateType.Mod));
		}
		this.currentTotalCardCount = WorldManager.instance.CardDataPrefabs.Count<CardData>((CardData x) => !x.HideFromCardopedia);
		if (this.activeTab != this.All)
		{
			this.currentTotalCardCount = WorldManager.instance.CardDataPrefabs.Count<CardData>(delegate(CardData x)
			{
				if (!x.HideFromCardopedia)
				{
					CardUpdateType cardUpdateType = x.CardUpdateType;
					CardUpdateType? cardUpdateType2 = this.activeCardUpdateType;
					return (cardUpdateType == cardUpdateType2.GetValueOrDefault()) & (cardUpdateType2 != null);
				}
				return false;
			});
			this.SearchField.text = "";
		}
	}

	private void FilterEntriesCardUpdateType(CardUpdateType? cardUpdateType)
	{
		foreach (CardopediaEntryElement cardopediaEntryElement in this.entries)
		{
			if (cardUpdateType != null)
			{
				CardUpdateType cardUpdateType2 = cardopediaEntryElement.MyCardData.CardUpdateType;
				CardUpdateType? cardUpdateType3 = cardUpdateType;
				if (!((cardUpdateType2 == cardUpdateType3.GetValueOrDefault()) & (cardUpdateType3 != null)))
				{
					cardopediaEntryElement.IsFilteredUpdate = false;
					continue;
				}
			}
			cardopediaEntryElement.IsFilteredUpdate = true;
		}
		this.UpdateLabels();
		this.UpdateEntries();
	}

	private void FilterEntries()
	{
		string text = this.SearchField.text;
		if (!string.IsNullOrEmpty(text))
		{
			if (this.activeTab != this.All)
			{
				this.SwitchActiveTab(this.All);
			}
			text = text.ToLower().Replace(" ", "");
			using (List<CardopediaEntryElement>.Enumerator enumerator = this.entries.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					CardopediaEntryElement cardopediaEntryElement = enumerator.Current;
					if (cardopediaEntryElement.MyCardData.Name.ToLower().Replace(" ", "").Contains(text))
					{
						cardopediaEntryElement.IsFiltered = true;
					}
					else
					{
						cardopediaEntryElement.IsFiltered = false;
					}
				}
				goto IL_00E6;
			}
		}
		foreach (CardopediaEntryElement cardopediaEntryElement2 in this.entries)
		{
			cardopediaEntryElement2.IsFiltered = true;
		}
		IL_00E6:
		this.UpdateLabels();
	}

	public void UpdateLabels()
	{
		foreach (ExpandableLabelCardopedia expandableLabelCardopedia in this.labels)
		{
			expandableLabelCardopedia.ShowChildrenCardopedia();
			if (this.IsSearching)
			{
				if (this.GetActiveLabelChildrenCountSearch(expandableLabelCardopedia) > 0)
				{
					CardType type2 = expandableLabelCardopedia.Children[0].MyCardData.MyCardType;
					int num = WorldManager.instance.CardDataPrefabs.Count<CardData>((CardData x) => x.MyCardType == type2 && !x.HideFromCardopedia);
					expandableLabelCardopedia.SetText(this.CardTypeToText(type2) + string.Format(" ({0}/{1})", this.GetActiveLabelChildrenCountSearch(expandableLabelCardopedia), num));
					expandableLabelCardopedia.gameObject.SetActive(true);
					continue;
				}
			}
			else if (expandableLabelCardopedia.Children.Count<CardopediaEntryElement>((CardopediaEntryElement x) => x.IsFilteredUpdate) > 0)
			{
				CardType type = expandableLabelCardopedia.Children[0].MyCardData.MyCardType;
				int num2 = ((this.activeCardUpdateType != null) ? WorldManager.instance.CardDataPrefabs.Count<CardData>(delegate(CardData x)
				{
					if (x.MyCardType == type && !x.HideFromCardopedia)
					{
						CardUpdateType cardUpdateType = x.CardUpdateType;
						CardUpdateType? cardUpdateType2 = this.activeCardUpdateType;
						return (cardUpdateType == cardUpdateType2.GetValueOrDefault()) & (cardUpdateType2 != null);
					}
					return false;
				}) : WorldManager.instance.CardDataPrefabs.Count<CardData>((CardData x) => x.MyCardType == type && !x.HideFromCardopedia));
				expandableLabelCardopedia.SetText(this.CardTypeToText(type) + string.Format(" ({0}/{1})", this.GetActiveLabelChildrenCount(expandableLabelCardopedia), num2));
				expandableLabelCardopedia.gameObject.SetActive(true);
				continue;
			}
			expandableLabelCardopedia.gameObject.SetActive(false);
		}
		this.totalFoundCount = this.DetermineFoundCount(this.activeCardUpdateType);
	}

	private int GetActiveLabelChildrenCountSearch(ExpandableLabelCardopedia label)
	{
		return label.Children.Count<CardopediaEntryElement>((CardopediaEntryElement x) => x.IsFiltered && x.wasFound);
	}

	private int GetActiveLabelChildrenCount(ExpandableLabelCardopedia label)
	{
		return label.Children.Count<CardopediaEntryElement>((CardopediaEntryElement x) => x.IsFilteredUpdate && x.wasFound);
	}

	public void UpdateEntries()
	{
		float verticalNormalizedPosition = this.ScrollRect.verticalNormalizedPosition;
		this.FilterEntries();
		this.UpdatePositions();
		this.ScrollRect.verticalNormalizedPosition = verticalNormalizedPosition;
		this.UpdatePositions();
	}

	private void CreateEntries()
	{
		List<CardData> list = WorldManager.instance.CardDataPrefabs;
		list = (from x in list
			orderby x.MyCardType, x.FullName
			select x).ToList<CardData>();
		list.RemoveAll((CardData x) => x.HideFromCardopedia);
		new List<Transform>();
		foreach (object obj in this.EntriesParent)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		ExpandableLabelCardopedia expandableLabelCardopedia = null;
		this.labels = new List<ExpandableLabelCardopedia>();
		this.entries.Clear();
		this.listChildren.Clear();
		for (int i = 0; i < list.Count; i++)
		{
			CardData c = list[i];
			if (i == 0 || list[i - 1].MyCardType != list[i].MyCardType)
			{
				ExpandableLabelCardopedia label = Object.Instantiate<ExpandableLabelCardopedia>(this.LabelPrefab);
				label.transform.SetParentClean(this.EntriesParent);
				int num = list.Count<CardData>((CardData x) => x.MyCardType == c.MyCardType);
				int num2 = list.Count<CardData>((CardData x) => x.MyCardType == c.MyCardType && WorldManager.instance.CurrentSave.FoundCardIds.Contains(x.Id));
				label.SetText(this.CardTypeToText(list[i].MyCardType) + string.Format(" ({0}/{1})", num2, num));
				label.Tag = list[i].MyCardType;
				label.SetCallback(delegate
				{
					float num3 = -label.transform.localPosition.y - this.EntriesParent.localPosition.y;
					this.UpdateEntries();
					Vector3 localPosition = this.EntriesParent.transform.localPosition;
					localPosition.y = -label.transform.localPosition.y - num3;
					this.EntriesParent.transform.localPosition = localPosition;
				});
				label.SetExpanded(false);
				label.MyButton.ExplicitNavigationChanged += delegate(CustomButton cb, Navigation nav)
				{
					if (cb == this.GetFirstSelectableInList())
					{
						nav.selectOnUp = this.activeTab;
					}
					nav.selectOnLeft = (nav.selectOnRight = null);
					return nav;
				};
				this.listChildren.Add(label);
				this.labels.Add(label);
				expandableLabelCardopedia = label;
			}
			CardopediaEntryElement cardopediaEntryElement = Object.Instantiate<CardopediaEntryElement>(this.CardopediaEntryPrefab);
			cardopediaEntryElement.transform.SetParentClean(this.EntriesParent);
			cardopediaEntryElement.SetCardData(c);
			cardopediaEntryElement.IsEnabled = false;
			cardopediaEntryElement.IsFiltered = false;
			cardopediaEntryElement.IsFilteredUpdate = true;
			cardopediaEntryElement.Button.ExplicitNavigationChanged += delegate(CustomButton cb, Navigation nav)
			{
				nav.selectOnLeft = (nav.selectOnRight = null);
				return nav;
			};
			expandableLabelCardopedia.Children.Add(cardopediaEntryElement);
			this.entries.Add(cardopediaEntryElement);
			this.listChildren.Add(cardopediaEntryElement);
		}
		using (List<ExpandableLabelCardopedia>.Enumerator enumerator2 = this.labels.GetEnumerator())
		{
			while (enumerator2.MoveNext())
			{
				ExpandableLabelCardopedia l = enumerator2.Current;
				if (this.entries.Any<CardopediaEntryElement>((CardopediaEntryElement e) => e.IsNew && e.MyCardData.MyCardType == (CardType)l.Tag))
				{
					l.SetExpanded(true);
				}
			}
		}
	}

	private string CardTypeToText(CardType type)
	{
		return type.TranslateEnum<CardType>();
	}

	private void OnDisable()
	{
		this.SearchField.text = string.Empty;
		this.ClearScreen();
	}

	private void ClearScreen()
	{
		if (this.demoCard != null)
		{
			Object.Destroy(this.demoCard.gameObject);
		}
		this.CardDescription.transform.parent.gameObject.SetActive(false);
		this.lastHoveredEntry = null;
		if (this.CardopediaBackground != null)
		{
			this.CardopediaBackground.gameObject.SetActive(false);
		}
	}

	private void Update()
	{
		this.HoveredEntry = null;
		if (GameCanvas.instance.ScreenIsInteractable<CardopediaScreen>())
		{
			foreach (CardopediaEntryElement cardopediaEntryElement in this.entries)
			{
				if (cardopediaEntryElement.Button.IsHovered || cardopediaEntryElement.Button.IsSelected)
				{
					this.HoveredEntry = cardopediaEntryElement;
				}
			}
		}
		if (this.lastHoveredEntry != null)
		{
			this.lastHoveredEntry.Button.Image.color = ColorManager.instance.ButtonColor;
		}
		if (this.HoveredEntry != null)
		{
			this.HoveredEntry.Button.Image.color = ColorManager.instance.HoverButtonColor;
		}
		this.UpdatePositions();
		if (this.lastHoveredEntry != this.HoveredEntry && this.HoveredEntry != null)
		{
			if (this.demoCard != null)
			{
				Object.Destroy(this.demoCard.gameObject);
			}
			this.demoCard = Object.Instantiate<GameCard>(PrefabManager.instance.GameCardPrefab);
			CardData cardData = Object.Instantiate<CardData>(this.HoveredEntry.MyCardData);
			cardData.transform.SetParent(this.demoCard.transform);
			this.demoCard.CardData = cardData;
			cardData.MyGameCard = this.demoCard;
			this.demoCard.FaceUp = this.HoveredEntry.wasFound;
			this.demoCard.IsDemoCard = true;
			this.demoCard.SetDemoCardRotation();
			this.demoCard.CardData.UpdateCardText();
			this.demoCard.UpdateCardPalette();
			cardData.UpdateCard();
			this.demoCard.ForceUpdate();
		}
		if (this.demoCard != null)
		{
			Vector3 position = this.TargetCardPos.position;
			this.demoCard.transform.position = (this.demoCard.TargetPosition = position);
		}
		if (this.HoveredEntry != null)
		{
			this.CardDescription.transform.parent.gameObject.SetActive(true);
			if (this.HoveredEntry.wasFound)
			{
				this.demoCard.CardData.UpdateCardText();
				string dropSummaryFromCard = this.GetDropSummaryFromCard(this.HoveredEntry.MyCardData);
				string text = this.demoCard.CardData.Description;
				text = text.Replace("\\d", "\n\n");
				if (!string.IsNullOrEmpty(dropSummaryFromCard) && this.HoveredEntry.MyCardData.MyCardType != CardType.Locations)
				{
					text = text + "\n\n" + dropSummaryFromCard;
				}
				Blueprint blueprint = this.HoveredEntry.MyCardData as Blueprint;
				if (blueprint != null)
				{
					text = blueprint.GetText();
				}
				this.CardDescription.text = text;
			}
			else
			{
				this.CardDescription.text = SokLoc.Translate("label_card_not_found");
			}
		}
		this.SearchField.gameObject.SetActive(!InputController.instance.CurrentSchemeIsController && !this.SearchDisabled);
		this.CardFoundAmount.text = SokLoc.Translate("label_cards_found", new LocParam[]
		{
			LocParam.Create("found", this.totalFoundCount.ToString()),
			LocParam.Create("total", this.currentTotalCardCount.ToString())
		});
		this.lastHoveredEntry = this.HoveredEntry;
		this.UpdateTabs();
	}

	private void UpdateTabs()
	{
		foreach (CustomButton customButton in this.tabButtons)
		{
			if (customButton.gameObject.activeInHierarchy)
			{
				bool flag = customButton == this.activeTab;
				Color color;
				if (customButton.IsSelected)
				{
					color = ColorManager.instance.BackgroundColor2;
				}
				else if (flag)
				{
					color = ColorManager.instance.BackgroundColor;
				}
				else
				{
					color = ColorManager.instance.InactiveBackgroundColor;
				}
				customButton.Image.color = color;
			}
		}
	}

	public void UpdatePositions()
	{
		int num = 0;
		Vector2 sizeDelta = this.EntriesParent.sizeDelta;
		ref Vector2 ptr = this.EntriesParent.localPosition;
		Rect rect = this.EntriesParent.rect;
		float height = ((RectTransform)this.EntriesParent.parent).rect.height;
		float num2 = -ptr.y - height * 0.5f;
		for (int i = 0; i < this.listChildren.Count; i++)
		{
			object obj = this.listChildren[i];
			bool flag = false;
			RectTransform rectTransform = null;
			ExpandableLabelCardopedia expandableLabelCardopedia = obj as ExpandableLabelCardopedia;
			if (expandableLabelCardopedia != null)
			{
				flag = expandableLabelCardopedia.gameObject.activeInHierarchy;
				rectTransform = (RectTransform)expandableLabelCardopedia.transform;
			}
			CardopediaEntryElement cardopediaEntryElement = obj as CardopediaEntryElement;
			if (cardopediaEntryElement != null)
			{
				flag = cardopediaEntryElement.IsEnabled;
				rectTransform = (RectTransform)cardopediaEntryElement.transform;
				cardopediaEntryElement.Button.Image.raycastTarget = cardopediaEntryElement.IsEnabled;
			}
			if (flag)
			{
				Vector3 localPosition = rectTransform.localPosition;
				localPosition.x = 0f;
				localPosition.y = (float)(-(float)num) * 50f;
				rectTransform.localPosition = localPosition;
				Vector2 sizeDelta2 = rectTransform.sizeDelta;
				sizeDelta2.x = rect.width;
				rectTransform.sizeDelta = sizeDelta2;
				num++;
			}
			else
			{
				Vector3 vector = new Vector3(1000f, 1000f);
				rectTransform.position = vector;
			}
			CardopediaEntryElement cardopediaEntryElement2 = obj as CardopediaEntryElement;
			if (cardopediaEntryElement2 != null)
			{
				bool flag2 = Mathf.Abs(rectTransform.localPosition.y - num2) < height * 0.75f;
				cardopediaEntryElement2.Cull(!cardopediaEntryElement2.IsEnabled || !flag2);
			}
		}
		sizeDelta.y = (float)num * 50f;
		this.EntriesParent.sizeDelta = sizeDelta;
	}

	private string GetDropSummaryFromCard(CardData cardData)
	{
		if (cardData is Harvestable)
		{
			return BoosterpackData.GetSummaryFromAllCards(cardData.GetPossibleDrops(), "label_can_drop");
		}
		if (cardData is Enemy)
		{
			return BoosterpackData.GetSummaryFromAllCards(cardData.GetPossibleDrops(), "label_can_drop");
		}
		return "";
	}

	private void SetTempDemoCard(CardData data)
	{
		if (this.demoCard != null)
		{
			Object.Destroy(this.demoCard.gameObject);
		}
		this.demoCard = Object.Instantiate<GameCard>(PrefabManager.instance.GameCardPrefab);
		CardData cardData = Object.Instantiate<CardData>(data);
		cardData.transform.SetParent(this.demoCard.transform);
		this.demoCard.CardData = cardData;
		cardData.MyGameCard = this.demoCard;
		this.demoCard.FaceUp = true;
		this.demoCard.IsDemoCard = true;
		this.demoCard.SetDemoCardRotation();
		this.demoCard.UpdateCardPalette();
		cardData.UpdateCard();
		this.demoCard.ForceUpdate();
		this.CardDescription.transform.parent.gameObject.SetActive(true);
		this.demoCard.CardData.UpdateCardText();
		string dropSummaryFromCard = this.GetDropSummaryFromCard(cardData);
		string text = this.demoCard.CardData.Description;
		text = text.Replace("\\d", "\n\n");
		Combatable combatable = cardData as Combatable;
		if (combatable != null)
		{
			text += combatable.GetCombatableDescriptionAdvanced();
		}
		if (!string.IsNullOrEmpty(dropSummaryFromCard) && cardData.MyCardType != CardType.Locations)
		{
			text = text + "\n\n" + dropSummaryFromCard;
		}
		Blueprint blueprint = cardData as Blueprint;
		if (blueprint != null)
		{
			text = blueprint.GetText();
		}
		this.CardDescription.text = text;
	}

	public RectTransform EntriesParent;

	public CustomButton BackButton;

	public ScrollRect ScrollRect;

	public TMP_InputField SearchField;

	public ExpandableLabelCardopedia LabelPrefab;

	public CardopediaEntryElement CardopediaEntryPrefab;

	public CardopediaEntryElement HoveredEntry;

	public TextMeshProUGUI CardFoundAmount;

	public TextMeshProUGUI CardDescription;

	public Transform TargetCardPos;

	public Transform CardopediaBackground;

	public CustomButton All;

	public CustomButton Main;

	public CustomButton Island;

	public CustomButton Forest;

	public CustomButton Order;

	public CustomButton Spirit;

	public CustomButton Cities;

	public CustomButton Modded;

	private List<CustomButton> tabButtons;

	private CustomButton activeTab;

	private CardUpdateType? activeCardUpdateType;

	private List<CardopediaEntryElement> entries = new List<CardopediaEntryElement>();

	private List<ExpandableLabelCardopedia> labels = new List<ExpandableLabelCardopedia>();

	private CardopediaEntryElement lastHoveredEntry;

	private GameCard demoCard;

	private List<object> listChildren = new List<object>();

	public static CardopediaScreen instance;

	private bool SearchDisabled;

	private int totalFoundCount;

	private int currentTotalCardCount;
}
