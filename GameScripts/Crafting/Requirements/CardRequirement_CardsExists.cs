using System;

[Serializable]
public class CardRequirement_CardsExists : CardRequirement
{
	public override string RequirementDescriptionNeed(int multiplier)
	{
		CardData cardPrefab = WorldManager.instance.GetCardPrefab(this.CardId, true);
		cardPrefab.UpdateCardText();
		string text = string.Format("{0}", this.Amount * multiplier);
		return MewtationsLoc.Translate("label_requirement_take_card", new LocParam[]
		{
			LocParam.Create("amount", text),
			LocParam.Create("card", cardPrefab.Name)
		});
	}

	public override string RequirementDescriptionNeedNegative(int multiplier)
	{
		CardData cardPrefab = WorldManager.instance.GetCardPrefab(this.CardId, true);
		cardPrefab.UpdateCardText();
		string text = string.Format("{0}", this.Amount * multiplier);
		return MewtationsLoc.Translate("label_requirement_take_card_negative", new LocParam[]
		{
			LocParam.Create("amount", text),
			LocParam.Create("card", cardPrefab.Name)
		});
	}

	public override bool Satisfied(GameCard card)
	{
		return WorldManager.instance.GetCards(this.CardId).Count >= this.Amount;
	}

	[Card]
	public string CardId;

	public int Amount;
}
