using System;
using UnityEngine;

public class StatusEffect_Stunned : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "stunned";
		}
	}

	public override Sprite Sprite
	{
		get
		{
			return SpriteManager.instance.StunnedEffect;
		}
	}

	public override void Update()
	{
		this.FillAmount = new float?(1f - this.StatusTimer / 5f);
		if (this.StatusTimer >= 5f)
		{
			base.ParentCard.RemoveStatusEffect(this);
		}
		base.Update();
	}
}
