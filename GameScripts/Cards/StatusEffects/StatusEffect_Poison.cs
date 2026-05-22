using System;
using UnityEngine;

public class StatusEffect_Poison : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "poison";
		}
	}

	public override Sprite Sprite
	{
		get
		{
			return SpriteManager.instance.PoisonEffect;
		}
	}

	public override void Update()
	{
		if (base.ParentCard is Enemy)
		{
			this.timeToDamage = 30f;
		}
		else
		{
			this.timeToDamage = 60f;
		}
		this.FillAmount = new float?(1f - this.DamageTimer / this.timeToDamage);
		this.DamageTimer += Time.deltaTime * WorldManager.instance.TimeScale;
		if (this.DamageTimer >= this.timeToDamage)
		{
			this.DamageTimer = 0f;
			Combatable combatable = base.ParentCard as Combatable;
			if (combatable != null)
			{
				this.PoisonCount++;
				combatable.Damage(3);
				combatable.CreateHitText("3", PrefabManager.instance.PoisonHitText);
				AudioManager.me.PlaySound2D(AudioManager.me.Poison, Random.Range(0.8f, 1.2f), 0.2f);
				if (base.ParentCard is Enemy && this.PoisonCount >= 3)
				{
					base.ParentCard.RemoveStatusEffect(this);
				}
			}
		}
		base.Update();
	}

	[ExtraData("damage_timer")]
	public float DamageTimer;

	private float timeToDamage = 60f;

	[ExtraData("poison_count")]
	public int PoisonCount;
}
