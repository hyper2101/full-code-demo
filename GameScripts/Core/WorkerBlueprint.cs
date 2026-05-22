using System;
using System.Collections.Generic;

public class WorkerBlueprint : Blueprint
{
	public override void BlueprintComplete(GameCard rootCard, List<GameCard> involvedCards, Subprint print)
	{
		CardData cardData = WorldManager.instance.CreateCard(rootCard.transform.position, print.ResultCard, false, false, true);
		Apartment apartment = null;
		for (int i = involvedCards.Count - 1; i >= 0; i--)
		{
			GameCard gameCard = involvedCards[i];
			gameCard.RemoveFromStack();
			if (gameCard.CardData is Apartment)
			{
				apartment = (Apartment)gameCard.CardData;
			}
			else
			{
				gameCard.SendIt();
			}
		}
		if (apartment != null)
		{
			cardData.MyGameCard.SetParent(apartment.MyGameCard);
		}
	}
}
