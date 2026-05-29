using System;
using UnityEngine;

public class StatusEffect_Space : StatusEffect
{
	protected override string TermId
	{
		get
		{
			return "space";
		}
	}

	public Apartment apartment
	{
		get
		{
			return base.ParentCard as Apartment;
		}
	}

	public override Sprite Sprite
	{
		get
		{
			return SpriteManager.instance.HousingSpaceEffect;
		}
	}

	public override string Description
	{
		get
		{
			return MewtationsLoc.Translate("statuseffect_space_description", new LocParam[] { LocParam.Create("amount", this.apartment.FreeSpace.ToString()) });
		}
	}

	public override int? StatusNumber
	{
		get
		{
			return new int?(this.apartment.FreeSpace);
		}
	}

	public override Color? StatusNumberColor
	{
		get
		{
			return new Color?(Color.black);
		}
	}
}
