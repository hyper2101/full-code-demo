using System;

public class Bone : Resource
{
	public override void StoppedDragging()
	{
		if (this.MyGameCard.HasParent && this.MyGameCard.Parent.CardData.Id == "wolf")
		{
			this.MyGameCard.Parent.DestroyCard(false, true);
			this.MyGameCard.DestroyCard(false, true);
			CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "dog", true, true, true);
			WorldManager.instance.CreateSmoke(cardData.transform.position);
			cardData.MyGameCard.SendIt();
			return;
		}
		base.StoppedDragging();
	}
}
