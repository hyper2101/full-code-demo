using System;
using UnityEngine;

public class Royal : CardData
{
	public override void UpdateCard()
	{
		DemandEvent activeDemand = WorldManager.instance.CurrentRunVariables.ActiveDemand;
		if (this.MyGameCard.IsDemoCard)
		{
			return;
		}
		if (!WorldManager.instance.InAnimation)
		{
			this.UpdateInteractions();
		}
		if (activeDemand != null)
		{
			base.AddStatusEffect(new StatusEffect_Demand());
		}
		else if (base.HasStatusEffectOfType<StatusEffect_Demand>())
		{
			base.RemoveStatusEffect<StatusEffect_Demand>();
		}
		base.UpdateCard();
	}

	private void UpdateInteractions()
	{
		DemandEvent activeDemand = WorldManager.instance.CurrentRunVariables.ActiveDemand;
		if (this.MyGameCard.Child != null)
		{
			GameCard child = this.MyGameCard.Child;
			Demand demand = ((activeDemand != null) ? activeDemand.Demand : null);
			if (child.CardData is BaseVillager)
			{
				this.AttackTries++;
				if (this.AttackTries >= 9)
				{
					WorldManager.instance.ChangeToCard(this.MyGameCard, "angry_royal");
					WorldManager.instance.CurrentRunVariables.ActiveDemand = null;
					DemandManager.instance.CanReceiveDemand = false;
				}
				else
				{
					WorldManager.instance.Cutscene.QueueCutscene(GreedCutscenes.TryAttackRoyal(this, this.AttackTries));
				}
				child.RemoveFromParent();
				child.SendIt();
				return;
			}
			if (demand != null && child.CardData.Id == demand.CardToGet)
			{
				foreach (GameCard gameCard in this.MyGameCard.GetChildCards())
				{
					if (gameCard.CardData.Id == demand.CardToGet)
					{
						if (demand.IsFinalDemand)
						{
							child.RemoveFromParent();
							child.SendIt();
							WorldManager.instance.Cutscene.QueueCutscene(GreedCutscenes.FinalDemandEndSuccess(true));
						}
						else if (WorldManager.instance.CurrentRunVariables.ActiveDemand.AmountGiven < demand.Amount)
						{
							gameCard.RemoveFromStack();
							if (demand.ShouldDestroyOnComplete)
							{
								gameCard.DestroyCard(false, true);
								WorldManager.instance.CreateSmoke(base.transform.position);
							}
							else
							{
								gameCard.SendIt();
							}
							WorldManager.instance.CurrentRunVariables.ActiveDemand.AmountGiven++;
							if (demand.Amount == WorldManager.instance.CurrentRunVariables.ActiveDemand.AmountGiven)
							{
								WorldManager.instance.Cutscene.QueueCutscene(DemandManager.instance.FinishDemand(WorldManager.instance.CurrentRunVariables.ActiveDemand));
							}
						}
						else
						{
							child.RemoveFromParent();
							child.SendIt();
						}
					}
				}
			}
		}
	}

	public override void UpdateCardText()
	{
		DemandEvent activeDemand = WorldManager.instance.CurrentRunVariables.ActiveDemand;
		if (activeDemand != null && DemandManager.instance != null)
		{
			Demand demandById = DemandManager.instance.GetDemandById(activeDemand.DemandId);
			if (demandById != null)
			{
				if (demandById.IsFinalDemand)
				{
					this.descriptionOverride = MewtationsLoc.Translate("card_royal_description_demand_2");
					return;
				}
				this.descriptionOverride = DemandManager.instance.GetDemandStartDescription(demandById, activeDemand);
				if (demandById.Amount > 1)
				{
					this.descriptionOverride = this.descriptionOverride + "\n\n" + MewtationsLoc.Translate("label_greed_given", new LocParam[] { LocParam.Create("given", string.Format("{0}/{1}", activeDemand.AmountGiven, demandById.Amount)) });
					return;
				}
			}
		}
		else
		{
			this.descriptionOverride = "";
		}
	}

	public void Die()
	{
		WorldManager.instance.CreateCard(base.transform.position, "royal_crown", true, true, true);
		WorldManager.instance.CreateSmoke(base.transform.position);
		base.RemoveAllStatusEffects();
		WorldManager.instance.ChangeToCard(this.MyGameCard, "corpse");
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		Demand currentDemand = DemandManager.instance.GetCurrentDemand();
		return otherCard is BaseVillager || (currentDemand != null && currentDemand.CardToGet == otherCard.Id);
	}

	[HideInInspector]
	public float MoveTimer;

	public float MoveTime = 10f;

	[ExtraData("attack_tries")]
	public int AttackTries;
}
