using System;
using System.Linq;
using UnityEngine;

public class CityHall : Landmark
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is Dollar || otherCard is Worker || otherCard is CitiesCombatable;
	}

	public override void UpdateCard()
	{
		if (this.MyGameCard.HasChild && this.HasEnergyInput(null))
		{
			if (base.AllChildrenMatchPredicate((CardData x) => x is Dollar))
			{
				int num = (from x in this.MyGameCard.GetChildCards()
					select x.CardData into x
					where x is Dollar
					select x).Cast<Dollar>().Sum<Dollar>((Dollar x) => x.DollarValue);
				this.DollarAmount += num;
				base.DestroyChildrenMatchingPredicateAndRestack((CardData x) => x is Dollar, base.ChildrenMatchingPredicateCount((CardData x) => x is Dollar));
				QuestManager.instance.SpecialActionComplete("card_cap_increased", null);
				if (this.MyGameCard.HasChild)
				{
					GameCard child = this.MyGameCard.Child;
					child.RemoveFromParent();
					child.SendIt();
				}
			}
			else if (this.MyGameCard.GetChildCount() == 1 && (this.MyGameCard.Child.CardData is Worker || this.MyGameCard.Child.CardData is CitiesCombatable))
			{
				CardData cardData = null;
				CardData cardData2 = null;
				if ((base.HasCardOnTop<CardData>(out cardData) || base.IsOnCard<CardData>(out cardData2)) && !GameCanvas.instance.ModalIsOpen)
				{
					CardData bs = ((cardData != null) ? cardData : cardData2);
					if (this.CanHaveCard(bs))
					{
						GameCanvas.instance.ShowNameCombatableModal(bs, delegate
						{
							bs.MyGameCard.RemoveFromStack();
							bs.MyGameCard.SendIt();
						});
					}
					else
					{
						bs.MyGameCard.RemoveFromStack();
					}
				}
			}
		}
		if (this.DollarAmount > 0)
		{
			this.descriptionOverride = MewtationsLoc.Translate("card_city_hall_description_long", new LocParam[] { LocParam.Create("amount", this.DollarAmount.ToString()) });
		}
		base.UpdateCard();
	}

	[HideInInspector]
	[ExtraData("dollar_amount")]
	public int DollarAmount = 100;

	public static int DollarPerCardcap = 5;
}
