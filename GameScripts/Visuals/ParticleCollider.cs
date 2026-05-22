using System;
using UnityEngine;

public class ParticleCollider : Landmark
{
	protected override bool CanHaveCard(CardData otherCard)
	{
		return otherCard.Id == "educated_worker" || otherCard.Id == "genius" || otherCard.Id == "robot_genius" || otherCard.Id == "uranium";
	}

	public override bool DetermineCanHaveCardsWhenIsRoot
	{
		get
		{
			return true;
		}
	}

	public override void UpdateCard()
	{
		if (this.MyGameCard.HasChild && base.GetChildCount() == 2)
		{
			if (base.ChildrenMatchingPredicate((CardData x) => x.Id == "uranium").Count == 2 && !this.CutsceneQueued && base.WorkerAmountMet() && this.HasEnergyInput(null))
			{
				this.CutsceneQueued = true;
				WorldManager.instance.Cutscene.QueueCutscene(CitiesCutscenes.CitiesParticleCollider(this.MyGameCard));
			}
		}
		if (this.ColliderRunning)
		{
			this.MoveCard();
		}
		base.UpdateCard();
	}

	private void MoveCard()
	{
		this.MyGameCard.RotWobble(0.8f + Mathf.Cos(Time.time));
		GameCamera.instance.Screenshake = 0.1f;
	}

	[ExtraData("collider_running")]
	public bool ColliderRunning;

	[ExtraData("CutsceneQueued")]
	public bool CutsceneQueued;

	public AudioClip ColliderRunningSounds;

	public AudioClip ColliderDoneSounds;
}
