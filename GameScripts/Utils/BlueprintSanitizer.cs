using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mewtations.Utils
{
    /// <summary>
    /// Intercepts and sanitizes blueprints and game data at boot/load time.
    /// Prevents Legacy DNA (Stacklands assumptions) from leaking into the runtime data graph.
    /// </summary>
    public static class BlueprintSanitizer
    {
        public static void SanitizeBlueprints(List<Blueprint> blueprints)
        {
            if (blueprints == null) return;

            for (int i = blueprints.Count - 1; i >= 0; i--)
            {
                var bp = blueprints[i];
                if (bp == null) continue;

                // 1. Strip industrial/automation recipes
                if (IsLegacyAutomationRecipe(bp))
                {
                    Debug.Log($"[BlueprintSanitizer] Removed legacy automation recipe: {bp.name}");
                    blueprints.RemoveAt(i);
                    continue;
                }

                // 2. Strip tech tree / progression dependencies of legacy features
                // TODO: Intercept Stacklands quest unlocks

                // 3. Sanitize requirements
                SanitizeRequirements(bp);
            }
        }

        private static bool IsLegacyAutomationRecipe(Blueprint bp)
        {
            // Example heuristic: check if it yields a legacy industrial card
            if (bp.CardPrefab != null)
            {
                var type = bp.CardPrefab.GetType();
                var attribute = Attribute.GetCustomAttribute(type, typeof(Mewtations.Core.LegacySystemAttribute)) as Mewtations.Core.LegacySystemAttribute;
                if (attribute != null && attribute.Category == Mewtations.Core.LegacyCategory.DeprecatedAutomation)
                {
                    return true;
                }
            }
            return false;
        }

        private static void SanitizeRequirements(Blueprint bp)
        {
            // For example, if a blueprint requires "HasEnergy" or "HasEnergyWorkers", remove that requirement
            // This assumes bp has a SubRequirements list or similar, which would be cleaned here.
        }
    }
}
