using System;
using System.Collections.Generic;
using UnityEngine;
using Mewtations.Combat;
using Mewtations.Combat.Core;

namespace Mewtations.Cards.Cats
{
    public static class BranchingMutationTree
    {
        public static readonly string AshenTalons = "mutation_ashen_talons";
        public static readonly string BladeClaws = "mutation_blade_claws";

        static BranchingMutationTree()
        {
            // Register evolved mutations in the component registry dynamic dictionary
            MewtationsComponentRegistry.Register(AshenTalons, () => new AshenTalonsComponent());
            MewtationsComponentRegistry.Register(BladeClaws, () => new BladeClawsComponent());
        }

        public static string GetEvolvedDisplayName(string id)
        {
            if (id == AshenTalons) return "Hắc Ám Trảo (Evolved)";
            if (id == BladeClaws) return "Linh Kiếm Phách (Evolved)";
            return "Đột Biến Tiến Hóa";
        }

        public static string GetEvolvedDescription(string id)
        {
            if (id == AshenTalons) return "Đòn thường sát thương tăng 50%, tự rút 5 HP bản thân, gây trạng thái Chảy Máu (Bleeding) lên mục tiêu.";
            if (id == BladeClaws) return "Tỷ lệ Crit tăng +30%, đòn đánh cơ bản làm tiêu hao 20 Nộ khí của đồng đội kề bên.";
            return "Sức mạnh đột biến bạo phát cao độ.";
        }

        public static List<string> GetEvolutionChoices(string baseMutationId)
        {
            var list = new List<string>();
            if (baseMutationId == Expedition.UnstableMutation.UnstableClaws)
            {
                list.Add(AshenTalons);
                list.Add(BladeClaws);
            }
            return list;
        }

        // ==========================================
        // EVOLVED MUTATION COMPONENT IMPLEMENTATIONS
        // ==========================================

        private class AshenTalonsComponent : IMewtationsComponent
        {
            public string Id => AshenTalons;
            public string DisplayName => "Hắc Ám Trảo";
            public string Description => "Tăng 50% sát thương đòn đánh thường, tự rút 5 HP, gây chảy máu mục tiêu.";
            public void Initialize(CombatUnit unit) {}
            public void BeforeAttack(CombatUnit attacker, CombatUnit target, ref int damage, Action<string> logCallback)
            {
                damage = Mathf.RoundToInt(damage * 1.5f);
            }
            public void AfterAttack(CombatUnit attacker, CombatUnit target, int damage, Action<string> logCallback)
            {
                if (attacker.IsAlive)
                {
                    attacker.TakeDamage(5);
                    logCallback?.Invoke($"☣️ [HẮC ÁM TRẢO] Ma trảo phản chấn! {attacker.Name} tự tiêu hao 5 HP huyết tủy.");
                }
                if (target.IsAlive)
                {
                    target.AddDebuff(MewtationsDebuff.Bleeding, 2);
                    logCallback?.Invoke($"🩸 Đòn đánh của {attacker.Name} xé rách kinh mạch, gây CHẢY MÁU lên {target.Name}!");
                }
            }
        }

        private class BladeClawsComponent : IMewtationsComponent
        {
            public string Id => BladeClaws;
            public string DisplayName => "Linh Kiếm Phách";
            public string Description => "Tăng +30% Crit, đòn đánh tiêu hao 20 Nộ của đồng đội.";
            public void Initialize(CombatUnit unit)
            {
                unit.CritChance += 30; // 30% increase
            }
            public void BeforeAttack(CombatUnit attacker, CombatUnit target, ref int damage, Action<string> logCallback)
            {
                // Crit damage triggers organically from high CritChance
            }
            public void AfterAttack(CombatUnit attacker, CombatUnit target, int damage, Action<string> logCallback)
            {
                // Absorb rage from allies: subtract rage from friendly units in combat
                if (TurnBasedCombatManager.Instance != null && TurnBasedCombatManager.Instance.IsCombatActive)
                {
                    bool drained = false;
                    foreach (var ally in TurnBasedCombatManager.Instance.Formation.PlayerUnits)
                    {
                        if (ally != null && ally.IsAlive && ally != attacker && ally.CurrentRage > 0)
                        {
                            ally.CurrentRage = Mathf.Max(0, ally.CurrentRage - 20);
                            drained = true;
                        }
                    }
                    if (drained)
                    {
                        logCallback?.Invoke($"⚔️ [LINH KIẾM PHÁCH] Cướp nộ! Đòn đánh của {attacker.Name} nuốt chửng linh lực của đồng đội (-20 Nộ)!");
                    }
                }
            }
        }
    }
}
