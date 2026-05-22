using System;
using UnityEngine;

public class RangedProjectile : Projectile
{
	protected override void Update()
	{
		Vector3 vector = this.TargetPosition - this.StartPosition;
		this.position += vector.normalized * this.Speed * Time.deltaTime * WorldManager.instance.TimeScale;
		base.Update();
	}
}
