using System;

public class WaterTreatmentPlant : EnergyHarvestable
{
	protected override bool CanStartHarvesting()
	{
		for (int i = 0; i < this.MyGameCard.CardConnectorChildren.Count; i++)
		{
			CardConnector cardConnector = this.MyGameCard.CardConnectorChildren[i];
			if (cardConnector != null && cardConnector.ConnectionType == ConnectionType.Sewer && cardConnector.ConnectedNode == null)
			{
				return false;
			}
		}
		return base.CanStartHarvesting();
	}
}
