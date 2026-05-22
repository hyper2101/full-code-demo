using System;

public class AngryRoyal : Enemy
{
	public override void Die()
	{
		WorldManager.instance.Cutscene.QueueCutscene(GreedCutscenes.KillRoyalLiftCurse());
		base.Die();
	}

	public void DieInCutscene()
	{
		WorldManager.instance.CreateCard(base.transform.position, "royal_crown", true, true, true);
		WorldManager.instance.CreateSmoke(base.transform.position);
		base.RemoveAllStatusEffects();
		WorldManager.instance.ChangeToCard(this.MyGameCard, "corpse");
	}
}
