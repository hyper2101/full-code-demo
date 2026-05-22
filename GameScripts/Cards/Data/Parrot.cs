using System;

public class Parrot : Animal
{
	public override void StoppedDragging()
	{
		if (this.MyGameCard.HasParent && this.MyGameCard.Parent.CardData.Id == "pirate")
		{
			CardData cardData = WorldManager.instance.ChangeToCard(this.MyGameCard.Parent, "friendly_pirate");
			this.MyGameCard.DestroyCard(false, true);
			WorldManager.instance.CreateSmoke(cardData.transform.position);
			cardData.MyGameCard.SendIt();
			QuestManager.instance.SpecialActionComplete("befriend_pirate", null);
			return;
		}
		base.StoppedDragging();
	}
}
