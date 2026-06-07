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

    public enum WeaponArchetype
    {
        None,
        Rally,
        Stun,
        Vulnerability,
        RagePierce,
        HeavyPierce,
        HeavySweep,
        Fortress
    }

    public enum WeaponPassiveEffect
    {
        None,
        StackingAttackBoost,
        LowHpDamageBoost,
        ArmorShred,
        RageGainWhenHit,
        ComboHitScaling
    }

    public enum BuffType
    {
        None,
        CCImmunity
    }

    [Serializable]
    public class CombatBuffEffect
    {
        public BuffType Type;
        public int Duration;

        public CombatBuffEffect(BuffType type, int duration)
        {
            Type = type;
            Duration = duration;
        }
    }

    public enum DamageTag
    {
        None,
        Physical,
        Fire,
        Ice,
        Lightning,
        Poison,
        Heavy,
        Slash
    }

    public enum StatusTag
    {
        None,
        Burning,
        Poisoned,
        Frozen,
        Shocked,
        Bleeding
    }

    [Serializable]
    public class ComboTrigger
    {
        public StatusTag TriggerStatus;
        public DamageTag RequiredDamage;
        public bool ConsumeOnTrigger = true;
        public float DamageMultiplier = 1.5f;

        public ComboTrigger(StatusTag status, DamageTag dmg, bool consume = true, float mult = 1.5f)
        {
            TriggerStatus = status;
            RequiredDamage = dmg;
            ConsumeOnTrigger = consume;
            DamageMultiplier = mult;
        }
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
        Shocked,
        Bleeding
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
        
        public int ATK;
        public int DEF;
        public int MaxRage;
        public GameScripts.Combat.Core.CombatAttackPattern AttackPattern;
        public CatElement Element;
        public List<GameScripts.Combat.Core.CombatSkillDefinition> CombatSkills = new List<GameScripts.Combat.Core.CombatSkillDefinition>();

        public int Stamina = 100;
        public int MaxStamina = 100;
        public bool IsExhausted = false;
        public bool HoiQuangPhanChieuTriggered = false;
        public int ExhaustionLevel = 0;

        private int _speed;
        public int Speed
        {
            get
            {
                int val = _speed;
                if (IsExhausted)
                {
                    float penalty = 0.20f + (ExhaustionLevel / 3f) * 0.10f;
                    penalty = UnityEngine.Mathf.Min(0.50f, penalty);
                    val = UnityEngine.Mathf.RoundToInt(val * (1f - penalty));
                }
                return val;
            }
            set => _speed = value;
        }

        public int GetAttackDamage()
        {
            return GameScripts.Combat.Core.CombatCalculationService.CalculateRawAttackDamage(this);
        }

        public float GetMissingHpPercent()
        {
            if (MaxHP <= 0) return 0f;
            return 1.0f - ((float)CurrentHP / MaxHP);
        }

        public bool IsPlayer;
        public int SlotIndex; // 0-2 Front, 3-5 Back
        public List<CombatStatusEffect> ActiveDebuffs = new List<CombatStatusEffect>();
        public int Shield = 0;
        public bool IsBoss = false;
        public List<CombatBuffEffect> ActiveBuffs = new List<CombatBuffEffect>();

        public void AddBuff(BuffType type, int duration)
        {
            var existing = ActiveBuffs.Find(b => b.Type == type);
            if (existing != null)
            {
                existing.Duration = UnityEngine.Mathf.Max(existing.Duration, duration);
            }
            else
            {
                ActiveBuffs.Add(new CombatBuffEffect(type, duration));
            }
        }

        public bool HasBuff(BuffType type)
        {
            return ActiveBuffs.Exists(b => b.Type == type && b.Duration > 0);
        }

        public void TickBuffs(Action<string> logCallback)
        {
            for (int i = ActiveBuffs.Count - 1; i >= 0; i--)
            {
                var buff = ActiveBuffs[i];
                buff.Duration--;
                if (buff.Duration <= 0)
                {
                    logCallback?.Invoke($"✨ Hiệu ứng có lợi {buff.Type} trên {Name} đã hết hiệu lực.");
                    ActiveBuffs.RemoveAt(i);
                }
            }
        }

        public CatRole Role = CatRole.DPS;
        public bool HasRegenTalisman = false;
        public bool HasIronWill = false;

        public bool HasTrait(string id) { return false; }
        public bool HasMutation(string id) { return false; }
        public bool HasGameplayTag(string tag) { return false; }

        public bool IsAlive => CurrentHP > 0;

        public void TakeDamage(int damage)
        {
            float def = DEF;
            float clampedDef = UnityEngine.Mathf.Clamp(def, -20f, 95f);
            float resistance = clampedDef / 100f;
            damage = UnityEngine.Mathf.RoundToInt(damage * (1f - resistance));

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

            CurrentHP = UnityEngine.Mathf.Max(0, CurrentHP - damage);
            if (Source != null) Source.HealthPoints = CurrentHP;

            if (IsPlayer && IsExhausted && !HoiQuangPhanChieuTriggered && CurrentHP > 0 && CurrentHP <= MaxHP * 0.30f)
            {
                HoiQuangPhanChieuTriggered = true;
                if (Source != null) Source.HoiQuangPhanChieuTriggered = true;
                SetRage(100);
            }
        }

        public void Heal(int healAmount)
        {
            if (IsExhausted)
            {
                healAmount = UnityEngine.Mathf.RoundToInt(healAmount * 0.50f);
            }
            if (TurnBasedCombatManager.Instance != null && TurnBasedCombatManager.Instance.CurrentRound > TurnBasedCombatManager.Instance.AntiStallRound)
            {
                healAmount = UnityEngine.Mathf.RoundToInt(healAmount * (1f - TurnBasedCombatManager.Instance.AntiStallHealPenalty));
            }
            CurrentHP = UnityEngine.Mathf.Min(MaxHP, CurrentHP + healAmount);
            if (Source != null) Source.HealthPoints = CurrentHP;
        }

        public void AddShield(int shieldAmount)
        {
            if (TurnBasedCombatManager.Instance != null && TurnBasedCombatManager.Instance.CurrentRound > TurnBasedCombatManager.Instance.AntiStallRound)
            {
                shieldAmount = UnityEngine.Mathf.RoundToInt(shieldAmount * (1f - TurnBasedCombatManager.Instance.AntiStallHealPenalty));
            }
            Shield += shieldAmount;
        }

        public void AddRage(int amount)
        {
            CurrentRage = UnityEngine.Mathf.Min(MaxRage, CurrentRage + amount);
        }

        public void RemoveRage(int amount)
        {
            CurrentRage = UnityEngine.Mathf.Max(0, CurrentRage - amount);
        }

        public void SetRage(int amount)
        {
            CurrentRage = UnityEngine.Mathf.Clamp(amount, 0, MaxRage);
        }

        public void AddDebuff(MewtationsDebuff debuff, int duration)
        {
            if (debuff == MewtationsDebuff.Frozen)
            {
                if (HasIronWill || HasBuff(BuffType.CCImmunity) || IsBoss) return;
            }

            var existing = ActiveDebuffs.Find(d => d.Type == debuff);
            if (existing != null)
            {
                existing.Duration = UnityEngine.Mathf.Max(existing.Duration, duration);
                if (debuff != MewtationsDebuff.Shocked)
                {
                    existing.Stacks++;
                }
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
                        int poisonDamage = 2 * debuff.Stacks;
                        TakeDamage(poisonDamage);
                        logCallback?.Invoke($"{Name} nhận {poisonDamage} sát thương Kịch Độc ({debuff.Stacks} tầng độc).");
                        break;
                    case MewtationsDebuff.Bleeding:
                        int bleedDamage = 4 * debuff.Stacks;
                        TakeDamage(bleedDamage);
                        logCallback?.Invoke($"{Name} nhận {bleedDamage} sát thương Chảy Máu ({debuff.Duration} lượt còn lại).");
                        break;
                }

                debuff.Duration--;
                if (debuff.Duration <= 0)
                {
                    ActiveDebuffs.RemoveAt(i);
                }
            }

            if (HasRegenTalisman && IsAlive)
            {
                Heal(3);
                logCallback?.Invoke($"💚 [BÙA HỒI PHỤC] Bùa hộ thân giúp {Name} tự động hồi phục 3 HP dưỡng thương.");
            }
        }
    }

    public static class MewtationsWeaponRegistry
    {
        public static WeaponAttackPattern GetAttackPattern(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId)) return WeaponAttackPattern.Single;

            var card = WorldManager.instance.GameDataLoader.GetCardFromId(weaponId, true) as Equipable;
            if (card != null)
            {
                return card.MewtationsAttackPattern;
            }

            return WeaponAttackPattern.Single;
        }

        public static void ExecuteBasicAttack(CombatUnit attacker, CombatUnit target, List<CombatUnit> allies, List<CombatUnit> opponents, Action<string> logCallback)
        {
            if (target == null || !target.IsAlive || attacker == null || !attacker.IsAlive) return;
            
            // 1. Raw Damage
            int rawDamage = attacker.GetAttackDamage();
            
            // Apply Efficiency (from older weapon logic, temporarily handle here or in factory)
            float efficiency = 1.0f;
            if (attacker.Source is CatCardData c) {
                var weapon = c.GetEquipableOfEquipableType(EquipableType.Weapon);
                if (weapon != null) {
                    var oldPattern = GetAttackPattern(weapon.Id);
                    if (oldPattern == WeaponAttackPattern.Cleave) efficiency = 0.5f;
                    if (oldPattern == WeaponAttackPattern.ColumnAttack || oldPattern == WeaponAttackPattern.Row) efficiency = 0.75f;
                    if (oldPattern == WeaponAttackPattern.RageDrain) efficiency = 0.60f;
                }
            }
            rawDamage = UnityEngine.Mathf.RoundToInt(rawDamage * efficiency);

            // 2. Final Damage Calculation (includes DEF, Role, etc)
            int finalDamage = GameScripts.Combat.Core.CombatCalculationService.CalculateFinalDamage(rawDamage, attacker, target);

            // --- EVENT PIPELINE HOOKS ---
            // Trigger BeforeAttack
            MewtationsEventPipeline.TriggerBeforeAttack(attacker, target, ref finalDamage, logCallback);

            // Check TaMaLaoTo, KhoHanhTang, HonLoanTrieu
            if (attacker.Source is CatCardData c2)
            {
                if (c2.Constitution == CatConstitution.HonLoanTrieu)
                {
                    if (UnityEngine.Random.value <= 0.10f)
                    {
                        logCallback?.Invoke($"💢 [HỖN LOẠN TRIỀU] Sự điên loạn vượt kiểm soát! {attacker.Name} tự cào cấu chính mình (-2 HP)!");
                        attacker.TakeDamage(2);
                        return; // Action interrupted
                    }
                }
                if (c2.Constitution == CatConstitution.KhoHanhTang && attacker.CurrentHP <= (attacker.MaxHP * 0.30f))
                {
                    finalDamage = UnityEngine.Mathf.RoundToInt(finalDamage * 1.5f);
                }
                if (c2.Constitution == CatConstitution.TaMaLaoTo && Mewtations.Expedition.ExpeditionManager.Instance != null && Mewtations.Expedition.ExpeditionManager.Instance.RunState != null && Mewtations.Expedition.ExpeditionManager.Instance.RunState.CorruptionLevel >= 50)
                {
                    finalDamage = UnityEngine.Mathf.RoundToInt(finalDamage * 1.5f);
                }
            }

            // BeforeDamage
            MewtationsEventPipeline.TriggerBeforeDamage(target, attacker, ref finalDamage, logCallback);

            // Apply Damage
            target.TakeDamage(finalDamage);

            // UI Log
            if (finalDamage > 0)
            {
                logCallback?.Invoke($"⚔️ {attacker.Name} tấn công {target.Name} gây {finalDamage} sát thương.");
            }
            else
            {
                logCallback?.Invoke($"🛡️ {target.Name} cản được toàn bộ đòn đánh từ {attacker.Name}!");
            }

            // Post-Attack Effects
            attacker.AddRage(15);
            target.AddRage(10);
            
            // Apply Pill Effects
            if (attacker.Source is CatCardData catSource)
            {
                var pills = catSource.GetAllEquipables().Where(eq => eq.IsCultivationPill && !eq.IsBreakthroughPill).ToList();
                foreach (var pill in pills)
                {
                    // Placeholder for Pill Effects (Dual Pill mutation will have 2 pills in this list)
                    // e.g., MewtationsPillRegistry.ApplyPillEffect(pill, attacker, target, logCallback);
                }
            }
            
            // Post hooks
            MewtationsEventPipeline.TriggerAfterAttack(attacker, target, finalDamage, logCallback);
            MewtationsEventPipeline.TriggerAfterDamage(target, attacker, finalDamage, logCallback);
            
            if (!target.IsAlive)
            {
                MewtationsEventPipeline.TriggerOnKill(attacker, target, logCallback);
                MewtationsEventPipeline.TriggerOnDeath(target, logCallback);
            }
        }
    }
    [System.Obsolete("Legacy Ultimate System. Use CombatSkillExecutor instead.")]
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
            var cat = attacker.Source as CatCardData;
            if (cat != null && (cat.IsUltimateLocked || cat.HasScar(Mewtations.Combat.PermanentScar.CursedMeridians)))
            {
                logCallback?.Invoke($"[KHÓA KỸ NĂNG] Kỹ năng Nộ của {attacker.Name} đã bị khóa do bị nguyền rủa hoặc phế ấn! Thi triển Ultimate thất bại!");
                attacker.CurrentRage = 0; // Consume the Rage as backfire/dissipated energy
                var target = CombatTargetResolver.GetPrimaryTarget(enemies, attacker);
                if (target != null)
                {
                    MewtationsWeaponRegistry.ExecuteBasicAttack(attacker, target, allies, enemies, logCallback);
                }
                return;
            }

            float rageMultiplier = attacker.CurrentRage / 100f; // Scale damage by Rage overflow (e.g. 1.45x)
            attacker.CurrentRage = 0; // Consume all Rage

            var type = GetUltimateType(cat);

            int baseAttack = attacker.GetAttackDamage();

            // Apply Role damage multiplier to Ultimate damage as well!
            float roleDmgMultiplier = 1.0f;
            if (attacker.Role == CatRole.DPS)
            {
                roleDmgMultiplier += 0.20f;
                var target = CombatTargetResolver.GetPrimaryTarget(enemies, attacker);
                if (target != null && target.ActiveDebuffs.Exists(d => d.Duration > 0))
                {
                    roleDmgMultiplier += 0.25f;
                }
            }
            else if (attacker.Role == CatRole.Attrition)
            {
                int currentRound = (TurnBasedCombatManager.Instance != null) ? TurnBasedCombatManager.Instance.CurrentRound : 1;
                roleDmgMultiplier += currentRound * 0.10f;
            }

            // Calculate mutations damage multiplier
            float mutDmgMultiplier = 1.0f;
            bool isSpiritualBacklash = false;
            if (attacker.Source is CatCardData catData && catData.ActiveMutations.Count >= 2)
            {
                isSpiritualBacklash = true;
                mutDmgMultiplier *= 1.5f;
            }
            // Apply UnstableClaws damage boost removed (now dynamically handled in BeforeAttack hook)

            int ultDamage = Mathf.RoundToInt(baseAttack * 2.0f * rageMultiplier * roleDmgMultiplier * mutDmgMultiplier);

            // --- EVENT PIPELINE HOOKS & CONSTITUTIONS FOR ULTIMATE ---

            // 1. Trigger BeforeAttack Event hooks on Ultimate damage
            MewtationsEventPipeline.TriggerBeforeAttack(attacker, null, ref ultDamage, logCallback);

            // 2. High Corruption Scaling (Tà Ma Lão Tổ) constitution check
            if (attacker.Source is CatCardData c && c.Constitution == CatConstitution.TaMaLaoTo && ExpeditionManager.Instance != null && ExpeditionManager.Instance.RunState != null && ExpeditionManager.Instance.RunState.CorruptionLevel >= 50)
            {
                ultDamage = Mathf.RoundToInt(ultDamage * 1.5f);
            }

            // 3. Low Stability Genius (Hỗn Loạn Triều) constitution check
            if (attacker.Source is CatCardData catHL && catHL.Constitution == CatConstitution.HonLoanTrieu)
            {
                if (UnityEngine.Random.value <= 0.10f)
                {
                    logCallback?.Invoke($"💢 [HỖN LOẠN TRIỀU] Bí kỹ hỗn loạn thất bại! {attacker.Name} tự gây phản phệ tổn thương chính mình (-3 HP)!");
                    attacker.TakeDamage(3);
                    return; // Action interrupted!
                }
            }

            // 4. Cursed Survivor (Khổ Hạnh Tăng) constitution check
            if (attacker.Source is CatCardData catK && catK.Constitution == CatConstitution.KhoHanhTang && attacker.CurrentHP <= (attacker.MaxHP * 0.30f))
            {
                ultDamage = Mathf.RoundToInt(ultDamage * 1.5f);
            }

            // Collect hit enemies for elemental modifiers application
            List<CombatUnit> hitEnemies = new List<CombatUnit>();

            switch (type)
            {
                case UltimateType.DefaultBasicBoost:
                    // Attack single target with heavy damage
                    var target = CombatTargetResolver.GetPrimaryTarget(enemies, attacker);
                    if (target != null)
                    {
                        int finalUltDmg = ultDamage;
                        if (target.HasDebuff(MewtationsDebuff.Shocked))
                        {
                            finalUltDmg = Mathf.RoundToInt(finalUltDmg * 1.5f);
                            target.ActiveDebuffs.RemoveAll(d => d.Type == MewtationsDebuff.Shocked);
                            logCallback?.Invoke($"⚡💥 [KÍCH NỔ TRỌNG THƯƠNG] Bí kỹ Ultimate của {attacker.Name} kích nổ ấn ký Shocked trên {target.Name}! Gây +50% Sát thương bạo kích cực đại!");
                        }
                        target.TakeDamage(finalUltDmg);
                        logCallback?.Invoke($"★ {attacker.Name} kích hoạt Bí Kỹ mặc định: gây {finalUltDmg} sát thương cực mạnh lên {target.Name}!");
                        target.CurrentRage = Mathf.Min(145, target.CurrentRage + 15);
                        hitEnemies.Add(target);
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
                    int aoeDmg = Mathf.RoundToInt(baseAttack * 1.2f * rageMultiplier * roleDmgMultiplier * mutDmgMultiplier);
                    foreach (var enemy in enemies)
                    {
                        if (enemy.IsAlive)
                        {
                            int finalAoeDmg = aoeDmg;
                            if (enemy.HasDebuff(MewtationsDebuff.Shocked))
                            {
                                finalAoeDmg = Mathf.RoundToInt(finalAoeDmg * 1.5f);
                                enemy.ActiveDebuffs.RemoveAll(d => d.Type == MewtationsDebuff.Shocked);
                                logCallback?.Invoke($"⚡💥 [KÍCH NỔ TRỌNG THƯƠNG] Hỏa triều Ultimate của {attacker.Name} kích nổ ấn ký Shocked trên {enemy.Name}! Gây +50% Sát thương bạo kích cực đại!");
                            }
                            enemy.TakeDamage(finalAoeDmg);
                            enemy.AddDebuff(MewtationsDebuff.Burning, 2);
                            logCallback?.Invoke($" -> Gây {finalAoeDmg} sát thương và Thiêu Đốt lên {enemy.Name}.");
                            hitEnemies.Add(enemy);
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
                    var priTarget = CombatTargetResolver.GetPrimaryTarget(enemies, attacker);
                    if (priTarget != null)
                    {
                        int finalUltDmg = ultDamage;
                        if (priTarget.HasDebuff(MewtationsDebuff.Shocked))
                        {
                            finalUltDmg = Mathf.RoundToInt(finalUltDmg * 1.5f);
                            priTarget.ActiveDebuffs.RemoveAll(d => d.Type == MewtationsDebuff.Shocked);
                            logCallback?.Invoke($"⚡💥 [KÍCH NỔ TRỌNG THƯƠNG] Băng Trảm Ultimate của {attacker.Name} kích nổ ấn ký Shocked trên {priTarget.Name}! Gây +50% Sát thương bạo kích cực đại!");
                        }
                        priTarget.TakeDamage(finalUltDmg);
                        priTarget.AddDebuff(MewtationsDebuff.Frozen, 1);
                        logCallback?.Invoke($"★ {attacker.Name} kích hoạt Bí Kỹ Băng Trảm: gây {finalUltDmg} sát thương và đóng băng {priTarget.Name}!");
                        hitEnemies.Add(priTarget);
                    }
                    break;
            }

            // Trigger AfterAttack & AfterDamage pipeline hooks for Ultimate on all hit enemies
            foreach (var enemy in hitEnemies)
            {
                int tempDmg = ultDamage;
                MewtationsEventPipeline.TriggerBeforeDamage(enemy, attacker, ref tempDmg, logCallback);
                
                MewtationsEventPipeline.TriggerAfterAttack(attacker, enemy, tempDmg, logCallback);
                MewtationsEventPipeline.TriggerAfterDamage(enemy, attacker, tempDmg, logCallback);

                if (!enemy.IsAlive)
                {
                    MewtationsEventPipeline.TriggerOnKill(attacker, enemy, logCallback);
                    MewtationsEventPipeline.TriggerOnDeath(enemy, logCallback);
                }
            }

            // Apply Element Behavior Modifiers for Ultimate Skills on hit enemies
            if (hitEnemies.Count > 0)
            {
                foreach (var enemy in hitEnemies)
                {
                    if (!enemy.IsAlive) continue;

                    if (attacker.Element == CatElement.Fire)
                    {
                        enemy.AddDebuff(MewtationsDebuff.Burning, 3);
                        logCallback?.Invoke($"🔥 [HỎA BỘI] Bí Kỹ của {attacker.Name} gây Thiêu Đốt mạnh (3 lượt) lên {enemy.Name}!");
                    }
                    else if (attacker.Element == CatElement.Poison)
                    {
                        var poisonDebuff = enemy.ActiveDebuffs.Find(d => d.Type == MewtationsDebuff.Poisoned);
                        if (poisonDebuff != null)
                        {
                            poisonDebuff.Stacks *= 2;
                            poisonDebuff.Duration = Mathf.Max(poisonDebuff.Duration, 3);
                            logCallback?.Invoke($"☠️ [KỊCH ĐỘC] Bí Kỹ Độc tính phát tác! Nhân đôi số tầng độc và làm mới thời gian tác dụng (3 lượt) trên {enemy.Name} (Hiện tại: {poisonDebuff.Stacks} tầng)!");
                        }
                        else
                        {
                            enemy.AddDebuff(MewtationsDebuff.Poisoned, 3);
                            enemy.AddDebuff(MewtationsDebuff.Poisoned, 3);
                            logCallback?.Invoke($"☠️ [KỊCH ĐỘC] Bí Kỹ tiêm 2 tầng kịch độc cực mạnh vào {enemy.Name}!");
                        }
                    }
                    else if (attacker.Element == CatElement.Ice)
                    {
                        enemy.AddDebuff(MewtationsDebuff.Frozen, 1);
                        logCallback?.Invoke($"❄️ [BĂNG PHONG] Bí Kỹ Tuyết vực đóng băng hoàn toàn {enemy.Name}!");
                    }
                    else if (attacker.Element == CatElement.Lightning)
                    {
                        bool wasShocked = enemy.HasDebuff(MewtationsDebuff.Shocked);
                        enemy.AddDebuff(MewtationsDebuff.Shocked, 2);
                        logCallback?.Invoke($"⚡ [LÔI HOÀNH] Bí Kỹ Lôi Điện làm tê liệt hoàn toàn {enemy.Name} (+30% sát thương nhận vào)!");
                        if (wasShocked)
                        {
                            attacker.CurrentRage = Mathf.Min(145, attacker.CurrentRage + 10);
                            logCallback?.Invoke($"⚡ [LÔI CHẤN] {attacker.Name} kích hoạt Lôi Chấn trên mục tiêu bị Điện Giật, hấp thụ hạt sét phục hồi +10 Nộ khí!");
                        }
                    }
                }
            }

            // Apply HeavenlyPoisonBody to hit enemies from Ultimate
            if (attacker.HasTrait(Mewtations.Expedition.HeavenlyTalent.HeavenlyPoisonBody) && hitEnemies.Count > 0)
            {
                foreach (var enemy in hitEnemies)
                {
                    if (enemy.IsAlive)
                    {
                        enemy.AddDebuff(MewtationsDebuff.Poisoned, 3);
                        logCallback?.Invoke($"☠️ Bí Kỹ của {attacker.Name} tẩm độc linh lực, gây trúng độc lên {enemy.Name}!");
                    }
                }
            }

            // Apply RageOvercharger for Ultimate
            if (attacker.HasTrait(Mewtations.Expedition.HeavenlyTalent.RageOvercharger) && attacker.IsAlive)
            {
                attacker.CurrentRage = Mathf.Min(145, attacker.CurrentRage + 10);
                logCallback?.Invoke($"⚡ {attacker.Name} kích hoạt Nộ Khí Cuồng Triều từ Bí Kỹ, nhận thêm 10 Nộ khí!");
            }

            // Apply UnstableClaws self-damage for Ultimate
            if (attacker.HasMutation(Mewtations.Expedition.UnstableMutation.UnstableClaws) && attacker.IsAlive)
            {
                attacker.TakeDamage(2);
                logCallback?.Invoke($"☣️ {attacker.Name} bị đột biến tự phế kinh mạch sau Bí Kỹ, hao tổn 2 HP!");
            }

            // Apply Spiritual Backlash (Tẩu Hỏa Nhập Ma) self-damage for Ultimate
            if (isSpiritualBacklash && attacker.IsAlive)
            {
                attacker.TakeDamage(4);
                logCallback?.Invoke($"☣️ [TẨU HỎA NHẬP MA] Sức mạnh biến dị quá tải bùng nổ sau Bí Kỹ! {attacker.Name} gánh chịu 4 sát thương linh lực phản phệ!");
            }

            // Apply Role Specializations (ShieldSupport, RageSupport) for Ultimate Skills
            if (attacker.Role == CatRole.ShieldSupport && allies != null)
            {
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
                    lowestHPAlly.AddShield(5);
                    logCallback?.Invoke($"🛡️ [HỘ THỂ] Hỗ trợ {attacker.Name} ban tặng +5 Khiên bảo vệ cho {lowestHPAlly.Name}!");
                }
            }
            else if (attacker.Role == CatRole.RageSupport && allies != null)
            {
                foreach (var ally in allies)
                {
                    if (ally.IsAlive && ally != attacker)
                    {
                        ally.CurrentRage = Mathf.Min(145, ally.CurrentRage + 10);
                        logCallback?.Invoke($"⚡ [CỔ VŨ] {attacker.Name} truyền năng lượng, giúp đồng đội {ally.Name} nhận +10 Nộ!");
                    }
                }
            }

            // Apply BrokenFireVein backfire for Ultimate
            bool usesFireUlt = false;
            var weaponUlt = attacker.Source.GetEquipableOfEquipableType(EquipableType.Weapon);
            if (weaponUlt != null && (weaponUlt.Id.ToLower().Contains("fire") || weaponUlt.Id.ToLower().Contains("hỏa") || weaponUlt.Id.ToLower().Contains("hoa")))
            {
                usesFireUlt = true;
            }
            if (attacker.Element == CatElement.Fire)
            {
                usesFireUlt = true;
            }
            if (usesFireUlt && attacker.HasTrait(Mewtations.Combat.PermanentScar.BrokenFireVein) && attacker.IsAlive)
            {
                attacker.TakeDamage(2);
                logCallback?.Invoke($"🔥 [HỎA MẠCH ĐỨT GÃY] {attacker.Name} sử dụng Bí kỹ hệ Hỏa khi đang bị Đứt Hỏa Mạch! Tự chịu phản phệ -2 HP!");
            }
        }

        }
    }
}
