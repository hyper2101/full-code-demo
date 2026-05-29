using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomButton : Selectable, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler
{
	[HideInInspector]
	public event Action Clicked;

	[HideInInspector]
	public event Action<Vector2> StartDragging;

	public RectTransform RectTransform
	{
		get
		{
			return (RectTransform)base.transform;
		}
	}

	public bool SelectableWithController
	{
		get
		{
			return this.ButtonEnabled && (!(this.parentScreen != null) || (!GameCanvas.instance.ModalIsOpen && (!TransitionScreen.InTransition || TransitionScreen.instance.IsLeaving))) && (this.IsSelectableAction == null || this.IsSelectableAction());
		}
	}

	public event Func<CustomButton, Navigation, Navigation> ExplicitNavigationChanged;

	public bool IsSelected
	{
		get
		{
			if (this._isSelected == null)
			{
				if (InputController.instance.CurrentSchemeIsController && base.currentSelectionState == Selectable.SelectionState.Selected)
				{
					this._isSelected = new bool?(true);
				}
				else
				{
					this._isSelected = new bool?(false);
				}
			}
			return this._isSelected.Value;
		}
	}

	public TextMeshProUGUI TextMeshPro
	{
		get
		{
			if (!this.triedFindingTmPro)
			{
				this.tmPro = base.GetComponent<TextMeshProUGUI>();
				if (this.tmPro == null)
				{
					this.tmPro = base.GetComponentInChildren<TextMeshProUGUI>(true);
				}
				this.triedFindingTmPro = true;
			}
			return this.tmPro;
		}
	}

	protected override void Awake()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		this.Image = base.GetComponent<Image>();
		this.rectTransform = base.GetComponent<RectTransform>();
		this.startColor = this.Image.color;
		base.Awake();
	}

	protected override void Start()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		this.parentScreen = GameCanvas.instance.GetParentScreen(this.rectTransform);
		if (this.parentScreen == null && Application.isEditor)
		{
			Debug.LogWarning("No parent screen found for " + base.name);
		}
		base.Start();
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		if (this.canBeClicked)
		{
			this.isDown = true;
		}
		base.OnPointerDown(eventData);
	}

	private bool canBeClicked
	{
		get
		{
			return !(this.parentScreen != null) || !GameCanvas.instance.ModalIsOpen;
		}
	}

	private Camera cam
	{
		get
		{
			return null;
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		if (this.isDown)
		{
			this.lastEventData = eventData;
			if (RectTransformUtility.RectangleContainsScreenPoint(this.rectTransform, eventData.position, this.cam) && this.canBeClicked)
			{
				this.SubmitClick();
			}
			this.isDown = false;
		}
		base.OnPointerUp(eventData);
	}

	public bool WasRightClick
	{
		get
		{
			return this.lastEventData != null && this.lastEventData.button == PointerEventData.InputButton.Right;
		}
	}

	private void HorizontalStick()
	{
		Slider componentInChildren = base.GetComponentInChildren<Slider>();
		if (componentInChildren == null)
		{
			return;
		}
		if (this.HorizontalStickTimer == 0f)
		{
			componentInChildren.value += ((InputController.instance.GetMove().x > 0f) ? 0.05f : (-0.05f));
		}
		this.HorizontalStickTimer += Time.deltaTime;
		if (this.HorizontalStickTimer > 1.15f - Mathf.Abs(InputController.instance.GetMove().x))
		{
			this.HorizontalStickTimer = 0f;
		}
	}

	private void SubmitClick()
	{
		if (this.parentScreen == null)
		{
			this.parentScreen = GameCanvas.instance.GetParentScreen(this.rectTransform);
		}
		bool flag = this.parentScreen == null || GameCanvas.instance.ScreenIsInteractable(this.parentScreen);
		bool flag2 = InputController.instance.IsUsingMouse && InputController.instance.MouseIsDragging;
		if (this.Clicked != null && !TransitionScreen.InTransition && flag && !flag2 && this.ButtonEnabled)
		{
			this.Clicked();
			if (this.CustomSound == null)
			{
				AudioManager.me.PlaySound2D(AudioManager.me.Click, 1f, 0.1f);
				return;
			}
			AudioManager.me.PlaySound2D(new List<AudioClip> { this.CustomSound }, 1f, 0.1f);
		}
	}

	protected override void OnDisable()
	{
		this.isDown = false;
		base.OnDisable();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
	}

	public bool IsClicked
	{
		get
		{
			bool flag = false;
			if (this.isDown && InputController.instance.InputCount > 0 && RectTransformUtility.RectangleContainsScreenPoint(this.rectTransform, InputController.instance.GetInputPosition(0), this.cam))
			{
				flag = true;
			}
			if (TransitionScreen.InTransition || !this.ButtonEnabled || !GameCanvas.instance.ScreenIsInteractable(this.parentScreen) || (InputController.instance.IsUsingMouse && InputController.instance.MouseIsDragging))
			{
				flag = false;
			}
			return flag;
		}
	}

	public void Update()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (this.tryFindParentScrollRect)
		{
			this.tryFindParentScrollRect = false;
			this.parentScrollRect = base.GetComponentInParent<ScrollRect>();
		}
		if (this.ButtonEnabled && this.IsSelected && InputController.instance != null && InputController.instance.CurrentSchemeIsController && this.SelectableWithController)
		{
			if (InputController.instance.SubmitTriggered())
			{
				this.SubmitClick();
			}
			if (InputController.instance.GetStickHorizontal())
			{
				this.HorizontalStick();
			}
			else
			{
				this.HorizontalStickTimer = 0f;
			}
			if (this.parentScrollRect != null && this.ScrollToInRect)
			{
				this.ScrollToMe();
			}
		}
		if (this.IsHovered)
		{
			Tooltip.Text = this.TooltipText;
		}
		Color color = (this.SetColor ? ColorManager.instance.ButtonColor : this.startColor);
		if ((this.IsHovered || this.IsSelected) && this.ButtonEnabled && this.SetColor)
		{
			color = ColorManager.instance.HoverButtonColor;
		}
		if (this.TextMeshPro != null)
		{
			if (!this.ButtonEnabled)
			{
				this.TextMeshPro.color = ColorManager.instance.DisabledButtonTextColor;
			}
			else
			{
				this.TextMeshPro.color = ColorManager.instance.ButtonTextColor;
			}
			FontStyles fontStyles = this.TextMeshPro.fontStyle;
			if ((this.IsHovered || this.IsSelected) && this.ButtonEnabled && this.EnableUnderline)
			{
				fontStyles |= FontStyles.Underline;
			}
			else
			{
				fontStyles &= ~FontStyles.Underline;
			}
			this.TextMeshPro.fontStyle = fontStyles;
		}
		if (this.SetColor && this.Image != null)
		{
			this.Image.color = color;
		}
		base.interactable = this.ButtonEnabled;
		if (InputController.instance.CurrentSchemeIsController)
		{
			if (!this.SelectableWithController)
			{
				Navigation navigation = base.navigation;
				navigation.mode = Navigation.Mode.None;
				base.navigation = navigation;
				return;
			}
			Navigation navigation2 = base.navigation;
			if (this.ExplicitNavigationChanged != null && this.IsSelected)
			{
				navigation2.mode = Navigation.Mode.Explicit;
				navigation2.selectOnLeft = base.FindSelectable(Vector3.left);
				navigation2.selectOnRight = base.FindSelectable(Vector3.right);
				navigation2.selectOnUp = base.FindSelectable(Vector3.up);
				navigation2.selectOnDown = base.FindSelectable(Vector3.down);
				navigation2 = this.ExplicitNavigationChanged(this, navigation2);
			}
			else
			{
				navigation2.mode = Navigation.Mode.Automatic;
			}
			base.navigation = navigation2;
		}
	}

	public void ScrollToMe()
	{
		GameCanvas.SetScrollRectPosition(this.parentScrollRect, this.rectTransform, true);
	}

	private void LateUpdate()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (this.IsSelected && (!this.SelectableWithController || !this.ButtonEnabled) && InputController.instance.CurrentSchemeIsController)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
		this._isSelected = null;
	}

	protected override void DoStateTransition(Selectable.SelectionState state, bool instant)
	{
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (this.StartDragging != null)
		{
			this.StartDragging(eventData.position);
			return;
		}
		this.isDown = false;
		ScrollRect componentInParent = base.GetComponentInParent<ScrollRect>();
		if (componentInParent == null)
		{
			return;
		}
		componentInParent.OnBeginDrag(eventData);
	}

	public void OnDrag(PointerEventData eventData)
	{
		ScrollRect componentInParent = base.GetComponentInParent<ScrollRect>();
		if (componentInParent == null)
		{
			return;
		}
		componentInParent.OnDrag(eventData);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		ScrollRect componentInParent = base.GetComponentInParent<ScrollRect>();
		if (componentInParent == null)
		{
			return;
		}
		componentInParent.OnEndDrag(eventData);
	}

	public void OnInitializePotentialDrag(PointerEventData eventData)
	{
	}

	public void HardSetText(string text)
	{
		base.GetComponentInChildren<TextMeshProUGUI>().text = text;
	}

	public Image Image;

	public bool EnableUnderline = true;

	public bool ButtonEnabled = true;

	private RectTransform rectTransform;

	private bool isDown;

	public Func<bool> IsSelectableAction;

	public MewtationsScreen parentScreen;

	public bool SetColor = true;

	public AudioClip CustomSound;

	private Color startColor;

	public string TooltipText;

	private ScrollRect parentScrollRect;

	private bool? _isSelected;

	private bool triedFindingTmPro;

	private TextMeshProUGUI tmPro;

	private PointerEventData lastEventData;

	private float HorizontalStickTimer;

	public bool IsHovered;

	public bool ScrollToInRect = true;

	private bool tryFindParentScrollRect = true;
}
