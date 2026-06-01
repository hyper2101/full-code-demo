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
        public Backpack CurrentBackpack = null;
        public GameCard PortalCardSource = null;
        public int CurrentMapSeed = 0;
        public CardData RelicCardSource = null; // Cá»• váº­t Ä‘ang mang theo

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
                        continue; // Imprisoned/Dead equivalent. Allow exhausted/injured.
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

           public void StartExpedition(GameCard portalCard, CardData backpackCard, CardData relicCard = null)
        {
            if (IsExpeditionActive) return;

            // Kiá»ƒm tra Threat Lock
            if (GameScripts.Systems.Threat.ThreatManager.Instance != null && GameScripts.Systems.Threat.ThreatManager.Instance.HasActivePenalty(GameScripts.Systems.Threat.ThreatPenaltyType.LockExpedition))
            {
                WorldManager.instance.CreateFloatingText(portalCard, false, 0, "KhÃ´ng thá»ƒ khá»Ÿi hÃ nh! LÃ£nh Ä‘á»‹a Ä‘ang bá»‹ Ä‘e dá»a.", "", false, 0, 2f, true);
                return;
            }

            var cats = GetExpeditionEligibleCats();
            if (cats.Count == 0)
            {
                WorldManager.instance.CreateFloatingText(portalCard, false, 0, "KhÃ´ng cÃ³ mÃ¨o nÃ o Ä‘á»§ sá»©c khá»e Ä‘á»ƒ Ä‘i viá»…n chinh!", "", false, 0, 2f, true);
                return;
            }

            int capacity = (backpackCard != null && backpackCard.BackpackCapacity > 0) ? backpackCard.BackpackCapacity : 10;
            int seed = UnityEngine.Random.Range(0, 100000);

            RelicCardSource = relicCard;
            if (relicCard != null)
            {
                RunState.EquippedRelicId = relicCard.Id;
            }
            else
            {
                RunState.EquippedRelicId = "";
            }

            ExecuteStartExpedition(portalCard, cats, backpackCard, capacity, seed);
        }

        private void ExecuteStartExpedition(GameCard portalCard, List<CatCardData> cats, CardData backpackCard, int capacity, int seed)
        {
            IsExpeditionActive = true;
            State = ExpeditionState.MapNavigation;
            
            // Preserve base sacrifice appeasement points from world state before clearing the run
            int savedGreedAppeasement = RunState.BaseAppeasementGreed;
            int savedCorrAppeasement = RunState.BaseAppeasementCorruption;
            RunState.Clear();
            RunState.BaseAppeasementGreed = savedGreedAppeasement;
            RunState.BaseAppeasementCorruption = savedCorrAppeasement;

            PortalCardSource = portalCard;
            ActiveCats = cats;
            BackpackCardSource = backpackCard;

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
                
                // Hide cat from board
                if (cat.MyGameCard != null)
                {
                    var p = cat.MyGameCard.Parent;
                    var c = cat.MyGameCard.Child;
                    cat.MyGameCard.RemoveFromStack();
                    if (p != null && c != null) p.SetChild(c);
                    cat.MyGameCard.gameObject.SetActive(false);
                }
            }

            CurrentBackpack = new Backpack(capacity);

            CurrentMapSeed = seed;
            MapNodes = ExpeditionMapGenerator.GenerateMap(seed, maxLayers: 6, maxNodesPerLayer: 3);
            ActiveNode = null;

            // Initialize risk stats using non-static base appeasement
            ExpeditionRiskSystem.InitializeRunStats(RunState);

            // Freeze main board using WorldSimulationPaused
            WorldManager.WorldSimulationPaused = true;

            // Show Expedition Map UI Overlay
            if (ExpeditionMapUI.Instance != null)
            {
                ExpeditionMapUI.Instance.ShowWindow();
            }
            else
            {
                Debug.LogError("[ExpeditionManager] ExpeditionMapUI.Instance is null!");
            }

            Debug.Log($"[Expedition] Báº¯t Ä‘áº§u viá»…n chinh vá»›i {cats.Count} mÃ¨o. Balo dung tÃ­ch: {capacity}.");
        }

        public void AdvanceNode(ExpeditionNode nextNode)
        {
            if (!IsExpeditionActive) return;

            ActiveNode = nextNode;
            // Xá»­ lÃ½ logic Node
            
            // --- Phase 7: Event Integration (Expedition Ambush) ---
            // Táº¡m thá»i táº¯t á»Ÿ Phase nÃ y theo yÃªu cáº§u Ä‘á»ƒ táº­p trung test Dog Tax
            /*
            if (nextNode.Type == NodeType.Resource && UnityEngine.Random.value < 0.1f) // 10% bá»‹ phá»¥c kÃ­ch
            {
                Debug.Log("Expedition Ambush Event triggered!");
                if (GameScripts.Systems.Threat.ThreatManager.Instance != null && GameScripts.Systems.Threat.ThreatManager.Instance.CatGodWrathTemplate != null)
                {
                    // Giáº£ láº­p phá»¥c kÃ­ch vá»›i data máº«u
                    GameScripts.Systems.Threat.ThreatManager.Instance.CreateThreat(
                        GameScripts.Systems.Threat.ThreatManager.Instance.CatGodWrathTemplate, 
                        GameScripts.Systems.Threat.ThreatSourceType.Expedition, 
                        RunState.CurrentDifficultyLevel, 
                        0 // Phá»¥c kÃ­ch ngay láº­p tá»©c, khÃ´ng cÃ³ warning
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

            // Travel food consumption has been disabled per user request

            // Route Themes initial impacts
            if (node.Theme == RouteTheme.TaDao)
            {
                RunState.AddCorruption(25);
                Debug.Log("[Expedition] TÃ  Äáº¡o Ã¡p lá»±c! TÄƒng +25 Corruption khi bÆ°á»›c vÃ o khu vá»±c tÃ  khÃ­.");
            }
            else if (node.Theme == RouteTheme.ThamLam)
            {
                RunState.AddGreed(10);
                Debug.Log("[Expedition] Tham Lam Ã½ niá»‡m! TÄƒng +10 Greed khi bÆ°á»›c vÃ o khu vá»±c trÃ¹ phÃº.");
            }

            // Hide Map UI while resolving node activities
            if (ExpeditionMapUI.Instance != null)
            {
                ExpeditionMapUI.Instance.HideWindow();
            }

            Debug.Log($"[Expedition] Tiáº¿n vÃ o node {node.Id} ({node.Type}) á»Ÿ Táº§ng {node.Layer}. Lá»™ trÃ¬nh: {node.Theme}. Biome: {node.Biome}.");

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
                // Fallback for Event or Lore node types
                TriggerTextEventNode(node.Type);
            }
        }

        private void TriggerCombat(bool isBoss)
        {
            // Spawn random enemies based on the floor level
            List<Combatable> enemies = new List<Combatable>();
            int enemyCount = UnityEngine.Random.Range(1, 4);
            if (isBoss) enemyCount = 1; // Boss usually stands alone or with 1 guard

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

            // Start turn-based combat overlay (exclude Paralyzed cats)
            List<Combatable> playerCombats = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Cast<Combatable>(System.Linq.Enumerable.Where(ActiveCats, c => c != null && !c.IsParalyzed)));
            TurnBasedCombatManager.Instance.StartCombat(playerCombats, enemies, (result) =>
            {
                // Destroy leftover enemy cards if player won
                foreach (var enemy in enemies)
                {
                    if (enemy != null && enemy.MyGameCard != null)
                    {
                        enemy.MyGameCard.DestroyCard(true, true);
                    }
                }

                if (result == CombatResult.Victory)
                {
                    // Reward loot
                    RollLootForCombat(isBoss);
                    CompleteNodeResolution();
                }
                else if (result == CombatResult.Retreated)
                {
                    // Escape back to the map or retreat out of the expedition
                    Debug.Log("[Expedition] NgÆ°á»i chÆ¡i bá» cuá»™c! Há»§y bá» viá»…n chinh vÃ  quay vá» base.");
                    ReturnToBase(isDefeat: true);
                }
                else
                {
                    // Defeat: lose all loot and return
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
                CurrentBackpack.AddItem(loot);
            }

            // Náº¿u lÃ  Boss tiáº¿n Ä‘á»™, thÆ°á»Ÿng thÃªm Cá»• Váº­t tá»± Ä‘á»™ng hÃ³a ngáº«u nhiÃªn vÃ o balo
            if (isBoss)
            {
                string[] relics = { "item_ancient_relic_auto_farm", "item_ancient_relic_auto_collect", "item_ancient_relic_auto_heal" };
                string chosenRelic = relics[UnityEngine.Random.Range(0, relics.Length)];
                rolled.Add(chosenRelic);
                CurrentBackpack.AddItem(chosenRelic);
                Debug.Log($"[Expedition] Boss chiáº¿n tháº¯ng! Nháº­n thÃªm Cá»• Váº­t chÃ­ tÃ´n: {chosenRelic}");
            }

            string lootMsg = string.Join(", ", rolled.Select(id => id.Replace("resource_", "").Replace("item_", "")));
            Debug.Log($"[Expedition] Thu hoáº¡ch chiáº¿n lá»£i pháº©m: {lootMsg}");
        }

        private void TriggerResourceNode()
        {
            // Drop resources directly into the backpack
            int lootCount = UnityEngine.Random.Range(2, 5);
            string[] resources = { "resource_food", "item_wood", "item_stone", "resource_gold", "item_iron_ore" };

            List<string> added = new List<string>();
            for (int i = 0; i < lootCount; i++)
            {
                string res = resources[UnityEngine.Random.Range(0, resources.Length)];
                if (CurrentBackpack.AddItem(res))
                {
                    added.Add(res);
                }
            }

            string resMsg = added.Count > 0 
                ? string.Join(", ", added.Select(id => id.Replace("resource_", "").Replace("item_", "")))
                : "KhÃ´ng cÃ³ chá»— chá»©a trong Balo!";

            // Trigger dialogue overlay to display the resource gathering result
            string title = "Thu tháº­p TÃ i NguyÃªn";
            string text = $"Äá»™i cá»§a báº¡n Ä‘Ã£ tÃ¬m tháº¥y má»™t bÃ£i tÃ i nguyÃªn trÃ¹ phÃº!\n\nNháº­n Ä‘Æ°á»£c: {resMsg}";
            
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { "Äá»“ng Ã½" }, (choiceIdx) =>
            {
                CompleteNodeResolution();
            });
        }

        private void TriggerTextEventNode(NodeType type)
        {
            string title = "";
            string text = "";
            List<string> choices = new List<string>();
            Action<int> onChoice = null;

            if (type == NodeType.Event)
            {
                int eventRoll = UnityEngine.Random.Range(0, 7);
                if (eventRoll == 0)
                {
                    // LÃ´i kiáº¿p thá»­ thÃ¡ch
                    title = "âš¡ KIáº¾P LÃ”I THá»¬ THÃCH";
                    text = "Äá»™i ngÅ© mÃ¨o Ä‘i tá»›i má»™t Ä‘á»‰nh nÃºi hoang váº¯ng, mÃ¢y Ä‘en cuá»™n trÃ o ngháº¹t thá»Ÿ. Tá»«ng tia lÃ´i Ä‘iá»‡n khá»•ng lá»“ giÃ¡ng xuá»‘ng nhÆ° LÃ´i Kiáº¿p Ä‘á»™ kiáº¿p!\n\nLÃ´i linh lá»±c cuá»“ng báº¡o nÃ y áº©n chá»©a cÆ¡ duyÃªn lá»›n nhÆ°ng cá»±c ká»³ nguy hiá»ƒm. Báº¡n muá»‘n lÃ m gÃ¬?";
                    choices = new List<string> {
                        "Háº¥p thá»¥ LÃ´i Kiáº¿p (YÃªu cáº§u mÃ¨o há»‡ SÃ©t hoáº·c Tá»‘c Ä‘á»™ cao)",
                        "Äá»¡ Ä‘Ã²n há»™ Ä‘á»“ng Ä‘á»™i (YÃªu cáº§u Tank báº£o vá»‡)",
                        "Tráº­n phÃ¡p phÃ²ng thá»§ (LÃ¡ch qua an toÃ n)"
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
                                luckyCat.AddMemoir(MemoirType.Breakthrough, "LÃ´i Kiáº¿p Táº©y Tá»§y", "Háº¥p thá»¥ lÃ´i Ä‘iá»‡n Ä‘á»™t phÃ¡ vÃµ Ä‘áº¡o (+25 Speed)");
                                DialogueResult("LÃ´i quang rá»±c rá»¡!", $"Tuyá»‡t Ä‘á»‰nh! Nhá» sá»± nháº¡y bÃ©n cá»±c Ä‘á»™ (hoáº·c linh cÄƒn há»‡ SÃ©t), chÃº mÃ¨o <b>{luckyCat.Name}</b> Ä‘Ã£ háº¥p thá»¥ trá»n váº¹n LÃ´i Äiá»‡n Pháº¡t, vÄ©nh viá»…n gia tÄƒng <b>+25 Tháº§n Tá»‘c</b>!");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsUltimateLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, "Táº©u Há»a Nháº­p Ma", "TrÃºng lÃ´i Ä‘iá»‡n báº¡o phÃ¡t báº¿ táº¯c linh máº¡ch, khÃ³a ká»¹ nÄƒng Ná»™");
                                foreach (var cat in ActiveCats)
                                {
                                    cat.HealthPoints = Mathf.Max(1, cat.HealthPoints - 10);
                                }
                                DialogueResult("LÃ´i Pháº¡t Oanh Táº¡c!", $"Tá»‘c Ä‘á»™ quÃ¡ cháº­m! LÃ´i Ä‘iá»‡n cuá»“ng báº¡o thÃ¢m nháº­p tÃ n phÃ¡ kinh máº¡ch. ChÃº mÃ¨o <b>{victim.Name}</b> bá»‹ <b><color=red>Táº¨U Há»ŽA NHáº¬P MA (KHÃ“A Ká»¸ NÄ‚NG Ná»˜)</color></b> vÄ©nh viá»…n, toÃ n Ä‘á»™i thÆ°Æ¡ng náº·ng (-10 HP)!");
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
                                tank.AddMemoir(MemoirType.Breakthrough, "Há»™ Thá»ƒ LÃ´i Kiáº¿p", "Äá»¡ lÃ´i kiáº¿p cho Ä‘á»“ng Ä‘á»™i, khÃ³a Ã´ ThiÃªn PhÃº");
                                DialogueResult("Há»™ Thá»ƒ Tuyá»‡t Vá»i!", $"Anh hÃ¹ng! ChÃº Tank <b>{tank.Name}</b> Ä‘á»©ng ra Ä‘á»¡ lÃ´i pháº¡t cho toÃ n Ä‘á»™i. Tháº§n thá»ƒ Ä‘Æ°á»£c cÆ°á»ng hÃ³a (+10 Max HP) nhÆ°ng bÃ¹a chÃº bá»‹ phÃ¡ há»§y hoÃ n toÃ n, <b><color=red>Ã´ ThiÃªn PhÃº (Passive Slots) vÄ©nh viá»…n bá»‹ KHÃ“A</color></b>!");
                            }
                            else
                            {
                                foreach (var cat in ActiveCats)
                                {
                                    cat.HealthPoints = Mathf.Max(1, cat.HealthPoints - 15);
                                }
                                DialogueResult("Há»™ Vá»‡ Tháº¥t Báº¡i!", "Äá»™i hÃ¬nh khÃ´ng cÃ³ há»™ vá»‡ Tank chuyÃªn nghiá»‡p! Buá»™c pháº£i dÃ¹ng thÃ¢n xÃ¡c tráº§n tá»¥c chá»‘ng Ä‘á»¡, toÃ n Ä‘á»™i bá»‹ thÆ°Æ¡ng tá»•n cá»±c náº·ng (-15 HP)!");
                            }
                        }
                        else
                        {
                            DialogueResult("LÃ¡ch Qua An ToÃ n", "ToÃ n Ä‘á»™i thiáº¿t láº­p káº¿t giá»›i phÃ²ng thá»§ thÃ´ sÆ¡, cáº©n tháº­n Ä‘i vÃ²ng qua ngá»n nÃºi lÃ´i kiáº¿p an toÃ n.");
                        }
                    };
                }
                else if (eventRoll == 1)
                {
                    // Tráº¡m tuáº§n tra
                    title = "ðŸ• TRáº M TUáº¦N TRA Cá»¦A CHÃšNG CHÃ“";
                    text = "PhÃ­a trÆ°á»›c xuáº¥t hiá»‡n chá»‘t gÃ¡c kiÃªn cá»‘ cá»§a loÃ i ChÃ³ kiá»ƒm soÃ¡t tráº­t tá»± xÃ£ há»™i. LÃ­nh tuáº§n tra chÃ³ bá»c giÃ¡p sáº¯t Ä‘ang canh phÃ²ng nghiÃªm ngáº·t.\n\nÄá»™i mÃ¨o cá»§a báº¡n mang theo balo Ä‘áº§y áº¯p tÃ i nguyÃªn kháº£ nghi. Báº¡n muá»‘n á»©ng phÃ³ tháº¿ nÃ o?";
                    choices = new List<string> {
                        "ÄÃºt lÃ³t há»‘i lá»™ (TiÃªu hao 1 VÃ ng trong Balo - Giáº£m 20 Greed)",
                        "Quyáº¿t chiáº¿n Ä‘á»™t phÃ¡ (Tháº¯ng lá»£i Ä‘áº«m mÃ¡u - TÄƒng 25 Corruption)",
                        "LÃ©n lÃºt láº»n qua (YÃªu cáº§u Tá»‘c Ä‘á»™ trung bÃ¬nh > 115)",
                        "Thuyáº¿t giáº£ng tÃ¢m lÃ½ (Cáº§n Tháº§n MiÃªu Thiá»n Äáº¡o giáº£i thoÃ¡t Ä‘áº¡o tÃ¢m lÃ­nh gÃ¡c)"
                    };
                    onChoice = (idx) =>
                    {
                        if (idx == 0)
                        {
                            int goldIdx = CurrentBackpack.FindItemIndex("resource_gold");
                            if (goldIdx >= 0)
                            {
                                CurrentBackpack.RemoveItemAt(goldIdx);
                                RunState.GreedLevel = Mathf.Max(0, RunState.GreedLevel - 20);
                                DialogueResult("Há»‘i Lá»™ ThÃ nh CÃ´ng!", "LÃ­nh tuáº§n tra ChÃ³ nháº­n láº¥y VÃ ng, cÆ°á»i nham nhá»Ÿ má»Ÿ cá»•ng cho Ä‘i qua. Sá»©c Ã©p luáº­t phÃ¡p xoa dá»‹u (-20 Greed)!");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsEquipmentSlotsLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, "Tá»‹ch Thu Trang Bá»‹", "KhÃ´ng cÃ³ tiá»n Ä‘Ãºt lÃ³t, bá»‹ lÃ­nh tuáº§n tra khÃ³a Ã´ trang bá»‹");
                                DialogueResult("KhÃ´ng CÃ³ Tiá»n ÄÃºt LÃ³t!", $"Balo khÃ´ng cÃ³ VÃ ng Ä‘á»ƒ há»‘i lá»™! LÃ­nh tuáº§n tra ná»•i giáº­n khÃ¡m xÃ©t toÃ n Ä‘á»™i. ChÃº mÃ¨o <b>{victim.Name}</b> bá»‹ tá»‹ch thu sáº¡ch vÅ© khÃ­ bÃ¹a chÃº vÃ  vÄ©nh viá»…n <b><color=red>KHÃ“A Ã´ Trang Bá»‹ (Equipment Slots)</color></b>!");
                            }
                        }
                        else if (idx == 1)
                        {
                            foreach (var cat in ActiveCats)
                            {
                                cat.HealthPoints = Mathf.Max(1, cat.HealthPoints - 8);
                            }
                            CurrentBackpack.AddItem("resource_gold");
                            CurrentBackpack.AddItem("item_iron_ore");
                            RunState.AddCorruption(25);
                            DialogueResult("Huyáº¿t Chiáº¿n Äá»™t PhÃ¡!", "ToÃ n Ä‘á»™i tuá»‘t kiáº¿m liá»u cháº¿t xÃ´ng vÃ o! TiÃªu diá»‡t toÃ n bá»™ lÃ­nh canh, cÆ°á»›p láº¥y VÃ ng vÃ  Quáº·ng sáº¯t trong rÆ°Æ¡ng chá»‘t tuáº§n tra, toÃ n Ä‘á»™i bá»‹ thÆ°Æ¡ng nháº¹ (-8 HP) vÃ  tÄƒng máº¡nh sÃ¡t nghiá»‡p (+25 Corruption)!");
                        }
                        else if (idx == 2)
                        {
                            int avgSpeed = (int)ActiveCats.Average(c => c.Speed);
                            if (avgSpeed > 115)
                            {
                                DialogueResult("Láº»n Qua ThÃ nh CÃ´ng!", "BÃ³ng ma bÃ³ng tá»‘i! Báº±ng bÆ°á»›c di chuyá»ƒn tháº§n tá»‘c, khÃ´ng tiáº¿ng Ä‘á»™ng, toÃ n Ä‘á»™i mÃ¨o Ä‘Ã£ lÆ°á»›t qua tráº¡m canh gÃ¡c trÃ³t lá»t mÃ  lÃ­nh chÃ³ khÃ´ng há» hay biáº¿t!");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsEquipmentSlotsLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, "Báº¯t Giá»¯ Phong áº¤n", "LÃ©n láº»n tháº¥t báº¡i, bá»‹ khÃ³a Ã´ trang bá»‹ hÃ¬nh pháº¡t");
                                DialogueResult("Bá»‹ Báº¯t Quáº£ Tang!", $"Tá»‘c Ä‘á»™ trung bÃ¬nh ({avgSpeed} Speed) quÃ¡ cháº­m! LÃ­nh chÃ³ phÃ¡t hiá»‡n báº¯t giá»¯ toÃ n Ä‘á»™i tra kháº£o. ChÃº mÃ¨o <b>{victim.Name}</b> bá»‹ tá»‹ch thu khÃ­ giá»›i, <b><color=red>Ã´ Trang bá»‹ vÄ©nh viá»…n bá»‹ KHÃ“A</color></b> lÃ m hÃ¬nh pháº¡t!");
                            }
                        }
                        else if (idx == 3)
                        {
                            var zenCat = ActiveCats.Find(c => c.Specialization == Mewtations.Cards.Cats.DaoSpecialization.ZenDao);
                            if (zenCat != null)
                            {
                                CurrentBackpack.AddItem("item_heavenly_relic");
                                RunState.CorruptionLevel = Mathf.Max(0, RunState.CorruptionLevel - 20);
                                DialogueResult("GiÃ¡c Ngá»™ Äáº¡o TÃ¢m!", $"GiÃ¡c ngá»™ thÃ nh cÃ´ng! ChÃº mÃ¨o Thiá»n Äáº¡o <b>{zenCat.Name}</b> Ä‘Ã£ thuyáº¿t giáº£ng Äáº¡o lÃ½ NhÃ¢n sinh cá»±c ká»³ thÃ¢m sÃ¢u, khai má»Ÿ Ä‘áº¡o tÃ¢m cho lÃ­nh tuáº§n tra ChÃ³ thoÃ¡t khá»i sá»± kiá»ƒm soÃ¡t gÃ² bÃ³ cá»§a há»‡ thá»‘ng.\n\nChÃº ChÃ³ cáº£m kÃ­ch rÆ¡i lá»‡, má»Ÿ cá»•ng táº·ng toÃ n Ä‘á»™i viÃªn <b>ChÃ­ TÃ´n Cá»• KhÃ­ (Heavenly Relic)</b> cá»±c hiáº¿m vÃ  xoa dá»‹u tÃ  khÃ­ (-20 Corruption)!");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.HealthPoints = Mathf.Max(1, victim.HealthPoints - 10);
                                DialogueResult("GiÃ¡o HÃ³a Tháº¥t Báº¡i!", $"Äá»™i hÃ¬nh khÃ´ng cÃ³ Tháº§n MiÃªu Thiá»n Äáº¡o Ä‘á»ƒ giáº£ng giáº£i Äáº¡o phÃ¡p thuyáº¿t phá»¥c! LÃ­nh tuáº§n tra ChÃ³ cho ráº±ng báº¡n Ä‘ang sá»‰ nhá»¥c trÃ­ tuá»‡ cá»§a há», ná»•i giáº­n dÃ¹ng roi Ä‘iá»‡n Ä‘Ã¡nh thÆ°Æ¡ng <b>{victim.Name}</b> (-10 HP)!");
                            }
                        }
                    };
                }
                else if (eventRoll == 2)
                {
                    // LÃ² Ä‘an cá»•
                    title = "âš—ï¸ LÃ’ LUYá»†N ÄAN Cá»” KÃNH";
                    text = "Äan Ä‘iá»‡n pháº¿ tÃ­ch u Ã¡m hiá»‡n ra trÆ°á»›c máº¯t. á»ž trung tÃ¢m sáº£nh lá»›n lÃ  má»™t lÃ² luyá»‡n cá»• váº«n chÃ¡y Ã¢m á»‰ lá»­a tÃ­m nháº¡t rÃ² rá»‰ khÃ­ Ä‘á»™c. BÃªn trong cÃ³ thá»ƒ áº©n chá»©a nghá»‹ch thiÃªn linh Ä‘an hoáº·c ká»‹ch Ä‘á»™c pháº¿ linh máº¡ch.\n\nAi sáº½ Ä‘á»©ng ra xá»­ lÃ½ chiáº¿c lÃ² Ä‘an nÃ y?";
                    choices = new List<string> {
                        "DÃ¹ng linh Ä‘á»™c hÃ³a giáº£i (Cáº§n mÃ¨o há»‡ Äá»™c)",
                        "Lá»±c lÆ°á»£ng cÆ°á»¡ng cháº¿ má»Ÿ lÃ² (Rá»§i ro 50/50)",
                        "Äáº­p phÃ¡ lÃ² thu pháº¿ liá»‡u (An toÃ n)"
                    };
                    onChoice = (idx) =>
                    {
                        if (idx == 0)
                        {
                            var poisonCat = ActiveCats.Find(c => c.Element == CatElement.Poison);
                            if (poisonCat != null)
                            {
                                CurrentBackpack.AddItem("item_breakthrough_pill");
                                DialogueResult("Khá»‘ng Cháº¿ Ká»‹ch Äá»™c!", $"Tuyá»‡t Ä‘á»‰nh! Nhá» linh cÄƒn ká»‹ch Ä‘á»™c báº©m sinh cá»§a <b>{poisonCat.Name}</b>, chÃº Ä‘Ã£ trung hÃ²a Ä‘an khÃ­ tÃ­m, má»Ÿ lÃ² láº¥y Ä‘Æ°á»£c viÃªn <b>Äá»˜T PHÃ LINH ÄAN</b> cá»±c ká»³ quÃ½ giÃ¡!");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsPillSlotLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, "Linh Máº¡ch Äá»™c á»¨", "KhÃ­ Ä‘á»™c tÃ n phÃ¡ kinh máº¡ch Ä‘an dÆ°á»£c, khÃ³a Ã´ Linh Äan");
                                DialogueResult("KhÃ´ng CÃ³ MÃ¨o Há»‡ Äá»™c!", $"Äá»™c khÃ­ tÃ­m bÃ¹ng phÃ¡t cuá»“n cuá»™n do khÃ´ng cÃ³ mÃ¨o há»‡ Äá»™c khá»‘ng cháº¿! ChÃº mÃ¨o <b>{victim.Name}</b> hÃ­t pháº£i Ä‘á»™c sÆ°Æ¡ng tÃ n phÃ¡ pháº¿ linh máº¡ch, <b><color=red>Ã´ Linh Äan (Pill Slot) vÄ©nh viá»…n bá»‹ KHÃ“A</color></b>!");
                            }
                        }
                        else if (idx == 1)
                        {
                            if (UnityEngine.Random.value < 0.5f)
                            {
                                CurrentBackpack.AddItem("item_breakthrough_pill");
                                DialogueResult("Váº­n May Nghá»‹ch ThiÃªn!", "Váº­n may má»‰m cÆ°á»i! DÃ¹ khÃ­ Ä‘á»™c bá»‘c lÃªn ngÃ¹n ngá»¥t nhÆ°ng toÃ n Ä‘á»™i Ä‘Ã£ nhanh tay cÆ°á»›p láº¥y viÃªn <b>Äá»˜T PHÃ LINH ÄAN</b> thÃ nh cÃ´ng trÆ°á»›c khi Ä‘á»™c cháº¥n ná»• ra!");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsPillSlotLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, "LÃ² Äan Ná»• Tung", "TrÃºng khÃ­ Ä‘á»™c lÃ² Ä‘an ná»•, khÃ³a Ã´ Linh Äan");
                                DialogueResult("LÃ² Äan Ná»• Tung!", $"BÃ¹m! LÃ² luyá»‡n Ä‘an phÃ¡t ná»• lá»›n báº¯n ra tÃ n dÆ° Ä‘an dÆ°á»£c ká»‹ch Ä‘á»™c. ChÃº mÃ¨o <b>{victim.Name}</b> trÃºng Ä‘á»™c ngÆ°ng káº¿t kinh máº¡ch Ä‘an Ä‘iá»n, <b><color=red>Ã´ Linh Äan vÄ©nh viá»…n bá»‹ KHÃ“A</color></b>!");
                            }
                        }
                        else
                        {
                            CurrentBackpack.AddItem("item_stone");
                            CurrentBackpack.AddItem("item_iron_ore");
                            DialogueResult("Thu Hoáº¡ch Pháº¿ Liá»‡u", "Quyáº¿t Ä‘á»‹nh sÃ¡ng suá»‘t! ToÃ n Ä‘á»™i Ä‘áº­p vá»¡ lÃ² Ä‘an an toÃ n, thu vá» ÄÃ¡ vá»¥n vÃ  Sáº¯t pháº¿ liá»‡u bá» vÃ o Balo viá»…n chinh.");
                        }
                    };
                }
                else if (eventRoll == 3)
                {
                    // Ma huyá»‡t hiáº¿n táº¿
                    title = "ðŸ”´ MA HUYá»†T KHáº¤N NGUYá»†N";
                    text = "Má»™t ma huyá»‡t phÃ¡t ra há»“ng quang rá»±c mÃ¡u ngÄƒn giá»¯a Ä‘Æ°á»ng Ä‘i. Linh khÃ­ bÃªn trong cuá»™n trÃ o quyáº¿n rÅ©, nhÆ° khÆ¡i dáº­y Ã½ niá»‡m Tham Lam tá»™t cÃ¹ng cá»§a loÃ i mÃ¨o.\n\nTháº§n linh Ä‘Ã²i há»i cÃºng náº¡p linh thá»±c Äƒn uá»‘ng hoáº·c cá»‘t tá»§y kinh máº¡ch Ä‘á»ƒ ban phÃ¡t thiÃªn phÃº Ä‘á»™t phÃ¡ vÄ©nh viá»…n.";
                    choices = new List<string> {
                        "DÃ¢ng hiáº¿n Linh Thá»±c (TiÃªu hao 1 Thá»©c Äƒn trong Balo - Nháº­n ThiÃªn PhÃº vÄ©nh viá»…n)",
                        "Huyáº¿t Thá»‡ Cá»‘t Tá»§y (Äá»™t phÃ¡ Cáº£nh giá»›i - Cháº¥p nháº­n khÃ³a Ã´ Thá»©c Äƒn)",
                        "Tá»« bá» tham niá»‡m (Thanh táº©y linh há»“n)"
                    };
                    onChoice = (idx) =>
                    {
                        if (idx == 0)
                        {
                            int foodIdx = CurrentBackpack.FindItemIndex("food");
                            if (foodIdx >= 0)
                            {
                                CurrentBackpack.RemoveItemAt(foodIdx);
                                var lucky = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                string talent = UnityEngine.Random.value < 0.5f ? HeavenlyTalent.RageOvercharger : HeavenlyTalent.DivineShieldProtection;
                                lucky.AddTrait(talent);
                                lucky.CustomName = $"{HeavenlyTalent.GetDisplayName(talent)} {lucky.Name}";
                                lucky.AddMemoir(MemoirType.Breakthrough, HeavenlyTalent.GetDisplayName(talent), "DÃ¢ng hiáº¿n thá»©c Äƒn ma huyá»‡t nháº­n thiÃªn phÃº");
                                DialogueResult("Táº¿ Pháº©m Cháº¥p Thuáº­n!", $"Tháº§n linh hoan há»·! Nháº­n láº¥y Linh thá»±c hiáº¿n táº¿, ma lá»±c bÃ¹ng phÃ¡t táº©y tá»§y vÄ©nh viá»…n cho <b>{lucky.Name}</b>, thá»©c tá»‰nh thiÃªn phÃº vÄ©nh cá»­u: <b><color=#00ffcc>{HeavenlyTalent.GetDisplayName(talent)}</color></b>!");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsFoodSlotLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, "Nguyá»n Rá»§a ÄÃ³i KhÃ¡t", "Lá»«a dá»‘i ma huyá»‡t bá»‹ pháº¡t Ä‘Ã³i, khÃ³a Ã´ Thá»©c Äƒn");
                                DialogueResult("Tháº§n Linh Pháº«n Ná»™!", $"Balo khÃ´ng cÃ³ Thá»©c Äƒn hiáº¿n táº¿! Tháº§n linh ná»•i giáº­n giÃ¡ng nguyá»n rá»§a ÄÃ³i KhÃ¡t Ä‘Ã³i nghÃ¨o lÃªn toÃ n Ä‘á»™i. ChÃº mÃ¨o <b>{victim.Name}</b> bá»‹ <b><color=red>KHÃ“A Ã´ Thá»©c Äƒn (Food/Ultimate Slot)</color></b> vÄ©nh viá»…n!");
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
                            victim.AddMemoir(MemoirType.Breakthrough, "Huyáº¿t Thá»‡ Nghá»‹ch ThiÃªn", "Äá»™t phÃ¡ cÆ°á»¡ng cháº¿, vÄ©nh viá»…n khÃ³a Ã´ Thá»©c Äƒn");
                            DialogueResult("Huyáº¿t Thá»‡ ThÃ nh CÃ´ng!", $"Táº¿ lá»… Ä‘áº«m mÃ¡u nghá»‹ch thiÃªn! ChÃº mÃ¨o <b>{victim.Name}</b> hiáº¿n táº¿ kinh máº¡ch tiÃªu hÃ³a cá»§a báº£n thÃ¢n. Äá»™t phÃ¡ cáº£nh giá»›i vÆ°á»£t báº­c vÄ©nh viá»…n (+10 Max HP, +15 Speed) nhÆ°ng <b><color=red>Ã´ Thá»©c Äƒn (Food/Ultimate Slot) vÄ©nh viá»…n bá»‹ KHÃ“A</color></b>!");
                        }
                        else
                        {
                            RunState.CorruptionLevel = Mathf.Max(0, RunState.CorruptionLevel - 25);
                            DialogueResult("TÃ¢m Há»“n Thanh Tá»‹nh", "ToÃ n Ä‘á»™i tá»« bá» Ã½ chÃ­ tham lam, ma chÆ°á»›ng linh máº¡ch Ä‘Æ°á»£c táº©y rá»­a gá»™t sáº¡ch (-25 Corruption)!");
                        }
                    };
                }
                else if (eventRoll == 4)
                {
                    // Kiá»ƒm tra giáº¥y phÃ©p thÃ´ng hÃ nh láº­u
                    title = MewtationsLoc.Translate("exp_license_check_title", "âš ï¸ KIá»‚M TRA GIáº¤Y PHÃ‰P Äá»˜T XUáº¤T");
                    text = MewtationsLoc.Translate("exp_license_check_desc", "Má»™t toÃ¡n Lá»±c LÆ°á»£ng HÃ nh PhÃ¡p bá»c giÃ¡p sáº¯t báº¥t ngá» cháº·n Ä‘á»™i mÃ¨o cá»§a báº¡n láº¡i táº¡i chá»‘t ráº½. ÄÃ¨n linh Ã¡p quÃ©t tháº³ng qua chiáº¿c balo kháº£ nghi cá»§a báº¡n.\n\n\"Dá»«ng láº¡i! Kiá»ƒm tra giáº¥y phÃ©p thÃ´ng hÃ nh vÃ  quota khai thÃ¡c linh tháº¡ch. TrÃ¬nh diá»‡n ngay láº­p tá»©c!\"");
                    choices = new List<string> {
                        "TrÃ¬nh tháº» phÃ©p láº­u (ÄÃºt lÃ³t 1 VÃ ng) / Show Forged Permit (1 Gold)",
                        "Cháº¥p nháº­n tá»‹ch thu hÃ ng láº­u / Accept Confiscation",
                        "Cháº¡y trá»‘n láº­p tá»©c (YÃªu cáº§u Tá»‘c Ä‘á»™ > 120) / Flee"
                    };
                    onChoice = (idx) =>
                    {
                        if (idx == 0)
                        {
                            int goldIdx = CurrentBackpack.FindItemIndex("resource_gold");
                            if (goldIdx >= 0)
                            {
                                CurrentBackpack.RemoveItemAt(goldIdx);
                                DialogueResult("Há»‘i Lá»™ ThÃ nh CÃ´ng", "Lá»±c LÆ°á»£ng HÃ nh PhÃ¡p liáº¿c nhÃ¬n Ä‘á»“ng VÃ ng, lá» Ä‘i Ä‘á»‘ng quáº·ng báº¥t há»£p phÃ¡p trong balo: \"Giáº¥y phÃ©p há»£p lá»‡. Äi mau!\"");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.HealthPoints = Mathf.Max(1, victim.HealthPoints - 10);
                                RunState.AddCorruption(20);
                                DialogueResult("KhÃ´ng CÃ³ Tiá»n ÄÃºt LÃ³t", $"Bá»‹ phÃ¡t hiá»‡n dÃ¹ng giáº¥y thÃ´ng hÃ nh giáº£! ChÃºng láº­p tá»©c dÃ¹ng roi Ä‘iá»‡n Ä‘Ã¡nh thÆ°Æ¡ng náº·ng <b>{victim.Name}</b> (-10 HP) vÃ  nÃ¢ng má»©c tÃ  lá»±c ma Ä‘áº¡o (+20 Corruption)!");
                            }
                        }
                        else if (idx == 1)
                        {
                            if (CurrentBackpack.ContainedCardIds.Count > 0)
                            {
                                int randIdx = UnityEngine.Random.Range(0, CurrentBackpack.ContainedCardIds.Count);
                                string removed = CurrentBackpack.GetItemIdAt(randIdx);
                                CurrentBackpack.RemoveItemAt(randIdx);
                                DialogueResult("HÃ ng Láº­u Bá»‹ Tá»‹ch Thu", $"Äá»ƒ giá»¯ tÃ­nh máº¡ng, toÃ n Ä‘á»™i giao ná»™p <b>{removed.Replace("item_", "").Replace("resource_", "")}</b>. ChÃºng há»« láº¡nh thu giá»¯ rá»“i tháº£ Ä‘i.");
                            }
                            else
                            {
                                DialogueResult("Balo Trá»‘ng Rá»—ng", "ChÃºng khÃ¡m xÃ©t balo nhÆ°ng khÃ´ng tháº¥y gÃ¬ kháº£ nghi. KhÃ´ng cÃ³ gÃ¬ Ä‘á»ƒ tá»‹ch thu, chÃºng Ä‘Ã nh Ä‘Ã¡ Ä‘Ã­t xua Ä‘uá»•i báº¡n Ä‘i.");
                            }
                        }
                        else
                        {
                            int avgSpeed = (int)ActiveCats.Average(c => c.Speed);
                            if (avgSpeed > 120)
                            {
                                DialogueResult("Cháº¡y ThoÃ¡t ThÃ nh CÃ´ng", "Tháº§n tá»‘c! ToÃ n Ä‘á»™i mÃ¨o phÃ³ng Ä‘i trong chá»›p máº¯t, cáº¯t Ä‘uÃ´i toÃ¡n tuáº§n tra Dogma má»™t cÃ¡ch hoÃ n háº£o!");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.HealthPoints = Mathf.Max(1, victim.HealthPoints - 12);
                                RunState.AddCorruption(20);
                                DialogueResult("Cháº¡y Trá»‘n Tháº¥t Báº¡i", $"Tá»‘c Ä‘á»™ quÃ¡ cháº­m! ToÃ¡n tuáº§n tra vÃ¢y báº¯t vÃ  Ä‘Ã¡nh trá»ng thÆ°Æ¡ng <b>{victim.Name}</b> (-12 HP), tÃ  phÃ¡p giam giá»¯ gia tÄƒng (+20 Corruption)!");
                            }
                        }
                    };
                }
                else if (eventRoll == 5)
                {
                    // DÃ¢n nghÃ¨o cáº§u xin
                    title = MewtationsLoc.Translate("exp_beggar_title", "ðŸ± DÃ‚N NGHÃˆO Cáº¦U XIN LINH KHÃ");
                    text = MewtationsLoc.Translate("exp_beggar_desc", "Má»™t chÃº mÃ¨o tiá»u tá»¥y gáº§y trÆ¡ xÆ°Æ¡ng, cÆ¡ thá»ƒ dá»‹ biáº¿n náº·ng ná» Ä‘ang quá»³ bÃªn Ä‘á»‘ng pháº¿ tháº£i cÃ´ng nghiá»‡p, run ráº©y van xin:\n\n\"LÃ m Æ¡n... tÃ´i chá»‰ xin má»™t máº©u Linh Tháº¡ch vá»¥n Ä‘á»ƒ duy trÃ¬ linh cÄƒn Ä‘ang hÃ©o Ãºa cá»§a con tÃ´i... Bá»n Dogma Ä‘Ã£ siáº¿t háº¿t quota cá»§a khu nÃ y rá»“i...\"");
                    choices = new List<string> {
                        "Bá»‘ thÃ­ 1 Quáº·ng Linh Tháº¡ch thÃ´ / Give 1 Spirit Ore",
                        "Tá»« chá»‘i Ä‘i tháº³ng / Refuse"
                    };
                    onChoice = (idx) =>
                    {
                        if (idx == 0)
                        {
                            int oreIdx = CurrentBackpack.FindItemIndex("ore");
                            if (oreIdx >= 0)
                            {
                                CurrentBackpack.RemoveItemAt(oreIdx);
                                RunState.CorruptionLevel = Mathf.Max(0, RunState.CorruptionLevel - 30);
                                DialogueResult("TÃ­ch Äá»©c Giáº£i Nghiá»‡p", "ChÃº mÃ¨o má»«ng rá»¡ Ã´m láº¥y máº£nh quáº·ng khÃ³c náº¥c lÃªn. Linh há»“n toÃ n Ä‘á»™i Ä‘Æ°á»£c thanh tháº£n, gá»™t rá»­a bá»›t tÃ  khÃ­ ma kiáº¿p (-30 Corruption)!");
                            }
                            else
                            {
                                DialogueResult("KhÃ´ng CÃ³ Linh Tháº¡ch", "Báº¡n ráº¥t muá»‘n giÃºp nhÆ°ng balo viá»…n chinh khÃ´ng cÃ³ báº¥t ká»³ máº£nh Quáº·ng Linh Tháº¡ch nÃ o. ChÃº mÃ¨o nghÃ¨o tháº¥t vá»ng quay Ä‘i.");
                            }
                        }
                        else
                        {
                            RunState.GreedLevel = Mathf.Min(100, RunState.GreedLevel + 15);
                            DialogueResult("Quay LÆ°ng Bá» Äi", "Báº¡n láº¡nh lÃ¹ng bÆ°á»›c tiáº¿p. Tiáº¿ng khÃ³c than uáº¥t ngháº¹n cá»§a dÃ¢n nghÃ¨o bÃ¡m riáº¿t Ä‘áº¡o tÃ¢m cá»§a báº¡n (+15 Greed)!");
                        }
                    };
                }
                else
                {
                    // Gáº·p ThÆ°Æ¡ng nhÃ¢n láº­u (Black Market Merchant)
                    int maxBreakthrough = ActiveCats.Count > 0 ? ActiveCats.Max(c => c.BreakthroughLevel) : 0;
                    title = MewtationsLoc.Translate("exp_merchant_encounter_title", "âš–ï¸ THÆ¯Æ NG NHÃ‚N CHá»¢ ÄEN");
                    if (maxBreakthrough >= 2)
                    {
                        text = MewtationsLoc.Translate("exp_merchant_high_rank_desc", "Má»™t gÃ£ mÃ¨o trÃ¹m mÅ© kÃ­n mÃ­t hÃ© má»Ÿ chiáº¿c hÃ²m linh báº£o giáº¥u kÃ­n. Háº¯n thÃ¬ tháº§m Ä‘áº§y tÃ´n kÃ­nh:\n\n\"NhÃ¬n ngÃ i cÃ³ váº» lÃ  má»™t Há»™ PhÃ¡p cao cáº¥p... Tiá»ƒu nhÃ¢n cÃ³ vÃ i mÃ³n báº£o váº­t giáº¥u riÃªng, hoÃ n toÃ n khÃ´ng ghi trong sá»• sÃ¡ch kiá»ƒm kÃª cá»§a GiÃ¡o Äiá»u... NgÃ i cÃ³ muá»‘n xem qua?\"");
                        choices = new List<string> {
                            "Mua HÃ³a Tháº§n Tháº¡ch / Revive Pill (TiÃªu hao 15 VÃ ng) / 15 Gold",
                            "Mua Linh DÆ°á»£c Äá»™t PhÃ¡ / Breakthrough Pill (TiÃªu hao 15 VÃ ng) / 15 Gold",
                            "RÃºt lui / Leave"
                        };
                        onChoice = (idx) =>
                        {
                            if (idx == 0 || idx == 1)
                            {
                                int goldIdx = CurrentBackpack.FindItemIndex("resource_gold");
                                if (goldIdx >= 0)
                                {
                                    CurrentBackpack.RemoveItemAt(goldIdx);
                                    string itemSpawn = idx == 0 ? "item_revive_pill" : "item_breakthrough_pill";
                                    CurrentBackpack.AddItem(itemSpawn);
                                    DialogueResult("Giao Dá»‹ch ThÃ nh CÃ´ng", $"Báº£o váº­t báº¥t há»£p phÃ¡p <b>{itemSpawn.Replace("item_", "")}</b> Ä‘Ã£ Ä‘Æ°á»£c giao tay bÃ­ máº­t. ThÆ°Æ¡ng nhÃ¢n Ä‘Ã³ng rÆ°Æ¡ng vÃ  lá»§i máº¥t.");
                                }
                                else
                                {
                                    DialogueResult("KhÃ´ng Äá»§ VÃ ng", "KhÃ´ng Ä‘á»§ vÃ ng thanh toÃ¡n! Háº¯n láº§u báº§u Ä‘Ã³ng rÆ°Æ¡ng láº¡i: \"Quay láº¡i khi ngÃ i mang Ä‘á»§ vÃ ng!\"");
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
                        text = MewtationsLoc.Translate("exp_merchant_low_rank_desc", "Má»™t gÃ£ mÃ¨o trÃ¹m mÅ© kÃ­n mÃ­t liáº¿c nhÃ¬n Ä‘á»™i mÃ¨o sÆ¡ cáº¥p cá»§a báº¡n Ä‘áº§y khinh khá»‰nh, Ä‘Ã³ng sáº­p hÃ²m báº£o váº­t láº¡i:\n\n\"Biáº¿n Ä‘i! Loáº¡i táº¡p mÃ¨o tháº¥p kÃ©m nhÆ° cÃ¡c ngÆ°Æ¡i khÃ´ng Ä‘á»§ cáº¥p Ä‘á»ƒ xem hÃ ng nÃ y cá»§a ta. Äá»«ng lÃ m máº¥t thá»i gian!\"");
                        choices = new List<string> {
                            "Mua Quáº·ng Linh Tháº¡ch giÃ¡ ráº» (TiÃªu hao 3 VÃ ng) / 3 Gold",
                            "RÃºt lui / Leave"
                        };
                        onChoice = (idx) =>
                        {
                            if (idx == 0)
                            {
                                int goldIdx = CurrentBackpack.FindItemIndex("resource_gold");
                                if (goldIdx >= 0)
                                {
                                    CurrentBackpack.RemoveItemAt(goldIdx);
                                    CurrentBackpack.AddItem("item_iron_ore");
                                    DialogueResult("Giao Dá»‹ch Háº¡ng Tháº¥p", "Háº¯n nÃ©m cho báº¡n má»™t máº£nh Quáº·ng Linh Tháº¡ch thÃ´ ráº» tiá»n rá»“i thu tiá»n vÃ ng Ä‘áº§y thÃ´ báº¡o.");
                                }
                                else
                                {
                                    DialogueResult("KhÃ´ng CÃ³ VÃ ng", "KhÃ´ng cÃ³ vÃ ng! Háº¯n pháº¥t tay xua Ä‘uá»•i: \"KhÃ´ng cÃ³ tiá»n thÃ¬ biáº¿n Ä‘i chá»— khÃ¡c!\"");
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
                text = "M?t y si chó già dang ng?i nu?ng th?c an. Ông ta s?n sàng h?i ph?c ho?c ch?a tr? cho d?i c?a b?n.\n\n(Healing Pool: 100 HP)";
                choices = new List<string> {
                    "H?i máu toàn d?i (Chia d?u 100 HP)",
                    "Xóa Tê Li?t & Ki?t S?c toàn d?i (Mi?n phí)",
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
                            DialogueResult("H?i Ph?c Sinh L?c", "$C? d?i dã du?c chia d?u $pool HP t? Healing Pool.");
                        }
                        else 
                        {
                            DialogueResult("Không C?n Thi?t", "C? d?i dang d?y máu, không c?n h?i ph?c.");
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
                        DialogueResult("Gi?i Tr? Tr?ng Thái", "M?i tr?ng thái x?u dã du?c lo?i b?.");
                    }
                    else
                    {
                        CompleteNodeResolution();
                    }
                };
            }
            else if (type == NodeType.CampMerchant)
            {
                title = "ðŸ•ï¸ CAMP MERCHANT";
                text = "Má»™t thÆ°Æ¡ng nhÃ¢n háº¯c Ã¡m tiáº¿p cáº­n báº¡n vá»›i nhá»¯ng váº­t pháº©m tháº§n bÃ­. Äá»•i 2 Food hoáº·c 2 Gold Ä‘á»ƒ láº¥y 1 pháº§n thÆ°á»Ÿng báº¥t ká»³?";
                choices = new List<string> {
                    "Mua (Tá»‘n 2 Food/Gold)",
                    "Bá» qua"
                };
                onChoice = (idx) =>
                {
                    if (idx == 0)
                    {
                        if (CurrentBackpack != null && CurrentBackpack.IsFull)
                        {
                            DialogueResult("TÃºi Äá»“ ÄÃ£ Äáº§y!", "KhÃ´ng cÃ²n chá»— chá»©a! HÃ£y má»Ÿ tÃºi Ä‘á»“ (gÃ³c trÃ¡i) Ä‘á»ƒ vá»©t bá»›t váº­t pháº©m khÃ´ng cáº§n thiáº¿t, sau Ä‘Ã³ quay láº¡i nháº·t tiáº¿p.");
                            return;
                        }
                        
                        int foodIdx = CurrentBackpack.FindItemIndex("food");
                        int goldIdx = CurrentBackpack.FindItemIndex("resource_gold");
                        
                        if (foodIdx >= 0)
                        {
                            CurrentBackpack.RemoveItemAt(foodIdx);
                            CurrentBackpack.AddItem("item_ancient_relic_auto_collect");
                            DialogueResult("Giao d?ch thnh cng", "D?i 1 Food l?y C? V?t T? D?ng Nh?t!");
                        }
                        else if (goldIdx >= 0)
                        {
                            CurrentBackpack.RemoveItemAt(goldIdx);
                            CurrentBackpack.AddItem("item_ancient_relic_auto_farm");
                            DialogueResult("Giao d?ch thnh cng", "D?i 1 Gold l?y C? V?t T? D?ng Thu Ho?ch!");
                        }
                        else
                        {
                            DialogueResult("Khng d? ti?n", "Thuong nhn h? lnh v b?n khng c d? v?t ph?m trao d?i!");
                        }
                        CompleteNodeResolution();
                    }
                };
            }
            else if (type == NodeType.CampBlacksmith)
            {
                title = "ðŸ•ï¸ CAMP BLACKSMITH";
                text = "LÃ² rÃ¨n cá»§a má»™t thá»£ rÃ¨n lang thang.\n\nTá»‘n 2 Quáº·ng Sáº¯t Ä‘á»ƒ cÆ°á»ng hÃ³a táº¡m thá»i sá»©c máº¡nh cho cáº£ Ä‘á»™i (+5 HP Max, +10 Stamina Max)?";
                choices = new List<string> {
                    "RÃ¨n Trang Bá»‹ (Tá»‘n 2 Sáº¯t)",
                    "Bá» qua"
                };
                onChoice = (idx) =>
                {
                    if (idx == 0)
                    {
                        int oreIdx1 = CurrentBackpack.FindItemIndex("ore");
                        if (oreIdx1 >= 0)
                        {
                            CurrentBackpack.RemoveItemAt(oreIdx1);
                            foreach(var cat in ActiveCats) { if(cat != null) { cat.HealthPoints += 5; cat.Stamina += 10; } }
                            DialogueResult("CÆ°á»ng HÃ³a", "Cáº£ Ä‘á»™i Ä‘Æ°á»£c nÃ¢ng cáº¥p Ã¡o giÃ¡p vÃ  vÅ© khÃ­ táº¡m thá»i!");
                        }
                        else
                        {
                            DialogueResult("Thiáº¿u Quáº·ng Sáº¯t", "Thá»£ rÃ¨n láº¯c Ä‘áº§u, báº¡n khÃ´ng cÃ³ Ä‘á»§ quáº·ng sáº¯t (Ore).");
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
                title = "ðŸŽ REWARD NODE";
                text = "TrÆ°á»›c máº¯t báº¡n lÃ  má»™t rÆ°Æ¡ng kho bÃ¡u khá»•ng lá»“ bá»‹ bá» hoang. Ai Ä‘Ã³ Ä‘Ã£ gom ráº¥t nhiá»u váº­t pháº©m vÃ o Ä‘Ã¢y.";
                choices = new List<string> {
                    "Má»Ÿ RÆ°Æ¡ng!"
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
                    return; // Return early since TriggerWearyDogEncounter handles dialogue triggering
                }
                else
                {
                    title = "BÃ­ch Há»a Cá»• XÆ°a";
                    text = "Tráº£i rá»™ng trÃªn bá»©c tÆ°á»ng Ä‘Ã¡ rÃªu phong lÃ  nhá»¯ng bÃ­ch há»a mÃ´ táº£ vá» thá»i ká»³ 'Tháº§n MÃ¨o SÃ¡ng Tháº¿' vÃ  cuá»™c viá»…n chinh cá»• Ä‘áº¡i.\n\nLinh há»“n cá»§a toÃ n Ä‘á»™i Ä‘Æ°á»£c gá»™t rá»­a, giÃºp gia tÄƒng Speed táº¡m thá»i!";
                    choices = new List<string> { "Tiáº¿p thu tinh hoa" };
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
                title = "Pháº¿ TÃ­ch Hoang Pháº¿";
                text = "Äá»™i hÃ¬nh mÃ¨o tiáº¿n vÃ o má»™t pháº¿ tÃ­ch cung Ä‘iá»‡n Ä‘á»• nÃ¡t. á»ž giá»¯a cÃ³ má»™t lÃ² Ä‘an dÆ°á»£c cÅ© ká»¹ váº«n Ä‘ang chÃ¡y Ã¢m á»‰.\nBáº¡n cÃ³ muá»‘n lá»¥c lá»i khÃ´ng?";
                choices = new List<string> { "Má»Ÿ lÃ² Ä‘an dÆ°á»£c", "RÃºt lui" };
                onChoice = (idx) =>
                {
                    if (idx == 0)
                    {
                        if (UnityEngine.Random.value < 0.5f)
                        {
                            CurrentBackpack.AddItem("item_revive_pill");
                            DialogueResult("Luyá»‡n Äan Ká»³ TÃ­ch!", "Tuyá»‡t vá»i! BÃªn trong lÃ² Ä‘an váº«n cÃ²n lÆ°u giá»¯ má»™t viÃªn Linh Äan Há»“i Sinh cá»±c ká»³ quÃ½ hiáº¿m!");
                        }
                        else
                        {
                            DialogueResult("KhÃ³i Ä‘en mÃ¹ má»‹t", "LÃ² Ä‘an ná»• tung! KhÃ³i Ä‘en ká»‹t pháº£ tháº³ng vÃ o máº·t khiáº¿n toÃ n Ä‘á»™i bÃ¡m Ä‘áº§y tro bá»¥i (KhÃ´ng cÃ³ tá»•n tháº¥t thá»±c táº¿).");
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
                MewtationsLoc.Translate("opt_fight", "âš”ï¸ Force breakthrough (+20 Corruption)"),
                () =>
                {
                    RunState.AddCorruption(20);
                    DialogueResult(
                        MewtationsLoc.Translate("dog_fight_res", "Bloody Skirmish!"),
                        MewtationsLoc.Translate("dog_fight_res_desc", "You fought and defeated the guard. The path is clear, but at a bloody cost (+20 Corruption).")
                    );
                }
            ));

            // Option 2: Stealth
            choices.Add(new Mewtations.Dialogue.DialogueChoice(
                MewtationsLoc.Translate("opt_stealth", "ðŸƒ Sneak past silently (Requires Speed > 115)"),
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

            // Option 3: Comfort (Thiá»n Äáº¡o Cáº£m HÃ³a)
            choices.Add(new Mewtations.Dialogue.DialogueChoice(
                MewtationsLoc.Translate("opt_comfort", "â˜¯ï¸ [Zen Dao Comfort] Teach human philosophy & soothe his soul"),
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
                    CurrentBackpack.AddItem(hintId);

                    RunState.CorruptionLevel = Mathf.Max(0, RunState.CorruptionLevel - 25);

                    DialogueResult(
                        MewtationsLoc.Translate("dog_comfort_success", "A Soul Redeemed!"),
                        MewtationsLoc.Translate("dog_comfort_success_desc", "The officer wept upon hearing your Zen words, realizing both Cats and Dogs are victims of the system. He abandons his post, giving you an Ancient Scroll and purging your sins (-25 Corruption)!")
                    );
                },
                () => ActiveCats.Any(c => c.Specialization == Mewtations.Cards.Cats.DaoSpecialization.ZenDao),
                MewtationsLoc.Translate("opt_comfort_req", "Cáº§n cÃ³ MÃ¨o Thiá»n Äáº¡o / Requires Zen Cat")
            ));

            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, choices);
        }

        private void DialogueResult(string title, string text)
        {
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { "Tiáº¿p tá»¥c" }, (idx) =>
            {
                CompleteNodeResolution();
            });
        }

                public void CompleteNodeResolution()
        {
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

            // Automation Relic Tick logic
            ApplyRelicAutomationProgress();

            // Node is cleared. Check connections of visited nodes to unlock next layer nodes
            UpdateConnections();

            // Check if final boss node was visited and solved
            if (ActiveNode != null && ActiveNode.Type == NodeType.Boss)
            {
                Debug.Log("[Expedition] HoÃ n thÃ nh boss viá»…n chinh! Tháº¯ng lá»£i lá»›n!");
                ReturnToBase(isDefeat: false);
            }
            else
            {
                // Return to map overlay
                if (ExpeditionMapUI.Instance != null)
                {
                    ExpeditionMapUI.Instance.ShowWindow();
                }
            }
        }

        private void ApplyRelicAutomationProgress()
        {
            if (RunState == null || string.IsNullOrEmpty(RunState.EquippedRelicId)) return;

            string relic = RunState.EquippedRelicId;
            Debug.Log($"[Relic Automation] KÃ­ch hoáº¡t Cá»• Váº­t {relic} tá»± Ä‘á»™ng hÃ³a cÄƒn cá»© tá»« xa!");

            foreach (var gc in WorldManager.instance.AllCards)
            {
                if (gc != null && !gc.Destroyed && gc.CardData != null && gc.TimerRunning)
                {
                    string cid = gc.CardData.Id.ToLower();
                    
                    if (relic == "item_ancient_relic_smelt" && (cid.Contains("smelter") || cid.Contains("furnace")))
                    {
                        gc.CurrentTimerTime += 15f; // Smelting automation ticks by 15s!
                        Debug.Log($"   â€¢ [Cá»• Váº­t Tá»± Äá»™ng Nung] Tá»± Ä‘á»™ng thÃºc tiáº¿n +15s cho {gc.CardData.Name}!");
                    }
                    else if (relic == "item_ancient_relic_wood" && (cid.Contains("sawmill") || cid.Contains("mill")))
                    {
                        gc.CurrentTimerTime += 15f; // Wood processing automation ticks by 15s!
                        Debug.Log($"   â€¢ [Cá»• Váº­t Tá»± Äá»™ng Xáº»] Tá»± Ä‘á»™ng thÃºc tiáº¿n +15s cho {gc.CardData.Name}!");
                    }
                    else if (relic == "item_ancient_relic_booster")
                    {
                        gc.CurrentTimerTime += 5f; // Universal booster ticks all timers by 5s!
                        Debug.Log($"   â€¢ [Linh Tháº§n Thu Hoáº¡ch] Tá»± Ä‘á»™ng thÃºc tiáº¿n +5s cho cÃ´ng trÃ¬nh {gc.CardData.Name}!");
                    }
                }
            }
        }

        private void UpdateConnections()
        {
            if (ActiveNode == null) return;

            // Lock all nodes first
            foreach (var n in MapNodes)
            {
                if (n.State == NodeState.Available)
                {
                    n.State = NodeState.Locked;
                }
            }

            // Unlock nodes connected to the current active node
            foreach (int connectedId in ActiveNode.OutgoingConnections)
            {
                var targetNode = MapNodes.Find(n => n.Id == connectedId);
                if (targetNode != null && targetNode.State == NodeState.Locked)
                {
                    targetNode.State = NodeState.Available;
                }
            }

            // Always make layer 0 available if nothing has been visited yet
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
                // If it's not a defeat and we survived a run (or even if we retreated? The rule says "Sau má»—i map thÆ°á»ng: Counter++")
                bool visitedSpecial = MapNodes != null && MapNodes.Any(n => n.State == NodeState.Visited && n.Type == NodeType.SpecialMap);
                if (!visitedSpecial)
                {
                    Mewtations.Legacy.Stacklands.SaveManager.instance.CurrentSave.ExpeditionSpecialMapPityCounter++;
                }
            }
            IsExpeditionActive = false;
            State = ExpeditionState.Idle;

            // Close UI overlays
            if (ExpeditionMapUI.Instance != null) ExpeditionMapUI.Instance.HideWindow();
            if (CombatOverlayUI.Instance != null) CombatOverlayUI.Instance.HideWindow();
            if (Mewtations.Dialogue.DialogueSystem.Instance != null) Mewtations.Dialogue.DialogueSystem.Instance.HideWindow();

            // Resume base board time
            WorldManager.WorldSimulationPaused = false;

            if (PortalCardSource != null)
            {
                Vector3 spawnPos = PortalCardSource.transform.position + Vector3.back * 1.5f;

                // Return cats to base board and clear active temporary mutations
                foreach (var cat in ActiveCats)
                {
                    if (cat != null)
                    {
                        cat.ClearMutations(); // Mutations cleared upon returning to base!
                        
                        // Apply state from RuntimeCatStates
                        if (RuntimeCatStates.TryGetValue(cat.UniqueId, out var state))
                        {
                            cat.HealthPoints = state.HP;
                            cat.Stamina = state.Stamina;
                            cat.IsExhausted = state.IsExhausted;
                            cat.IsParalyzed = state.IsParalyzed;
                            cat.ExhaustionLevel = state.ExhaustionLevel;
                        }

                        // Phase 3: Expedition Aftermath (Exhaustion Debt)
                        int staminaDebt = 20; // Base stamina cost of going on an expedition
                        if (RunState != null) {
                            staminaDebt += (RunState.CurrentLayer * 5); // +5 stamina per layer deepened
                        }
                        cat.Stamina = UnityEngine.Mathf.Max(0, cat.Stamina - staminaDebt);
                        
                        // Adding Memoirs
                        if (cat.Stamina == 0) {
                            cat.AddMemoir("Trá»Ÿ vá» trong tráº¡ng thÃ¡i kiá»‡t sá»©c! (Exhausted Return)");
                        }
                        if (RunState != null && RunState.CorruptionLevel > 50) {
                            cat.AddMemoir("Trá»Ÿ vá» vá»›i tÃ  khÃ­ (Corrupted Return)");
                        }
                          int insuredSlots = 0;
                          if (BackpackCardSource is Mewtations.Legacy.Stacklands.StorageRingCardData ringData) insuredSlots = ringData.InsuredSlots;
                        if (isManualRetreat) {
                            cat.AddMemoir("Bá» trá»‘n khá»i viá»…n chinh (Retreat)");
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
                    // Dung há»£p thiÃªn phÃº vÄ©nh viá»…n (Song Trá»ng Dá»‹ Biáº¿n: tá»‘i Ä‘a 2 Ä‘á»™t biáº¿n vÄ©nh viá»…n)
                    MutationPersistenceSystem.ProcessRunVictoryTraits(ActiveCats);

                    // Spawn Backpack loot items around the portal safely
                    foreach (var cardId in CurrentBackpack.ContainedCardIds)
                    {
                        var spawnedCard = WorldManager.instance.GetCardWithUniqueId(cardId);
                        if (spawnedCard != null)
                        {
                            spawnedCard.transform.position = spawnPos;
                            spawnedCard.gameObject.SetActive(true);
                            WorldManager.instance.SendToBoard(spawnedCard, WorldManager.instance.CurrentBoard, spawnPos);
                            
                            // Randomize spread
                            spawnPos.x += UnityEngine.Random.Range(-0.5f, 0.5f);
                            spawnPos.z += UnityEngine.Random.Range(-0.5f, 0.5f);
                        }
                    }

                    // Special Boss Victory Reward: A new Heavenly Talent cat!
                    if (ActiveNode != null && ActiveNode.Type == NodeType.Boss)
                    {
                        var summoning = new CatSummoningSystem(WorldManager.instance);
                        summoning.SummonCat(spawnPos, highestBreakthroughLevel: 2); // Guaranteed breakthrough potential
                        Debug.Log("[Expedition] Triá»‡u há»“i ThiÃªn KiÃªu mÃ¨o lÃ m pháº§n thÆ°á»Ÿng chiáº¿n tháº¯ng Boss!");
                    }
                }
                else
                {
                    // Calculate and apply scaled drop penalty on force abandon/retreat/defeat
                    if (CurrentBackpack != null)
                    {
                          int insuredSlots = 0;
                          if (BackpackCardSource is Mewtations.Legacy.Stacklands.StorageRingCardData ringData) insuredSlots = ringData.InsuredSlots;
                        if (isManualRetreat)
                        {
                            // Cowardice Tax: lose exactly 50% of backpack items randomly, and add +15 Greed!
                            if (RunState != null)
                            {
                                RunState.GreedLevel = Mathf.Min(100, RunState.GreedLevel + 15);
                            }
                            ExpeditionExtractionSystem.ApplyManualRetreatPenalty(CurrentBackpack, insuredSlots);
                            Debug.Log("[Expedition] NgÆ°á»i chÆ¡i chá»§ Ä‘á»™ng rÃºt lui! Ãp dá»¥ng Thuáº¿ NhÃ¡t Gan: Máº¥t ngáº«u nhiÃªn 50% balo, +15 Greed khÃ­ váº­n.");
                        }
                        else
                        {
                            float rate = ExpeditionExtractionSystem.CalculateLootRetentionRate(RunState, CurrentBackpack);
                              ExpeditionExtractionSystem.ApplyAbandonPenalty(CurrentBackpack, rate, insuredSlots);
                            Debug.Log("[Expedition] Viá»…n chinh tháº¥t báº¡i hoáº·c bá»‹ tiÃªu diá»‡t! Ãp dá»¥ng hÃ¬nh pháº¡t hao há»¥t balo nghiÃªm trá»ng.");
                        }
                        foreach (var cardId in CurrentBackpack.ContainedCardIds)
                        {
                            var spawnedCard = WorldManager.instance.GetCardWithUniqueId(cardId);
                            if (spawnedCard != null)
                            {
                                spawnedCard.transform.position = spawnPos;
                                spawnedCard.gameObject.SetActive(true);
                                WorldManager.instance.SendToBoard(spawnedCard, WorldManager.instance.CurrentBoard, spawnPos);
                                spawnPos.x += UnityEngine.Random.Range(-0.5f, 0.5f);
                                spawnPos.z += UnityEngine.Random.Range(-0.5f, 0.5f);
                            }
                        }
                    }
                }

                // Restore Backpack Card if present
                if (BackpackCardSource != null && BackpackCardSource.MyGameCard != null)
                {
                    BackpackCardSource.MyGameCard.transform.position = spawnPos + Vector3.right * 1.0f;
                    BackpackCardSource.MyGameCard.gameObject.SetActive(true);
                }

                // Restore Relic Card if present
                if (RelicCardSource != null && RelicCardSource.MyGameCard != null)
                {
                    RelicCardSource.MyGameCard.transform.position = spawnPos + Vector3.left * 1.0f;
                    RelicCardSource.MyGameCard.gameObject.SetActive(true);
                }
                RelicCardSource = null;
                RunState.EquippedRelicId = "";

                // If portal is strange/one-time, destroy it
                if (PortalCardSource.CardData.Id == "strange_portal")
                {
                    PortalCardSource.DestroyCard(false, true);
                }
            }

            Debug.Log("[Expedition] Káº¿t thÃºc viá»…n chinh. Trá»Ÿ vá» base.");
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
            list.SetOrAdd("Expedition_EquippedRelicId", RunState.EquippedRelicId);
            list.SetOrAdd("Expedition_ActiveCatsUniqueIds", string.Join(",", ActiveCats.Select(c => c.UniqueId)));
            list.SetOrAdd("Expedition_BackpackMaxCapacity", CurrentBackpack != null ? CurrentBackpack.MaxCapacity.ToString() : "10");
            list.SetOrAdd("Expedition_BackpackItems", CurrentBackpack != null ? string.Join(",", CurrentBackpack.ContainedCardIds) : "");
            
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

            // Load persisted unlocked hints
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
            RunState.EquippedRelicId = GetValueOrDefault(list, "Expedition_EquippedRelicId", "");

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

            int backpackCap = int.Parse(GetValueOrDefault(list, "Expedition_BackpackMaxCapacity", "10"));
            CurrentBackpack = new Backpack(backpackCap);
            string backpackItemsStr = GetValueOrDefault(list, "Expedition_BackpackItems", "");
            if (!string.IsNullOrEmpty(backpackItemsStr))
            {
                foreach (string item in backpackItemsStr.Split(','))
                {
                    CurrentBackpack.AddItem(item);
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


            // Hide the actual game cards of cats and backpack from board
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

            // Re-open UI overlay based on state
            if (State == ExpeditionState.MapNavigation && ExpeditionMapUI.Instance != null)
            {
                ExpeditionMapUI.Instance.ShowWindow();
            }
        }
    }
}




