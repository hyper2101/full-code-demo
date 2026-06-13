using System.Collections.Generic;
using System.Linq;

namespace Mewtations.Systems.Economy
{
    public static class CurrencyUtility
    {
        public static int GetTotalCurrencyValue(IEnumerable<ICurrency> currencies)
        {
            if (currencies == null) return 0;
            return currencies.Sum(c => c.RawValue);
        }

        public static bool CanSatisfyTierRequirement(IEnumerable<ICurrency> currencies, CurrencyTier requiredTier, int requiredValue)
        {
            if (currencies == null) return false;

            int validValue = 0;
            foreach (var currency in currencies)
            {
                // Higher tier or same tier is valid (since CanSubstituteLowerTier should be true for higher tiers, 
                // but we also explicitly check if the currency is allowed to substitute)
                if (currency.Tier >= requiredTier)
                {
                    validValue += currency.RawValue;
                }
            }

            return validValue >= requiredValue;
        }

        public static Dictionary<CurrencyTier, int> CreateCurrencyBreakdown(IEnumerable<ICurrency> currencies)
        {
            var breakdown = new Dictionary<CurrencyTier, int>();
            if (currencies == null) return breakdown;

            foreach (var currency in currencies)
            {
                if (!breakdown.ContainsKey(currency.Tier))
                {
                    breakdown[currency.Tier] = 0;
                }
                breakdown[currency.Tier] += currency.RawValue;
            }

            return breakdown;
        }
    }
}
