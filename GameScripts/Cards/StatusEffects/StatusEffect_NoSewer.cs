using System;
using UnityEngine;

public class StatusEffect_NoSewer : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "no_sewer";
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
			return SpriteManager.instance.NoSewerEffect;
		}
	}
}
