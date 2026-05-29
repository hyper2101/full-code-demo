using System;
using System.Collections.Generic;
using UnityEngine;

public class WishingWell : CardData
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

	public override void UpdateCardText()
	{
		if (this.WishCount > 0)
		{
			this.descriptionOverride = MewtationsLoc.Translate("card_wishing_well_description_long", new LocParam[]
			{
				LocParam.Plural("amount", this.WishCount),
				LocParam.Create("count", this.WishCost.ToString())
			});
			return;
		}
		this.descriptionOverride = MewtationsLoc.Translate("card_wishing_well_description", new LocParam[] { LocParam.Create("count", this.WishCost.ToString()) });
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
					if (chest.CoinCount < this.WishCost - this.CoinCount)
					{
						this.CoinCount += chest.CoinCount;
						chest.CoinCount = 0;
						WorldManager.instance.CreateSmoke(this.MyGameCard.transform.position);
						chest.MyGameCard.RemoveFromStack();
						chest.MyGameCard.SendIt();
					}
					else if (chest.CoinCount >= this.WishCost - this.CoinCount)
					{
						chest.CoinCount -= this.WishCost - this.CoinCount;
						this.CoinCount = this.WishCost;
						WorldManager.instance.CreateSmoke(this.MyGameCard.transform.position);
						chest.MyGameCard.RemoveFromStack();
						chest.MyGameCard.SendIt();
					}
				}
				if (!(gameCard.CardData.Id != this.HeldCardId))
				{
					if (this.CoinCount >= this.WishCost)
					{
						gameCard.RemoveFromParent();
						break;
					}
					gameCard.DestroyCard(true, true);
					this.CoinCount++;
				}
			}
			if (this.CoinCount == this.WishCost)
			{
				this.GiveWish();
			}
		}
		base.UpdateCard();
	}

	private void GiveWish()
	{
		AudioManager.me.PlaySound2D(this.WishSound, 1f, 0.1f);
		WorldManager.instance.CreateSmoke(base.transform.position);
		this.CoinCount = 0;
		this.WishCount++;
		int wishCount = this.WishCount;
		if (wishCount <= 10)
		{
			switch (wishCount)
			{
			case 1:
				WorldManager.instance.Cutscene.QueueCutscene(Cutscenes.Wish1(this));
				return;
			case 2:
				WorldManager.instance.Cutscene.QueueCutscene(Cutscenes.Wish2(this));
				return;
			case 3:
			case 4:
				break;
			case 5:
				WorldManager.instance.Cutscene.QueueCutscene(Cutscenes.Wish5(this));
				return;
			default:
				if (wishCount != 10)
				{
					return;
				}
				WorldManager.instance.Cutscene.QueueCutscene(Cutscenes.Wish10(this));
				return;
			}
		}
		else
		{
			if (wishCount == 20)
			{
				WorldManager.instance.Cutscene.QueueCutscene(Cutscenes.Wish20(this));
				return;
			}
			if (wishCount != 50)
			{
				return;
			}
			WorldManager.instance.Cutscene.QueueCutscene(Cutscenes.Wish50(this));
		}
	}

	public int WishCost = 500;

	public List<AudioClip> WishSound;

	public Sprite SpecialIcon;

	[ExtraData("coin_count")]
	[HideInInspector]
	public int CoinCount;

	[ExtraData("wish_count")]
	[HideInInspector]
	public int WishCount;

	private string HeldCardId = "gold";
}
