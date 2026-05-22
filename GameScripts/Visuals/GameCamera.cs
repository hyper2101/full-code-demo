using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GameCamera : MonoBehaviour
{
	public bool IsDragging
	{
		get
		{
			return this.isDraggingCamera;
		}
	}

	public Vector3? TargetPositionOverride
	{
		get
		{
			return this._targetPositionOverride;
		}
		set
		{
			this._targetCardOverride = null;
			this._targetPositionOverride = value;
		}
	}

	public IGameCardOrCardData TargetCardOverride
	{
		get
		{
			CardData cardData = this._targetCardOverride as CardData;
			if (cardData != null && cardData.MyGameCard == null)
			{
				this._targetCardOverride = null;
				return null;
			}
			return this._targetCardOverride;
		}
		set
		{
			this._targetPositionOverride = null;
			this._targetCardOverride = value;
		}
	}

	public void CenterOnBoard(GameBoard board)
	{
		this.cameraTargetPosition = (base.transform.position = board.MiddleOfBoard() + this.GameStartCameraPosition);
	}

	public void KeepCameraAtCurrentPos()
	{
		this.cameraTargetPosition = base.transform.position;
	}

	private void Awake()
	{
		GameCamera.instance = this;
		this.TempSpiritBackgroundMaterial = new Material(this.SpiritBackgroundMaterial);
	}

	private Transform GetIntroCameraTransform()
	{
		GameBoard gameBoard;
		if (WorldManager.instance.IsCitiesDlcActive())
		{
			gameBoard = WorldManager.instance.GetBoardWithId("cities");
		}
		else if (WorldManager.instance.IsSpiritDlcActive())
		{
			gameBoard = WorldManager.instance.GetBoardWithId("death");
		}
		else
		{
			gameBoard = WorldManager.instance.GetBoardWithId("main");
		}
		return gameBoard.CameraIntroPosition;
	}

	private void Start()
	{
		base.transform.position = this.GetIntroCameraTransform().position;
		this.cameraStartPosition = base.transform.position;
		this.cameraTargetPosition = base.transform.position;
	}

	public Vector3 ScreenPosToWorldPos(Vector3 p)
	{
		Ray ray = this.MyCam.ScreenPointToRay(p);
		Plane plane = new Plane(Vector3.up, Vector3.zero);
		float num;
		plane.Raycast(ray, out num);
		return ray.origin + ray.direction * num;
	}

	public Vector3 ScreenPosToWorldPos(Vector2 pos, Vector3 camPos)
	{
		Vector3 position = this.MyCam.transform.position;
		this.MyCam.transform.position = camPos;
		Vector3 vector = this.ScreenPosToWorldPos(pos);
		this.MyCam.transform.position = position;
		return vector;
	}

	public void StartDragging()
	{
		if (!this.canControlCamera)
		{
			return;
		}
		if (InputController.instance.InputCount == 2)
		{
			return;
		}
		this.startInputPosition = InputController.instance.GetInputPosition(0);
		this.startPosition = base.transform.position;
		this.startTime = Time.time;
		this.isDraggingCamera = true;
	}

	private bool canControlCamera
	{
		get
		{
			return WorldManager.instance.IsPlaying && this.TargetPositionOverride == null && WorldManager.instance.IntroPack == null && !GameScreen.instance.ControllerIsInUI;
		}
	}

	private Draggable FindNextDraggableInDirection(Vector3 curPos, Vector3 wantedDir)
	{
		float num = float.MinValue;
		Draggable draggable = null;
		foreach (Draggable draggable2 in WorldManager.instance.AllDraggables)
		{
			if (draggable2.gameObject.activeInHierarchy && !(draggable2 == this.currentlySelectedDraggable) && !(draggable2 == WorldManager.instance.DraggingDraggable))
			{
				if (draggable2.MyBoard == null)
				{
					Draggable draggable3 = draggable2;
					Debug.Log(((draggable3 != null) ? draggable3.ToString() : null) + " does not have a board");
				}
				else if (draggable2.MyBoard.IsCurrent && draggable2.CanBeAutoMovedTo)
				{
					Vector3 vector = draggable2.AutoMoveSnapPosition - curPos;
					float num2 = Vector3.Dot(wantedDir, vector);
					if (num2 > 0f)
					{
						float num3 = num2 / vector.sqrMagnitude;
						if (num3 > num)
						{
							num = num3;
							draggable = draggable2;
						}
					}
				}
			}
		}
		return draggable;
	}

	private Draggable FindNextDraggable(Vector2 snapMoveInput)
	{
		return this.FindNextDraggableInDirection(WorldManager.instance.mouseWorldPosition, new Vector3(snapMoveInput.x, 0f, snapMoveInput.y));
	}

	private void Update()
	{
		this.Screenshake -= Time.deltaTime;
		Vector3 vector;
		if (this.Screenshake > 0f && AccessibilityScreen.ScreenshakeEnabled)
		{
			Vector2 insideUnitCircle = Random.insideUnitCircle;
			vector = this.Screenshake * (base.transform.right * insideUnitCircle.x + base.transform.up * insideUnitCircle.y);
		}
		else
		{
			vector = Vector3.zero;
		}
		if (this.isDraggingCamera && !this.canControlCamera)
		{
			this.isDraggingCamera = false;
		}
		if (this.isDraggingCamera && InputController.instance.InputCount == 2)
		{
			this.isDraggingCamera = false;
		}
		Vector2 move = InputController.instance.GetMove();
		Vector3 zero = new Vector3(move.x, 0f, move.y);
		Vector2 vector2 = InputController.instance.GetSnapMovePressed();
		if (!this.canControlCamera)
		{
			zero = Vector3.zero;
			vector2 = Vector2.zero;
		}
		if (vector2.magnitude > 0f)
		{
			if (WorldManager.instance.DraggingCard != null)
			{
				WorldManager.instance.grabOffset = WorldManager.instance.DraggingCard.CardNameText.transform.position - WorldManager.instance.DraggingCard.transform.position;
			}
			Draggable draggable = this.FindNextDraggable(vector2);
			if (draggable != null)
			{
				this.currentlySelectedDraggable = draggable;
				this.cameraTargetPosition = draggable.AutoMoveSnapPosition - this.GetCurrentGroundOffset();
			}
		}
		if (zero.magnitude > 0.01f)
		{
			this.currentlySelectedDraggable = null;
		}
		this.cameraTargetPosition += zero * Time.deltaTime * (this.MoveSpeed + Mathf.Clamp(this.cameraTargetPosition.y / 2f - 4f, 0f, 10f));
		bool flag = this.canControlCamera;
		if (InputController.instance.CurrentSchemeIsMouseKeyboard && GameCanvas.instance.MousePositionIsOverUI())
		{
			flag = false;
		}
		if (WorldManager.instance.InAnimation && !WorldManager.instance.CutsceneBoardView)
		{
			flag = false;
		}
		if (InputController.instance.InputCount == 2)
		{
			Vector2 inputPosition = InputController.instance.GetInputPosition(0);
			Vector2 inputPosition2 = InputController.instance.GetInputPosition(1);
			float num = Vector2.Distance(inputPosition, inputPosition2);
			if (this.prevInputCount != 2f && flag)
			{
				Debug.Log("Started zooming");
				this.touchCameraStartPosition = base.transform.position;
				this.zoomStartPosition = Vector2.Lerp(inputPosition, inputPosition2, 0.5f);
				this.startZoom = base.transform.position.y;
				this.startDist = num;
			}
			if (flag && num > 1E-45f)
			{
				Vector2 vector3 = Vector2.Lerp(inputPosition, inputPosition2, 0.5f);
				Vector3 vector4 = this.ScreenPosToWorldPos(vector3) - this.ScreenPosToWorldPos(this.zoomStartPosition);
				this.cameraTargetPosition = this.touchCameraStartPosition - vector4;
				Vector3 vector5 = this.ScreenPosToWorldPos(vector3, this.cameraTargetPosition);
				float num2 = this.MaxZoom + WorldManager.instance.CurrentBoard.WorldSizeIncrease * 2.7f;
				this.cameraTargetPosition.y = Mathf.Clamp(this.startZoom * (this.startDist / num), this.MinZoom, num2);
				Vector3 vector6 = this.ScreenPosToWorldPos(vector3, this.cameraTargetPosition);
				Vector3 vector7 = vector5 - vector6;
				vector7.y = 0f;
				this.cameraTargetPosition += vector7;
				base.transform.position = this.cameraTargetPosition;
			}
		}
		if (flag && !InputController.instance.CurrentSchemeIsTouch)
		{
			float num3 = InputController.instance.GetZoom() * 0.2f;
			Vector2 vector8 = InputController.instance.ClampedMousePosition();
			if (InputController.instance.CurrentSchemeIsController)
			{
				vector8 = new Vector2((float)Screen.width, (float)Screen.height) * 0.5f;
			}
			Vector3 vector9 = this.ScreenPosToWorldPos(vector8, this.cameraTargetPosition);
			vector9.y = this.cameraTargetPosition.y;
			float y = this.cameraTargetPosition.y;
			this.cameraTargetPosition.y = this.cameraTargetPosition.y + this.ZoomSpeed * num3;
			float num4 = this.MaxZoom + WorldManager.instance.CurrentBoard.WorldSizeIncrease * 2.7f;
			this.cameraTargetPosition.y = Mathf.Clamp(this.cameraTargetPosition.y, this.MinZoom, num4);
			if (y != this.cameraTargetPosition.y && Mathf.Abs(num3) > 0.0001f)
			{
				Vector3 vector10 = this.ScreenPosToWorldPos(vector8, this.cameraTargetPosition);
				Vector3 vector11 = vector9 - vector10;
				vector11.y = 0f;
				this.cameraTargetPosition += vector11;
			}
		}
		if (WorldManager.instance.DraggingCard)
		{
			if (InputController.instance.CurrentSchemeIsTouch)
			{
				this.isTouchDraggingCard = true;
			}
		}
		else
		{
			this.isTouchDraggingCard = false;
		}
		if (this.isTouchDraggingCard)
		{
			float num5 = 0.5f * this.cameraTargetPosition.y;
			if (InputController.instance.GetInputPosition(0).y >= (float)Screen.height * 0.8f)
			{
				float num6 = Mathf.InverseLerp((float)Screen.height * 0.8f, (float)Screen.height, InputController.instance.GetInputPosition(0).y) * num5;
				this.cameraTargetPosition += Vector3.forward * Time.deltaTime * num6;
			}
			else if (InputController.instance.GetInputPosition(0).y <= (float)Screen.height * 0.2f)
			{
				float num7 = Mathf.InverseLerp((float)Screen.height * 0.2f, 0f, InputController.instance.GetInputPosition(0).y) * num5;
				this.cameraTargetPosition += Vector3.back * Time.deltaTime * num7;
			}
			if (InputController.instance.GetInputPosition(0).x >= (float)Screen.width * 0.8f)
			{
				float num8 = Mathf.InverseLerp((float)Screen.width * 0.8f, (float)Screen.width, InputController.instance.GetInputPosition(0).x) * num5;
				this.cameraTargetPosition += Vector3.right * Time.deltaTime * num8;
			}
			else if (InputController.instance.GetInputPosition(0).x <= (float)Screen.width * 0.2f)
			{
				float num9 = Mathf.InverseLerp((float)Screen.width * 0.2f, 0f, InputController.instance.GetInputPosition(0).x) * num5;
				this.cameraTargetPosition += Vector3.left * Time.deltaTime * num9;
			}
		}
		if (this.isDraggingCamera && InputController.instance.GetInput(0))
		{
			Vector3 vector12 = this.ScreenPosToWorldPos(InputController.instance.GetInputPosition(0)) - this.ScreenPosToWorldPos(this.startInputPosition);
			this.cameraTargetPosition = (base.transform.position = this.startPosition - vector12);
		}
		if (this.isDraggingCamera && InputController.instance.GetInputEnded(0))
		{
			Vector2 vector13 = this.startInputPosition - InputController.instance.GetInputPosition(0);
			if (Time.time - this.startTime < 0.2f && vector13.magnitude <= 5f)
			{
				this.Clicked();
			}
			this.isDraggingCamera = false;
		}
		Vector3 vector14 = this.cameraTargetPosition;
		bool flag2 = true;
		bool flag3 = this.TargetPositionOverride != null || this.TargetCardOverride != null;
		Vector3? targetPositionOverride = this.TargetPositionOverride;
		if (this.TargetCardOverride != null)
		{
			targetPositionOverride = new Vector3?(this.TargetCardOverride.Position);
		}
		if (WorldManager.instance.IntroPack != null)
		{
			targetPositionOverride = new Vector3?(WorldManager.instance.IntroPack.transform.position);
			this.cameraTargetPosition = base.transform.position;
			flag3 = true;
		}
		if (targetPositionOverride != null)
		{
			Vector3 value = targetPositionOverride.Value;
			value.y = 0.01f;
			float num10 = ((this.CameraPositionDistanceOverride != null) ? this.CameraPositionDistanceOverride.Value : 7f);
			vector14 = value - base.transform.forward * num10;
			flag2 = false;
		}
		if (WorldManager.instance.CurrentGameState == WorldManager.GameState.InMenu)
		{
			vector14 = (this.cameraTargetPosition = this.GetIntroCameraTransform().position);
			flag2 = false;
		}
		base.transform.position = Vector3.Lerp(base.transform.position, vector14 + vector * 0.5f, Time.deltaTime * this.CameraMoveSpeed);
		if (flag2)
		{
			base.transform.position = this.ClampPos(base.transform.position);
			this.cameraTargetPosition = this.ClampPos(this.cameraTargetPosition);
		}
		this.PauseVolume.enabled = WorldManager.instance.Time.SpeedUp == 0f && !WorldManager.instance.InAnimation;
		this.PauseVolume.gameObject.SetActive(WorldManager.instance.Time.SpeedUp == 0f && !WorldManager.instance.InAnimation);
		if (WorldManager.instance.currentAnimation != null || WorldManager.instance.currentAnimationRoutine != null)
		{
			flag3 = true;
		}
		if (WorldManager.instance.CurrentGameState != WorldManager.GameState.Playing)
		{
			flag3 = true;
		}
		this.FocusVolume.weight = Mathf.Lerp(this.FocusVolume.weight, flag3 ? 1f : 0f, Time.deltaTime * 16f);
		if (Screenshotter.instance != null && Screenshotter.instance.IsScreenshotting)
		{
			this.FocusVolume.weight = 0f;
		}
		if (WorldManager.instance.CurrentBoard != null)
		{
			this.MyCam.backgroundColor = WorldManager.instance.CurrentBoard.MyMaterial.GetColor("_Color");
		}
		this.UpdateSpiritEffect();
		this.prevInputCount = (float)InputController.instance.InputCount;
	}

	public void OnRestartGame()
	{
		base.transform.position = (this.cameraTargetPosition = this.GetIntroCameraTransform().position);
	}

	private void UpdateSpiritEffect()
	{
		bool flag = false;
		bool inAnimation = WorldManager.instance.InAnimation;
		if (WorldManager.instance.CardQuery.GetCard<Spirit>() != null)
		{
			flag = true;
		}
		this.spiritEffectStrength = Mathf.Lerp(this.spiritEffectStrength, (flag && inAnimation) ? 1f : 0f, Time.deltaTime * 4f);
		this.SpiritVolume.weight = this.spiritEffectStrength;
		this.SpiritImageEffect.Weight = this.spiritEffectStrength;
		Color color = this.TempSpiritBackgroundMaterial.color;
		color.a = this.spiritEffectStrength * 0.5f;
		this.TempSpiritBackgroundMaterial.color = color;
		if (TransitionScreen.instance.CurrentTransitionType.Id == "spirit")
		{
			this.SpiritTransitionEffect.Weight = TransitionScreen.instance.TransitionAmount;
			return;
		}
		this.SpiritTransitionEffect.Weight = 0f;
	}

	public void Clicked()
	{
		WorldManager.instance.CloseOpenInventories();
	}

	private Vector3 GetCurrentGroundOffset()
	{
		Ray ray;
		return WorldManager.instance.ScreenPosToWorldPos(new Vector2((float)Screen.width, (float)Screen.height) * 0.5f, out ray) - base.transform.position;
	}

	private Vector3 ClampPos(Vector3 p)
	{
		Bounds worldBounds = WorldManager.instance.CurrentBoard.WorldBounds;
		float num = Vector3.Dot(this.GetCurrentGroundOffset(), Vector3.forward);
		float num2 = 0.2f;
		p.x = Mathf.Clamp(p.x, worldBounds.min.x - num2, worldBounds.max.x + num2);
		p.z = Mathf.Clamp(p.z, worldBounds.min.z - num - num2, worldBounds.max.z - num + num2);
		return p;
	}

	public static GameCamera instance;

	public float MoveSpeed = 1f;

	public float ZoomSpeed = 1f;

	public float MinZoom = 3f;

	public float MaxZoom = 11f;

	public Vector3 cameraStartPosition;

	public Vector3 cameraTargetPosition;

	public float CameraMoveSpeed = 12f;

	private Vector2 startInputPosition;

	private Vector3 startPosition;

	private float startTime;

	private bool isTouchDraggingCard;

	private Vector3 touchCameraStartPosition;

	private Vector3 zoomStartPosition;

	private float startZoom;

	private float startDist;

	private float prevInputCount;

	private bool isDraggingCamera;

	private Vector3? _targetPositionOverride;

	private IGameCardOrCardData _targetCardOverride;

	public float? CameraPositionDistanceOverride;

	public PostProcessVolume PauseVolume;

	public PostProcessVolume FocusVolume;

	public PostProcessVolume SpiritVolume;

	public PostProcessVolume EnergyVolume;

	public ImageEffect SpiritImageEffect;

	public ImageEffect SpiritTransitionEffect;

	public Material SpiritBackgroundMaterial;

	[HideInInspector]
	public Material TempSpiritBackgroundMaterial;

	public Vector3 GameStartCameraPosition;

	public Camera MyCam;

	public float Screenshake;

	private Draggable currentlySelectedDraggable;

	private float spiritEffectStrength;
}
