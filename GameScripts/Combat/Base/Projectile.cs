using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	protected virtual void Start()
	{
		this.position = base.transform.position;
		this.distanceToTravel = (this.TargetPosition - this.StartPosition).magnitude;
	}

	protected virtual void Update()
	{
		Vector3 vector = this.TargetPosition - this.StartPosition;
		this.timer2 += Time.deltaTime;
		base.transform.position = this.position + Extensions.Perlin(this.timer2 * this.WobbleSpeed) * this.WobbleAmplitude;
		base.transform.rotation = Quaternion.LookRotation(vector);
		if ((this.position - this.StartPosition).magnitude >= this.distanceToTravel)
		{
			if (this.ShotBy != null)
			{
				this.ShotBy.PerformAttack(this.Target, this.OriginAnimation.AttackTargetPosition);
				this.OriginAnimation.IsDone = true;
			}
			Object.Destroy(base.gameObject);
		}
	}

	[HideInInspector]
	public Vector3 StartPosition;

	[HideInInspector]
	public Vector3 TargetPosition;

	[HideInInspector]
	public Combatable ShotBy;

	[HideInInspector]
	public Combatable Target;

	public AttackAnimation OriginAnimation;

	public float KnockbackMultiplier = 0.3f;

	public float Speed = 3f;

	public float WobbleSpeed = 1f;

	public float WobbleAmplitude = 0.1f;

	private float timer2;

	protected Vector3 position;

	private float distanceToTravel;
}
