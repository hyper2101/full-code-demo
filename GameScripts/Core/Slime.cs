using System;

public class Slime : Enemy
{
	public override void Die()
	{
		for (int i = 0; i < 3; i++)
		{
			WorldManager.instance.CreateCard(base.transform.position, "small_slime", true, false, true).MyGameCard.SendIt();
		}
		base.Die();
	}
}
