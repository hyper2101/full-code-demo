using System;
using System.Collections.Generic;

public class BlueprintOffspring : Blueprint
{
	public override void Init(GameDataLoader loader)
	{
		base.Init(loader);
		this.Subprints = (loader.SpiritDlcLoaded ? this.SpiritsSubprints : this.BaseSubprints);
	}

	public override void BlueprintComplete(GameCard rootCard, List<GameCard> involvedCards, Subprint print)
	{
		CardData cardData = WorldManager.instance.CreateCard(rootCard.transform.position, print.ResultCard, false, false, true);
		cardData.MyGameCard.SendIt();
		House house = null;
		for (int i = involvedCards.Count - 1; i >= 0; i--)
		{
			GameCard gameCard = involvedCards[i];
			gameCard.RemoveFromStack();
			if (gameCard.CardData is House)
			{
				house = (House)gameCard.CardData;
			}
			gameCard.SendIt();
		}
		if (house != null)
		{
			cardData.MyGameCard.SetParent(house.MyGameCard);
		}
	}

	public List<Subprint> BaseSubprints;

	public List<Subprint> SpiritsSubprints;
}
