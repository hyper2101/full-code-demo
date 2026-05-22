using System;
using UnityEngine;

public class StatusEffect_Frenzy : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "frenzy";
		}
	}

	public override Sprite Sprite
	{
		get
		{
			return SpriteManager.instance.FrenzyEffect;
		}
	}

	public override void Update()
	{
		this.FillAmount = new float?(1f - this.FrenzyTimer / 10f);
		this.FrenzyTimer += Time.deltaTime * WorldManager.instance.TimeScale;
		if (this.FrenzyTimer >= 10f)
		{
			this.FrenzyTimer = 0f;
			base.ParentCard.RemoveStatusEffect(this);
		}
		base.Update();
	}

	[ExtraData("frenzy_timer")]
	public float FrenzyTimer;
}
