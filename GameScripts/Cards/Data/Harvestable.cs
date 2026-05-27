using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mewtations.Systems.Labor;

public class Harvestable : CardData
{
	public string StatusText
	{
		get
		{
			return SokLoc.Translate(this.StatusTerm);
		}
	}

	public override bool DetermineCanHaveCardsWhenIsRoot
	{
		get
		{
			return true;
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return LaborUtility.IsLaborCapable(otherCard) || otherCard.Id == this.Id || (this.MyCardType == CardType.Weather && otherCard.MyCardType == CardType.Weather) || this.CanHaveCardIds.Contains(otherCard.Id);
	}

	public override void SetFoil()
	{
		base.SetFoil();
	}

	protected override void Awake()
	{
		base.Awake();
		SokLoc.instance.LanguageChanged += this.UpdateDescription;
		this.UpdateDescription();
	}

	private void OnDestroy()
	{
		SokLoc.instance.LanguageChanged -= this.UpdateDescription;
	}

	private void UpdateDescription()
	{
		if (this.MyCardType == CardType.Locations)
		{
			this.descriptionOverride = SokLoc.Translate(this.DescriptionTerm, new LocParam[] { LocParam.Create("required_count", this.RequiredVillagerCount.ToString()) }) + "\n\n" + BoosterpackData.GetSummaryFromAllCards(this.MyCardBag.GetCardsInBag(), "label_may_contain");
		}
	}

	public override void UpdateCard()
	{
		if (!(this is EnergyHarvestable))
		{
			base.GetChildrenMatchingPredicate((CardData x) => LaborUtility.IsLaborCapable(x), this.villagers);
			bool flag = true;
			GameCard cardWithStatusInStack = this.MyGameCard.GetCardWithStatusInStack();
			if (cardWithStatusInStack != null && cardWithStatusInStack.TimerRunning && cardWithStatusInStack.TimerActionId == "finish_blueprint")
			{
				flag = false;
			}
			if (this.villagers.Count >= this.RequiredVillagerCount && (HasLaborCapableOnTop()) && flag)
			{
								string actionId = base.GetActionId("CompleteHarvest");
				float num = 1f;
				List<CardData> list = this.villagers.FindAll((CardData x) => LaborUtility.IsLaborCapable(x));
				if (list.Count > 0)
				{
                    // Fallback to max efficiency among labor capable actors.
					num = list.Min(v => {
                        if (v is ILaborCapable laborCap) return 1f / laborCap.GetLaborEfficiency();
                        return 1f;
                    });
				}
				this.MyGameCard.StartTimer(num * this.HarvestTime, new TimerAction(this.CompleteHarvest), this.StatusText, actionId, true, false, false);
			}
			else
			{
				this.MyGameCard.CancelTimer(base.GetActionId("CompleteHarvest"));
			}
		}
		base.UpdateCard();
	}

	public virtual void SendCard(GameCard card)
	{
		WorldManager.instance.StackSend(card, this.OutputDir, null, true);
	}

	public virtual ICardId GetCardToGive()
	{
		ICardId cardId = this.MyCardBag.GetCard(true);
		if (this.Id == "catacombs" && this.Amount == 1)
		{
			cardId = (CardId)"goblet";
		}
		if (this.Id == "cave" && this.Amount == 1)
		{
			cardId = (CardId)"treasure_map";
		}
		if (this.Id == "ruins" && this.Amount == 1)
		{
			cardId = (CardId)"blueprint_fountain_of_youth";
		}
		if (this.Id == "old_tome")
		{
			List<CardData> list = WorldManager.instance.CardDataPrefabs.Where<CardData>((CardData x) => x.MyCardType == CardType.Ideas && !WorldManager.instance.HasFoundCard(x.Id) && !x.HideFromCardopedia).ToList<CardData>();
			list.RemoveAll((CardData x) => x.CardUpdateType == CardUpdateType.Spirit);
			if (!WorldManager.instance.CurrentRunVariables.VisitedIsland)
			{
				list.RemoveAll((CardData x) => x.CardUpdateType == CardUpdateType.Island);
			}
			if (list.Count > 0)
			{
				cardId = (CardId)list.Choose<CardData>().Id;
			}
			else
			{
				cardId = (CardId)"map";
			}
		}
		return cardId;
	}

	[TimedAction("complete_harvest")]
	public void CompleteHarvest()
	{
		if (!this.IsUnlimited)
		{
			this.Amount--;
		}
				GameCard gameCard = null;
		if (HasLaborCapableOnTop())
		{
			gameCard = this.MyGameCard.Child;
			gameCard.RotWobble(0.5f);
		}
        foreach (CardData worker in this.villagers)
        {
            if (worker != null && LaborUtility.IsLaborCapable(worker))
            {
                float staminaCost = 5f;
                if (this.Id == "berrybush" || this.Id == "apple_tree") staminaCost = 2f;
                else if (this.Id == "tree" || this.Id == "wood") staminaCost = 5f;
                else if (this.Id == "rock" || this.Id == "iron_rock") staminaCost = 8f;
                LaborUtility.ConsumeLaborStamina(worker, staminaCost);
            }
        }
		ICardId cardToGive = this.GetCardToGive();
		if (cardToGive != null && !string.IsNullOrEmpty(cardToGive.Id))
		{
			CardData cardData = WorldManager.instance.CreateCard(this.MyGameCard.transform.position, cardToGive, false, false, true);
			WorldManager.instance.StackSendCheckTarget(this.MyGameCard, cardData.MyGameCard, this.OutputDir, null, true, -1);
			Combatable combatable = cardData as Combatable;
			if (combatable != null)
			{
				combatable.HealthPoints = combatable.ProcessedCombatStats.MaxHealth;
			}
			Creditcard creditcard = cardData as Creditcard;
			if (creditcard != null)
			{
				creditcard.DollarCount = Random.Range(1, 3) * 10;
			}
		}
		if (!this.IsUnlimited && this.Amount <= 0)
		{
			if (gameCard != null && this.MyGameCard.HasParent && this.MyGameCard.Parent.CardData.Id == this.Id)
			{
				GameCard parent = this.MyGameCard.Parent;
				this.MyGameCard.RemoveFromStack();
				gameCard.SetParent(parent);
			}
			this.Emptied();
			this.MyGameCard.DestroyCard(true, true);
		}
		this.OnHarvestComplete();
		this.UpdateDescription();
	}

	public virtual void OnHarvestComplete()
	{
	}

	protected virtual void Emptied()
	{
	}

	[Header("Harvestable")]
	[Term]
	public string StatusTerm = "";

	[ExtraData("amount")]
	public int Amount = 3;

	public bool IsUnlimited;

	public float HarvestTime = 10f;

	public CardBag MyCardBag;

	[Header("Multiple villager options")]
	public int RequiredVillagerCount = 1;

	private List<CardData> villagers = new List<CardData>();

	[Card]
	public List<string> CanHaveCardIds = new List<string>();
}



