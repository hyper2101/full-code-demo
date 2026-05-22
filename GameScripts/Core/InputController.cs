using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Users;

public class InputController : MonoBehaviour
{
	public string InputString
	{
		get
		{
			return this.inputString;
		}
	}

	public event Action<ControlScheme> ControlSchemeChanged;

	private void Awake()
	{
		InputController.instance = this;
		this.SetupInputActions();
		EnhancedTouchSupport.Enable();
		this.Vibrator = new ControllerVibrator();
		InputSystem.pollingFrequency = 120f;
		InputUser.onChange += this.InputUser_onChange;
		InputSystem.onActionChange += delegate(object obj, InputActionChange change)
		{
			if (change == InputActionChange.ActionPerformed)
			{
				this.lastDevice = ((InputAction)obj).activeControl.device;
			}
		};
		if (Keyboard.current != null)
		{
			Keyboard.current.onTextInput += this.OnTextInput;
		}
	}

	private void OnTextInput(char c)
	{
		this.inputString += c.ToString();
	}

	private void SetupInputActions()
	{
		this.cancel = this.PlayerInput.actions["cancel"];
		this.submit = this.PlayerInput.actions["submit"];
		this.time_pause = this.PlayerInput.actions["time_pause"];
		this.pause = this.PlayerInput.actions["pause"];
		this.move = this.PlayerInput.actions["move"];
		this.snap_cards = this.PlayerInput.actions["snap_cards"];
		this.time_1 = this.PlayerInput.actions["time_1"];
		this.time_2 = this.PlayerInput.actions["time_2"];
		this.time_3 = this.PlayerInput.actions["time_3"];
		this.zoom = this.PlayerInput.actions["zoom"];
		this.panel_collapse = this.PlayerInput.actions["panel_collapse"];
		this.activate_ui = this.PlayerInput.actions["activate_ui"];
		this.time_toggle = this.PlayerInput.actions["time_toggle"];
		this.sell = this.PlayerInput.actions["sell"];
		this.toggle_inventory = this.PlayerInput.actions["toggle_inventory"];
		this.toggle_view = this.PlayerInput.actions["toggle_view"];
		this.grab = this.PlayerInput.actions["grab"];
		this.snap_move = this.PlayerInput.actions["snap_move"];
	}

	private void OnApplicationFocus(bool focus)
	{
		if (!focus)
		{
			if (Keyboard.current != null)
			{
				InputSystem.ResetDevice(Keyboard.current, false);
			}
			this.ClearInputs();
			if (this.Vibrator != null)
			{
				this.Vibrator.StopVibrate();
			}
		}
	}

	private void OnDestroy()
	{
		if (this.Vibrator != null)
		{
			this.Vibrator.StopVibrate();
		}
		InputUser.onChange -= this.InputUser_onChange;
	}

	public void ClearInputs()
	{
		this.Inputs.Clear();
	}

	public void LogCurrentState()
	{
		string text = "Input controller state log\n";
		foreach (InputController.UserInput userInput in this.Inputs)
		{
			text = text + userInput.ToString() + "\n";
		}
		text += string.Format("Active touches report: {0} touches!", Touch.activeTouches.Count);
		Debug.Log(text);
	}

	public bool IsUsingMouse
	{
		get
		{
			return !this.CurrentSchemeIsController && !this.CurrentSchemeIsTouch && Mouse.current != null;
		}
	}

	public bool MouseIsDragging
	{
		get
		{
			return this.mouseIsDragging;
		}
	}

	private float dragDistanceThreshold
	{
		get
		{
			return (float)Screen.height * 0.025f;
		}
	}

	private float dragTimeThreshold
	{
		get
		{
			return 0.4f;
		}
	}

	private ButtonControl GetMouseButton(int buttonId)
	{
		if (buttonId == 0)
		{
			return Mouse.current.leftButton;
		}
		if (buttonId == 1)
		{
			return Mouse.current.rightButton;
		}
		return Mouse.current.middleButton;
	}

	public Vector2 ClampedMousePosition()
	{
		if (Mouse.current == null)
		{
			return new Vector2((float)Screen.width, (float)Screen.height) * 0.5f;
		}
		Vector2 vector = Mouse.current.position.ReadValue();
		vector.x = Mathf.Clamp(vector.x, 0f, (float)Screen.width);
		vector.y = Mathf.Clamp(vector.y, 0f, (float)Screen.height);
		return vector;
	}

	private bool MousePositionIsInScreen()
	{
		Vector2 vector = Mouse.current.position.ReadValue();
		return vector.x > 0f && vector.x < (float)Screen.width && vector.y > 0f && vector.y < (float)Screen.height;
	}

	public Vector2 GetSafeTouchPosition(int i)
	{
		if (!this.GetInput(i))
		{
			return new Vector2((float)Screen.width, (float)Screen.height) * 0.5f;
		}
		return this.GetInputPosition(i);
	}

	private void Update()
	{
		InputSystem.Update();
		this.Vibrator.UpdateVibrate(Time.unscaledDeltaTime);
		foreach (InputController.UserInput userInput in this.inputsToRemove)
		{
			this.Inputs.Remove(userInput);
		}
		this.inputsToRemove.Clear();
		foreach (InputController.UserInput userInput2 in this.Inputs)
		{
			userInput2.JustStarted = false;
			userInput2.UpdatedThisFrame = false;
		}
		if (Mouse.current != null)
		{
			int num = 0;
			if (AccessibilityScreen.ClickToDragEnabled)
			{
				num = 1;
			}
			int mouseId2;
			int mouseId;
			for (mouseId = 0; mouseId <= num; mouseId = mouseId2 + 1)
			{
				ButtonControl mouseButton = this.GetMouseButton(mouseId);
				if (mouseButton.wasPressedThisFrame)
				{
					if (this.MousePositionIsInScreen() && this.Inputs.Count <= 0)
					{
						this.mouseIsDragging = false;
						this.Inputs.Add(new InputController.UserInput
						{
							ScreenPosition = this.ClampedMousePosition(),
							MouseId = mouseId,
							JustStarted = true,
							StartPosition = this.ClampedMousePosition(),
							StartTime = Time.time,
							UpdatedThisFrame = true
						});
					}
				}
				else if (mouseButton.isPressed)
				{
					InputController.UserInput userInput3 = this.Inputs.FirstOrDefault<InputController.UserInput>((InputController.UserInput x) => x.MouseId == mouseId);
					if (userInput3 != null)
					{
						userInput3.DeltaPosition = this.ClampedMousePosition() - userInput3.ScreenPosition;
						userInput3.ScreenPosition = this.ClampedMousePosition();
						userInput3.UpdatedThisFrame = true;
						if (!this.mouseIsDragging && ((userInput3.StartPosition - userInput3.ScreenPosition).magnitude > this.dragDistanceThreshold || Time.time - userInput3.StartTime >= this.dragTimeThreshold))
						{
							this.mouseIsDragging = true;
						}
					}
				}
				else
				{
					for (int i = 0; i < this.Inputs.Count; i++)
					{
						if (this.Inputs[i].MouseId == mouseId)
						{
							this.Inputs[i].JustEnded = true;
							this.Inputs[i].UpdatedThisFrame = true;
							this.inputsToRemove.Add(this.Inputs[i]);
						}
					}
				}
				mouseId2 = mouseId;
			}
		}
		else
		{
			this.mouseIsDragging = false;
		}
		if (this.Inputs.Count == 0)
		{
			this.mouseIsDragging = false;
		}
		if (this.TouchesEnabled)
		{
			for (int j = 0; j < Touch.activeTouches.Count; j++)
			{
				Touch touch = Touch.activeTouches[j];
				if (!touch.valid)
				{
					Debug.Log("Invalid touch in active touches!");
				}
				else if (touch.phase == TouchPhase.Began)
				{
					this.Inputs.Add(new InputController.UserInput
					{
						ScreenPosition = touch.screenPosition,
						TouchId = touch.touchId,
						JustStarted = true,
						StartPosition = touch.screenPosition,
						StartTime = Time.time,
						UpdatedThisFrame = true
					});
				}
				else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
				{
					InputController.UserInput userInput4 = this.Inputs.FirstOrDefault<InputController.UserInput>((InputController.UserInput x) => x.TouchId == touch.touchId);
					if (userInput4 != null)
					{
						userInput4.DeltaPosition = touch.delta;
						userInput4.ScreenPosition = touch.screenPosition;
						userInput4.UpdatedThisFrame = true;
					}
				}
				else
				{
					for (int k = 0; k < this.Inputs.Count; k++)
					{
						if (this.Inputs[k].TouchId == touch.touchId)
						{
							this.Inputs[k].JustEnded = true;
							this.Inputs[k].UpdatedThisFrame = true;
							this.inputsToRemove.Add(this.Inputs[k]);
						}
					}
				}
			}
		}
		this.UpdatePen();
		for (int l = this.Inputs.Count - 1; l >= 0; l--)
		{
			if (!this.Inputs[l].UpdatedThisFrame)
			{
				Debug.Log("Removed a non-updated input!");
				this.Inputs.RemoveAt(l);
			}
		}
		Cursor.visible = !this.CurrentSchemeIsController;
		if (this.lastControlScheme != this.CurrentScheme)
		{
			this.ClearBindingDisplayCache();
		}
		this.ActiveScheme = this.PlayerInput.currentControlScheme;
		this.lastControlScheme = this.CurrentScheme;
	}

	private void UpdatePen()
	{
		if (Pen.current == null)
		{
			return;
		}
		ButtonControl tip = Pen.current.tip;
		Vector2Control position = Pen.current.position;
		Vector2 vector = new Vector2(position.x.ReadValue(), position.y.ReadValue());
		if (tip.wasPressedThisFrame)
		{
			this.Inputs.Add(new InputController.UserInput
			{
				ScreenPosition = vector,
				PenId = Pen.current.deviceId,
				JustStarted = true,
				StartPosition = vector,
				StartTime = Time.time,
				UpdatedThisFrame = true
			});
			return;
		}
		if (tip.isPressed)
		{
			InputController.UserInput userInput = this.Inputs.FirstOrDefault<InputController.UserInput>((InputController.UserInput x) => x.PenId == Pen.current.deviceId);
			if (userInput != null)
			{
				userInput.DeltaPosition = vector - userInput.ScreenPosition;
				userInput.ScreenPosition = vector;
				userInput.UpdatedThisFrame = true;
				return;
			}
		}
		else
		{
			for (int i = 0; i < this.Inputs.Count; i++)
			{
				if (this.Inputs[i].PenId == Pen.current.deviceId)
				{
					this.Inputs[i].JustEnded = true;
					this.Inputs[i].UpdatedThisFrame = true;
					this.inputsToRemove.Add(this.Inputs[i]);
				}
			}
		}
	}

	private bool TouchesEnabled
	{
		get
		{
			return true;
		}
	}

	private void LateUpdate()
	{
		this.lastGrab = this.GetGrab();
		this.lastMove = this.GetMove();
		this.lastSnapMove = this.GetSnapMove();
		this.inputString = "";
		this.LastInputCount = this.InputCount;
		this._currentScheme = null;
	}

	public Vector2 AllDeltaPos()
	{
		if (this.DisableAllInput)
		{
			return Vector2.zero;
		}
		if (this.InputCount == 0)
		{
			return Vector2.zero;
		}
		Vector2 vector = Vector2.zero;
		for (int i = 0; i < this.InputCount; i++)
		{
			vector += this.GetDeltaPosition(i);
		}
		return vector / (float)this.InputCount;
	}

	public int InputCount
	{
		get
		{
			return this.Inputs.Count;
		}
	}

	public bool GetInputBegan(int i)
	{
		return this.GetInput(i) && this.Inputs[i].JustStarted;
	}

	public bool GetRightMouseBegan()
	{
		return this.GetInput(0) && this.Inputs[0].JustStarted && this.Inputs[0].MouseId == 1;
	}

	public bool GetLeftMouseBegan()
	{
		return this.GetInput(0) && this.Inputs[0].JustStarted && this.Inputs[0].MouseId == 0;
	}

	public bool GetRightMouseEnded()
	{
		return this.GetInput(0) && this.Inputs[0].JustEnded && this.Inputs[0].MouseId == 1;
	}

	public bool GetLeftMouseEnded()
	{
		return this.GetInput(0) && this.Inputs[0].JustEnded && this.Inputs[0].MouseId == 0;
	}

	public bool GetInputTapped(int i)
	{
		InputController.UserInput userInput = this.Inputs[i];
		return userInput.JustEnded && Time.time - userInput.StartTime <= 0.15f && (userInput.ScreenPosition - userInput.StartPosition).magnitude < (float)Screen.width * 0.05f;
	}

	public bool IsNotRightClick(int i)
	{
		return this.Inputs[i].MouseId != 1;
	}

	public bool GetInputEnded(int i)
	{
		return this.GetInput(i) && this.Inputs[i].JustEnded;
	}

	public bool GetInput(int i)
	{
		return this.Inputs.Count > i;
	}

	public bool GetInputMoving(int i)
	{
		return this.Inputs.Count > i && this.Inputs[i].DeltaPosition != Vector2.zero;
	}

	public Vector2 GetDeltaPosition(int i)
	{
		return this.Inputs[i].DeltaPosition;
	}

	public Vector2 GetDeltaPositionSinceStart(int i)
	{
		return this.Inputs[i].ScreenPosition - this.Inputs[i].StartPosition;
	}

	public Vector2 GetInputPosition(int i)
	{
		return this.Inputs[i].ScreenPosition;
	}

	public Vector2 GetStartPosition(int i)
	{
		return this.Inputs[i].StartPosition;
	}

	public bool GetStickHorizontal()
	{
		return (double)this.move.ReadValue<Vector2>().x > 0.3 || (double)this.move.ReadValue<Vector2>().x < -0.3;
	}

	public bool CancelTriggered()
	{
		return this.cancel.triggered;
	}

	public bool SubmitTriggered()
	{
		return !this.DisableAllInput && this.submit.triggered;
	}

	public bool TimePauseTriggered()
	{
		return !this.DisableAllInput && this.time_pause.triggered;
	}

	public bool PauseTriggered()
	{
		return !this.DisableAllInput && this.pause.triggered;
	}

	public bool SnapCardsTriggered()
	{
		return !this.DisableAllInput && this.snap_cards.triggered;
	}

	public bool Time1_Triggered()
	{
		return !this.DisableAllInput && this.time_1.triggered;
	}

	public bool Time2_Triggered()
	{
		return !this.DisableAllInput && this.time_2.triggered;
	}

	public bool Time3_Triggered()
	{
		return !this.DisableAllInput && this.time_3.triggered;
	}

	public float GetZoom()
	{
		if (this.DisableAllInput)
		{
			return 0f;
		}
		return this.zoom.ReadValue<float>();
	}

	public bool PanelCollapse_Triggered()
	{
		return !this.DisableAllInput && this.panel_collapse.triggered;
	}

	public bool ActivateUI_Triggered()
	{
		return !this.DisableAllInput && this.activate_ui.triggered;
	}

	public bool TimeToggleTriggered()
	{
		return !this.DisableAllInput && this.time_toggle.triggered;
	}

	public bool SellTriggered()
	{
		return !this.DisableAllInput && this.sell.triggered;
	}

	public bool ToggleInventoryTriggered()
	{
		return !this.DisableAllInput && this.toggle_inventory.triggered;
	}

	public bool ToggleViewTriggered()
	{
		return !this.DisableAllInput && this.toggle_view.triggered;
	}

	public Vector2 GetMove()
	{
		if (this.DisableAllInput)
		{
			return Vector2.zero;
		}
		return this.move.ReadValue<Vector2>();
	}

	public Vector2 GetSnapMovePressed()
	{
		Vector2 snapMove = this.GetSnapMove();
		if (snapMove.magnitude == 0f)
		{
			return Vector2.zero;
		}
		return (snapMove - this.lastSnapMove).normalized;
	}

	public Vector2 GetSnapMove()
	{
		if (this.DisableAllInput)
		{
			return Vector2.zero;
		}
		return this.snap_move.ReadValue<Vector2>();
	}

	public float GetGrab()
	{
		if (this.DisableAllInput)
		{
			return 0f;
		}
		return this.grab.ReadValue<float>();
	}

	public Vector2 GetDeltaMove()
	{
		if (this.DisableAllInput)
		{
			return Vector2.zero;
		}
		return this.GetMove() - this.lastMove;
	}

	public bool StartedGrabbing()
	{
		return !this.DisableAllInput && this.grab.triggered;
	}

	public bool StoppedGrabbing()
	{
		return !this.DisableAllInput && this.GetGrab() < 0.5f && this.lastGrab > 0.5f;
	}

	public string GetActionDisplayString(string name)
	{
		if (!this.bindingDisplayCache.ContainsKey(name))
		{
			this.bindingDisplayCache[name] = "[" + this.PlayerInput.actions[name].GetBindingDisplayString((InputBinding.DisplayStringOptions)0, null) + "]";
		}
		return this.bindingDisplayCache[name];
	}

	public void ClearBindingDisplayCache()
	{
		this.bindingDisplayCache.Clear();
	}

	public bool GetKeyDown(Key key)
	{
		return Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame;
	}

	public bool GetKey(Key key)
	{
		return Keyboard.current != null && Keyboard.current[key].isPressed;
	}

	public bool AnyInputDone()
	{
		return (this.InputCount > 0 && this.GetInputTapped(0)) || (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) || this.SubmitTriggered();
	}

	public ControlScheme CurrentScheme
	{
		get
		{
			if (this.SchemeOverride != null)
			{
				return this.SchemeOverride.Value;
			}
			if (this._currentScheme == null)
			{
				this._currentScheme = new ControlScheme?(this.GetSchemeFromName(this.PlayerInput.currentControlScheme));
			}
			return this._currentScheme.Value;
		}
	}

	public bool CurrentSchemeIsController
	{
		get
		{
			return this.CurrentScheme == ControlScheme.Controller;
		}
	}

	public bool CurrentSchemeIsMouseKeyboard
	{
		get
		{
			return this.CurrentScheme == ControlScheme.KeyboardMouse;
		}
	}

	public bool CurrentSchemeIsTouch
	{
		get
		{
			return this.CurrentScheme == ControlScheme.Touch;
		}
	}

	private ControlScheme GetSchemeFromName(string scheme)
	{
		if (scheme == "Keyboard&Mouse")
		{
			return ControlScheme.KeyboardMouse;
		}
		if (scheme == "Gamepad")
		{
			return ControlScheme.Controller;
		}
		return ControlScheme.Touch;
	}

	private void InputUser_onChange(InputUser user, InputUserChange change, InputDevice device)
	{
		if (change == InputUserChange.ControlSchemeChanged)
		{
			ControlScheme schemeFromName = this.GetSchemeFromName(user.controlScheme.Value.name);
			Action<ControlScheme> controlSchemeChanged = this.ControlSchemeChanged;
			if (controlSchemeChanged == null)
			{
				return;
			}
			controlSchemeChanged(schemeFromName);
		}
	}

	public void LogDevices()
	{
		for (int i = 0; i < InputSystem.devices.Count; i++)
		{
			InputDevice inputDevice = InputSystem.devices[i];
			Debug.Log(string.Concat(new string[]
			{
				string.Format("Device {0}\n", i),
				"Display name: ",
				inputDevice.displayName,
				"\nInterface name: ",
				inputDevice.description.interfaceName,
				"\nDevice class: ",
				inputDevice.description.deviceClass,
				"\nProduct: ",
				inputDevice.description.product,
				"\n"
			}));
		}
	}

	public static InputController instance;

	public PlayerInput PlayerInput;

	public bool DisableAllInput;

	private string inputString;

	public string ActiveScheme;

	private List<InputController.UserInput> Inputs = new List<InputController.UserInput>();

	private List<InputController.UserInput> inputsToRemove = new List<InputController.UserInput>();

	public ControllerVibrator Vibrator;

	private InputAction cancel;

	private InputAction submit;

	private InputAction time_pause;

	private InputAction pause;

	private InputAction move;

	private InputAction snap_cards;

	private InputAction time_1;

	private InputAction time_2;

	private InputAction time_3;

	private InputAction zoom;

	private InputAction panel_collapse;

	private InputAction activate_ui;

	private InputAction time_toggle;

	private InputAction sell;

	private InputAction toggle_inventory;

	private InputAction toggle_view;

	private InputAction grab;

	private InputAction snap_move;

	private bool mouseIsDragging;

	private Vector2 lastMove;

	private float lastGrab;

	private Vector2 lastSnapMove;

	private ControlScheme lastControlScheme;

	public int LastInputCount;

	private Dictionary<string, string> bindingDisplayCache = new Dictionary<string, string>();

	public ControlScheme? SchemeOverride;

	private ControlScheme? _currentScheme;

	private InputDevice lastDevice;

	private class UserInput
	{
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				this.MouseId.ToString(),
				" ",
				this.TouchId.ToString(),
				this.ScreenPosition.ToString(),
				" ",
				this.DeltaPosition.ToString(),
				" ",
				this.JustStarted.ToString()
			});
		}

		public Vector2 ScreenPosition;

		public Vector2 DeltaPosition = Vector2.zero;

		public Vector2 StartPosition;

		public float StartTime;

		public int MouseId = -1;

		public int TouchId = -1;

		public int PenId = -1;

		public bool JustStarted = true;

		public bool JustEnded;

		public bool UpdatedThisFrame;
	}
}
