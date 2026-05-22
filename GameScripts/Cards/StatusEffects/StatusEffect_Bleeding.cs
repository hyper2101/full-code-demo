using System;
using UnityEngine;

public class StatusEffect_Bleeding : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "bleeding";
		}
	}

	public override Sprite Sprite
	{
		get
		{
			return SpriteManager.instance.BleedingEffect;
		}
	}

	public override void Update()
	{
		this.FillAmount = new float?(1f - this.StatusTimer / 10f);
		this.DamageTimer += Time.deltaTime * WorldManager.instance.TimeScale;
		if (this.DamageTimer >= 2f)
		{
			Combatable combatable = base.ParentCard as Combatable;
			if (combatable != null)
			{
				combatable.Damage(1);
				AudioManager.me.PlaySound2D(AudioManager.me.Bleed, Random.Range(0.8f, 1.2f), 0.2f);
				combatable.CreateHitText("1", PrefabManager.instance.BleedHitText);
			}
			this.DamageTimer = 0f;
		}
		if (this.StatusTimer >= 10f)
		{
			this.DamageTimer = 0f;
			this.StatusTimer = 0f;
			base.ParentCard.RemoveStatusEffect(this);
		}
		base.Update();
	}

	[ExtraData("damage_timer")]
	public float DamageTimer;
}
