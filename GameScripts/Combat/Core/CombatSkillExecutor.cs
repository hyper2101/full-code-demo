using System;
using System.Collections.Generic;
using UnityEngine;
using Mewtations.Combat;

namespace GameScripts.Combat.Core
{
    public static class CombatSkillExecutor
    {
        public static void ExecuteSkill(CombatUnit attacker, CombatSkillDefinition skill, int rageAllocated, List<CombatUnit> allies, List<CombatUnit> enemies, Action<string> logCallback)
        {
            if (attacker == null || !attacker.IsAlive || skill == null) return;
            
            float skillScale = rageAllocated / 100f;

            if (skill.Id == "full_battlefield_strike")
            {
                ExecuteFullBattlefieldStrike(attacker, skill, skillScale, enemies, logCallback);
            }
            else if (skill.Id == "execution_barrage")
            {
                ExecuteExecutionBarrage(attacker, skill, skillScale, enemies, logCallback);
            }
            else if (skill.Id == "crushing_formation")
            {
                ExecuteCrushingFormation(attacker, skill, skillScale, enemies, logCallback);
            }
            else
            {
                // Generic single target skill
                var target = CombatTargetResolver.GetPrimaryTarget(enemies, attacker);
                if (target != null)
                {
                    int rawDamage = CombatCalculationService.CalculateRawSkillDamage(attacker, 1.0f, 0f, target);
                    
                    // Implementation of Multiplicative Execute Ratio (Cap 0.25)
                    float executeRatio = Mathf.Min(target.GetMissingHpPercent() * skill.ExecuteRatio, 0.25f);
                    float finalScale = (skill.FinalDamageMultiplier * skillScale) * (1f + executeRatio);
                    
                    int finalDamage = CombatCalculationService.CalculateFinalDamage(rawDamage, attacker, target, finalScale);
                    target.TakeDamage(finalDamage);
                    logCallback?.Invoke($"⚔️ {attacker.Name} tung kỹ năng {Mewtations.Core.MewtationsLoc.Translate(skill.SkillNameKey)} gây {finalDamage} sát thương lên {target.Name}!");
                }
            }
        }

        private static void ExecuteFullBattlefieldStrike(CombatUnit attacker, CombatSkillDefinition skill, float skillScale, List<CombatUnit> enemies, Action<string> logCallback)
        {
            logCallback?.Invoke($"💥 {attacker.Name} thi triển {Mewtations.Core.MewtationsLoc.Translate(skill.SkillNameKey)} quét qua toàn bộ đội hình địch!");
            int rawDamage = CombatCalculationService.CalculateRawAttackDamage(attacker);
            foreach (var enemy in enemies)
            {
                if (enemy.IsAlive)
                {
                    float executeRatio = Mathf.Min(enemy.GetMissingHpPercent() * skill.ExecuteRatio, 0.25f);
                    float finalScale = (skill.FinalDamageMultiplier * skillScale) * (1f + executeRatio);
                    int finalDamage = CombatCalculationService.CalculateFinalDamage(rawDamage, attacker, enemy, finalScale);
                    enemy.TakeDamage(finalDamage);
                    logCallback?.Invoke($" -> {enemy.Name} nhận {finalDamage} sát thương.");
                }
            }
        }

        private static void ExecuteExecutionBarrage(CombatUnit attacker, CombatSkillDefinition skill, float skillScale, List<CombatUnit> enemies, Action<string> logCallback)
        {
            logCallback?.Invoke($"🔪 {attacker.Name} thi triển {Mewtations.Core.MewtationsLoc.Translate(skill.SkillNameKey)} với {skill.HitCount} chém liên hoàn!");
            CombatUnit currentTarget = CombatTargetResolver.GetPrimaryTarget(enemies, attacker);
            
            for (int i = 0; i < skill.HitCount; i++)
            {
                if (currentTarget == null || !currentTarget.IsAlive)
                {
                    if (skill.MultiHitPolicy == MultiHitPolicy.RetargetOnDeath)
                    {
                        currentTarget = CombatTargetResolver.GetPrimaryTarget(enemies, attacker);
                        if (currentTarget == null) break; // No more enemies
                        logCallback?.Invoke($" -> Chuyển mục tiêu sang {currentTarget.Name}!");
                    }
                    else
                    {
                        logCallback?.Invoke($" -> Chuỗi chém bị đứt đoạn vì mục tiêu đã gục ngã.");
                        break;
                    }
                }

                float executeRatio = Mathf.Min(currentTarget.GetMissingHpPercent() * skill.ExecuteRatio, 0.25f);
                float finalScale = (skill.FinalDamageMultiplier * skillScale) * (1f + executeRatio);
                
                int rawDamage = CombatCalculationService.CalculateRawSkillDamage(attacker, skill.RawAtkMultiplier, skill.RawMissingHpMultiplier, currentTarget);
                int finalDamage = CombatCalculationService.CalculateFinalDamage(rawDamage, attacker, currentTarget, finalScale);
                currentTarget.TakeDamage(finalDamage);
                logCallback?.Invoke($" -> Chém trúng {currentTarget.Name} gây {finalDamage} sát thương.");
            }
        }

        private static void ExecuteCrushingFormation(CombatUnit attacker, CombatSkillDefinition skill, float skillScale, List<CombatUnit> enemies, Action<string> logCallback)
        {
            logCallback?.Invoke($"☄️ {attacker.Name} thi triển {Mewtations.Core.MewtationsLoc.Translate(skill.SkillNameKey)} nghiền nát hàng ngang!");
            
            // Just picking primary and secondary target simulating Row logic for now
            var targets = new List<CombatUnit>();
            var pTarget = CombatTargetResolver.GetPrimaryTarget(enemies, attacker);
            if (pTarget != null)
            {
                targets.Add(pTarget);
                foreach (var enemy in enemies)
                {
                    if (enemy != pTarget && enemy.IsAlive && enemy.SlotIndex / 3 == pTarget.SlotIndex / 3) // same row
                    {
                        targets.Add(enemy);
                    }
                }
            }

            foreach (var target in targets)
            {
                float executeRatio = Mathf.Min(target.GetMissingHpPercent() * skill.ExecuteRatio, 0.25f);
                float finalScale = (skill.FinalDamageMultiplier * skillScale) * (1f + executeRatio);
                
                int rawDamage = CombatCalculationService.CalculateRawAttackDamage(attacker);
                int finalDamage = CombatCalculationService.CalculateFinalDamage(rawDamage, attacker, target, finalScale);
                target.TakeDamage(finalDamage);
                logCallback?.Invoke($" -> {target.Name} nhận {finalDamage} sát thương.");

                if (target.IsAlive && skill.RageReduction > 0)
                {
                    target.RemoveRage(skill.RageReduction);
                    logCallback?.Invoke($" -> {target.Name} bị áp đảo nhuệ khí, giảm {skill.RageReduction} Nộ!");
                }
            }
        }
    }
}
