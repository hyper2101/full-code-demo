using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Shaman : CardData
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		if (this.MyGameCard.Child == null && otherCard is BaseVillager)
		{
			return this.AltarBlueprints.Where<string>((string x) => !WorldManager.instance.HasFoundCard(x)).Count<string>() > 0;
		}
		return false;
	}

	public override void UpdateCard()
	{
		if (this.MyGameCard.Child != null && !this.MyGameCard.TimerRunning)
		{
			this.MyGameCard.StartTimer(this.TalkTime, new TimerAction(this.Talking), MewtationsLoc.Translate("card_shaman_status"), base.GetActionId("Talking"), true, false, false);
		}
		if (this.MyGameCard.Child == null && this.MyGameCard.TimerRunning)
		{
			this.MyGameCard.CancelTimer(base.GetActionId("Talking"));
		}
		base.UpdateCard();
	}

	[TimedAction("talking")]
	public void Talking()
	{
		if (this.MyGameCard.Child != null)
		{
			this.MyGameCard.Child.RemoveFromParent();
		}
		string text;
		if (!WorldManager.instance.HasFoundCard("blueprint_altar"))
		{
			text = "blueprint_altar";
		}
		else
		{
			text = this.AltarBlueprints.Where<string>((string x) => !WorldManager.instance.HasFoundCard(x)).ToList<string>().Choose<string>();
		}
		AudioManager.me.PlaySound2D(this.GiveIdea, 1f, 0.2f);
		CardData cardData = WorldManager.instance.CreateCard(base.Position, text, true, false, true);
		WorldManager.instance.CreateSmoke(base.Position);
		cardData.MyGameCard.SendIt();
		if (this.AltarBlueprints.Count<string>((string x) => !WorldManager.instance.HasFoundCard(x)) == 0)
		{
			WorldManager.instance.Cutscene.QueueCutscene(Cutscenes.ShamanLeaving(this));
		}
	}

	public AudioClip GiveIdea;

	private List<string> AltarBlueprints = new List<string> { "blueprint_altar", "death_recipe", "greed_recipe", "happiness_recipe" };

	public float TalkTime = 20f;
}
