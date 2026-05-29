using System;
using System.Collections.Generic;
using System.Linq;

public class FishTrap : CardData
{
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

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is Food;
	}

	public override void UpdateCard()
	{
		Food food;
		if (base.HasCardOnTop<Food>(out food))
		{
			this.MyGameCard.StartTimer(this.FishTime, new TimerAction(this.CompleteFishing), MewtationsLoc.Translate("card_fish_trap_status"), "complete_fishing", true, false, false);
		}
		else
		{
			this.MyGameCard.CancelTimer("complete_fishing");
		}
		base.UpdateCard();
	}

	[TimedAction("complete_fishing")]
	public void CompleteFishing()
	{
		Food food;
		base.HasCardOnTop<Food>(out food);
		BaitBag baitBag = this.BaitBags.FirstOrDefault<BaitBag>((BaitBag x) => x.BaitId == food.Id);
		if (baitBag == null)
		{
			baitBag = this.DefaultBaitBag;
		}
		ICardId cardId = baitBag.GetCard(false);
		if (cardId == null)
		{
			cardId = (CardId)"cod";
		}
		CardData cardData = WorldManager.instance.CreateCard(base.transform.position, cardId, false, false, true);
		WorldManager.instance.StackSendCheckTarget(this.MyGameCard, cardData.MyGameCard, this.OutputDir, null, true, -1);
		base.DestroyChildrenMatchingPredicateAndRestack((CardData c) => c == food, 1);
	}

	public BaitBag DefaultBaitBag;

	public List<BaitBag> BaitBags;

	public float FishTime = 30f;
}
