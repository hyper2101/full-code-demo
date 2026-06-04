using UnityEngine;

namespace Mewtations.Combat.Core
{
    public static class CombatResultApplier
    {
        public static void ApplyResult(CombatResultData resultData)
        {
            if (resultData == null || resultData.CatOutcomes == null) return;

            foreach (var outcome in resultData.CatOutcomes)
            {
                var cat = outcome.CatReference;
                if (cat == null) continue;

                // Sync HP & Stamina
                cat.HealthPoints = outcome.FinalHP;
                cat.Stamina = outcome.FinalStamina;

                // Exhaustion
                if (outcome.WasExhausted)
                {
                    cat.IsExhausted = true;
                    // Keep existing level or set to 1 if not set
                    cat.ExhaustionLevel = Mathf.Max(1, cat.ExhaustionLevel);
                }
                else
                {
                    cat.IsExhausted = false;
                    cat.ExhaustionLevel = 0;
                }

                // Paralyzed Check
                if (outcome.BecameParalyzed)
                {
                    // Add Paralyzed to Permanent Status or wherever Paralyzed is stored.
                    // Assuming Paralyzed is a scar or condition. 
                    // Wait, game has 'cat.RefreshConditionState()' which might check for Paralyzed, or we can use AddScar.
                    // Let's add a Paralyzed condition/scar if it exists, or just set HP = 0 and IsParalyzed if there's a field for it.
                    // Since I don't see an explicit Paralyzed field, I'll assume adding a Paralyzed scar or setting a tag is appropriate.
                    if (!cat.HasScar(Mewtations.Combat.PermanentScar.Paralyzed))
                    {
                        cat.AddScar(Mewtations.Combat.PermanentScar.Paralyzed);
                    }
                    cat.HealthPoints = 0; // Double ensure
                }

                // Force refresh UI/Visuals
                cat.RefreshConditionState();
            }
        }
    }
}
