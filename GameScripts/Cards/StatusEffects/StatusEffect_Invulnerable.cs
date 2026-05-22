using System;
using UnityEngine;

public class StatusEffect_Invulnerable : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "invulnerable";
		}
	}

	public override Sprite Sprite
	{
		get
		{
			return SpriteManager.instance.InvulnerableEffect;
		}
	}

	public override void Update()
	{
		this.FillAmount = new float?(1f - this.InvulnerableTimer / 5f);
		this.InvulnerableTimer += Time.deltaTime * WorldManager.instance.TimeScale;
		if (this.InvulnerableTimer >= 5f)
		{
			this.InvulnerableTimer = 0f;
			base.ParentCard.RemoveStatusEffect(this);
		}
		base.Update();
	}

	[ExtraData("invulnerable_timer")]
	public float InvulnerableTimer;

	private const float invulnerableTime = 5f;
}
