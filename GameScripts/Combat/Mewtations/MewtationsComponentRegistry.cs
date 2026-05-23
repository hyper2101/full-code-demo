using System;
using System.Collections.Generic;
using UnityEngine;
using Mewtations.Expedition;

namespace Mewtations.Combat
{
    public static class MewtationsComponentRegistry
    {
        private static Dictionary<string, Func<IMewtationsComponent>> _factory = new Dictionary<string, Func<IMewtationsComponent>>();

        static MewtationsComponentRegistry()
        {
            // --- PERMANENT HEAVENLY TALENTS ---
            Register("talent_divine_shield", () => new DivineShieldTalent());
            Register("talent_rage_overcharger", () => new RageOverchargerTalent());
            Register("talent_poison_body", () => new HeavenlyPoisonBodyTalent());
            Register("talent_martial_cleave", () => new MartialArtsCleaveTalent());

            // Map string constants from HeavenlyTalent registry
            Register(HeavenlyTalent.DivineShieldProtection, () => new DivineShieldTalent());
            Register(HeavenlyTalent.RageOvercharger, () => new RageOverchargerTalent());
            Register(HeavenlyTalent.HeavenlyPoisonBody, () => new HeavenlyPoisonBodyTalent());
            Register(HeavenlyTalent.MartialArtsCleave, () => new MartialArtsCleaveTalent());

            // --- UNSTABLE MUTATIONS ---
            Register(UnstableMutation.LethargicNap, () => new LethargicNapMutation());
            Register(UnstableMutation.UnstableClaws, () => new UnstableClawsMutation());
            Register(UnstableMutation.CursedFur, () => new CursedFurMutation());
            Register("morale_collapse", () => new MoraleCollapseComponent());
            // demonic_transcendence registration removed per user request

            // --- PERMANENT SCARS ---
            Register(PermanentScar.CrippledMeridians, () => PermanentScar.CreateComponent(PermanentScar.CrippledMeridians));
            Register(PermanentScar.BloodDepletion, () => PermanentScar.CreateComponent(PermanentScar.BloodDepletion));
            Register(PermanentScar.SoulScar, () => PermanentScar.CreateComponent(PermanentScar.SoulScar));

            // --- TALISMANS ---
            Register("talisman_heavy_armor", () => new HeavyArmorTalisman());
            Register("talisman_rage_core", () => new RageCoreTalisman());
            Register("talisman_health_regen", () => new HealthRegenTalisman());
            Register("talisman_iron_will", () => new IronWillTalisman());
        }

        public static void Register(string id, Func<IMewtationsComponent> creator)
        {
            _factory[id.ToLower()] = creator;
        }

        public static IMewtationsComponent Create(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            string key = id.ToLower();
            if (_factory.TryGetValue(key, out var creator))
            {
                return creator();
            }

            // Fallback checking sub-strings
            foreach (var pair in _factory)
            {
                if (key.Contains(pair.Key))
                {
                    return pair.Value();
                }
            }

            return null;
        }

        // ==========================================
        // INDIVIDUAL COMPONENT CLASSES IMPLEMENTATION
        // ==========================================

        // --- 1. Heavenly Talents ---

        private class DivineShieldTalent : IMewtationsComponent
        {
            public string Id => "talent_divine_shield";
            public string DisplayName => "Divine Shield";
            public string Description => "Starts combat with +15 Shield.";
            public void Initialize(CombatUnit unit) { unit.Shield += 15; }
        }

        private class RageOverchargerTalent : IMewtationsComponent
        {
            public string Id => "talent_rage_overcharger";
            public string DisplayName => "Rage Overcharger";
            public string Description => "Gains +10 extra Rage per attack action.";
            public void Initialize(CombatUnit unit) {}
            public void AfterAttack(CombatUnit attacker, CombatUnit target, int damage, Action<string> logCallback)
            {
                if (attacker.IsAlive)
                {
                    attacker.CurrentRage = Mathf.Min(145, attacker.CurrentRage + 10);
                    logCallback?.Invoke($"⚡ {attacker.Name} kích hoạt Nộ Khí Cuồng Triều, nhận thêm 10 Nộ khí!");
                }
            }
        }

        private class HeavenlyPoisonBodyTalent : IMewtationsComponent
        {
            public string Id => "talent_poison_body";
            public string DisplayName => "Heavenly Poison Body";
            public string Description => "Inflicts Poisoned status (3 turns) on target with every attack.";
            public void Initialize(CombatUnit unit) {}
            public void AfterAttack(CombatUnit attacker, CombatUnit target, int damage, Action<string> logCallback)
            {
                if (target.IsAlive)
                {
                    target.AddDebuff(MewtationsDebuff.Poisoned, 3);
                    logCallback?.Invoke($"☠️ Đòn đánh của {attacker.Name} tẩm độc linh lực, gây trúng độc lên {target.Name}!");
                }
            }
        }

        private class MartialArtsCleaveTalent : IMewtationsComponent
        {
            public string Id => "talent_martial_cleave";
            public string DisplayName => "Martial Cleave";
            public string Description => "Attacks automatically cleave horizontally adjacent targets.";
            public void Initialize(CombatUnit unit) {}
            // Cleave behavior is queried directly via trait check in weapon registry
        }

        // --- 2. Unstable Mutations ---

        private class LethargicNapMutation : IMewtationsComponent
        {
            public string Id => UnstableMutation.LethargicNap;
            public string DisplayName => "Lethargic Nap";
            public string Description => "-15 Speed, but recovers 5 HP at turn start.";
            public void Initialize(CombatUnit unit)
            {
                unit.Speed = Mathf.Max(10, unit.Speed - 15);
            }
            public void OnTurnStart(CombatUnit unit, Action<string> logCallback)
            {
                if (unit.IsAlive)
                {
                    unit.Heal(5);
                    logCallback?.Invoke($"💤 {unit.Name} đang ngái ngủ tự hồi phục 5 HP dưỡng thương.");
                }
            }
        }

        private class UnstableClawsMutation : IMewtationsComponent
        {
            public string Id => UnstableMutation.UnstableClaws;
            public string DisplayName => "Unstable Claws";
            public string Description => "+30% Damage, but suffers 2 self-damage after attacking.";
            public void Initialize(CombatUnit unit) {}
            public void BeforeAttack(CombatUnit attacker, CombatUnit target, ref int damage, Action<string> logCallback)
            {
                damage = Mathf.RoundToInt(damage * 1.3f);
            }
            public void AfterAttack(CombatUnit attacker, CombatUnit target, int damage, Action<string> logCallback)
            {
                if (attacker.IsAlive)
                {
                    attacker.TakeDamage(2);
                    logCallback?.Invoke($"☣️ {attacker.Name} bị đột biến tự phế kinh mạch, hao tổn 2 HP!");
                }
            }
        }

        private class CursedFurMutation : IMewtationsComponent
        {
            public string Id => UnstableMutation.CursedFur;
            public string DisplayName => "Cursed Fur";
            public string Description => "Locked from receiving any defensive Shields.";
            public void Initialize(CombatUnit unit) { unit.Shield = 0; }
            // Shield immunity is checked inside CombatUnit.AddShield
        }

        // --- 3. Talismans ---

        private class HeavyArmorTalisman : IMewtationsComponent
        {
            public string Id => "talisman_heavy_armor";
            public string DisplayName => "Heavy Armor Talisman";
            public string Description => "Starts combat with +10 Shield.";
            public void Initialize(CombatUnit unit) { unit.Shield += 10; }
        }

        private class RageCoreTalisman : IMewtationsComponent
        {
            public string Id => "talisman_rage_core";
            public string DisplayName => "Rage Core Talisman";
            public string Description => "Starts combat with +30 starting Rage.";
            public void Initialize(CombatUnit unit) { unit.CurrentRage = Mathf.Min(145, unit.CurrentRage + 30); }
        }

        private class HealthRegenTalisman : IMewtationsComponent
        {
            public string Id => "talisman_health_regen";
            public string DisplayName => "Regen Talisman";
            public string Description => "Heals 3 HP at the beginning of each turn.";
            public void Initialize(CombatUnit unit) {}
            public void OnTurnStart(CombatUnit unit, Action<string> logCallback)
            {
                if (unit.IsAlive)
                {
                    unit.Heal(3);
                    logCallback?.Invoke($"💚 [BÙA HỒI PHỤC] Bùa hộ thân giúp {unit.Name} tự động hồi phục 3 HP dưỡng thương.");
                }
            }
        }

        private class IronWillTalisman : IMewtationsComponent
        {
            public string Id => "talisman_iron_will";
            public string DisplayName => "Iron Will Talisman";
            public string Description => "Immune to freeze and rage-drain effects.";
            public void Initialize(CombatUnit unit) { unit.HasIronWill = true; }
        }

        private class MoraleCollapseComponent : IMewtationsComponent
        {
            public string Id => "morale_collapse";
            public string DisplayName => "Đạo Tâm Trì Trệ";
            public string Description => "Tông môn thiếu thốn bổng lộc: Giảm 25% Thần Tốc và 25% HP tối đa.";
            public void Initialize(CombatUnit unit)
            {
                unit.Speed = Mathf.RoundToInt(unit.Speed * 0.75f);
                unit.MaxHP = Mathf.RoundToInt(unit.MaxHP * 0.75f);
                unit.CurrentHP = Mathf.Min(unit.MaxHP, unit.CurrentHP);
            }
        }

        // DemonicTranscendenceComponent removed per user request
    }
}
