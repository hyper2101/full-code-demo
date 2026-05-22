using System;
using UnityEngine;

public class AttackAnimationRanged : AttackAnimation
{
	public override void Start()
	{
		Projectile projectile = this.Origin.CreateProjectile(PrefabManager.instance.RangedProjectilePrefab, this.Target, this);
		this.SetKnockback(projectile);
		AudioManager.me.PlaySound2D(AudioManager.me.RangedRelease, Random.Range(0.8f, 1.2f), 0.3f);
		base.Start();
	}

	public override void Update()
	{
		this.Position = (this.TargetPosition = this.AttackStartPosition + this.knockback);
		base.Update();
	}
}
