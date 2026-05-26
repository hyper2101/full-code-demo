using System;
using UnityEngine;

public class MagicProjectile : Projectile
{
	protected override void Start()
	{
		base.Start();
		this.startScale = base.transform.localScale;
		base.transform.localScale = Vector3.zero;
	}

	protected override void Update()
	{
		Vector3 vector = this.TargetPosition - this.StartPosition;
		this.waitTimer += Time.deltaTime * WorldManager.instance.TimeScale;
		if (this.waitTimer >= this.WaitTime)
		{
			this.position += vector.normalized * this.Speed * Time.deltaTime * WorldManager.instance.TimeScale;
			if (!this.knockedBack)
			{
				this.knockedBack = true;
				AudioManager.me.PlaySound2D(AudioManager.me.MagicRelease, Random.Range(0.8f, 1.2f), 0.5f);
				this.OriginAnimation.SetKnockback(this);
			}
		}
		base.transform.localScale = Vector3.Lerp(base.transform.localScale, this.startScale, Time.deltaTime * 6f);
		base.Update();
	}

	public float WaitTime = 0.5f;

	private float waitTimer;

	private Vector3 startScale;

	private bool knockedBack;
}
