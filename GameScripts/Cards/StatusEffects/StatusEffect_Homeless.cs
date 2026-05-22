using System;
using UnityEngine;

public class StatusEffect_Homeless : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "homeless";
		}
	}

	public override Sprite Sprite
	{
		get
		{
			return SpriteManager.instance.HomelessEffect;
		}
	}
}
