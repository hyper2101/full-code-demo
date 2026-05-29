using System;
using System.Collections.Generic;
using UnityEngine;

public class TrashCan : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return !(otherCard is Curse) && !(otherCard is Spirit) && !(otherCard is Royal) && !(otherCard is Shaman) && !(otherCard is Unhappiness) && !(otherCard.Id == "royal_crown") && (!(otherCard is Poop) || !WorldManager.instance.CurseIsActive(CurseType.Death)) && (otherCard.MyCardType != CardType.Humans && otherCard.MyCardType != CardType.Mobs) && otherCard.MyCardType != CardType.Fish;
	}

	public override bool CanBePushedBy(CardData otherCard)
	{
		return true;
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	public override bool DetermineCanHaveCardsWhenIsRoot
	{
		get
		{
			return true;
		}
	}

	public override void UpdateCard()
	{
		if (this.MyGameCard.HasChild)
		{
			this.MyGameCard.StartTimer(this.DestroyTime, new TimerAction(this.DestroyChild), MewtationsLoc.Translate("card_trash_can_status_0"), base.GetActionId("DestroyChild"), true, false, false);
		}
		else
		{
			this.MyGameCard.CancelTimer(base.GetActionId("DestroyChild"));
		}
		base.UpdateCard();
	}

	[TimedAction("destroy_child")]
	public void DestroyChild()
	{
		if (this.MyGameCard.HasChild)
		{
			foreach (GameCard gameCard in this.MyGameCard.GetChildCards())
			{
				gameCard.DestroyCard(false, true);
			}
			AudioManager.me.PlaySound2D(this.DestroySounds, Random.Range(1.2f, 1.3f), 0.1f);
			WorldManager.instance.CreateSmoke(this.MyGameCard.transform.position);
		}
	}

	public float DestroyTime;

	public List<AudioClip> DestroySounds;
}
