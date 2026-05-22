using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mewtations.Combat
{
    public enum WeaponAttackPattern
    {
        Single,
        Row,
        ColumnAttack,
        Cleave,
        RageDrain,
        RageGain
    }

    public enum UltimateType
    {
        DefaultBasicBoost,
        HealLowest,
        AoeFireBurn,
        ShieldTeam,
        DisruptStun
    }

    public enum MewtationsDebuff
    {
        None,
        Burning,
        Poisoned,
        Frozen,
        Shocked
    }

    [Serializable]
    public class CombatStatusEffect
    {
        public MewtationsDebuff Type;
        public int Duration;
        public int Stacks;

        public CombatStatusEffect(MewtationsDebuff type, int duration, int stacks = 1)
        {
            Type = type;
            Duration = duration;
            Stacks = stacks;
        }
    }

    [Serializable]
    public class CombatUnit
    {
        public Combatable Source;
        public string Name;
        public int MaxHP;
        public int CurrentHP;
        public int CurrentRage;
        public int Speed;
        public bool IsPlayer;
        public int SlotIndex; // 0-2 Front, 3-5 Back
        public List<CombatStatusEffect> ActiveDebuffs = new List<CombatStatusEffect>();
        public int Shield = 0;

        public bool HasTrait(string id)
        {
            if (Source is CatCardData cat)
            {
                return cat.HasTrait(id);
            }
            return false;
        }

        public bool HasMutation(string id)
        {
            if (Source is CatCardData cat)
            {
                return cat.HasMutation(id);
            }
            return false;
        }

        public CombatUnit(Combatable source, bool isPlayer, int slotIndex)
        {
            Source = source;
            Name = source.Name;
            IsPlayer = isPlayer;
            SlotIndex = slotIndex;

            // Extract base stats
            var stats = source.ProcessedCombatStats;
            MaxHP = stats.MaxHealth;
            CurrentHP = source.HealthPoints;
            if (CurrentHP <= 0) CurrentHP = MaxHP; // Fallback
            
            // Extract Mewtations-specific stats
            if (source is CatCardData cat)
            {
                Speed = cat.Speed;
                CurrentRage = cat.CurrentRage;

                // Apply permanent Heavenly Talents
                if (cat.HasTrait(Mewtations.Expedition.HeavenlyTalent.DivineShieldProtection))
                {
                    Shield += 15;
                }

                // Apply temporary Unstable Mutations
                if (cat.HasMutation(Mewtations.Expedition.UnstableMutation.LethargicNap))
                {
                    Speed = Mathf.Max(10, Speed - 15);
                }
            }
            else
            {
                // Default stats for enemies
                Speed = 100;
                CurrentRage = 0;
            }
        }

        public bool IsAlive => CurrentHP > 0;

        public void TakeDamage(int damage)
        {
            if (Shield > 0)
            {
                if (Shield >= damage)
                {
                    Shield -= damage;
                    damage = 0;
                }
                else
                {
                    damage -= Shield;
                    Shield = 0;
                }
            }

            CurrentHP = Mathf.Max(0, CurrentHP - damage);
            Source.HealthPoints = CurrentHP; // Sync back to CardData
        }

        public void Heal(int healAmount)
        {
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + healAmount);
            Source.HealthPoints = CurrentHP;
        }

        public void AddShield(int shieldAmount)
        {
            if (HasMutation(Mewtations.Expedition.UnstableMutation.CursedFur))
            {
                return; // Locks ability to gain shield!
            }
            Shield += shieldAmount;
        }

        public void AddDebuff(MewtationsDebuff debuff, int duration)
        {
            var existing = ActiveDebuffs.Find(d => d.Type == debuff);
            if (existing != null)
            {
                existing.Duration = Mathf.Max(existing.Duration, duration);
                existing.Stacks++;
            }
            else
            {
                ActiveDebuffs.Add(new CombatStatusEffect(debuff, duration));
            }
        }

        public bool HasDebuff(MewtationsDebuff debuff)
        {
            return ActiveDebuffs.Exists(d => d.Type == debuff && d.Duration > 0);
        }

        public void TickDebuffs(Action<string> logCallback)
        {
            for (int i = ActiveDebuffs.Count - 1; i >= 0; i--)
            {
                var debuff = ActiveDebuffs[i];
                if (debuff.Duration <= 0)
                {
                    ActiveDebuffs.RemoveAt(i);
                    continue;
                }

                switch (debuff.Type)
                {
                    case MewtationsDebuff.Burning:
                        int burnDamage = 3 * debuff.Stacks;
                        TakeDamage(burnDamage);
                        logCallback?.Invoke($"{Name} nhận {burnDamage} sát thương Thiêu Đốt ({debuff.Duration} lượt còn lại).");
                        break;
                    case MewtationsDebuff.Poisoned:
                        int poisonDamage = 2 * debuff.Stacks; // Scales strongly with stacks
                        TakeDamage(poisonDamage);
                        logCallback?.Invoke($"{Name} nhận {poisonDamage} sát thương Kịch Độc ({debuff.Stacks} tầng độc).");
                        break;
                }

                debuff.Duration--;
                if (debuff.Duration <= 0)
                {
                    ActiveDebuffs.RemoveAt(i);
                }
            }

            // Apply Lethargic Nap end-of-round healing
            if (HasMutation(Mewtations.Expedition.UnstableMutation.LethargicNap) && IsAlive)
            {
                Heal(5);
                logCallback?.Invoke($"💤 {Name} đang ngái ngủ tự hồi phục 5 HP dưỡng thương.");
            }
        }
    }

    public static class MewtationsWeaponRegistry
    {
        public static WeaponAttackPattern GetAttackPattern(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId)) return WeaponAttackPattern.Single;

            if (weaponId.Contains("spear")) return WeaponAttackPattern.ColumnAttack;
            if (weaponId.Contains("club") || weaponId.Contains("hammer")) return WeaponAttackPattern.Cleave;
            if (weaponId.Contains("bow")) return WeaponAttackPattern.RageDrain;
            if (weaponId.Contains("sword")) return WeaponAttackPattern.RageGain;
            if (weaponId.Contains("wand") || weaponId.Contains("lôi")) return WeaponAttackPattern.Row;

            return WeaponAttackPattern.Single;
        }

        public static void ExecuteBasicAttack(CombatUnit attacker, CombatUnit target, List<CombatUnit> allTargets, Action<string> logCallback)
        {
            var pattern = GetAttackPattern(attacker.Source.GetEquipableOfEquipableType(EquipableType.Weapon)?.Id);
            if (attacker.HasTrait(Mewtations.Expedition.HeavenlyTalent.MartialArtsCleave))
            {
                pattern = WeaponAttackPattern.Cleave;
            }

            int baseDamage = attacker.Source.ProcessedCombatStats.AttackDamage;

            // Check Spiritual Backlash (Tẩu Hỏa Nhập Ma) if attacker has >= 2 active mutations
            bool isSpiritualBacklash = false;
            if (attacker.Source is CatCardData catData && catData.ActiveMutations.Count >= 2)
            {
                isSpiritualBacklash = true;
                baseDamage = Mathf.RoundToInt(baseDamage * 1.5f);
            }

            // Apply UnstableClaws damage boost
            if (attacker.HasMutation(Mewtations.Expedition.UnstableMutation.UnstableClaws))
            {
                baseDamage = Mathf.RoundToInt(baseDamage * 1.3f);
            }

            // Apply Shocked extra damage
            if (target.HasDebuff(MewtationsDebuff.Shocked))
            {
                baseDamage = Mathf.RoundToInt(baseDamage * 1.3f);
            }

            switch (pattern)
            {
                case WeaponAttackPattern.Single:
                    target.TakeDamage(baseDamage);
                    logCallback?.Invoke($"{attacker.Name} tấn công {target.Name} gây {baseDamage} sát thương.");
                    target.CurrentRage = Mathf.Min(145, target.CurrentRage + 10); // Target gains Rage on hit
                    break;

                case WeaponAttackPattern.ColumnAttack:
                    // Hits target and the unit behind/in front of it
                    int targetCol = target.SlotIndex % 3;
                    foreach (var unit in allTargets)
                    {
                        if (unit.IsAlive && (unit.SlotIndex % 3 == targetCol))
                        {
                            unit.TakeDamage(baseDamage);
                            logCallback?.Invoke($"{attacker.Name} đâm thương hàng dọc vào {unit.Name} gây {baseDamage} sát thương.");
                            unit.CurrentRage = Mathf.Min(145, unit.CurrentRage + 10);
                        }
                    }
                    break;

                case WeaponAttackPattern.Cleave:
                    // Hits target and adjacent horizontal units
                    int rowStart = target.SlotIndex < 3 ? 0 : 3;
                    int col = target.SlotIndex % 3;
                    foreach (var unit in allTargets)
                    {
                        if (unit.IsAlive && (unit.SlotIndex >= rowStart && unit.SlotIndex < rowStart + 3))
                        {
                            int diff = Mathf.Abs((unit.SlotIndex % 3) - col);
                            if (diff <= 1)
                            {
                                int dmg = diff == 0 ? baseDamage : Mathf.Max(1, baseDamage / 2);
                                unit.TakeDamage(dmg);
                                logCallback?.Invoke($"{attacker.Name} quẹt búa lan trúng {unit.Name} gây {dmg} sát thương.");
                                unit.CurrentRage = Mathf.Min(145, unit.CurrentRage + 10);
                            }
                        }
                    }
                    break;

                case WeaponAttackPattern.RageDrain:
                    target.TakeDamage(baseDamage);
                    int drained = Mathf.Min(target.CurrentRage, 30);
                    target.CurrentRage -= drained;
                    logCallback?.Invoke($"{attacker.Name} bắn cung hút nộ {target.Name} gây {baseDamage} sát thương và giảm {drained} Nộ.");
                    break;

                case WeaponAttackPattern.RageGain:
                    target.TakeDamage(baseDamage);
                    attacker.CurrentRage = Mathf.Min(145, attacker.CurrentRage + 20); // Extra rage for self
                    logCallback?.Invoke($"{attacker.Name} chém kiếm kích nộ vào {target.Name} gây {baseDamage} sát thương và tích thêm 20 Nộ.");
                    target.CurrentRage = Mathf.Min(145, target.CurrentRage + 10);
                    break;

                case WeaponAttackPattern.Row:
                    // Hits entire row (front or back)
                    int targetRowStart = target.SlotIndex < 3 ? 0 : 3;
                    foreach (var unit in allTargets)
                    {
                        if (unit.IsAlive && (unit.SlotIndex >= targetRowStart && unit.SlotIndex < targetRowStart + 3))
                        {
                            unit.TakeDamage(baseDamage);
                            logCallback?.Invoke($"{attacker.Name} gọi sét quét hàng ngang trúng {unit.Name} gây {baseDamage} sát thương.");
                            unit.CurrentRage = Mathf.Min(145, unit.CurrentRage + 10);
                        }
                    }
                    break;
            }

            // Apply HeavenlyPoisonBody to target
            if (attacker.HasTrait(Mewtations.Expedition.HeavenlyTalent.HeavenlyPoisonBody) && target.IsAlive)
            {
                target.AddDebuff(MewtationsDebuff.Poisoned, 3);
                logCallback?.Invoke($"☠️ Đòn đánh của {attacker.Name} tẩm độc linh lực, gây trúng độc lên {target.Name}!");
            }

            // Apply RageOvercharger
            if (attacker.HasTrait(Mewtations.Expedition.HeavenlyTalent.RageOvercharger) && attacker.IsAlive)
            {
                attacker.CurrentRage = Mathf.Min(145, attacker.CurrentRage + 10);
                logCallback?.Invoke($"⚡ {attacker.Name} kích hoạt Nộ Khí Cuồng Triều, nhận thêm 10 Nộ khí!");
            }

            // Apply UnstableClaws self-damage
            if (attacker.HasMutation(Mewtations.Expedition.UnstableMutation.UnstableClaws) && attacker.IsAlive)
            {
                attacker.TakeDamage(2);
                logCallback?.Invoke($"☣️ {attacker.Name} bị đột biến tự phế kinh mạch, hao tổn 2 HP!");
            }

            // Apply Spiritual Backlash (Tẩu Hỏa Nhập Ma) self-damage
            if (isSpiritualBacklash && attacker.IsAlive)
            {
                attacker.TakeDamage(4);
                logCallback?.Invoke($"☣️ [TẨU HỎA NHẬP MA] Sức mạnh biến dị quá tải bùng nổ! {attacker.Name} gánh chịu 4 sát thương linh lực phản phệ!");
            }
        }
    }

    public static class MewtationsUltimateRegistry
    {
        public static UltimateType GetUltimateType(CatCardData cat)
        {
            if (cat == null || !cat.HasFoodSlot) return UltimateType.DefaultBasicBoost;

            // Thức ăn lắp trong slot quyết định Ultimate
            var food = cat.GetEquipableOfEquipableType(EquipableType.Food);
            if (food == null) return UltimateType.DefaultBasicBoost;

            string id = food.Id;
            if (id.Contains("stew") || id.Contains("soup")) return UltimateType.HealLowest;
            if (id.Contains("meat") || id.Contains("chili")) return UltimateType.AoeFireBurn;
            if (id.Contains("bread") || id.Contains("omelette")) return UltimateType.ShieldTeam;
            if (id.Contains("berry") || id.Contains("ice")) return UltimateType.DisruptStun;

            return UltimateType.DefaultBasicBoost;
        }

        public static void ExecuteUltimate(CombatUnit attacker, List<CombatUnit> allies, List<CombatUnit> enemies, Action<string> logCallback)
        {
            float rageMultiplier = attacker.CurrentRage / 100f; // Scale damage by Rage overflow (e.g. 1.45x)
            attacker.CurrentRage = 0; // Consume all Rage

            var cat = attacker.Source as CatCardData;
            var type = GetUltimateType(cat);

            int baseAttack = attacker.Source.ProcessedCombatStats.AttackDamage;
            int ultDamage = Mathf.RoundToInt(baseAttack * 2.0f * rageMultiplier);

            switch (type)
            {
                case UltimateType.DefaultBasicBoost:
                    // Attack single target with heavy damage
                    var target = GetPrimaryTarget(enemies);
                    if (target != null)
                    {
                        target.TakeDamage(ultDamage);
                        logCallback?.Invoke($"★ {attacker.Name} kích hoạt Bí Kỹ mặc định: gây {ultDamage} sát thương cực mạnh lên {target.Name}!");
                        target.CurrentRage = Mathf.Min(145, target.CurrentRage + 15);
                    }
                    break;

                case UltimateType.HealLowest:
                    // Heal the lowest HP ally
                    CombatUnit lowestHPAlly = null;
                    int minHP = int.MaxValue;
                    foreach (var ally in allies)
                    {
                        if (ally.IsAlive && ally.CurrentHP < minHP)
                        {
                            minHP = ally.CurrentHP;
                            lowestHPAlly = ally;
                        }
                    }
                    if (lowestHPAlly != null)
                    {
                        int healAmount = Mathf.RoundToInt(baseAttack * 3.0f * rageMultiplier);
                        lowestHPAlly.Heal(healAmount);
                        logCallback?.Invoke($"★ {attacker.Name} ăn Linh Súp kích hoạt Bí Kỹ Trị Liệu: Hồi phục {healAmount} HP cho {lowestHPAlly.Name}!");
                    }
                    break;

                case UltimateType.AoeFireBurn:
                    // Attack all enemies + burn
                    logCallback?.Invoke($"★ {attacker.Name} ăn Linh Nhục kích hoạt Bí Kỹ Hỏa Diệm: Triệu hồi hỏa triều quét toàn bộ quân địch!");
                    int aoeDmg = Mathf.RoundToInt(baseAttack * 1.2f * rageMultiplier);
                    foreach (var enemy in enemies)
                    {
                        if (enemy.IsAlive)
                        {
                            enemy.TakeDamage(aoeDmg);
                            enemy.AddDebuff(MewtationsDebuff.Burning, 2);
                            logCallback?.Invoke($" -> Gây {aoeDmg} sát thương và Thiêu Đốt lên {enemy.Name}.");
                        }
                    }
                    break;

                case UltimateType.ShieldTeam:
                    // Shield all allies
                    int shieldAmount = Mathf.RoundToInt(baseAttack * 1.5f * rageMultiplier);
                    logCallback?.Invoke($"★ {attacker.Name} kích hoạt Bí Kỹ Hộ Thể: Tạo khiên hấp thụ {shieldAmount} sát thương cho toàn đội!");
                    foreach (var ally in allies)
                    {
                        if (ally.IsAlive)
                        {
                            ally.AddShield(shieldAmount);
                        }
                    }
                    break;

                case UltimateType.DisruptStun:
                    // Disrupt enemy speed and freeze them
                    var priTarget = GetPrimaryTarget(enemies);
                    if (priTarget != null)
                    {
                        priTarget.TakeDamage(ultDamage);
                        priTarget.AddDebuff(MewtationsDebuff.Frozen, 1);
                        logCallback?.Invoke($"★ {attacker.Name} kích hoạt Bí Kỹ Băng Trảm: gây {ultDamage} sát thương và đóng băng {priTarget.Name}!");
                    }
                    break;
            }
        }

        public static CombatUnit GetPrimaryTarget(List<CombatUnit> enemies)
        {
            // Front row (slots 0, 1, 2) prioritized
            for (int i = 0; i < 3; i++)
            {
                var unit = enemies.Find(u => u.SlotIndex == i && u.IsAlive);
                if (unit != null) return unit;
            }
            // Back row (slots 3, 4, 5) secondary
            for (int i = 3; i < 6; i++)
            {
                var unit = enemies.Find(u => u.SlotIndex == i && u.IsAlive);
                if (unit != null) return unit;
            }
            // Fallback
            return enemies.Find(u => u.IsAlive);
        }
    }
}
