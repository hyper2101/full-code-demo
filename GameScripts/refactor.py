import sys

file_path = r"c:\Users\Administrator\Documents\GitHub\full-code-demo\GameScripts\Combat\Core\MewtationsCombatStructs.cs"

with open(file_path, "r", encoding="utf-8") as f:
    lines = f.readlines()

start_index = -1
end_index = -1

for i, line in enumerate(lines):
    if "[Serializable]" in line and "public class CombatUnit" in lines[i+1]:
        start_index = i
    if "public static class MewtationsWeaponRegistry" in line:
        end_index = i - 1
        break

if start_index != -1 and end_index != -1:
    new_class = """    [Serializable]
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
        public GameScripts.Combat.Core.CombatSkillDefinition ActiveCombatSkill;

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
"""
    new_lines = lines[:start_index] + [new_class] + lines[end_index:]
    with open(file_path, "w", encoding="utf-8") as f:
        f.writelines(new_lines)
    print("Successfully replaced CombatUnit")
else:
    print("Could not find boundaries")
