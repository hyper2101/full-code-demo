using System;
using System.Collections.Generic;
using UnityEngine;

public enum StatusEffectStackingRule
{
	RefreshDuration, // Reset thời gian hiệu lực
	AccumulateStacks, // Tăng stack (gây sát thương dồn)
	ReplaceStronger  // Thay bằng cái mạnh hơn
}

public class ActiveStatusEffect
{
	public string Id { get; }
	public string DisplayName { get; }
	public float Duration { get; set; }
	public float MaxDuration { get; }
	public int Stacks { get; set; }
	public int MaxStacks { get; }
	public StatusEffectStackingRule StackingRule { get; }
	public float TickInterval { get; }
	public float TickTimer { get; set; }
	
	public Action<Combatable, int> OnTickEffect { get; }

	public ActiveStatusEffect(string id, string displayName, float duration, StatusEffectStackingRule stackingRule, 
	                          int maxStacks = 1, float tickInterval = 0f, Action<Combatable, int> onTickEffect = null)
	{
		Id = id;
		DisplayName = displayName;
		Duration = duration;
		MaxDuration = duration;
		StackingRule = stackingRule;
		Stacks = 1;
		MaxStacks = maxStacks;
		TickInterval = tickInterval;
		TickTimer = 0f;
		OnTickEffect = onTickEffect;
	}
}

public class StatusEffectPipeline
{
	private readonly List<ActiveStatusEffect> _activeEffects = new List<ActiveStatusEffect>();
	private readonly Combatable _owner;

	public StatusEffectPipeline(Combatable owner)
	{
		_owner = owner;
	}

	public void ApplyEffect(ActiveStatusEffect effect)
	{
		if (effect == null || _owner == null) return;

		ActiveStatusEffect existing = _activeEffects.Find(e => e.Id == effect.Id);
		if (existing != null)
		{
			switch (existing.StackingRule)
			{
				case StatusEffectStackingRule.RefreshDuration:
					existing.Duration = existing.MaxDuration;
					break;
				case StatusEffectStackingRule.AccumulateStacks:
					existing.Duration = existing.MaxDuration;
					if (existing.Stacks < existing.MaxStacks)
					{
						existing.Stacks++;
					}
					break;
				case StatusEffectStackingRule.ReplaceStronger:
					if (effect.Duration > existing.Duration)
					{
						existing.Duration = effect.Duration;
					}
					break;
			}
		}
		else
		{
			_activeEffects.Add(effect);
		}

		// Phát sự kiện an toàn qua Event Bus
		EventBus.Publish(new OnStatusAppliedEvent(_owner, effect.Id));
	}

	public void RemoveEffect(string id)
	{
		_activeEffects.RemoveAll(e => e.Id == id);
	}

	public void UpdateTick(float deltaTime)
	{
		if (_owner == null) return;

		for (int i = _activeEffects.Count - 1; i >= 0; i--)
		{
			ActiveStatusEffect effect = _activeEffects[i];
			effect.Duration -= deltaTime;

			// Quản lý các hiệu ứng DOT (Damage Over Time) theo chu kỳ tick
			if (effect.TickInterval > 0f)
			{
				effect.TickTimer += deltaTime;
				if (effect.TickTimer >= effect.TickInterval)
				{
					effect.TickTimer -= effect.TickInterval;
					effect.OnTickEffect?.Invoke(_owner, effect.Stacks);
				}
			}

			if (effect.Duration <= 0f)
			{
				_activeEffects.RemoveAt(i);
			}
		}
	}

	public int GetSlowPercent()
	{
		// Lấy chỉ số làm chậm mạnh nhất (Strongest Only) từ các hiệu ứng Băng
		int maxSlow = 0;
		foreach (var effect in _activeEffects)
		{
			if (effect.Id.ToLower().Contains("slow") || effect.Id.ToLower().Contains("freeze"))
			{
				maxSlow = Math.Max(maxSlow, 40); // Làm chậm mặc định 40%
			}
		}
		return maxSlow;
	}

	public int GetEffectStacks(string id)
	{
		ActiveStatusEffect effect = _activeEffects.Find(e => e.Id == id);
		return effect != null ? effect.Stacks : 0;
	}

	public void Clear()
	{
		_activeEffects.Clear();
	}
}

// Hệ thống bảo vệ Chain Lightning (Xích Sét) chống vô hạn đệ quy
public static class ChainLightningSystem
{
	public static void CastChainLightning(Combatable origin, Combatable target, int baseDamage, int maxDepth = 4, float falloff = 0.25f)
	{
		if (origin == null || target == null) return;

		HashSet<string> alreadyHit = new HashSet<string>();
		ExecuteChain(origin, target, baseDamage, 1, maxDepth, falloff, alreadyHit);
	}

	private static void ExecuteChain(Combatable origin, Combatable currentTarget, int currentDamage, int currentDepth, 
	                                 int maxDepth, float falloff, HashSet<string> alreadyHit)
	{
		if (currentDepth > maxDepth || currentDamage <= 0 || currentTarget == null || currentTarget.MyGameCard == null) return;

		// Đăng ký ID mục tiêu vào lịch sử để chống giật lại mục tiêu cũ
		alreadyHit.Add(currentTarget.MyGameCard.CardData.UniqueId);
		
		// Gây sát thương lên mục tiêu hiện tại
		currentTarget.Damage(currentDamage);
		currentTarget.MyGameCard.RotWobble(0.8f);

		// Tìm mục tiêu lân cận tiếp theo chưa từng bị sét đánh trúng
		Combatable nextTarget = FindNextChainTarget(currentTarget, alreadyHit);
		if (nextTarget != null)
		{
			// Giảm sát thương sau mỗi lần giật sét lan (Falloff 25%)
			int nextDamage = Mathf.Max(1, Mathf.RoundToInt(currentDamage * (1f - falloff)));
			
			// Thực hiện lần giật sét tiếp theo
			ExecuteChain(origin, nextTarget, nextDamage, currentDepth + 1, maxDepth, falloff, alreadyHit);
		}
	}

	private static Combatable FindNextChainTarget(Combatable current, HashSet<string> alreadyHit)
	{
		if (WorldManager.instance == null || WorldManager.instance.AllCards == null) return null;

		Combatable closest = null;
		float minDistance = float.MaxValue;
		Vector3 currentPos = current.transform.position;

		foreach (GameCard gc in WorldManager.instance.AllCards)
		{
			if (gc != null && gc.CardData is Combatable targetCombatable && gc.CardData != current.CardData && !gc.Destroyed)
			{
				if (alreadyHit.Contains(gc.CardData.UniqueId)) continue;

				float dist = Vector3.Distance(currentPos, gc.transform.position);
				if (dist < 4.0f && dist < minDistance) // Giới hạn tầm nhảy của xích sét là 4.0 đơn vị
				{
					minDistance = dist;
					closest = targetCombatable;
				}
			}
		}

		return closest;
	}
}
