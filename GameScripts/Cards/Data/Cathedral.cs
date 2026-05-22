using System;

public class Cathedral : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == "island_relic";
	}

	public override void UpdateCard()
	{
		CardData cardData;
		if (WorldManager.instance.IsPlaying && !WorldManager.instance.InAnimation && base.HasCardOnTop("island_relic", out cardData))
		{
			QuestManager.instance.SpecialActionComplete("island_relic_to_cathedral", this);
			WorldManager.instance.Cutscene.QueueCutscene(Cutscenes.BossFight2(this, cardData));
		}
		base.UpdateCard();
	}
}
