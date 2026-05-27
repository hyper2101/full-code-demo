using System;

namespace Mewtations.Core
{
    public enum LegacyCategory
    {
        DeprecatedMechanic,
        DeprecatedEconomyLoop,
        DeprecatedAutomation,
        DeprecatedNarrativeFlow,
        DeprecatedTopologyUsage,
        DeprecatedUI,
        DeprecatedCombatV1
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class LegacySystemAttribute : Attribute
    {
        public LegacyCategory Category { get; }

        public LegacySystemAttribute(LegacyCategory category)
        {
            Category = category;
        }
    }
}
