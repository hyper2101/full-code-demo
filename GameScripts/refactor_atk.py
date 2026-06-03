import sys

file_path = r"c:\Users\Administrator\Documents\GitHub\full-code-demo\GameScripts\Combat\Core\MewtationsCombatStructs.cs"
with open(file_path, "r", encoding="utf-8") as f:
    lines = f.readlines()

start_index = -1
end_index = -1

for i, line in enumerate(lines):
    if "public static void ExecuteBasicAttack(CombatUnit attacker, CombatUnit target, List<CombatUnit> allies, List<CombatUnit> opponents, Action<string> logCallback)" in line:
        start_index = i
    if start_index != -1 and "public static class MewtationsUltimateRegistry" in line:
        end_index = i - 1
        break

if start_index != -1 and end_index != -1:
    new_method = """        public static void ExecuteBasicAttack(CombatUnit attacker, CombatUnit target, List<CombatUnit> allies, List<CombatUnit> opponents, Action<string> logCallback)
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
"""
    new_lines = lines[:start_index] + [new_method] + lines[end_index+1:]
    with open(file_path, "w", encoding="utf-8") as f:
        f.writelines(new_lines)
    print("Successfully replaced ExecuteBasicAttack")
else:
    print("Could not find boundaries")
