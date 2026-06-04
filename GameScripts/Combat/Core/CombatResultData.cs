using System;
using System.Collections.Generic;

namespace Mewtations.Combat.Core
{
    public enum CombatResult
    {
        Ongoing,
        Victory,
        Defeat,
        Retreated
    }

    public enum CombatEndReason
    {
        EnemyDefeated,
        TeamDefeated,
        TurnLimitReached,
        Retreat
    }

    [Serializable]
    public class CatCombatOutcome
    {
        public CatCardData CatReference;
        public int FinalHP;
        public int FinalStamina;
        public bool WasDefeated;
        public bool BecameParalyzed;
        public bool WasExhausted;
    }

    [Serializable]
    public class CombatResultData
    {
        public CombatResult Result;
        public CombatEndReason EndReason;
        public List<CatCombatOutcome> CatOutcomes = new List<CatCombatOutcome>();
    }
}
