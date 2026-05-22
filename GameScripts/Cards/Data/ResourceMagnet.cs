using System;
using UnityEngine;

public class ResourceMagnet : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return ((otherCard.MyCardType == CardType.Resources || otherCard.MyCardType == CardType.Food || otherCard is Animal) && (otherCard.Id == this.PullCardId || !this.MyGameCard.HasChild)) || base.CanHaveCard(otherCard);
	}

	public override bool DetermineCanHaveCardsWhenIsRoot
	{
		get
		{
			return true;
		}
	}

	public override void Clicked()
	{
		this.PullCardId = null;
	}

	protected override bool CanToggleOnOff()
	{
		return WorldManager.instance.CurrentBoard.Id == "cities";
	}

	public override void UpdateCard()
	{
		if (this.MyGameCard.HasChild && (string.IsNullOrEmpty(this.PullCardId) || this.PullCardId != this.MyGameCard.Child.CardData.Id))
		{
			this.PullCardId = this.MyGameCard.Child.CardData.Id;
		}
		if (!string.IsNullOrEmpty(this.PullCardId))
		{
			this.nameOverride = SokLoc.Translate("card_resource_magnet_name_override", new LocParam[] { LocParam.Create("resource", WorldManager.instance.GameDataLoader.GetCardFromId(this.PullCardId, true).Name) });
			this.descriptionOverride = SokLoc.Translate("card_resource_magnet_description_long", new LocParam[] { LocParam.Create("resource", WorldManager.instance.GameDataLoader.GetCardFromId(this.PullCardId, true).Name) });
		}
		else
		{
			this.nameOverride = SokLoc.Translate("card_resource_magnet_name");
			this.descriptionOverride = null;
		}
		base.UpdateCard();
		if (string.IsNullOrEmpty(this.PullCardId))
		{
			this.Icon = SpriteManager.instance.EmptyTexture;
		}
		else
		{
			this.Icon = WorldManager.instance.GetCardPrefab(this.PullCardId, true).Icon;
		}
		this.MyGameCard.UpdateIcon();
	}

	[ExtraData("resource_id")]
	[HideInInspector]
	public string PullCardId;
}
