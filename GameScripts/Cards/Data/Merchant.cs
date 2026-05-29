using System;
using UnityEngine;

public class Merchant : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		if (!(otherCard.Id == this.HeldCardId))
		{
			Chest chest = otherCard as Chest;
			return chest != null && chest.HeldCardId == this.HeldCardId;
		}
		return true;
	}

	public override void UpdateCard()
	{
		if (!this.MyGameCard.HasParent || this.MyGameCard.Parent.CardData is HeavyFoundation)
		{
			foreach (GameCard gameCard in this.MyGameCard.GetChildCards())
			{
				Chest chest = gameCard.CardData as Chest;
				if (chest != null)
				{
					if (chest.CoinCount < this.AmountNeeded - this.AmountGiven)
					{
						this.AmountGiven += chest.CoinCount;
						chest.CoinCount = 0;
						WorldManager.instance.CreateSmoke(this.MyGameCard.transform.position);
						chest.MyGameCard.RemoveFromStack();
						chest.MyGameCard.SendIt();
					}
					else if (chest.CoinCount >= this.AmountNeeded - this.AmountGiven)
					{
						chest.CoinCount -= this.AmountNeeded - this.AmountGiven;
						this.AmountGiven = this.AmountNeeded;
						WorldManager.instance.CreateSmoke(this.MyGameCard.transform.position);
						chest.MyGameCard.RemoveFromStack();
						chest.MyGameCard.SendIt();
					}
				}
				if (!(gameCard.CardData.Id != this.HeldCardId))
				{
					if (this.AmountGiven >= this.AmountNeeded)
					{
						gameCard.RemoveFromParent();
						break;
					}
					gameCard.DestroyCard(true, true);
					this.AmountGiven++;
				}
			}
			if (this.AmountGiven == this.AmountNeeded)
			{
				WorldManager.instance.CreateCard(base.Position, "dragon_egg", true, true, true).MyGameCard.SendIt();
				WorldManager.instance.CreateSmoke(base.Position);
				AudioManager.me.PlaySound2D(this.BuySound, 1f, 0.3f);
				this.MyGameCard.DestroyCard(false, true);
			}
		}
		base.UpdateCard();
	}

	public override void UpdateCardText()
	{
		if (this.AmountGiven > 0)
		{
			this.descriptionOverride = MewtationsLoc.Translate("card_merchant_description_2", new LocParam[] { LocParam.Create("coinsNeeded", (this.AmountNeeded - this.AmountGiven).ToString()) });
		}
		else
		{
			this.descriptionOverride = "";
		}
		base.UpdateCardText();
	}

	public int AmountNeeded = 100;

	[ExtraData("amountGiven")]
	public int AmountGiven;

	private string HeldCardId = "gold";

	public AudioClip BuySound;
}
