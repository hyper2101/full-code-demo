using System;
using System.Collections.Generic;
using Mewtations.Combat;

// ACTIVE TURN-BASED COMBAT SYSTEM
// ALL NEW COMBAT FEATURES MUST USE Combat
namespace Mewtations.Combat.Core
{
    [Serializable]
    public class StatusChangeData
    {
        public CombatUnit TargetUnit;
        public MewtationsDebuff DebuffType;
        public int NewDuration;
        public int NewStacks;
        public bool IsAdded;
        public bool IsRemoved;
    }

    [Serializable]
    public class RoundSnapshot
    {
        public int RoundIndex;
        public List<CombatUnit> TurnOrder = new List<CombatUnit>();
        public List<CombatSnapshot> ActionsExecuted = new List<CombatSnapshot>();
        public List<CombatUnit> Deaths = new List<CombatUnit>();
        public List<StatusChangeData> StatusChanges = new List<StatusChangeData>();
    }
}
