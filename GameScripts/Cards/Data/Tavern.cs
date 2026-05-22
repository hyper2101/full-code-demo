using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tavern : CardData
{
	private List<string> givenCards
	{
		get
		{
			if (this._givenCards == null)
			{
				this._givenCards = this.SavedGivenCardIds.Split(',', StringSplitOptions.None).ToList<string>();
			}
			return this._givenCards;
		}
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

	public void GiveCard(CardData card)
	{
		if (this.CardWasGiven(card))
		{
			return;
		}
		this.givenCards.Add(card.Id);
		this.UpdateData();
	}

	public bool CardWasGiven(CardData card)
	{
		return this.givenCards.Contains(card.Id);
	}

	private void UpdateData()
	{
		this.SavedGivenCardIds = string.Join(",", this.givenCards);
	}

	public override void UpdateCard()
	{
		base.UpdateCard();
		Food food;
		if (base.HasCardOnTop<Food>(out food))
		{
			this.MyGameCard.StartTimer(30f, new TimerAction(this.ResearchedFood), SokLoc.Translate("card_tavern_status_0"), base.GetActionId("ResearchedFood"), true, false, false);
			return;
		}
		this.MyGameCard.CancelTimer(base.GetActionId("ResearchedFood"));
	}

	[TimedAction("research_food")]
	public void ResearchedFood()
	{
		Food food;
		if (base.HasCardOnTop<Food>(out food))
		{
			base.RemoveFirstChildFromStack();
			food.MyGameCard.DestroyCard(false, true);
			WorldManager.instance.TryCreateHappiness(base.transform.position, Mathf.Max(1, food.FoodValue / 3));
			this.GiveCard(food);
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		Food food = otherCard as Food;
		return food != null && food.FoodValue > 0;
	}

	[ExtraData("given_card_ids")]
	[HideInInspector]
	public string SavedGivenCardIds;

	private List<string> _givenCards;
}
