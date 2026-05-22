using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class CardRequirementResult_TakeCards : CardRequirementResult
{
	public override IEnumerator EndOfCutscenePerform(GameCard card)
	{
		if (this.TakeThisCard)
		{
			card.DestroyCard(true, true);
		}
		else
		{
			List<CardData> list = WorldManager.instance.GetCards(this.CardId);
			list.Reverse();
			list = list.OrderBy<CardData, int>((CardData x) => x.MyGameCard.GetCardIndex()).ToList<CardData>();
			for (int i = 0; i < this.Amount; i++)
			{
				WorldManager.instance.CreateSmoke(list.Last<CardData>().Position);
				list.Last<CardData>().MyGameCard.DestroyCard(false, true);
			}
		}
		return null;
	}

	public override RequirementType GetRequirementType()
	{
		return RequirementType.Card;
	}

	public override IEnumerator Perform(GameCard card)
	{
		return null;
	}

	public override string RequirementDescriptionNegative(int multiplier, GameCard card)
	{
		return "";
	}

	public override string RequirementDescriptionPositive(int multiplier, GameCard card)
	{
		return "";
	}

	public bool TakeThisCard;

	[Card]
	public string CardId;

	public int Amount;

	public bool IsNegative;
}
