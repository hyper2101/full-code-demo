using System;
using System.Collections.Generic;
using System.Linq;

public class ArtGallery : Landmark
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return this.AcceptedCards.Contains(otherCard.Id);
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	public override void UpdateCard()
	{
		if (this.MyGameCard.HasChild && this.MyGameCard.Child.CardData is ICurrency)
		{
			if (base.ChildrenMatchingPredicate((CardData x) => x is ICurrency).Cast<ICurrency>().ToList<ICurrency>()
				.Sum<ICurrency>((ICurrency x) => x.CurrencyValue) >= this.ArtPrice)
			{
				if (!this.MyGameCard.TimerRunning)
				{
					this.MyGameCard.StartTimer(60f, new TimerAction(this.CreatePainting), MewtationsLoc.Translate("card_art_gallery_status_1"), base.GetActionId("CreatePainting"), true, false, false);
				}
			}
			else
			{
				this.MyGameCard.CancelTimer(base.GetActionId("CreatePainting"));
			}
		}
		else
		{
			this.MyGameCard.CancelTimer(base.GetActionId("CreatePainting"));
		}
		base.UpdateCard();
	}

	[TimedAction("create_painting")]
	public void CreatePainting()
	{
		List<ICurrency> list = base.ChildrenMatchingPredicate((CardData x) => x is ICurrency).Cast<ICurrency>().ToList<ICurrency>();
		if (list.Sum<ICurrency>((ICurrency x) => x.CurrencyValue) >= this.ArtPrice)
		{
			CitiesManager.instance.TryUseDollars(list, this.ArtPrice, true, true, true);
			CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "artwork", true, true, true);
			cardData.MyGameCard.RemoveFromStack();
			WorldManager.instance.StackSend(cardData.MyGameCard, this.OutputDir, null, true);
		}
	}

	public int ArtPrice = 50;

	[Card]
	public List<string> AcceptedCards;
}
