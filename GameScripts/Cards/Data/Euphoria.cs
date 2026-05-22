using System;
using UnityEngine;

public class Euphoria : Resource
{
	public override void OnInitialCreate()
	{
		AudioManager.me.PlaySound2D(this.CreateCardSound, 1f, 0.5f);
		base.OnInitialCreate();
	}

	protected override bool CanHaveCard(CardData otherCard)
	{
		Curse curse = otherCard as Curse;
		return (curse != null && curse.CurseType == CurseType.Happiness) || base.CanHaveCard(otherCard);
	}

	public AudioClip CreateCardSound;
}
