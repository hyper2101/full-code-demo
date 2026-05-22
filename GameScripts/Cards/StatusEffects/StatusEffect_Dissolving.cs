using System;
using UnityEngine;

public class StatusEffect_Dissolving : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "dissolving";
		}
	}

	public override Sprite Sprite
	{
		get
		{
			return SpriteManager.instance.DissolvingEffect;
		}
	}

	public override void Update()
	{
		float num = this.StatusTimer / WorldManager.instance.MonthTime;
		DissolvingResource dissolvingResource = base.ParentCard as DissolvingResource;
		if (dissolvingResource != null)
		{
			num *= dissolvingResource.DissolvingTimeMultiplier;
		}
		this.FillAmount = new float?(num = 1f - num * 5f);
		if (num <= 0f)
		{
			DissolvingResource dissolvingResource2 = base.ParentCard as DissolvingResource;
			if (dissolvingResource2 != null)
			{
				dissolvingResource2.Dissolve();
			}
			else
			{
				base.ParentCard.MyGameCard.DestroyCard(true, true);
			}
		}
		base.Update();
	}
}
