using System;
using System.Collections.Generic;
using UnityEngine;

public class WeightedRewardEntry
{
    public string RewardId;
    public string DisplayNameKey;
    public string DefaultDisplayName;
    public int Weight;
    public int MinProgress;
    public int MaxProgress;
    public int MinGreed;
    public int MaxGreed;
    public int MinCorruption;
    public int MaxCorruption;

    public WeightedRewardEntry(string id, string nameKey, string defaultName, int weight, int minProg = 0, int maxProg = int.MaxValue, int minG = 0, int maxG = 100, int minC = 0, int maxC = 100)
    {
        RewardId = id;
        DisplayNameKey = nameKey;
        DefaultDisplayName = defaultName;
        Weight = weight;
        MinProgress = minProg;
        MaxProgress = maxProg;
        MinGreed = minG;
        MaxGreed = maxG;
        MinCorruption = minC;
        MaxCorruption = maxC;
    }

    public bool IsEligible(int progress, int greed, int corruption)
    {
        return progress >= MinProgress && progress <= MaxProgress &&
               greed >= MinGreed && greed <= MaxGreed &&
               corruption >= MinCorruption && corruption <= MaxCorruption;
    }
}

public static class WeightedRewardPool
{
    private static List<WeightedRewardEntry> _pool = new List<WeightedRewardEntry>();

    static WeightedRewardPool()
    {
        // Initial setup for Cat God Milestones & Drops (Milestone 50, 150, 350, 700)
        
        // Milestone 50: Gold coins
        _pool.Add(new WeightedRewardEntry("resource_gold", "resource_gold_bag", "Túi Tiền Vàng của Thần Mèo", 100, 50, 149));

        // Milestone 150: Revive Pill
        _pool.Add(new WeightedRewardEntry("item_revive_pill", "item_revive_pill", "Linh Đan Hồi Sinh", 100, 150, 349));

        // Milestone 350: Breakthrough Pill
        _pool.Add(new WeightedRewardEntry("item_breakthrough_pill", "item_breakthrough_pill", "Linh Đan Đột Phá", 100, 350, 699));

        // Milestone 700: Legendary Tier
        _pool.Add(new WeightedRewardEntry("item_heavenly_relic", "item_heavenly_relic", "Chí Tôn Cổ Khí (1% Cực Hiếm)", 10, 700)); // Relic has lower weight (approx 10%)
        _pool.Add(new WeightedRewardEntry("cat_basic", "cat_basic_blessed", "Một Thần Miêu Mới (Được gia trì Thiên Kiêu)", 90, 700)); // Cat has higher weight (approx 90%)
        
        // High greed/corruption specific entries (example of scalable conditions)
        _pool.Add(new WeightedRewardEntry("mob_void_spirit", "mob_void_spirit", "Tà Linh Hư Không", 50, 0, int.MaxValue, minG: 80)); // Spawn Void Spirit if Greed is extremely high
    }

    public static void AddReward(WeightedRewardEntry entry)
    {
        if (entry != null)
        {
            _pool.Add(entry);
        }
    }

    public static WeightedRewardEntry RollReward(int progress, int greed, int corruption)
    {
        List<WeightedRewardEntry> eligible = new List<WeightedRewardEntry>();
        int totalWeight = 0;

        foreach (var entry in _pool)
        {
            if (entry.IsEligible(progress, greed, corruption))
            {
                eligible.Add(entry);
                totalWeight += entry.Weight;
            }
        }

        if (eligible.Count == 0 || totalWeight <= 0) return null;

        int roll = UnityEngine.Random.Range(0, totalWeight);
        int accumulated = 0;

        foreach (var entry in eligible)
        {
            accumulated += entry.Weight;
            if (roll < accumulated)
            {
                return entry;
            }
        }

        return eligible[eligible.Count - 1];
    }
}
