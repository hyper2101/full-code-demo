using System;
using UnityEngine;

public class Poop : CardData
{
	public bool CanMakeSick
	{
		get
		{
			bool flag = true;
			if (this.MyGameCard.Parent != null && this.MyGameCard.Parent.CardData is Cesspool)
			{
				flag = false;
			}
			if (this.MyGameCard.CardData.CreationMonth == WorldManager.instance.Time.CurrentMonth)
			{
				flag = false;
			}
			return flag;
		}
	}

	public override void OnInitialCreate()
	{
		CardData nearestCardMatchingPred = WorldManager.instance.GetNearestCardMatchingPred(this.MyGameCard, (GameCard x) => x.CardData.Id == "sewer");
		if (nearestCardMatchingPred != null)
		{
			WorldManager.instance.StackSendTo(this.MyGameCard, nearestCardMatchingPred.MyGameCard);
		}
		base.OnInitialCreate();
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.MyCardType == CardType.Resources || otherCard.MyCardType == CardType.Humans || otherCard.MyCardType == CardType.Food || otherCard.Id == this.Id || (otherCard.MyCardType == CardType.Structures && !otherCard.IsBuilding);
	}

	public override void UpdateCardText()
	{
		if (WorldManager.instance.CurseIsActive(CurseType.Death))
		{
			this.descriptionOverride = MewtationsLoc.Translate(this.DescriptionTerm) + "\n\n<i>" + MewtationsLoc.Translate("card_poop_cant_sell") + "</i>";
		}
		else
		{
			this.descriptionOverride = null;
		}
		base.UpdateCardText();
	}

	public override void UpdateCard()
	{
		if (WorldManager.instance.CurseIsActive(CurseType.Death))
		{
			this.Value = -1;
		}
		else
		{
			this.Value = 1;
		}
		base.UpdateCard();
	}

	public float SickChance = 20f;

	public AudioClip PoopSound;
}
