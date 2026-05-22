using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SpawnHelper
{
	public static CardIdWithEquipment GetEnemyToSpawn(List<SetCardBagType> cardbags, float strength, bool canHaveInventory = true)
	{
		List<SpawnHelper.CardIdWithEquipmentCombat> list = SpawnHelper.GetAllPossibleEnemiesWithEquipment(SpawnHelper.GetEnemyPoolFromCardbags(cardbags, canHaveInventory));
		list.RemoveAll((SpawnHelper.CardIdWithEquipmentCombat x) => x.TotalCombatLevel > strength);
		list = list.OrderByDescending<SpawnHelper.CardIdWithEquipmentCombat, float>((SpawnHelper.CardIdWithEquipmentCombat x) => x.TotalCombatLevel).ToList<SpawnHelper.CardIdWithEquipmentCombat>();
		if (list.Count == 0)
		{
			return null;
		}
		List<string> list2 = list.Select<SpawnHelper.CardIdWithEquipmentCombat, string>((SpawnHelper.CardIdWithEquipmentCombat x) => x.Id).Distinct<string>().ToList<string>();
		string enemyId = list2.Choose<string>();
		return list.Where<SpawnHelper.CardIdWithEquipmentCombat>((SpawnHelper.CardIdWithEquipmentCombat x) => x.Id == enemyId).ToList<SpawnHelper.CardIdWithEquipmentCombat>().Choose<SpawnHelper.CardIdWithEquipmentCombat>()
			.ToCardIdWithEquipment();
	}

	public static List<CardIdWithEquipment> GetEnemiesToSpawn(List<SetCardBagType> cardbags, float strength, bool canHaveInventory = true)
	{
		return SpawnHelper.GetEnemiesToSpawn(SpawnHelper.GetEnemyPoolFromCardbags(cardbags, canHaveInventory), strength);
	}

	public static List<Combatable> GetEnemyPoolFromCardbags(List<SetCardBagType> cardbags, bool canHaveInventory)
	{
		List<Combatable> list = new List<Combatable>();
		foreach (SetCardBagType setCardBagType in cardbags)
		{
			foreach (CardChance cardChance in CardBag.GetChancesForSetCardBag(WorldManager.instance.GameDataLoader, setCardBagType, null))
			{
				Combatable combatable = WorldManager.instance.GetCardPrefab(cardChance.Id, true) as Combatable;
				if (canHaveInventory || !combatable.HasInventory)
				{
					list.Add(combatable);
				}
			}
		}
		return list;
	}

	public static List<CardIdWithEquipment> GetEnemiesToSpawn(List<Combatable> enemyPool, float maxStrength)
	{
		List<SpawnHelper.CardIdWithEquipmentCombat> list = new List<SpawnHelper.CardIdWithEquipmentCombat>();
		List<SpawnHelper.CardIdWithEquipmentCombat> allPossibleEnemiesWithEquipment = SpawnHelper.GetAllPossibleEnemiesWithEquipment(enemyPool);
		allPossibleEnemiesWithEquipment.RemoveAll((SpawnHelper.CardIdWithEquipmentCombat x) => x.TotalCombatLevel > maxStrength);
		List<SpawnHelper.CardIdWithEquipmentCombat> list2 = new List<SpawnHelper.CardIdWithEquipmentCombat>(allPossibleEnemiesWithEquipment);
		if (allPossibleEnemiesWithEquipment.Count == 0)
		{
			return new List<CardIdWithEquipment>();
		}
		float num;
		SpawnHelper.CardIdWithEquipmentCombat cardIdWithEquipmentCombat;
		for (num = 0f; num < maxStrength; num += cardIdWithEquipmentCombat.TotalCombatLevel)
		{
			float leftover = maxStrength - num;
			allPossibleEnemiesWithEquipment.RemoveAll((SpawnHelper.CardIdWithEquipmentCombat x) => x.TotalCombatLevel > leftover);
			if (allPossibleEnemiesWithEquipment.Count == 0)
			{
				break;
			}
			bool shouldHaveInventory = (double)Random.value <= 0.5;
			List<SpawnHelper.CardIdWithEquipmentCombat> list3 = allPossibleEnemiesWithEquipment.Where<SpawnHelper.CardIdWithEquipmentCombat>((SpawnHelper.CardIdWithEquipmentCombat x) => x.Equipment.Count > 0 == shouldHaveInventory).ToList<SpawnHelper.CardIdWithEquipmentCombat>();
			if (list3.Count == 0)
			{
				list3 = allPossibleEnemiesWithEquipment;
			}
			List<string> list4 = list3.Select<SpawnHelper.CardIdWithEquipmentCombat, string>((SpawnHelper.CardIdWithEquipmentCombat x) => x.Id).Distinct<string>().ToList<string>();
			string enemyId = list4.Choose<string>();
			cardIdWithEquipmentCombat = allPossibleEnemiesWithEquipment.Where<SpawnHelper.CardIdWithEquipmentCombat>((SpawnHelper.CardIdWithEquipmentCombat x) => x.Id == enemyId).ToList<SpawnHelper.CardIdWithEquipmentCombat>().Choose<SpawnHelper.CardIdWithEquipmentCombat>();
			list.Add(cardIdWithEquipmentCombat);
		}
		list = SpawnHelper.OptimizeList(list, list2);
		string text = string.Join<SpawnHelper.CardIdWithEquipmentCombat>(", ", list);
		Debug.Log(string.Format("{0} (combat level: {1})", text, num));
		return list.Select<SpawnHelper.CardIdWithEquipmentCombat, CardIdWithEquipment>((SpawnHelper.CardIdWithEquipmentCombat x) => x.ToCardIdWithEquipment()).ToList<CardIdWithEquipment>();
	}

	public static CardIdWithEquipment GetEnemyToSpawn(List<Combatable> enemyPool, float maxStrength)
	{
		List<SpawnHelper.CardIdWithEquipmentCombat> allPossibleEnemiesWithEquipment = SpawnHelper.GetAllPossibleEnemiesWithEquipment(enemyPool);
		allPossibleEnemiesWithEquipment.RemoveAll((SpawnHelper.CardIdWithEquipmentCombat x) => x.TotalCombatLevel > maxStrength);
		new List<SpawnHelper.CardIdWithEquipmentCombat>(allPossibleEnemiesWithEquipment);
		if (allPossibleEnemiesWithEquipment.Count == 0)
		{
			return null;
		}
		bool shouldHaveInventory = (double)Random.value <= 0.5;
		List<SpawnHelper.CardIdWithEquipmentCombat> list = allPossibleEnemiesWithEquipment.Where<SpawnHelper.CardIdWithEquipmentCombat>((SpawnHelper.CardIdWithEquipmentCombat x) => x.Equipment.Count > 0 == shouldHaveInventory).ToList<SpawnHelper.CardIdWithEquipmentCombat>();
		if (list.Count == 0)
		{
			list = allPossibleEnemiesWithEquipment;
		}
		List<string> list2 = list.Select<SpawnHelper.CardIdWithEquipmentCombat, string>((SpawnHelper.CardIdWithEquipmentCombat x) => x.Id).Distinct<string>().ToList<string>();
		string enemyId = list2.Choose<string>();
		return allPossibleEnemiesWithEquipment.Where<SpawnHelper.CardIdWithEquipmentCombat>((SpawnHelper.CardIdWithEquipmentCombat x) => x.Id == enemyId).ToList<SpawnHelper.CardIdWithEquipmentCombat>().Choose<SpawnHelper.CardIdWithEquipmentCombat>()
			.ToCardIdWithEquipment();
	}

	private static List<SpawnHelper.CardIdWithEquipmentCombat> OptimizeList(List<SpawnHelper.CardIdWithEquipmentCombat> list, List<SpawnHelper.CardIdWithEquipmentCombat> possibleEnemies)
	{
		possibleEnemies = possibleEnemies.OrderBy<SpawnHelper.CardIdWithEquipmentCombat, float>((SpawnHelper.CardIdWithEquipmentCombat x) => x.TotalCombatLevel).ToList<SpawnHelper.CardIdWithEquipmentCombat>();
		int num = 7;
		while (list.Count > num)
		{
			list = list.OrderBy<SpawnHelper.CardIdWithEquipmentCombat, float>((SpawnHelper.CardIdWithEquipmentCombat x) => x.TotalCombatLevel).ToList<SpawnHelper.CardIdWithEquipmentCombat>();
			SpawnHelper.CardIdWithEquipmentCombat cardIdWithEquipmentCombat = list[0];
			SpawnHelper.CardIdWithEquipmentCombat cardIdWithEquipmentCombat2 = list[1];
			float num2 = cardIdWithEquipmentCombat.TotalCombatLevel + cardIdWithEquipmentCombat2.TotalCombatLevel;
			SpawnHelper.CardIdWithEquipmentCombat enemyWithStrength = SpawnHelper.GetEnemyWithStrength(possibleEnemies, num2);
			list.Remove(cardIdWithEquipmentCombat);
			list.Remove(cardIdWithEquipmentCombat2);
			list.Add(enemyWithStrength);
		}
		return list;
	}

	private static SpawnHelper.CardIdWithEquipmentCombat GetEnemyWithStrength(List<SpawnHelper.CardIdWithEquipmentCombat> possibleEnemies, float strength)
	{
		for (int i = 0; i < possibleEnemies.Count - 1; i++)
		{
			if (possibleEnemies[i + 1].TotalCombatLevel > strength)
			{
				return possibleEnemies[i];
			}
		}
		return possibleEnemies[possibleEnemies.Count - 1];
	}

	private static List<SpawnHelper.CardIdWithEquipmentCombat> GetAllPossibleEnemiesWithEquipment(List<Combatable> enemyPool)
	{
		List<SpawnHelper.CardIdWithEquipmentCombat> list = new List<SpawnHelper.CardIdWithEquipmentCombat>();
		foreach (Combatable combatable in enemyPool)
		{
			list.Add(new SpawnHelper.CardIdWithEquipmentCombat(combatable.Id, new List<string>(), combatable.RealBaseCombatStats.CombatLevel));
			if (combatable.HasInventory)
			{
				List<Equipable> equipableOfType = SpawnHelper.GetEquipableOfType(combatable.PossibleEquipables, EquipableType.Head);
				equipableOfType.Add(null);
				List<Equipable> equipableOfType2 = SpawnHelper.GetEquipableOfType(combatable.PossibleEquipables, EquipableType.Weapon);
				equipableOfType2.Add(null);
				List<Equipable> equipableOfType3 = SpawnHelper.GetEquipableOfType(combatable.PossibleEquipables, EquipableType.Torso);
				equipableOfType3.Add(null);
				foreach (Equipable equipable in equipableOfType)
				{
					foreach (Equipable equipable2 in equipableOfType2)
					{
						foreach (Equipable equipable3 in equipableOfType3)
						{
							List<string> list2 = new List<string>();
							CombatStats combatStats = new CombatStats();
							combatStats.InitStats(combatable.RealBaseCombatStats);
							if (equipable != null)
							{
								combatStats.AddStats(equipable.MyStats);
								list2.Add(equipable.Id);
							}
							if (equipable2 != null)
							{
								combatStats.AddStats(equipable2.MyStats);
								list2.Add(equipable2.Id);
							}
							if (equipable3 != null)
							{
								combatStats.AddStats(equipable3.MyStats);
								list2.Add(equipable3.Id);
							}
							if (list2.Count > 0)
							{
								list.Add(new SpawnHelper.CardIdWithEquipmentCombat(combatable.Id, list2, combatStats.CombatLevel));
							}
						}
					}
				}
			}
		}
		return list;
	}

	private static List<Equipable> GetEquipableOfType(List<Equipable> equipables, EquipableType t)
	{
		return equipables.Where<Equipable>((Equipable x) => x.EquipableType == t).ToList<Equipable>();
	}

	private class CardIdWithEquipmentCombat
	{
		public string Id { get; set; }

		public CardIdWithEquipmentCombat(string id, List<string> equipment, float totalCombatlevel)
		{
			this.Id = id;
			this.Equipment = equipment;
			this.TotalCombatLevel = Mathf.Max(1f, totalCombatlevel);
		}

		public CardIdWithEquipment ToCardIdWithEquipment()
		{
			return new CardIdWithEquipment(this.Id, this.Equipment);
		}

		public override string ToString()
		{
			if (this.Equipment.Count == 0)
			{
				return this.Id;
			}
			return this.Id + " (" + string.Join(", ", this.Equipment) + ")";
		}

		public List<string> Equipment = new List<string>();

		public float TotalCombatLevel;
	}
}
