using System;
using Mewtations.Combat;
using Mewtations.Combat.Core;

// ACTIVE TURN-BASED COMBAT SYSTEM
// ALL NEW COMBAT FEATURES MUST USE Combat
namespace Mewtations.Combat.Actions
{
    public abstract class CombatAction
    {
        public CombatUnit Actor;
        public CombatUnit MainTarget;

        public abstract bool CanExecute(CombatEncounter encounter);
        public abstract CombatSnapshot Execute(CombatEncounter encounter);
    }
}
