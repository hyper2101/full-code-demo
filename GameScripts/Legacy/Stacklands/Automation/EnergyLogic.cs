using Mewtations.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Mewtations.Core.LegacySystem(Mewtations.Core.LegacyCategory.DeprecatedAutomation)]
    public class EnergyLogic : CardData
{
	public override bool HasEnergyOutput(CardConnector connectedNode, List<CardConnector> nodeTracker)
	{
		if (nodeTracker.Contains(connectedNode))
		{
			return false;
		}
		nodeTracker.Add(connectedNode);
		if (this.MyGameCard.CardConnectorChildren.Where<CardConnector>((CardConnector x) => x.CardDirection == CardDirection.input).Count<CardConnector>() <= 0)
		{
			this.HasEnergy = false;
		}
		if (this.MyGameCard.CardConnectorChildren.Where<CardConnector>((CardConnector x) => x.CardDirection == CardDirection.input).All<CardConnector>((CardConnector x) => x.ConnectedNode != null && x.ConnectedNode.Parent.CardData.HasEnergyOutput(x.ConnectedNode, nodeTracker)))
		{
			this.HasEnergy = true;
		}
		else
		{
			this.HasEnergy = false;
		}
		if (this.HasEnergy != this.prevHasEnergy)
		{
			base.NotifyEnergyConsumers();
		}
		this.prevHasEnergy = this.HasEnergy;
		return this.HasEnergy;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return false;
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	[ExtraData("has_energy")]
	[HideInInspector]
	public bool HasEnergy;

	private bool prevHasEnergy;
}


