using System;
using System.Collections.Generic;
using System.Linq;

namespace Mewtations.Combat
{
    /// <summary>
    /// =========================================================================
    /// STRICT DESIGN LOCK: BATTLEFIELD TOPOLOGY
    /// =========================================================================
    /// Current production battlefield dimensions are intentionally fixed to:
    ///   - 3 Layers (depth)
    ///   - 3 Lanes (width)
    ///   - 9 Slots total
    /// 
    /// Do NOT generalize the combat system into dynamic-width or dynamic-height 
    /// grids during this refactor. The helper abstraction exists for readability 
    /// and topology isolation only.
    /// 
    /// Visual Grid:
    /// 0 1 2  (Layer 0 - Frontline)
    /// 3 4 5  (Layer 1 - Midline)
    /// 6 7 8  (Layer 2 - Backline)
    /// 
    /// Left-to-right columns represent Lanes 0, 1, 2.
    /// 
    /// BEHAVIORAL PARITY REQUIREMENT:
    /// Refactoring MUST preserve the exact emergent targeting behavior of Mewtations:
    /// Target priority = nearest layer from attacker, THEN leftmost lane inside that layer.
    /// 
    /// Example 1 (Attacker is positioned at Backline - Layer 2):
    /// Priority order against alive enemies MUST remain:
    ///   - First: Layer 2 (slots 6, 7, 8 in left-to-right order)
    ///   - Second: Layer 1 (slots 3, 4, 5 in left-to-right order)
    ///   - Third: Layer 0 (slots 0, 1, 2 in left-to-right order)
    /// 
    /// Example 2 (Attacker is positioned at Midline - Layer 1):
    /// Priority order against alive enemies MUST remain:
    ///   - First: Layer 1 (slots 3, 4, 5 in left-to-right order)
    ///   - Second: Layer 0 (slots 0, 1, 2 in left-to-right order)
    ///   - Third: Layer 2 (slots 6, 7, 8 in left-to-right order)
    /// 
    /// TARGET TIEBREAK RULE:
    /// When multiple valid targets exist in the same layer, selection MUST remain 
    /// deterministic using ascending lane order (left-to-right priority).
    /// 
    /// EVENT ORDER LOCK (DO NOT MERGE PHASES):
    /// Do NOT collapse combat phases into generic pipelines or merged processing passes.
    /// Existing sequential execution order must remain explicit and readable to
    /// preserve deterministic proc timing and side effects.
    /// 
    /// NO DESIGN INTERPRETATION:
    /// If current combat behavior appears unintuitive, asymmetric, or non-standard 
    /// compared to traditional frontline-tank RPG targeting, assume it is intentional 
    /// design and do NOT "fix" or change it.
    /// =========================================================================
    /// </summary>
    public static class CombatBattlefieldHelper
    {
        public static int GetLayer(int slotIndex) => slotIndex / 3;
        
        public static int GetLane(int slotIndex) => slotIndex % 3;
        
        public static int[] GetSlotsInLayer(int layer) => new[] { layer * 3, layer * 3 + 1, layer * 3 + 2 };
        
        public static List<CombatUnit> GetAliveUnitsInLayer(List<CombatUnit> units, int layer)
        {
            return units.FindAll(u => u.IsAlive && GetLayer(u.SlotIndex) == layer);
        }
        
        /// <summary>
        /// Generates the nearest-layer search order starting from attacker's layer.
        /// Scalable, deterministic, handles arbitrary layers without hardcoded logic.
        /// </summary>
        public static List<int> GetNearestLayerOrder(int attackerLayer, int maxLayers = 3)
        {
            return Enumerable
                .Range(0, maxLayers)
                .OrderBy(layer => Math.Abs(layer - attackerLayer))
                .ThenBy(layer => layer)
                .ToList();
        }
    }
}

