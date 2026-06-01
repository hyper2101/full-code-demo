using System;
using UnityEngine;

public class StatusEffect_Exhausted : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "exhausted";
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
			return SpriteManager.instance.ExhaustedIcon;
		}
	}
}
