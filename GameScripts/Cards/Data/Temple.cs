using System;

public class Temple : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == "goblet";
	}

	public override void UpdateCard()
	{
		CardData cardData;
		if (WorldManager.instance.IsPlaying && !WorldManager.instance.InAnimation && base.HasCardOnTop("goblet", out cardData))
		{
			QuestManager.instance.SpecialActionComplete("goblet_to_temple", this);
			WorldManager.instance.Cutscene.QueueCutscene(Cutscenes.BossFight(this, cardData));
		}
		base.UpdateCard();
	}
}
