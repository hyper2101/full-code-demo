using System;
using System.Collections.Generic;
using UnityEngine;

using Mewtations.Combat.Core;

namespace Mewtations.Combat.Battlefield
{
    public interface ICombatHazard
    {
        string Name { get; }
        void OnRoundStart(TurnBasedCombatManager manager, int round, Action<string> logCallback);
        void OnRoundEnd(TurnBasedCombatManager manager, int round, Action<string> logCallback);
    }

    public class GreedPunishmentHazard : ICombatHazard
    {
        public string Name => "Lôi Phạt Trừng Phạt";
        
        public void OnRoundStart(TurnBasedCombatManager manager, int round, Action<string> logCallback)
        {
            logCallback?.Invoke("⚡ LÔI PHẠT TRỪNG PHẠT! Thần Mèo phẫn nộ trước lòng tham vô độ (Greed >= 75)! Sét đánh giáng xuống toàn bộ Thần Miêu!");
            var aliveUnits = manager.Formation.PlayerUnits.FindAll(u => u.IsAlive);
            foreach (var unit in aliveUnits)
            {
                unit.CurrentHP = Mathf.Max(0, unit.CurrentHP - 3);
                if (unit.Source != null)
                {
                    unit.Source.HealthPoints = unit.CurrentHP;
                }
                logCallback?.Invoke($"   • {unit.Name} gánh chịu 3 sát thương lôi phạt (Máu hiện tại: {unit.CurrentHP} HP)");
            }
            
            // Check deaths
            foreach (var unit in aliveUnits)
            {
                if (unit.CurrentHP <= 0)
                {
                    manager.CheckUnitDeath(unit);
                }
            }
            manager.CheckCombatEndConditions();
        }

        public void OnRoundEnd(TurnBasedCombatManager manager, int round, Action<string> logCallback) {}
    }

    public class ThienLoiHazard : ICombatHazard
    {
        public string Name => "Kiếp Lôi Bạo Phá";

        public void OnRoundStart(TurnBasedCombatManager manager, int round, Action<string> logCallback) {}

        public void OnRoundEnd(TurnBasedCombatManager manager, int round, Action<string> logCallback)
        {
            if (UnityEngine.Random.value <= 0.15f)
            {
                var alivePlayerUnits = manager.Formation.PlayerUnits.FindAll(u => u.IsAlive);
                var aliveEnemyUnits = manager.Formation.EnemyUnits.FindAll(u => u.IsAlive);
                var allAlive = new List<CombatUnit>();
                allAlive.AddRange(alivePlayerUnits);
                allAlive.AddRange(aliveEnemyUnits);

                if (allAlive.Count > 0)
                {
                    var targetUnit = allAlive[UnityEngine.Random.Range(0, allAlive.Count)];
                    targetUnit.CurrentHP = Mathf.Max(0, targetUnit.CurrentHP - 5);
                    if (targetUnit.Source != null)
                    {
                        targetUnit.Source.HealthPoints = targetUnit.CurrentHP;
                    }
                    logCallback?.Invoke($"⚡ KIẾP LÔI BẠO PHÁ! Sét đánh ngẫu nhiên giáng xuống {targetUnit.Name} gây 5 sát thương lôi pháp! (Máu hiện tại: {targetUnit.CurrentHP} HP)");

                    if (targetUnit.CurrentHP <= 0)
                    {
                        manager.CheckUnitDeath(targetUnit);
                    }
                    manager.CheckCombatEndConditions();
                }
            }
        }
    }

    public class FireFieldHazard : ICombatHazard
    {
        public string Name => "Trận Pháp Mưa Lửa";

        public void OnRoundStart(TurnBasedCombatManager manager, int round, Action<string> logCallback)
        {
            // Check if any player cat has the FireResist tag
            bool countered = false;
            foreach (var unit in manager.Formation.PlayerUnits)
            {
                if (unit.IsAlive && unit.HasGameplayTag("FireResist"))
                {
                    countered = true;
                    break;
                }
            }

            if (countered)
            {
                if (round % 2 == 0)
                {
                    logCallback?.Invoke("🔥 [TRẬN PHÁP MƯA LỬA] Dù có Bùa Hỏa Linh (FireResist) bảo hộ toàn đội, linh hỏa thâm nhập vẫn gây 1 sát thương lên toàn bộ Thần Miêu.");
                    var aliveUnits = manager.Formation.PlayerUnits.FindAll(u => u.IsAlive);
                    foreach (var unit in aliveUnits)
                    {
                        unit.TakeDamage(1);
                        if (unit.CurrentHP <= 0) manager.CheckUnitDeath(unit);
                    }
                    manager.CheckCombatEndConditions();
                }
                else
                {
                    logCallback?.Invoke("🔥 [TRẬN PHÁP MƯA LỬA] Bùa Hỏa Linh phát quang bảo hộ toàn đội khỏi luồng hỏa triều (Sát thương: 0).");
                }
                return;
            }

            logCallback?.Invoke("🔥 [TRẬN PHÁP MƯA LỬA] Mưa lửa bùng phát thiêu đốt chiến trường! Gây 2 sát thương lên toàn bộ Thần Miêu.");
            var unitsToDamage = manager.Formation.PlayerUnits.FindAll(u => u.IsAlive);
            foreach (var unit in unitsToDamage)
            {
                unit.TakeDamage(2);
                logCallback?.Invoke($"   • {unit.Name} bị lửa thiêu chịu 2 sát thương (Máu: {unit.CurrentHP} HP)");
                if (unit.CurrentHP <= 0)
                {
                    manager.CheckUnitDeath(unit);
                }
            }
            manager.CheckCombatEndConditions();
        }

        public void OnRoundEnd(TurnBasedCombatManager manager, int round, Action<string> logCallback) {}
    }

    public class SwampFieldHazard : ICombatHazard
    {
        public string Name => "Đầm Lầy Tộc Cóc";

        public void OnRoundStart(TurnBasedCombatManager manager, int round, Action<string> logCallback)
        {
            // Check if any player cat has the SwampAdapted tag
            bool countered = false;
            foreach (var unit in manager.Formation.PlayerUnits)
            {
                if (unit.IsAlive && unit.HasGameplayTag("SwampAdapted"))
                {
                    countered = true;
                    break;
                }
            }

            float slowPercent = countered ? 0.05f : 0.25f;
            string textMsg = countered 
                ? "🐸 [ĐẦM LẦY TỘC CÓC] Bùa hộ thân bảo hộ giúp toàn đội đi nhanh qua bùn lầy (Giảm nhẹ -5% Thần Tốc)."
                : "🐸 [ĐẦM LẦY TỘC CÓC] Bùn lầy ngập ngụa làm giảm mạnh thần tốc của toàn đội (-25% Thần Tốc)!";
            
            logCallback?.Invoke(textMsg);
            foreach (var unit in manager.Formation.PlayerUnits)
            {
                if (unit.IsAlive)
                {
                    int slowAmount = Mathf.RoundToInt(unit.Speed * slowPercent);
                    unit.Speed = Mathf.Max(10, unit.Speed - slowAmount);
                    logCallback?.Invoke($"   • {unit.Name} bị sa lầy, Tốc độ giảm -{slowAmount} (Tốc độ hiện tại: {unit.Speed})");
                }
            }
        }

        public void OnRoundEnd(TurnBasedCombatManager manager, int round, Action<string> logCallback) {}
    }
}
