using System;
using System.Collections.Generic;

namespace Mewtations.Combat
{
    /// <summary>
    /// =========================================================================
    /// STRICT DESIGN LOCK: TARGET RESOLUTION & REDIRECTION AI
    /// =========================================================================
    /// Encapsulates targeting decision flows, redirection (taunt/tanking), and 
    /// AOE/multi-target weapon patterns.
    /// 
    /// BEHAVIORAL PARITY REQUIREMENT:
    /// All targeting decisions must strictly respect nearest layer priority first,
    /// then leftmost lane within that layer.
    /// =========================================================================
    /// </summary>
    public static class CombatTargetResolver
    {
        /// <summary>
        /// Selects the primary target based on distance priority (layer order) 
        /// and leftmost lane (left-to-right) priority within that layer.
        /// </summary>
        public static CombatUnit GetPrimaryTarget(List<CombatUnit> enemies, CombatUnit attacker)
        {
            int attackerLayer = 0;
            if (attacker != null)
            {
                attackerLayer = CombatBattlefieldHelper.GetLayer(attacker.SlotIndex);
            }

            var priorityLayers = CombatBattlefieldHelper.GetNearestLayerOrder(attackerLayer);
            foreach (int layer in priorityLayers)
            {
                var aliveInLayer = CombatBattlefieldHelper.GetAliveUnitsInLayer(enemies, layer);
                if (aliveInLayer.Count > 0)
                {
                    return GetLeftmostUnit(aliveInLayer);
                }
            }

            var allAlive = enemies.FindAll(e => e.IsAlive);
            return GetLeftmostUnit(allAlive);
        }

        /// <summary>
        /// Handles tanking/redirection mechanics. 
        /// Opponent tanks in Layer 0 (Frontline) have a fixed 30% chance to redirect 
        /// basic attacks targeted at mid/backline slots, gaining +5 Shield in the process.
        /// </summary>
        public static CombatUnit ResolveRedirectedTarget(CombatUnit attacker, CombatUnit target, List<CombatUnit> opponents, Action<string> logCallback)
        {
            if (target == null || opponents == null)
                return target;

            // Tanker redirection check (indices >= 3 indicates backline / midline)
            if (target.SlotIndex >= 3)
            {
                var defenderTanks = opponents.FindAll(u => u.IsAlive && u.Role == CatRole.Tank && CombatBattlefieldHelper.GetLayer(u.SlotIndex) == 0);
                if (defenderTanks.Count > 0 && UnityEngine.Random.value <= 0.30f)
                {
                    var tank = defenderTanks[UnityEngine.Random.Range(0, defenderTanks.Count)];
                    logCallback?.Invoke($"🛡️ [ĐỠ ĐÒN HỘ] Tanker {tank.Name} vung khiên đỡ đòn hộ cho {target.Name}! (+5 Khiên)");
                    tank.AddShield(5);
                    return tank;
                }
            }
            return target;
        }

        /// <summary>
        /// Expands the primary target into multiple targets based on weapon patterns.
        /// Utilizes slotIndex tracking internally to prevent any target duplicate anomalies even during entity cloning.
        /// </summary>
        public static List<CombatUnit> ResolvePatternTargets(WeaponAttackPattern pattern, CombatUnit primaryTarget, List<CombatUnit> opponents)
        {
            var uniqueTargets = new List<CombatUnit>();
            var visitedSlots = new HashSet<int>();
            if (primaryTarget == null || !primaryTarget.IsAlive) return uniqueTargets;

            Action<CombatUnit> tryAdd = (unit) =>
            {
                if (unit != null && unit.IsAlive && visitedSlots.Add(unit.SlotIndex))
                {
                    uniqueTargets.Add(unit);
                }
            };

            if (pattern == WeaponAttackPattern.Single)
            {
                tryAdd(primaryTarget);
            }
            else if (pattern == WeaponAttackPattern.Row) // Layer sweep
            {
                int targetLayer = CombatBattlefieldHelper.GetLayer(primaryTarget.SlotIndex);
                var rowUnits = CombatBattlefieldHelper.GetAliveUnitsInLayer(opponents, targetLayer);
                foreach (var unit in rowUnits)
                {
                    tryAdd(unit);
                }
            }
            else if (pattern == WeaponAttackPattern.ColumnAttack) // Lane pierce
            {
                int targetLane = CombatBattlefieldHelper.GetLane(primaryTarget.SlotIndex);
                var colUnits = opponents.FindAll(u => u.IsAlive && CombatBattlefieldHelper.GetLane(u.SlotIndex) == targetLane);
                foreach (var unit in colUnits)
                {
                    tryAdd(unit);
                }
            }
            else if (pattern == WeaponAttackPattern.Cleave)
            {
                tryAdd(primaryTarget);
                int primaryLayer = CombatBattlefieldHelper.GetLayer(primaryTarget.SlotIndex);
                int primaryLane = CombatBattlefieldHelper.GetLane(primaryTarget.SlotIndex);
                
                foreach (var unit in opponents)
                {
                    if (unit.IsAlive && CombatBattlefieldHelper.GetLayer(unit.SlotIndex) == primaryLayer)
                    {
                        int lane = CombatBattlefieldHelper.GetLane(unit.SlotIndex);
                        // Cleave hits immediate adjacent lanes only (distance of 1, e.g. lane 0 and 2 from 1).
                        // Note: Assumes standard 3-lane horizontal grid mapping. If grid sizes increase in the future,
                        // this only targets lanes immediately adjacent (left/right) to the primary target.
                        if (Math.Abs(lane - primaryLane) == 1)
                        {
                            tryAdd(unit);
                        }
                    }
                }
            }

            return uniqueTargets;
        }

        private static CombatUnit GetLeftmostUnit(List<CombatUnit> units)
        {
            if (units == null || units.Count == 0) return null;
            CombatUnit best = units[0];
            int minLane = CombatBattlefieldHelper.GetLane(best.SlotIndex);
            for (int i = 1; i < units.Count; i++)
            {
                int lane = CombatBattlefieldHelper.GetLane(units[i].SlotIndex);
                if (lane < minLane)
                {
                    minLane = lane;
                    best = units[i];
                }
            }
            return best;
        }
    }
}

