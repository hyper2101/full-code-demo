using System;
using UnityEngine;

public class StatusEffect_Damaged : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "damaged";
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
			return SpriteManager.instance.DamagedEffect;
		}
	}
}
