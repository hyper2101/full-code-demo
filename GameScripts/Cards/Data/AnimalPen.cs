using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimalPen : CardData
{
	public override bool DetermineCanHaveCardsWhenIsRoot
	{
		get
		{
			return true;
		}
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		if (this.Id == "animal_cage" && otherCard.Id == "animal_cage")
		{
			return true;
		}
		if (otherCard.Id == "wheat")
		{
			return true;
		}
		if (otherCard.Id == "egg")
		{
			return true;
		}
		if (otherCard.Id == "magic_dust" || otherCard.Id == "soil")
		{
			return true;
		}
		int num = base.GetChildCount() + (1 + otherCard.GetChildCount());
		if (!this.IsForFish)
		{
			return otherCard is Animal && otherCard.MyCardType != CardType.Fish && num <= this.MaxAnimalCount;
		}
		return otherCard is Animal && otherCard.MyCardType == CardType.Fish && num <= this.MaxAnimalCount;
	}

	private Animal GetAnimalInStack()
	{
		base.GetChildrenMatchingPredicate((CardData x) => x is Animal, this.animals);
		if (this.animals.Count == 0)
		{
			return null;
		}
		return this.animals.Choose<CardData>() as Animal;
	}

	public override void UpdateCard()
	{
		CardData cardData;
		if (base.AnyChildMatchesPredicate((CardData x) => x.Id == "wheat", out cardData))
		{
			if (this.GetAnimalInStack() != null)
			{
				this.MyGameCard.StartTimer(5f, new TimerAction(this.EatWheat), MewtationsLoc.Translate("card_animal_eating_status"), "eat_wheat", true, false, false);
			}
		}
		else
		{
			this.MyGameCard.CancelTimer("eat_wheat");
			this.ShowFakeAnimalProgressBar();
		}
		base.UpdateCard();
	}

	private void ShowFakeAnimalProgressBar()
	{
		Animal firstAnimalToProduce = this.GetFirstAnimalToProduce();
		if (firstAnimalToProduce != null)
		{
			CardData cardPrefab = WorldManager.instance.GetCardPrefab(firstAnimalToProduce.CreateCard, true);
			string text = MewtationsLoc.Translate("card_animal_pen_status", new LocParam[]
			{
				LocParam.Create("card", cardPrefab.Name),
				LocParam.Create("name", firstAnimalToProduce.Name)
			});
			this.MyGameCard.StartTimer(firstAnimalToProduce.CreateTime, new TimerAction(this.AnimalCreate), text, "animal_create", true, false, false);
			this.MyGameCard.CurrentTimerTime = firstAnimalToProduce.CreateTimer;
			return;
		}
		this.MyGameCard.CancelTimer("animal_create");
	}

	[TimedAction("animal_create")]
	public void AnimalCreate()
	{
	}

	private Animal GetFirstAnimalToProduce()
	{
		base.GetChildrenMatchingPredicate((CardData x) => x is Animal, this.animals);
		Animal animal = null;
		float num = float.MaxValue;
		foreach (CardData cardData in this.animals)
		{
			Animal animal2 = (Animal)cardData;
			if (animal2.CanCreate && animal2.TimeUntilCreate < num)
			{
				num = animal2.TimeUntilCreate;
				animal = animal2;
			}
		}
		return animal;
	}

	[TimedAction("eat_wheat")]
	public void EatWheat()
	{
		Animal animalInStack = this.GetAnimalInStack();
		CardData cardData;
		if (base.AnyChildMatchesPredicate((CardData x) => x.Id == "wheat", out cardData) && animalInStack != null)
		{
			animalInStack.ConsumeWheat(cardData);
		}
	}

	[Header("Animals")]
	public int MaxAnimalCount = 5;

	public bool IsForFish;

	private List<CardData> animals = new List<CardData>();
}
