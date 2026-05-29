using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CutsceneScreen : MewtationsScreen
{
	public override bool IsFrameRateUncapped
	{
		get
		{
			return true;
		}
	}

	private void Awake()
	{
		CutsceneScreen.instance = this;
		this.GenerateWellbeingBarLines();
	}

	private void GenerateWellbeingBarLines()
	{
		CityState[] array = (CityState[])Enum.GetValues(typeof(CityState));
		float num = 50f;
		for (int i = 0; i < array.Length - 1; i++)
		{
			LayoutElement layoutElement = new GameObject
			{
				name = "Spacer"
			}.AddComponent<LayoutElement>();
			layoutElement.transform.SetParentClean(this.WellbeingLineParent);
			float num2 = ((float)array[i + 1] - (float)array[i]) / num * 10f;
			layoutElement.flexibleWidth = num2;
			if (i < array.Length - 2)
			{
				Object.Instantiate<LayoutElement>(PrefabManager.instance.WellbeingLinePrefab).transform.SetParentClean(this.WellbeingLineParent);
			}
		}
	}

	private void OnEnable()
	{
		this.Background.rectTransform.anchoredPosition = new Vector2(20f, -200f);
		this.ContinueButton.Clicked += delegate
		{
			WorldManager.instance.ContinueClicked = true;
		};
	}

	public void ClearMultipleOptions()
	{
		foreach (CustomButton customButton in this.multipleButtons)
		{
			Object.Destroy(customButton.gameObject);
		}
		this.multipleButtons.Clear();
	}

	public void EnableWellbeingBar(int wellbeingAmount)
	{
		this.IsEndOfMonthCutscene = true;
		this.wellbeingCounter = (float)wellbeingAmount;
		this.WellbeingAmount = wellbeingAmount;
		this.WellbeingFill.fillAmount = (float)wellbeingAmount / 50f;
		this.WellbeingFill2.fillAmount = (float)wellbeingAmount / 50f;
		this.WellbeingFillText.text = this.wellbeingCounter.ToString();
	}

	public void CreateMultipleOptions(params string[] options)
	{
		for (int i = 0; i < options.Length; i++)
		{
			string text = options[i];
			int cap = i;
			CustomButton customButton = Object.Instantiate<CustomButton>(PrefabManager.instance.CutsceneButtonPrefab);
			customButton.transform.SetParentClean(this.MultipleButtonsParent);
			customButton.HardSetText(text);
			customButton.Clicked += delegate
			{
				WorldManager.instance.ContinueButtonIndex = cap;
				WorldManager.instance.ContinueClicked = true;
			};
			this.multipleButtons.Add(customButton);
		}
	}

	private void Update()
	{
		this.Crosshair.gameObject.SetActive(InputController.instance.CurrentSchemeIsController && (WorldManager.instance.RemovingCards || this.CanMoveScreen || WorldManager.instance.ConnectConnectors));
		GameObject gameObject = this.WellbeingBar.gameObject;
		GameBoard currentBoard = WorldManager.instance.CurrentBoard;
		gameObject.SetActive(((currentBoard != null) ? currentBoard.Id : null) == "cities" && this.IsEndOfMonthCutscene && WorldManager.instance.currentAnimationRoutine != null);
		if (this.WellbeingBar.gameObject.activeSelf)
		{
			this.WellbeingStateText.text = CitiesManager.GetCityStateTranslated(CitiesManager.GetCityStateForWellbeing(this.WellbeingAmount));
			float num = (float)this.WellbeingAmount / 50f;
			if ((double)Mathf.Abs(this.fillVelo2) < 0.001 && (double)Mathf.Abs(this.fillVelo) < 0.001)
			{
				if (num > this.WellbeingFill.fillAmount)
				{
					this.WellbeingFill.color = ColorManager.instance.FloatingTextColorSuccess;
					this.isPossitive = true;
				}
				else
				{
					this.WellbeingFill.color = ColorManager.instance.FloatingTextColorFailed;
					this.isPossitive = false;
				}
			}
			if (!this.isPossitive)
			{
				this.WellbeingFill2.fillAmount = FRILerp.Spring(this.WellbeingFill2.fillAmount, num, 2f, 5f, ref this.fillVelo2);
			}
			else
			{
				this.WellbeingFill.fillAmount = FRILerp.Spring(this.WellbeingFill.fillAmount, num, 2f, 5f, ref this.fillVelo);
			}
			this.wellbeingCounter = Mathf.Lerp(this.wellbeingCounter, (float)this.WellbeingAmount, Time.deltaTime * 2f);
			if (Mathf.Abs(this.prevWellbeingCounter - (float)Mathf.RoundToInt(this.wellbeingCounter)) >= 1f)
			{
				AudioManager.me.PlaySound2D(AudioManager.me.WellbeingCounter, 1f, 0.7f);
				this.prevWellbeingCounter = (float)Mathf.RoundToInt(this.wellbeingCounter);
			}
			this.WellbeingFillText.text = Mathf.RoundToInt(this.wellbeingCounter).ToString();
		}
		this.blinkTimer += Time.deltaTime;
		if (this.blinkTimer >= 0.25f)
		{
			this.blinkTimer = 0f;
			this.blink = !this.blink;
		}
		foreach (CustomButton customButton in this.multipleButtons)
		{
			customButton.Image.color = (this.blink ? ColorManager.instance.ButtonColor : this.Color2);
		}
		this.ContinueButton.Image.color = (this.blink ? ColorManager.instance.ButtonColor : this.Color2);
		this.Background.rectTransform.anchoredPosition = Vector3.Lerp(this.Background.rectTransform.anchoredPosition, new Vector2(10f, 10f), Time.deltaTime * 20f);
		this.ContinueButton.TextMeshPro.text = WorldManager.instance.ContinueButtonText;
		if (this.ContinueButton.gameObject.activeInHierarchy && (InputController.instance.GetKeyDown(Key.Space) || InputController.instance.SubmitTriggered()))
		{
			WorldManager.instance.ContinueClicked = true;
		}
		this.CheckAdvisorCutscene();
	}

	public void CheckAdvisorCutscene()
	{
		CardData card = WorldManager.instance.GetCard("city_advisor");
		if (this.IsAdvisorCutscene && card == null)
		{
			Vector3 vector = GameCamera.instance.ScreenPosToWorldPos(new Vector2((float)Screen.width, (float)Screen.height) * 0.5f);
			WorldManager.instance.CreateCard(vector, "city_advisor", true, false, true);
			WorldManager.instance.CreateSmoke(vector);
		}
		if (!this.IsAdvisorCutscene && card != null)
		{
			card.MyGameCard.DestroyCard(true, true);
		}
	}

	private bool IsPronounced(char c)
	{
		return c != ' ' && c != '.' && c != ',';
	}

	private void LateUpdate()
	{
		this.TitleText.text = WorldManager.instance.CutsceneTitle ?? "";
		this.StatusText.gameObject.SetActive(!string.IsNullOrEmpty(WorldManager.instance.CutsceneText));
		if (this.IsAdvisorCutscene)
		{
			if (this.lastCutsceneText != WorldManager.instance.CutsceneText)
			{
				this.index = 0;
			}
			char c = ((this.index < WorldManager.instance.CutsceneText.Length) ? WorldManager.instance.CutsceneText[this.index] : ' ');
			this.timer += Time.deltaTime;
			if (this.timer >= 0.02f)
			{
				this.index++;
				this.timer = 0f;
				CardData card = WorldManager.instance.GetCard("city_advisor");
				if (card != null)
				{
					if (this.index >= WorldManager.instance.CutsceneText.Length)
					{
						card.MyGameCard.ZRotOffset = 0f;
					}
					else if (this.index % 4 == 0)
					{
						if (this.IsPronounced(c))
						{
							card.MyGameCard.transform.localScale *= 0.98f;
							if (card.MyGameCard.ZRotOffset == 5f)
							{
								card.MyGameCard.ZRotOffset = -5f;
							}
							else
							{
								card.MyGameCard.ZRotOffset = 5f;
							}
						}
						else
						{
							card.MyGameCard.ZRotOffset = 0f;
						}
					}
				}
			}
		}
		else
		{
			this.index = 99999;
		}
		this.StatusText.maxVisibleCharacters = this.index;
		this.StatusText.text = WorldManager.instance.CutsceneText;
		this.ContinueButton.gameObject.SetActive(WorldManager.instance.ShowContinueButton);
		this.ButtonSpacer.gameObject.SetActive(this.ContinueButton.gameObject.activeInHierarchy);
		this.lastCutsceneText = WorldManager.instance.CutsceneText;
	}

	public static CutsceneScreen instance;

	public TextMeshProUGUI TitleText;

	public TextMeshProUGUI StatusText;

	public Image Background;

	private List<CustomButton> multipleButtons = new List<CustomButton>();

	public CustomButton ContinueButton;

	public RectTransform ButtonSpacer;

	public RectTransform MultipleButtonsParent;

	public RectTransform WellbeingBar;

	public Image WellbeingFill;

	public Image WellbeingFill2;

	public Image WellbeingBackground;

	public float TargetWellbeingFillAmount;

	public TextMeshProUGUI WellbeingFillText;

	public TextMeshProUGUI WellbeingStateText;

	public int WellbeingAmount;

	public RectTransform WellbeingLineParent;

	public bool CanMoveScreen;

	public bool IsEndOfMonthCutscene;

	public bool IsAdvisorCutscene;

	private bool blink;

	private float blinkTimer;

	public Image Crosshair;

	public Color Color2;

	private float fillVelo;

	private float fillVelo2;

	private bool isPossitive;

	private float wellbeingCounter;

	private float prevWellbeingCounter;

	private float timer;

	private int index;

	private string lastCutsceneText;
}
