using System;
using UnityEngine;

public class StatusEffect_MaxOnBoard : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "max_on_board";
		}
	}

	public override Sprite Sprite
	{
		get
		{
			return SpriteManager.instance.MaxReachedEffect;
		}
	}
}
