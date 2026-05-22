using System;

public class BreedingPen : CardData
{
	public override bool DetermineCanHaveCardsWhenIsRoot
	{
		get
		{
			return true;
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		int childCount = this.MyGameCard.GetChildCount();
		if (childCount == 0)
		{
			Animal animal = otherCard as Animal;
			return animal != null && animal.IsBreedable;
		}
		return childCount == 1 && this.MyGameCard.Child.CardData.Id == otherCard.Id;
	}

	public override void UpdateCard()
	{
		if (this.MyGameCard.GetChildCount() == 2)
		{
			this.MyGameCard.StartTimer(120f, new TimerAction(this.BreedAnimals), SokLoc.Translate("action_breeding_status"), base.GetActionId("BreedAnimals"), true, false, false);
		}
		else if (this.MyGameCard.GetChildCount() > 2)
		{
			GameCard gameCard = this.MyGameCard.TryGetNthChild(3);
			if (gameCard != null)
			{
				gameCard.RemoveFromParent();
			}
			this.MyGameCard.CancelTimer(base.GetActionId("BreedAnimals"));
		}
		else
		{
			this.MyGameCard.CancelTimer(base.GetActionId("BreedAnimals"));
		}
		base.UpdateCard();
	}

	[TimedAction("breed_animals")]
	public void BreedAnimals()
	{
		CardData cardData = WorldManager.instance.CreateCard(base.transform.position, this.MyGameCard.Child.CardData.Id, true, true, true);
		WorldManager.instance.StackSendCheckTarget(this.MyGameCard, cardData.MyGameCard, this.OutputDir, null, true, -1);
		GameCard child = this.MyGameCard.Child;
		if (child.Child != null)
		{
			GameCard child2 = child.Child;
			child2.RemoveFromStack();
			WorldManager.instance.StackSend(child2, this.OutputDir, null, true);
		}
		QuestManager.instance.SpecialActionComplete("breed_" + cardData.Id, null);
		child.RemoveFromStack();
		WorldManager.instance.StackSend(child, this.OutputDir, null, true);
	}
}
