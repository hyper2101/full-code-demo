using System;

public class Junction : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.MyCardType != CardType.Structures;
	}

	public bool AnyTransportConnected()
	{
		for (int i = 0; i < this.MyGameCard.CardConnectorChildren.Count; i++)
		{
			CardConnector cardConnector = this.MyGameCard.CardConnectorChildren[i];
			if (cardConnector.ConnectionType == ConnectionType.Transport && cardConnector.ConnectedNode != null)
			{
				return true;
			}
		}
		return false;
	}

	public override void UpdateCard()
	{
		if (this.MyGameCard.HasChild)
		{
			for (int i = this.MyGameCard.GetChildCards().Count - 1; i >= 0; i--)
			{
				GameCard gameCard = this.MyGameCard.GetChildCards()[i];
				gameCard.RemoveFromStack();
				if (this.AnyTransportConnected())
				{
					WorldManager.instance.StackSendCheckTarget(this.MyGameCard, gameCard, this.OutputDir, null, true, -1);
				}
				else
				{
					gameCard.SendIt();
				}
			}
		}
		base.UpdateCard();
	}
}
