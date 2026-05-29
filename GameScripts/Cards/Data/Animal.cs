using System;
using UnityEngine;

public class Animal : Mob
{
	public override bool CanBeDragged
	{
		get
		{
			return true;
		}
	}

	public override bool CanMove
	{
		get
		{
			return !this.InAnimalPen && !this.MyGameCard.HasParent && !this.MyGameCard.HasChild && this.MyGameCard.GetCardWithStatusInStack() == null;
		}
	}

	public AnimalPen RootPen
	{
		get
		{
			if (!this.MyGameCard.HasParent)
			{
				return null;
			}
			return this.rootStructure as AnimalPen;
		}
	}

	public BreedingPen RootBreedingPen
	{
		get
		{
			if (!this.MyGameCard.HasParent)
			{
				return null;
			}
			return this.rootStructure as BreedingPen;
		}
	}

	public ResourceMagnet RootMagnet
	{
		get
		{
			if (!this.MyGameCard.HasParent)
			{
				return null;
			}
			return this.rootStructure as ResourceMagnet;
		}
	}

	public SlaughterHouse RootSlaughterHouse
	{
		get
		{
			if (!this.MyGameCard.HasParent)
			{
				return null;
			}
			return this.rootStructure as SlaughterHouse;
		}
	}

	public PettingZoo RootPettingZoo
	{
		get
		{
			if (!this.MyGameCard.HasParent)
			{
				return null;
			}
			return this.rootStructure as PettingZoo;
		}
	}

	private CardData rootStructure
	{
		get
		{
			GameCard gameCard = this.MyGameCard.GetRootCard();
			if (gameCard.CardData is HeavyFoundation && gameCard.HasChild)
			{
				gameCard = gameCard.Child;
			}
			return gameCard.CardData;
		}
	}

	public bool InAnimalPen
	{
		get
		{
			return this.RootPen != null || this.RootBreedingPen != null || this.RootMagnet != null || this.RootSlaughterHouse != null || this.RootPettingZoo != null;
		}
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is NamingStone || (!(otherCard is Animal) && (otherCard.Id == "wheat" || base.CanHaveCard(otherCard)));
	}

	public virtual bool CanCreate
	{
		get
		{
			return !string.IsNullOrEmpty(this.CreateCard) && !base.InConflict;
		}
	}

	public float TimeUntilCreate
	{
		get
		{
			if (!this.CanCreate)
			{
				return -1f;
			}
			return this.CreateTime - this.CreateTimer;
		}
	}

	public override void UpdateCardText()
	{
		if (!string.IsNullOrEmpty(this.CustomName))
		{
			if (this.IsOld)
			{
				this.nameOverride = MewtationsLoc.Translate("card_animal_old_name", new LocParam[] { LocParam.Create("name", this.CustomName) });
			}
			else
			{
				this.nameOverride = this.CustomName;
			}
		}
		else if (this.IsOld)
		{
			this.nameOverride = MewtationsLoc.Translate(this.NameTerm + "_old");
		}
		else
		{
			this.nameOverride = MewtationsLoc.Translate(this.NameTerm);
		}
		base.UpdateCardText();
	}

	[TimedAction("eat_wheat")]
	private void EatWheat()
	{
		CardData cardData;
		if (base.HasCardOnTop("wheat", out cardData))
		{
			this.ConsumeWheat(cardData);
		}
	}

	public void ConsumeWheat(CardData wheat)
	{
		this.MyGameCard.GetRootCard().CardData.RestackChildrenMatchingPredicate((CardData x) => x == wheat);
		wheat.MyGameCard.DestroyCard(false, true);
		this.CreateTimer = 0f;
		this.TryCreateItem();
		this.MyGameCard.RotWobble(1f);
	}

	public override void UpdateCard()
	{
		base.UpdateCard();
		if (this.CanCreate)
		{
			this.CreateTimer += Time.deltaTime * WorldManager.instance.TimeScale;
		}
		if (this.CreateTimer >= this.CreateTime && (this.moveFlag || this.InAnimalPen || this.MyGameCard.GetRootCard().CardData is HeavyFoundation))
		{
			this.CreateTimer -= this.CreateTime;
			this.TryCreateItem();
		}
		CardData cardData;
		if (base.HasCardOnTop("wheat", out cardData) && !this.InAnimalPen)
		{
			this.MyGameCard.StartTimer(5f, new TimerAction(this.EatWheat), MewtationsLoc.Translate("card_animal_eating_status"), "eat_wheat", true, false, false);
		}
		else
		{
			this.MyGameCard.CancelTimer("eat_wheat");
		}
		Animal animal;
		if (!this.MyGameCard.BeingDragged && !this.InAnimalPen && !base.InConflict && base.HasCardOnTop<Animal>(out animal))
		{
			animal.MyGameCard.RemoveFromStack();
		}
	}

	private void TryCreateItem()
	{
		if (this.CanCreate)
		{
			CardData cardData = WorldManager.instance.CreateCard(this.MyGameCard.transform.position, this.CreateCard, true, false, true);
			if (this.RootBreedingPen != null || this.RootSlaughterHouse != null || this.RootPen != null)
			{
				WorldManager.instance.StackSendCheckTarget(this.rootStructure.MyGameCard, cardData.MyGameCard, this.OutputDir, null, true, -1);
			}
			else
			{
				WorldManager.instance.StackSend(cardData.MyGameCard, this.OutputDir, null, true);
			}
			this.ItemsCreated++;
			if (WorldManager.instance.CurseIsActive(CurseType.Death) && this.ItemsCreated % 4 == 0 && this.CreateCard != "poop")
			{
				CardData cardData2 = WorldManager.instance.CreateCard(this.MyGameCard.transform.position, "poop", true, false, true);
				if (this.RootBreedingPen != null || this.RootSlaughterHouse != null || this.RootPen != null)
				{
					WorldManager.instance.StackSendCheckTarget(this.rootStructure.MyGameCard, cardData2.MyGameCard, this.OutputDir, null, true, -1);
					return;
				}
				WorldManager.instance.StackSend(cardData2.MyGameCard, this.OutputDir, null, true);
			}
		}
	}

	public override void Clicked()
	{
		if (this.InAnimalPen)
		{
			return;
		}
		if (this.MyGameCard.Velocity == null)
		{
			this.MoveTimer = this.MoveTime;
		}
		base.Clicked();
	}

	protected override void Move()
	{
		AudioManager.me.PlaySound2D(AudioManager.me.AnimalMove, Random.Range(0.8f, 1.2f), 0.2f);
		base.Move();
	}

	public override void Die()
	{
		if (!this.IsAggressive)
		{
			WorldManager.instance.TryCreateUnhappiness(base.transform.position, 2);
		}
		base.Die();
	}

	private bool CanGrowOld()
	{
		return WorldManager.instance.CurseIsActive(CurseType.Death);
	}

	[Header("Animal")]
	public float CreateTime = 10f;

	[Card]
	public string CreateCard;

	public bool IsBreedable;

	[ExtraData("age")]
	public int Age;

	public bool IsOld;

	[ExtraData("createtimer")]
	[HideInInspector]
	public float CreateTimer;

	[ExtraData("itemscreated")]
	[HideInInspector]
	public int ItemsCreated;
}
