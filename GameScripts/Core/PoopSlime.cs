using System;
using UnityEngine;

public class PoopSlime : Enemy
{
	public override void Die()
	{
		for (int i = 0; i < 4; i++)
		{
			if (Random.value > 0.5f)
			{
				WorldManager.instance.CreateCard(base.Position, "poop", true, false, true).MyGameCard.SendIt();
			}
			else
			{
				WorldManager.instance.CreateCard(base.Position, "bone", true, false, true).MyGameCard.SendIt();
			}
		}
		base.Die();
	}
}
