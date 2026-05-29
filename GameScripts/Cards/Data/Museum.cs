using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Museum : CardData
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
			this.MyGameCard.StartTimer(5f, new TimerAction(this.ResearchedItem), MewtationsLoc.Translate("card_tavern_status_0"), base.GetActionId("ResearchedItem"), true, false, false);
			return;
		}
		this.MyGameCard.CancelTimer(base.GetActionId("ResearchedItem"));
	}

	[TimedAction("research_food")]
	public void ResearchedItem()
	{
		CardData cardData;
		if (base.HasCardOnTop<CardData>(out cardData) && !this.CardWasGiven(cardData))
		{
			base.RemoveFirstChildFromStack();
			cardData.MyGameCard.DestroyCard(false, true);
			WorldManager.instance.TryCreateHappiness(base.transform.position, 2);
			this.GiveCard(cardData);
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is Resource && !this.CardWasGiven(otherCard);
	}

	[ExtraData("given_card_ids")]
	[HideInInspector]
	public string SavedGivenCardIds;

	private List<string> _givenCards;
}
