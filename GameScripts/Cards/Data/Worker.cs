using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Worker : CardData, HousingConsumer
{
	[HideInInspector]
	public Apartment Housing
	{
		get
		{
			if (this.HousingUniqueId != null)
			{
				GameCard cardWithUniqueId = WorldManager.instance.GetCardWithUniqueId(this.HousingUniqueId);
				if (cardWithUniqueId != null)
				{
					return cardWithUniqueId.CardData as Apartment;
				}
			}
			return null;
		}
		set
		{
			this.HousingUniqueId = ((value != null) ? value.UniqueId : "");
		}
	}

	public string HousingId
	{
		get
		{
			return this.HousingUniqueId;
		}
	}

	public override void OnInitialCreate()
	{
		this.Housing = null;
		QuestManager.instance.SpecialActionComplete("worker_created", this);
		base.OnInitialCreate();
	}

	public override void UpdateCard()
	{
		GameBoard currentBoard = WorldManager.instance.CurrentBoard;
		if (currentBoard == null || currentBoard.Location != Location.Cities)
		{
			base.RemoveStatusEffect<StatusEffect_Homeless>();
			base.UpdateCard();
			return;
		}
		Apartment housing = this.Housing;
		bool flag = housing != null && !housing.IsDamaged && housing.HasEnergyInput(null);
		if (this.GetHousingSpaceRequired() == 0)
		{
			flag = true;
		}
		if (!flag && !base.HasStatusEffectOfType<StatusEffect_Homeless>())
		{
			base.AddStatusEffect(new StatusEffect_Homeless());
		}
		if (flag && base.HasStatusEffectOfType<StatusEffect_Homeless>())
		{
			base.RemoveStatusEffect<StatusEffect_Homeless>();
		}
		if (CitiesManager.instance.WorkersOnBoard.Count <= 1)
		{
			this.CitiesValue = -1;
		}
		else if (this.WorkerType == WorkerType.Educated)
		{
			this.CitiesValue = 30;
		}
		else if (this.WorkerType == WorkerType.Normal)
		{
			this.CitiesValue = 20;
		}
		else if (this.WorkerType == WorkerType.Robot)
		{
			this.CitiesValue = 40;
		}
		base.UpdateCard();
	}

	public override void UpdateCardText()
	{
		if (!string.IsNullOrEmpty(this.CustomName))
		{
			this.nameOverride = this.CustomName;
		}
	}

	public float GetActionTimeModifier()
	{
		if (this.Id == "educated_worker")
		{
			return 0.75f;
		}
		if (this.Id == "genius" || this.Id == "robot_genius")
		{
			return 0.5f;
		}
		return 1f;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is Worker;
	}

	public override void StoppedDragging()
	{
		List<CardData> list = base.CardsInStackMatchingPredicate((CardData x) => x is Worker);
		List<GameCard> list2 = (from x in this.MyGameCard.GetAllCardsInStack()
			where x.CardData.WorkerAmount > 0
			select x).ToList<GameCard>();
		if (list2.Count > 0)
		{
			using (List<GameCard>.Enumerator enumerator = list2.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					GameCard gameCard = enumerator.Current;
					foreach (CardData cardData in list)
					{
						Worker worker = (Worker)cardData;
						if (worker != null)
						{
							worker.TryEquipOnCard(gameCard);
						}
					}
				}
				goto IL_0105;
			}
		}
		foreach (CardData cardData2 in list)
		{
			((Worker)cardData2).TryUnequipSelf();
		}
		IL_0105:
		base.StoppedDragging();
	}

	private void TryUnequipSelf()
	{
		GameCard workerHolder = this.MyGameCard.WorkerHolder;
		if (workerHolder == null)
		{
			return;
		}
		workerHolder.UnequipWorker(this.MyGameCard);
	}

	private void TryEquipOnCard(GameCard card)
	{
		if (card != null)
		{
			CardData cardData = card.CardData;
			int? num = ((cardData != null) ? new int?(cardData.WorkerAmount) : null);
			int num2 = 0;
			if ((num.GetValueOrDefault() <= num2) & (num != null))
			{
				return;
			}
		}
		if (this.MyGameCard.WorkerHolder != null)
		{
			this.MyGameCard.WorkerHolder.UnequipWorker(this.MyGameCard);
		}
		if (card != null && card.CardData.WorkerAmount > 0)
		{
			card.OpenInventory(true);
			card.CardData.EquipWorker(this);
		}
	}

	public GameCard GetGameCard()
	{
		return this.MyGameCard;
	}

	public int GetHousingSpaceRequired()
	{
		return this.HousingSpaceRequired;
	}

	public WorkerType GetWorkerType()
	{
		return this.WorkerType;
	}

	public override void OnSellCard()
	{
		if (this.Housing != null)
		{
			this.Housing.UsedSpace -= this.GetHousingSpaceRequired();
			this.Housing = null;
		}
		QuestManager.instance.SpecialActionComplete("worker_removed", this);
		base.OnSellCard();
	}

	public override void OnDestroyCard()
	{
		if (this.Housing != null)
		{
			this.Housing.UsedSpace -= this.GetHousingSpaceRequired();
			this.Housing = null;
		}
		QuestManager.instance.SpecialActionComplete("worker_removed", this);
		base.OnDestroyCard();
	}

	public int HousingSpaceRequired = 1;

	public WorkerType WorkerType;

	[HideInInspector]
	[ExtraData("housingUniqueId")]
	public string HousingUniqueId;
}
