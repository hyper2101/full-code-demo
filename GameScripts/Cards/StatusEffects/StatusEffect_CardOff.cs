using System;
using UnityEngine;

public class StatusEffect_CardOff : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "card_off";
		}
	}

	public override Sprite Sprite
	{
		get
		{
			return SpriteManager.instance.CardOffEffect;
		}
	}
}
