using System;
using UnityEngine;

public class CardAnimation_FakeMeleeAttack : CardAnimation
{
	public CardAnimation_FakeMeleeAttack(GameCard start, GameCard end)
	{
		this.startCard = start;
		this.endCard = end;
		this.StartPosition = this.startCard.Position;
		this.EndPosition = this.endCard.Position;
		this.Position = (this.TargetPosition = this.StartPosition);
	}

	public override void Update()
	{
		this.timer += Time.deltaTime * WorldManager.instance.CombatSpeed;
		float num = WorldManager.instance.CombatFlatPositionCurve.Evaluate(this.timer);
		float num2 = WorldManager.instance.CombatYPosition.Evaluate(this.timer);
		Vector3 zero = Vector3.zero;
		zero.x = Mathf.Lerp(this.StartPosition.x, this.EndPosition.x, num);
		zero.y = this.EndPosition.y + num2;
		zero.z = Mathf.Lerp(this.StartPosition.z, this.EndPosition.z, num);
		this.Position = (this.TargetPosition = zero);
		if (this.timer >= 0.5f && !this.attacked)
		{
			this.attacked = true;
			this.endCard.SetHitEffect(null);
			AudioManager.me.PlaySound2D(AudioManager.me.HitMelee, Random.Range(0.8f, 1.2f), 0.2f);
		}
		if (this.timer >= 1f)
		{
			this.IsDone = true;
		}
	}

	private GameCard startCard;

	private GameCard endCard;

	private bool attacked;
}
