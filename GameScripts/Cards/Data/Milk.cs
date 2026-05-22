using System;

public class Milk : Food
{
	public override void StoppedDragging()
	{
		if (this.MyGameCard.HasParent && this.MyGameCard.Parent.CardData.Id == "feral_cat" && WorldManager.instance.IsSpiritDlcActive())
		{
			this.MyGameCard.Parent.DestroyCard(false, true);
			this.MyGameCard.DestroyCard(false, true);
			CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "cat", true, true, true);
			WorldManager.instance.CreateSmoke(cardData.transform.position);
			cardData.MyGameCard.SendIt();
			return;
		}
		base.StoppedDragging();
	}
}
