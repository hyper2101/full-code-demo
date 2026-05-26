using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mewtations.Expedition;
using Mewtations.Combat.Battlefield;
using Mewtations.Combat.UI;

// TURN-BASED CORE SYSTEM
// DO NOT REMOVE DURING LEGACY COMBAT CLEANUP
namespace Mewtations.Combat.Core
{
    public enum CombatResult
    {
        Ongoing,
        Victory,
        Defeat,
        Retreated
    }

    public enum MewtationsCombatState
    {
        Preparation,
        Active,
        Idle
    }

    public class TurnBasedCombatManager : MonoBehaviour
    {
        public static TurnBasedCombatManager Instance { get; private set; }

        public FormationManager Formation = new FormationManager();
        public List<string> CombatLog = new List<string>();
        public bool IsCombatActive = false;
        public CombatResult Result = CombatResult.Ongoing;
        public MewtationsCombatState State = MewtationsCombatState.Idle;
        public List<Combatable> AvailableCats = new List<Combatable>();
        public List<Combatable> EnemySourceList = new List<Combatable>();
        public int CurrentRound = 1;
        public List<ICombatHazard> ActiveHazards = new List<ICombatHazard>();

        public int AntiStallRound = 10;
        public float AntiStallHealPenalty = 0.50f;

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
            State = MewtationsCombatState.Preparation;
            CombatLog.Clear();
            _onCombatEnd = onCombatEnd;

            AvailableCats = new List<Combatable>(playerCats);
            EnemySourceList = new List<Combatable>(enemies);

            // Clear unified event pipeline before registering units
            MewtationsEventPipeline.Clear();

            // Freeze main board
            WorldManager.instance.SetViewType(ViewType.Default);
            
            // Set up formations (executes CombatUnit constructors, registering components)
            Formation.SetupPlayerTeam(playerCats);
            Formation.SetupEnemyTeam(enemies);

            AddLog("▶ Đang chuẩn bị trận hình...");

            // Open Combat Overlay
            if (CombatOverlayUI.Instance != null)
            {
                CombatOverlayUI.Instance.ShowWindow();
            }
        }

        public void ConfirmFight()
        {
            if (Formation.PlayerUnits.Count == 0)
            {
                AddLog("⚠️ Không thể chiến đấu mà không có Mèo nào trên lưới!");
                return;
            }
            if (Formation.PlayerUnits.Count > 5)
            {
                AddLog("⚠️ Đội hình tối đa là 5 Mèo!");
                return;
            }

            State = MewtationsCombatState.Active;
            AddLog("⚔️ Đội hình xuất kích! Trận chiến bắt đầu...");

            // Re-instantiate final combat units on final slots to clean the event pipeline and register fresh components!
            List<CombatUnit> finalPlayerUnits = new List<CombatUnit>();
            foreach (var unit in Formation.PlayerUnits)
            {
                finalPlayerUnits.Add(new CombatUnit(unit.Source, true, unit.SlotIndex));
            }
            Formation.PlayerUnits = finalPlayerUnits;

            List<CombatUnit> finalEnemyUnits = new List<CombatUnit>();
            foreach (var unit in Formation.EnemyUnits)
            {
                finalEnemyUnits.Add(new CombatUnit(unit.Source, false, unit.SlotIndex));
            }
            Formation.EnemyUnits = finalEnemyUnits;

            // Initialize hazards for the current battle
            InitializeHazards();

            // Apply environmental hazards from depth layer
            if (ExpeditionManager.Instance != null && ExpeditionManager.Instance.IsExpeditionActive && ExpeditionManager.Instance.ActiveNode != null)
            {
                MewtationsPressureSystem.ApplyEnvironmentalModifiers(
                    ExpeditionManager.Instance.ActiveNode.Biome,
                    Formation.PlayerUnits,
                    msg => AddLog(msg)
                );
            }

            _combatCoroutine = StartCoroutine(CombatLoopRoutine());
        }

        public void Retreat()
        {
            if (!IsCombatActive) return;

            AddLog("🏳 Quân ta quyết định Bỏ Cuộc! Rút lui an toàn...");
            Result = CombatResult.Retreated;

            if (_combatCoroutine != null)
            {
                StopCoroutine(_combatCoroutine);
            }

            EndCombat();
        }

        private void InitializeHazards()
        {
            ActiveHazards.Clear();
            if (ExpeditionManager.Instance != null && ExpeditionManager.Instance.IsExpeditionActive)
            {
                var runState = ExpeditionManager.Instance.RunState;
                var activeNode = ExpeditionManager.Instance.ActiveNode;

                if (runState != null && runState.GreedLevel >= 75)
                {
                    ActiveHazards.Add(new GreedPunishmentHazard());
                }

                if (activeNode != null && activeNode.Theme == Mewtations.Expedition.RouteTheme.ThienLoi)
                {
                    ActiveHazards.Add(new ThienLoiHazard());
                }

                // Register Field hazards based on biome and theme
                if (activeNode != null)
                {
                    if (activeNode.Biome == ExpeditionBiome.Swamp)
                    {
                        ActiveHazards.Add(new SwampFieldHazard());
                        Debug.Log("[CombatHazards] Trận địa Đầm Lầy Tộc Cóc được thiết lập cho cuộc chiến!");
                    }
                    else if (activeNode.Biome == ExpeditionBiome.Peak || activeNode.Theme == RouteTheme.TaDao)
                    {
                        ActiveHazards.Add(new FireFieldHazard());
                        Debug.Log("[CombatHazards] Trận Pháp Mưa Lửa được thiết lập cho cuộc chiến!");
                    }
                }
            }
        }

        private IEnumerator CombatLoopRoutine()
        {
            int round = 1;
            while (Result == CombatResult.Ongoing)
            {
                CurrentRound = round;
                AddLog($"--- VÒNG LẦN {round} ---");

                // Trigger Round Start environment and greed hazards
                foreach (var hazard in ActiveHazards)
                {
                    hazard.OnRoundStart(this, round, AddLog);
                }
                if (Result != CombatResult.Ongoing) break;

                // (Wait 1.0s between events loop omitted for brief)

                // Trigger Round End hazards
                foreach (var hazard in ActiveHazards)
                {
                    hazard.OnRoundEnd(this, round, AddLog);
                }
                if (Result != CombatResult.Ongoing) break;

                // Get all active combat units
                List<CombatUnit> allUnits = new List<CombatUnit>();
                allUnits.AddRange(Formation.PlayerUnits.FindAll(u => u.IsAlive));
                allUnits.AddRange(Formation.EnemyUnits.FindAll(u => u.IsAlive));

                // Sort by Speed descending
                allUnits = allUnits.OrderByDescending(u => u.Speed).ToList();

                foreach (var unit in allUnits)
                {
                    if (!unit.IsAlive || Result != CombatResult.Ongoing) continue;

                    // Trigger Turn Start Event Hooks!
                    MewtationsEventPipeline.TriggerOnTurnStart(unit, msg => AddLog(msg));

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
                        unit.AddBuff(BuffType.CCImmunity, 1);
                        AddLog($"✨ {unit.Name} thoát khỏi băng phong, nhận trạng thái [Kháng Khống Chế (CC Immunity)] trong 1 lượt!");
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
                        var target = MewtationsUltimateRegistry.GetPrimaryTarget(opponents, unit);
                        if (target != null)
                        {
                            MewtationsWeaponRegistry.ExecuteBasicAttack(unit, target, allies, opponents, msg => AddLog(msg));
                        }
                    }

                    // 4. Rage Accumulation
                    unit.CurrentRage = Mathf.Min(145, unit.CurrentRage + 20); // +20 Rage per action

                    // Trigger Turn End Event Hooks!
                    MewtationsEventPipeline.TriggerOnTurnEnd(unit, msg => AddLog(msg));

                    // Tick buffs của đơn vị sau lượt đi
                    unit.TickBuffs(msg => AddLog(msg));

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
                // --- END OF ROUND: STAMINA DRAIN & EXHAUSTION ESCALATION ---
                int staminaCost = 5 + round * 1;
                foreach (var unit in Formation.PlayerUnits.FindAll(u => u.IsAlive))
                {
                    if (unit.IsExhausted)
                    {
                        unit.ExhaustionLevel++;
                    }
                    else
                    {
                        unit.Stamina = Mathf.Max(0, unit.Stamina - staminaCost);
                        if (unit.Stamina <= 0)
                        {
                            unit.IsExhausted = true;
                            unit.ExhaustionLevel = 1;
                            AddLog($"💤 {unit.Name} đã cạn kiệt Thể Lực và rơi vào trạng thái Kiệt Sức!");
                        }
                    }
                }

                // Anti-stall warning & check
                if (round == AntiStallRound)
                {
                    AddLog($"⚠️ [CẠN KIỆT LINH KHÍ] Trận chiến kéo dài quá lâu! Từ nay, toàn bộ hiệu quả hồi máu và hồi giáp bị giảm {Mathf.RoundToInt(AntiStallHealPenalty * 100)}%!");
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
                    // Remove from active expedition list if in an expedition
                    if (unit.Source is CatCardData catData)
                    {
                        if (ExpeditionManager.Instance != null && ExpeditionManager.Instance.IsExpeditionActive)
                        {
                            ExpeditionManager.Instance.ActiveCats.Remove(catData);
                            if (catData.MyGameCard != null)
                            {
                                catData.MyGameCard.gameObject.SetActive(true);
                            }
                        }
                    }
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
                    if (unit.Source is CatCardData catData)
                    {
                        if (ExpeditionManager.Instance != null && ExpeditionManager.Instance.IsExpeditionActive)
                        {
                            ExpeditionManager.Instance.ActiveCats.Remove(catData);
                        }
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
                        cat.Stamina = unit.Stamina;
                        cat.IsExhausted = unit.IsExhausted;
                        cat.HoiQuangPhanChieuTriggered = unit.HoiQuangPhanChieuTriggered;
                        cat.ExhaustionLevel = unit.IsExhausted ? unit.ExhaustionLevel : 0;
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

        public void AddLog(string message)
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
