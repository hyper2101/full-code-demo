using System;
using UnityEngine;

public class StatusEffect_WellFed : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "wellfed";
		}
	}

	public override Sprite Sprite
	{
		get
		{
			return SpriteManager.instance.WellFedEffect;
		}
	}

	public override void Update()
	{
		this.FillAmount = new float?(1f - this.StatusTimer / WorldManager.instance.MonthTime);
		if (this.StatusTimer >= WorldManager.instance.MonthTime)
		{
			base.ParentCard.RemoveStatusEffect(this);
		}
		base.Update();
	}
}
