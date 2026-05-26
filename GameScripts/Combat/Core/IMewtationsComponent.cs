using System;
using System.Collections.Generic;

namespace Mewtations.Combat
{
    public interface IMewtationsComponent
    {
        string Id { get; }
        string DisplayName { get; }
        string Description { get; }

        void Initialize(CombatUnit unit);

        // Turn Events
        void OnTurnStart(CombatUnit unit, Action<string> logCallback) {}
        void OnTurnEnd(CombatUnit unit, Action<string> logCallback) {}

        // Attack Pipeline Events
        void BeforeAttack(CombatUnit attacker, CombatUnit target, ref int damage, Action<string> logCallback) {}
        void AfterAttack(CombatUnit attacker, CombatUnit target, int damage, Action<string> logCallback) {}

        // Damage Pipeline Events
        void BeforeDamage(CombatUnit victim, CombatUnit attacker, ref int damage, Action<string> logCallback) {}
        void AfterDamage(CombatUnit victim, CombatUnit attacker, int damage, Action<string> logCallback) {}

        // General Combat Events
        void OnKill(CombatUnit killer, CombatUnit victim, Action<string> logCallback) {}
        void OnDeath(CombatUnit unit, Action<string> logCallback) {}
    }
}
