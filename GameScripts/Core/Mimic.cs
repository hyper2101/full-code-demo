using System;
using UnityEngine;

public class Mimic : Enemy
{
	public override bool CanBeDragged
	{
		get
		{
			return !this.WasDetected;
		}
	}

	public override void Clicked()
	{
		this.Detected();
		base.Clicked();
	}

	public override void UpdateCard()
	{
		if (this.MyGameCard.IsDemoCard)
		{
			this.Detected();
		}
		if (!this.WasDetected && (base.InConflict || this.MyGameCard.BeingDragged))
		{
			this.Detected();
		}
		base.UpdateCard();
		this.Icon = (this.WasDetected ? this.RealIcon : this.TreasureChestIcon);
		this.MyGameCard.UpdateIcon();
		this.nameOverride = (this.WasDetected ? MewtationsLoc.Translate("card_mimic_name") : MewtationsLoc.Translate("card_treasure_chest_name"));
		if (!this.WasDetected)
		{
			this.descriptionOverride = MewtationsLoc.Translate("card_treasure_chest_description");
		}
		if (!this.WasDetected)
		{
			this.MyGameCard.SpecialValue = null;
		}
	}

	private void Detected()
	{
		if (this.WasDetected)
		{
			return;
		}
		if (!this.MyGameCard.IsDemoCard)
		{
			WorldManager.instance.CreateSmoke(this.MyGameCard.transform.position);
		}
		this.MyGameCard.UpdateCardPalette();
		this.WasDetected = true;
	}

	public Sprite TreasureChestIcon;

	public Sprite RealIcon;

	[ExtraData("was_detected")]
	public bool WasDetected;
}
