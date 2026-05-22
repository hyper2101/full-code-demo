using System;

public class RadarStation : CardData
{
	public override void UpdateCard()
	{
		int currentMonth = WorldManager.instance.Time.CurrentMonth;
		int nextConflictMonth = CitiesManager.instance.NextConflictMonth;
		if (currentMonth >= nextConflictMonth - 3 && currentMonth < nextConflictMonth)
		{
			this.descriptionOverride = SokLoc.Translate(this.DescriptionTerm) + ". " + SokLoc.Translate("statuseffect_radar_description", new LocParam[] { LocParam.Create("amount", (CitiesManager.instance.NextConflictMonth - 1).ToString()) });
			base.AddStatusEffect(new StatusEffect_Radar());
		}
		else
		{
			base.RemoveStatusEffect<StatusEffect_Radar>();
		}
		base.UpdateCard();
	}
}
