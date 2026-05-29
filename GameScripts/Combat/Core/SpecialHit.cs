using System;

[Serializable]
public class SpecialHit
{
	public string GetText()
	{
		string text = MewtationsLoc.Translate("target_" + this.Target.ToString().ToLower());
		return MewtationsLoc.Translate("specialhit_" + this.HitType.ToString().ToLower() + "_long", new LocParam[]
		{
			LocParam.Create("chance", this.Chance.ToString()),
			LocParam.Create("target", text)
		});
	}

	public bool IsDebuff()
	{
		return this.HitType == SpecialHitType.Poison || this.HitType == SpecialHitType.Stun || this.HitType == SpecialHitType.LifeSteal || this.HitType == SpecialHitType.Bleeding || this.HitType == SpecialHitType.Damage || this.HitType == SpecialHitType.Crit || this.HitType == SpecialHitType.Sick || this.HitType == SpecialHitType.Anxious;
	}

	public float Chance = 1f;

	public SpecialHitType HitType;

	public SpecialHitTarget Target;
}
