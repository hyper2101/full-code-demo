using System;
using UnityEngine;

public class DirectionCircleElement : Interactable
{
	protected override void Start()
	{
		this.startScale = base.transform.localScale;
		base.Start();
	}

	public override bool CanBeAutoMovedTo
	{
		get
		{
			return base.gameObject.activeInHierarchy && !(WorldManager.instance.DraggingCard != null) && !this.ParentCard.BeingDragged;
		}
	}

	public override string GetTooltipText()
	{
		return "Toggle output direction";
	}

	public override void Clicked()
	{
		this.ParentCard.ToggleDirection();
	}

	protected override void Update()
	{
		if (this.ParentCard.CardData.OutputDir == Vector3.zero)
		{
			this.DirectionSpriteRenderer.sprite = this.RandomArrow;
		}
		else
		{
			this.DirectionSpriteRenderer.sprite = this.DirectionArrow;
		}
		base.transform.rotation = Quaternion.LookRotation(Vector3.down, this.ParentCard.CardData.OutputDir);
		Vector3 vector = (this.IsHovered ? (this.startScale * 1.1f) : this.startScale);
		base.transform.localScale = Vector3.Lerp(base.transform.localScale, vector, Time.deltaTime * 12f);
	}

	public override bool CanBeDragged()
	{
		return false;
	}

	public override bool CanBePushed()
	{
		return false;
	}

	public override bool CanBePushedBy(Draggable draggable)
	{
		return false;
	}

	protected override void ClampPos()
	{
	}

	private Vector3 startScale;

	public GameCard ParentCard;

	public Sprite DirectionArrow;

	public Sprite RandomArrow;

	public SpriteRenderer DirectionSpriteRenderer;
}
