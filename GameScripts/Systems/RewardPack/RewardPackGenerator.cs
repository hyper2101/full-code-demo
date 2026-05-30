using System.Collections.Generic;
using UnityEngine;

public static class RewardPackGenerator
{
    public static List<string> GeneratePackCards(RewardPackData data)
    {
        List<string> result = new List<string>();

        // 1. Thêm các thẻ Guaranteed (bắt buộc)
        if (data.GuaranteedEntries != null)
        {
            foreach (var guaranteed in data.GuaranteedEntries)
            {
                for (int i = 0; i < guaranteed.Count; i++)
                {
                    result.Add(guaranteed.CardId);
                }
            }
        }

        // 2. Roll thêm các thẻ ngẫu nhiên
        if (data.GenerateOnSpawn && data.Entries != null && data.Entries.Count > 0)
        {
            int targetCount = Random.Range(data.MinCards, data.MaxCards + 1);
            int randomToSpawn = Mathf.Max(0, targetCount - result.Count);

            Dictionary<string, int> spawnCounts = new Dictionary<string, int>();
            foreach (var card in result)
            {
                spawnCounts[card] = spawnCounts.TryGetValue(card, out int c) ? c + 1 : 1;
            }

            for (int i = 0; i < randomToSpawn; i++)
            {
                string rolledCard = RollCard(data.Entries, spawnCounts);
                if (!string.IsNullOrEmpty(rolledCard))
                {
                    result.Add(rolledCard);
                    spawnCounts[rolledCard] = spawnCounts.TryGetValue(rolledCard, out int c) ? c + 1 : 1;
                }
            }
        }

        // 3. Shuffle kết quả
        Shuffle(result);

        return result;
    }

    private static string RollCard(List<RewardPackEntry> entries, Dictionary<string, int> currentCounts)
    {
        List<RewardPackEntry> activePool = new List<RewardPackEntry>();
        int totalWeight = 0;

        foreach (var entry in entries)
        {
            currentCounts.TryGetValue(entry.CardId, out int count);
            if (count < entry.MaxCopiesPerPack)
            {
                activePool.Add(entry);
                totalWeight += entry.Weight;
            }
        }

        if (totalWeight <= 0) return null;

        int roll = Random.Range(0, totalWeight);
        int currentSum = 0;
        foreach (var entry in activePool)
        {
            currentSum += entry.Weight;
            if (roll < currentSum)
            {
                return entry.CardId;
            }
        }

        return null;
    }

    private static void Shuffle(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            string tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }
}
