using System;
using System.Collections.Generic;
using UnityEngine;

public class WickedWitch : Enemy
{
	public override void Die()
	{
		AudioManager.me.PlaySound2D(this.WitchDieSounds, Random.Range(1.1f, 1.3f), 0.5f);
		WorldManager.instance.CreateSmoke(this.MyGameCard.transform.position);
		QuestManager.instance.SpecialActionComplete("fight_wicked_witch", null);
		WorldManager.instance.CurrentRunVariables.FinishedWickedWitch = true;
		base.Die();
	}

	public override void UpdateCard()
	{
		this.Icon = (this.IsOldLady ? this.OldLadyIcon : this.NormalIcon);
		this.NameTerm = (this.IsOldLady ? "card_wicked_witch_name_2" : "card_wicked_witch_name");
		this.MyGameCard.UpdateIcon();
		base.UpdateCard();
		if (this.IsOldLady)
		{
			this.MyGameCard.SpecialValue = null;
		}
	}

	public List<AudioClip> WitchDieSounds;

	public bool IsOldLady;

	public Sprite NormalIcon;

	public Sprite OldLadyIcon;
}
