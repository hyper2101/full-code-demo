using System;
using UnityEngine;

public class Conveyor : CardData
{
	private Vector3 directionVector
	{
		get
		{
			if (this.Direction == 0)
			{
				return Vector3.back;
			}
			if (this.Direction == 1)
			{
				return Vector3.left;
			}
			if (this.Direction == 2)
			{
				return Vector3.forward;
			}
			if (this.Direction == 3)
			{
				return Vector3.right;
			}
			return Vector3.back;
		}
	}

	protected override bool CanToggleOnOff()
	{
		return WorldManager.instance.CurrentBoard.Id == "cities";
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return false;
	}

	private bool CanBeInputCard(CardData card)
	{
		if (card.MyGameCard.Velocity != null || card.MyGameCard.BounceTarget != null)
		{
			return false;
		}
		if (this.MyGameCard.IsParentOf(card.MyGameCard))
		{
			return false;
		}
		ResourceChest resourceChest = card as ResourceChest;
		if (resourceChest != null)
		{
			return !string.IsNullOrEmpty(resourceChest.HeldCardId) && this.CanBeConveyed(resourceChest.HeldCardId);
		}
		ResourceMagnet resourceMagnet = card as ResourceMagnet;
		if (resourceMagnet != null)
		{
			return !string.IsNullOrEmpty(resourceMagnet.PullCardId) && this.CanBeConveyed(resourceMagnet.PullCardId);
		}
		return this.CanBeConveyed(card) && !card.MyGameCard.HasChild;
	}

	private bool CanBeConveyed(string cardId)
	{
		CardData cardPrefab = WorldManager.instance.GetCardPrefab(cardId, true);
		return this.CanBeConveyed(cardPrefab);
	}

	private CardData GetConveyableCardFromInputCard(CardData card)
	{
		ResourceChest resourceChest = card as ResourceChest;
		if (resourceChest != null && resourceChest.ResourceCount > 0)
		{
			return resourceChest.RemoveResources(1).CardData;
		}
		ResourceMagnet resourceMagnet = card as ResourceMagnet;
		if (resourceMagnet != null && resourceMagnet.MyGameCard.HasChild)
		{
			return resourceMagnet.MyGameCard.GetLeafCard().CardData;
		}
		if (this.CanBeConveyed(card))
		{
			return card;
		}
		return null;
	}

	private bool InputCardHasConveyableCard(CardData card)
	{
		ResourceChest resourceChest = card as ResourceChest;
		if (resourceChest != null)
		{
			return resourceChest.ResourceCount > 0;
		}
		ResourceMagnet resourceMagnet = card as ResourceMagnet;
		return (resourceMagnet != null && resourceMagnet.MyGameCard.HasChild) || this.CanBeConveyed(card);
	}

	private CardData GetPrefabForId(string id)
	{
		return WorldManager.instance.GetCardPrefab(id, true);
	}

	private CardData GetInputCardConveyablePrefab(CardData card)
	{
		ResourceChest resourceChest = card as ResourceChest;
		if (resourceChest != null)
		{
			return this.GetPrefabForId(resourceChest.HeldCardId);
		}
		ResourceMagnet resourceMagnet = card as ResourceMagnet;
		if (resourceMagnet != null)
		{
			return this.GetPrefabForId(resourceMagnet.PullCardId);
		}
		if (this.CanBeConveyed(card))
		{
			return this.GetPrefabForId(card.Id);
		}
		return null;
	}

	private CardData GetInputCard(bool allowDraggingCards)
	{
		GameCard bestCardInDirection = WorldManager.instance.GetBestCardInDirection(this.MyGameCard, this.directionVector, allowDraggingCards, (GameCard card) => this.CanBeInputCard(card.CardData));
		if (bestCardInDirection == null)
		{
			return null;
		}
		return bestCardInDirection.CardData;
	}

	private bool CanBeConveyed(CardData otherCard)
	{
		if (otherCard.MyCardType != CardType.Resources && otherCard.MyCardType != CardType.Food && otherCard.MyCardType != CardType.Humans)
		{
			Mob mob = otherCard as Mob;
			return mob != null && !mob.IsAggressive;
		}
		return true;
	}

	public override void UpdateCard()
	{
		if (this.MyGameCard.IsDemoCard)
		{
			return;
		}
		bool flag = true;
		if (this.MyGameCard.Velocity != null)
		{
			flag = false;
		}
		CardData cardData = null;
		if (flag)
		{
			cardData = this.GetInputCard(true);
		}
		if (cardData != null && this.InputCardHasConveyableCard(cardData))
		{
			CardData inputCardConveyablePrefab = this.GetInputCardConveyablePrefab(cardData);
			string text = SokLoc.Translate("card_conveyor_status", new LocParam[] { LocParam.Create("resource", inputCardConveyablePrefab.Name) });
			this.MyGameCard.StartTimer(this.TotalTime, new TimerAction(this.LoadCard), text, base.GetActionId("LoadCard"), true, false, false);
		}
		else
		{
			this.MyGameCard.CancelAnyTimer();
		}
		CardData cardData2 = null;
		if (cardData != null)
		{
			CardData inputCardConveyablePrefab2 = this.GetInputCardConveyablePrefab(cardData);
			if (inputCardConveyablePrefab2 != null)
			{
				GameCard targetCard = WorldManager.instance.GetTargetCard(this.MyGameCard, inputCardConveyablePrefab2, -this.directionVector, true, cardData.MyGameCard);
				cardData2 = ((targetCard != null) ? targetCard.CardData : null);
			}
		}
		this.DrawArrows(cardData, cardData2);
		base.UpdateCard();
	}

	public override void Clicked()
	{
		this.Direction = (this.Direction + 1) % 4;
		base.Clicked();
	}

	[TimedAction("load_card")]
	public void LoadCard()
	{
		CardData inputCard = this.GetInputCard(false);
		if (inputCard == null)
		{
			return;
		}
		CardData conveyableCardFromInputCard = this.GetConveyableCardFromInputCard(inputCard);
		if (conveyableCardFromInputCard == null)
		{
			return;
		}
		conveyableCardFromInputCard.MyGameCard.RemoveFromStack();
		GameCard targetCard = WorldManager.instance.GetTargetCard(this.MyGameCard, conveyableCardFromInputCard, -this.directionVector, false, inputCard.MyGameCard);
		if (targetCard != null)
		{
			this.SendToTargetCard(conveyableCardFromInputCard.MyGameCard, targetCard);
		}
		else
		{
			if (conveyableCardFromInputCard.MyGameCard.BounceTarget == inputCard.MyGameCard)
			{
				conveyableCardFromInputCard.MyGameCard.BounceTarget = null;
			}
			conveyableCardFromInputCard.MyGameCard.SendToPosition(this.MyGameCard.transform.position - this.directionVector);
		}
		QuestManager.instance.SpecialActionComplete("use_conveyor", null);
	}

	private void SendToTargetCard(GameCard card, GameCard targetCard)
	{
		Vector3 vector = targetCard.transform.position - card.transform.position;
		vector.y = 0f;
		Vector3 vector2 = new Vector3(vector.x * 4f, 7f, vector.z * 4f);
		card.BounceTarget = targetCard.GetRootCard();
		card.Velocity = new Vector3?(vector2);
	}

	private Vector2 GetPointOnCardEdge(Vector2 start, Vector2 end, GameCard card)
	{
		Bounds bounds = card.GetBounds();
		this.corners[0] = new Vector2(bounds.min.x, bounds.min.z);
		this.corners[1] = new Vector2(bounds.max.x, bounds.min.z);
		this.corners[2] = new Vector2(bounds.max.x, bounds.max.z);
		this.corners[3] = new Vector2(bounds.min.x, bounds.max.z);
		for (int i = 0; i < 4; i++)
		{
			Vector2 vector = this.corners[i];
			Vector2 vector2 = this.corners[(i + 1) % 4];
			Vector2 vector3;
			float num;
			if (MathHelper.LineSegmentsIntersection(start, end, vector, vector2, out vector3, out num))
			{
				return vector3;
			}
		}
		return start;
	}

	private Vector3 TransformToEdge(Vector3 start, Vector3 end, GameCard card, float dir)
	{
		Vector2 vector = new Vector2(start.x, start.z);
		Vector2 vector2 = new Vector2(end.x, end.z);
		Vector2 pointOnCardEdge = this.GetPointOnCardEdge(vector, vector2, card);
		return new Vector3(pointOnCardEdge.x, 0f, pointOnCardEdge.y) + (start - end).normalized * this.ExtraSideDistance * dir;
	}

	private void DrawInputArrow(CardData inputCard)
	{
		Vector3 vector = this.MyGameCard.transform.position;
		Vector3 vector2;
		if (inputCard != null)
		{
			vector2 = this.TransformToEdge(inputCard.transform.position, vector, inputCard.MyGameCard, -1f);
		}
		else
		{
			vector2 = this.MyGameCard.transform.position + this.directionVector * 0.5f;
		}
		vector = this.TransformToEdge(vector2, vector, this.MyGameCard, 1f);
		DrawManager.instance.DrawShape(new ConveyorArrow
		{
			Start = vector2,
			End = vector
		});
	}

	private void DrawOutputArrow(CardData outputCard)
	{
		Vector3 vector = this.MyGameCard.transform.position;
		Vector3 vector2;
		if (outputCard != null)
		{
			vector2 = this.TransformToEdge(vector, outputCard.transform.position, outputCard.MyGameCard, 1f);
		}
		else
		{
			vector2 = this.MyGameCard.transform.position - this.directionVector * 0.5f;
		}
		vector = this.TransformToEdge(vector, vector2, this.MyGameCard, -1f);
		DrawManager.instance.DrawShape(new ConveyorArrow
		{
			Start = vector,
			End = vector2
		});
	}

	private void DrawArrows(CardData inputCard, CardData outputCard)
	{
		this.DrawInputArrow(inputCard);
		this.DrawOutputArrow(outputCard);
	}

	public float ExtraSideDistance = 0.01f;

	[ExtraData("direction")]
	[HideInInspector]
	public int Direction;

	public float TotalTime = 5f;

	private Vector2[] corners = new Vector2[4];
}
