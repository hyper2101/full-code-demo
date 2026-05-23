using System;
using System.Collections.Generic;
using UnityEngine;
using Mewtations.Combat;

namespace Mewtations.Cards.Cats
{
    public enum CultivationStage
    {
        QiCondensation = 1,        // Luyện Khí
        FoundationEstablishment = 2, // Trúc Cơ
        GoldenCore = 3,            // Kết Đan
        NascentSoul = 4            // Nguyên Anh
    }

    public enum DaoSpecialization
    {
        None,
        SwordDao,   // Kiếm Đạo
        DemonDao,   // Ma Đạo
        SpellDao,   // Pháp Đạo
        ZenDao      // Thiền Đạo
    }

    public static class CultivationSpecializationRegistry
    {
        public static string GetStageName(CultivationStage stage)
        {
            switch (stage)
            {
                case CultivationStage.QiCondensation: return "Luyện Khí Kỳ";
                case CultivationStage.FoundationEstablishment: return "Trúc Cơ Kỳ";
                case CultivationStage.GoldenCore: return "Kết Đan Kỳ";
                case CultivationStage.NascentSoul: return "Nguyên Anh Kỳ";
                default: return "Phàm Nhân";
            }
        }

        public static string GetSpecializationName(DaoSpecialization spec)
        {
            switch (spec)
            {
                case DaoSpecialization.SwordDao: return "Kiếm Đạo Đạo Quả";
                case DaoSpecialization.DemonDao: return "Ma Đạo Đạo Quả";
                case DaoSpecialization.SpellDao: return "Pháp Đạo Đạo Quả";
                case DaoSpecialization.ZenDao: return "Thiền Đạo Đạo Quả";
                default: return "Vô Phái";
            }
        }

        public static string GetSpecializationDescription(DaoSpecialization spec)
        {
            switch (spec)
            {
                case DaoSpecialization.SwordDao:
                    return "Tăng 50% sát thương đòn đánh cơ bản. Khóa vĩnh viễn ô Linh Đan.";
                case DaoSpecialization.DemonDao:
                    return "Tăng +25% sát thương với mỗi 20 điểm Corruption toàn cầu. Khóa vĩnh viễn ô Trang Bị.";
                case DaoSpecialization.SpellDao:
                    return "Bắt đầu trận chiến với 50 Nộ khí, hồi 10 Nộ khí mỗi khi thi triển kỹ năng.";
                case DaoSpecialization.ZenDao:
                    return "Hồi 10 HP khi máu giảm xuống dưới 30% (mỗi trận 1 lần), miễn dịch trạng thái Trúng Độc.";
                default:
                    return "Chưa thức tỉnh Đạo Quả hoàn mỹ.";
            }
        }

        public static IMewtationsComponent CreateComponent(DaoSpecialization spec)
        {
            switch (spec)
            {
                case DaoSpecialization.SwordDao: return new SwordDaoComponent();
                case DaoSpecialization.DemonDao: return new DemonDaoComponent();
                case DaoSpecialization.SpellDao: return new SpellDaoComponent();
                case DaoSpecialization.ZenDao: return new ZenDaoComponent();
                default: return null;
            }
        }

        // ==========================================
        // INDIVIDUAL DAO COMPONENTS IMPLEMENTATIONS
        // ==========================================

        private class SwordDaoComponent : IMewtationsComponent
        {
            public string Id => "spec_sword_dao";
            public string DisplayName => "Kiếm Đạo Đạo Quả";
            public string Description => "Tăng 50% sát thương đòn đánh cơ bản.";
            public void Initialize(CombatUnit unit) {}
            public void BeforeAttack(CombatUnit attacker, CombatUnit target, ref int damage, Action<string> logCallback)
            {
                damage = Mathf.RoundToInt(damage * 1.5f);
                logCallback?.Invoke($"⚔️ [KIẾM ĐẠO] Kiếm ý trùng thiên! {attacker.Name} gây thêm 50% sát thương đòn đánh thường!");
            }
        }

        private class DemonDaoComponent : IMewtationsComponent
        {
            public string Id => "spec_demon_dao";
            public string DisplayName => "Ma Đạo Đạo Quả";
            public string Description => "Sát thương gia tăng theo độ ô nhiễm Corruption.";
            public void Initialize(CombatUnit unit) {}
            public void BeforeAttack(CombatUnit attacker, CombatUnit target, ref int damage, Action<string> logCallback)
            {
                if (Expedition.ExpeditionManager.Instance != null && Expedition.ExpeditionManager.Instance.IsExpeditionActive)
                {
                    int corr = Expedition.ExpeditionManager.Instance.RunState.CorruptionLevel;
                    float multiplier = 1.0f + (corr / 20f) * 0.25f;
                    damage = Mathf.RoundToInt(damage * multiplier);
                    logCallback?.Invoke($"🔴 [MA ĐẠO] Hóa ma cướp đoạt sinh cơ! Nhận {multiplier - 1.0f:P0} sát thương từ {corr}% Corruption!");
                }
            }
        }

        private class SpellDaoComponent : IMewtationsComponent
        {
            public string Id => "spec_spell_dao";
            public string DisplayName => "Pháp Đạo Đạo Quả";
            public string Description => "Bắt đầu với +50 Nộ khí, dùng kỹ năng hồi 15 Nộ khí.";
            public void Initialize(CombatUnit unit)
            {
                unit.CurrentRage = Mathf.Min(145, unit.CurrentRage + 50);
            }
            public void AfterAttack(CombatUnit attacker, CombatUnit target, int damage, Action<string> logCallback)
            {
                if (attacker.CurrentRage >= 100)
                {
                    attacker.CurrentRage = Mathf.Min(145, attacker.CurrentRage + 15);
                    logCallback?.Invoke($"⚡ [PHÁP ĐẠO] Linh khí hồi lưu! {attacker.Name} nhận lại 15 Nộ khí sau khi thi triển pháp thuật!");
                }
            }
        }

        private class ZenDaoComponent : IMewtationsComponent
        {
            public string Id => "spec_zen_dao";
            public string DisplayName => "Thiền Đạo Đạo Quả";
            public string Description => "Hồi phục linh hồn khi nguy kịch, miễn dịch kịch độc.";
            private bool _triggered = false;
            public void Initialize(CombatUnit unit) {}
            public void OnTurnStart(CombatUnit unit, Action<string> logCallback)
            {
                if (unit.IsAlive && unit.HealthPoints < (unit.ProcessedCombatStats.MaxHealth * 0.3f) && !_triggered)
                {
                    unit.Heal(15);
                    _triggered = true;
                    logCallback?.Invoke($"💚 [THIỀN ĐẠO] Bồ Đề tâm gột rửa! {unit.Name} nguy cơ độ mạng tự hồi phục 15 HP!");
                }
                // Immune to Poisoned debuff
                unit.RemoveDebuff(MewtationsDebuff.Poisoned);
            }
        }
    }
}
