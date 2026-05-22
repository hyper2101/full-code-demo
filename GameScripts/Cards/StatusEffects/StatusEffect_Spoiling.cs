using System;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect_Spoiling : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "spoiling";
		}
	}

	public override Sprite Sprite
	{
		get
		{
			return SpriteManager.instance.SpoilingEffect;
		}
	}

	public override void Update()
	{
		this.FillAmount = new float?(1f - this.StatusTimer / WorldManager.instance.MonthTime);
		float monthTime = WorldManager.instance.MonthTime;
		bool flag = base.ParentCard.MyGameCard.GetCardWithStatusInStack() == null;
		if (WorldManager.instance.InAnimation)
		{
			flag = false;
		}
		if (this.StatusTimer >= monthTime && flag)
		{
			Food food = base.ParentCard as Food;
			food.FoodValue -= 2;
			if (food.FoodValue <= 0)
			{
				CardData cardData = WorldManager.instance.CreateCard(base.ParentCard.transform.position, "goop", false, false, true);
				WorldManager.instance.StackSend(cardData.MyGameCard, base.ParentCard.OutputDir, null, true);
				List<GameCard> allCardsInStack = base.ParentCard.MyGameCard.GetAllCardsInStack();
				allCardsInStack.Remove(base.ParentCard.MyGameCard);
				base.ParentCard.MyGameCard.DestroyCard(true, false);
				WorldManager.instance.Restack(allCardsInStack);
			}
			this.StatusTimer -= monthTime;
		}
		base.Update();
	}
}
