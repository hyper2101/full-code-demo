using System;
using System.Collections.Generic;
using UnityEngine;

public class University : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		if (this.AllInventionsFound())
		{
			return false;
		}
		if (!(otherCard.Id == this.HeldCardId))
		{
			Chest chest = otherCard as Chest;
			return chest != null && chest.HeldCardId == this.HeldCardId;
		}
		return true;
	}

	public override void UpdateCardText()
	{
		if (this.AllInventionsFound())
		{
			this.descriptionOverride = SokLoc.Translate("card_university_description_completed");
			return;
		}
		if (this.CoinCount > 0)
		{
			this.descriptionOverride = SokLoc.Translate("card_university_description_long", new LocParam[]
			{
				LocParam.Create("count", this.CoinCount.ToString()),
				LocParam.Create("max_count", this.InventionCost.ToString())
			});
			return;
		}
		this.descriptionOverride = SokLoc.Translate("card_university_description", new LocParam[] { LocParam.Create("max_count", this.InventionCost.ToString()) });
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
					if (chest.CoinCount < this.InventionCost - this.CoinCount)
					{
						this.CoinCount += chest.CoinCount;
						chest.CoinCount = 0;
						WorldManager.instance.CreateSmoke(this.MyGameCard.transform.position);
						chest.MyGameCard.RemoveFromStack();
						chest.MyGameCard.SendIt();
					}
					else if (chest.CoinCount >= this.InventionCost - this.CoinCount)
					{
						chest.CoinCount -= this.InventionCost - this.CoinCount;
						this.CoinCount = this.InventionCost;
						WorldManager.instance.CreateSmoke(this.MyGameCard.transform.position);
						chest.MyGameCard.RemoveFromStack();
						chest.MyGameCard.SendIt();
					}
				}
				if (!(gameCard.CardData.Id != this.HeldCardId))
				{
					if (this.CoinCount >= this.InventionCost)
					{
						gameCard.RemoveFromParent();
						break;
					}
					gameCard.DestroyCard(true, true);
					this.CoinCount++;
				}
			}
			if (this.CoinCount == this.InventionCost)
			{
				this.MyGameCard.StartTimer(10f, new TimerAction(this.GiveInvention), SokLoc.Translate("card_university_status"), base.GetActionId("GiveInvention"), true, false, false);
			}
		}
		if (this.AllInventionsFound())
		{
			this.MyGameCard.CancelTimer(base.GetActionId("GiveInvention"));
		}
		base.UpdateCard();
	}

	private bool AllInventionsFound()
	{
		bool flag = true;
		foreach (string text in this.BlueprintDrops)
		{
			if (!WorldManager.instance.HasFoundCard(text))
			{
				flag = false;
				break;
			}
		}
		return (!WorldManager.instance.IsCitiesDlcActive() || WorldManager.instance.HasFoundCard("industrial_revolution")) && flag;
	}

	[TimedAction("give_invention")]
	public void GiveInvention()
	{
		if (WorldManager.instance.IsCitiesDlcActive() && !WorldManager.instance.HasFoundCard("industrial_revolution"))
		{
			CardData cardData = WorldManager.instance.CreateCard(this.MyGameCard.transform.position, "industrial_revolution", true, false, true);
			WorldManager.instance.CreateSmoke(cardData.transform.position);
			cardData.MyGameCard.SendIt();
			AudioManager.me.PlaySound2D(this.InventionSound, 1f, 0.1f);
			this.CoinCount = 0;
			return;
		}
		foreach (string text in this.BlueprintDrops)
		{
			Blueprint blueprint = WorldManager.instance.GameDataLoader.GetCardFromId(text, true) as Blueprint;
			if (blueprint && !WorldManager.instance.HasFoundCard(blueprint.Id))
			{
				CardData cardData2 = WorldManager.instance.CreateCard(this.MyGameCard.transform.position, blueprint, true, false, true, true);
				WorldManager.instance.CreateSmoke(cardData2.transform.position);
				cardData2.MyGameCard.SendIt();
				AudioManager.me.PlaySound2D(this.InventionSound, 1f, 0.1f);
				this.CoinCount = 0;
				break;
			}
		}
	}

	public int InventionCost = 50;

	[Card]
	public List<string> BlueprintDrops = new List<string>();

	public List<AudioClip> InventionSound;

	public Sprite SpecialIcon;

	[ExtraData("coin_count")]
	[HideInInspector]
	public int CoinCount;

	private string HeldCardId = "gold";
}
