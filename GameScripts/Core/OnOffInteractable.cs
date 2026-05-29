using System;
using Shapes;
using UnityEngine;

public class OnOffInteractable : Interactable
{
	public override bool CanBeAutoMovedTo
	{
		get
		{
			return base.gameObject.activeInHierarchy && !(WorldManager.instance.DraggingCard != null) && !this.ParentCard.ShowInventory && !this.ParentCard.BeingDragged;
		}
	}

	public override string GetTooltipText()
	{
		return MewtationsLoc.Translate(this.TooltipTerm);
	}

	public override void Clicked()
	{
		this.ParentCard.ToggleCardOnOff();
	}

	public override bool CanBeDragged()
	{
		return false;
	}

	protected override void Start()
	{
		this.startScale = base.transform.localScale;
		base.Start();
	}

	protected override void Update()
	{
		this.MyBoard = this.ParentCard.MyBoard;
		base.gameObject.name = MewtationsLoc.Translate(this.gameObjectTerm);
		Vector3 vector = (this.IsHovered ? (this.startScale * 1.1f) : this.startScale);
		base.transform.localScale = Vector3.Lerp(base.transform.localScale, vector, Time.deltaTime * 12f);
		if (this.ParentCard.CardData != null)
		{
			if (this.ParentCard.CardData.IsOn)
			{
				this.ButtonShape.Color = ColorManager.instance.FloatingTextColorSuccess;
				return;
			}
			this.ButtonShape.Color = ColorManager.instance.FloatingTextColorFailed;
		}
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

	public GameCard ParentCard;

	private Vector3 startScale;

	public Rectangle ButtonShape;

	public string TooltipTerm;

	public string gameObjectTerm;
}
