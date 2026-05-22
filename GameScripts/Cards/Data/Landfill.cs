using System;
using UnityEngine;

public class Landfill : SewerCard
{
	public override void OnInitialCreate()
	{
		this.PollutionOverflow = Random.Range(this.PollutionOverflowMin, this.PollutionOverflowMax);
		base.OnInitialCreate();
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		return !this.IsOverflowing && otherCard.Id == "pollution";
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	public override void UpdateCardText()
	{
		if (this.IsOverflowing)
		{
			this.nameOverride = SokLoc.Translate("card_overflowing_landfill_name");
			this.descriptionOverride = SokLoc.Translate("card_overflowing_landfill_description");
			return;
		}
		if (this.StoredPollution <= 0)
		{
			this.descriptionOverride = SokLoc.Translate("card_landfill_description", new LocParam[] { LocParam.Create("amount", this.PollutionOverflowMin.ToString()) });
			return;
		}
		this.descriptionOverride = SokLoc.Translate("card_landfill_description_long", new LocParam[]
		{
			LocParam.Create("amount", this.PollutionOverflowMin.ToString()),
			LocParam.Create("current", this.StoredPollution.ToString())
		});
	}

	public override void UpdateCard()
	{
		if (!this.IsOverflowing)
		{
			if (this.MyGameCard.HasChild)
			{
				if (base.AllChildrenMatchPredicate((CardData x) => x is Pollution))
				{
					foreach (CardData cardData in base.ChildrenMatchingPredicate((CardData x) => x is Pollution))
					{
						Pollution pollution = (Pollution)cardData;
						this.StoredPollution += pollution.PollutionAmount;
						pollution.PollutionAmount -= pollution.PollutionAmount;
						if (pollution.PollutionAmount == 0)
						{
							pollution.MyGameCard.DestroyCard(true, true);
						}
						if (this.StoredPollution >= this.PollutionOverflow)
						{
							this.IsOverflowing = true;
							GameCamera.instance.Screenshake = 1f;
							pollution.MyGameCard.RemoveFromParent();
							AudioManager.me.PlaySound(AudioManager.me.LandfillOverflow, base.transform, 1f, 0.3f);
							WorldManager.instance.Cutscene.QueueCutscene("cities_landfill_overflow");
							break;
						}
					}
				}
			}
			if (!this.MyGameCard.TimerRunning && this.StoredPollution > 0)
			{
				this.MyGameCard.StartTimer(60f, new TimerAction(this.DumpPollution), SokLoc.Translate("card_landfill_status_1", new LocParam[] { LocParam.Create("amount", this.PollutionRemovalRate.ToString()) }), base.GetActionId("DumpPollution"), true, false, false);
			}
			if (this.StoredPollution >= this.PollutionOverflowMin / 2)
			{
				this.Icon = this.HalfFullIcon;
				this.MyGameCard.UpdateIcon();
			}
			else
			{
				this.Icon = this.EmptyIcon;
				this.MyGameCard.UpdateIcon();
			}
			this.MyGameCard.SpecialValue = new int?(this.StoredPollution);
		}
		else
		{
			this.Icon = this.FullIcon;
			this.MyGameCard.UpdateIcon();
			this.MyGameCard.CancelTimer(base.GetActionId("DumpPollution"));
			if (!this.MyGameCard.TimerRunning)
			{
				this.MyGameCard.StartTimer(120f, new TimerAction(this.ResolveOverflow), SokLoc.Translate("card_landfill_status_2"), base.GetActionId("ResolveOverflow"), true, false, false);
			}
		}
		this.MyGameCard.SpecialIcon.sprite = SpriteManager.instance.PollutionIcon;
		base.UpdateCard();
	}

	[TimedAction("resolve_overflow")]
	public void ResolveOverflow()
	{
		WorldManager.instance.CreateSmoke(base.Position);
		this.StoredPollution = this.PollutionOverflow - 10;
		this.IsOverflowing = false;
	}

	[TimedAction("dump_pollution")]
	public void DumpPollution()
	{
		if (this.StoredPollution > 0)
		{
			int num = Mathf.Min(this.StoredPollution, this.PollutionRemovalRate);
			this.StoredPollution -= num;
			AudioManager.me.PlaySound(AudioManager.me.ClearPollution, base.transform, 1f, 0.3f);
			WorldManager.instance.CreateSmoke(base.Position);
		}
	}

	public int PollutionOverflowMin;

	public int PollutionOverflowMax;

	[ExtraData("is_overflowing")]
	public bool IsOverflowing;

	public Sprite EmptyIcon;

	public Sprite HalfFullIcon;

	public Sprite FullIcon;

	[HideInInspector]
	[ExtraData("stored_pollution")]
	public int StoredPollution;

	[HideInInspector]
	[ExtraData("pollution_overflow")]
	public int PollutionOverflow;

	public int PollutionRemovalRate = 5;
}
