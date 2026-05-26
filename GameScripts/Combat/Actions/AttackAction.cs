using System;
using System.Collections.Generic;
using UnityEngine;
using Mewtations.Combat;
using Mewtations.Combat.Core;

// ACTIVE TURN-BASED COMBAT SYSTEM
// ALL NEW COMBAT FEATURES MUST USE Combat
namespace Mewtations.Combat.Actions
{
    public class AttackAction : CombatAction
    {
        public override bool CanExecute(CombatEncounter encounter)
        {
            if (Actor == null || !Actor.IsAlive) return false;
            if (MainTarget == null || !MainTarget.IsAlive) return false;
            return true;
        }

        public override CombatSnapshot Execute(CombatEncounter encounter)
        {
            CombatSnapshot snapshot = new CombatSnapshot();
            snapshot.Attacker = Actor;
            snapshot.Target = MainTarget;
            snapshot.ActionType = CombatActionType.Attack;

            if (MainTarget != null)
            {
                int prevHp = MainTarget.CurrentHP;
                int prevShield = MainTarget.Shield;

                // Execute utilizing legacy calculated formula cleanly!
                MewtationsWeaponRegistry.ExecuteBasicAttack(
                    Actor, 
                    MainTarget, 
                    encounter.GetAlliesOf(Actor), 
                    encounter.GetOpponentsOf(Actor), 
                    encounter.AddLog
                );

                snapshot.FinalDamage = Mathf.Max(0, prevHp - MainTarget.CurrentHP);
                snapshot.IsBlocked = MainTarget.Shield < prevShield;
                snapshot.TargetDied = !MainTarget.IsAlive;
            }

            snapshot.ElementType = Actor.Element;
            snapshot.OriginCell = new Vector2Int(Actor.SlotIndex % 3, Actor.SlotIndex / 3);
            if (MainTarget != null)
            {
                snapshot.TargetCell = new Vector2Int(MainTarget.SlotIndex % 3, MainTarget.SlotIndex / 3);
            }

            return snapshot;
        }
    }
}
