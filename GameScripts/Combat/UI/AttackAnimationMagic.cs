using System;
using UnityEngine;

public class AttackAnimationMagic : AttackAnimation
{
	public override void Start()
	{
		this.Origin.CreateProjectile(PrefabManager.instance.MagicProjectilePrefab, this.Target, this);
		AudioManager.me.PlaySound2D(AudioManager.me.MagicCharge, Random.Range(0.8f, 1.2f), 0.5f);
		base.Start();
	}

	public override void Update()
	{
		this.Position = (this.TargetPosition = this.AttackStartPosition + this.knockback);
		base.Update();
	}
}
