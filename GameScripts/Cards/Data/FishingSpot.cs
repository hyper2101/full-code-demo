using System;

public class FishingSpot : Harvestable
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == "rope" || otherCard.Id == "stone" || otherCard.Id == "sandstone" || base.CanHaveCard(otherCard);
	}

	public override ICardId GetCardToGive()
	{
		BaseVillager baseVillager;
		if (base.HasCardOnTop<BaseVillager>(out baseVillager) && baseVillager.Id == "fisher")
		{
			return this.FisherCardBag.GetCard(true);
		}
		return this.NormalCardBag.GetCard(true);
	}

	public CardBag NormalCardBag;

	public CardBag FisherCardBag;
}
