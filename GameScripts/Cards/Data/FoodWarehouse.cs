using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FoodWarehouse : Food
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard is Hotpot)
		{
			return false;
		}
		Food food = otherCard as Food;
		return food != null && food.FoodValue > 0 && (string.IsNullOrEmpty(this.HeldCardId) || (!string.IsNullOrEmpty(this.HeldCardId) && otherCard.Id == this.HeldCardId));
	}

	public override bool DetermineCanHaveCardsWhenIsRoot
	{
		get
		{
			return true;
		}
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	public override void UpdateCard()
	{
		this.MyGameCard.SpecialValue = new int?(this.FoodValue);
		this.MyGameCard.SpecialIcon.sprite = SpriteManager.instance.FoodIcon;
		if ((!this.MyGameCard.HasParent || this.MyGameCard.Parent.CardData is HeavyFoundation) && this.MyGameCard.HasChild && !(this.MyGameCard.Child.CardData is FoodWarehouse) && (string.IsNullOrEmpty(this.HeldCardId) || (!string.IsNullOrEmpty(this.HeldCardId) && this.MyGameCard.Child.CardData.Id == this.HeldCardId)))
		{
			this.StoreFood();
		}
		if (!string.IsNullOrEmpty(this.HeldCardId))
		{
			Food food = WorldManager.instance.GameDataLoader.GetCardFromId(this.HeldCardId, true) as Food;
			this.nameOverride = SokLoc.Translate("card_food_warehouse_name_long", new LocParam[] { LocParam.Create("food", WorldManager.instance.GameDataLoader.GetCardFromId(this.HeldCardId, true).Name) });
			this.descriptionOverride = SokLoc.Translate("card_food_warehouse_description_long", new LocParam[]
			{
				LocParam.Create("food", WorldManager.instance.GameDataLoader.GetCardFromId(this.HeldCardId, true).Name),
				LocParam.Create("amount", (this.FoodValue / food.FoodValue).ToString())
			});
		}
		else
		{
			this.nameOverride = SokLoc.Translate("card_food_warehouse_name");
			this.descriptionOverride = null;
		}
		if (this.outputConnector == null)
		{
			this.outputConnector = this.GetOutputConnector();
		}
		if (this.FoodValue > 0)
		{
			CardConnector cardConnector = this.outputConnector;
			if (((cardConnector != null) ? cardConnector.ConnectedNode : null) != null)
			{
				this.MyGameCard.StartTimer(10f, new TimerAction(this.OutputCard), SokLoc.Translate("idea_resourcechest_status_2"), base.GetActionId("OutputCard"), true, false, false);
				goto IL_0232;
			}
		}
		this.MyGameCard.CancelTimer(base.GetActionId("OutputCard"));
		IL_0232:
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

	public void StoreFood()
	{
		foreach (GameCard gameCard in this.MyGameCard.GetChildCards())
		{
			if (string.IsNullOrEmpty(this.HeldCardId))
			{
				this.HeldCardId = gameCard.CardData.Id;
			}
			if (!(gameCard.CardData.Id != this.HeldCardId) && !(gameCard.CardData is Hotpot) && !(gameCard.CardData is FoodWarehouse))
			{
				if (gameCard.SpecialValue != null)
				{
					int? num = this.FoodValue + gameCard.SpecialValue;
					int maxFoodValue = this.MaxFoodValue;
					if ((num.GetValueOrDefault() <= maxFoodValue) & (num != null))
					{
						this.FoodValue += gameCard.SpecialValue.Value;
						gameCard.DestroyCard(true, true);
						continue;
					}
				}
				Food food = gameCard.CardData as Food;
				if (food != null)
				{
					int num2 = Mathf.Min(this.MaxFoodValue - this.FoodValue, food.FoodValue);
					this.FoodValue += num2;
					food.FoodValue -= num2;
					if (food.FoodValue <= 0)
					{
						gameCard.DestroyCard(true, true);
					}
					else
					{
						gameCard.RemoveFromParent();
					}
				}
			}
		}
	}

	public GameCard RemoveFood(int count, bool checkOutput = false)
	{
		Food food = WorldManager.instance.GameDataLoader.GetCardFromId(this.HeldCardId, true) as Food;
		List<GameCard> list = new List<GameCard>();
		for (int i = 0; i < count; i++)
		{
			CardData cardData;
			if (this.FoodValue >= food.FoodValue)
			{
				cardData = WorldManager.instance.CreateCard(base.transform.position, this.HeldCardId, true, false, true);
				this.FoodValue -= food.FoodValue;
			}
			else
			{
				int num = Mathf.Min(this.FoodValue, food.FoodValue);
				cardData = WorldManager.instance.CreateCard(base.transform.position, this.HeldCardId, true, false, true);
				Food food2 = cardData as Food;
				if (food2 != null)
				{
					food2.FoodValue = num;
				}
				this.FoodValue -= num;
			}
			if (cardData != null)
			{
				list.Add(cardData.MyGameCard);
			}
			if (this.FoodValue <= 0)
			{
				this.FoodValue = 0;
				break;
			}
		}
		WorldManager.instance.Restack(list);
		if (checkOutput)
		{
			WorldManager.instance.StackSendCheckTarget(this.MyGameCard, list[0], this.OutputDir, null, true, -1);
		}
		else
		{
			WorldManager.instance.StackSend(list[0], this.OutputDir, null, true);
		}
		return list[0].GetRootCard();
	}

	[TimedAction("output_card")]
	public void OutputCard()
	{
		if (this.FoodValue > 0)
		{
			this.RemoveFood(1, true);
		}
	}

	public override void Clicked()
	{
		int num = 1;
		if (InputController.instance.GetKey(Key.LeftShift) || InputController.instance.GetKey(Key.RightShift))
		{
			num = 5;
		}
		if (this.FoodValue > 0)
		{
			this.RemoveFood(num, false);
		}
		if (this.FoodValue == 0)
		{
			this.HeldCardId = null;
		}
		base.Clicked();
	}

	private int MaxFoodValue = 999;

	[ExtraData("resource_id")]
	[HideInInspector]
	public string HeldCardId = "";

	private CardConnector outputConnector;
}
