using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mewtations.Expedition
{
    public enum RouteTheme
    {
        Standard,   // Cổ Đạo (Standard route)
        TaDao,      // Tà Đạo (Moral temptations, sacrifice teammate, +10 Corruption)
        ThienLoi,   // Thiên Lôi (Kiếp lôi hazard in combat, but +Breakthrough potential)
        ThamLam,    // Tham Lam (Gold doubled, +10 Greed)
        ThuTrieu    // Thú Triều (Beast swarm, harder combat, rich loot)
    }

    public enum MemoirType
    {
        Birth,            // Khởi đầu xuất thân
        Breakthrough,     // Đột phá tu vi
        Equip,            // Trang bị thần binh
        Unequip,          // Tháo trang bị
        BossKill,         // Trảm sát thủ lĩnh
        Mutation,         // Tích tụ dị biến linh khí
        Resurrection,     // Trọng sinh dòng dõi
        Death,            // Tử trận oanh liệt
        AppeasementOffer  // Hiến tế xoa dịu
    }

    [Serializable]
    public class MemoirEntry
    {
        public MemoirType Type;
        public string ParamA;
        public string ParamB;
        public long Timestamp; // Store month/day index

        public MemoirEntry(MemoirType type, string paramA = "", string paramB = "", long timestamp = 1)
        {
            Type = type;
            ParamA = paramA;
            ParamB = paramB;
            Timestamp = timestamp;
        }

        public static MemoirEntry Parse(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return null;
            string[] parts = raw.Split('|');
            if (parts.Length < 1) return null;

            MemoirType type = MemoirType.Birth;
            Enum.TryParse(parts[0], out type);

            string paramA = parts.Length > 1 ? parts[1] : "";
            string paramB = parts.Length > 2 ? parts[2] : "";

            long timestamp = 1;
            if (parts.Length > 3)
            {
                long.TryParse(parts[3], out timestamp);
            }

            return new MemoirEntry(type, paramA, paramB, timestamp);
        }

        public override string ToString()
        {
            return $"{(int)Type}|{ParamA}|{ParamB}|{Timestamp}";
        }

        public string ToLocalizedText()
        {
            switch (Type)
            {
                case MemoirType.Birth:
                    return $"[Ngày {Timestamp}] Khởi đầu: {ParamA}";
                case MemoirType.Breakthrough:
                    return $"[Ngày {Timestamp}] Đột phá: Đạt cảnh giới {ParamA}";
                case MemoirType.Equip:
                    return $"[Ngày {Timestamp}] Gia trì: Dung hợp linh bảo {ParamA}";
                case MemoirType.Unequip:
                    return $"[Ngày {Timestamp}] Tháo gỡ: Rời bỏ linh bảo {ParamA}";
                case MemoirType.BossKill:
                    return $"[Ngày {Timestamp}] Trảm sát: Tiêu diệt thủ lĩnh {ParamA}";
                case MemoirType.Mutation:
                    return $"[Ngày {Timestamp}] Dị biến: Linh khí bạo tàn gây đột biến {ParamA} ({ParamB})";
                case MemoirType.Resurrection:
                    return $"[Ngày {Timestamp}] Kiếp mới: Phục sinh dòng dõi đời thứ {ParamA}";
                case MemoirType.Death:
                    return $"[Ngày {Timestamp}] Tử trận: {ParamA}";
                case MemoirType.AppeasementOffer:
                    return $"[Ngày {Timestamp}] Tế lễ: Dâng hiến lễ vật xoa dịu thiên địa ({ParamA})";
                default:
                    return $"[Ngày {Timestamp}] Điển tích: {ParamA}";
            }
        }
    }

    public static class ExpeditionRiskSystem
    {
        public static void InitializeRunStats(ExpeditionRunState state)
        {
            // Apply non-static Base Appeasement values from state instance
            state.GreedLevel = Mathf.Clamp(state.GreedLevel - state.BaseAppeasementGreed, 0, 100);
            state.CorruptionLevel = Mathf.Clamp(state.CorruptionLevel - state.BaseAppeasementCorruption, 0, 100);

            // Reset base appeasements after scaling down
            state.BaseAppeasementGreed = 0;
            state.BaseAppeasementCorruption = 0;

            Debug.Log($"[RiskSystem] Khởi chạy viễn chinh. Greed ban đầu: {state.GreedLevel}%, Corruption ban đầu: {state.CorruptionLevel}%.");
        }

        public static int CalculateDailyCorpseCorruptionMultiplier(int corpseCount)
        {
            // Each cat corpse on board raises initial corruption of the next run by +5
            return corpseCount * 5;
        }
    }

    public static class ExpeditionExtractionSystem
    {
        public static float CalculateLootRetentionRate(ExpeditionRunState state, Backpack backpack)
        {
            // Base retention is 60% (base loss penalty is 40%)
            float rate = 0.60f;

            // Greed reduces retention rate (up to -20% at 100 Greed)
            float greedMod = (state.GreedLevel / 100f) * 0.20f;
            rate -= greedMod;

            // Corruption reduces retention rate (up to -25% at 100 Corruption)
            float corruptionMod = (state.CorruptionLevel / 100f) * 0.25f;
            rate -= corruptionMod;

            // Backpack carrying load reduces retention (up to -15% if full)
            if (backpack.MaxCapacity > 0)
            {
                float weightRatio = (float)backpack.ContainedCardIds.Count / backpack.MaxCapacity;
                rate -= weightRatio * 0.15f;
            }

            // Layer depth reduces escape probability (each layer deeper decreases retention by 3%)
            rate -= state.CurrentLayer * 0.03f;

            // Clamp retention between 10% (severe loss) and 90% (mild loss)
            return Mathf.Clamp(rate, 0.10f, 0.90f);
        }

        public static void ApplyAbandonPenalty(Backpack backpack, float retentionRate)
        {
            int originalCount = backpack.ContainedCardIds.Count;
            if (originalCount == 0) return;

            int keepCount = Mathf.Clamp(Mathf.RoundToInt(originalCount * retentionRate), 1, originalCount);
            
            // Randomly shuffle item indices to drop items
            List<string> items = new List<string>(backpack.ContainedCardIds);
            
            // Shuffle
            System.Random rnd = new System.Random();
            items = items.OrderBy(x => rnd.Next()).ToList();

            backpack.Clear();
            for (int i = 0; i < keepCount; i++)
            {
                backpack.AddItem(items[i]);
            }

            Debug.Log($"[Extraction] Áp dụng trừng phạt: Chỉ giữ lại {keepCount}/{originalCount} vật phẩm (Tỉ lệ giữ: {retentionRate:P0}).");
        }
    }

    public static class MutationPersistenceSystem
    {
        public static void ProcessRunVictoryTraits(List<CatCardData> cats)
        {
            foreach (var cat in cats)
            {
                if (cat == null) continue;

                // Song Trọng Dị Biến: limit of max 2 permanent traits
                int permCount = cat.PermanentTraits.Count;
                if (permCount >= 2)
                {
                    Debug.Log($"[Mutation] {cat.Name} đã đạt cực hạn Song Trọng Dị Biến (2). Không thể tích lũy thêm.");
                    continue;
                }

                List<string> mutations = new List<string>(cat.ActiveMutations);
                foreach (string mut in mutations)
                {
                    if (permCount >= 2) break;

                    // 30% breakthrough chance to integrate mutation permanently
                    if (UnityEngine.Random.value <= 0.30f)
                    {
                        cat.AddTrait(mut);
                        cat.AddMemoir(MemoirType.Mutation, UnstableMutation.GetDisplayName(mut), "Tích hợp dị biến vĩnh hằng (Song Trọng Dị Biến)");
                        permCount++;
                        Debug.Log($"[Mutation] Đột biến {mut} của {cat.Name} đã dung hợp vĩnh viễn!");
                    }
                }
            }
        }
    }

    public static class ExpeditionRewardSystem
    {
        public static void SpawnBackpackLoot(Backpack backpack, Vector3 spawnPos)
        {
            if (backpack == null) return;

            foreach (string lootId in backpack.ContainedCardIds)
            {
                Vector3 jitterPos = spawnPos + new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, UnityEngine.Random.Range(-0.5f, 0.5f));
                WorldManager.instance.CreateCard(jitterPos, lootId, true, true, true);
            }
            backpack.Clear();
        }
    }
}
