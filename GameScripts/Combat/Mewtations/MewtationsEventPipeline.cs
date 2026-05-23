using System;
using System.Collections.Generic;

namespace Mewtations.Combat
{
    public static class MewtationsEventPipeline
    {
        // Active components registered on CombatUnits
        private static Dictionary<CombatUnit, List<IMewtationsComponent>> _unitComponents = new Dictionary<CombatUnit, List<IMewtationsComponent>>();

        public static void Clear()
        {
            _unitComponents.Clear();
        }

        public static void RegisterUnitComponents(CombatUnit unit, List<IMewtationsComponent> components)
        {
            if (unit == null) return;
            _unitComponents[unit] = components ?? new List<IMewtationsComponent>();
            foreach (var comp in _unitComponents[unit])
            {
                comp.Initialize(unit);
            }
        }

        public static List<IMewtationsComponent> GetComponents(CombatUnit unit)
        {
            if (unit != null && _unitComponents.TryGetValue(unit, out var list))
            {
                return list;
            }
            return new List<IMewtationsComponent>();
        }

        public static void TriggerOnTurnStart(CombatUnit unit, Action<string> logCallback)
        {
            if (unit == null) return;
            var comps = GetComponents(unit);
            for (int i = 0; i < comps.Count; i++)
            {
                comps[i].OnTurnStart(unit, logCallback);
            }
        }

        public static void TriggerOnTurnEnd(CombatUnit unit, Action<string> logCallback)
        {
            if (unit == null) return;
            var comps = GetComponents(unit);
            for (int i = 0; i < comps.Count; i++)
            {
                comps[i].OnTurnEnd(unit, logCallback);
            }
        }

        public static void TriggerBeforeAttack(CombatUnit attacker, CombatUnit target, ref int damage, Action<string> logCallback)
        {
            if (attacker == null) return;
            var comps = GetComponents(attacker);
            for (int i = 0; i < comps.Count; i++)
            {
                comps[i].BeforeAttack(attacker, target, ref damage, logCallback);
            }
        }

        public static void TriggerAfterAttack(CombatUnit attacker, CombatUnit target, int damage, Action<string> logCallback)
        {
            if (attacker == null) return;
            var comps = GetComponents(attacker);
            for (int i = 0; i < comps.Count; i++)
            {
                comps[i].AfterAttack(attacker, target, damage, logCallback);
            }
        }

        public static void TriggerBeforeDamage(CombatUnit victim, CombatUnit attacker, ref int damage, Action<string> logCallback)
        {
            if (victim == null) return;
            var comps = GetComponents(victim);
            for (int i = 0; i < comps.Count; i++)
            {
                comps[i].BeforeDamage(victim, attacker, ref damage, logCallback);
            }
        }

        public static void TriggerAfterDamage(CombatUnit victim, CombatUnit attacker, int damage, Action<string> logCallback)
        {
            if (victim == null) return;
            var comps = GetComponents(victim);
            for (int i = 0; i < comps.Count; i++)
            {
                comps[i].AfterDamage(victim, attacker, damage, logCallback);
            }
        }

        public static void TriggerOnKill(CombatUnit killer, CombatUnit victim, Action<string> logCallback)
        {
            if (killer == null) return;
            var comps = GetComponents(killer);
            for (int i = 0; i < comps.Count; i++)
            {
                comps[i].OnKill(killer, victim, logCallback);
            }
        }

        public static void TriggerOnDeath(CombatUnit unit, Action<string> logCallback)
        {
            if (unit == null) return;
            var comps = GetComponents(unit);
            for (int i = 0; i < comps.Count; i++)
            {
                comps[i].OnDeath(unit, logCallback);
            }
        }
    }
}
