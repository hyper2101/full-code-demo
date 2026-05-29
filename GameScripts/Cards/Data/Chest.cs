using System;
using System.Linq;
using UnityEngine;

public class Chest : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == this.Id || (otherCard.Id == this.HeldCardId && this.GetChestWithSpace() != null);
	}

	private Chest GetChestWithSpace()
	{
		GameCard gameCard = this.MyGameCard.GetAllCardsInStack().FirstOrDefault<GameCard>(delegate(GameCard x)
		{
			Chest chest = x.CardData as Chest;
			return chest != null && chest.CoinCount < chest.MaxCoinCount;
		});
		if (gameCard == null)
		{
			return null;
		}
		return gameCard.CardData as Chest;
	}

	public override void UpdateCard()
	{
		this.Value = this.CoinCount;
		if (!this.MyGameCard.HasParent || this.MyGameCard.Parent.CardData is HeavyFoundation)
		{
			foreach (GameCard gameCard in this.MyGameCard.GetChildCards())
			{
				if (!(gameCard.CardData.Id != this.HeldCardId))
				{
					Chest chestWithSpace = this.GetChestWithSpace();
					if (!(chestWithSpace != null))
					{
						gameCard.RemoveFromParent();
						break;
					}
					if (chestWithSpace.CoinCount < chestWithSpace.MaxCoinCount)
					{
						gameCard.DestroyCard(true, true);
						chestWithSpace.CoinCount++;
					}
				}
			}
		}
		this.descriptionOverride = MewtationsLoc.Translate(this.ChestTerm, new LocParam[]
		{
			LocParam.Create("count", this.CoinCount.ToString()),
			LocParam.Create("max_count", this.MaxCoinCount.ToString()),
			LocParam.Create("goldicon", Icons.Gold),
			LocParam.Create("shellicon", Icons.Shell)
		});
		base.UpdateCard();
	}

	public override void Clicked()
	{
		int num = 5;
		if (this.CoinCount > 0)
		{
			int num2 = Mathf.Min(num, this.CoinCount);
			GameCard gameCard = WorldManager.instance.CreateCardStack(base.transform.position + Vector3.up * 0.2f, num2, this.HeldCardId, false);
			WorldManager.instance.StackSend(gameCard.GetRootCard(), this.OutputDir, null, false);
			this.CoinCount -= num2;
		}
		base.Clicked();
	}

	[ExtraData("coin_count")]
	[HideInInspector]
	public int CoinCount;

	public int MaxCoinCount = 100;

	public string HeldCardId = "gold";

	public string ChestTerm = "card_coin_chest_description_long";
}
