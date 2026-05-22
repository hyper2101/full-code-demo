using System;
using UnityEngine;

public class Hotpot : Food
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		Food food = otherCard as Food;
		return (food == null || food.FoodValue != 0) && otherCard.MyCardType == CardType.Food;
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
		if (!this.MyGameCard.HasParent || this.MyGameCard.Parent.CardData is HeavyFoundation)
		{
			if (this.MyGameCard.HasChild && !this.MyGameCard.TimerRunning && !(this.MyGameCard.Child.CardData is Hotpot))
			{
				this.MyGameCard.StartTimer(10f, new TimerAction(this.CookFood), SokLoc.Translate("card_hotpot_name"), base.GetActionId("CookFood"), true, false, false);
			}
			if (!this.MyGameCard.HasChild && this.MyGameCard.TimerRunning)
			{
				this.MyGameCard.CancelTimer(base.GetActionId("CookFood"));
			}
		}
		GameCard rootCard = this.MyGameCard.GetRootCard();
		if (rootCard != null && rootCard.CardData is MessHall)
		{
			this.MyGameCard.CancelTimer(base.GetActionId("CookFood"));
		}
		if (this.FoodValue > 0)
		{
			this.descriptionOverride = "";
		}
		base.UpdateCard();
	}

	[TimedAction("cook_food")]
	public void CookFood()
	{
		foreach (GameCard gameCard in this.MyGameCard.GetChildCards())
		{
			if (!(gameCard.CardData is Hotpot))
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
				}
			}
		}
	}

	private int MaxFoodValue = 50;
}
