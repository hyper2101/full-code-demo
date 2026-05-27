namespace Mewtations.Core
{
    /// <summary>
    /// Soft quarantine layer for managing legacy Stacklands DNA runtime execution.
    /// This is NOT a permanent architecture, but a safety valve for controlled migration.
    /// Uses static bools instead of consts to avoid compile-time inlining and hot-reload bugs.
    /// </summary>
    public static class LegacyRuntimeFlags
    {
        public static bool EnableCitiesSystem = false;
        public static bool EnableDemands = false;
        public static bool EnableRealtimeCombat = false;
        public static bool EnableQuestHooks = false;
    }
}
