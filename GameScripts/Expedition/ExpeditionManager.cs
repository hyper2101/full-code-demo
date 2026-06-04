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
        public CardData RelicCardSource = null; //
        
        public ExpeditionRunContext Context = null; //
        public Action<Mewtations.Combat.Core.CombatResultData> CurrentCombatCallback;


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
                        continue; //
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

            //
            if (GameScripts.Systems.Threat.ThreatManager.Instance != null && GameScripts.Systems.Threat.ThreatManager.Instance.HasActivePenalty(GameScripts.Systems.Threat.ThreatPenaltyType.LockExpedition))
            {
                WorldManager.instance.CreateFloatingText(context.Ordering != null ? context.Ordering.MyGameCard : null, false, 0, Mewtations.Core.MewtationsLoc.Translate("exp_fail_start", "Lỗi khởi hành"), "", false, 0, 2f, true);
                return;
            }

            var cats = GetExpeditionEligibleCats();
            if (cats.Count == 0)
            {
                WorldManager.instance.CreateFloatingText(context.Ordering != null ? context.Ordering.MyGameCard : null, false, 0, Mewtations.Core.MewtationsLoc.Translate("exp_fail_start", "Lỗi khởi hành"), "", false, 0, 2f, true);
                return;
            }

            int capacity = 10; //
            int seed = UnityEngine.Random.Range(0, 100000);

            //
            RelicCardSource = null;

            ExecuteStartExpedition(context, cats, capacity, seed);
        }

        private void ExecuteStartExpedition(ExpeditionRunContext context, List<CatCardData> cats, int capacity, int seed)
        {
            IsExpeditionActive = true;
            State = ExpeditionState.MapNavigation;
            
            //
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
            
            Context = context; //

            PortalCardSource = null; //
            ActiveCats = cats;
            BackpackCardSource = null; //

            // Removed hiding logic for cats per user request. Cats stay on the board.
            ActiveCats = cats; // Keep reference if needed for text events, but don't hide them.

            CurrentMapSeed = seed;
            MapNodes = ExpeditionMapGenerator.GenerateMap(seed, maxLayers: 6, maxNodesPerLayer: 3);
            ActiveNode = null;

            //
            ExpeditionRiskSystem.InitializeRunStats(RunState);

            //
            WorldManager.WorldSimulationPaused = true;

            //
            if (ExpeditionMapUI.Instance != null)
            {
                ExpeditionMapUI.Instance.ShowWindow();
            }
            else
            {
                Debug.LogError("[ExpeditionManager] ExpeditionMapUI.Instance is null!");
            }

            Debug.Log("[Expedition] Started");
                if (GameScripts.Systems.Threat.ThreatManager.Instance != null && GameScripts.Systems.Threat.ThreatManager.Instance.CatGodWrathTemplate != null)
                {
                    //
                    GameScripts.Systems.Threat.ThreatManager.Instance.CreateThreat(
                        GameScripts.Systems.Threat.ThreatManager.Instance.CatGodWrathTemplate, 
                        GameScripts.Systems.Threat.ThreatSourceType.Expedition, 
                        RunState.CurrentDifficultyLevel, 
                        0 //
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

            //

            //
            if (node.Theme == RouteTheme.TaDao)
            {
                RunState.AddCorruption(25);
                Debug.Log("[Expedition] Log");
            }
            else if (node.Theme == RouteTheme.ThamLam)
            {
                RunState.AddGreed(10);
                Debug.Log("[Expedition] Log");
            }

            //
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
                //
                TriggerTextEventNode(node.Type);
            }
        }

        private void TriggerCombat(bool isBoss)
        {
            //
            List<Combatable> enemies = new List<Combatable>();
            int enemyCount = UnityEngine.Random.Range(1, 4);
            if (isBoss) enemyCount = 1; //

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

            //
            List<Combatable> playerCombats = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Cast<Combatable>(System.Linq.Enumerable.Where(ActiveCats, c => c != null && !c.IsParalyzed)));
            TurnBasedCombatManager.Instance.StartCombat(playerCombats, enemies, (result) =>
            {
                //
                foreach (var enemy in enemies)
                {
                    if (enemy != null && enemy.MyGameCard != null)
                    {
                        enemy.MyGameCard.DestroyCard(true, true);
                    }
                }

                if (result == CombatResult.Victory)
                {
                    //
                    RollLootForCombat(isBoss);
                    CompleteNodeResolution();
                }
                else if (result == CombatResult.Retreated)
                {
                    //
                    Debug.Log("[Expedition] Log");
                    ReturnToBase(isDefeat: true);
                }
                else
                {
                    //
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

            //
            if (isBoss)
            {
                string[] relics = { "item_ancient_relic_auto_farm", "item_ancient_relic_auto_collect", "item_ancient_relic_auto_heal" };
                string chosenRelic = relics[UnityEngine.Random.Range(0, relics.Length)];
                rolled.Add(chosenRelic);
                RunState.PendingRewards.Add(chosenRelic);
                Debug.Log("[Expedition] Log");
            }

            string lootMsg = string.Join(", ", rolled.Select(id => id.Replace("resource_", "").Replace("item_", "")));
            Debug.Log("[Expedition] Log");
        }

        private void TriggerResourceNode()
        {
            //
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
                : Mewtations.Core.MewtationsLoc.Translate("exp_res_empty", "KhÃ´ng cÃ³ chá»— chá»©a trong Balo!");

            //
            string title = Mewtations.Core.MewtationsLoc.Translate("exp_res_title", "Thu tháº­p TÃ i NguyÃªn");
            string text = string.Format(Mewtations.Core.MewtationsLoc.Translate("exp_res_desc", "Äá»™i cá»§a báº¡n Ä‘Ã£ tÃ¬m tháº¥y má»™t bÃ£i tÃ i nguyÃªn trÃ¹ phÃº!\n\nNháº­n Ä‘Æ°á»£c: {0}"), resMsg);
            
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { Mewtations.Core.MewtationsLoc.Translate("exp_res_continue", "Tuyá»‡t vá»i!") }, (choiceIdx) =>
            {
                CompleteNodeResolution();
            });
        }

                private void TriggerTextEventNode(NodeType type)
        {
            int eventRoll = UnityEngine.Random.Range(0, 7);
            Action<int> onChoice = null;
            System.Collections.Generic.List<string> choices = new System.Collections.Generic.List<string>();
            string title = "";
            string text = "";

            if (type == NodeType.Event)
            {
                if (eventRoll == 0)
                {
                    title = Mewtations.Core.MewtationsLoc.Translate("exp_evt0_title", "⚡ KIẾP LÔI THỬ THÁCH");
                    text = Mewtations.Core.MewtationsLoc.Translate("exp_evt0_desc", "Đội ngũ mèo đi tới một đỉnh núi hoang vắng, mây đen cuộn trào nghẹt thở. Từng tia lôi điện khổng lồ giáng xuống như Lôi Kiếp độ kiếp!\n\nLôi linh lực cuồng bạo này ẩn chứa cơ duyên lớn nhưng cực kỳ nguy hiểm. Bạn muốn làm gì?");
                    choices = new System.Collections.Generic.List<string> {
                        Mewtations.Core.MewtationsLoc.Translate("exp_evt0_opt0", "Hấp thụ Lôi Kiếp (Yêu cầu mèo hệ Sét hoặc Tốc độ cao)"),
                        Mewtations.Core.MewtationsLoc.Translate("exp_evt0_opt1", "Đỡ đòn hộ đồng đội (Yêu cầu Tank bảo vệ)"),
                        Mewtations.Core.MewtationsLoc.Translate("exp_evt0_opt2", "Trận pháp phòng thủ (Lách qua an toàn)")
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
                                luckyCat.AddMemoir(MemoirType.Breakthrough, Mewtations.Core.MewtationsLoc.Translate("exp_evt0_t1", "Lôi Kiếp Tẩy Tủy"), Mewtations.Core.MewtationsLoc.Translate("exp_evt0_d1", "Hấp thụ lôi điện đột phá võ đạo (+25 Speed)"));
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt0_t1", "Lôi quang rực rỡ!"), string.Format(Mewtations.Core.MewtationsLoc.Translate("exp_evt0_d1", "Tuyệt đỉnh! Nhờ sự nhạy bén cực độ (hoặc linh căn hệ Sét), chú mèo <b>{0}</b> đã hấp thụ trọn vẹn Lôi Điện Phạt, vĩnh viễn gia tăng <b>+25 Thần Tốc</b>!"), luckyCat.Name));
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsUltimateLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, Mewtations.Core.MewtationsLoc.Translate("exp_evt0_t2", "Tẩu Hỏa Nhập Ma"), Mewtations.Core.MewtationsLoc.Translate("exp_evt0_d2", "Trúng lôi điện bạo phát bế tắc linh mạch, khóa kỹ năng Nộ"));
                                foreach (var cat in ActiveCats) { cat.HealthPoints = UnityEngine.Mathf.Max(1, cat.HealthPoints - 10); }
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt0_t2", "Lôi Phạt Oanh Tạc!"), string.Format(Mewtations.Core.MewtationsLoc.Translate("exp_evt0_d2", "Tốc độ quá chậm! Lôi điện cuồng bạo thâm nhập tàn phá kinh mạch. Chú mèo <b>{0}</b> bị <b><color=red>TẨU HỎA NHẬP MA (KHÓA KỸ NĂNG NỘ)</color></b> vĩnh viễn, toàn đội thương nặng (-10 HP)!"), victim.Name));
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
                                tank.AddMemoir(MemoirType.Breakthrough, Mewtations.Core.MewtationsLoc.Translate("exp_evt0_t3", "Hộ Thể Lôi Kiếp"), Mewtations.Core.MewtationsLoc.Translate("exp_evt0_d3", "Đỡ lôi kiếp cho đồng đội, khóa ô Thiên Phú"));
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt0_t3", "Hộ Thể Tuyệt Vời!"), string.Format(Mewtations.Core.MewtationsLoc.Translate("exp_evt0_d3", "Anh hùng! Chú Tank <b>{0}</b> đứng ra đỡ lôi phạt cho toàn đội. Thần thể được cường hóa (+10 Max HP) nhưng bùa chú bị phá hủy hoàn toàn, <b><color=red>ô Thiên Phú (Passive Slots) vĩnh viễn bị KHÓA</color></b>!"), tank.Name));
                            }
                            else
                            {
                                foreach (var cat in ActiveCats) { cat.HealthPoints = UnityEngine.Mathf.Max(1, cat.HealthPoints - 15); }
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt0_t4", "Hộ Vệ Thất Bại!"), Mewtations.Core.MewtationsLoc.Translate("exp_evt0_d4", "Đội hình không có hộ vệ Tank chuyên nghiệp! Buộc phải dùng thân xác trần tục chống đỡ, toàn đội bị thương tổn cực nặng (-15 HP)!"));
                            }
                        }
                        else
                        {
                            DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt0_t5", "Lách Qua An Toàn"), Mewtations.Core.MewtationsLoc.Translate("exp_evt0_d5", "Toàn đội thiết lập kết giới phòng thủ thô sơ, cẩn thận đi vòng qua ngọn núi lôi kiếp an toàn."));
                        }
                    };
                }
                else if (eventRoll == 1)
                {
                    title = Mewtations.Core.MewtationsLoc.Translate("exp_evt1_title", "🐕 TRẠM TUẦN TRA CỦA CHÚNG CHÓ");
                    text = Mewtations.Core.MewtationsLoc.Translate("exp_evt1_desc", "Phía trước xuất hiện chốt gác kiên cố của loài Chó kiểm soát trật tự xã hội. Lính tuần tra chó bọc giáp sắt đang canh phòng nghiêm ngặt.\n\nĐội mèo của bạn mang theo balo đầy ắp tài nguyên khả nghi. Bạn muốn ứng phó thế nào?");
                    choices = new System.Collections.Generic.List<string> {
                        Mewtations.Core.MewtationsLoc.Translate("exp_evt1_opt0", "Đút lót hối lộ (Tiêu hao 1 Vàng)"),
                        Mewtations.Core.MewtationsLoc.Translate("exp_evt1_opt1", "Quyết chiến đột phá (Thắng lợi đẫm máu)"),
                        Mewtations.Core.MewtationsLoc.Translate("exp_evt1_opt2", "Lén lút lẻn qua (Yêu cầu Tốc độ cao)"),
                        Mewtations.Core.MewtationsLoc.Translate("exp_evt1_opt3", "Thuyết giảng tâm lý (Cần Thần Miêu Thiền Đạo)")
                    };
                    onChoice = (idx) =>
                    {
                        if (idx == 0)
                        {
                            if (ConsumeItemFromOrdering("resource_gold")) {
                                RunState.GreedLevel = UnityEngine.Mathf.Max(0, RunState.GreedLevel - 20);
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt1_t1", "Hối Lộ Thành Công!"), Mewtations.Core.MewtationsLoc.Translate("exp_evt1_d1", "Lính tuần tra Chó nhận lấy Vàng, cười nham nhở mở cổng cho đi qua. Sức ép luật pháp xoa dịu (-20 Greed)!"));
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsEquipmentSlotsLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, Mewtations.Core.MewtationsLoc.Translate("exp_evt1_t2", "Tịch Thu Trang Bị"), Mewtations.Core.MewtationsLoc.Translate("exp_evt1_d2", "Không có tiền đút lót, bị lính tuần tra khóa ô trang bị"));
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt1_t2", "Không Có Tiền Đút Lót!"), string.Format(Mewtations.Core.MewtationsLoc.Translate("exp_evt1_d2", "Balo không có Vàng để hối lộ! Lính tuần tra nổi giận khám xét toàn đội. Chú mèo <b>{0}</b> bị tịch thu sạch vũ khí bùa chú và vĩnh viễn <b><color=red>KHÓA ô Trang Bị (Equipment Slots)</color></b>!"), victim.Name));
                            }
                        }
                        else if (idx == 1)
                        {
                            foreach (var cat in ActiveCats) { cat.HealthPoints = UnityEngine.Mathf.Max(1, cat.HealthPoints - 8); }
                            RunState.PendingRewards.Add("resource_gold");
                            RunState.PendingRewards.Add("item_iron_ore");
                            RunState.AddCorruption(25);
                            DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt1_t3", "Huyết Chiến Đột Phá!"), Mewtations.Core.MewtationsLoc.Translate("exp_evt1_d3", "Toàn đội tuốt kiếm liều chết xông vào! Tiêu diệt toàn bộ lính canh, cướp lấy Vàng và Quặng sắt trong rương chốt tuần tra, toàn đội bị thương nhẹ (-8 HP) và tăng mạnh sát nghiệp (+25 Corruption)!"));
                        }
                        else if (idx == 2)
                        {
                            int avgSpeed = (int)ActiveCats.Average(c => c.Speed);
                            if (avgSpeed > 115)
                            {
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt1_t4", "Lẻn Qua Thành Công!"), Mewtations.Core.MewtationsLoc.Translate("exp_evt1_d4", "Bóng ma bóng tối! Bằng bước di chuyển thần tốc, không tiếng động, toàn đội mèo đã lướt qua trạm canh gác trót lọt mà lính chó không hề hay biết!"));
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsEquipmentSlotsLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, Mewtations.Core.MewtationsLoc.Translate("exp_evt1_t5", "Bắt Giữ Phong Ấn"), Mewtations.Core.MewtationsLoc.Translate("exp_evt1_d5", "Lén lẻn thất bại, bị khóa ô trang bị hình phạt"));
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt1_t5", "Bị Bắt Quả Tang!"), string.Format(Mewtations.Core.MewtationsLoc.Translate("exp_evt1_d5", "Tốc độ trung bình quá chậm! Lính chó phát hiện bắt giữ toàn đội tra khảo. Chú mèo <b>{0}</b> bị tịch thu khí giới, <b><color=red>ô Trang bị vĩnh viễn bị KHÓA</color></b> làm hình phạt!"), victim.Name));
                            }
                        }
                        else if (idx == 3)
                        {
                            var zenCat = ActiveCats.Find(c => c.Specialization == Mewtations.Cards.Cats.DaoSpecialization.ZenDao);
                            if (zenCat != null)
                            {
                                RunState.PendingRewards.Add("item_heavenly_relic");
                                RunState.CorruptionLevel = UnityEngine.Mathf.Max(0, RunState.CorruptionLevel - 20);
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt1_t6", "Giác Ngộ Đạo Tâm!"), string.Format(Mewtations.Core.MewtationsLoc.Translate("exp_evt1_d6", "Giác ngộ thành công! Chú mèo Thiền Đạo <b>{0}</b> đã thuyết giảng Đạo lý Nhân sinh cực kỳ thâm sâu, khai mở đạo tâm cho lính tuần tra Chó thoát khỏi sự kiểm soát gò bó của hệ thống.\n\nChú Chó cảm kích rơi lệ, mở cổng tặng toàn đội viên <b>Chí Tôn Cổ Khí (Heavenly Relic)</b> cực hiếm và xoa dịu tà khí (-20 Corruption)!"), zenCat.Name));
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.HealthPoints = UnityEngine.Mathf.Max(1, victim.HealthPoints - 10);
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt1_t7", "Giáo Hóa Thất Bại!"), string.Format(Mewtations.Core.MewtationsLoc.Translate("exp_evt1_d7", "Đội hình không có Thần Miêu Thiền Đạo để giảng giải Đạo pháp thuyết phục! Lính tuần tra Chó cho rằng bạn đang sỉ nhục trí tuệ của họ, nổi giận dùng roi điện đánh thương <b>{0}</b> (-10 HP)!"), victim.Name));
                            }
                        }
                    };
                }
                else if (eventRoll == 2)
                {
                    title = Mewtations.Core.MewtationsLoc.Translate("exp_evt2_title", "⚗️ LÒ LUYỆN ĐAN CỔ KÍNH");
                    text = Mewtations.Core.MewtationsLoc.Translate("exp_evt2_desc", "Đan điện phế tích u ám hiện ra trước mắt. Ở trung tâm sảnh lớn là một lò luyện cổ vẫn cháy âm ỉ lửa tím nhạt rò rỉ khí độc. Bên trong có thể ẩn chứa nghịch thiên linh đan hoặc kịch độc phế linh mạch.\n\nAi sẽ đứng ra xử lý chiếc lò đan này?");
                    choices = new System.Collections.Generic.List<string> {
                        Mewtations.Core.MewtationsLoc.Translate("exp_evt2_opt0", "Dùng linh độc hóa giải (Cần mèo hệ Độc)"),
                        Mewtations.Core.MewtationsLoc.Translate("exp_evt2_opt1", "Lực lượng cưỡng chế mở lò (Rủi ro 50/50)"),
                        Mewtations.Core.MewtationsLoc.Translate("exp_evt2_opt2", "Đập phá lò thu phế liệu (An toàn)")
                    };
                    onChoice = (idx) =>
                    {
                        if (idx == 0)
                        {
                            var poisonCat = ActiveCats.Find(c => c.Element == CatElement.Poison);
                            if (poisonCat != null)
                            {
                                RunState.PendingRewards.Add("item_breakthrough_pill");
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt2_t1", "Khống Chế Kịch Độc!"), string.Format(Mewtations.Core.MewtationsLoc.Translate("exp_evt2_d1", "Tuyệt đỉnh! Nhờ linh căn kịch độc bẩm sinh của <b>{0}</b>, chú đã trung hòa đan khí tím, mở lò lấy được viên <b>ĐỘT PHÁ LINH ĐAN</b> cực kỳ quý giá!"), poisonCat.Name));
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsPillSlotLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, Mewtations.Core.MewtationsLoc.Translate("exp_evt2_t2", "Linh Mạch Độc Ứ"), Mewtations.Core.MewtationsLoc.Translate("exp_evt2_d2", "Khí độc tàn phá kinh mạch đan dược, khóa ô Linh Đan"));
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt2_t2", "Không Có Mèo Hệ Độc!"), string.Format(Mewtations.Core.MewtationsLoc.Translate("exp_evt2_d2", "Độc khí tím bùng phát cuồn cuộn do không có mèo hệ Độc khống chế! Chú mèo <b>{0}</b> hít phải độc sương tàn phá phế linh mạch, <b><color=red>ô Linh Đan (Pill Slot) vĩnh viễn bị KHÓA</color></b>!"), victim.Name));
                            }
                        }
                        else if (idx == 1)
                        {
                            if (UnityEngine.Random.value < 0.5f)
                            {
                                RunState.PendingRewards.Add("item_breakthrough_pill");
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt2_t3", "Vận May Nghịch Thiên!"), Mewtations.Core.MewtationsLoc.Translate("exp_evt2_d3", "Vận may mỉm cười! Dù khí độc bốc lên ngùn ngụt nhưng toàn đội đã nhanh tay cướp lấy viên <b>ĐỘT PHÁ LINH ĐAN</b> thành công trước khi độc chấn nổ ra!"));
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsPillSlotLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, Mewtations.Core.MewtationsLoc.Translate("exp_evt2_t4", "Lò Đan Nổ Tung"), Mewtations.Core.MewtationsLoc.Translate("exp_evt2_d4", "Trúng khí độc lò đan nổ, khóa ô Linh Đan"));
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt2_t4", "Lò Đan Nổ Tung!"), string.Format(Mewtations.Core.MewtationsLoc.Translate("exp_evt2_d4", "Bùm! Lò luyện đan phát nổ lớn bắn ra tàn dư đan dược kịch độc. Chú mèo <b>{0}</b> trúng độc ngưng kết kinh mạch đan điền, <b><color=red>ô Linh Đan vĩnh viễn bị KHÓA</color></b>!"), victim.Name));
                            }
                        }
                        else
                        {
                            RunState.PendingRewards.Add("item_stone");
                            RunState.PendingRewards.Add("item_iron_ore");
                            DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt2_t5", "Thu Hoạch Phế Liệu"), Mewtations.Core.MewtationsLoc.Translate("exp_evt2_d5", "Quyết định sáng suốt! Toàn đội đập vỡ lò đan an toàn, thu về Đá vụn và Sắt phế liệu bỏ vào Balo viễn chinh."));
                        }
                    };
                }
                else if (eventRoll == 3)
                {
                    title = Mewtations.Core.MewtationsLoc.Translate("exp_evt3_title", "🔴 MA HUYỆT KHẤN NGUYỆN");
                    text = Mewtations.Core.MewtationsLoc.Translate("exp_evt3_desc", "Một ma huyệt phát ra hồng quang rực máu ngăn giữa đường đi. Linh khí bên trong cuộn trào quyến rũ, như khơi dậy ý niệm Tham Lam tột cùng của loài mèo.\n\nThần linh đòi hỏi cúng nạp linh thực ăn uống hoặc cốt tủy kinh mạch để ban phát thiên phú đột phá vĩnh viễn.");
                    choices = new System.Collections.Generic.List<string> {
                        Mewtations.Core.MewtationsLoc.Translate("exp_evt3_opt0", "Dâng hiến Linh Thực (Tiêu hao Thức ăn)"),
                        Mewtations.Core.MewtationsLoc.Translate("exp_evt3_opt1", "Huyết Thệ Cốt Tủy (Đột phá - khóa ô Thức ăn)"),
                        Mewtations.Core.MewtationsLoc.Translate("exp_evt3_opt2", "Từ bỏ tham niệm (Thanh tẩy linh hồn)")
                    };
                    onChoice = (idx) =>
                    {
                        if (idx == 0)
                        {
                            if (ConsumeItemFromOrdering("food")) {
                                var lucky = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                var talent = (HeavenlyTalent)UnityEngine.Random.Range(1, Enum.GetValues(typeof(HeavenlyTalent)).Length);
                                lucky.AddTrait(talent);
                                lucky.AddMemoir(MemoirType.Breakthrough, HeavenlyTalent.GetDisplayName(talent), Mewtations.Core.MewtationsLoc.Translate("exp_evt3_m1", "Dâng hiến thức ăn ma huyệt nhận thiên phú"));
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt3_t1", "Tế Phẩm Chấp Thuận!"), string.Format(Mewtations.Core.MewtationsLoc.Translate("exp_evt3_d1", "Thần linh hoan hỷ! Nhận lấy Linh thực hiến tế, ma lực bùng phát tẩy tủy vĩnh viễn cho <b>{0}</b>, thức tỉnh thiên phú vĩnh cửu: <b><color=#00ffcc>{1}</color></b>!"), lucky.Name, HeavenlyTalent.GetDisplayName(talent)));
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsFoodSlotLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, Mewtations.Core.MewtationsLoc.Translate("exp_evt3_t2", "Nguyền Rủa Đói Khát"), Mewtations.Core.MewtationsLoc.Translate("exp_evt3_d2", "Lừa dối ma huyệt bị phạt đói, khóa ô Thức ăn"));
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt3_t2", "Thần Linh Phẫn Nộ!"), string.Format(Mewtations.Core.MewtationsLoc.Translate("exp_evt3_d2", "Balo không có Thức ăn hiến tế! Thần linh nổi giận giáng nguyền rủa Đói Khát đói nghèo lên toàn đội. Chú mèo <b>{0}</b> bị <b><color=red>KHÓA ô Thức ăn (Food/Ultimate Slot)</color></b> vĩnh viễn!"), victim.Name));
                            }
                        }
                        else if (idx == 1)
                        {
                            var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                            victim.BreakthroughLevel++;
                            victim.BaseCombatStats.MaxHealth += 10;
                            victim.BaseCombatStats.Speed += 15;
                            victim.IsFoodSlotLocked = true;
                            victim.AddMemoir(MemoirType.Breakthrough, Mewtations.Core.MewtationsLoc.Translate("exp_evt3_t3", "Huyết Thệ Nghịch Thiên"), Mewtations.Core.MewtationsLoc.Translate("exp_evt3_d3", "Đột phá cưỡng chế, vĩnh viễn khóa ô Thức ăn"));
                            DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt3_t3", "Huyết Thệ Thành Công!"), string.Format(Mewtations.Core.MewtationsLoc.Translate("exp_evt3_d3", "Tế lễ đẫm máu nghịch thiên! Chú mèo <b>{0}</b> hiến tế kinh mạch tiêu hóa của bản thân. Đột phá cảnh giới vượt bậc vĩnh viễn (+10 Max HP, +15 Speed) nhưng <b><color=red>ô Thức ăn (Food/Ultimate Slot) vĩnh viễn bị KHÓA</color></b>!"), victim.Name));
                        }
                        else
                        {
                            RunState.CorruptionLevel = UnityEngine.Mathf.Max(0, RunState.CorruptionLevel - 25);
                            DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt3_t4", "Tâm Hồn Thanh Tịnh"), Mewtations.Core.MewtationsLoc.Translate("exp_evt3_d4", "Toàn đội từ bỏ ý chí tham lam, ma chướng linh mạch được tẩy rửa gột sạch (-25 Corruption)!"));
                        }
                    };
                }
                else if (eventRoll == 4)
                {
                    title = Mewtations.Core.MewtationsLoc.Translate("exp_evt4_title", "⚠️ KIỂM TRA GIẤY PHÉP ĐỘT XUẤT");
                    text = Mewtations.Core.MewtationsLoc.Translate("exp_evt4_desc", "Một toán Lực Lượng Hành Pháp bọc giáp sắt bất ngờ chặn đội mèo của bạn lại tại chốt rẽ. Đèn linh áp quét thẳng qua chiếc balo khả nghi của bạn.\n\n\"Dừng lại! Kiểm tra giấy phép thông hành và quota khai thác linh thạch. Trình diện ngay lập tức!\"");
                    choices = new System.Collections.Generic.List<string> {
                        Mewtations.Core.MewtationsLoc.Translate("exp_evt4_opt0", "Trình thẻ phép lậu (Đút lót Vàng)"),
                        Mewtations.Core.MewtationsLoc.Translate("exp_evt4_opt1", "Chấp nhận tịch thu hàng lậu"),
                        Mewtations.Core.MewtationsLoc.Translate("exp_evt4_opt2", "Chạy trốn lập tức (Yêu cầu Tốc độ > 120)")
                    };
                    onChoice = (idx) =>
                    {
                        if (idx == 0)
                        {
                            if (ConsumeItemFromOrdering("resource_gold")) {
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt4_t1", "Hối Lộ Thành Công"), Mewtations.Core.MewtationsLoc.Translate("exp_evt4_d1", "Lực Lượng Hành Pháp liếc nhìn đồng Vàng, lờ đi đống quặng bất hợp pháp trong balo: \"Giấy phép hợp lệ. Đi mau!\""));
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.HealthPoints = UnityEngine.Mathf.Max(1, victim.HealthPoints - 10);
                                RunState.AddCorruption(20);
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt4_t2", "Không Có Tiền Đút Lót"), string.Format(Mewtations.Core.MewtationsLoc.Translate("exp_evt4_d2", "Bị phát hiện dùng giấy thông hành giả! Chúng lập tức dùng roi điện đánh thương nặng <b>{0}</b> (-10 HP) và nâng mức tà lực ma đạo (+20 Corruption)!"), victim.Name));
                            }
                        }
                        else if (idx == 1)
                        {
                            var inventory = Context.Ordering.MyGameCard.GetInventory();
                            if (inventory.Count > 0)
                            {
                                var removed = inventory[UnityEngine.Random.Range(0, inventory.Count)];
                                inventory.Remove(removed);
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt4_t3", "Hàng Lậu Bị Tịch Thu"), string.Format(Mewtations.Core.MewtationsLoc.Translate("exp_evt4_d3", "Để giữ tính mạng, toàn đội giao nộp <b>{0}</b>. Chúng hừ lạnh thu giữ rồi thả đi."), removed.CardId.Replace("item_", "").Replace("resource_", "")));
                            }
                            else
                            {
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt4_t4", "Balo Trống Rỗng"), Mewtations.Core.MewtationsLoc.Translate("exp_evt4_d4", "Chúng khám xét balo nhưng không thấy gì khả nghi. Không có gì để tịch thu, chúng đành đá đít xua đuổi bạn đi."));
                            }
                        }
                        else
                        {
                            int avgSpeed = (int)ActiveCats.Average(c => c.Speed);
                            if (avgSpeed > 120)
                            {
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt4_t5", "Chạy Thoát Thành Công"), Mewtations.Core.MewtationsLoc.Translate("exp_evt4_d5", "Thần tốc! Toàn đội mèo phóng đi trong chớp mắt, cắt đuôi toán tuần tra Dogma một cách hoàn hảo!"));
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.HealthPoints = UnityEngine.Mathf.Max(1, victim.HealthPoints - 12);
                                RunState.AddCorruption(20);
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt4_t6", "Chạy Trốn Thất Bại"), string.Format(Mewtations.Core.MewtationsLoc.Translate("exp_evt4_d6", "Tốc độ quá chậm! Toán tuần tra vây bắt và đánh trọng thương <b>{0}</b> (-12 HP), tà pháp giam giữ gia tăng (+20 Corruption)!"), victim.Name));
                            }
                        }
                    };
                }
                else if (eventRoll == 5)
                {
                    title = Mewtations.Core.MewtationsLoc.Translate("exp_evt5_title", "🐱 DÂN NGHÈO CẦU XIN LINH KHÍ");
                    text = Mewtations.Core.MewtationsLoc.Translate("exp_evt5_desc", "Một chú mèo tiều tụy gầy trơ xương, cơ thể dị biến nặng nề đang quỳ bên đống phế thải công nghiệp, run rẩy van xin:\n\n\"Làm ơn... tôi chỉ xin một mẩu Linh Thạch vụn để duy trì linh căn đang héo úa của con tôi... Bọn Dogma đã siết hết quota của khu này rồi...\"");
                    choices = new System.Collections.Generic.List<string> {
                        Mewtations.Core.MewtationsLoc.Translate("exp_evt5_opt0", "Bố thí Quặng Linh Thạch thô"),
                        Mewtations.Core.MewtationsLoc.Translate("exp_evt5_opt1", "Từ chối đi thẳng")
                    };
                    onChoice = (idx) =>
                    {
                        if (idx == 0)
                        {
                            if (ConsumeItemFromOrdering("ore")) {
                                RunState.CorruptionLevel = UnityEngine.Mathf.Max(0, RunState.CorruptionLevel - 30);
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt5_t1", "Tích Đức Giải Nghiệp"), Mewtations.Core.MewtationsLoc.Translate("exp_evt5_d1", "Chú mèo mừng rỡ ôm lấy mảnh quặng khóc nấc lên. Linh hồn toàn đội được thanh thản, gột rửa bớt tà khí ma kiếp (-30 Corruption)!"));
                            }
                            else
                            {
                                DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt5_t2", "Không Có Linh Thạch"), Mewtations.Core.MewtationsLoc.Translate("exp_evt5_d2", "Bạn rất muốn giúp nhưng balo viễn chinh không có bất kỳ mảnh Quặng Linh Thạch nào. Chú mèo nghèo thất vọng quay đi."));
                            }
                        }
                        else
                        {
                            RunState.GreedLevel = UnityEngine.Mathf.Min(100, RunState.GreedLevel + 15);
                            DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt5_t3", "Quay Lưng Bỏ Đi"), Mewtations.Core.MewtationsLoc.Translate("exp_evt5_d3", "Bạn lạnh lùng bước tiếp. Tiếng khóc than uất nghẹn của dân nghèo bám riết đạo tâm của bạn (+15 Greed)!"));
                        }
                    };
                }
                else if (eventRoll == 6)
                {
                    title = Mewtations.Core.MewtationsLoc.Translate("exp_evt6_title", "⚖️ THƯƠNG NHÂN CHỢ ĐEN");
                    int maxBreakthrough = ActiveCats.Count > 0 ? ActiveCats.Max(c => c.BreakthroughLevel) : 0;
                    if (maxBreakthrough >= 2)
                    {
                        text = Mewtations.Core.MewtationsLoc.Translate("exp_evt6_desc1", "Một gã mèo trùm mũ kín mít hé mở chiếc hòm linh bảo giấu kín. Hắn thì thầm đầy tôn kính:\n\n\"Nhìn ngài có vẻ là một Hộ Pháp cao cấp... Tiểu nhân có vài món bảo vật giấu riêng, hoàn toàn không ghi trong sổ sách kiểm kê của Giáo Điều... Ngài có muốn xem qua?\"");
                        choices = new System.Collections.Generic.List<string> {
                            Mewtations.Core.MewtationsLoc.Translate("exp_evt6_opt0", "Mua Hóa Thần Thạch (Tiêu hao Vàng)"),
                            Mewtations.Core.MewtationsLoc.Translate("exp_evt6_opt1", "Mua Linh Dược Đột Phá (Tiêu hao Vàng)"),
                            Mewtations.Core.MewtationsLoc.Translate("exp_evt6_opt2", "Rút lui")
                        };
                        onChoice = (idx) =>
                        {
                            if (idx == 0 || idx == 1)
                            {
                                if (ConsumeItemFromOrdering("resource_gold")) {
                                    string itemSpawn = idx == 0 ? "item_revive_pill" : "item_breakthrough_pill";
                                    RunState.PendingRewards.Add(itemSpawn);
                                    DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt6_t1", "Giao Dịch Thành Công"), string.Format(Mewtations.Core.MewtationsLoc.Translate("exp_evt6_d1", "Bảo vật bất hợp pháp <b>{0}</b> đã được giao tay bí mật. Thương nhân đóng rương và lủi mất."), itemSpawn.Replace("item_", "")));
                                }
                                else
                                {
                                    DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt6_t2", "Không Đủ Tiền"), Mewtations.Core.MewtationsLoc.Translate("exp_evt6_d2", "\"Ngài đang đùa với tôi à?\" Hắn gắt gỏng khi thấy bạn không có đủ Vàng rồi biến mất vào bóng tối."));
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
                        text = Mewtations.Core.MewtationsLoc.Translate("exp_evt6_desc2", "Một kẻ buôn lậu lén lút tiếp cận đội ngũ. Hắn thì thầm mời mọc:\n\n\"Ê, muốn mua hàng xách tay không? Quặng sắt chất lượng cao, giá rẻ mạt, chỉ bằng một góc ngoài chợ!\"");
                        choices = new System.Collections.Generic.List<string> {
                            Mewtations.Core.MewtationsLoc.Translate("exp_evt6_opt3", "Mua Quặng Sắt Lậu (Tiêu hao Vàng)"),
                            Mewtations.Core.MewtationsLoc.Translate("exp_evt6_opt2", "Bỏ qua")
                        };
                        onChoice = (idx) =>
                        {
                            if (idx == 0)
                            {
                                if (ConsumeItemFromOrdering("resource_gold")) {
                                    RunState.PendingRewards.Add("item_iron_ore");
                                    DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt6_t1", "Giao Dịch Thành Công"), Mewtations.Core.MewtationsLoc.Translate("exp_evt6_d3", "Giao dịch nhanh gọn. Lấy được Quặng sắt lậu."));
                                }
                                else
                                {
                                    DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_evt6_t2", "Không Đủ Tiền"), Mewtations.Core.MewtationsLoc.Translate("exp_evt6_d2", "\"Tưởng thế nào!\" Hắn bĩu môi rồi nhanh chóng biến mất."));
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
                title = Mewtations.Core.MewtationsLoc.Translate("exp_camp1_title", "⛲ CAMP HEALER: BỒN NƯỚC THẦN");
                text = Mewtations.Core.MewtationsLoc.Translate("exp_camp1_desc", "Một dòng suối linh khí dồi dào có thể chữa lành vết thương.");
                choices = new System.Collections.Generic.List<string> {
                    Mewtations.Core.MewtationsLoc.Translate("exp_camp1_opt0", "Ngâm mình trị thương"),
                    Mewtations.Core.MewtationsLoc.Translate("exp_camp1_opt1", "Tẩy tủy giải hiệu ứng"),
                    Mewtations.Core.MewtationsLoc.Translate("exp_camp1_opt2", "Bỏ qua")
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
                                    cat.HealthPoints = UnityEngine.Mathf.Min(cat.ProcessedCombatStats.MaxHealth, cat.HealthPoints + healPerCat);
                                }
                            }
                            DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_camp1_t1", "Hồi phục"), string.Format(Mewtations.Core.MewtationsLoc.Translate("exp_camp1_d1", "Đội ngũ uống nước thần và chia nhau hồi lại {0} HP!"), pool));
                        }
                        else 
                        {
                            DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_camp1_t2", "Khỏe mạnh"), Mewtations.Core.MewtationsLoc.Translate("exp_camp1_d2", "Tất cả các thành viên đều đang khỏe mạnh."));
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
                        DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_camp1_t3", "Tẩy tủy"), Mewtations.Core.MewtationsLoc.Translate("exp_camp1_d3", "Toàn đội được thanh tẩy, giải trừ trạng thái Kiệt Sức và Tê Liệt!"));
                    }
                    else
                    {
                        CompleteNodeResolution();
                    }
                };
            }
            else if (type == NodeType.CampMerchant)
            {
                title = Mewtations.Core.MewtationsLoc.Translate("exp_merch2_title", "🤖 CAMP MERCHANT");
                text = Mewtations.Core.MewtationsLoc.Translate("exp_merch2_desc", "Một cỗ máy giao dịch cổ đại bị bỏ hoang.\n\nĐổi 1 Food lấy Cổ Vật Tự Động Nhặt\nĐổi 1 Gold lấy Cổ Vật Tự Động Thu Hoạch");
                choices = new System.Collections.Generic.List<string> {
                    Mewtations.Core.MewtationsLoc.Translate("exp_merch2_opt0", "Đổi Thức ăn lấy Cổ Vật Nhặt"),
                    Mewtations.Core.MewtationsLoc.Translate("exp_merch2_opt1", "Đổi Vàng lấy Cổ Vật Thu Hoạch")
                };
                onChoice = (idx) =>
                {
                    if (idx == 0)
                    {
                        if (ConsumeItemFromOrdering("food")) {
                            RunState.PendingRewards.Add("item_ancient_relic_auto_collect");
                            DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_merch2_t1", "Giao dịch thành công"), Mewtations.Core.MewtationsLoc.Translate("exp_merch2_d1", "Đổi 1 Food lấy Cổ Vật Tự Động Nhặt!"));
                        }
                        else
                        {
                            DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_merch2_t3", "Không đủ tiền"), Mewtations.Core.MewtationsLoc.Translate("exp_merch2_d3", "Thương nhân hừ lạnh vì bạn không có đủ vật phẩm trao đổi!"));
                        }
                    }
                    else if (idx == 1)
                    {
                        if (ConsumeItemFromOrdering("resource_gold")) {
                            RunState.PendingRewards.Add("item_ancient_relic_auto_farm");
                            DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_merch2_t2", "Giao dịch thành công"), Mewtations.Core.MewtationsLoc.Translate("exp_merch2_d2", "Đổi 1 Gold lấy Cổ Vật Tự Động Thu Hoạch!"));
                        }
                        else
                        {
                            DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_merch2_t3", "Không đủ tiền"), Mewtations.Core.MewtationsLoc.Translate("exp_merch2_d3", "Thương nhân hừ lạnh vì bạn không có đủ vật phẩm trao đổi!"));
                        }
                    }
                    CompleteNodeResolution();
                };
            }
            else if (type == NodeType.CampBlacksmith)
            {
                title = Mewtations.Core.MewtationsLoc.Translate("exp_smith_title", "⚒️ CAMP BLACKSMITH");
                text = Mewtations.Core.MewtationsLoc.Translate("exp_smith_desc", "Lò rèn của một thợ rèn lang thang.\n\nTốn Quặng Sắt để cường hóa tạm thời sức mạnh cho cả đội (+5 HP Max, +10 Stamina Max)?");
                choices = new System.Collections.Generic.List<string> {
                    Mewtations.Core.MewtationsLoc.Translate("exp_smith_opt0", "Rèn Trang Bị (Tốn Sắt)"),
                    Mewtations.Core.MewtationsLoc.Translate("exp_smith_opt1", "Bỏ qua")
                };
                onChoice = (idx) =>
                {
                    if (idx == 0)
                    {
                        if (ConsumeItemFromOrdering("ore")) {
                            foreach(var cat in ActiveCats) { if(cat != null) { cat.HealthPoints += 5; cat.Stamina += 10; } }
                            DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_smith_t1", "Cường Hóa"), Mewtations.Core.MewtationsLoc.Translate("exp_smith_d1", "Cả đội được nâng cấp áo giáp và vũ khí tạm thời!"));
                        }
                        else
                        {
                            DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_smith_t2", "Thiếu Quặng Sắt"), Mewtations.Core.MewtationsLoc.Translate("exp_smith_d2", "Thợ rèn lắc đầu, bạn không có đủ quặng sắt (Ore)."));
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
                title = Mewtations.Core.MewtationsLoc.Translate("exp_reward_title", "🎁 REWARD NODE");
                text = Mewtations.Core.MewtationsLoc.Translate("exp_reward_desc", "Trước mặt bạn là một rương kho báu khổng lồ bị bỏ hoang. Ai đó đã gom rất nhiều vật phẩm vào đây.");
                choices = new System.Collections.Generic.List<string> {
                    Mewtations.Core.MewtationsLoc.Translate("exp_reward_opt0", "Mở rương!")
                };
                onChoice = (idx) =>
                {
                    if (ExpeditionRewardUI.Instance != null)
                    {
                        int rewardCount = UnityEngine.Random.Range(2, 6);
                        System.Collections.Generic.List<string> rewards = new System.Collections.Generic.List<string>();
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
                title = Mewtations.Core.MewtationsLoc.Translate("exp_lore_title", "Bích Họa Cổ Xưa");
                text = Mewtations.Core.MewtationsLoc.Translate("exp_lore_desc", "Trải rộng trên bức tường đá rêu phong là những bích họa mô tả về thời kỳ 'Thần Miêu Sáng Thế' và cuộc viễn chinh cổ đại.\n\nLinh hồn của toàn đội được gột rửa, giúp gia tăng Speed tạm thời!");
                choices = new System.Collections.Generic.List<string> { Mewtations.Core.MewtationsLoc.Translate("exp_lore_opt0", "Tiếp thu tinh hoa") };
                onChoice = (idx) =>
                {
                    foreach (var cat in ActiveCats)
                    {
                        cat.Speed += 10;
                    }
                    CompleteNodeResolution();
                };
            }
            else // Ruins
            {
                title = Mewtations.Core.MewtationsLoc.Translate("exp_ruins_title", "Phế Tích Hoang Phế");
                text = Mewtations.Core.MewtationsLoc.Translate("exp_ruins_desc", "Đội hình mèo tiến vào một phế tích cung điện đổ nát. Ở giữa có một lò đan dược cũ kỹ vẫn đang cháy âm ỉ.\nBạn có muốn lục lọi không?");
                choices = new System.Collections.Generic.List<string> { Mewtations.Core.MewtationsLoc.Translate("exp_ruins_opt0", "Mở lò đan dược"), Mewtations.Core.MewtationsLoc.Translate("exp_ruins_opt1", "Rút lui") };
                onChoice = (idx) =>
                {
                    if (idx == 0)
                    {
                        if (UnityEngine.Random.value < 0.5f)
                        {
                            RunState.PendingRewards.Add("item_revive_pill");
                            DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_ruins_t1", "Luyện Đan Kỳ Tích!"), Mewtations.Core.MewtationsLoc.Translate("exp_ruins_d1", "Tuyệt vời! Bên trong lò đan vẫn còn lưu giữ một viên Linh Đan Hồi Sinh cực kỳ quý hiếm!"));
                        }
                        else
                        {
                            DialogueResult(Mewtations.Core.MewtationsLoc.Translate("exp_ruins_t2", "Khói đen mù mịt"), Mewtations.Core.MewtationsLoc.Translate("exp_ruins_d2", "Lò đan nổ tung! Khói đen kịt phả thẳng vào mặt khiến toàn đội bám đầy tro bụi (Không có tổn thất thực tế)."));
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

		private void DialogueResult(string title, string text)
        {
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { Mewtations.Core.MewtationsLoc.Translate("exp_continue", "Tiáº¿p tá»¥c") }, (idx) =>
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
                    return; //
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

            //
            ApplyRelicAutomationProgress();

            //
            UpdateConnections();

            //
            if (ActiveNode != null && ActiveNode.Type == NodeType.Boss)
            {
                Debug.Log(Mewtations.Core.MewtationsLoc.Translate("exp_log_boss_defeat", "[Expedition] Đã đánh bại Boss! Tự động trở về căn cứ."));
                ReturnToBase(isDefeat: false);
            }
            else
            {
                //
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
            Debug.Log(Mewtations.Core.MewtationsLoc.Translate("exp_log_relic_apply", "[Expedition] Kích hoạt hiệu ứng Thánh Vật tự động."));

            foreach (var gc in WorldManager.instance.AllCards)
            {
                if (gc != null && !gc.Destroyed && gc.CardData != null && gc.TimerRunning)
                {
                    string cid = gc.CardData.Id.ToLower();
                    
                    if (relic == "item_ancient_relic_smelt" && (cid.Contains("smelter") || cid.Contains("furnace")))
                    {
                        gc.CurrentTimerTime += 15f; //
                        Debug.Log(Mewtations.Core.MewtationsLoc.Translate("exp_log_relic_smelt", "[Expedition] Thánh Vật Lò Luyện: Tăng tốc tiến trình nung chảy +15s."));
                    }
                    else if (relic == "item_ancient_relic_wood" && (cid.Contains("sawmill") || cid.Contains("mill")))
                    {
                        gc.CurrentTimerTime += 15f; //
                        Debug.Log(Mewtations.Core.MewtationsLoc.Translate("exp_log_relic_wood", "[Expedition] Thánh Vật Chặt Cây: Tăng tốc tiến trình xưởng mộc +15s."));
                    }
                    else if (relic == "item_ancient_relic_booster")
                    {
                        gc.CurrentTimerTime += 5f; //
                        Debug.Log(Mewtations.Core.MewtationsLoc.Translate("exp_log_relic_boost", "[Expedition] Thánh Vật Tăng Tốc: Tăng tốc tiến trình chung +5s."));
                    }
                }
            }
        }

        private void UpdateConnections()
        {
            if (ActiveNode == null) return;

            //
            foreach (var n in MapNodes)
            {
                if (n.State == NodeState.Available)
                {
                    n.State = NodeState.Locked;
                }
            }

            //
            foreach (int connectedId in ActiveNode.OutgoingConnections)
            {
                var targetNode = MapNodes.Find(n => n.Id == connectedId);
                if (targetNode != null && targetNode.State == NodeState.Locked)
                {
                    targetNode.State = NodeState.Available;
                }
            }

            //
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
                //
                bool visitedSpecial = MapNodes != null && MapNodes.Any(n => n.State == NodeState.Visited && n.Type == NodeType.SpecialMap);
                if (!visitedSpecial)
                {
                    Mewtations.Legacy.Stacklands.SaveManager.instance.CurrentSave.ExpeditionSpecialMapPityCounter++;
                }
            }
            IsExpeditionActive = false;
            State = ExpeditionState.Idle;
            ActiveCats.Clear();

            //
            if (ExpeditionMapUI.Instance != null) ExpeditionMapUI.Instance.HideWindow();
            if (CombatOverlayUI.Instance != null) CombatOverlayUI.Instance.HideWindow();
            if (Mewtations.Dialogue.DialogueSystem.Instance != null) Mewtations.Dialogue.DialogueSystem.Instance.HideWindow();

            //
            WorldManager.WorldSimulationPaused = false;

            if (Context != null && Context.Ordering != null && Context.Ordering.MyGameCard != null)
            {
                var gatewayCard = Context.Ordering.MyGameCard.Parent;
                Vector3 spawnPos = (gatewayCard != null ? gatewayCard.transform.position : Context.Ordering.MyGameCard.transform.position) + Vector3.back * 1.5f;

                //
                Context.Ordering.MyGameCard.RemoveFromStack();
                Context.Ordering.MyGameCard.transform.position = spawnPos + Vector3.right * 1.5f;
                WorldManager.instance.SendToBoard(Context.Ordering.MyGameCard, WorldManager.instance.CurrentBoard, Context.Ordering.MyGameCard.transform.position);

                //
                foreach (var cat in ActiveCats)
                {
                    if (cat != null)
                    {
                        cat.ClearMutations(); //
                        
                        //
                        if (RuntimeCatStates.TryGetValue(cat.UniqueId, out var state))
                        {
                            cat.HealthPoints = state.HP;
                            cat.Stamina = state.Stamina;
                            cat.IsExhausted = state.IsExhausted;
                            cat.IsParalyzed = state.IsParalyzed;
                            cat.ExhaustionLevel = state.ExhaustionLevel;
                        }

                        //
                        int staminaDebt = 20; //
                        if (RunState != null) {
                            staminaDebt += (RunState.CurrentLayer * 5); //
                        }
                        cat.Stamina = UnityEngine.Mathf.Max(0, cat.Stamina - staminaDebt);
                        
                        //
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
                    //
                    MutationPersistenceSystem.ProcessRunVictoryTraits(ActiveCats);

                    //
                    

                    //
                    if (ActiveNode != null && ActiveNode.Type == NodeType.Boss)
                    {
                        var summoning = new CatSummoningSystem(WorldManager.instance);
                        summoning.SummonCat(spawnPos, highestBreakthroughLevel: 2); //
                        Debug.Log(Mewtations.Core.MewtationsLoc.Translate("exp_log_boss_reward", "[Expedition] Phần thưởng Boss: Triệu hồi một Thần Miêu!"));
                    }
                }
                else
                {
                    //
                    if (Context != null && Context.Ordering != null && Context.Ordering.MyGameCard != null && Context.Ordering.MyGameCard.InventoryContainer != null) { int insuredSlots = Context.Ordering.InsuredSlots;
                        if (isManualRetreat)
                        {
                            //
                            if (RunState != null)
                            {
                                RunState.GreedLevel = Mathf.Min(100, RunState.GreedLevel + 15);
                            }
                            ExpeditionExtractionSystem.ApplyManualRetreatPenalty(Context.Ordering.MyGameCard.InventoryContainer, insuredSlots);
                            Debug.Log(Mewtations.Core.MewtationsLoc.Translate("exp_log_retreat", "[Expedition] Rút lui chiến thuật: Bị phạt một phần tài nguyên."));
                        }
                        else
                        {
                            float rate = ExpeditionExtractionSystem.CalculateLootRetentionRate(RunState, Context.Ordering.MyGameCard.InventoryContainer, Context.Ordering.StorageCapacity);
                              ExpeditionExtractionSystem.ApplyAbandonPenalty(Context.Ordering.MyGameCard.InventoryContainer, rate, insuredSlots);
                            Debug.Log(Mewtations.Core.MewtationsLoc.Translate("exp_log_abandon", "[Expedition] Đội bị tiêu diệt: Mất phần lớn tài nguyên."));
                        }
                        
                    }
                }

                //
                if (BackpackCardSource != null && BackpackCardSource.MyGameCard != null)
                {
                    BackpackCardSource.MyGameCard.transform.position = spawnPos + Vector3.right * 1.0f;
                    BackpackCardSource.MyGameCard.gameObject.SetActive(true);
                }

                //
                if (RelicCardSource != null && RelicCardSource.MyGameCard != null)
                {
                    RelicCardSource.MyGameCard.transform.position = spawnPos + Vector3.left * 1.0f;
                    RelicCardSource.MyGameCard.gameObject.SetActive(true);
                }
                RelicCardSource = null;
                RunState.EquippedRelicId = "";

                //
                if (PortalCardSource.CardData.Id == "strange_portal")
                {
                    PortalCardSource.DestroyCard(false, true);
                }
            }

            Debug.Log(Mewtations.Core.MewtationsLoc.Translate("exp_log_return_base", "[Expedition] Chuyến Viễn Chinh kết thúc. Trở về căn cứ."));
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

            //
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


            //
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

            //
            if (State == ExpeditionState.MapNavigation && ExpeditionMapUI.Instance != null)
            {
                ExpeditionMapUI.Instance.ShowWindow();
            }
        }
    }
}




















