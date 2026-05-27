using UnityEngine;

namespace Mewtations.Systems.Labor
{
    /// <summary>
    /// Helper layer to replace legacy `x is Worker` checks.
    /// Acts as an architectural firewall to prevent reverting back to Stacklands workers.
    /// </summary>
    public static class LaborUtility
    {
        /// <summary>
        /// Checks if a CardData is capable of performing labor and is currently allowed to.
        /// Replaces `cardData is Worker`.
        /// </summary>
        public static bool IsLaborCapable(CardData cardData)
        {
            if (cardData == null) return false;

            // Preserve compatibility with legacy workers during transition (if they still exist)
            if (cardData is Worker) return true;
            if (cardData is BaseVillager) return true;

            // Modern Mewtations check
            if (cardData is ILaborCapable laborCard)
            {
                return laborCard.CanPerformLabor();
            }

            return false;
        }

        /// <summary>
        /// Safely consumes labor stamina if the card implements ILaborCapable.
        /// </summary>
        public static void ConsumeLaborStamina(CardData cardData, float amount)
        {
            if (cardData is ILaborCapable laborCard)
            {
                laborCard.ConsumeLaborStamina(amount);
            }
        }

        /// <summary>
        /// Gets the efficiency multiplier for crafting/gathering time.
        /// Returns 1.0f by default for non-ILaborCapable cards.
        /// </summary>
        public static float GetLaborEfficiency(CardData cardData)
        {
            if (cardData is ILaborCapable laborCard)
            {
                return laborCard.GetLaborEfficiency();
            }
            return 1.0f;
        }
    }
}
