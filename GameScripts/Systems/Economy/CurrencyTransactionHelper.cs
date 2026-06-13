using System.Collections.Generic;

namespace Mewtations.Systems.Economy
{
    public static class CurrencyTransactionHelper
    {
        public static bool ConsumeCurrency(List<ICurrency> availableCurrencies, CurrencyTier requiredTier, int requiredValue)
        {
            if (!CurrencyUtility.CanSatisfyTierRequirement(availableCurrencies, requiredTier, requiredValue))
            {
                return false;
            }

            // In a real implementation, this would handle the actual removal of the physical cards from the board or player's inventory.
            // For now, this helper just outlines the logic.
            return true;
        }

        public static List<ICurrency> TryMakeChange(int excessValue)
        {
            // Outline for generating change. 
            // In full implementation, this should spawn the appropriate physical currency cards.
            // Returned change NEVER upgrades denomination authority.
            var change = new List<ICurrency>();
            return change;
        }
    }
}
