using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindElement : MonoBehaviour
{
	private void Start()
	{
		InputAction action;
		int bindingIndex;
		if (!this.ResolveActionAndBinding(out action, out bindingIndex))
		{
			return;
		}
		this.SetBindingButton.Clicked += delegate
		{
			if (ControlsScreen.instance.IsRebinding)
			{
				return;
			}
			this.StartRebind();
		};
		this.ResetButton.Clicked += delegate
		{
			if (action.bindings[bindingIndex].isComposite)
			{
				for (int i = bindingIndex + 1; i < action.bindings.Count; i++)
				{
					if (!action.bindings[i].isPartOfComposite)
					{
						break;
					}
					action.RemoveBindingOverride(i);
				}
			}
			else
			{
				action.RemoveBindingOverride(bindingIndex);
			}
			ControlsScreen.instance.SaveRebinds();
		};
	}

	public bool ResolveActionAndBinding(out InputAction action, out int bindingIndex)
	{
		bindingIndex = -1;
		action = InputController.instance.PlayerInput.actions[this.MyAction];
		for (int i = 0; i < action.bindings.Count; i++)
		{
			InputBinding inputBinding = action.bindings[i];
			if (inputBinding.isComposite)
			{
				if (action.bindings[i + 1].groups.Contains(this.Scheme))
				{
					bindingIndex = i;
					return true;
				}
			}
			else if (inputBinding.groups.Contains(this.Scheme))
			{
				bindingIndex = i;
				return true;
			}
		}
		Debug.LogError("No action found for " + this.MyAction + " in " + this.Scheme);
		return false;
	}

	private void StartRebind()
	{
		InputAction inputAction;
		int num;
		if (!this.ResolveActionAndBinding(out inputAction, out num))
		{
			return;
		}
		if (inputAction.bindings[num].isComposite)
		{
			int num2 = num + 1;
			if (num2 < inputAction.bindings.Count && inputAction.bindings[num2].isPartOfComposite)
			{
				this.Rebind(inputAction, num2, true);
				return;
			}
		}
		else
		{
			this.Rebind(inputAction, num, false);
		}
	}

	private void Rebind(InputAction action, int bindingIndex, bool isComposite = false)
	{
		action.Disable();
		InputActionRebindingExtensions.RebindingOperation rebindOperation = action.PerformInteractiveRebinding(bindingIndex).WithCancelingThrough("<Keyboard>/escape").OnMatchWaitForAnother(0.1f)
			.WithControlsExcluding("<Mouse>/leftButton");
		rebindOperation.OnComplete(delegate(InputActionRebindingExtensions.RebindingOperation op)
		{
			if (this.CheckDuplicateBinding(action, bindingIndex))
			{
				action.RemoveBindingOverride(bindingIndex);
			}
			InputActionRebindingExtensions.RebindingOperation rebindOperation3 = rebindOperation;
			if (rebindOperation3 != null)
			{
				rebindOperation3.Dispose();
			}
			rebindOperation = null;
			action.Enable();
			ControlsScreen.instance.RebindInfo = null;
			ControlsScreen.instance.SaveRebinds();
			if (isComposite)
			{
				int num = bindingIndex + 1;
				if (num < action.bindings.Count && action.bindings[num].isPartOfComposite)
				{
					this.Rebind(action, num, true);
				}
			}
			InputController.instance.ClearBindingDisplayCache();
		});
		rebindOperation.OnCancel(delegate(InputActionRebindingExtensions.RebindingOperation op)
		{
			InputActionRebindingExtensions.RebindingOperation rebindOperation2 = rebindOperation;
			if (rebindOperation2 != null)
			{
				rebindOperation2.Dispose();
			}
			rebindOperation = null;
			ControlsScreen.instance.RebindInfo = null;
			action.Enable();
		});
		ControlsScreen.instance.RebindInfo = new RebindInfo
		{
			Action = action,
			BindingIndex = bindingIndex
		};
		rebindOperation.Start();
	}

	private bool CheckDuplicateBinding(InputAction action, int bindingIndex)
	{
		InputBinding inputBinding = action.bindings[bindingIndex];
		foreach (InputBinding inputBinding2 in action.actionMap.bindings)
		{
			if (!(inputBinding2.action == inputBinding.action) && inputBinding2.effectivePath == inputBinding.effectivePath)
			{
				return true;
			}
		}
		if (inputBinding.isPartOfComposite)
		{
			for (int i = 1; i < bindingIndex; i++)
			{
				if (action.bindings[i].overridePath == inputBinding.overridePath)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void Update()
	{
		InputAction inputAction;
		int num;
		if (!this.ResolveActionAndBinding(out inputAction, out num))
		{
			return;
		}
		this.ActionName.text = SokLoc.Translate("control_" + inputAction.name);
		this.SetBindingButton.TextMeshPro.text = inputAction.GetBindingDisplayString(num, (InputBinding.DisplayStringOptions)0);
	}

	public string MyAction;

	public string Scheme;

	public TextMeshProUGUI ActionName;

	public CustomButton ResetButton;

	public CustomButton SetBindingButton;
}
