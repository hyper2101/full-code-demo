using System;
using System.Collections.Generic;
using UnityEngine;

public enum TargetStat
{
	MaxHealth,
	AttackDamage,
	Speed
}

public enum ModifierType
{
	Flat,        // Cộng trực tiếp (ví dụ: +10 ATK)
	PercentAdd,  // Cộng phần trăm tích lũy (ví dụ: +20% HP)
	PercentMult  // Nhân phần trăm trực tiếp (ví dụ: -50% HP)
}

public class StatModifier
{
	public string Id { get; }
	public TargetStat Stat { get; }
	public ModifierType Type { get; }
	public float Value { get; }
	public float Duration { get; set; } // Thời gian tính bằng giây, nếu <= 0f nghĩa là vĩnh viễn
	public string Source { get; }

	public StatModifier(string id, TargetStat stat, ModifierType type, float value, float duration = 0f, string source = "")
	{
		Id = id;
		Stat = stat;
		Type = type;
		Value = value;
		Duration = duration;
		Source = source;
	}
}

public class ModifierPipeline
{
	private readonly List<StatModifier> _modifiers = new List<StatModifier>();
	private readonly Action _onChangedCallback;

	public ModifierPipeline(Action onChangedCallback)
	{
		_onChangedCallback = onChangedCallback;
	}

	public void AddModifier(StatModifier mod)
	{
		if (mod == null) return;
		
		// Kiểm tra Stacking Rules: Nếu cùng ID, thay thế hoặc refresh
		StatModifier existing = _modifiers.Find(m => m.Id == mod.Id);
		if (existing != null)
		{
			_modifiers.Remove(existing);
		}

		_modifiers.Add(mod);
		_onChangedCallback?.Invoke();
	}

	public void RemoveModifier(string id)
	{
		StatModifier mod = _modifiers.Find(m => m.Id == id);
		if (mod != null)
		{
			_modifiers.Remove(mod);
			_onChangedCallback?.Invoke();
		}
	}

	public void RemoveAllModifiersFromSource(string source)
	{
		int removedCount = _modifiers.RemoveAll(m => m.Source == source);
		if (removedCount > 0)
		{
			_onChangedCallback?.Invoke();
		}
	}

	public void UpdateTick(float deltaTime)
	{
		bool changed = false;
		for (int i = _modifiers.Count - 1; i >= 0; i--)
		{
			if (_modifiers[i].Duration > 0f)
			{
				_modifiers[i].Duration -= deltaTime;
				if (_modifiers[i].Duration <= 0f)
				{
					_modifiers.RemoveAt(i);
					changed = true;
				}
			}
		}

		if (changed)
		{
			_onChangedCallback?.Invoke();
		}
	}

	public float CalculateValue(TargetStat stat, float baseValue)
	{
		float finalValue = baseValue;
		float percentAddSum = 0f;
		float percentMult = 1f;

		// Vòng 1: Tính toán các modifier Flat
		foreach (var mod in _modifiers)
		{
			if (mod.Stat == stat)
			{
				if (mod.Type == ModifierType.Flat)
				{
					finalValue += mod.Value;
				}
				else if (mod.Type == ModifierType.PercentAdd)
				{
					percentAddSum += mod.Value;
				}
				else if (mod.Type == ModifierType.PercentMult)
				{
					percentMult *= mod.Value;
				}
			}
		}

		// Vòng 2: Áp dụng PercentAdd (ví dụ baseValue * (1 + 0.20 + 0.10))
		if (percentAddSum != 0f)
		{
			finalValue += baseValue * percentAddSum;
		}

		// Vòng 3: Áp dụng PercentMult (ví dụ finalValue * 0.5)
		finalValue *= percentMult;

		return finalValue;
	}

	public List<StatModifier> GetActiveModifiers()
	{
		return new List<StatModifier>(_modifiers);
	}

	public void Clear()
	{
		_modifiers.Clear();
		_onChangedCallback?.Invoke();
	}
}
