using System;
using System.Collections.Generic;

public class House : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is BaseVillager || otherCard is Kid || otherCard.Id == this.Id || base.CanHaveCard(otherCard);
	}

	public override void UpdateCard()
	{
		if (base.HasCardOnTop<Kid>())
		{
			this.MyGameCard.StartTimer(120f, new TimerAction(this.GrowUpKid), MewtationsLoc.Translate("new_growing_up"), base.GetActionId("GrowUpKid"), true, false, false);
		}
		else
		{
			this.MyGameCard.CancelTimer(base.GetActionId("GrowUpKid"));
		}
		base.UpdateCard();
	}

	[TimedAction("growup_kid")]
	public void GrowUpKid()
	{
		Kid kid;
		base.HasCardOnTop<Kid>(out kid);
		List<ExtraCardData> extraCardData = kid.GetExtraCardData();
		kid.MyGameCard.DestroyCard(true, true);
		CardData cardData;
		if (WorldManager.instance.IsSpiritDlcActive())
		{
			cardData = WorldManager.instance.CreateCard(this.MyGameCard.transform.position, "teenage_villager", true, false, true);
			(cardData as BaseVillager).UpdateLifeStage();
		}
		else
		{
			cardData = WorldManager.instance.CreateCard(this.MyGameCard.transform.position, "villager", true, false, true);
		}
		cardData.SetExtraCardData(extraCardData);
		cardData.MyGameCard.SendIt();
	}
}
