using System;
using UnityEngine;

public class StatusEffect_Radar : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "radar";
		}
	}

	public override Sprite Sprite
	{
		get
		{
			return SpriteManager.instance.RadarEffect;
		}
	}

	public override bool FadeInNonDefaultView
	{
		get
		{
			return false;
		}
	}

	public override string Description
	{
		get
		{
			return MewtationsLoc.Translate("statuseffect_radar_description", new LocParam[] { LocParam.Create("amount", (CitiesManager.instance.NextConflictMonth - 1).ToString()) });
		}
	}

	public override void Update()
	{
		this.FillAmount = new float?((this.StatusTimer > 1f) ? 1f : 0f);
		if (this.StatusTimer > 2f)
		{
			this.StatusTimer = 0f;
		}
		base.Update();
	}
}
