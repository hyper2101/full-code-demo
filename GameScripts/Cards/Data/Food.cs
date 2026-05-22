using System;
using UnityEngine;

public class Food : CardData
{
	public bool IsSpoiling
	{
		get
		{
			return base.HasStatusEffectOfType<StatusEffect_Spoiling>();
		}
	}

	public override void UpdateCard()
	{
		if (this.FoodValue > 0)
		{
			this.MyGameCard.SpecialValue = new int?(this.FoodValue);
			this.MyGameCard.SpecialIcon.sprite = SpriteManager.instance.FoodIcon;
		}
		if (this.FoodValue <= 0)
		{
			this.FoodValue = 0;
		}
		if (!this.MyGameCard.IsDemoCard && this.MyGameCard.MyBoard.BoardOptions.FoodSpoils && !this.IsSpoiling && this.CanSpoil)
		{
			this.SpoilTime += Time.deltaTime * WorldManager.instance.TimeScale;
			float num = WorldManager.instance.MonthTime;
			if (this.IsCookedFood)
			{
				num = WorldManager.instance.MonthTime * 2f;
			}
			if (this.SpoilTime >= num)
			{
				base.AddStatusEffect(new StatusEffect_Spoiling());
			}
		}
		base.UpdateCard();
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard is Food || otherCard.MyCardType == CardType.Resources || ((otherCard is BaseVillager || otherCard is Worker) && this.CanBePlacedOnVillager);
	}

	public virtual void ConsumedBy(Combatable vill)
	{
		vill.ParseAction(this.ResultAction);
	}

	private bool CanGiveFoodPoisoning()
	{
		return WorldManager.instance.CurseIsActive(CurseType.Death);
	}

	public virtual void FullyConsumed(CardData c)
	{
		c.ParseAction(this.FullyConsumeResultAction);
	}

	public override void OnSellCard()
	{
		if (WorldManager.instance.CurseIsActive(CurseType.Happiness) && this.FoodValue > 0)
		{
			WorldManager.instance.TryCreateUnhappiness(base.transform.position, 1);
			WorldManager.instance.Cutscene.QueueCutsceneIfNotPlayed("happiness_food");
		}
		base.OnSellCard();
	}

	[Header("Food")]
	[ExtraData("food_value")]
	public int FoodValue = 1;

	public bool CanBePlacedOnVillager;

	[ExtraData("spoil_time")]
	[HideInInspector]
	public float SpoilTime;

	public bool CanSpoil = true;

	[Header("Special Actions")]
	public string ResultAction;

	public string FullyConsumeResultAction;

	[HideInInspector]
	public bool IsReserved;

	[HideInInspector]
	public bool IsConsumed;
}
