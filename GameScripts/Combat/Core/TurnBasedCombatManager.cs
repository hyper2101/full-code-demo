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
// CombatResult enum moved to CombatResultData.cs
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

        public int MaxRounds = 30;
        public CombatEndReason EndReason = CombatEndReason.Retreat;

        public int AntiStallRound = 10;
        public float AntiStallHealPenalty = 0.50f;

        [Header("Stamina Costs")]
        public int BaseStaminaCostPerRound = 5;
        public int StaminaCostIncreasePerRound = 1;

        private Coroutine _combatCoroutine;
        private Action<CombatResultData> _onCombatEnd;

        private void Awake()
        {
            Instance = this;
        }

        public void StartCombat(List<Combatable> playerCats, List<Combatable> enemies, Action<CombatResult> onCombatEnd)
        {
            if (IsCombatActive) return;

            // --- LEGACY WRAPPER ---
            var snapshot = new Mewtations.Combat.Encounters.EncounterSetupSnapshot();
            snapshot.PlayerTeam = new List<Mewtations.Combat.Encounters.PlayerUnitSnapshot>();
            for (int i = 0; i < playerCats.Count && i < 5; i++)
            {
                if (playerCats[i] is CatCardData catData)
                {
                    snapshot.PlayerTeam.Add(new Mewtations.Combat.Encounters.PlayerUnitSnapshot
                    {
                        CatReference = catData,
                        FinalSlotIndex = i
                    });
                }
            }

            snapshot.LegacyEnemies = new List<Combatable>(enemies);
            StartCombat(snapshot, (resData) => {
                CombatResultApplier.ApplyResult(resData);
                onCombatEnd?.Invoke(resData.Result);
            });
        }
        
        public void StartCombat(List<Combatable> playerCats, List<GameScripts.Systems.Enemies.DogEnemyInstance> dogs, Action<CombatResult> onCombatEnd)
        {
            if (IsCombatActive) return;

            // --- LEGACY WRAPPER ---
            var snapshot = new Mewtations.Combat.Encounters.EncounterSetupSnapshot();
            snapshot.Encounter = ScriptableObject.CreateInstance<Mewtations.Combat.Encounters.EncounterData>();
            snapshot.Encounter.Enemies = new List<Mewtations.Combat.Encounters.EnemySpawnData>();
            snapshot.PlayerTeam = new List<Mewtations.Combat.Encounters.PlayerUnitSnapshot>();
            
            for (int i = 0; i < playerCats.Count && i < 5; i++)
            {
                if (playerCats[i] is CatCardData catData)
                {
                    snapshot.PlayerTeam.Add(new Mewtations.Combat.Encounters.PlayerUnitSnapshot
                    {
                        CatReference = catData,
                        FinalSlotIndex = i
                    });
                }
            }

            for (int i = 0; i < dogs.Count; i++)
            {
                snapshot.Encounter.Enemies.Add(new Mewtations.Combat.Encounters.EnemySpawnData
                {
                    Enemy = dogs[i],
                    SlotIndex = i + 5 // Arbitrary slots for legacy
                });
            }

            StartCombat(snapshot, (resData) => {
                CombatResultApplier.ApplyResult(resData);
                onCombatEnd?.Invoke(resData.Result);
            });
        }

        public void StartCombat(Mewtations.Combat.Encounters.EncounterSetupSnapshot snapshot, Action<CombatResultData> onCombatEnd = null)
        {
            if (IsCombatActive) return;

            IsCombatActive = true;
            Result = CombatResult.Ongoing;
            State = MewtationsCombatState.Preparation;
            CombatLog.Clear();
            _onCombatEnd = onCombatEnd;

            AvailableCats = new List<Combatable>();
            EnemySourceList = new List<Combatable>();

            // Clear unified event pipeline before registering units
            MewtationsEventPipeline.Clear();

            // Freeze main board
            WorldManager.instance.SetViewType(ViewType.Default);
            WorldManager.WorldSimulationPaused = true;

            // Setup Formations precisely based on the snapshot
            Formation.PlayerUnits.Clear();
            foreach (var pSnap in snapshot.PlayerTeam)
            {
                if (pSnap.CatReference != null)
                {
                    AvailableCats.Add(pSnap.CatReference);
                    Formation.PlayerUnits.Add(GameScripts.Combat.Core.CombatUnitFactory.CreateFromCat(pSnap.CatReference, pSnap.FinalSlotIndex));
                }
            }

            Formation.EnemyUnits.Clear();
            if (snapshot.LegacyEnemies != null && snapshot.LegacyEnemies.Count > 0)
            {
                EnemySourceList = new List<Combatable>(snapshot.LegacyEnemies);
                Formation.SetupEnemyTeam(snapshot.LegacyEnemies);
            }
            else if (snapshot.Encounter != null && snapshot.Encounter.Enemies != null)
            {
                foreach (var eSnap in snapshot.Encounter.Enemies)
                {
                    if (eSnap.Enemy != null)
                    {
                        Formation.EnemyUnits.Add(GameScripts.Combat.Core.CombatUnitFactory.CreateFromDog(eSnap.Enemy, eSnap.SlotIndex));
                    }
                }
            }

            AddLog("▶ Đang chuẩn bị trận hình từ Encounter Snapshot...");

            if (CombatOverlayUI.Instance != null)
            {
                CombatOverlayUI.Instance.Show(Formation);
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
                if (unit.Source is CatCardData cat)
                {
                    finalPlayerUnits.Add(GameScripts.Combat.Core.CombatUnitFactory.CreateFromCat(cat, unit.SlotIndex));
                }
                else
                {
                    finalPlayerUnits.Add(GameScripts.Combat.Core.CombatUnitFactory.CreateFromLegacyEnemy(unit.Source, unit.SlotIndex));
                }
            }
            Formation.PlayerUnits = finalPlayerUnits;

            List<CombatUnit> finalEnemyUnits = new List<CombatUnit>();
            foreach (var unit in Formation.EnemyUnits)
            {
                if (unit.Source == null)
                {
                    // It's a non-card enemy like DogEnemyInstance
                    finalEnemyUnits.Add(unit);
                }
                else
                {
                    finalEnemyUnits.Add(GameScripts.Combat.Core.CombatUnitFactory.CreateFromLegacyEnemy(unit.Source, unit.SlotIndex));
                }
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
            EndReason = CombatEndReason.Retreat;

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

                // Sort by speed and slot index deterministically using the InitiativeResolver
                allUnits = Mewtations.Combat.TurnOrder.InitiativeResolver.BuildTurnQueue(allUnits);

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

                    bool castedSkill = false;
                    bool hasActiveSkill = unit.CombatSkills != null && unit.CombatSkills.Count > 0;

                    if (unit.CurrentRage >= 100)
                    {
                        if (hasActiveSkill)
                        {
                            castedSkill = true;
                            int skillsToCast = unit.CombatSkills.Count;
                            int capturedRage = unit.CurrentRage; // Snapshot rage 1 lần
                            int ragePerSkill = capturedRage / skillsToCast; // Chia đều rage cho các skill
                            
                            foreach (var skill in unit.CombatSkills)
                            {
                                if (!unit.IsAlive) break; // Dừng nếu chủ thể chết giữa chừng
                                
                                unit.RemoveRage(ragePerSkill); // Mỗi skill chỉ tiêu hao phần rage của nó
                                CombatSkillExecutor.ExecuteSkill(unit, skill, ragePerSkill, allies, opponents, msg => AddLog(msg));
                                yield return new WaitForSeconds(0.5f);
                            }
                        }
                        else
                        {
                            bool canRageBurst = false;
                            if (unit.IsPlayer && unit.Source is CatCardData catSource)
                            {
                                if (catSource.BreakthroughLevel >= 2) canRageBurst = true;
                            }

                            if (canRageBurst)
                            {
                                castedSkill = true;
                                var defaultBurst = GetDefaultRageBurst();
                                int capturedRage = unit.CurrentRage;
                                unit.RemoveRage(capturedRage);
                                CombatSkillExecutor.ExecuteSkill(unit, defaultBurst, capturedRage, allies, opponents, msg => AddLog(msg));
                            }
                            else
                            {
                                ExecuteBasicAttacks(unit, allies, opponents);
                            }
                        }
                    }
                    else
                    {
                        ExecuteBasicAttacks(unit, allies, opponents);
                    }

                    // 4. Rage Accumulation
                    if (!castedSkill)
                    {
                        unit.CurrentRage = Mathf.Min(145, unit.CurrentRage + 20); // +20 Rage per action
                    }


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
                int staminaCost = BaseStaminaCostPerRound + round * StaminaCostIncreasePerRound;
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
                if (round > MaxRounds)
                {
                    AddLog("⚔️ Trận đấu đã đạt giới hạn hiệp (Turn Limit)! Người chơi không thể kết thúc trận đấu. Được tính là Thất bại.");
                    Result = CombatResult.Defeat;
                    EndReason = CombatEndReason.TurnLimitReached;
                    break;
                }

                yield return new WaitForSeconds(0.5f);
            }

            EndCombat();
        }

        private void CheckUnitDeath(CombatUnit unit)
        {
            AddLog($"💤 {unit.Name} đã mất khả năng chiến đấu và bị tê liệt!");

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
                    // No corpse anymore! Cat is paralyzed with 0 HP
                    AddLog($"💤 {unit.Name} bị đánh bại và rơi vào trạng thái Tê Liệt (0 HP).");
                    unit.CurrentHP = 0;
                    unit.Stamina = 0;
                    unit.IsExhausted = true;
                    unit.ExhaustionLevel = Mathf.Max(1, unit.ExhaustionLevel);

                    if (unit.Source is CatCardData catData)
                    {
                        // Write loss milestone to memoirs
                        string layerInfo = (ExpeditionManager.Instance != null && ExpeditionManager.Instance.IsExpeditionActive) 
                            ? "Tầng " + ExpeditionManager.Instance.RunState.CurrentLayer 
                            : "Căn Cứ";
                        catData.AddMemoir("Bị thương nặng và tê liệt tại viễn chinh " + layerInfo);
                    }
                    
                    // We DO NOT remove them from Formation.PlayerUnits so they sync back HP=0
                }
            }
        }

        private void CheckCombatEndConditions()
        {
            if (Formation.IsPlayerDefeated())
            {
                Result = CombatResult.Defeat;
                EndReason = CombatEndReason.TeamDefeated;
                AddLog("❌ Thất bại! Đội hình mèo đã bị quét sạch.");
            }
            else if (Formation.IsEnemyDefeated())
            {
                Result = CombatResult.Victory;
                EndReason = CombatEndReason.EnemyDefeated;
                AddLog("🏆 Chiến thắng vang dội! Quân địch đã bị tiêu diệt.");
            }
        }

        private void ProcessOrderingLossRules()
        {
            if (Result != CombatResult.Defeat && Result != CombatResult.Retreated) return;

            var rings = WorldManager.instance.BoardQuery.GetVisibleBoardCards()
                .Where(c => c != null && c.CardData is Mewtations.Legacy.Stacklands.OrderingCardData && !c.Destroyed)
                .ToList();

            foreach (var ringCard in rings)
            {
                if (ringCard.InventoryContainer == null) continue;

                var items = ringCard.InventoryContainer.GetChildren().ToList();
                if (items.Count == 0) continue;

                bool hasRelic = ShrineCardData.IsRelicActiveInShrine("item_ancient_relic_insurance");
                
                List<GameCard> destroyableItems = new List<GameCard>();
                for (int i = 0; i < items.Count; i++)
                {
                    if (hasRelic && i < 5) continue;
                    destroyableItems.Add(items[i]);
                }

                if (destroyableItems.Count == 0) continue;

                int destroyCount = Mathf.CeilToInt(destroyableItems.Count * 0.5f);
                var shuffled = destroyableItems.OrderBy(x => UnityEngine.Random.value).ToList();
                for (int i = 0; i < destroyCount && i < shuffled.Count; i++)
                {
                    var item = shuffled[i];
                    ringCard.InventoryContainer.Remove(item);
                    item.DestroyCard(true, false);
                    AddLog($"💥 Nhẫn Trữ Vật: Tiêu hủy vật phẩm '{item.CardData.Name}'!");
                }
            }
        }

        private void EndCombat()
        {
            ProcessOrderingLossRules();
            IsCombatActive = false;
            WorldManager.WorldSimulationPaused = false;
            
            // Build and send result data
            CombatResultData resultData = new CombatResultData
            {
                Result = this.Result,
                EndReason = this.EndReason,
                CatOutcomes = new List<CatCombatOutcome>()
            };

            foreach (var unit in Formation.PlayerUnits)
            {
                if (unit.Source is CatCardData cat)
                {
                    resultData.CatOutcomes.Add(new CatCombatOutcome
                    {
                        CatReference = cat,
                        FinalHP = unit.CurrentHP,
                        FinalStamina = unit.Stamina,
                        WasDefeated = !unit.IsAlive,
                        BecameParalyzed = !unit.IsAlive,
                        WasExhausted = unit.IsExhausted
                    });
                }
            }

            // Close UI overlay after short delay
            StartCoroutine(CloseUiDelayRoutine(resultData));
        }

        private IEnumerator CloseUiDelayRoutine(CombatResultData resultData)
        {
            yield return new WaitForSeconds(3.0f); // Allow player to read end log

            if (CombatOverlayUI.Instance != null)
            {
                CombatOverlayUI.Instance.HideWindow();
            }

            _onCombatEnd?.Invoke(resultData);
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

        private void ExecuteBasicAttacks(CombatUnit unit, List<CombatUnit> allies, List<CombatUnit> opponents)
        {
            var target = CombatTargetResolver.GetPrimaryTarget(opponents, unit);
            if (target != null)
            {
                MewtationsWeaponRegistry.ExecuteBasicAttack(unit, target, allies, opponents, msg => AddLog(msg));
                if (unit.Source is CatCardData cat && cat.HasMutation(Mewtations.Expedition.UnstableMutation.DualWeapon))
                {
                    var target2 = CombatTargetResolver.GetPrimaryTarget(opponents, unit);
                    if (target2 != null)
                    {
                        AddLog($"⚔️ [SONG KIẾM HỢP BÍCH] {unit.Name} tung thêm nhát chém thứ hai bằng vũ khí phụ!");
                        MewtationsWeaponRegistry.ExecuteBasicAttack(unit, target2, allies, opponents, msg => AddLog(msg));
                    }
                }
            }
        }

        private static CombatSkillDefinition _defaultRageBurst;
        public static CombatSkillDefinition GetDefaultRageBurst()
        {
            if (_defaultRageBurst == null)
            {
                _defaultRageBurst = ScriptableObject.CreateInstance<CombatSkillDefinition>();
                _defaultRageBurst.Id = "combat_default_rage_burst";
                _defaultRageBurst.SkillNameKey = "combat_rage_burst";
                _defaultRageBurst.DescKey = "combat_rage_burst_desc";
                _defaultRageBurst.RequiredRage = 100;
                _defaultRageBurst.TargetType = SkillTargetType.SingleEnemy;
                _defaultRageBurst.FinalDamageMultiplier = 1.5f;
                _defaultRageBurst.HitCount = 1;
                _defaultRageBurst.RawAtkMultiplier = 1.0f;
            }
            return _defaultRageBurst;
        }
    }
}
