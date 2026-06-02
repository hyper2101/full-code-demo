using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mewtations.Combat;
using Mewtations.Combat.Core;

namespace Mewtations.Expedition
{
    public class ExpeditionManager : MonoBehaviour
    {
        public static ExpeditionManager Instance { get; private set; }

        public bool IsExpeditionActive = false;
        public ExpeditionState State = ExpeditionState.Idle;
        public ExpeditionRunState RunState = new ExpeditionRunState();
        public List<ExpeditionNode> MapNodes = new List<ExpeditionNode>();
        public ExpeditionNode ActiveNode = null;
        public List<CatCardData> ActiveCats = new List<CatCardData>();
        public CardData BackpackCardSource = null;
        public GameCard PortalCardSource = null;
        public int CurrentMapSeed = 0;
        public CardData RelicCardSource = null; // Translated comment
        
        public ExpeditionRunContext Context = null; // Translated comment


        public Dictionary<string, ExpeditionCatState> RuntimeCatStates = new Dictionary<string, ExpeditionCatState>();

        public List<CatCardData> GetExpeditionEligibleCats()
        {
            var eligible = new List<CatCardData>();
            foreach (var gameCard in WorldManager.instance.AllCards)
            {
                if (gameCard.MyBoard.IsCurrent && gameCard.CardData is CatCardData cat)
                {
                    if (cat.IsParalyzed || cat.HealthPoints <= 0)
                    {
                        continue; // Translated comment
                    }


                    eligible.Add(cat);
                }
            }
            return eligible;
        }

        private void Awake()
        {
            Instance = this;
        }

        public void StartExpedition(ExpeditionRunContext context)
        {
            if (IsExpeditionActive) return;

            // Translated comment
            if (GameScripts.Systems.Threat.ThreatManager.Instance != null && GameScripts.Systems.Threat.ThreatManager.Instance.HasActivePenalty(GameScripts.Systems.Threat.ThreatPenaltyType.LockExpedition))
            {
                WorldManager.instance.CreateFloatingText(context.Ordering != null ? context.Ordering.MyGameCard : null, false, 0, "(Translated Log)", "", false, 0, 2f, true);
                return;
            }

            var cats = GetExpeditionEligibleCats();
            if (cats.Count == 0)
            {
                WorldManager.instance.CreateFloatingText(context.Ordering != null ? context.Ordering.MyGameCard : null, false, 0, "(Translated Log)", "", false, 0, 2f, true);
                return;
            }

            int capacity = 10; // Translated comment
            int seed = UnityEngine.Random.Range(0, 100000);

            // Translated comment
            RelicCardSource = null;

            ExecuteStartExpedition(context, cats, capacity, seed);
        }

        private void ExecuteStartExpedition(ExpeditionRunContext context, List<CatCardData> cats, int capacity, int seed)
        {
            IsExpeditionActive = true;
            State = ExpeditionState.MapNavigation;
            
            // Translated comment
            int savedGreedAppeasement = RunState.BaseAppeasementGreed;
            int savedCorrAppeasement = RunState.BaseAppeasementCorruption;
            RunState.Clear();
            RunState.BaseAppeasementGreed = savedGreedAppeasement;
            RunState.BaseAppeasementCorruption = savedCorrAppeasement;
            
            if (context.Ordering != null && context.Ordering.MyGameCard != null && context.Ordering.MyGameCard.InventoryContainer != null)
            {
                foreach (var child in context.Ordering.MyGameCard.InventoryContainer.GetChildren())
                {
                    if (child != null && child.CardData != null && child.CardData.Id.StartsWith("item_ancient_relic_"))
                    {
                        RunState.ActiveRelicList.Add(child.CardData.Id);
                    }
                }
            }
            
            Context = context; // Translated comment

            PortalCardSource = null; // Translated comment
            ActiveCats = cats;
            BackpackCardSource = null; // Translated comment

            RuntimeCatStates.Clear();
            foreach (var cat in ActiveCats)
            {
                RuntimeCatStates[cat.UniqueId] = new ExpeditionCatState
                {
                    UniqueId = cat.UniqueId,
                    HP = cat.HealthPoints,
                    Stamina = cat.Stamina,
                    IsExhausted = cat.IsExhausted,
                    IsParalyzed = cat.IsParalyzed,
                    ExhaustionLevel = cat.ExhaustionLevel,
                    ParentCardUniqueId = (cat.MyGameCard != null && cat.MyGameCard.Parent != null) ? cat.MyGameCard.Parent.CardData.UniqueId : ""
                };
                
                // Translated comment
                if (cat.MyGameCard != null)
                {
                    var p = cat.MyGameCard.Parent;
                    var c = cat.MyGameCard.Child;
                    cat.MyGameCard.RemoveFromStack();
                    if (p != null && c != null) p.SetChild(c);
                    cat.MyGameCard.gameObject.SetActive(false);
                }
            }

            CurrentMapSeed = seed;
            MapNodes = ExpeditionMapGenerator.GenerateMap(seed, maxLayers: 6, maxNodesPerLayer: 3);
            ActiveNode = null;

            // Translated comment
            ExpeditionRiskSystem.InitializeRunStats(RunState);

            // Translated comment
            WorldManager.WorldSimulationPaused = true;

            // Translated comment
            if (ExpeditionMapUI.Instance != null)
            {
                ExpeditionMapUI.Instance.ShowWindow();
            }
            else
            {
                Debug.LogError("[ExpeditionManager] ExpeditionMapUI.Instance is null!");
            }

            Debug.Log("[Expedition] (Translated Log)"(Translated Log)"[Expedition] (Translated Log)");
                if (GameScripts.Systems.Threat.ThreatManager.Instance != null && GameScripts.Systems.Threat.ThreatManager.Instance.CatGodWrathTemplate != null)
                {
                    // Translated comment
                    GameScripts.Systems.Threat.ThreatManager.Instance.CreateThreat(
                        GameScripts.Systems.Threat.ThreatManager.Instance.CatGodWrathTemplate, 
                        GameScripts.Systems.Threat.ThreatSourceType.Expedition, 
                        RunState.CurrentDifficultyLevel, 
                        0 // Translated comment
                    );
                }
            }
            */
        }

        public void EnterNode(ExpeditionNode node)
        {
            if (node.State != NodeState.Available) return;

            ActiveNode = node;
            node.State = NodeState.Visited;
            State = ExpeditionState.InEncounter;
            RunState.CurrentLayer = node.Layer;
            
            if (node.Type == NodeType.SpecialMap)
            {
                if (Mewtations.Legacy.Stacklands.SaveManager.instance != null && Mewtations.Legacy.Stacklands.SaveManager.instance.CurrentSave != null)
                {
                    Mewtations.Legacy.Stacklands.SaveManager.instance.CurrentSave.ExpeditionSpecialMapPityCounter = 0;
                }
            }

            // Translated comment

            // Translated comment
            if (node.Theme == RouteTheme.TaDao)
            {
                RunState.AddCorruption(25);
                Debug.Log("[Expedition] (Translated Log)");
            }
            else if (node.Theme == RouteTheme.ThamLam)
            {
                RunState.AddGreed(10);
                Debug.Log("[Expedition] (Translated Log)");
            }

            // Translated comment
            if (ExpeditionMapUI.Instance != null)
            {
                ExpeditionMapUI.Instance.HideWindow();
            }

            Debug.Log($"[Expedition] Entered node {node.Id} ({node.Type}), Layer {node.Layer}. Theme: {node.Theme}. Biome: {node.Biome}.");

            IExpeditionEncounter encounter = null;
            switch (node.Type)
            {
                case NodeType.Combat:
                    encounter = new CombatEncounter(node.Layer, isBoss: false);
                    break;

                case NodeType.Boss:
                    encounter = new CombatEncounter(node.Layer, isBoss: true);
                    break;

                case NodeType.Resource:
                    encounter = new ResourceGatherEncounter();
                    break;

                case NodeType.Altar:
                    encounter = new CatGodAltarEncounter();
                    break;

                case NodeType.Ruins:
                    encounter = new MysteryMutationEncounter();
                    break;

                case NodeType.Elite:
                    encounter = new EliteEncounter(node.Layer);
                    break;

                case NodeType.Extraction:
                    encounter = new ExtractionEncounter();
                    break;

                case NodeType.SafeRetreat:
                    encounter = new SafeRetreatEncounter();
                    break;
            }

            if (encounter != null)
            {
                encounter.Resolve(() =>
                {
                    CompleteNodeResolution();
                });
            }
            else
            {
                // Translated comment
                TriggerTextEventNode(node.Type);
            }
        }

        private void TriggerCombat(bool isBoss)
        {
            // Translated comment
            List<Combatable> enemies = new List<Combatable>();
            int enemyCount = UnityEngine.Random.Range(1, 4);
            if (isBoss) enemyCount = 1; // Translated comment

            Vector3 spawnPos = Vector3.zero;
            for (int i = 0; i < enemyCount; i++)
            {
                string enemyId = isBoss ? "boss_goblin_king" : RollEnemyId(ActiveNode.Layer);
                var enemyCard = WorldManager.instance.CreateCard(spawnPos, enemyId, false, false, false);
                if (enemyCard != null && enemyCard.CardData is Combatable comb)
                {
                    enemies.Add(comb);
                }
            }

            // Translated comment
            List<Combatable> playerCombats = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Cast<Combatable>(System.Linq.Enumerable.Where(ActiveCats, c => c != null && !c.IsParalyzed)));
            TurnBasedCombatManager.Instance.StartCombat(playerCombats, enemies, (result) =>
            {
                // Translated comment
                foreach (var enemy in enemies)
                {
                    if (enemy != null && enemy.MyGameCard != null)
                    {
                        enemy.MyGameCard.DestroyCard(true, true);
                    }
                }

                if (result == CombatResult.Victory)
                {
                    // Translated comment
                    RollLootForCombat(isBoss);
                    CompleteNodeResolution();
                }
                else if (result == CombatResult.Retreated)
                {
                    // Translated comment
                    Debug.Log("[Expedition] (Translated Log)");
                    ReturnToBase(isDefeat: true);
                }
                else
                {
                    // Translated comment
                    ReturnToBase(isDefeat: true);
                }
            });
        }

        private string RollEnemyId(int layer)
        {
            // Pick enemies depending on depth
            string[] lowTier = { "goblin", "rat", "slime" };
            string[] medTier = { "skeleton", "wolf", "goblin" };
            string[] highTier = { "demon", "skeleton_mage", "wolf" };

            if (layer <= 1) return lowTier[UnityEngine.Random.Range(0, lowTier.Length)];
            if (layer <= 3) return medTier[UnityEngine.Random.Range(0, medTier.Length)];
            return highTier[UnityEngine.Random.Range(0, highTier.Length)];
        }

        private void RollLootForCombat(bool isBoss)
        {
            int lootCount = isBoss ? 4 : UnityEngine.Random.Range(1, 3);
            string[] possibleLoot = { "resource_gold", "resource_food", "item_healing_potion", "item_iron_ore", "item_wood", "item_stone" };

            List<string> rolled = new List<string>();
            for (int i = 0; i < lootCount; i++)
            {
                string loot = possibleLoot[UnityEngine.Random.Range(0, possibleLoot.Length)];
                rolled.Add(loot);
                RunState.PendingRewards.Add(loot);
            }

            // Translated comment
            if (isBoss)
            {
                string[] relics = { "item_ancient_relic_auto_farm", "item_ancient_relic_auto_collect", "item_ancient_relic_auto_heal" };
                string chosenRelic = relics[UnityEngine.Random.Range(0, relics.Length)];
                rolled.Add(chosenRelic);
                RunState.PendingRewards.Add(chosenRelic);
                Debug.Log("[Expedition] (Translated Log)");
            }

            string lootMsg = string.Join(", ", rolled.Select(id => id.Replace("resource_", "").Replace("item_", "")));
            Debug.Log("[Expedition] (Translated Log)");
        }

        private void TriggerResourceNode()
        {
            // Translated comment
            int lootCount = UnityEngine.Random.Range(2, 5);
            string[] resources = { "resource_food", "item_wood", "item_stone", "resource_gold", "item_iron_ore" };

            List<string> added = new List<string>();
            for (int i = 0; i < lootCount; i++)
            {
                string res = resources[UnityEngine.Random.Range(0, resources.Length)];
                RunState.PendingRewards.Add(res); if (true)
                {
                    added.Add(res);
                }
            }

            string resMsg = added.Count > 0 
                ? string.Join(", ", added.Select(id => id.Replace("resource_", "").Replace("item_", "")))
                : "(Translated Log)";

            // Translated comment
            string title = "(Translated Log)";
            string text = $"(Translated Log)";
            
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { "(Translated Log)" }, (choiceIdx) =>
            {
                CompleteNodeResolution();
            });
        }

        private void TriggerTextEventNode(NodeType type)
        {
            string title = "";
            string text = ""(Translated Log)"(Translated Log)";
                    text = "(Translated Log)";
                    choices = new List<string> {
                        "(Translated Log)",
                        "(Translated Log)",
                        "(Translated Log)"
                    };
                    onChoice = (idx) =>
                    {
                        if (idx == 0)
                        {
                            bool hasLightning = ActiveCats.Any(c => c.Element == CatElement.Lightning);
                            int avgSpeed = (int)ActiveCats.Average(c => c.Speed);
                            if (hasLightning || avgSpeed > 120)
                            {
                                var luckyCat = hasLightning ? ActiveCats.First(c => c.Element == CatElement.Lightning) : ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                luckyCat.Speed += 25;
                                luckyCat.AddMemoir(MemoirType.Breakthrough, "(Translated Log)", "(Translated Log)");
                                DialogueResult("(Translated Log)", $"(Translated Log)");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsUltimateLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, "(Translated Log)", "(Translated Log)");
                                foreach (var cat in ActiveCats)
                                {
                                    cat.HealthPoints = Mathf.Max(1, cat.HealthPoints - 10);
                                }
                                DialogueResult("(Translated Log)", $"(Translated Log)");
                            }
                        }
                        else if (idx == 1)
                        {
                            var tank = ActiveCats.Find(c => c.Role == CatRole.Tank);
                            if (tank != null)
                            {
                                tank.BaseCombatStats.MaxHealth += 10;
                                tank.HealthPoints = tank.ProcessedCombatStats.MaxHealth;
                                tank.IsPassiveSlotsLocked = true;
                                tank.AddMemoir(MemoirType.Breakthrough, "(Translated Log)", "(Translated Log)");
                                DialogueResult("(Translated Log)", $"(Translated Log)");
                            }
                            else
                            {
                                foreach (var cat in ActiveCats)
                                {
                                    cat.HealthPoints = Mathf.Max(1, cat.HealthPoints - 15);
                                }
                                DialogueResult("(Translated Log)", "(Translated Log)");
                            }
                        }
                        else
                        {
                            DialogueResult("(Translated Log)", "(Translated Log)");
                        }
                    };
                }
                else if (eventRoll == 1)
                {
                    // Translated comment
                    title = "(Translated Log)";
                    text = "(Translated Log)";
                    choices = new List<string> {
                        "(Translated Log)",
                        "(Translated Log)",
                        "(Translated Log)",
                        "(Translated Log)"
                    };
                    onChoice = (idx) =>
                    {
                        if (idx == 0)
                        {
                            if (ConsumeItemFromOrdering("resource_gold")) {
                                RunState.GreedLevel = Mathf.Max(0, RunState.GreedLevel - 20);
                                DialogueResult("(Translated Log)", "(Translated Log)");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsEquipmentSlotsLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, "(Translated Log)", "(Translated Log)");
                                DialogueResult("(Translated Log)", $"(Translated Log)");
                            }
                        }
                        else if (idx == 1)
                        {
                            foreach (var cat in ActiveCats)
                            {
                                cat.HealthPoints = Mathf.Max(1, cat.HealthPoints - 8);
                            }
                            RunState.PendingRewards.Add("resource_gold");
                            RunState.PendingRewards.Add("item_iron_ore");
                            RunState.AddCorruption(25);
                            DialogueResult("(Translated Log)", "(Translated Log)");
                        }
                        else if (idx == 2)
                        {
                            int avgSpeed = (int)ActiveCats.Average(c => c.Speed);
                            if (avgSpeed > 115)
                            {
                                DialogueResult("(Translated Log)", "(Translated Log)");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsEquipmentSlotsLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, "(Translated Log)", "(Translated Log)");
                                DialogueResult("(Translated Log)", $"(Translated Log)");
                            }
                        }
                        else if (idx == 3)
                        {
                            var zenCat = ActiveCats.Find(c => c.Specialization == Mewtations.Cards.Cats.DaoSpecialization.ZenDao);
                            if (zenCat != null)
                            {
                                RunState.PendingRewards.Add("item_heavenly_relic");
                                RunState.CorruptionLevel = Mathf.Max(0, RunState.CorruptionLevel - 20);
                                DialogueResult("Zen Awakening!", $"The Zen Dao cat calmed the guards. You gained a Heavenly Relic and reduced Corruption (-20)!");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.HealthPoints = Mathf.Max(1, victim.HealthPoints - 10);
                                DialogueResult("(Translated Log)", $"(Translated Log)");
                            }
                        }
                    };
                }
                else if (eventRoll == 2)
                {
                    // Translated comment
                    title = "(Translated Log)";
                    text = "(Translated Log)";
                    choices = new List<string> {
                        "(Translated Log)",
                        "(Translated Log)",
                        "(Translated Log)"
                    };
                    onChoice = (idx) =>
                    {
                        if (idx == 0)
                        {
                            var poisonCat = ActiveCats.Find(c => c.Element == CatElement.Poison);
                            if (poisonCat != null)
                            {
                                RunState.PendingRewards.Add("item_breakthrough_pill");
                                DialogueResult("Poison Immunity!", $"Your poison cat neutralized the toxic mist. You successfully obtained a Breakthrough Pill!");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsPillSlotLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, "(Translated Log)", "(Translated Log)");
                                DialogueResult("(Translated Log)", $"(Translated Log)");
                            }
                        }
                        else if (idx == 1)
                        {
                            if (UnityEngine.Random.value < 0.5f)
                            {
                                RunState.PendingRewards.Add("item_breakthrough_pill");
                                DialogueResult("(Translated Log)", "(Translated Log)");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsPillSlotLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, "(Translated Log)", "(Translated Log)");
                                DialogueResult("(Translated Log)", $"(Translated Log)");
                            }
                        }
                        else
                        {
                            RunState.PendingRewards.Add("item_stone");
                            RunState.PendingRewards.Add("item_iron_ore");
                            DialogueResult("(Translated Log)", "(Translated Log)");
                        }
                    };
                }
                else if (eventRoll == 3)
                {
                    // Translated comment
                    title = "(Translated Log)";
                    text = "(Translated Log)";
                    choices = new List<string> {
                        "(Translated Log)",
                        "(Translated Log)",
                        "(Translated Log)"
                    };
                    onChoice = (idx) =>
                    {
                        if (idx == 0)
                        {
                            if (ConsumeItemFromOrdering("food")) {
                                var lucky = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                string talent = UnityEngine.Random.value < 0.5f ? HeavenlyTalent.RageOvercharger : HeavenlyTalent.DivineShieldProtection;
                                lucky.AddTrait(talent);
                                lucky.CustomName = $"{HeavenlyTalent.GetDisplayName(talent)} {lucky.Name}";
                                lucky.AddMemoir(MemoirType.Breakthrough, HeavenlyTalent.GetDisplayName(talent), "(Translated Log)");
                                DialogueResult("(Translated Log)", $"(Translated Log)");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsFoodSlotLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, "(Translated Log)", "(Translated Log)");
                                DialogueResult("(Translated Log)", $"(Translated Log)");
                            }
                        }
                        else if (idx == 1)
                        {
                            var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                            victim.BreakthroughLevel++;
                            victim.BaseCombatStats.MaxHealth += 10;
                            victim.HealthPoints = victim.ProcessedCombatStats.MaxHealth;
                            victim.Speed += 15;
                            victim.IsFoodSlotLocked = true;
                            victim.AddMemoir(MemoirType.Breakthrough, "(Translated Log)", "(Translated Log)");
                            DialogueResult("(Translated Log)", $"(Translated Log)");
                        }
                        else
                        {
                            RunState.CorruptionLevel = Mathf.Max(0, RunState.CorruptionLevel - 25);
                            DialogueResult("(Translated Log)", "(Translated Log)");
                        }
                    };
                }
                else if (eventRoll == 4)
                {
                    // Translated comment
                    title = MewtationsLoc.Translate("exp_license_check_title", "(Translated Log)");
                    text = MewtationsLoc.Translate("exp_license_check_desc", "(Translated Log)"(Translated Log)"");
                    choices = new List<string> {
                        "(Translated Log)",
                        "(Translated Log)",
                        "(Translated Log)"
                    };
                    onChoice = (idx) =>
                    {
                        if (idx == 0)
                        {
                            if (ConsumeItemFromOrdering("resource_gold")) {
                                DialogueResult("(Translated Log)", "(Translated Log)"(Translated Log)"");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.HealthPoints = Mathf.Max(1, victim.HealthPoints - 10);
                                RunState.AddCorruption(20);
                                DialogueResult("(Translated Log)", $"(Translated Log)");
                            }
                        }
                        else if (idx == 1)
                        {
                            if (Context != null && Context.Ordering != null && Context.Ordering.MyGameCard != null && Context.Ordering.MyGameCard.InventoryContainer != null)
{
    var container = Context.Ordering.MyGameCard.InventoryContainer;
    var children = container.GetChildren();
    if (children.Count > 0)
    {
        int randIdx = UnityEngine.Random.Range(0, children.Count);
        string removed = children[randIdx].CardData.Id;
        children[randIdx].DestroyCard(true, true);
        DialogueResult("(Translated Log)", "(Translated Log)" + removed.Replace("item_", "").Replace("resource_", "") + "(Translated Log)");
    }
}
                            else
                            {
                                DialogueResult("(Translated Log)", "(Translated Log)");
                            }
                        }
                        else
                        {
                            int avgSpeed = (int)ActiveCats.Average(c => c.Speed);
                            if (avgSpeed > 120)
                            {
                                DialogueResult("(Translated Log)", "(Translated Log)");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.HealthPoints = Mathf.Max(1, victim.HealthPoints - 12);
                                RunState.AddCorruption(20);
                                DialogueResult("Failed Escape", $"Too slow! The patrol caught up and severely injured {victim.Name} (-12 HP), increasing fear (+20 Corruption)!"(Translated Log)"exp_beggar_title", "(Translated Log)");
                    text = MewtationsLoc.Translate("exp_beggar_desc", "(Translated Log)"(Translated Log)"");
                    choices = new List<string> {
                        "(Translated Log)",
                        "(Translated Log)"
                    };
                    onChoice = (idx) =>
                    {
                        if (idx == 0)
                        {
                            if (ConsumeItemFromOrdering("ore")) {
                                RunState.CorruptionLevel = Mathf.Max(0, RunState.CorruptionLevel - 30);
                                DialogueResult("(Translated Log)", "(Translated Log)");
                            }
                            else
                            {
                                DialogueResult("(Translated Log)", "(Translated Log)");
                            }
                        }
                        else
                        {
                            RunState.GreedLevel = Mathf.Min(100, RunState.GreedLevel + 15);
                            DialogueResult("(Translated Log)", "(Translated Log)");
                        }
                    };
                }
                else
                {
                    // Translated comment
                    int maxBreakthrough = ActiveCats.Count > 0 ? ActiveCats.Max(c => c.BreakthroughLevel) : 0;
                    title = MewtationsLoc.Translate("exp_merchant_encounter_title", "(Translated Log)");
                    if (maxBreakthrough >= 2)
                    {
                        text = MewtationsLoc.Translate("exp_merchant_high_rank_desc", "(Translated Log)"(Translated Log)"");
                        choices = new List<string> {
                            "(Translated Log)",
                            "(Translated Log)",
                            "(Translated Log)"
                        };
                        onChoice = (idx) =>
                        {
                            if (idx == 0 || idx == 1)
                            {
                                if (ConsumeItemFromOrdering("resource_gold")) {
                                    string itemSpawn = idx == 0 ? "item_revive_pill" : "item_breakthrough_pill";
                                    RunState.PendingRewards.Add(itemSpawn);
                                    DialogueResult("(Translated Log)", $"(Translated Log)"item_", ""(Translated Log)");
                                }
                                else
                                {
                                    DialogueResult("(Translated Log)", "(Translated Log)"(Translated Log)"");
                                }
                            }
                            else
                            {
                                CompleteNodeResolution();
                            }
                        };
                    }
                    else
                    {
                        text = MewtationsLoc.Translate("exp_merchant_low_rank_desc", "(Translated Log)"(Translated Log)"");
                        choices = new List<string> {
                            "(Translated Log)",
                            "(Translated Log)"
                        };
                        onChoice = (idx) =>
                        {
                            if (idx == 0)
                            {
                                if (ConsumeItemFromOrdering("resource_gold")) {
                                    RunState.PendingRewards.Add("item_iron_ore");
                                    DialogueResult("(Translated Log)", "(Translated Log)");
                                }
                                else
                                {
                                    DialogueResult("(Translated Log)", "(Translated Log)"(Translated Log)"");
                                }
                            }
                            else
                            {
                                CompleteNodeResolution();
                            }
                        };
                    }
                }
            }
            else if (type == NodeType.CampHealer)
            {
                title = "??? CAMP HEALER";
                text = "(Translated Log)";
                choices = new List<string> {
                    "(Translated Log)",
                    "(Translated Log)",
                    "B? qua"
                };
                onChoice = (idx) =>
                {
                    if (idx == 0)
                    {
                        int pool = 100;
                        int needHealCount = ActiveCats.Count(c => c != null && c.HealthPoints < c.ProcessedCombatStats.MaxHealth);
                        if (needHealCount > 0)
                        {
                            int healPerCat = pool / needHealCount;
                            foreach(var cat in ActiveCats)
                            {
                                if(cat != null && cat.HealthPoints < cat.ProcessedCombatStats.MaxHealth) 
                                {
                                    cat.HealthPoints = Mathf.Min(cat.ProcessedCombatStats.MaxHealth, cat.HealthPoints + healPerCat);
                                }
                            }
                            DialogueResult("Restored Vitality", $"Your squad recovered {$pool} HP from the Healing Pool.");
                        }
                        else 
                        {
                            DialogueResult("(Translated Log)", "(Translated Log)");
                        }
                    }
                    else if (idx == 1)
                    {
                        foreach(var cat in ActiveCats) 
                        {
                            if(cat != null) 
                            {
                                cat.IsParalyzed = false;
                                cat.IsExhausted = false;
                            }
                        }
                        DialogueResult("(Translated Log)", "(Translated Log)");
                    }
                    else
                    {
                        CompleteNodeResolution();
                    }
                };
            }
            else if (type == NodeType.CampMerchant)
            {
                title = "(Translated Log)";
                text = "(Translated Log)";
                choices = new List<string> {
                    "(Translated Log)",
                    "(Translated Log)"
                };
                onChoice = (idx) =>
                {
                    if (idx == 0)
                    {
                        // (Removed IsFull check)
                        
                        if (ConsumeItemFromOrdering("food")) {
                            RunState.PendingRewards.Add("item_ancient_relic_auto_collect");
                            DialogueResult("(Translated Log)", "(Translated Log)");
                        }
                        else if (ConsumeItemFromOrdering("resource_gold")) {
                            RunState.PendingRewards.Add("item_ancient_relic_auto_farm");
                            DialogueResult("(Translated Log)", "(Translated Log)");
                        }
                        else
                        {
                            DialogueResult("(Translated Log)", "(Translated Log)");
                        }
                        CompleteNodeResolution();
                    }
                };
            }
            else if (type == NodeType.CampBlacksmith)
            {
                title = "(Translated Log)";
                text = "(Translated Log)";
                choices = new List<string> {
                    "(Translated Log)",
                    "(Translated Log)"
                };
                onChoice = (idx) =>
                {
                    if (idx == 0)
                    {
                        if (ConsumeItemFromOrdering("ore")) {
                            foreach(var cat in ActiveCats) { if(cat != null) { cat.HealthPoints += 5; cat.Stamina += 10; } }
                            DialogueResult("(Translated Log)", "(Translated Log)");
                        }
                        else
                        {
                            DialogueResult("(Translated Log)", "(Translated Log)");
                        }
                    }
                    else
                    {
                        CompleteNodeResolution();
                    }
                };
            }
            else if (type == NodeType.Reward)
            {
                title = "(Translated Log)";
                text = "(Translated Log)";
                choices = new List<string> {
                    "(Translated Log)"
                };
                onChoice = (idx) =>
                {
                    if (ExpeditionRewardUI.Instance != null)
                      {
                          int rewardCount = UnityEngine.Random.Range(2, 6);
                          List<string> rewards = new List<string>();
                          string[] pool = { "card_reward_pack", "resource_gold", "resource_food", "item_iron_ore", "item_wood" };
                          for (int i = 0; i < rewardCount; i++) rewards.Add(pool[UnityEngine.Random.Range(0, pool.Length)]);
                          ExpeditionRewardUI.Instance.ShowRewards(rewards);
                      }
                      else
                      {
                          CompleteNodeResolution();
                      }
                };
            }
            else if (type == NodeType.Lore)
            {
                if (UnityEngine.Random.value <= 0.50f)
                {
                    TriggerWearyDogEncounter();
                    return; // Translated comment
                }
                else
                {
                    title = "(Translated Log)";
                    text = "(Translated Log)";
                    choices = new List<string> { "(Translated Log)" };
                    onChoice = (idx) =>
                    {
                        foreach (var cat in ActiveCats)
                        {
                            cat.Speed += 10;
                        }
                        CompleteNodeResolution();
                    };
                }
            }
            else // Ruins
            {
                title = "(Translated Log)";
                text = "(Translated Log)";
                choices = new List<string> { "(Translated Log)", "(Translated Log)" };
                onChoice = (idx) =>
                {
                    if (idx == 0)
                    {
                        if (UnityEngine.Random.value < 0.5f)
                        {
                            RunState.PendingRewards.Add("item_revive_pill");
                            DialogueResult("(Translated Log)", "(Translated Log)");
                        }
                        else
                        {
                            DialogueResult("(Translated Log)", "(Translated Log)");
                        }
                    }
                    else
                    {
                        CompleteNodeResolution();
                    }
                };
            }

            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, choices, onChoice);
        }

        private void TriggerWearyDogEncounter()
        {
            string title = MewtationsLoc.Translate("dog_patrol_title", "THE WEARY DOG PATROL OFFICER");
            int maxBreakthrough = ActiveCats.Count > 0 ? ActiveCats.Max(c => c.BreakthroughLevel) : 0;
            string text = MewtationsLoc.Translate("dog_patrol_desc");
            if (maxBreakthrough >= 2)
            {
                text = MewtationsLoc.Translate("dog_patrol_high_rank_desc", text);
            }
            else
            {
                text = MewtationsLoc.Translate("dog_patrol_low_rank_desc", text);
            }

            List<Mewtations.Dialogue.DialogueChoice> choices = new List<Mewtations.Dialogue.DialogueChoice>();

            // Option 1: Fight
            choices.Add(new Mewtations.Dialogue.DialogueChoice(
                MewtationsLoc.Translate("opt_fight", "(Translated Log)"),
                () =>
                {
                    RunState.AddCorruption(20);
                    DialogueResult(
                        MewtationsLoc.Translate("dog_fight_res", "Bloody Skirmish!"),
                        MewtationsLoc.Translate("dog_fight_res_desc", "You fought and defeated the guard. The path is clear, but at a bloody cost (+20 Corruption).")
                    );
                }
            ));

            // Translated comment
            choices.Add(new Mewtations.Dialogue.DialogueChoice(
                MewtationsLoc.Translate("opt_stealth", "(Translated Log)"),
                () =>
                {
                    int avgSpeed = 100;
                    if (ActiveCats.Count > 0)
                    {
                        avgSpeed = (int)ActiveCats.Average(c => c.Speed);
                    }

                    if (avgSpeed > 115)
                    {
                        DialogueResult(
                            MewtationsLoc.Translate("dog_stealth_success", "Stealth Success!"),
                            MewtationsLoc.Translate("dog_stealth_success_desc", "Your agile cats slipped by in the shadows without alerting the guard.")
                        );
                    }
                    else
                    {
                        foreach (var cat in ActiveCats)
                        {
                            cat.HealthPoints = Mathf.Max(1, cat.HealthPoints - 5);
                        }
                        DialogueResult(
                            MewtationsLoc.Translate("dog_stealth_fail", "Stealth Failed!"),
                            MewtationsLoc.Translate("dog_stealth_fail_desc", "The weary guard noticed you. You had to force your way through and suffered minor injuries (-5 HP).")
                        );
                    }
                }
            ));

            // Translated comment
            choices.Add(new Mewtations.Dialogue.DialogueChoice(
                MewtationsLoc.Translate("opt_comfort", "(Translated Log)"),
                () =>
                {
                    string hintId = "item_secret_lore_hint_1";
                    if (ChronicleManager.IsHintUnlocked("item_secret_lore_hint_1"))
                    {
                        if (ChronicleManager.IsHintUnlocked("item_secret_lore_hint_2"))
                        {
                            hintId = "item_secret_lore_hint_3";
                        }
                        else
                        {
                            hintId = "item_secret_lore_hint_2";
                        }
                    }

                    ChronicleManager.UnlockHint(hintId);
                    RunState.PendingRewards.Add(hintId);

                    RunState.CorruptionLevel = Mathf.Max(0, RunState.CorruptionLevel - 25);

                    DialogueResult(
                        MewtationsLoc.Translate("dog_comfort_success", "A Soul Redeemed!"),
                        MewtationsLoc.Translate("dog_comfort_success_desc", "The officer wept upon hearing your Zen words, realizing both Cats and Dogs are victims of the system. He abandons his post, giving you an Ancient Scroll and purging your sins (-25 Corruption)!")
                    );
                },
                () => ActiveCats.Any(c => c.Specialization == Mewtations.Cards.Cats.DaoSpecialization.ZenDao),
                MewtationsLoc.Translate("opt_comfort_req", "(Translated Log)")
            ));

            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, choices);
        }

        private void DialogueResult(string title, string text)
        {
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { "(Translated Log)" }, (idx) =>
            {
                CompleteNodeResolution();
            });
        }

                        public bool ConsumeItemFromOrdering(string itemId)
        {
            if (Context == null || Context.Ordering == null || Context.Ordering.MyGameCard == null) return false;
            var container = Context.Ordering.MyGameCard.InventoryContainer;
            if (container == null) return false;
            foreach (var child in container.GetChildren())
            {
                if (child != null && child.CardData != null && child.CardData.Id == itemId)
                {
                    child.DestroyCard(true, true);
                    return true;
                }
            }
            return false;
        }
        
        public void CompleteNodeResolution()
        {
            if (RunState.PendingRewards.Count > 0)
            {
                var rewards = new List<string>(RunState.PendingRewards);
                RunState.PendingRewards.Clear();
                if (ExpeditionRewardUI.Instance != null)
                {
                    ExpeditionRewardUI.Instance.ShowRewards(rewards);
                    return; // Translated comment
                }
            }

            if (ActiveNode != null && ActiveNode.Type == NodeType.SpecialMap)
            {
                if (Mewtations.Legacy.Stacklands.SaveManager.instance != null && Mewtations.Legacy.Stacklands.SaveManager.instance.CurrentSave != null)
                {
                    if (Mewtations.Legacy.Stacklands.SaveManager.instance.CurrentSave.CompletedSpecialMaps == null)
                    {
                        Mewtations.Legacy.Stacklands.SaveManager.instance.CurrentSave.CompletedSpecialMaps = new List<string>();
                    }
                    if (!Mewtations.Legacy.Stacklands.SaveManager.instance.CurrentSave.CompletedSpecialMaps.Contains(ActiveNode.Theme.ToString()))
                    {
                        Mewtations.Legacy.Stacklands.SaveManager.instance.CurrentSave.CompletedSpecialMaps.Add(ActiveNode.Theme.ToString());
                    }
                }
            }
            State = ExpeditionState.MapNavigation;

            // Translated comment
            ApplyRelicAutomationProgress();

            // Translated comment
            UpdateConnections();

            // Translated comment
            if (ActiveNode != null && ActiveNode.Type == NodeType.Boss)
            {
                Debug.Log("[Expedition] (Translated Log)");
                ReturnToBase(isDefeat: false);
            }
            else
            {
                // Translated comment
                if (ExpeditionMapUI.Instance != null)
                {
                    ExpeditionMapUI.Instance.ShowWindow();
                }
            }
        }

        private void ApplyRelicAutomationProgress()
        {
            if (RunState == null || RunState.ActiveRelicList.Count == 0) return;

            foreach (var relic in RunState.ActiveRelicList) {
            Debug.Log("[Expedition] (Translated Log)");

            foreach (var gc in WorldManager.instance.AllCards)
            {
                if (gc != null && !gc.Destroyed && gc.CardData != null && gc.TimerRunning)
                {
                    string cid = gc.CardData.Id.ToLower();
                    
                    if (relic == "item_ancient_relic_smelt" && (cid.Contains("smelter") || cid.Contains("furnace")))
                    {
                        gc.CurrentTimerTime += 15f; // Translated comment
                        Debug.Log("[Expedition] (Translated Log)");
                    }
                    else if (relic == "item_ancient_relic_wood" && (cid.Contains("sawmill") || cid.Contains("mill")))
                    {
                        gc.CurrentTimerTime += 15f; // Translated comment
                        Debug.Log("[Expedition] (Translated Log)");
                    }
                    else if (relic == "item_ancient_relic_booster")
                    {
                        gc.CurrentTimerTime += 5f; // Translated comment
                        Debug.Log("[Expedition] (Translated Log)");
                    }
                }
            }
        }

        private void UpdateConnections()
        {
            if (ActiveNode == null) return;

            // Translated comment
            foreach (var n in MapNodes)
            {
                if (n.State == NodeState.Available)
                {
                    n.State = NodeState.Locked;
                }
            }

            // Translated comment
            foreach (int connectedId in ActiveNode.OutgoingConnections)
            {
                var targetNode = MapNodes.Find(n => n.Id == connectedId);
                if (targetNode != null && targetNode.State == NodeState.Locked)
                {
                    targetNode.State = NodeState.Available;
                }
            }

            // Translated comment
            if (!MapNodes.Any(n => n.State == NodeState.Visited))
            {
                foreach (var n in MapNodes.Where(n => n.Layer == 0))
                {
                    n.State = NodeState.Available;
                }
            }
        }

        public void ReturnToBase(bool isDefeat, bool isManualRetreat = false)
        {
            if (Mewtations.Legacy.Stacklands.SaveManager.instance != null && Mewtations.Legacy.Stacklands.SaveManager.instance.CurrentSave != null)
            {
                // Translated comment
                bool visitedSpecial = MapNodes != null && MapNodes.Any(n => n.State == NodeState.Visited && n.Type == NodeType.SpecialMap);
                if (!visitedSpecial)
                {
                    Mewtations.Legacy.Stacklands.SaveManager.instance.CurrentSave.ExpeditionSpecialMapPityCounter++;
                }
            }
            IsExpeditionActive = false;
            State = ExpeditionState.Idle;

            // Translated comment
            if (ExpeditionMapUI.Instance != null) ExpeditionMapUI.Instance.HideWindow();
            if (CombatOverlayUI.Instance != null) CombatOverlayUI.Instance.HideWindow();
            if (Mewtations.Dialogue.DialogueSystem.Instance != null) Mewtations.Dialogue.DialogueSystem.Instance.HideWindow();

            // Translated comment
            WorldManager.WorldSimulationPaused = false;

            if (Context != null && Context.Ordering != null && Context.Ordering.MyGameCard != null)
            {
                var gatewayCard = Context.Ordering.MyGameCard.Parent;
                Vector3 spawnPos = (gatewayCard != null ? gatewayCard.transform.position : Context.Ordering.MyGameCard.transform.position) + Vector3.back * 1.5f;

                // Translated comment
                Context.Ordering.MyGameCard.RemoveFromStack();
                Context.Ordering.MyGameCard.transform.position = spawnPos + Vector3.right * 1.5f;
                WorldManager.instance.SendToBoard(Context.Ordering.MyGameCard, WorldManager.instance.CurrentBoard, Context.Ordering.MyGameCard.transform.position);

                // Translated comment
                foreach (var cat in ActiveCats)
                {
                    if (cat != null)
                    {
                        cat.ClearMutations(); // Translated comment
                        
                        // Translated comment
                        if (RuntimeCatStates.TryGetValue(cat.UniqueId, out var state))
                        {
                            cat.HealthPoints = state.HP;
                            cat.Stamina = state.Stamina;
                            cat.IsExhausted = state.IsExhausted;
                            cat.IsParalyzed = state.IsParalyzed;
                            cat.ExhaustionLevel = state.ExhaustionLevel;
                        }

                        // Translated comment
                        int staminaDebt = 20; // Translated comment
                        if (RunState != null) {
                            staminaDebt += (RunState.CurrentLayer * 5); // Translated comment
                        }
                        cat.Stamina = UnityEngine.Mathf.Max(0, cat.Stamina - staminaDebt);
                        
                        // Translated comment
                        if (cat.Stamina == 0) {
                            cat.AddMemoir("Returned in an exhausted state! (Exhausted Return)");
                        }
                        if (RunState != null && RunState.CorruptionLevel > 50) {
                            cat.AddMemoir("Returned with dark aura (Corrupted Return)");
                        }
                          int insuredSlots = 0;
                          if (BackpackCardSource is Mewtations.Legacy.Stacklands.OrderingCardData ringData) insuredSlots = ringData.InsuredSlots;
                        if (isManualRetreat) {
                            cat.AddMemoir("Fled from the expedition (Retreat)");
                        }

                        if (cat.MyGameCard != null)
                        {
                            cat.MyGameCard.gameObject.SetActive(true);
                            if (RuntimeCatStates.TryGetValue(cat.UniqueId, out var state) && !cat.IsParalyzed && !cat.IsExhausted && !string.IsNullOrEmpty(state.ParentCardUniqueId))
                            {
                                GameCard parentCard = WorldManager.instance.GetCardWithUniqueId(state.ParentCardUniqueId);
                                if (parentCard != null && parentCard.gameObject.activeInHierarchy && !parentCard.CardData.IsPendingDestruction)
                                {
                                    cat.MyGameCard.SetParent(parentCard);
                                }
                                else
                                {
                                    cat.MyGameCard.RemoveFromStack();
                                    cat.MyGameCard.transform.position = spawnPos;
                                    WorldManager.instance.SendToBoard(cat.MyGameCard, WorldManager.instance.CurrentBoard, spawnPos);
                                }
                            }
                            else
                            {
                                cat.MyGameCard.RemoveFromStack();
                                cat.MyGameCard.transform.position = spawnPos;
                                WorldManager.instance.SendToBoard(cat.MyGameCard, WorldManager.instance.CurrentBoard, spawnPos);
                            }
                        }
                    }
                }

                if (!isDefeat)
                {
                    // Translated comment
                    MutationPersistenceSystem.ProcessRunVictoryTraits(ActiveCats);

                    // Translated comment
                    

                    // Translated comment
                    if (ActiveNode != null && ActiveNode.Type == NodeType.Boss)
                    {
                        var summoning = new CatSummoningSystem(WorldManager.instance);
                        summoning.SummonCat(spawnPos, highestBreakthroughLevel: 2); // Translated comment
                        Debug.Log("[Expedition] (Translated Log)");
                    }
                }
                else
                {
                    // Translated comment
                    if (Context != null && Context.Ordering != null && Context.Ordering.MyGameCard != null && Context.Ordering.MyGameCard.InventoryContainer != null) { int insuredSlots = Context.Ordering.InsuredSlots;
                        if (isManualRetreat)
                        {
                            // Translated comment
                            if (RunState != null)
                            {
                                RunState.GreedLevel = Mathf.Min(100, RunState.GreedLevel + 15);
                            }
                            ExpeditionExtractionSystem.ApplyManualRetreatPenalty(Context.Ordering.MyGameCard.InventoryContainer, insuredSlots);
                            Debug.Log("[Expedition] (Translated Log)");
                        }
                        else
                        {
                            float rate = ExpeditionExtractionSystem.CalculateLootRetentionRate(RunState, Context.Ordering.MyGameCard.InventoryContainer, Context.Ordering.StorageCapacity);
                              ExpeditionExtractionSystem.ApplyAbandonPenalty(Context.Ordering.MyGameCard.InventoryContainer, rate, insuredSlots);
                            Debug.Log("[Expedition] (Translated Log)");
                        }
                        
                    }
                }

                // Translated comment
                if (BackpackCardSource != null && BackpackCardSource.MyGameCard != null)
                {
                    BackpackCardSource.MyGameCard.transform.position = spawnPos + Vector3.right * 1.0f;
                    BackpackCardSource.MyGameCard.gameObject.SetActive(true);
                }

                // Translated comment
                if (RelicCardSource != null && RelicCardSource.MyGameCard != null)
                {
                    RelicCardSource.MyGameCard.transform.position = spawnPos + Vector3.left * 1.0f;
                    RelicCardSource.MyGameCard.gameObject.SetActive(true);
                }
                RelicCardSource = null;
                RunState.EquippedRelicId = "";

                // Translated comment
                if (PortalCardSource.CardData.Id == "strange_portal")
                {
                    PortalCardSource.DestroyCard(false, true);
                }
            }

            Debug.Log("[Expedition] (Translated Log)");
        }

        public void SaveToExtraKeyValues(List<SerializedKeyValuePair> list)
        {
            if (list == null) return;
            
            // Persist unlocked hints
            list.SetOrAdd("Mewtations_UnlockedHints", ChronicleManager.Serialize());

            list.SetOrAdd("Expedition_IsActive", IsExpeditionActive.ToString());
            if (!IsExpeditionActive) return;

            list.SetOrAdd("Expedition_State", ((int)State).ToString());
            list.SetOrAdd("Expedition_PortalCardUniqueId", PortalCardSource != null ? PortalCardSource.UniqueId : "");
            list.SetOrAdd("Expedition_BackpackCardUniqueId", BackpackCardSource != null ? BackpackCardSource.UniqueId : "");
            list.SetOrAdd("Expedition_RelicCardUniqueId", RelicCardSource != null ? RelicCardSource.UniqueId : "");
            list.SetOrAdd("Expedition_ActiveRelicList", string.Join(",", RunState.ActiveRelicList));
            list.SetOrAdd("Expedition_ActiveCatsUniqueIds", string.Join(",", ActiveCats.Select(c => c.UniqueId)));
            list.SetOrAdd("Expedition_GreedLevel", RunState.GreedLevel.ToString());
            list.SetOrAdd("Expedition_CorruptionLevel", RunState.CorruptionLevel.ToString());
            list.SetOrAdd("Expedition_CurrentLayer", RunState.CurrentLayer.ToString());
            list.SetOrAdd("Expedition_TotalGoldCollected", RunState.TotalGoldCollected.ToString());
            list.SetOrAdd("Expedition_BaseAppeasementGreed", RunState.BaseAppeasementGreed.ToString());
            list.SetOrAdd("Expedition_BaseAppeasementCorruption", RunState.BaseAppeasementCorruption.ToString());
            list.SetOrAdd("Expedition_ActiveMutations", string.Join(",", RunState.RunActiveMutations));

            list.SetOrAdd("Expedition_ActiveNodeId", ActiveNode != null ? ActiveNode.Id.ToString() : "-1");
            list.SetOrAdd("Expedition_MapSeed", CurrentMapSeed.ToString());
            list.SetOrAdd("Expedition_MapNodeStates", string.Join(",", MapNodes.Select(n => ((int)n.State).ToString())));
        }

        private string GetValueOrDefault(List<SerializedKeyValuePair> list, string key, string defaultValue)
        {
            var pair = list.GetWithKey(key);
            return pair != null ? pair.Value : defaultValue;
        }

        public void LoadFromExtraKeyValues(List<SerializedKeyValuePair> list)
        {
            if (list == null)
            {
                IsExpeditionActive = false;
                State = ExpeditionState.Idle;
                ChronicleManager.Reset();
                return;
            }

            // Translated comment
            string unlockedHints = GetValueOrDefault(list, "Mewtations_UnlockedHints", "");
            ChronicleManager.Deserialize(unlockedHints);

            var activePair = list.GetWithKey("Expedition_IsActive");
            if (activePair == null || activePair.Value != "True")
            {
                IsExpeditionActive = false;
                State = ExpeditionState.Idle;
                return;
            }

            IsExpeditionActive = true;
            State = (ExpeditionState)int.Parse(GetValueOrDefault(list, "Expedition_State", "0"));

            string portalUid = GetValueOrDefault(list, "Expedition_PortalCardUniqueId", "");
            if (!string.IsNullOrEmpty(portalUid) && WorldManager.instance.UniqueIdToCard.TryGetValue(portalUid, out var portalGameCard))
            {
                PortalCardSource = portalGameCard;
            }

            string backpackUid = GetValueOrDefault(list, "Expedition_BackpackCardUniqueId", "");
            if (!string.IsNullOrEmpty(backpackUid) && WorldManager.instance.UniqueIdToCard.TryGetValue(backpackUid, out var backpackGameCard))
            {
                BackpackCardSource = backpackGameCard.CardData;
            }

            string relicUid = GetValueOrDefault(list, "Expedition_RelicCardUniqueId", "");
            if (!string.IsNullOrEmpty(relicUid) && WorldManager.instance.UniqueIdToCard.TryGetValue(relicUid, out var relicGameCard))
            {
                RelicCardSource = relicGameCard.CardData;
            }
            else
            {
                RelicCardSource = null;
            }
            
            string activeRelicsStr = GetValueOrDefault(list, "Expedition_ActiveRelicList", "");
            RunState.ActiveRelicList.Clear();
            if (!string.IsNullOrEmpty(activeRelicsStr))
            {
                RunState.ActiveRelicList.AddRange(activeRelicsStr.Split(','));
            }

            string activeCatsUidsStr = GetValueOrDefault(list, "Expedition_ActiveCatsUniqueIds", "");
            ActiveCats.Clear();
            if (!string.IsNullOrEmpty(activeCatsUidsStr))
            {
                foreach (string uid in activeCatsUidsStr.Split(','))
                {
                    if (WorldManager.instance.UniqueIdToCard.TryGetValue(uid, out var catGameCard) && catGameCard.CardData is CatCardData catData)
                    {
                        ActiveCats.Add(catData);
                    }
                }
            }

            string backpackItemsStr = GetValueOrDefault(list, "Expedition_BackpackItems", "");
            if (!string.IsNullOrEmpty(backpackItemsStr))
            {
                foreach (string item in backpackItemsStr.Split(','))
                {
                    RunState.PendingRewards.Add(item);
                }
            }

            RunState.Clear();
            RunState.GreedLevel = int.Parse(GetValueOrDefault(list, "Expedition_GreedLevel", "0"));
            RunState.CorruptionLevel = int.Parse(GetValueOrDefault(list, "Expedition_CorruptionLevel", "0"));
            RunState.CurrentLayer = int.Parse(GetValueOrDefault(list, "Expedition_CurrentLayer", "0"));
            RunState.TotalGoldCollected = int.Parse(GetValueOrDefault(list, "Expedition_TotalGoldCollected", "0"));
            RunState.BaseAppeasementGreed = int.Parse(GetValueOrDefault(list, "Expedition_BaseAppeasementGreed", "0"));
            RunState.BaseAppeasementCorruption = int.Parse(GetValueOrDefault(list, "Expedition_BaseAppeasementCorruption", "0"));

            string mutationsStr = GetValueOrDefault(list, "Expedition_ActiveMutations", "");
            if (!string.IsNullOrEmpty(mutationsStr))
            {
                RunState.RunActiveMutations = mutationsStr.Split(',').ToList();
            }

            CurrentMapSeed = int.Parse(GetValueOrDefault(list, "Expedition_MapSeed", "0"));
            MapNodes = ExpeditionMapGenerator.GenerateMap(CurrentMapSeed, maxLayers: 6, maxNodesPerLayer: 3);

            string nodeStatesStr = GetValueOrDefault(list, "Expedition_MapNodeStates", "");
            if (!string.IsNullOrEmpty(nodeStatesStr))
            {
                var states = nodeStatesStr.Split(',').Select(int.Parse).ToList();
                for (int i = 0; i < MapNodes.Count && i < states.Count; i++)
                {
                    MapNodes[i].State = (NodeState)states[i];
                }
            }

            int activeNodeId = int.Parse(GetValueOrDefault(list, "Expedition_ActiveNodeId", "-1"));
            ActiveNode = activeNodeId >= 0 ? MapNodes.Find(n => n.Id == activeNodeId) : null;


            // Translated comment
            foreach (var cat in ActiveCats)
            {
                if (cat != null && cat.MyGameCard != null)
                {
                    var p = cat.MyGameCard.Parent;
                    var c = cat.MyGameCard.Child;
                    cat.MyGameCard.RemoveFromStack();
                    if (p != null && c != null) p.SetChild(c);
                    cat.MyGameCard.gameObject.SetActive(false);
                }
            }
            if (BackpackCardSource != null && BackpackCardSource.MyGameCard != null)
            {
                BackpackCardSource.MyGameCard.gameObject.SetActive(false);
            }

            // Translated comment
            if (State == ExpeditionState.MapNavigation && ExpeditionMapUI.Instance != null)
            {
                ExpeditionMapUI.Instance.ShowWindow();
            }
        }
    }
}















