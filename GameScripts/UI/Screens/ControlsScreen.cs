using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlsScreen : MewtationsScreen
{
	public bool IsRebinding
	{
		get
		{
			return this.RebindInfo != null;
		}
	}

	private void Awake()
	{
		ControlsScreen.instance = this;
		this.BackButton.Clicked += delegate
		{
			GameCanvas.instance.SetScreen<OptionsScreen>();
		};
		this.LoadRebinds();
	}

	private void OnEnable()
	{
		List<Transform> list = new List<Transform>();
		foreach (object obj in this.RebindElementsParent)
		{
			Transform transform = (Transform)obj;
			list.Add(transform);
		}
		foreach (Transform transform2 in list)
		{
			Object.Destroy(transform2.gameObject);
		}
		this.CreateRebindElements();
	}

	private void CreateRebindElements()
	{
		this.MakeLabel(MewtationsLoc.Translate("label_keyboard_mouse"));
		this.CreateElementsForScheme("Keyboard&Mouse");
		this.MakeLabel(MewtationsLoc.Translate("label_controller"));
		this.CreateElementsForScheme("Gamepad");
	}

	private void MakeLabel(string s)
	{
		RectTransform rectTransform = Object.Instantiate<RectTransform>(PrefabManager.instance.NormalLabelPrefab);
		rectTransform.SetParentClean(this.RebindElementsParent);
		TextMeshProUGUI componentInChildren = rectTransform.GetComponentInChildren<TextMeshProUGUI>();
		componentInChildren.text = s;
		componentInChildren.fontSize = 28f;
	}

	private void CreateElementsForScheme(string scheme)
	{
		using (IEnumerator<InputAction> enumerator = InputController.instance.PlayerInput.actions.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				InputAction action = enumerator.Current;
				if (!(action.actionMap.name == "UI") && this.ActionSupportsScheme(action, scheme) && !this.ExcludedControls.Any<ExcludedControl>((ExcludedControl x) => x.ActionName == action.name && x.Scheme == scheme))
				{
					RebindElement rebindElement = Object.Instantiate<RebindElement>(PrefabManager.instance.RebindElementPrefab);
					rebindElement.transform.SetParentClean(this.RebindElementsParent);
					rebindElement.MyAction = action.name;
					rebindElement.Scheme = scheme;
				}
			}
		}
	}

	private bool ActionSupportsScheme(InputAction action, string scheme)
	{
		for (int i = 0; i < action.bindings.Count; i++)
		{
			InputBinding inputBinding = action.bindings[i];
			if (inputBinding.isComposite)
			{
				if (action.bindings[i + 1].groups.Contains(scheme))
				{
					return true;
				}
			}
			else if (inputBinding.groups.Contains(scheme))
			{
				return true;
			}
		}
		return false;
	}

	private void Update()
	{
		string text = "";
		if (this.RebindInfo != null && this.RebindInfo.Action.bindings[this.RebindInfo.BindingIndex].isPartOfComposite)
		{
			text = MewtationsLoc.Translate("label_binding", new LocParam[] { LocParam.Create("control", this.RebindInfo.Action.bindings[this.RebindInfo.BindingIndex].name) });
		}
		if (string.IsNullOrEmpty(text))
		{
			this.WaitingForInputText.text = MewtationsLoc.Translate("label_waiting_for_input");
		}
		else
		{
			this.WaitingForInputText.text = text + "\n" + MewtationsLoc.Translate("label_waiting_for_input");
		}
		this.WaitingForInputText.transform.parent.gameObject.SetActive(this.IsRebinding);
	}

	public void SaveRebinds()
	{
		string text = InputController.instance.PlayerInput.actions.SaveBindingOverridesAsJson();
		PlayerPrefs.SetString("rebinds", text);
	}

	private void LoadRebinds()
	{
		string @string = PlayerPrefs.GetString("rebinds");
		InputController.instance.PlayerInput.actions.LoadBindingOverridesFromJson(@string, true);
	}

	public static ControlsScreen instance;

	public CustomButton BackButton;

	public TextMeshProUGUI WaitingForInputText;

	public RectTransform RebindElementsParent;

	public List<ExcludedControl> ExcludedControls = new List<ExcludedControl>();

	public RebindInfo RebindInfo;
}
