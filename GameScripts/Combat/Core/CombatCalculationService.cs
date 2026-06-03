using System;
using UnityEngine;
using Mewtations.Combat;

namespace GameScripts.Combat.Core
{
    public static class CombatCalculationService
    {
        public static int CalculateRawAttackDamage(CombatUnit attacker)
        {
            float raw = attacker.ATK;
            // Nếu có efficiency từ Weapon, ta sẽ xử lý riêng (vì tương lai Dog không có Weapon)
            return Mathf.RoundToInt(raw);
        }

        public static int CalculateRawSkillDamage(CombatUnit attacker, float atkMultiplier, float missingHpMultiplier, CombatUnit target = null)
        {
            float raw = attacker.ATK * atkMultiplier;
            if (target != null && missingHpMultiplier > 0)
            {
                int missingHp = target.MaxHP - target.HP;
                raw += missingHp * missingHpMultiplier;
            }
            return Mathf.RoundToInt(raw);
        }

        public static int CalculateFinalDamage(int rawDamage, CombatUnit attacker, CombatUnit target, float finalDamageMultiplier = 1.0f)
        {
            // 1. Role Modifier (DPS vs Attrition)
            float roleMultiplier = 1.0f;
            if (attacker.Role == CatRole.DPS)
            {
                roleMultiplier += 0.20f;
                bool hasDebuff = target.ActiveDebuffs.Exists(d => d.Duration > 0);
                if (hasDebuff) roleMultiplier += 0.25f;
            }
            else if (attacker.Role == CatRole.Attrition)
            {
                int currentRound = (TurnBasedCombatManager.Instance != null) ? TurnBasedCombatManager.Instance.CurrentRound : 1;
                roleMultiplier += currentRound * 0.10f;
            }

            // 2. Element / Constitution / Event Pipeline có thể gắn vào đây
            // Ở đây tạm gom các hiệu ứng cường hóa (Spiritual Backlash, TaMaLaoTo, KhoHanhTang) - sẽ refactor sau
            
            float damage = rawDamage * roleMultiplier;

            // 3. Defense Reduction (Công thức chuẩn)
            float def = target.DEF;
            float clampedDef = Mathf.Clamp(def, -20f, 95f);
            float resistance = clampedDef / 100f;
            
            damage = damage * (1f - resistance);

            // 4. Final Damage Multiplier (Skill 1, Crushing Formation, v.v.)
            damage *= finalDamageMultiplier;

            return Mathf.Max(1, Mathf.RoundToInt(damage)); // At least 1 damage if hit
        }
    }
}
