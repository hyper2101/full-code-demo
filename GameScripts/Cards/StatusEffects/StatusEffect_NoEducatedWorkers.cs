using System;
using UnityEngine;

public class StatusEffect_NoEducatedWorkers : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "no_educated_workers";
		}
	}

	public override Sprite Sprite
	{
		get
		{
			return SpriteManager.instance.NoEducatedWorkersEffect;
		}
	}

	public override bool FadeInNonDefaultView
	{
		get
		{
			return false;
		}
	}
}
