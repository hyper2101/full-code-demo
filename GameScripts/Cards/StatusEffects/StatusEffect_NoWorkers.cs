using System;
using UnityEngine;

public class StatusEffect_NoWorkers : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "no_workers";
		}
	}

	public override Sprite Sprite
	{
		get
		{
			return SpriteManager.instance.NoWorkersEffect;
		}
	}

	public override bool FadeInNonDefaultView
	{
		get
		{
			return false;
		}
	}

	public override void Update()
	{
		this.FillAmount = new float?(FRILerp.Spring((this.FillAmount != null) ? this.FillAmount.Value : 0f, (float)base.ParentCard.MyGameCard.WorkerChildren.Count / (float)base.ParentCard.WorkerAmount, 10f, 10f, ref this.velo));
		base.Update();
	}

	private float velo;
}
