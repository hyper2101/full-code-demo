using System;

public class Laboratory : EnergyHarvestable
{
	public override void UpdateCard()
	{
		CardData cardData;
		if (base.HasCardOnTop("fossil", out cardData) && !this.InCutscene)
		{
			this.InCutscene = true;
			WorldManager.instance.Cutscene.QueueCutscene(CitiesCutscenes.DinoBoss(this, cardData));
		}
		base.UpdateCard();
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == "science" || otherCard.Id == "fossil";
	}

	public bool InCutscene;
}
