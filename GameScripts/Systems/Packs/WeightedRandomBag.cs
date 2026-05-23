using System;
using System.Collections.Generic;
using UnityEngine;

public class WeightedRandomBag<T>
{
	public int Count
	{
		get
		{
			return this.entries.Count;
		}
	}

	public void AddEntry(T item, float weight)
	{
		this.totalWeight += weight;
		this.entries.Add(new WeightedRandomBag<T>.Entry
		{
			item = item,
			accumulatedWeight = this.totalWeight,
			weight = weight
		});
	}

	public T Choose()
	{
		if (this.entries.Count == 0)
		{
			Debug.LogWarning("[WeightedRandomBag] Attempted to Choose() from an empty bag! Returning default(T).");
			return default(T);
		}
		if (this.totalWeight <= 0f)
		{
			Debug.LogWarning($"[WeightedRandomBag] Total weight <= 0 ({this.totalWeight}) with {this.entries.Count} entries! Falling back to first entry.");
			this.lastPickedEntry = this.entries[0];
			return this.entries[0].item;
		}
		float num = Random.value * this.totalWeight;
		foreach (WeightedRandomBag<T>.Entry entry in this.entries)
		{
			if (entry.accumulatedWeight >= num)
			{
				this.lastPickedEntry = entry;
				return entry.item;
			}
		}
		return default(T);
	}

	public WeightedRandomBag<T>.Entry GetLastPickedEntry()
	{
		return this.lastPickedEntry;
	}

	public void Clear()
	{
		this.entries.Clear();
		this.totalWeight = 0f;
	}

	private List<WeightedRandomBag<T>.Entry> entries = new List<WeightedRandomBag<T>.Entry>();

	private float totalWeight;

	private WeightedRandomBag<T>.Entry lastPickedEntry;

	public struct Entry
	{
		public float accumulatedWeight;

		public float weight;

		public T item;
	}
}
