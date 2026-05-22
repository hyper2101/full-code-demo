using System;

public class Egg : Food
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return !(otherCard.MyGameCard == null) && ((otherCard.Id == "chicken" && (!this.MyGameCard.HasParent || this.MyGameCard.Parent.CardData is HeavyFoundation) && !otherCard.MyGameCard.HasChild) || ((!(otherCard.Id == "egg") || !otherCard.MyGameCard.HasChild || !(otherCard.MyGameCard.Child.CardData.Id == "chicken")) && base.CanHaveCard(otherCard)));
	}
}
