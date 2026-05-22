using System;
using UnityEngine;

public class FilteredJunction : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.MyCardType != CardType.Structures && otherCard.MyCardType != CardType.Humans;
	}

	public override void UpdateCard()
	{
		if (this.MyGameCard.HasChild)
		{
			if (string.IsNullOrEmpty(this.FilteredCard))
			{
				this.FilteredCard = this.MyGameCard.Child.CardData.Id;
				this.MyGameCard.Child.DestroyCard(true, true);
				return;
			}
			for (int i = this.MyGameCard.GetChildCards().Count - 1; i >= 0; i--)
			{
				int num = -1;
				GameCard gameCard = this.MyGameCard.GetChildCards()[i];
				if (!string.IsNullOrEmpty(this.FilteredCard))
				{
					if (gameCard.CardData.Id == this.FilteredCard)
					{
						num = 1;
					}
					else
					{
						num = 0;
					}
				}
				gameCard.RemoveFromStack();
				WorldManager.instance.StackSendCheckTarget(this.MyGameCard, gameCard, this.OutputDir, null, true, num);
			}
		}
		base.UpdateCard();
	}

	public override void Clicked()
	{
		if (!string.IsNullOrEmpty(this.FilteredCard))
		{
			WorldManager.instance.CreateCard(base.Position, this.FilteredCard, true, false, true).MyGameCard.SendIt();
			this.FilteredCard = "";
		}
		base.Clicked();
	}

	public override void UpdateCardText()
	{
		if (!string.IsNullOrEmpty(this.FilteredCard))
		{
			this.nameOverride = SokLoc.Translate(this.NameOverride, new LocParam[] { LocParam.Create("card", WorldManager.instance.GameDataLoader.GetCardFromId(this.FilteredCard, true).Name) });
			return;
		}
		this.nameOverride = null;
	}

	[ExtraData("filtered_card")]
	[HideInInspector]
	public string FilteredCard;

	[Term]
	public string NameOverride;
}
