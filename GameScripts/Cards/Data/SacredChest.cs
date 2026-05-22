using System;

public class SacredChest : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == "sacred_key";
	}

	public override void UpdateCard()
	{
		CardData cardData;
		if (base.HasCardOnTop("sacred_key", out cardData))
		{
			WorldManager.instance.CreateCard(base.transform.position, "island_relic", false, false, true).MyGameCard.SendIt();
			QuestManager.instance.SpecialActionComplete("sacred_chest_opened", this);
			if (!WorldManager.instance.CurrentRunOptions.IsPeacefulMode)
			{
				WorldManager.instance.Cutscene.QueueCutscene(Cutscenes.SpawnTentacles());
			}
			else
			{
				WorldManager.instance.CurrentRunVariables.FinishedKraken = true;
			}
			cardData.MyGameCard.DestroyCard(false, true);
			this.MyGameCard.DestroyCard(false, true);
		}
		base.UpdateCard();
	}
}
