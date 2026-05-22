using System;
using UnityEngine;

public class StatusEffect_Depleted : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "depleted";
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
			return SpriteManager.instance.BleedingEffect;
		}
	}
}
