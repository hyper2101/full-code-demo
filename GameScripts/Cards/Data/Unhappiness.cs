using System;

public class Unhappiness : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is Happiness || otherCard is Unhappiness;
	}

	public override void OnInitialCreate()
	{
		if (WorldManager.instance.CardQuery.GetCardCount<Unhappiness>() >= 20 && WorldManager.instance.GetCardCount("sadness_demon") <= 0)
		{
			WorldManager.instance.Cutscene.QueueCutsceneIfNotQueued(Cutscenes.DemonOfSadness(), "sadness_demon");
		}
		base.OnInitialCreate();
	}
}
