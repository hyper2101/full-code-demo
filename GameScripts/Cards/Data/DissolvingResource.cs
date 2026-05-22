using System;
using UnityEngine;

public class DissolvingResource : Resource
{
	public override void UpdateCard()
	{
		if (!base.HasStatusEffectOfType<StatusEffect_Dissolving>() && !this.MyGameCard.TimerRunningInStack)
		{
			base.AddStatusEffect(new StatusEffect_Dissolving());
		}
		else if (base.HasStatusEffectOfType<StatusEffect_Dissolving>() && this.MyGameCard.TimerRunningInStack)
		{
			base.RemoveStatusEffect<StatusEffect_Dissolving>();
		}
		base.UpdateCard();
	}

	public void Dissolve()
	{
		AudioManager.me.PlaySound(this.DissolveSound, base.transform, 1f, 0.5f);
		this.MyGameCard.DestroyCard(true, true);
	}

	public float DissolvingTimeMultiplier = 1f;

	public AudioClip DissolveSound;
}
