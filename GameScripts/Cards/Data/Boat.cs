using System;
using System.Collections.Generic;
using UnityEngine;

public class Boat : CardData
{
	public override bool DetermineCanHaveCardsWhenIsRoot
	{
		get
		{
			return true;
		}
	}

	public bool IsSailingOff
	{
		get
		{
			return this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == base.GetActionId("SailOff");
		}
	}

	public bool InSailOff
	{
		get
		{
			return this.InSailOffPrompt;
		}
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return base.GetChildCount() + (otherCard.GetChildCount() + 1) <= this.MaxCapacity && this.CardIsAllowedOnBoat(otherCard) && otherCard.AllChildrenMatchPredicate((CardData x) => this.CardIsAllowedOnBoat(x));
	}

	private bool CardIsAllowedOnBoat(CardData otherCard)
	{
		return !(otherCard.Id == "trained_monkey") && (otherCard.MyCardType == CardType.Food || otherCard.MyCardType == CardType.Humans || otherCard.MyCardType == CardType.Resources);
	}

	public override bool CanBeDragged
	{
		get
		{
			return !this.IsSailingOff;
		}
	}

	public override void UpdateCard()
	{
		if (!TransitionScreen.InTransition && !WorldManager.instance.InAnimation)
		{
			int num = base.ChildrenMatchingPredicateCount((CardData x) => x is BaseVillager);
			if (num > 0)
			{
				if (!WorldManager.instance.CurrentBoard.BoardOptions.CanTravelToIsland)
				{
					GameCanvas.instance.ShowCantChangeBoardSpirit();
					this.Stay();
					return;
				}
				this.RemoveTrainedMonkeys();
				int cardCount = WorldManager.instance.GetCardCount((CardData x) => x is BaseVillager);
				int requiredFoodCount = WorldManager.instance.GetRequiredFoodCount();
				if (WorldManager.instance.GetFoodCount(true) < requiredFoodCount)
				{
					this.MyGameCard.Child.RemoveFromParent();
					GameCanvas.instance.NotEnoughFoodToSailOffPrompt();
				}
				else if (WorldManager.instance.CurrentBoard.Id == "main" && num == cardCount)
				{
					this.MyGameCard.CancelTimer(base.GetActionId("SailOff"));
					GameCanvas.instance.OneVillagerNeedsToStayPrompt("label_sailing_off_title");
					this.RemoveLastVillagerOnBoat();
				}
				else
				{
					this.MyGameCard.StartTimer(this.TravelTime, new TimerAction(this.SailOff), MewtationsLoc.Translate("card_boat_status"), base.GetActionId("SailOff"), true, false, false);
				}
			}
			else
			{
				this.MyGameCard.CancelTimer(base.GetActionId("SailOff"));
			}
		}
		base.UpdateCard();
	}

	private void RemoveTrainedMonkeys()
	{
		List<GameCard> allCardsInStack = this.MyGameCard.GetAllCardsInStack();
		for (int i = allCardsInStack.Count - 1; i >= 0; i--)
		{
			if (allCardsInStack[i].CardData.Id == "trained_monkey")
			{
				allCardsInStack.RemoveAt(i);
				break;
			}
		}
		WorldManager.instance.Restack(allCardsInStack);
	}

	private void RemoveLastVillagerOnBoat()
	{
		List<GameCard> allCardsInStack = this.MyGameCard.GetAllCardsInStack();
		for (int i = allCardsInStack.Count - 1; i >= 0; i--)
		{
			if (allCardsInStack[i].CardData is BaseVillager)
			{
				allCardsInStack.RemoveAt(i);
				break;
			}
		}
		WorldManager.instance.Restack(allCardsInStack);
	}

	[TimedAction("sail_off")]
	public void SailOff()
	{
		if (!TransitionScreen.InTransition && !WorldManager.instance.InAnimation)
		{
			GameCanvas.instance.ChangeLocationPrompt(new Action(this.GoAway), new Action(this.Stay), "island");
		}
	}

	private void Stay()
	{
		GameCard parent = this.MyGameCard.Parent;
		base.RestackChildrenMatchingPredicate((CardData c) => c is BaseVillager);
		if (parent != null && parent.CardData is HeavyFoundation)
		{
			this.MyGameCard.SetParent(parent);
		}
	}

	private void RemoveStacksFromAllBoats()
	{
		foreach (Boat boat in WorldManager.instance.CardQuery.GetCards<Boat>())
		{
			if (!(boat == this) && boat.MyGameCard.HasChild)
			{
				boat.MyGameCard.Child.RemoveFromParent();
			}
		}
	}

	private void GoAway()
	{
		EndOfMonthParameters endOfMonthParameters = new EndOfMonthParameters();
		endOfMonthParameters.SkipSpecialEvents = true;
		endOfMonthParameters.CutsceneTitle = MewtationsLoc.Translate("label_sailing_off_full");
		endOfMonthParameters.SkipEndConfirmation = true;
		this.InSailOffPrompt = true;
		this.RemoveStacksFromAllBoats();
		endOfMonthParameters.OnDone = delegate
		{
			this.InSailOffPrompt = false;
			GameCanvas.instance.SetScreen<CutsceneScreen>();
			string text;
			if (WorldManager.instance.CurrentBoard.Id == "main")
			{
				text = "island";
			}
			else
			{
				text = "main";
			}
			GameBoard targetBoard = WorldManager.instance.GetBoardWithId(text);
			WorldManager.instance.GoToBoard(targetBoard, delegate
			{
				GameCanvas.instance.SetScreen<GameScreen>();
				WorldManager.instance.SendStackToBoard(this.MyGameCard, targetBoard, new Vector2(0.4f, 0.5f));
				this.RestackChildrenMatchingPredicate((CardData v) => v is BaseVillager);
			}, "default");
		};
		WorldManager.instance.ForceEndOfMoon(endOfMonthParameters);
	}

	public int MaxCapacity = 5;

	public float TravelTime = 30f;

	public bool InSailOffPrompt;
}
