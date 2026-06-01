using System;
using UnityEngine;

public class StatusEffect_Paralyzed : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "paralyzed";
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
			return SpriteManager.instance.ParalyzedIcon;
		}
	}
}
