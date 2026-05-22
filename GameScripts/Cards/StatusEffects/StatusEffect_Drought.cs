using System;
using UnityEngine;

public class StatusEffect_Drought : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "drought";
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
			return SpriteManager.instance.DroughtEffect;
		}
	}
}
