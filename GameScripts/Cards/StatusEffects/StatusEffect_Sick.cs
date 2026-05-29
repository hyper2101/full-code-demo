using System;
using UnityEngine;

public class StatusEffect_Sick : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "sick";
		}
	}

	public override Sprite Sprite
	{
		get
		{
			return SpriteManager.instance.SickEffect;
		}
	}

	public override string Description
	{
		get
		{
			return MewtationsLoc.Translate("statuseffect_sick_description", new LocParam[]
			{
				LocParam.Create("damage", 2.ToString()),
				LocParam.Create("time", GameCanvas.FormatTimeShort(30f))
			});
		}
	}

	public override void Update()
	{
		this.FillAmount = new float?(1f - this.StatusTimer / 30f);
		if (this.StatusTimer >= 30f)
		{
			this.StatusTimer = 0f;
			Combatable combatable = base.ParentCard as Combatable;
			if (combatable != null)
			{
				combatable.Damage(2);
				combatable.CreateHitText(2.ToString(), PrefabManager.instance.SickHitText);
				AudioManager.me.PlaySound2D(AudioManager.me.Poison, Random.Range(0.8f, 1.2f), 0.2f);
			}
		}
		base.Update();
	}

	private const int damage = 2;

	private const float damageTime = 30f;
}
