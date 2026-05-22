using System;
using UnityEngine;

public class DragonEgg : CardData
{
	public override void UpdateCard()
	{
		this.Icon = this.NormalIcon;
		if (this.CrackedState == 1)
		{
			this.Icon = this.CrackedIcon;
		}
		if (this.CrackedState == 2)
		{
			this.Icon = this.CrackedIcon_2;
		}
		this.NameTerm = ((this.CrackedState == 0) ? "card_dragon_egg_name" : "card_dragon_egg_name_cracked");
		this.MyGameCard.UpdateIcon();
		base.UpdateCard();
	}

	public int CrackedState;

	public Sprite NormalIcon;

	public Sprite CrackedIcon;

	public Sprite CrackedIcon_2;

	public AudioClip CrackedSound;

	public AudioClip CrackedSound2;
}
