using System;
using System.Linq;
using UnityEngine;

public class Draggable : MonoBehaviour
{
	public virtual bool IsHovered
	{
		get
		{
			return WorldManager.instance.HoveredDraggable == this;
		}
	}

	public virtual bool CanBeAutoMovedTo
	{
		get
		{
			return true;
		}
	}

	public virtual Vector3 AutoMoveSnapPosition
	{
		get
		{
			return base.transform.position;
		}
	}

	public Bounds DraggableBounds
	{
		get
		{
			Vector3 vector;
			Vector3 vector2;
			Quaternion quaternion;
			this.boxCollider.ToWorldSpaceBox(out vector, out vector2, out quaternion);
			return new Bounds(vector, vector2 * 2f);
		}
	}

	public virtual bool CanBePushedBy(Draggable draggable)
	{
		GameCard gameCard = draggable as GameCard;
		return (gameCard == null || (!gameCard.IsEquipped && !gameCard.IsWorking)) && !(draggable is InventoryInteractable) && !(draggable is OnOffInteractable) && !(draggable is FloatingStatus) && !(draggable is CardConnector) && !(draggable is DirectionCircleElement) && !draggable.BeingDragged && (draggable.Velocity == null || draggable.Velocity.Value.y < 0f);
	}

	public virtual bool CanBePushed()
	{
		return true;
	}

	protected virtual bool HasPhysics
	{
		get
		{
			return false;
		}
	}

	protected virtual void Awake()
	{
		if (this.boxCollider == null)
		{
			BoxCollider[] components = base.GetComponents<BoxCollider>();
			this.boxCollider = components.FirstOrDefault<BoxCollider>((BoxCollider x) => !x.isTrigger);
		}
		WorldManager.instance.AllDraggables.Add(this);
		if (this.HasPhysics)
		{
			WorldManager.instance.PhysicsDraggables.Add(this);
		}
		this.layerMask = LayerMask.GetMask(new string[] { "Pushables" });
	}

	protected virtual void OnDestroy()
	{
		if (WorldManager.instance != null)
		{
			WorldManager.instance.AllDraggables.Remove(this);
			if (this.HasPhysics)
			{
				WorldManager.instance.PhysicsDraggables.Remove(this);
			}
		}
	}

	protected virtual void Start()
	{
		this.TargetPosition = base.transform.position;
	}

	public virtual void SendIt()
	{
		if (WorldManager.instance.CurrentBoard.Id == "forest")
		{
			this.Velocity = new Vector3?(new Vector3(0f, 10f, -10f));
			return;
		}
		Vector2 vector = Random.insideUnitCircle.normalized * 3f * 1.5f;
		this.Velocity = new Vector3?(new Vector3(vector.x, 5f, vector.y));
	}

	public virtual void SendDirection(Vector3 direction)
	{
		this.Velocity = new Vector3?(new Vector3(direction.x * 6f, 5f, direction.z * 7f));
	}

	public virtual void SendToPosition(Vector3 position)
	{
		Vector3 vector = position - base.transform.position;
		this.Velocity = new Vector3?(new Vector3(vector.x * 4f, 5f, vector.z * 4f));
	}

	protected virtual void Bounce()
	{
	}

	public void UpdatePhysics(float dt)
	{
		if (this.Velocity != null)
		{
			Vector3 vector = base.transform.position;
			vector += this.Velocity.Value * dt;
			Vector3 value = this.Velocity.Value;
			if (vector.y <= this.MinY && value.y < 0f)
			{
				value.x *= 0.8f;
				value.z *= 0.8f;
				value.y *= -0.6f;
				vector.y = this.MinY;
				this.Bounce();
				if (this.Velocity == null)
				{
					return;
				}
			}
			value.x *= 0.93f;
			value.y -= 30f * dt;
			value.z *= 0.93f;
			Vector3 vector2 = value;
			vector2.y = 0f;
			if (vector2.magnitude < 0.01f && vector.y <= this.MinY + 0.1f)
			{
				this.Velocity = null;
			}
			else
			{
				this.Velocity = new Vector3?(value);
			}
			this.TargetPosition = (base.transform.position = vector);
		}
	}

	protected virtual void Update()
	{
		Vector3 targetPosition = this.TargetPosition;
		targetPosition.y = -targetPosition.z * 0.001f;
		targetPosition.y += (this.BeingDragged ? 0.1f : 0f);
		if (this.IsHovered)
		{
			targetPosition.y += 0.03f;
		}
		if (this is Boosterpack)
		{
			targetPosition.y += 0.03f;
		}
		base.transform.position = Vector3.Lerp(base.transform.position, targetPosition, Time.deltaTime * 20f);
	}

	protected virtual void LateUpdate()
	{
		this.PushAwayFromOthers();
		this.ClampPos();
	}

	public virtual bool CanBeDragged()
	{
		return !this.BeingDragged;
	}

	public virtual void Clicked()
	{
	}

	public virtual void StartDragging()
	{
		this.dragStartTime = Time.time;
		this.Velocity = null;
		this.BeingDragged = true;
	}

	protected bool DragThresholdReached
	{
		get
		{
			Vector3 vector = this.DragStartPosition - base.transform.position;
			vector.y = 0f;
			return Time.time - this.dragStartTime > 0.2f || vector.magnitude > 0.1f;
		}
	}

	public virtual void StopDragging()
	{
		if (!this.DragThresholdReached)
		{
			if (this.ClickedObject == null)
			{
				this.Clicked();
			}
			else
			{
				this.ClickedObject.Clicked();
			}
		}
		this.ClickedObject = null;
		this.DragTag = null;
		this.BeingDragged = false;
	}

	protected virtual void PushAwayFromOthers()
	{
		if (!this.CanBePushed())
		{
			return;
		}
		Draggable draggable = this;
		BoxCollider boxCollider = draggable.boxCollider;
		BoxCollider boxCollider2 = null;
		GameCard gameCard = this as GameCard;
		if (gameCard != null)
		{
			if (gameCard.Parent != null)
			{
				return;
			}
			draggable = gameCard.GetRootCard();
			boxCollider2 = gameCard.GetLeafCard().boxCollider;
		}
		GetComponentCacher<Draggable> draggableLookup = WorldManager.instance.DraggableLookup;
		int num;
		if (boxCollider2 == null)
		{
			num = PhysicsExtensions.OverlapBoxNonAlloc(this.boxCollider, this.hits, this.layerMask, QueryTriggerInteraction.UseGlobal);
		}
		else
		{
			num = PhysicsExtensions.OverlapTwoBoxNonAlloc(boxCollider, boxCollider2, this.hits, this.layerMask, QueryTriggerInteraction.UseGlobal);
		}
		for (int i = 0; i < num; i++)
		{
			Collider collider = this.hits[i];
			Draggable component = draggableLookup.GetComponent(collider.gameObject);
			if (!(component == null) && !(component == this) && !component.BeingDragged && this.CanBePushedBy(component))
			{
				GameCard gameCard2 = this as GameCard;
				if (gameCard2 != null && gameCard2.GetRootCard() != null && gameCard2.GetRootCard().CardData is HeavyFoundation)
				{
					GameCard gameCard3 = component as GameCard;
					if (gameCard3 != null && !(gameCard3.CardData is HeavyFoundation))
					{
						goto IL_01CB;
					}
				}
				Vector3 vector = component.transform.position - draggable.transform.position;
				vector.y = 0f;
				float num2 = draggable.Mass + component.Mass;
				float num3 = 1f - draggable.Mass / num2;
				if (component.PushDir != null)
				{
					vector = component.PushDir.Value;
					num3 = 1f;
				}
				draggable.TargetPosition -= num3 * vector.normalized * 2f * Time.deltaTime;
				return;
			}
			IL_01CB:;
		}
	}

	protected virtual float Mass
	{
		get
		{
			return 1f;
		}
	}

	protected virtual void ClampPos()
	{
		Vector3 position = base.transform.position;
		Bounds worldBounds = this.MyBoard.WorldBounds;
		Vector3 vector;
		Vector3 vector2;
		Quaternion quaternion;
		this.boxCollider.ToWorldSpaceBox(out vector, out vector2, out quaternion);
		float num = 0.1f;
		position.x = Mathf.Clamp(position.x, worldBounds.min.x + vector2.x + num, worldBounds.max.x - vector2.x - num);
		position.z = Mathf.Clamp(position.z, worldBounds.min.z + vector2.y + num, worldBounds.max.z - vector2.y - num);
		base.transform.position = position;
	}

	protected GameBoard DetermineParentBoard()
	{
		return base.GetComponentInParent<GameBoard>();
	}

	public bool BeingDragged;

	[HideInInspector]
	public GameBoard MyBoard;

	[HideInInspector]
	public Vector3 TargetPosition;

	[HideInInspector]
	public Vector3 DragStartPosition;

	public Vector3? Velocity;

	public const float Drag = 0.93f;

	public const float Gravity = 30f;

	public const float Bounciness = 0.6f;

	public Vector3? PushDir;

	public BoxCollider boxCollider;

	[HideInInspector]
	public string DragTag;

	[HideInInspector]
	public Draggable ClickedObject;

	[HideInInspector]
	public float MinY;

	private float dragStartTime;

	protected Collider[] hits = new Collider[50];

	private int layerMask;

	protected Bounds debugBounds;

	protected bool wasPushed;
}
