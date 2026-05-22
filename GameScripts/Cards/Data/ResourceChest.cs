using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ResourceChest : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		if (!string.IsNullOrEmpty(this.HeldCardId) && otherCard.Id != this.HeldCardId)
		{
			return false;
		}
		Food food = otherCard as Food;
		return (food != null && food.FoodValue <= 0 && WorldManager.instance.CurrentBoard.Id == "cities") || (otherCard.MyCardType == CardType.Resources && !(otherCard.Id == "gold") && !(otherCard.Id == "shell") && (!(WorldManager.instance.CurrentBoard.Id == "cities") || (!(otherCard.Id == "poop") && !(otherCard.Id == "quantum_entangled_uranium"))) && !(otherCard is Dollar));
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	public override void UpdateCard()
	{
		this.MyGameCard.SpecialValue = new int?(this.ResourceCount);
		this.MyGameCard.SpecialIcon.sprite = this.SpecialIcon;
		if (!this.MyGameCard.HasParent || this.MyGameCard.Parent.CardData is HeavyFoundation)
		{
			foreach (GameCard gameCard in this.MyGameCard.GetChildCards())
			{
				if (string.IsNullOrEmpty(this.HeldCardId))
				{
					this.HeldCardId = gameCard.CardData.Id;
				}
				if (!(gameCard.CardData.Id != this.HeldCardId))
				{
					if (this.ResourceCount >= this.MaxResourceCount)
					{
						gameCard.RemoveFromParent();
						break;
					}
					gameCard.DestroyCard(true, true);
					this.ResourceCount++;
					if (this.ResourceCount == this.MaxResourceCount)
					{
						QuestManager.instance.SpecialActionComplete("full_chest", null);
					}
				}
			}
		}
		if (this.outputConnector == null)
		{
			this.outputConnector = this.GetOutputConnector();
		}
		if (this.ResourceCount > 0)
		{
			CardConnector cardConnector = this.outputConnector;
			if (((cardConnector != null) ? cardConnector.ConnectedNode : null) != null)
			{
				this.MyGameCard.StartTimer(10f, new TimerAction(this.OutputCard), SokLoc.Translate("idea_resourcechest_status_2"), base.GetActionId("OutputCard"), true, false, false);
				goto IL_0198;
			}
		}
		this.MyGameCard.CancelTimer(base.GetActionId("OutputCard"));
		IL_0198:
		base.UpdateCard();
		if (string.IsNullOrEmpty(this.HeldCardId))
		{
			this.Icon = SpriteManager.instance.EmptyTexture;
		}
		else
		{
			this.Icon = WorldManager.instance.GetCardPrefab(this.HeldCardId, true).Icon;
		}
		this.MyGameCard.UpdateIcon();
	}

	[TimedAction("output_card")]
	public void OutputCard()
	{
		if (this.ResourceCount > 0)
		{
			CardData cardData = WorldManager.instance.CreateCard(base.Position, this.HeldCardId, true, false, true);
			WorldManager.instance.StackSendCheckTarget(this.MyGameCard, cardData.MyGameCard, Vector3.right, null, true, -1);
			this.ResourceCount--;
		}
	}

	public CardConnector GetOutputConnector()
	{
		CardConnector cardConnector = null;
		for (int i = 0; i < this.MyGameCard.CardConnectorChildren.Count; i++)
		{
			CardConnector cardConnector2 = this.MyGameCard.CardConnectorChildren[i];
			if (cardConnector2 != null && cardConnector2.ConnectionType == ConnectionType.Transport && cardConnector2.CardDirection == CardDirection.output)
			{
				cardConnector = cardConnector2;
			}
		}
		return cardConnector;
	}

	public override void UpdateCardText()
	{
		if (!string.IsNullOrEmpty(this.HeldCardId))
		{
			CardData cardFromId = WorldManager.instance.GameDataLoader.GetCardFromId(this.HeldCardId, true);
			this.nameOverride = SokLoc.Translate(this.ChestTermOverride, new LocParam[] { LocParam.Create("resource", cardFromId.Name) });
			if (this.MyGameCard.IsHovered)
			{
				this.descriptionOverride = SokLoc.Translate(this.ChestDescriptionLong, new LocParam[]
				{
					LocParam.Create("resource", cardFromId.Name),
					LocParam.Create("amount", this.ResourceCount.ToString())
				});
				return;
			}
		}
		else
		{
			this.nameOverride = null;
			this.descriptionOverride = null;
		}
	}

	public GameCard RemoveResources(int count)
	{
		count = Mathf.Min(count, this.ResourceCount);
		GameCard gameCard = WorldManager.instance.CreateCardStack(base.transform.position + Vector3.up * 0.2f, count, this.HeldCardId, false);
		WorldManager.instance.StackSend(gameCard.GetRootCard(), Vector3.right, null, true);
		this.ResourceCount -= count;
		return gameCard.GetRootCard();
	}

	public override void Clicked()
	{
		if (this.IsDamaged)
		{
			return;
		}
		int num = 1;
		if (InputController.instance.GetKey(Key.LeftShift) || InputController.instance.GetKey(Key.RightShift))
		{
			num = 5;
		}
		if (this.ResourceCount > 0)
		{
			this.RemoveResources(num);
		}
		if (this.ResourceCount == 0)
		{
			this.HeldCardId = null;
		}
		base.Clicked();
	}

	[ExtraData("resource_count")]
	[HideInInspector]
	public int ResourceCount;

	[ExtraData("resource_id")]
	[HideInInspector]
	public string HeldCardId = "";

	public Sprite SpecialIcon;

	[Term]
	public string ChestTermOverride = "card_storage_container_name_override";

	[Term]
	public string ChestDescriptionLong = "card_storage_container_description_long";

	public int MaxResourceCount = 100;

	private CardConnector outputConnector;
}
