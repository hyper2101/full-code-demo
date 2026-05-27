using System;

public enum RunEntityState
{
    Alive,
    Downed,
    Dead,
    Sealed,
    Petrified,
    Missing,
    Corrupted
}

public interface IPrimaryRunEntity
{
    bool CountsForRunSurvival { get; }
    RunEntityState CurrentRunState { get; }
    
    // Focused gameplay semantics
    bool CanAct { get; }
    bool BlocksRunFailure { get; }
}
