using GameScripts.Combat.Core;
using GameScripts.Systems.Enemies;
using Mewtations.Combat;
using System.Collections.Generic;

namespace GameScripts.Combat.Core
{
    public static class CombatUnitFactory
    {
        public static CombatUnit CreateFromCat(CatCardData cat, int slotIndex)
        {
            var unit = new CombatUnit();
            unit.IsPlayer = true;
            unit.SlotIndex = slotIndex;
            unit.Source = cat;
            unit.Name = cat.Name;
            unit.IsBoss = cat.IsBoss;

            unit.MaxHP = cat.ProcessedCombatStats.MaxHealth;
            unit.CurrentHP = cat.HealthPoints;
            unit.ATK = cat.ProcessedCombatStats.AttackDamage;
            unit.DEF = (int)cat.ProcessedCombatStats.Defence;
            unit.Speed = cat.Speed;

            unit.CurrentRage = cat.CurrentRage;
            unit.MaxRage = 145; // Legacy hardcode, could be adjustable
            unit.Element = cat.Element;
            unit.Role = cat.Role;

            unit.Stamina = cat.Stamina;
            unit.MaxStamina = cat.MaxStamina;
            unit.IsExhausted = cat.IsExhausted;
            unit.HoiQuangPhanChieuTriggered = cat.HoiQuangPhanChieuTriggered;
            unit.ExhaustionLevel = cat.ExhaustionLevel;

            // Attack Pattern from Weapon
            var weaponCard = cat.GetEquipableOfEquipableType(EquipableType.Weapon) as Equipable;
            unit.AttackPattern = MapLegacyWeaponToCombatPattern(weaponCard);

            // Combat Skills
            if (!cat.HasMutation(Mewtations.Expedition.UnstableMutation.DualWeapon))
            {
                int maxSkills = cat.HasMutation(Mewtations.Expedition.UnstableMutation.DualSkill) ? 2 : 1;
                var allEquipables = cat.GetAllEquipables();
                foreach (var eq in allEquipables)
                {
                    if (eq != null && eq.EquipableType == EquipableType.Skill && eq.ProvidedCombatSkill != null)
                    {
                        unit.CombatSkills.Add(eq.ProvidedCombatSkill);
                        if (unit.CombatSkills.Count >= maxSkills) break;
                    }
                }
            }

            // Traits, Mutations, Scars (Legacy registration via MewtationsEventPipeline)
            var activeComps = new List<IMewtationsComponent>();
            foreach (var traitId in cat.PermanentTraits)
            {
                var comp = MewtationsComponentRegistry.Create(traitId);
                if (comp != null) activeComps.Add(comp);
            }
            foreach (var mutId in cat.ActiveMutations)
            {
                var comp = MewtationsComponentRegistry.Create(mutId);
                if (comp != null) activeComps.Add(comp);
            }
            foreach (var scarId in cat.PermanentScars)
            {
                var comp = MewtationsComponentRegistry.Create(scarId);
                if (comp != null) activeComps.Add(comp);
            }
            if (cat.Specialization != Cards.Cats.DaoSpecialization.None)
            {
                var comp = Cards.Cats.CultivationSpecializationRegistry.CreateComponent(cat.Specialization);
                if (comp != null) activeComps.Add(comp);
            }
            var allEquipables = cat.GetAllEquipables();
            foreach (var eq in allEquipables)
            {
                if (eq != null && eq.EquipableType == EquipableType.Talisman)
                {
                    var comp = MewtationsComponentRegistry.Create(eq.Id);
                    if (comp != null) activeComps.Add(comp);
                }
            }
            MewtationsEventPipeline.RegisterUnitComponents(unit, activeComps);

            return unit;
        }

        public static CombatUnit CreateFromDog(DogEnemyInstance dog, int slotIndex)
        {
            var unit = new CombatUnit();
            unit.IsPlayer = false;
            unit.SlotIndex = slotIndex;
            unit.Name = Mewtations.Core.MewtationsLoc.Translate(dog.Definition.NameKey);
            unit.IsBoss = false; // Could be extended in Definition

            unit.MaxHP = dog.HP;
            unit.CurrentHP = dog.HP;
            unit.ATK = dog.ATK;
            unit.DEF = dog.DEF;
            unit.Speed = dog.SPD;

            unit.CurrentRage = 0;
            unit.MaxRage = 145; // Default combat rage max
            unit.Element = dog.Definition.Element;
            unit.Role = CatRole.DPS; // Dogs default to DPS role for now

            unit.AttackPattern = dog.Definition.AttackPattern;
            if (dog.Definition.ActiveCombatSkill != null)
            {
                unit.CombatSkills.Add(dog.Definition.ActiveCombatSkill);
            }

            MewtationsEventPipeline.RegisterUnitComponents(unit, new List<IMewtationsComponent>());

            return unit;
        }

        // Helper for backwards compatibility with legacy generator
        public static CombatUnit CreateFromLegacyEnemy(Combatable enemy, int slotIndex)
        {
            if (enemy is CatCardData cat)
            {
                var unit = CreateFromCat(cat, slotIndex);
                unit.IsPlayer = false; // Override since it's an enemy
                return unit;
            }
            
            // Fallback for non-Cat combatables (if any)
            var fallback = new CombatUnit();
            fallback.IsPlayer = false;
            fallback.SlotIndex = slotIndex;
            fallback.Source = enemy;
            fallback.Name = enemy.Name;
            fallback.MaxHP = enemy.ProcessedCombatStats.MaxHealth;
            fallback.CurrentHP = enemy.HealthPoints;
            fallback.ATK = enemy.ProcessedCombatStats.AttackDamage;
            fallback.DEF = (int)enemy.ProcessedCombatStats.Defence;
            fallback.Speed = 100;
            fallback.CurrentRage = 0;
            fallback.MaxRage = 145;
            fallback.AttackPattern = CombatAttackPattern.Sword;
            MewtationsEventPipeline.RegisterUnitComponents(fallback, new List<IMewtationsComponent>());
            return fallback;
        }

        private static CombatAttackPattern MapLegacyWeaponToCombatPattern(Equipable weapon)
        {
            if (weapon == null) return CombatAttackPattern.Bite; // Unarmed cat defaults to Bite

            // In Phase 4, we decoupled WeaponAttackPattern into CombatAttackPattern.
            // Map legacy WeaponAttackPattern if you want, or just read it from WeaponData directly
            // For now, let's map using the old logic mapping
            var oldPattern = MewtationsWeaponRegistry.GetAttackPattern(weapon.Id);
            switch (oldPattern)
            {
                case WeaponAttackPattern.Single: return CombatAttackPattern.Sword;
                case WeaponAttackPattern.Row: return CombatAttackPattern.Bow;
                case WeaponAttackPattern.ColumnAttack: return CombatAttackPattern.Spear;
                case WeaponAttackPattern.Cleave: return CombatAttackPattern.Axe;
                case WeaponAttackPattern.RageDrain: return CombatAttackPattern.Magic;
                case WeaponAttackPattern.RageGain: return CombatAttackPattern.Sword;
                default: return CombatAttackPattern.Bite;
            }
        }
    }
}
