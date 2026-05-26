using System;
using System.Collections.Generic;
using System.Linq;
using Mewtations.Combat;

// ACTIVE TURN-BASED COMBAT SYSTEM
// ALL NEW COMBAT FEATURES MUST USE Combat
namespace Mewtations.Combat.TurnOrder
{
    public static class InitiativeResolver
    {
        public static List<CombatUnit> BuildTurnQueue(List<CombatUnit> units)
        {
            if (units == null) return new List<CombatUnit>();

            var activeUnits = units.FindAll(u => u.IsAlive);

            // Stable Tie-Breaker Rule:
            // 1. Sort by Speed descending.
            // 2. If tie, prioritize based on SlotIndex (Stable spawn order) to ensure fully deterministic results.
            return activeUnits
                .OrderByDescending(u => u.Speed)
                .ThenBy(u => u.SlotIndex)
                .ToList();
        }
    }
}
