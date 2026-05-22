using System;
using System.Collections;
using UnityEngine;

public class Altar : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == "charcoal" || otherCard.Id == "raw_meat" || otherCard.Id == "gold";
	}

	public override void UpdateCard()
	{
		if (!this.inCutscene)
		{
			if (base.ChildrenMatchingPredicateCount((CardData c) => c.Id == "charcoal") >= 1)
			{
				this.TryStartSpiritCutscene(Cutscenes.AltarIntro(this, CurseType.Happiness));
			}
			else if (base.ChildrenMatchingPredicateCount((CardData c) => c.Id == "raw_meat") >= 1)
			{
				this.TryStartSpiritCutscene(Cutscenes.AltarIntro(this, CurseType.Death));
			}
			else if (base.ChildrenMatchingPredicateCount((CardData c) => c.Id == "gold") >= 1)
			{
				this.TryStartSpiritCutscene(Cutscenes.AltarIntro(this, CurseType.Greed));
			}
		}
		base.UpdateCard();
	}

	private void TryStartSpiritCutscene(IEnumerator cutscene)
	{
		if (WorldManager.instance.IsSpiritDlcActive())
		{
			this.inCutscene = true;
			AudioManager.me.PlaySound2D(this.AltarActive, 1f, 0.2f);
			WorldManager.instance.Cutscene.QueueCutscene(cutscene);
			return;
		}
		GameCanvas.instance.ShowDlcNotInstalledModal();
		this.MyGameCard.Child.RemoveFromParent();
	}

	public override bool CanHaveCardsWhileHasStatus()
	{
		return true;
	}

	public bool inCutscene;

	public AudioClip AltarActive;
}
