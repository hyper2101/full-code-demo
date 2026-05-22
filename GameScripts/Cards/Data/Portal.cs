using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Portal : CardData
{
	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return !WorldManager.instance.CurrentRunOptions.IsPeacefulMode && this.CardIsAllowedInPortal(otherCard) && otherCard.AllChildrenMatchPredicate((CardData x) => this.CardIsAllowedInPortal(x));
	}

	public override bool DetermineCanHaveCardsWhenIsRoot
	{
		get
		{
			return true;
		}
	}

	public bool IsTakingPortal
	{
		get
		{
			return this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == base.GetActionId("TakePortal");
		}
	}

	public override bool CanBeDragged
	{
		get
		{
			return !this.IsTakingPortal;
		}
	}

	private bool CardIsAllowedInPortal(CardData otherCard)
	{
		return otherCard.MyCardType == CardType.Humans || otherCard is CatCardData || otherCard.BackpackCapacity > 0;
	}

	public override void UpdateCard()
	{
		base.UpdateCard();
	}

	public void RemoveNonHuman()
	{
		GameCard parent = this.MyGameCard.Parent;
		base.RestackChildrenMatchingPredicate((CardData x) => x.MyCardType != CardType.Humans);
		if (parent != null && parent.CardData is HeavyFoundation)
		{
			this.MyGameCard.Parent = parent;
		}
	}

	public void RemoveLastVillagerInPortal()
	{
		GameCard parent = this.MyGameCard.Parent;
		List<GameCard> childCards = this.MyGameCard.GetChildCards();
		for (int i = childCards.Count - 1; i >= 0; i--)
		{
			if (childCards[i].CardData is BaseVillager)
			{
				childCards[i].RemoveFromParent();
				break;
			}
		}
		if (parent != null && parent.CardData is HeavyFoundation)
		{
			this.MyGameCard.Parent = parent;
		}
	}

	public void RemoveExcessVillagersInPortal()
	{
		GameCard parent = this.MyGameCard.Parent;
		List<GameCard> childCards = this.MyGameCard.GetChildCards();
		for (int i = childCards.Count - 1; i >= this.MaxVillagerCount; i--)
		{
			if (childCards[i].CardData is BaseVillager)
			{
				childCards[i].RemoveFromParent();
			}
		}
		if (parent != null && parent.CardData is HeavyFoundation)
		{
			this.MyGameCard.Parent = parent;
		}
	}

	[TimedAction("take_portal")]
	public void TakePortal()
	{
		if (!TransitionScreen.InTransition && !WorldManager.instance.InAnimation)
		{
			GameCanvas.instance.ChangeLocationPrompt(new Action(this.GoAway), new Action(this.Stay), "forest");
		}
	}

	public void Stay()
	{
		GameCard parent = this.MyGameCard.Parent;
		base.RestackChildrenMatchingPredicate((CardData c) => c is BaseVillager);
		if (parent != null && parent.CardData is HeavyFoundation)
		{
			this.MyGameCard.Parent = parent;
		}
	}

	private void RemoveStacksFromAllPortals()
	{
		foreach (CardData cardData in WorldManager.instance.CardQuery.GetCards<StablePortal>().Cast<CardData>().Concat<CardData>(WorldManager.instance.CardQuery.GetCards<StrangePortal>().Cast<CardData>()))
		{
			if (!(cardData == this) && cardData.MyGameCard.HasChild)
			{
				cardData.MyGameCard.Child.RemoveFromParent();
			}
		}
	}

	private void GoAway()
	{
		List<CardData> catsInStack = this.MyGameCard.GetAllCardsInStack().Select(c => c.CardData).Where(d => d is CatCardData).ToList();
		if (catsInStack.Count > 0)
		{
			CardData backpackCard = this.MyGameCard.GetAllCardsInStack().Select(c => c.CardData).FirstOrDefault(d => d.BackpackCapacity > 0);
			
			foreach (var cat in catsInStack)
			{
				if (cat.MyGameCard != null)
				{
					cat.MyGameCard.RemoveFromStack();
					cat.MyGameCard.gameObject.SetActive(false);
				}
			}
			if (backpackCard != null && backpackCard.MyGameCard != null)
			{
				backpackCard.MyGameCard.RemoveFromStack();
				backpackCard.MyGameCard.gameObject.SetActive(false);
			}

			Mewtations.Expedition.ExpeditionManager.Instance.StartExpedition(this.MyGameCard, catsInStack.Cast<CatCardData>().ToList(), backpackCard);
			return;
		}

		this.RemoveStacksFromAllPortals();
		GameCanvas.instance.SetScreen<CutsceneScreen>();
		GameBoard targetBoard = WorldManager.instance.GetBoardWithId("forest");
		WorldManager.instance.GoToBoard(targetBoard, delegate
		{
			GameCanvas.instance.SetScreen<GameScreen>();
			WorldManager.instance.SendToBoard(this.MyGameCard.Child, targetBoard, new Vector2(0.4f, 0.5f));
			this.RestackChildrenMatchingPredicate((CardData v) => v is BaseVillager);
			if (this is StrangePortal)
			{
				this.MyGameCard.DestroyCard(false, true);
			}
		}, "default");
	}

	public int MaxVillagerCount = 7;
}
