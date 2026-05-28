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
                    float penalty = 0.20f + (ExhaustionLevel / 3) * 0.10f;
                    penalty = Mathf.Min(0.50f, penalty);
                    val = Mathf.RoundToInt(val * (1f - penalty));
                }
                return val;
            }
            set => _speed = value;
        }

        public int GetAttackDamage()
        {
            int baseDamage = Source != null ? Source.ProcessedCombatStats.AttackDamage : 10;
            if (IsExhausted)
            {
                float penalty = 0.20f + (ExhaustionLevel / 3) * 0.10f;
                penalty = Mathf.Min(0.50f, penalty);
                baseDamage = Mathf.RoundToInt(baseDamage * (1f - penalty));
            }

            // Check for Scaling Passive Talisman
            if (Source is CatCardData cat)
            {
                bool hasScalingPassive = false;
                if (cat.EquipmentSlots != null)
                {
                    if (cat.EquipmentSlots.ContainsKey(Mewtations.Expedition.CatSlotType.Passive1))
                    {
                        var p1 = cat.EquipmentSlots[Mewtations.Expedition.CatSlotType.Passive1].EquippedItem;
                        if (p1 != null && p1 is Mewtations.Cards.Data.ScalingPassiveTalisman) hasScalingPassive = true;
                    }
                    if (cat.EquipmentSlots.ContainsKey(Mewtations.Expedition.CatSlotType.Passive2))
                    {
                        var p2 = cat.EquipmentSlots[Mewtations.Expedition.CatSlotType.Passive2].EquippedItem;
                        if (p2 != null && p2 is Mewtations.Cards.Data.ScalingPassiveTalisman) hasScalingPassive = true;
                    }
                }

                if (hasScalingPassive && TurnBasedCombatManager.Instance != null)
                {
                    int currentRound = TurnBasedCombatManager.Instance.CurrentRound;
                    if (currentRound >= 6)
                    {
                        float bonus = 0.05f * (currentRound - 5);
                        baseDamage = Mathf.RoundToInt(baseDamage * (1f + bonus));
                    }
                }
            }

            return baseDamage;
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
                existing.Duration = Mathf.Max(existing.Duration, duration);
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
        public CatElement Element = CatElement.None;
        public bool HasRegenTalisman = false;
        public bool HasIronWill = false;

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

        private static readonly Dictionary<string, List<string>> _idToTags = new Dictionary<string, List<string>>
        {
            { "talisman_heavy_armor", new List<string> { "HeavyArmor", "FireResist" } },
            { "talisman_iron_will", new List<string> { "IronWill", "FireResist", "SwampAdapted" } },
            { "talisman_health_regen", new List<string> { "HealthRegen", "SwampAdapted" } }
        };

        private List<string> GetTagsForId(string id)
        {
            if (string.IsNullOrEmpty(id)) return new List<string>();
            string lower = id.ToLower();
            if (_idToTags.TryGetValue(lower, out var tags))
            {
                return tags;
            }
            return new List<string>();
        }

        public bool HasGameplayTag(string tag)
        {
            if (Source is CatCardData cat)
            {
                // Equipment
                var allEquipables = cat.GetAllEquipables();
                foreach (var eq in allEquipables)
                {
                    if (eq != null && GetTagsForId(eq.Id).Contains(tag)) return true;
                }
                // Traits
                foreach (var trait in cat.PermanentTraits)
                {
                    if (GetTagsForId(trait).Contains(tag)) return true;
                }
                // Mutations
                foreach (var mut in cat.ActiveMutations)
                {
                    if (GetTagsForId(mut).Contains(tag)) return true;
                }
            }
            return false;
        }

        public CombatUnit(Combatable source, bool isPlayer, int slotIndex)
        {
            Source = source;
            Name = source.Name;
            IsPlayer = isPlayer;
            SlotIndex = slotIndex;

            if (source != null)
            {
                IsBoss = source.IsBoss;
            }

            // Extract base stats
            MaxHP = source.ProcessedCombatStats.MaxHealth;
            CurrentHP = source.HealthPoints;
            // Removed Necromancy fallback: Paralyzed cats (if they somehow get in) will stay at 1 HP, dead enemies stay dead.
            
            // Extract Mewtations-specific stats
            if (source is CatCardData cat)
            {
                Speed = cat.Speed;
                CurrentRage = cat.CurrentRage;
                Role = cat.Role;
                Element = cat.Element;

                Stamina = cat.Stamina;
                MaxStamina = cat.MaxStamina;
                IsExhausted = cat.IsExhausted;
                HoiQuangPhanChieuTriggered = cat.HoiQuangPhanChieuTriggered;
                ExhaustionLevel = cat.ExhaustionLevel;

                // Load all dynamic components
                var activeComps = new List<IMewtationsComponent>();

                // Permanent Traits
                foreach (var traitId in cat.PermanentTraits)
                {
                    var comp = MewtationsComponentRegistry.Create(traitId);
                    if (comp != null) activeComps.Add(comp);
                }

                // Temporary Mutations
                foreach (var mutId in cat.ActiveMutations)
                {
                    var comp = MewtationsComponentRegistry.Create(mutId);
                    if (comp != null) activeComps.Add(comp);
                }

                // Permanent Scars
                foreach (var scarId in cat.PermanentScars)
                {
                    var comp = MewtationsComponentRegistry.Create(scarId);
                    if (comp != null) activeComps.Add(comp);
                }

                // Dao Specializations
                if (cat.Specialization != Cards.Cats.DaoSpecialization.None)
                {
                    var comp = Cards.Cats.CultivationSpecializationRegistry.CreateComponent(cat.Specialization);
                    if (comp != null) activeComps.Add(comp);
                }

                // Equipped Talismans
                var allEquipables = cat.GetAllEquipables();
                foreach (var eq in allEquipables)
                {
                    if (eq != null && eq.EquipableType == EquipableType.Talisman)
                    {
                        var comp = MewtationsComponentRegistry.Create(eq.Id);
                        if (comp != null) activeComps.Add(comp);
                    }
                }

                // Register inside pipeline
                MewtationsEventPipeline.RegisterUnitComponents(this, activeComps);
            }
            else
            {
                // Default stats for enemies
                Speed = 100;
                CurrentRage = 0;
                Role = CatRole.DPS;
                Element = CatElement.None;
                MewtationsEventPipeline.RegisterUnitComponents(this, new List<IMewtationsComponent>());
            }
        }

        public bool IsAlive => CurrentHP > 0;

        public void TakeDamage(int damage)
        {
            var weapon = Source.GetEquipableOfEquipableType(EquipableType.Weapon) as Equipable;
            if (weapon != null && weapon.DamageResistance > 0f)
            {
                damage = Mathf.RoundToInt(damage * (1f - weapon.DamageResistance));
            }

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

            // Hồi Quang Phản Chiếu: Khi mèo Thiên Kiêu kiệt sức tàn huyết <= 30% HP
            if (IsPlayer && IsExhausted && !HoiQuangPhanChieuTriggered && CurrentHP > 0 && CurrentHP <= MaxHP * 0.30f)
            {
                if (Source is CatCardData cat && cat.Constitution == CatConstitution.BaoLinhThienKieu)
                {
                    HoiQuangPhanChieuTriggered = true;
                    cat.HoiQuangPhanChieuTriggered = true;
                    CurrentRage = 100;
                    if (TurnBasedCombatManager.Instance != null)
                    {
                        TurnBasedCombatManager.Instance.AddLog($"🔥 [HỒI QUANG PHẢN CHIẾU] Thiên kiêu {Name} tàn huyết bùng nổ! Hồi phục 100 NỘ KHÍ chuẩn bị phản kích cực hạn!");
                    }
                }
            }
        }

        public void Heal(int healAmount)
        {
            if (IsExhausted)
            {
                healAmount = Mathf.RoundToInt(healAmount * 0.50f);
            }
            if (TurnBasedCombatManager.Instance != null && TurnBasedCombatManager.Instance.CurrentRound > TurnBasedCombatManager.Instance.AntiStallRound)
            {
                healAmount = Mathf.RoundToInt(healAmount * (1f - TurnBasedCombatManager.Instance.AntiStallHealPenalty));
            }
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + healAmount);
            Source.HealthPoints = CurrentHP;
        }

        public void AddShield(int shieldAmount)
        {
            if (HasMutation(Mewtations.Expedition.UnstableMutation.CursedFur))
            {
                return; // Locks ability to gain shield!
            }
            if (TurnBasedCombatManager.Instance != null && TurnBasedCombatManager.Instance.CurrentRound > TurnBasedCombatManager.Instance.AntiStallRound)
            {
                shieldAmount = Mathf.RoundToInt(shieldAmount * (1f - TurnBasedCombatManager.Instance.AntiStallHealPenalty));
            }
            Shield += shieldAmount;
        }

        public void AddDebuff(MewtationsDebuff debuff, int duration)
        {
            if (debuff == MewtationsDebuff.Frozen)
            {
                if (HasIronWill || HasBuff(BuffType.CCImmunity) || IsBoss)
                {
                    return; // Immune to freeze!
                }
            }

            if (debuff == MewtationsDebuff.Poisoned && HasTrait(Mewtations.Expedition.HeavenlyTalent.HeavenlyPoisonBody))
            {
                return; // Immune to poison!
            }

            var existing = ActiveDebuffs.Find(d => d.Type == debuff);
            if (existing != null)
            {
                existing.Duration = Mathf.Max(existing.Duration, duration);
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
                        int poisonDamage = 2 * debuff.Stacks; // Scales strongly with stacks
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

            // Apply health regen talisman
            if (HasRegenTalisman && IsAlive)
            {
                Heal(3);
                logCallback?.Invoke($"💚 [BÙA HỒI PHỤC] Bùa hộ thân giúp {Name} tự động hồi phục 3 HP dưỡng thương.");
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

            var card = WorldManager.instance.GameDataLoader.GetCardFromId(weaponId, true) as Equipable;
            if (card != null)
            {
                return card.MewtationsAttackPattern;
            }

            return WeaponAttackPattern.Single;
        }

        public static void ExecuteBasicAttack(CombatUnit attacker, CombatUnit target, List<CombatUnit> allies, List<CombatUnit> opponents, Action<string> logCallback)
        {
            var weaponCard = attacker.Source.GetEquipableOfEquipableType(EquipableType.Weapon) as Equipable;
            float efficiency = 1.0f;
            WeaponArchetype archetype = WeaponArchetype.None;
            int shieldOnAttack = 0;
            int rageOnHit = 0;

            if (weaponCard != null)
            {
                efficiency = weaponCard.OutputEfficiency;
                archetype = weaponCard.WeaponArchetype;
                shieldOnAttack = weaponCard.ShieldOnAttack;
                rageOnHit = weaponCard.RageOnHit;
            }

            // Tank redirection check resolved via CombatTargetResolver!
            target = CombatTargetResolver.ResolveRedirectedTarget(attacker, target, opponents, logCallback);

            if (archetype == WeaponArchetype.Rally)
            {
                int rGain = rageOnHit > 0 ? rageOnHit : 15;
                logCallback?.Invoke($"📣 [TRẬN PHÁP CỔ VŨ] {attacker.Name} sử dụng Trận Pháp Cổ Vũ! Hồi +{rGain} Nộ Khí cho toàn bộ đồng đội còn sống.");
                foreach (var ally in allies)
                {
                    if (ally.IsAlive && ally != attacker) // Không tự tăng cho bản thân!
                    {
                        ally.CurrentRage = Mathf.Min(145, ally.CurrentRage + rGain);
                    }
                }
                return;
            }

            var pattern = GetAttackPattern(weaponCard?.Id);
            if (attacker.HasTrait(Mewtations.Expedition.HeavenlyTalent.MartialArtsCleave))
            {
                pattern = WeaponAttackPattern.Cleave;
            }

            int baseDamage = attacker.GetAttackDamage();
            baseDamage = Mathf.RoundToInt(baseDamage * efficiency);

            // Apply Role damage multiplier
            float roleDmgMultiplier = 1.0f;
            if (attacker.Role == CatRole.DPS)
            {
                roleDmgMultiplier += 0.20f;
                bool hasDebuff = target.ActiveDebuffs.Exists(d => d.Duration > 0);
                if (hasDebuff)
                {
                    roleDmgMultiplier += 0.25f;
                }
            }
            else if (attacker.Role == CatRole.Attrition)
            {
                int currentRound = (TurnBasedCombatManager.Instance != null) ? TurnBasedCombatManager.Instance.CurrentRound : 1;
                roleDmgMultiplier += currentRound * 0.10f;
            }

            baseDamage = Mathf.RoundToInt(baseDamage * roleDmgMultiplier);

            // Check Spiritual Backlash (Tẩu Hỏa Nhập Ma) if attacker has >= 2 active mutations
            bool isSpiritualBacklash = false;
            if (attacker.Source is CatCardData catData && catData.ActiveMutations.Count >= 2)
            {
                isSpiritualBacklash = true;
                baseDamage = Mathf.RoundToInt(baseDamage * 1.5f);
            }

            // Apply UnstableClaws damage boost removed (now dynamically handled in BeforeAttack hook)

            // --- EVENT PIPELINE HOOKS & CONSTITUTIONS ---

            // 1. Trigger BeforeAttack Event hooks
            MewtationsEventPipeline.TriggerBeforeAttack(attacker, target, ref baseDamage, logCallback);

            // 2. High Corruption Scaling (Tà Ma Lão Tổ) constitution check
            if (attacker.Source is CatCardData c && c.Constitution == CatConstitution.TaMaLaoTo && ExpeditionManager.Instance != null && ExpeditionManager.Instance.RunState != null && ExpeditionManager.Instance.RunState.CorruptionLevel >= 50)
            {
                baseDamage = Mathf.RoundToInt(baseDamage * 1.5f);
            }

            // 3. Low Stability Genius (Hỗn Loạn Triều) constitution check
            if (attacker.Source is CatCardData catHL && catHL.Constitution == CatConstitution.HonLoanTrieu)
            {
                if (UnityEngine.Random.value <= 0.10f)
                {
                    logCallback?.Invoke($"💢 [HỖN LOẠN TRIỀU] Chiêu thức hỗn loạn thất bại! {attacker.Name} tự gây phản phệ tổn thương chính mình (-3 HP)!");
                    attacker.TakeDamage(3);
                    return; // Action interrupted!
                }
            }

            // 4. Cursed Survivor (Khổ Hạnh Tăng) constitution check
            if (attacker.Source is CatCardData catK && catK.Constitution == CatConstitution.KhoHanhTang && attacker.CurrentHP <= (attacker.MaxHP * 0.30f))
            {
                baseDamage = Mathf.RoundToInt(baseDamage * 1.5f);
            }

            // 5. Trigger Target's BeforeDamage Event hooks
            MewtationsEventPipeline.TriggerBeforeDamage(target, attacker, ref baseDamage, logCallback);

            bool isFortress = (archetype == WeaponArchetype.Fortress);
            if (isFortress)
            {
                int sGain = shieldOnAttack > 0 ? shieldOnAttack : 25;
                int rGain = rageOnHit > 0 ? rageOnHit : 15;
                attacker.AddShield(sGain);
                attacker.CurrentRage = Mathf.Min(145, attacker.CurrentRage + rGain);
                logCallback?.Invoke($"🛡️ [TRẤN THỦ THÀNH TRÌ] {attacker.Name} kích hoạt Trấn Thủ! Nhận +{sGain} Giáp và tích +{rGain} Nộ Khí.");
            }

            bool isStun = (archetype == WeaponArchetype.Stun);
            bool isVuln = (archetype == WeaponArchetype.Vulnerability);

            switch (pattern)
            {
                case WeaponAttackPattern.Single:
                    target.TakeDamage(baseDamage);
                    logCallback?.Invoke($"{attacker.Name} tấn công {target.Name} gây {baseDamage} sát thương.");
                    if (isStun && target.IsAlive)
                    {
                        if (UnityEngine.Random.value <= 0.35f)
                        {
                            target.AddDebuff(MewtationsDebuff.Frozen, 1);
                            logCallback?.Invoke($"❄️ [ĐẬP CHOÁNG] Pháp bảo của {attacker.Name} gây Choáng lên {target.Name} (1 lượt)!");
                        }
                    }
                    if (isVuln && target.IsAlive)
                    {
                        target.AddDebuff(MewtationsDebuff.Shocked, 2);
                        logCallback?.Invoke($"⚡ [TRỌNG THƯƠNG] {attacker.Name} gây Trọng Thương lên {target.Name} (Nhận +30% sát thương trong 2 lượt)!");
                    }
                    target.CurrentRage = Mathf.Min(145, target.CurrentRage + 10); // Target gains Rage on hit
                    break;

                case WeaponAttackPattern.ColumnAttack:
                    var colTargets = CombatTargetResolver.ResolvePatternTargets(pattern, target, opponents);
                    bool isRagePierce = (archetype == WeaponArchetype.RagePierce);
                    foreach (var unit in colTargets)
                    {
                        unit.TakeDamage(baseDamage);
                        logCallback?.Invoke($"{attacker.Name} đâm thương hàng dọc vào {unit.Name} gây {baseDamage} sát thương.");
                        if (isRagePierce)
                        {
                            unit.CurrentRage = Mathf.Max(0, unit.CurrentRage - 20);
                            logCallback?.Invoke($"  ↳ 📉 [GIẢM NỘ] Xuyên Nộ Thương giảm 20 Nộ khí của {unit.Name}!");
                        }
                        if (isStun && unit.IsAlive && unit == target)
                        {
                            if (UnityEngine.Random.value <= 0.35f)
                            {
                                unit.AddDebuff(MewtationsDebuff.Frozen, 1);
                                logCallback?.Invoke($"❄️ [ĐẬP CHOÁNG] Đòn đánh của {attacker.Name} gây Choáng lên {unit.Name} (1 lượt)!");
                            }
                        }
                        if (isVuln && unit.IsAlive && unit == target)
                        {
                            unit.AddDebuff(MewtationsDebuff.Shocked, 2);
                            logCallback?.Invoke($"⚡ [TRỌNG THƯƠNG] {attacker.Name} gây Trọng Thương lên {unit.Name} (Nhận +30% sát thương trong 2 lượt)!");
                        }
                        unit.CurrentRage = Mathf.Min(145, unit.CurrentRage + 10);
                    }
                    break;

                case WeaponAttackPattern.Cleave:
                    var cleaveTargets = CombatTargetResolver.ResolvePatternTargets(pattern, target, opponents);
                    foreach (var unit in cleaveTargets)
                    {
                        int diff = Mathf.Abs(CombatBattlefieldHelper.GetLane(unit.SlotIndex) - CombatBattlefieldHelper.GetLane(target.SlotIndex));
                        int dmg = diff == 0 ? baseDamage : Mathf.Max(1, baseDamage / 2);
                        unit.TakeDamage(dmg);
                        logCallback?.Invoke($"{attacker.Name} quẹt búa lan trúng {unit.Name} gây {dmg} sát thương.");
                        unit.CurrentRage = Mathf.Min(145, unit.CurrentRage + 10);
                    }
                    break;

                case WeaponAttackPattern.RageDrain:
                    target.TakeDamage(baseDamage);
                    int drained = 0;
                    if (target.HasIronWill)
                    {
                        logCallback?.Invoke($"🛡️ {target.Name} sở hữu Ý Chí Sắt Đá, miễn nhiễm hút Nộ!");
                    }
                    else
                    {
                        drained = Mathf.Min(target.CurrentRage, 30);
                        target.CurrentRage -= drained;
                        attacker.CurrentRage = Mathf.Min(145, attacker.CurrentRage + drained);
                    }
                    logCallback?.Invoke($"{attacker.Name} bắn cung hút nộ {target.Name} gây {baseDamage} sát thương, giảm {drained} Nộ của mục tiêu và nhận {drained} Nộ.");
                    break;

                case WeaponAttackPattern.RageGain:
                    target.TakeDamage(baseDamage);
                    attacker.CurrentRage = Mathf.Min(145, attacker.CurrentRage + 20); // Extra rage for self
                    logCallback?.Invoke($"{attacker.Name} chém kiếm kích nộ vào {target.Name} gây {baseDamage} sát thương và tích thêm 20 Nộ.");
                    target.CurrentRage = Mathf.Min(145, target.CurrentRage + 10);
                    break;

                case WeaponAttackPattern.Row:
                    var rowTargets = CombatTargetResolver.ResolvePatternTargets(pattern, target, opponents);
                    foreach (var unit in rowTargets)
                    {
                        unit.TakeDamage(baseDamage);
                        logCallback?.Invoke($"{attacker.Name} gọi sét quét hàng ngang trúng {unit.Name} gây {baseDamage} sát thương.");
                        unit.CurrentRage = Mathf.Min(145, unit.CurrentRage + 10);
                    }
                    break;
            }

            // Trigger AfterAttack & AfterDamage pipeline hooks
            MewtationsEventPipeline.TriggerAfterAttack(attacker, target, baseDamage, logCallback);
            MewtationsEventPipeline.TriggerAfterDamage(target, attacker, baseDamage, logCallback);

            // Trigger OnKill & OnDeath hooks if target has fallen
            if (!target.IsAlive)
            {
                MewtationsEventPipeline.TriggerOnKill(attacker, target, logCallback);
                MewtationsEventPipeline.TriggerOnDeath(target, logCallback);
            }

            // Apply Element Behavior Modifiers for Basic Attacks
            if (attacker.Element == CatElement.Fire && target.IsAlive)
            {
                if (UnityEngine.Random.value <= 0.50f)
                {
                    target.AddDebuff(MewtationsDebuff.Burning, 2);
                    logCallback?.Invoke($"🔥 [HỎA] Đòn đánh của {attacker.Name} gây Thiêu Đốt lên {target.Name}!");
                }
            }
            else if (attacker.Element == CatElement.Poison && target.IsAlive)
            {
                target.AddDebuff(MewtationsDebuff.Poisoned, 3);
                logCallback?.Invoke($"☠️ [ĐỘC] Đòn đánh của {attacker.Name} tích độc dược lên {target.Name}!");
            }
            else if (attacker.Element == CatElement.Ice && target.IsAlive)
            {
                if (UnityEngine.Random.value <= 0.25f)
                {
                    target.AddDebuff(MewtationsDebuff.Frozen, 1);
                    logCallback?.Invoke($"❄️ [BĂNG] Đòn đánh buốt lạnh của {attacker.Name} Đóng Băng {target.Name}!");
                }
            }
            else if (attacker.Element == CatElement.Lightning && target.IsAlive)
            {
                if (target.HasDebuff(MewtationsDebuff.Shocked))
                {
                    attacker.CurrentRage = Mathf.Min(145, attacker.CurrentRage + 10);
                    logCallback?.Invoke($"⚡ [LÔI CHẤN] {attacker.Name} đánh trúng mục tiêu Điện Giật, hấp thụ hạt sét phục hồi +10 Nộ khí!");
                }
                if (UnityEngine.Random.value <= 0.30f)
                {
                    target.AddDebuff(MewtationsDebuff.Shocked, 2);
                    logCallback?.Invoke($"⚡ [LÔI] Sét đánh cực nhanh từ {attacker.Name} gây Điện Giật lên {target.Name} (+30% sát thương nhận vào)!");
                }
            }

            // Apply Role Specializations (ShieldSupport, RageSupport, Debuff, Disruption)
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
            else if (attacker.Role == CatRole.Debuff && target.IsAlive)
            {
                if (UnityEngine.Random.value <= 0.40f)
                {
                    MewtationsDebuff debuffToApply = MewtationsDebuff.None;
                    int duration = 2;
                    switch (attacker.Element)
                    {
                        case CatElement.Fire:
                            debuffToApply = MewtationsDebuff.Burning;
                            break;
                        case CatElement.Poison:
                            debuffToApply = MewtationsDebuff.Poisoned;
                            duration = 3;
                            break;
                        case CatElement.Ice:
                            debuffToApply = MewtationsDebuff.Frozen;
                            duration = 1;
                            break;
                        case CatElement.Lightning:
                            debuffToApply = MewtationsDebuff.Shocked;
                            break;
                        default:
                            MewtationsDebuff[] possible = { MewtationsDebuff.Burning, MewtationsDebuff.Poisoned, MewtationsDebuff.Frozen, MewtationsDebuff.Shocked };
                            debuffToApply = possible[UnityEngine.Random.Range(0, possible.Length)];
                            if (debuffToApply == MewtationsDebuff.Frozen) duration = 1;
                            else if (debuffToApply == MewtationsDebuff.Poisoned) duration = 3;
                            break;
                    }

                    if (debuffToApply != MewtationsDebuff.None)
                    {
                        target.AddDebuff(debuffToApply, duration);
                        string viName = "";
                        switch(debuffToApply)
                        {
                            case MewtationsDebuff.Burning: viName = "Thiêu Đốt"; break;
                            case MewtationsDebuff.Poisoned: viName = "Kịch Độc"; break;
                            case MewtationsDebuff.Frozen: viName = "Đóng Băng"; break;
                            case MewtationsDebuff.Shocked: viName = "Điện Giật"; break;
                        }
                        logCallback?.Invoke($"☠️ [SUY YẾU] Kẻ suy yếu {attacker.Name} kích hoạt hiệu ứng xấu ngẫu nhiên: gây {viName} lên {target.Name}!");
                    }
                }
            }
            else if (attacker.Role == CatRole.Disruption && target.IsAlive)
            {
                if (target.HasIronWill)
                {
                    logCallback?.Invoke($"🛡️ {target.Name} sở hữu Ý Chí Sắt Đá, miễn nhiễm mọi hiệu ứng Quấy Nhiễu!");
                }
                else
                {
                    int drained = Mathf.Min(target.CurrentRage, 15);
                    target.CurrentRage -= drained;
                    target.Speed = Mathf.Max(10, target.Speed - 10);
                    logCallback?.Invoke($"💢 [QUẤY NHIỄU] {attacker.Name} quấy rối làm {target.Name} tiêu hao {drained} Nộ và giảm 10 Tốc Độ!");
                }
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

            // Apply BrokenFireVein backfire
            bool usesFire = false;
            var weapon = attacker.Source.GetEquipableOfEquipableType(EquipableType.Weapon);
            if (weapon != null && (weapon.Id.ToLower().Contains("fire") || weapon.Id.ToLower().Contains("hỏa") || weapon.Id.ToLower().Contains("hoa")))
            {
                usesFire = true;
            }
            if (attacker.Element == CatElement.Fire)
            {
                usesFire = true;
            }
            if (usesFire && attacker.HasTrait(Mewtations.Combat.PermanentScar.BrokenFireVein) && attacker.IsAlive)
            {
                attacker.TakeDamage(2);
                logCallback?.Invoke($"🔥 [HỎA MẠCH ĐỨT GÃY] {attacker.Name} sử dụng vũ khí/chiêu thức hệ Hỏa khi đang bị Đứt Hỏa Mạch! Tự chịu phản phệ -2 HP!");
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
