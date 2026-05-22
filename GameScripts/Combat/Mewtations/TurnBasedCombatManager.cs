using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mewtations.Combat
{
    public enum CombatResult
    {
        Ongoing,
        Victory,
        Defeat,
        Retreated
    }

    public class TurnBasedCombatManager : MonoBehaviour
    {
        public static TurnBasedCombatManager Instance { get; private set; }

        public FormationManager Formation = new FormationManager();
        public List<string> CombatLog = new List<string>();
        public bool IsCombatActive = false;
        public CombatResult Result = CombatResult.Ongoing;

        private Coroutine _combatCoroutine;
        private Action<CombatResult> _onCombatEnd;

        private void Awake()
        {
            Instance = this;
        }

        public void StartCombat(List<Combatable> playerCats, List<Combatable> enemies, Action<CombatResult> onCombatEnd)
        {
            if (IsCombatActive) return;

            IsCombatActive = true;
            Result = CombatResult.Ongoing;
            CombatLog.Clear();
            _onCombatEnd = onCombatEnd;

            // Freeze main board
            WorldManager.instance.SetViewType(ViewType.Default);
            
            // Set up formations
            Formation.SetupPlayerTeam(playerCats);
            Formation.SetupEnemyTeam(enemies);

            AddLog("▶ Trận chiến bắt đầu!");

            // Open Combat Overlay
            if (CombatOverlayUI.Instance != null)
            {
                CombatOverlayUI.Instance.ShowWindow();
            }

            _combatCoroutine = StartCoroutine(CombatLoopRoutine());
        }

        public void Retreat()
        {
            if (!IsCombatActive || Result != CombatResult.Ongoing) return;

            AddLog("🏳 Quân ta quyết định Bỏ Cuộc! Rút lui an toàn...");
            Result = CombatResult.Retreated;

            if (_combatCoroutine != null)
            {
                StopCoroutine(_combatCoroutine);
            }

            EndCombat();
        }

        private IEnumerator CombatLoopRoutine()
        {
            int round = 1;
            while (Result == CombatResult.Ongoing)
            {
                AddLog($"--- VÒNG LẦT {round} ---");

                // Cat God's Punishment Curse (Linh Thần Trừng Phạt) for high Greed
                if (ExpeditionManager.Instance != null && ExpeditionManager.Instance.IsExpeditionActive && ExpeditionManager.Instance.RunState.GreedLevel >= 75)
                {
                    AddLog("⚡ LÔI PHẠT TRỪNG PHẠT! Thần Mèo phẫn nộ trước lòng tham vô độ (Greed >= 75)! Sét đánh giáng xuống toàn bộ Thần Miêu!");
                    var aliveUnits = Formation.PlayerUnits.FindAll(u => u.IsAlive);
                    foreach (var unit in aliveUnits)
                    {
                        unit.CurrentHP = Mathf.Max(0, unit.CurrentHP - 3);
                        if (unit.Source != null)
                        {
                            unit.Source.HealthPoints = unit.CurrentHP;
                        }
                        AddLog($"   • {unit.Name} gánh chịu 3 sát thương lôi phạt (Máu hiện tại: {unit.CurrentHP} HP)");
                    }
                    
                    // Check deaths caused by curse
                    foreach (var unit in aliveUnits)
                    {
                        if (unit.CurrentHP <= 0)
                        {
                            CheckUnitDeath(unit);
                        }
                    }
                    CheckCombatEndConditions();
                    if (Result != CombatResult.Ongoing) break;
                }

                // Kiếp Lôi Bạo Phá hazard (Thiên Lôi Theme)
                if (ExpeditionManager.Instance != null && ExpeditionManager.Instance.IsExpeditionActive && ExpeditionManager.Instance.ActiveNode != null && ExpeditionManager.Instance.ActiveNode.Theme == Mewtations.Expedition.RouteTheme.ThienLoi)
                {
                    if (UnityEngine.Random.value <= 0.15f)
                    {
                        var alivePlayerUnits = Formation.PlayerUnits.FindAll(u => u.IsAlive);
                        var aliveEnemyUnits = Formation.EnemyUnits.FindAll(u => u.IsAlive);
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
                            AddLog($"⚡ KIẾP LÔI BẠO PHÁ! Sét đánh ngẫu nhiên giáng xuống {targetUnit.Name} gây 5 sát thương lôi pháp! (Máu hiện tại: {targetUnit.CurrentHP} HP)");

                            if (targetUnit.CurrentHP <= 0)
                            {
                                CheckUnitDeath(targetUnit);
                            }
                            CheckCombatEndConditions();
                            if (Result != CombatResult.Ongoing) break;
                        }
                    }
                }

                // Get all active combat units
                List<CombatUnit> allUnits = new List<CombatUnit>();
                allUnits.AddRange(Formation.PlayerUnits.FindAll(u => u.IsAlive));
                allUnits.AddRange(Formation.EnemyUnits.FindAll(u => u.IsAlive));

                // Sort by Speed descending
                allUnits = allUnits.OrderByDescending(u => u.Speed).ToList();

                foreach (var unit in allUnits)
                {
                    if (!unit.IsAlive || Result != CombatResult.Ongoing) continue;

                    // 1. Tick debuffs (Burning/Poisoned)
                    unit.TickDebuffs(msg => AddLog(msg));
                    if (!unit.IsAlive)
                    {
                        CheckUnitDeath(unit);
                        CheckCombatEndConditions();
                        continue;
                    }

                    // 2. Control effects check (Frozen/Stunned)
                    if (unit.HasDebuff(MewtationsDebuff.Frozen))
                    {
                        AddLog($"❄ {unit.Name} đang bị Đóng Băng và bỏ qua lượt!");
                        unit.ActiveDebuffs.RemoveAll(d => d.Type == MewtationsDebuff.Frozen);
                        continue;
                    }

                    yield return new WaitForSeconds(1.0f); // 1-second delay for premium readability

                    // 3. Action Execution
                    List<CombatUnit> allies = unit.IsPlayer ? Formation.PlayerUnits : Formation.EnemyUnits;
                    List<CombatUnit> opponents = unit.IsPlayer ? Formation.EnemyUnits : Formation.PlayerUnits;

                    if (unit.CurrentRage >= 100)
                    {
                        // Cast Ultimate
                        MewtationsUltimateRegistry.ExecuteUltimate(unit, allies, opponents, msg => AddLog(msg));
                    }
                    else
                    {
                        // Cast Basic Attack
                        var target = MewtationsUltimateRegistry.GetPrimaryTarget(opponents);
                        if (target != null)
                        {
                            MewtationsWeaponRegistry.ExecuteBasicAttack(unit, target, opponents, msg => AddLog(msg));
                        }
                    }

                    // 4. Rage Accumulation
                    unit.CurrentRage = Mathf.Min(145, unit.CurrentRage + 20); // +20 Rage per action

                    // 5. Post-Action Checks
                    foreach (var opp in opponents)
                    {
                        if (!opp.IsAlive)
                        {
                            CheckUnitDeath(opp);
                        }
                    }
                    foreach (var ally in allies)
                    {
                        if (!ally.IsAlive)
                        {
                            CheckUnitDeath(ally);
                        }
                    }

                    CheckCombatEndConditions();
                    if (Result != CombatResult.Ongoing) break;
                }

                round++;
                if (round > 50) // Safe exit from endless loops
                {
                    AddLog("⚔️ Trận đấu kéo dài quá lâu! Tự động hòa.");
                    Result = CombatResult.Retreated;
                    break;
                }

                yield return new WaitForSeconds(0.5f);
            }

            EndCombat();
        }

        private void CheckUnitDeath(CombatUnit unit)
        {
            AddLog($"☠ {unit.Name} đã gục ngã!");

            if (unit.IsPlayer && unit.Source != null)
            {
                // Check Insurance
                bool hasInsurance = false;
                var insuranceItem = unit.Source.GetEquipableOfEquipableType(EquipableType.Talisman); // Put in talisman slot or insurance slot
                if (insuranceItem != null && insuranceItem.Id.Contains("insurance"))
                {
                    hasInsurance = true;
                    unit.Source.UnequipItem(insuranceItem); // Insurance consumed
                    if (insuranceItem.MyGameCard != null)
                    {
                        insuranceItem.MyGameCard.DestroyCard(true, true);
                    }
                }

                if (hasInsurance)
                {
                    unit.CurrentHP = 1;
                    unit.Source.HealthPoints = 1;
                    AddLog($"🛡 Bảo Hiểm Tu Tiên kích hoạt! {unit.Name} hồi sinh với 1 HP và trốn thoát về base.");
                    // Remove from active list
                    Formation.PlayerUnits.Remove(unit);
                }
                else
                {
                    // Convert to Corpse card on board
                    Vector3 spawnPos = unit.Source.transform.position;
                    GameCard corpseCard = WorldManager.instance.CreateCard(spawnPos, "cat_corpse", true, true, true);
                    CatCorpseData corpseData = corpseCard.CardData as CatCorpseData;
                    if (corpseData != null)
                    {
                        corpseData.OriginalCatId = unit.Source.Id;
                        corpseData.OriginalCatName = unit.Name;
                        if (unit.Source is CatCardData cat)
                        {
                            corpseData.OriginalCatRole = cat.Role;
                            corpseData.OriginalCatElement = cat.Element;

                            // Save breakthrough and stats
                            corpseData.OriginalBreakthroughLevel = cat.BreakthroughLevel;
                            corpseData.OriginalHasPillSlot = cat.HasPillSlot;
                            corpseData.OriginalHasFoodSlot = cat.HasFoodSlot;
                            corpseData.OriginalHasPassive1Slot = cat.HasPassive1Slot;
                            corpseData.OriginalHasPassive2Slot = cat.HasPassive2Slot;
                            corpseData.OriginalSpeed = cat.Speed;
                            if (cat.BaseCombatStats != null)
                            {
                                corpseData.OriginalMaxHealth = cat.BaseCombatStats.MaxHealth;
                            }

                            // Write death milestone to memoirs and serialize
                            string layerInfo = (ExpeditionManager.Instance != null && ExpeditionManager.Instance.IsExpeditionActive) 
                                ? "Tầng " + ExpeditionManager.Instance.RunState.CurrentLayer 
                                : "Căn Cứ";
                            cat.AddMemoir("Tử trận tại viễn chinh " + layerInfo);
                            corpseData.OriginalLineageGeneration = cat.LineageGeneration;
                            corpseData.OriginalCharacterMemoirs = cat.CharacterMemoirsString;
                        }
                    }
                    
                    // Destroy original cat
                    if (unit.Source.MyGameCard != null)
                    {
                        unit.Source.MyGameCard.DestroyCard(true, true);
                    }
                    Formation.PlayerUnits.Remove(unit);
                }
            }
        }

        private void CheckCombatEndConditions()
        {
            if (Formation.IsPlayerDefeated())
            {
                Result = CombatResult.Defeat;
                AddLog("❌ Thất bại! Đội hình mèo đã bị quét sạch.");
            }
            else if (Formation.IsEnemyDefeated())
            {
                Result = CombatResult.Victory;
                AddLog("🏆 Chiến thắng vang dội! Quân địch đã bị tiêu diệt.");
            }
        }

        private void EndCombat()
        {
            IsCombatActive = false;
            
            // Sync final health states back to base units
            foreach (var unit in Formation.PlayerUnits)
            {
                if (unit.Source != null)
                {
                    unit.Source.HealthPoints = unit.CurrentHP;
                    if (unit.Source is CatCardData cat)
                    {
                        cat.CurrentRage = unit.CurrentRage;
                    }
                }
            }

            // Close UI overlay after short delay
            StartCoroutine(CloseUiDelayRoutine());
        }

        private IEnumerator CloseUiDelayRoutine()
        {
            yield return new WaitForSeconds(3.0f); // Allow player to read end log

            if (CombatOverlayUI.Instance != null)
            {
                CombatOverlayUI.Instance.HideWindow();
            }

            _onCombatEnd?.Invoke(Result);
        }

        private void AddLog(string message)
        {
            CombatLog.Add(message);
            if (CombatLog.Count > 100)
            {
                CombatLog.RemoveAt(0);
            }
            Debug.Log($"[CombatLog] {message}");
        }
    }
}
