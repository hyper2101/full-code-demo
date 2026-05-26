using System;
using UnityEngine;

public class AttackAnimationMelee : AttackAnimation
{
	public override bool IsBlocking
	{
		get
		{
			return true;
		}
	}

	public override void Update()
	{
		this.timer += Time.deltaTime * WorldManager.instance.TimeScale * WorldManager.instance.CombatSpeed;
		float num = WorldManager.instance.CombatFlatPositionCurve.Evaluate(this.timer);
		float num2 = WorldManager.instance.CombatYPosition.Evaluate(this.timer);
		Vector3 zero = Vector3.zero;
		zero.x = Mathf.Lerp(this.AttackStartPosition.x, this.AttackTargetPosition.x, num);
		zero.y = this.AttackTargetPosition.y + num2;
		zero.z = Mathf.Lerp(this.AttackStartPosition.z, this.AttackTargetPosition.z, num);
		this.Position = (this.TargetPosition = zero);
		if (this.timer >= 0.5f && !this.attacked)
		{
			this.attacked = true;
			this.Origin.PerformAttack(this.Target, this.AttackTargetPosition);
		}
		if (this.timer >= 1f)
		{
			this.IsDone = true;
		}
		base.Update();
	}

	private bool attacked;

	private float timer;
}
