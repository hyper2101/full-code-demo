using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class CombatStats
{
	public void InitStats(CombatStats stats)
	{
		this.Initiative = stats.Initiative;
		this.SpecialHits = new List<SpecialHit>();
		if (stats.SpecialHits != null)
		{
			foreach (var sh in stats.SpecialHits)
			{
				this.SpecialHits.Add(sh.Clone());
			}
		}
		this.MaxHealth = stats.MaxHealth;
		this.AttackDamage = stats.AttackDamage;
		this.Defence = stats.Defence;
	}

	public CombatStats Clone()
	{
		CombatStats clone = new CombatStats();
		clone.InitStats(this);
		clone.InitiativeIncrement = this.InitiativeIncrement;
		clone.AttackDamageIncrement = this.AttackDamageIncrement;
		clone.DefenceIncrement = this.DefenceIncrement;
		return clone;
	}

	public void AddStats(CombatStats equipment)
	{
		this.Initiative += equipment.InitiativeIncrement;
		this.SpecialHits = this.AddSpecialHits(equipment.SpecialHits);
		this.MaxHealth += equipment.MaxHealth;
		this.AttackDamage += equipment.AttackDamageIncrement;
		this.Defence += equipment.DefenceIncrement;
	}

	public string SummarizeSpecialHits()
	{
		string text = "";
		for (int i = 0; i < this.SpecialHits.Count; i++)
		{
			SpecialHit specialHit = this.SpecialHits[i];
			text += specialHit.GetText();
			if (i < this.SpecialHits.Count - 1)
			{
				text += "\n";
			}
		}
		return text;
	}

	public List<SpecialHit> AddSpecialHits(List<SpecialHit> specialHits)
	{
		List<SpecialHit> list = new List<SpecialHit>();
		using (List<SpecialHit>.Enumerator enumerator = specialHits.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				SpecialHit hit2 = enumerator.Current;
				SpecialHit specialHit = this.SpecialHits.Find((SpecialHit x) => x.HitType == hit2.HitType && x.Target == hit2.Target);
				SpecialHit specialHit2 = new SpecialHit();
				specialHit2.HitType = hit2.HitType;
				specialHit2.Target = hit2.Target;
				specialHit2.Chance = hit2.Chance;
				if (specialHit != null)
				{
					specialHit2.Chance += specialHit.Chance;
				}
				list.Add(specialHit2);
			}
		}
		using (List<SpecialHit>.Enumerator enumerator = this.SpecialHits.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				SpecialHit hit = enumerator.Current;
				if (list.FindIndex((SpecialHit x) => x.HitType == hit.HitType && x.Target == hit.Target) == -1)
				{
					list.Add(hit);
				}
			}
		}
		return list;
	}

	public int MaxHealth;

	[SerializeField]
	[FormerlySerializedAs("AttackSpeed")]
	[FormerlySerializedAs("attackSpeed")]
	private float initiative = 0f;

	public float Initiative
	{
		get => initiative;
		set => initiative = value;
	}

	public int AttackDamage = 1;

	public int Defence = 1;

	[SerializeField]
	[FormerlySerializedAs("AttackSpeedIncrement")]
	[FormerlySerializedAs("attackSpeedIncrement")]
	private float initiativeIncrement;

	public float InitiativeIncrement
	{
		get => initiativeIncrement;
		set => initiativeIncrement = value;
	}

	public int AttackDamageIncrement;

	public int DefenceIncrement;

	public List<SpecialHit> SpecialHits = new List<SpecialHit>();
}
