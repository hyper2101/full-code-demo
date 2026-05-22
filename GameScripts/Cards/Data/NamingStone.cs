using System;

public class NamingStone : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is BaseVillager || otherCard is Animal || otherCard is Kid;
	}

	public override void UpdateCard()
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
					if (bs is BaseVillager)
					{
						QuestManager.instance.SpecialActionComplete("name_villager", null);
					}
					bs.MyGameCard.RemoveFromStack();
					bs.MyGameCard.SendIt();
				});
			}
			else
			{
				bs.MyGameCard.RemoveFromStack();
			}
		}
		base.UpdateCard();
	}
}
