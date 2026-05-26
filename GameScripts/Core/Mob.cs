using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Mob : Combatable
{
	public virtual bool CanMove
	{
		get
		{
			return this.MyGameCard.GetCardWithStatusInStack() == null;
		}
	}

	public override bool CanBeDragged
	{
		get
		{
			return false;
		}
	}

	public override bool CanBePushedBy(CardData otherCard)
	{
		return otherCard is Mob;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (otherCard.Id == "bone" && this.Id == "wolf")
		{
			return true;
		}
		if (otherCard.Id == "milk" && this.Id == "feral_cat")
		{
			return true;
		}
		return base.AllCardsInStackMatchPred(otherCard, delegate(CardData x)
		{
			Combatable combatable = x as Combatable;
			return combatable != null && combatable.CanAttack && !(combatable is Animal);
		});
	}

	public override void UpdateCard()
	{
		this.moveFlag = false;
		bool flag = this.CanMove && !base.InConflict && this.MoveTime != -1f;
		if (this.MyGameCard.BounceTarget != null)
		{
			flag = false;
		}
		if (flag)
		{
			float num = ((this is Enemy) ? 2f : 1f);
			if (WorldManager.instance.CurrentBoard != null && WorldManager.instance.CurrentBoard.Id == "forest")
			{
				num = 3f;
			}
			this.MoveTimer += Time.deltaTime * WorldManager.instance.TimeScale * num;
			if (this.MoveTimer >= this.MoveTime)
			{
				this.moveFlag = true;
				this.MoveTimer = 0f;
				this.Move();
			}
		}
		else
		{
			this.MoveTimer = 0f;
		}
		if (this.IsAggressive && !base.InConflict && WorldManager.instance.TimeScale > 0f)
		{
			if (this.CurrentTarget == null || this.CurrentTarget.MyGameCard.MyBoard != this.MyGameCard.MyBoard)
			{
				this.CurrentTarget = this.FindTarget();
			}
			foreach (Combatable combatable in this.GetOverlappingCombatables())
			{
				if (combatable.InConflict)
				{
					combatable.MyConflict.JoinConflict(this);
					break;
				}
				if (combatable.Team != base.Team)
				{
					List<GameCard> list = (from x in combatable.MyGameCard.GetAllCardsInStack()
						where x.Combatable != null
						select x).ToList<GameCard>();
					list.Add(this.MyGameCard);
					list = list.Distinct<GameCard>().ToList<GameCard>();
					WorldManager.instance.Restack(list);
					break;
				}
			}
		}
		base.UpdateCard();
	}

	public override void Die()
	{
		this.TryDropItems();
		base.Die();
	}

	public void TryDropItems()
	{
		List<GameCard> list = new List<GameCard>();
		bool flag = false;
		if (WorldManager.instance.CurrentRunVariables.CanDropItem)
		{
			flag = this.TryDropEquipment();
			if (flag)
			{
				Debug.Log("Dropped special equipment!");
				WorldManager.instance.CurrentRunVariables.CanDropItem = false;
			}
		}
		if (!flag)
		{
			Debug.Log("Dropped normal item!");
			if (this.Drops.CardBagType == CardBagType.Chances && this.Drops.Chances.Count == 0)
			{
				base.Die();
				return;
			}
			for (int i = 0; i < this.Drops.CardsInPack; i++)
			{
				ICardId cardFiltered = this.Drops.GetCardFiltered(new Predicate<string>(this.Filter), false);
				if (cardFiltered != null && !string.IsNullOrWhiteSpace(cardFiltered.Id))
				{
					CardData cardFromId = WorldManager.instance.GameDataLoader.GetCardFromId(cardFiltered.Id, true);
					if (cardFromId is Equipable)
					{
						WorldManager.instance.CurrentRunVariables.CanDropItem = false;
					}
					CardData cardData = WorldManager.instance.CreateCard(base.transform.position, cardFromId, true, false, true, true);
					cardData.MyGameCard.SendIt();
					list.Add(cardData.MyGameCard);
				}
			}
			if (list.Count > 0 && WorldManager.instance.StackAllSame(list[0]))
			{
				foreach (GameCard gameCard in list)
				{
					gameCard.Velocity = null;
				}
				WorldManager.instance.Restack(list);
				WorldManager.instance.StackSend(list[0], this.OutputDir, null, true);
			}
		}
	}

	private bool Filter(string cardId)
	{
		return this.AlwaysDrop || (!(WorldManager.instance.GameDataLoader.GetCardFromId(cardId, true) is Equipable) || WorldManager.instance.CurrentRunVariables.CanDropItem);
	}

	public bool TryDropEquipment()
	{
		if (!this.HasInventory)
		{
			return false;
		}
		List<Equipable> list = (from x in base.GetAllEquipables()
			orderby Random.value
			select x).ToList<Equipable>();
		if (list.Count == 0)
		{
			return false;
		}
		List<Equipable> list2 = new List<Equipable>();
		foreach (Equipable equipable in list)
		{
			list2.Add(equipable);
		}
		list2.RemoveAll((Equipable x) => x.blueprint != null && WorldManager.instance.HasFoundCard(x.Id));
		if (list2.Count == 0)
		{
			return false;
		}
		CardData cardData = WorldManager.instance.CreateCard(base.transform.position, list2[0], true, false, true, true);
		Equipable equipable2 = cardData as Equipable;
		if (equipable2 != null && equipable2.blueprint != null)
		{
			WorldManager.instance.CreateCard(base.transform.position, equipable2.blueprint, true, false, true, true).MyGameCard.SetChild(cardData.MyGameCard);
		}
		cardData.MyGameCard.SendIt();
		return true;
	}

	public List<Combatable> GetOverlappingCombatables()
	{
		this.overlappingCombatables.Clear();
		foreach (GameCard gameCard in this.MyGameCard.GetOverlappingCards())
		{
			foreach (GameCard gameCard2 in gameCard.GetAllCardsInStack())
			{
				if (gameCard2.Combatable != null && !gameCard2.BeingDragged)
				{
					this.overlappingCombatables.Add(gameCard2.Combatable);
				}
			}
		}
		return this.overlappingCombatables;
	}

	protected virtual void Move()
	{
		this.MyGameCard.SendIt();
	}

	protected virtual Combatable FindTarget()
	{
		if (WorldManager.instance.CurrentBoard.Id == "cities")
		{
			return WorldManager.instance.CardQuery.GetCard<CitiesCombatable>();
		}
		return WorldManager.instance.CardQuery.GetCard<BaseVillager>();
	}

	[HideInInspector]
	public float MoveTimer;

	[Header("Mob")]
	public float MoveTime = 3f;

	public bool IsAggressive;

	public bool AlwaysDrop;

	[HideInInspector]
	public Combatable CurrentTarget;

	public CardBag Drops;

	protected bool moveFlag;

	private List<Combatable> overlappingCombatables = new List<Combatable>();

	private delegate bool DropFilter(string cardId);
}
