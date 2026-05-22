using System;

[Serializable]
public class CardChance
{
	public CardChance()
	{
	}

	public CardChance(string id, int chance)
	{
		this.Id = id;
		this.Chance = chance;
	}

	[Card]
	public string Id;

	public int Chance = 1;

	public float PercentageChance;

	public bool HasMaxCount;

	public int MaxCountToGive = 1;

	public bool HasPrerequisiteCard;

	[Card]
	public string PrerequisiteCardId;

	public bool IsEnemy;

	public EnemySetCardBag EnemyBag;

	public float Strength;
}
