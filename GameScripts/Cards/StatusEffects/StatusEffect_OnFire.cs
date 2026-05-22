using System;
using UnityEngine;

public class StatusEffect_OnFire : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "on_fire";
		}
	}

	public override bool FadeInNonDefaultView
	{
		get
		{
			return false;
		}
	}

	public override Sprite Sprite
	{
		get
		{
			return SpriteManager.instance.OnFireEffect;
		}
	}
}
