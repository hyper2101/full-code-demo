using System;
using System.Collections.Generic;

namespace Mewtations.Combat
{
    /// <summary>
    /// =========================================================================
    /// STRICT DESIGN LOCK: TARGET RESOLUTION & REDIRECTION AI (LANE-BASED)
    /// =========================================================================
    /// Encapsulates targeting decision flows, redirection (taunt/tanking), and 
    /// AOE/multi-target weapon patterns.
    /// 
    /// BEHAVIORAL PARITY REQUIREMENT:
    /// All targeting decisions must strictly respect:
    /// 1. Nearest Lane priority (with Top Lane bias on tie).
    /// 2. Front-to-Back Depth priority within that Lane.
    /// =========================================================================
    /// </summary>
    public static class CombatTargetResolver
    {
        /// <summary>
        /// Selects the primary target based on Lane priority, then Frontmost Depth.
        /// </summary>
        public static CombatUnit GetPrimaryTarget(List<CombatUnit> enemies, CombatUnit attacker)
        {
            int attackerLane = 1; // Default to mid
            if (attacker != null)
            {
                attackerLane = CombatBattlefieldHelper.GetLane(attacker.SlotIndex);
            }

            var priorityLanes = CombatBattlefieldHelper.GetNearestLaneOrder(attackerLane);
            foreach (int lane in priorityLanes)
            {
                var aliveInLane = CombatBattlefieldHelper.GetAliveUnitsInLane(enemies, lane);
                if (aliveInLane.Count > 0)
                {
                    return GetFrontmostUnit(aliveInLane);
                }
            }

            var allAlive = enemies.FindAll(e => e.IsAlive);
            return GetFrontmostUnit(allAlive);
        }

        /// <summary>
        /// Handles tanking/redirection mechanics. 
        /// Opponent tanks in Depth 0 (Frontline) have a fixed 30% chance to redirect 
        /// basic attacks targeted at mid/backline slots, gaining +5 Shield in the process.
        /// </summary>
        public static CombatUnit ResolveRedirectedTarget(CombatUnit attacker, CombatUnit target, List<CombatUnit> opponents, Action<string> logCallback)
        {
            if (target == null || opponents == null)
                return target;

            // Tanker redirection check (Depth >= 1 indicates backline / midline)
            if (CombatBattlefieldHelper.GetLayer(target.SlotIndex) >= 1)
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

            if (pattern == WeaponAttackPattern.Single || pattern == WeaponAttackPattern.RageDrain || pattern == WeaponAttackPattern.RageGain)
            {
                tryAdd(primaryTarget);
            }
            else if (pattern == WeaponAttackPattern.Row) // Hàng / Row: Đánh cùng y (Lane) cắt qua các Depth
            {
                int targetLane = CombatBattlefieldHelper.GetLane(primaryTarget.SlotIndex);
                var laneUnits = CombatBattlefieldHelper.GetAliveUnitsInLane(opponents, targetLane);
                foreach (var unit in laneUnits)
                {
                    tryAdd(unit);
                }
            }
            else if (pattern == WeaponAttackPattern.ColumnAttack) // Cột / Column: Đánh cùng x (Depth) cắt qua các Lane
            {
                int targetDepth = CombatBattlefieldHelper.GetLayer(primaryTarget.SlotIndex);
                var depthUnits = opponents.FindAll(u => u.IsAlive && CombatBattlefieldHelper.GetLayer(u.SlotIndex) == targetDepth);
                foreach (var unit in depthUnits)
                {
                    tryAdd(unit);
                }
            }
            else if (pattern == WeaponAttackPattern.Cleave)
            {
                tryAdd(primaryTarget);
                int primaryDepth = CombatBattlefieldHelper.GetLayer(primaryTarget.SlotIndex);
                int primaryLane = CombatBattlefieldHelper.GetLane(primaryTarget.SlotIndex);
                
                foreach (var unit in opponents)
                {
                    if (unit.IsAlive && CombatBattlefieldHelper.GetLayer(unit.SlotIndex) == primaryDepth)
                    {
                        int lane = CombatBattlefieldHelper.GetLane(unit.SlotIndex);
                        // Cleave hits immediate adjacent lanes only at the same Depth.
                        if (Math.Abs(lane - primaryLane) == 1)
                        {
                            tryAdd(unit);
                        }
                    }
                }
            }

            return uniqueTargets;
        }

        private static CombatUnit GetFrontmostUnit(List<CombatUnit> units)
        {
            if (units == null || units.Count == 0) return null;
            CombatUnit best = units[0];
            int minDepth = CombatBattlefieldHelper.GetLayer(best.SlotIndex);
            for (int i = 1; i < units.Count; i++)
            {
                int depth = CombatBattlefieldHelper.GetLayer(units[i].SlotIndex);
                if (depth < minDepth)
                {
                    minDepth = depth;
                    best = units[i];
                }
            }
            return best;
        }
    }
}

