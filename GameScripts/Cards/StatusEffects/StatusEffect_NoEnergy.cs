using System;
using UnityEngine;

public class StatusEffect_NoEnergy : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "no_energy";
		}
	}

	public override string Description
	{
		get
		{
			return MewtationsLoc.Translate("statuseffect_no_energy_description");
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
			return SpriteManager.instance.NoEnergyEffect;
		}
	}
}
