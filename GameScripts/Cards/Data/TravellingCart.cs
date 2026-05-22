using System;

public class TravellingCart : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard.MyGameCard == null)
		{
			return otherCard.Id == "gold";
		}
		return WorldManager.instance.BoughtWithGold(otherCard.MyGameCard, this.GoldToUse, true) || WorldManager.instance.BoughtWithGoldChest(otherCard.MyGameCard, this.GoldToUse);
	}

	public override void UpdateCard()
	{
		if (this.MyGameCard.HasChild)
		{
			GameCard child = this.MyGameCard.Child;
			if (WorldManager.instance.BoughtWithGold(child, this.GoldToUse, false))
			{
				WorldManager.instance.RemoveCardsFromStackPred(child, this.GoldToUse, (GameCard x) => x.CardData.Id == "gold");
				this.Buy();
			}
			else if (WorldManager.instance.BoughtWithGoldChest(child, this.GoldToUse))
			{
				WorldManager.instance.BuyWithChest(child, this.GoldToUse);
				this.Buy();
			}
		}
		base.UpdateCard();
	}

	private void Buy()
	{
		ICardId cardId = this.MyCardBag.GetCard(false);
		if (this.ItemsBought == 5 && WorldManager.instance.GetCardCount("goblet") == 0)
		{
			cardId = (CardId)"goblet";
		}
		QuestManager.instance.SpecialActionComplete("travelling_cart_buy", this);
		WorldManager.instance.CreateCard(base.transform.position, cardId, true, false, true).MyGameCard.SendIt();
		this.ItemsBought++;
	}

	public CardBag MyCardBag;

	public int GoldToUse = 3;

	[ExtraData("items_bought")]
	public int ItemsBought;
}
