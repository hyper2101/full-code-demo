using System;
using System.Collections.Generic;
using UnityEngine;
using Mewtations.Combat;

// ACTIVE TURN-BASED COMBAT SYSTEM
// ALL NEW COMBAT FEATURES MUST USE Combat
namespace Mewtations.Combat.Core
{
    public enum CombatActionType
    {
        Attack,
        Defend,
        Move,
        Skill,
        Escape,
        Wait
    }

    public class AppliedStatusData
    {
        public MewtationsDebuff Type;
        public int Duration;
        public int Stacks;

        public AppliedStatusData(MewtationsDebuff type, int duration, int stacks = 1)
        {
            Type = type;
            Duration = duration;
            Stacks = stacks;
        }
    }

    public class CombatSnapshot
    {
        public CombatUnit Attacker;
        public CombatUnit Target;

        public int FinalDamage;

        public bool IsCrit;
        public bool IsBlocked;
        public bool IsDodged;

        public List<AppliedStatusData> AppliedStatuses = new List<AppliedStatusData>();

        public bool TargetDied;

        public CatElement ElementType;

        public Vector2Int OriginCell;
        public Vector2Int TargetCell;

        public CombatActionType ActionType;
    }
}
