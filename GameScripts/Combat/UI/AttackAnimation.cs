using System;
using UnityEngine;

public class AttackAnimation
{
	public virtual bool IsBlocking { get; }

	public virtual void Start()
	{
		this.HasStarted = true;
		this.Position = (this.TargetPosition = this.AttackStartPosition);
	}

	public virtual void Update()
	{
		this.knockback = Vector3.Lerp(this.knockback, Vector3.zero, Time.deltaTime * 6f * WorldManager.instance.TimeScale);
	}

	public virtual void SetKnockback(Projectile p)
	{
		this.knockback = -(p.TargetPosition - p.StartPosition).normalized * p.KnockbackMultiplier;
		if (this.Origin != null)
		{
			this.Origin.MyGameCard.transform.localScale *= 0.9f;
		}
	}

	public bool HasStarted;

	public bool IsDone;

	public Combatable Origin;

	public Combatable Target;

	public Vector3 AttackStartPosition;

	public Vector3 AttackTargetPosition;

	public Vector3 Position;

	public Vector3 TargetPosition;

	protected Vector3 knockback;
}
