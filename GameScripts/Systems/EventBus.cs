using System;
using System.Collections.Generic;

public static class EventBus
{
	private static readonly Dictionary<Type, List<object>> Listeners = new Dictionary<Type, List<object>>();

	public static void Subscribe<T>(Action<T> listener)
	{
		if (listener == null) return;
		Type type = typeof(T);
		if (!Listeners.ContainsKey(type))
		{
			Listeners[type] = new List<object>();
		}
		
		// Tránh double registration
		if (!Listeners[type].Contains(listener))
		{
			Listeners[type].Add(listener);
		}
	}

	public static void Unsubscribe<T>(Action<T> listener)
	{
		if (listener == null) return;
		Type type = typeof(T);
		if (Listeners.ContainsKey(type))
		{
			Listeners[type].Remove(listener);
			if (Listeners[type].Count == 0)
			{
				Listeners.Remove(type);
			}
		}
	}

	public static void Publish<T>(T eventData)
	{
		Type type = typeof(T);
		if (Listeners.TryGetValue(type, out var list))
		{
			// Copy list để tránh lỗi "Collection was modified" nếu listener tự unsubscribe trong callback
			var listCopy = new List<object>(list);
			foreach (var listener in listCopy)
			{
				if (listener is Action<T> action)
				{
					try
					{
						action.Invoke(eventData);
					}
					catch (Exception ex)
					{
						UnityEngine.Debug.LogError($"[EventBus] Lỗi khi thực thi event {type.Name}: {ex.Message}\n{ex.StackTrace}");
					}
				}
			}
		}
	}

	public static void ClearAll()
	{
		Listeners.Clear();
	}
}

// Định nghĩa các sự kiện Immutable đại diện cho các thay đổi Async trong Game
public readonly struct OnStatsChangedEvent
{
	public readonly CatCardData Cat;
	public OnStatsChangedEvent(CatCardData cat)
	{
		Cat = cat;
	}
}

public readonly struct OnShrineStackChangedEvent
{
	public readonly ShrineCardData Shrine;
	public OnShrineStackChangedEvent(ShrineCardData shrine)
	{
		Shrine = shrine;
	}
}

public readonly struct OnBreakthroughSuccessEvent
{
	public readonly CatCardData Cat;
	public readonly int TargetLevel;
	public OnBreakthroughSuccessEvent(CatCardData cat, int targetLevel)
	{
		Cat = cat;
		TargetLevel = targetLevel;
	}
}

public readonly struct OnStatusAppliedEvent
{
	public readonly Combatable Target;
	public readonly string StatusEffectId;
	public OnStatusAppliedEvent(Combatable target, string statusEffectId)
	{
		Target = target;
		StatusEffectId = statusEffectId;
	}
}
