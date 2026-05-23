using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mewtations.Expedition;

namespace Mewtations.Combat
{
    public static class MewtationsPressureSystem
    {
        // 1. Food Upkeep: Requires 1 food card on the board per active cat in the expedition
        public static bool ValidateAndConsumeFoodUpkeep(List<CatCardData> cats, out string consumeReport)
        {
            consumeReport = "";
            int requiredFood = cats.Count;
            if (requiredFood <= 0) return true;

            // Find all available food cards on the main board
            var foodCards = new List<GameCard>();
            foreach (var gc in WorldManager.instance.AllCards)
            {
                if (gc != null && !gc.Destroyed && gc.CardData != null)
                {
                    if (gc.CardData.MyCardType == CardType.Food || gc.CardData.Id == "resource_food")
                    {
                        foodCards.Add(gc);
                    }
                }
            }

            if (foodCards.Count < requiredFood)
            {
                consumeReport = $"Thiếu hụt thực phẩm! Yêu cầu {requiredFood} Linh Thực nhưng chỉ có {foodCards.Count} trên bàn chơi.";
                return false;
            }

            // Consume required food
            int consumed = 0;
            for (int i = foodCards.Count - 1; i >= 0 && consumed < requiredFood; i--)
            {
                if (foodCards[i] != null && !foodCards[i].Destroyed)
                {
                    foodCards[i].DestroyCard(true, true);
                    consumed++;
                }
            }

            consumeReport = $"Đã tiêu hao thành công {consumed} Linh Thực làm lương thảo viễn chinh.";
            return true;
        }

        // 2. Board Spoilage / Resource Decay & Sect Upkeep: Processed at end-of-month
        public static void ProcessSectUpkeepAndDecay()
        {
            // Disabled per user request.
        }

        // 3. Danger Level Environmental Modifiers: Inflicted at the start of combat based on Biome
        public static void ApplyEnvironmentalModifiers(ExpeditionBiome biome, List<CombatUnit> playerUnits, Action<string> logCallback)
        {
            if (biome == ExpeditionBiome.Swamp)
            {
                logCallback?.Invoke("☣️ [ĐẦM LẦY ĐỘC LỰC] Khói độc mịt mù! Toàn bộ Thần Miêu gánh chịu trạng thái TRÚNG ĐỘC (2 lượt) ngay từ khi lâm trận.");
                foreach (var unit in playerUnits)
                {
                    unit.AddDebuff(MewtationsDebuff.Poisoned, 2);
                }
            }
            else if (biome == ExpeditionBiome.Peak)
            {
                logCallback?.Invoke("⚡ [ĐỈNH LÔI KIẾP] Lôi điện nổ giòn dã! Toàn bộ Thần Miêu nhận ngay 10 NỘ KHÍ khởi đầu nhưng giảm 5 Giáp.");
                foreach (var unit in playerUnits)
                {
                    unit.CurrentRage = Mathf.Min(145, unit.CurrentRage + 10);
                    unit.Shield = Mathf.Max(0, unit.Shield - 5);
                }
            }
            else if (biome == ExpeditionBiome.Abyss)
            {
                logCallback?.Invoke("🌫️ [SƯƠNG MÙ MA CHƯỚNG] Ma khí cắn nuốt linh thức! Tốc độ hành động (Speed) của toàn đội giảm 15.");
                foreach (var unit in playerUnits)
                {
                    unit.Speed = Mathf.Max(10, unit.Speed - 15);
                }
            }
        }

        public static bool IsDemonic(CatCardData cat)
        {
            if (cat.Constitution == CatConstitution.TaMaLaoTo || cat.Constitution == CatConstitution.HonLoanTrieu) return true;
            if (cat.HasScar(PermanentScar.SoulScar)) return true;
            if (cat.HasMutation(UnstableMutation.CursedFur) || cat.HasMutation(UnstableMutation.UnstableClaws)) return true;
            if (cat.Specialization == Cards.Cats.DaoSpecialization.DemonDao) return true;
            return false;
        }

        public static bool IsOrthodox(CatCardData cat)
        {
            if (cat.Constitution == CatConstitution.BaoLinhThienKieu || cat.Constitution == CatConstitution.KhoHanhTang) return true;
            if (cat.HasTrait(HeavenlyTalent.DivineShieldProtection)) return true;
            if (cat.Specialization == Cards.Cats.DaoSpecialization.SwordDao || cat.Specialization == Cards.Cats.DaoSpecialization.SpellDao || cat.Specialization == Cards.Cats.DaoSpecialization.ZenDao) return true;
            return false;
        }

        public static void CheckIdeologicalConflict(CatCardData catA, CatCardData catB)
        {
            // Disabled per user request
        }
    }
}
