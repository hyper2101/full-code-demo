using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mewtations.Expedition
{
    public enum RouteTheme
    {
        Standard,   // Cá»• Äáº¡o (Standard route)
        TaDao,      // TÃ  Äáº¡o (Moral temptations, sacrifice teammate, +10 Corruption)
        ThienLoi,   // ThiÃªn LÃ´i (Kiáº¿p lÃ´i hazard in combat, but +Breakthrough potential)
        ThamLam,    // Tham Lam (Gold doubled, +10 Greed)
        ThuTrieu,    // ThA Tri?u (Beast swarm, harder combat, rich loot)
        KhuRungSieuNhien, // Special Map
        LangVangLai,      // Special Map
        BaiGiacGiaTu      // Special Map
    }

    public enum MemoirType
    {
        Birth,            // Khá»Ÿi Ä‘áº§u xuáº¥t thÃ¢n
        Breakthrough,     // Äá»™t phÃ¡ tu vi
        Equip,            // Trang bá»‹ tháº§n binh
        Unequip,          // ThÃ¡o trang bá»‹
        BossKill,         // Tráº£m sÃ¡t thá»§ lÄ©nh
        Mutation,         // TÃ­ch tá»¥ dá»‹ biáº¿n linh khÃ­
        Resurrection,     // Trá»ng sinh dÃ²ng dÃµi
        Death,            // Tá»­ tráº­n oanh liá»‡t
        AppeasementOffer  // Hiáº¿n táº¿ xoa dá»‹u
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
                    return $"[NgÃ y {Timestamp}] Khá»Ÿi Ä‘áº§u: {ParamA}";
                case MemoirType.Breakthrough:
                    return $"[NgÃ y {Timestamp}] Äá»™t phÃ¡: Äáº¡t cáº£nh giá»›i {ParamA}";
                case MemoirType.Equip:
                    return $"[NgÃ y {Timestamp}] Gia trÃ¬: Dung há»£p linh báº£o {ParamA}";
                case MemoirType.Unequip:
                    return $"[NgÃ y {Timestamp}] ThÃ¡o gá»¡: Rá»i bá» linh báº£o {ParamA}";
                case MemoirType.BossKill:
                    return $"[NgÃ y {Timestamp}] Tráº£m sÃ¡t: TiÃªu diá»‡t thá»§ lÄ©nh {ParamA}";
                case MemoirType.Mutation:
                    return $"[NgÃ y {Timestamp}] Dá»‹ biáº¿n: Linh khÃ­ báº¡o tÃ n gÃ¢y Ä‘á»™t biáº¿n {ParamA} ({ParamB})";
                case MemoirType.Resurrection:
                    return $"[NgÃ y {Timestamp}] Kiáº¿p má»›i: Phá»¥c sinh dÃ²ng dÃµi Ä‘á»i thá»© {ParamA}";
                case MemoirType.Death:
                    return $"[NgÃ y {Timestamp}] Tá»­ tráº­n: {ParamA}";
                case MemoirType.AppeasementOffer:
                    return $"[NgÃ y {Timestamp}] Táº¿ lá»…: DÃ¢ng hiáº¿n lá»… váº­t xoa dá»‹u thiÃªn Ä‘á»‹a ({ParamA})";
                default:
                    return $"[NgÃ y {Timestamp}] Äiá»ƒn tÃ­ch: {ParamA}";
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

            Debug.Log($"[RiskSystem] Khá»Ÿi cháº¡y viá»…n chinh. Greed ban Ä‘áº§u: {state.GreedLevel}%, Corruption ban Ä‘áº§u: {state.CorruptionLevel}%.");
        }

        public static int CalculateDailyCorpseCorruptionMultiplier(int corpseCount)
        {
            // Each cat corpse on board raises initial corruption of the next run by +5
            return corpseCount * 5;
        }
    }

    public static class ExpeditionExtractionSystem
    {
        public static float CalculateLootRetentionRate(ExpeditionRunState state, Mewtations.Legacy.Stacklands.InventoryContainer container, int maxCapacity)
        {
            float rate = 0.60f;
            float greedMod = (state.GreedLevel / 100f) * 0.20f;
            rate -= greedMod;
            float corruptionMod = (state.CorruptionLevel / 100f) * 0.25f;
            rate -= corruptionMod;

            if (maxCapacity > 0 && container != null)
            {
                float weightRatio = (float)container.GetChildren().Count / maxCapacity;
                rate -= weightRatio * 0.15f;
            }
            rate -= state.CurrentLayer * 0.03f;
            return Mathf.Clamp(rate, 0.10f, 0.90f);
        }

        public static void ApplyAbandonPenalty(Mewtations.Legacy.Stacklands.InventoryContainer container, float retentionRate, int insuredSlots = 0)
        {
            if (container == null) return;
            var items = container.GetChildren();
            int originalCount = items.Count;
            if (originalCount == 0) return;

            int vulnerableCount = Mathf.Max(0, originalCount - insuredSlots);
            int keepVulnerableCount = Mathf.Clamp(Mathf.RoundToInt(vulnerableCount * retentionRate), 0, vulnerableCount);

            List<Mewtations.Legacy.Stacklands.GameCard> vulnerableItems = new List<Mewtations.Legacy.Stacklands.GameCard>();
            for (int i = 0; i < items.Count; i++) {
                if (i >= insuredSlots) {
                    vulnerableItems.Add(items[i]);
                }
            }

            for (int i = 0; i < vulnerableItems.Count; i++) {
                var temp = vulnerableItems[i];
                int randomIndex = UnityEngine.Random.Range(i, vulnerableItems.Count);
                vulnerableItems[i] = vulnerableItems[randomIndex];
                vulnerableItems[randomIndex] = temp;
            }

            for (int i = keepVulnerableCount; i < vulnerableItems.Count; i++) {
                vulnerableItems[i].DestroyCard(true, true);
            }
        }

        public static void ApplyManualRetreatPenalty(Mewtations.Legacy.Stacklands.InventoryContainer container, int insuredSlots = 0)
        {
            ApplyAbandonPenalty(container, 0.5f, insuredSlots);
        }
    }
    public static class MutationPersistenceSystem
    {
        public static void ProcessRunVictoryTraits(List<CatCardData> cats)
        {
            foreach (var cat in cats)
            {
                if (cat == null) continue;

                // Song Trá»ng Dá»‹ Biáº¿n: limit of max 2 permanent traits
                int permCount = cat.PermanentTraits.Count;
                if (permCount >= 2)
                {
                    Debug.Log($"[Mutation] {cat.Name} Ä‘Ã£ Ä‘áº¡t cá»±c háº¡n Song Trá»ng Dá»‹ Biáº¿n (2). KhÃ´ng thá»ƒ tÃ­ch lÅ©y thÃªm.");
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
                        cat.AddMemoir(MemoirType.Mutation, UnstableMutation.GetDisplayName(mut), "TÃ­ch há»£p dá»‹ biáº¿n vÄ©nh háº±ng (Song Trá»ng Dá»‹ Biáº¿n)");
                        permCount++;
                        Debug.Log($"[Mutation] Äá»™t biáº¿n {mut} cá»§a {cat.Name} Ä‘Ã£ dung há»£p vÄ©nh viá»…n!");
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


