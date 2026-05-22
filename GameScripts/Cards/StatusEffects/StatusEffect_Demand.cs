using System;
using UnityEngine;

public class StatusEffect_Demand : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "demand";
		}
	}

	public override Sprite Sprite
	{
		get
		{
			return SpriteManager.instance.DemandEffect;
		}
	}
}
