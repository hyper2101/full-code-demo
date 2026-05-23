using System;
using System.Collections.Generic;
using UnityEngine;

public enum RelicEffectType
{
	AutoFarm,
	AutoHeal,
	AutoCollect
}

public interface IRelicEffect
{
	void Execute(WorldManager manager, int stackCount);
}

// 1. Chiến lược Tự động Canh Tác (Auto Farm Relic Strategy)
public class AutoFarmRelicEffect : IRelicEffect
{
	public void Execute(WorldManager manager, int stackCount)
	{
		if (manager == null || manager.AllCards == null || stackCount <= 0) return;

		// Tốc độ tăng tốc phụ thuộc tuyến tính nhưng giảm dần theo số stack (diminishing returns nhẹ)
		float boostAmount = 1.5f * stackCount; 

		foreach (GameCard gc in manager.AllCards)
		{
			if (gc != null && gc.CardData != null && !gc.Destroyed)
			{
				if (gc.CardData is Farmland || gc.CardData.Id == "garden" || gc.CardData.Id == "greenhouse")
				{
					if (gc.TimerRunning && (gc.TimerActionId.ToLower().Contains("harvest") || gc.TimerActionId.ToLower().Contains("water")))
					{
						gc.CurrentTimerTime += boostAmount;
					}
				}
			}
		}
	}
}

// 2. Chiến lược Tự động Trị Liệu (Auto Heal Relic Strategy)
public class AutoHealRelicEffect : IRelicEffect
{
	public void Execute(WorldManager manager, int stackCount)
	{
		if (manager == null || manager.AllCards == null || stackCount <= 0) return;

		CatCardData lowestCat = null;
		int lowestHpDiff = 0;

		foreach (GameCard gc in manager.AllCards)
		{
			if (gc != null && gc.CardData is CatCardData cat && !gc.Destroyed)
			{
				int hpDiff = cat.ProcessedCombatStats.MaxHealth - cat.HealthPoints;
				if (hpDiff > lowestHpDiff)
				{
					lowestHpDiff = hpDiff;
					lowestCat = cat;
				}
			}
		}

		if (lowestCat != null && lowestHpDiff > 0)
		{
			// Lượng hồi máu tăng theo số stack cổ vật
			int healAmount = 3 * stackCount + 3;
			lowestCat.HealthPoints = Mathf.Min(lowestCat.HealthPoints + healAmount, lowestCat.ProcessedCombatStats.MaxHealth);
			Debug.Log($"[Relic Strategy] AutoHeal hồi phục +{healAmount} HP cho mèo {lowestCat.Name} (Cổ vật stack x{stackCount})");
		}
	}
}

// 3. Chiến lược Tự động Thu Gom gộp thẻ (Auto Collect Relic Strategy)
public class AutoCollectRelicEffect : IRelicEffect
{
	public void Execute(WorldManager manager, int stackCount)
	{
		// Auto Collect chạy mượt mà bằng Lerp mỗi frame trong RelicAutomationSystem.
		// Không cần hành động gián đoạn chu kỳ 5 giây của Execute này.
	}
}
