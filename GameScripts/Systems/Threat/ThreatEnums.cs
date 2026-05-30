namespace GameScripts.Systems.Threat
{
    public enum ThreatSourceType
    {
        TimeCycle,
        CatGod,
        Expedition,
        Story,
        Manual
    }

    public enum ThreatState
    {
        Scheduled,
        Warning,
        Active,
        Cooldown,
        Resolved
    }

    public enum ThreatPenaltyType
    {
        LockExpedition,
        ReduceProduction,
        DisableBuilding,
        Custom
    }
}
