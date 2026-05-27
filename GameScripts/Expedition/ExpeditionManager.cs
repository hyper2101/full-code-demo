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
        public CardData RelicCardSource = null; // Cổ vật đang mang theo

        private void Awake()
        {
            Instance = this;
        }

                public void StartExpedition(GameCard portalCard, List<CatCardData> cats, CardData backpackCard, CardData relicCard = null)
        {
            if (IsExpeditionActive) return;

            // Phase 2: Check for Exhausted or Recovering cats
            foreach (var cat in cats)
            {
                if (cat.CurrentLaborState == Mewtations.Systems.Labor.LaborReadinessState.Exhausted || 
                    cat.CurrentLaborState == Mewtations.Systems.Labor.LaborReadinessState.Recovering)
                {
                    WorldManager.instance.CreateFloatingText(portalCard, false, 0, "?? Khng th? k?i hnh! M?t s? M?o dang ki?t s?c ho?c h?i ph?c.", "", false, 0, 2f, true);
                    return;
                }
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

            CurrentBackpack = new Backpack(capacity);

            CurrentMapSeed = seed;
            MapNodes = ExpeditionMapGenerator.GenerateMap(seed, maxLayers: 6, maxNodesPerLayer: 3);
            ActiveNode = null;

            // Initialize risk stats using non-static base appeasement
            ExpeditionRiskSystem.InitializeRunStats(RunState);

            // Freeze main board by setting TimeScale to 0
            Time.timeScale = 0f; 

            // Show Expedition Map UI Overlay
            if (ExpeditionMapUI.Instance != null)
            {
                ExpeditionMapUI.Instance.ShowWindow();
            }
            else
            {
                Debug.LogError("[ExpeditionManager] ExpeditionMapUI.Instance is null!");
            }

            Debug.Log($"[Expedition] Bắt đầu viễn chinh với {cats.Count} mèo. Balo dung tích: {capacity}.");
        }

        public void EnterNode(ExpeditionNode node)
        {
            if (node.State != NodeState.Available) return;

            ActiveNode = node;
            node.State = NodeState.Visited;
            State = ExpeditionState.InEncounter;
            RunState.CurrentLayer = node.Layer;

            // Travel food consumption has been disabled per user request

            // Route Themes initial impacts
            if (node.Theme == RouteTheme.TaDao)
            {
                RunState.AddCorruption(25);
                Debug.Log("[Expedition] Tà Đạo áp lực! Tăng +25 Corruption khi bước vào khu vực tà khí.");
            }
            else if (node.Theme == RouteTheme.ThamLam)
            {
                RunState.AddGreed(10);
                Debug.Log("[Expedition] Tham Lam ý niệm! Tăng +10 Greed khi bước vào khu vực trù phú.");
            }

            // Hide Map UI while resolving node activities
            if (ExpeditionMapUI.Instance != null)
            {
                ExpeditionMapUI.Instance.HideWindow();
            }

            Debug.Log($"[Expedition] Tiến vào node {node.Id} ({node.Type}) ở Tầng {node.Layer}. Lộ trình: {node.Theme}. Biome: {node.Biome}.");

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
                    Debug.Log("[Expedition] Người chơi bỏ cuộc! Hủy bỏ viễn chinh và quay về base.");
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

            // Nếu là Boss tiến độ, thưởng thêm Cổ Vật tự động hóa ngẫu nhiên vào balo
            if (isBoss)
            {
                string[] relics = { "item_ancient_relic_auto_farm", "item_ancient_relic_auto_collect", "item_ancient_relic_auto_heal" };
                string chosenRelic = relics[UnityEngine.Random.Range(0, relics.Length)];
                rolled.Add(chosenRelic);
                CurrentBackpack.AddItem(chosenRelic);
                Debug.Log($"[Expedition] Boss chiến thắng! Nhận thêm Cổ Vật chí tôn: {chosenRelic}");
            }

            string lootMsg = string.Join(", ", rolled.Select(id => id.Replace("resource_", "").Replace("item_", "")));
            Debug.Log($"[Expedition] Thu hoạch chiến lợi phẩm: {lootMsg}");
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
                : "Không có chỗ chứa trong Balo!";

            // Trigger dialogue overlay to display the resource gathering result
            string title = "Thu thập Tài Nguyên";
            string text = $"Đội của bạn đã tìm thấy một bãi tài nguyên trù phú!\n\nNhận được: {resMsg}";
            
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { "Đồng ý" }, (choiceIdx) =>
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
                    // Lôi kiếp thử thách
                    title = "⚡ KIẾP LÔI THỬ THÁCH";
                    text = "Đội ngũ mèo đi tới một đỉnh núi hoang vắng, mây đen cuộn trào nghẹt thở. Từng tia lôi điện khổng lồ giáng xuống như Lôi Kiếp độ kiếp!\n\nLôi linh lực cuồng bạo này ẩn chứa cơ duyên lớn nhưng cực kỳ nguy hiểm. Bạn muốn làm gì?";
                    choices = new List<string> {
                        "Hấp thụ Lôi Kiếp (Yêu cầu mèo hệ Sét hoặc Tốc độ cao)",
                        "Đỡ đòn hộ đồng đội (Yêu cầu Tank bảo vệ)",
                        "Trận pháp phòng thủ (Lách qua an toàn)"
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
                                luckyCat.AddMemoir(MemoirType.Breakthrough, "Lôi Kiếp Tẩy Tủy", "Hấp thụ lôi điện đột phá võ đạo (+25 Speed)");
                                DialogueResult("Lôi quang rực rỡ!", $"Tuyệt đỉnh! Nhờ sự nhạy bén cực độ (hoặc linh căn hệ Sét), chú mèo <b>{luckyCat.Name}</b> đã hấp thụ trọn vẹn Lôi Điện Phạt, vĩnh viễn gia tăng <b>+25 Thần Tốc</b>!");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsUltimateLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, "Tẩu Hỏa Nhập Ma", "Trúng lôi điện bạo phát bế tắc linh mạch, khóa kỹ năng Nộ");
                                foreach (var cat in ActiveCats)
                                {
                                    cat.HealthPoints = Mathf.Max(1, cat.HealthPoints - 10);
                                }
                                DialogueResult("Lôi Phạt Oanh Tạc!", $"Tốc độ quá chậm! Lôi điện cuồng bạo thâm nhập tàn phá kinh mạch. Chú mèo <b>{victim.Name}</b> bị <b><color=red>TẨU HỎA NHẬP MA (KHÓA KỸ NĂNG NỘ)</color></b> vĩnh viễn, toàn đội thương nặng (-10 HP)!");
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
                                tank.AddMemoir(MemoirType.Breakthrough, "Hộ Thể Lôi Kiếp", "Đỡ lôi kiếp cho đồng đội, khóa ô Thiên Phú");
                                DialogueResult("Hộ Thể Tuyệt Vời!", $"Anh hùng! Chú Tank <b>{tank.Name}</b> đứng ra đỡ lôi phạt cho toàn đội. Thần thể được cường hóa (+10 Max HP) nhưng bùa chú bị phá hủy hoàn toàn, <b><color=red>ô Thiên Phú (Passive Slots) vĩnh viễn bị KHÓA</color></b>!");
                            }
                            else
                            {
                                foreach (var cat in ActiveCats)
                                {
                                    cat.HealthPoints = Mathf.Max(1, cat.HealthPoints - 15);
                                }
                                DialogueResult("Hộ Vệ Thất Bại!", "Đội hình không có hộ vệ Tank chuyên nghiệp! Buộc phải dùng thân xác trần tục chống đỡ, toàn đội bị thương tổn cực nặng (-15 HP)!");
                            }
                        }
                        else
                        {
                            DialogueResult("Lách Qua An Toàn", "Toàn đội thiết lập kết giới phòng thủ thô sơ, cẩn thận đi vòng qua ngọn núi lôi kiếp an toàn.");
                        }
                    };
                }
                else if (eventRoll == 1)
                {
                    // Trạm tuần tra
                    title = "🐕 TRẠM TUẦN TRA CỦA CHÚNG CHÓ";
                    text = "Phía trước xuất hiện chốt gác kiên cố của loài Chó kiểm soát trật tự xã hội. Lính tuần tra chó bọc giáp sắt đang canh phòng nghiêm ngặt.\n\nĐội mèo của bạn mang theo balo đầy ắp tài nguyên khả nghi. Bạn muốn ứng phó thế nào?";
                    choices = new List<string> {
                        "Đút lót hối lộ (Tiêu hao 1 Vàng trong Balo - Giảm 20 Greed)",
                        "Quyết chiến đột phá (Thắng lợi đẫm máu - Tăng 25 Corruption)",
                        "Lén lút lẻn qua (Yêu cầu Tốc độ trung bình > 115)",
                        "Thuyết giảng tâm lý (Cần Thần Miêu Thiền Đạo giải thoát đạo tâm lính gác)"
                    };
                    onChoice = (idx) =>
                    {
                        if (idx == 0)
                        {
                            int goldIdx = CurrentBackpack.ContainedCardIds.IndexOf("resource_gold");
                            if (goldIdx >= 0)
                            {
                                CurrentBackpack.RemoveItemAt(goldIdx);
                                RunState.GreedLevel = Mathf.Max(0, RunState.GreedLevel - 20);
                                DialogueResult("Hối Lộ Thành Công!", "Lính tuần tra Chó nhận lấy Vàng, cười nham nhở mở cổng cho đi qua. Sức ép luật pháp xoa dịu (-20 Greed)!");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsEquipmentSlotsLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, "Tịch Thu Trang Bị", "Không có tiền đút lót, bị lính tuần tra khóa ô trang bị");
                                DialogueResult("Không Có Tiền Đút Lót!", $"Balo không có Vàng để hối lộ! Lính tuần tra nổi giận khám xét toàn đội. Chú mèo <b>{victim.Name}</b> bị tịch thu sạch vũ khí bùa chú và vĩnh viễn <b><color=red>KHÓA ô Trang Bị (Equipment Slots)</color></b>!");
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
                            DialogueResult("Huyết Chiến Đột Phá!", "Toàn đội tuốt kiếm liều chết xông vào! Tiêu diệt toàn bộ lính canh, cướp lấy Vàng và Quặng sắt trong rương chốt tuần tra, toàn đội bị thương nhẹ (-8 HP) và tăng mạnh sát nghiệp (+25 Corruption)!");
                        }
                        else if (idx == 2)
                        {
                            int avgSpeed = (int)ActiveCats.Average(c => c.Speed);
                            if (avgSpeed > 115)
                            {
                                DialogueResult("Lẻn Qua Thành Công!", "Bóng ma bóng tối! Bằng bước di chuyển thần tốc, không tiếng động, toàn đội mèo đã lướt qua trạm canh gác trót lọt mà lính chó không hề hay biết!");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsEquipmentSlotsLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, "Bắt Giữ Phong Ấn", "Lén lẻn thất bại, bị khóa ô trang bị hình phạt");
                                DialogueResult("Bị Bắt Quả Tang!", $"Tốc độ trung bình ({avgSpeed} Speed) quá chậm! Lính chó phát hiện bắt giữ toàn đội tra khảo. Chú mèo <b>{victim.Name}</b> bị tịch thu khí giới, <b><color=red>ô Trang bị vĩnh viễn bị KHÓA</color></b> làm hình phạt!");
                            }
                        }
                        else if (idx == 3)
                        {
                            var zenCat = ActiveCats.Find(c => c.Specialization == Mewtations.Cards.Cats.DaoSpecialization.ZenDao);
                            if (zenCat != null)
                            {
                                CurrentBackpack.AddItem("item_heavenly_relic");
                                RunState.CorruptionLevel = Mathf.Max(0, RunState.CorruptionLevel - 20);
                                DialogueResult("Giác Ngộ Đạo Tâm!", $"Giác ngộ thành công! Chú mèo Thiền Đạo <b>{zenCat.Name}</b> đã thuyết giảng Đạo lý Nhân sinh cực kỳ thâm sâu, khai mở đạo tâm cho lính tuần tra Chó thoát khỏi sự kiểm soát gò bó của hệ thống.\n\nChú Chó cảm kích rơi lệ, mở cổng tặng toàn đội viên <b>Chí Tôn Cổ Khí (Heavenly Relic)</b> cực hiếm và xoa dịu tà khí (-20 Corruption)!");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.HealthPoints = Mathf.Max(1, victim.HealthPoints - 10);
                                DialogueResult("Giáo Hóa Thất Bại!", $"Đội hình không có Thần Miêu Thiền Đạo để giảng giải Đạo pháp thuyết phục! Lính tuần tra Chó cho rằng bạn đang sỉ nhục trí tuệ của họ, nổi giận dùng roi điện đánh thương <b>{victim.Name}</b> (-10 HP)!");
                            }
                        }
                    };
                }
                else if (eventRoll == 2)
                {
                    // Lò đan cổ
                    title = "⚗️ LÒ LUYỆN ĐAN CỔ KÍNH";
                    text = "Đan điện phế tích u ám hiện ra trước mắt. Ở trung tâm sảnh lớn là một lò luyện cổ vẫn cháy âm ỉ lửa tím nhạt rò rỉ khí độc. Bên trong có thể ẩn chứa nghịch thiên linh đan hoặc kịch độc phế linh mạch.\n\nAi sẽ đứng ra xử lý chiếc lò đan này?";
                    choices = new List<string> {
                        "Dùng linh độc hóa giải (Cần mèo hệ Độc)",
                        "Lực lượng cưỡng chế mở lò (Rủi ro 50/50)",
                        "Đập phá lò thu phế liệu (An toàn)"
                    };
                    onChoice = (idx) =>
                    {
                        if (idx == 0)
                        {
                            var poisonCat = ActiveCats.Find(c => c.Element == CatElement.Poison);
                            if (poisonCat != null)
                            {
                                CurrentBackpack.AddItem("item_breakthrough_pill");
                                DialogueResult("Khống Chế Kịch Độc!", $"Tuyệt đỉnh! Nhờ linh căn kịch độc bẩm sinh của <b>{poisonCat.Name}</b>, chú đã trung hòa đan khí tím, mở lò lấy được viên <b>ĐỘT PHÁ LINH ĐAN</b> cực kỳ quý giá!");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsPillSlotLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, "Linh Mạch Độc Ứ", "Khí độc tàn phá kinh mạch đan dược, khóa ô Linh Đan");
                                DialogueResult("Không Có Mèo Hệ Độc!", $"Độc khí tím bùng phát cuồn cuộn do không có mèo hệ Độc khống chế! Chú mèo <b>{victim.Name}</b> hít phải độc sương tàn phá phế linh mạch, <b><color=red>ô Linh Đan (Pill Slot) vĩnh viễn bị KHÓA</color></b>!");
                            }
                        }
                        else if (idx == 1)
                        {
                            if (UnityEngine.Random.value < 0.5f)
                            {
                                CurrentBackpack.AddItem("item_breakthrough_pill");
                                DialogueResult("Vận May Nghịch Thiên!", "Vận may mỉm cười! Dù khí độc bốc lên ngùn ngụt nhưng toàn đội đã nhanh tay cướp lấy viên <b>ĐỘT PHÁ LINH ĐAN</b> thành công trước khi độc chấn nổ ra!");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsPillSlotLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, "Lò Đan Nổ Tung", "Trúng khí độc lò đan nổ, khóa ô Linh Đan");
                                DialogueResult("Lò Đan Nổ Tung!", $"Bùm! Lò luyện đan phát nổ lớn bắn ra tàn dư đan dược kịch độc. Chú mèo <b>{victim.Name}</b> trúng độc ngưng kết kinh mạch đan điền, <b><color=red>ô Linh Đan vĩnh viễn bị KHÓA</color></b>!");
                            }
                        }
                        else
                        {
                            CurrentBackpack.AddItem("item_stone");
                            CurrentBackpack.AddItem("item_iron_ore");
                            DialogueResult("Thu Hoạch Phế Liệu", "Quyết định sáng suốt! Toàn đội đập vỡ lò đan an toàn, thu về Đá vụn và Sắt phế liệu bỏ vào Balo viễn chinh.");
                        }
                    };
                }
                else if (eventRoll == 3)
                {
                    // Ma huyệt hiến tế
                    title = "🔴 MA HUYỆT KHẤN NGUYỆN";
                    text = "Một ma huyệt phát ra hồng quang rực máu ngăn giữa đường đi. Linh khí bên trong cuộn trào quyến rũ, như khơi dậy ý niệm Tham Lam tột cùng của loài mèo.\n\nThần linh đòi hỏi cúng nạp linh thực ăn uống hoặc cốt tủy kinh mạch để ban phát thiên phú đột phá vĩnh viễn.";
                    choices = new List<string> {
                        "Dâng hiến Linh Thực (Tiêu hao 1 Thức ăn trong Balo - Nhận Thiên Phú vĩnh viễn)",
                        "Huyết Thệ Cốt Tủy (Đột phá Cảnh giới - Chấp nhận khóa ô Thức ăn)",
                        "Từ bỏ tham niệm (Thanh tẩy linh hồn)"
                    };
                    onChoice = (idx) =>
                    {
                        if (idx == 0)
                        {
                            int foodIdx = CurrentBackpack.ContainedCardIds.FindIndex(id => id == "resource_food" || id.Contains("food"));
                            if (foodIdx >= 0)
                            {
                                CurrentBackpack.RemoveItemAt(foodIdx);
                                var lucky = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                string talent = UnityEngine.Random.value < 0.5f ? HeavenlyTalent.RageOvercharger : HeavenlyTalent.DivineShieldProtection;
                                lucky.AddTrait(talent);
                                lucky.CustomName = $"{HeavenlyTalent.GetDisplayName(talent)} {lucky.Name}";
                                lucky.AddMemoir(MemoirType.Breakthrough, HeavenlyTalent.GetDisplayName(talent), "Dâng hiến thức ăn ma huyệt nhận thiên phú");
                                DialogueResult("Tế Phẩm Chấp Thuận!", $"Thần linh hoan hỷ! Nhận lấy Linh thực hiến tế, ma lực bùng phát tẩy tủy vĩnh viễn cho <b>{lucky.Name}</b>, thức tỉnh thiên phú vĩnh cửu: <b><color=#00ffcc>{HeavenlyTalent.GetDisplayName(talent)}</color></b>!");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.IsFoodSlotLocked = true;
                                victim.AddMemoir(MemoirType.Mutation, "Nguyền Rủa Đói Khát", "Lừa dối ma huyệt bị phạt đói, khóa ô Thức ăn");
                                DialogueResult("Thần Linh Phẫn Nộ!", $"Balo không có Thức ăn hiến tế! Thần linh nổi giận giáng nguyền rủa Đói Khát đói nghèo lên toàn đội. Chú mèo <b>{victim.Name}</b> bị <b><color=red>KHÓA ô Thức ăn (Food/Ultimate Slot)</color></b> vĩnh viễn!");
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
                            victim.AddMemoir(MemoirType.Breakthrough, "Huyết Thệ Nghịch Thiên", "Đột phá cưỡng chế, vĩnh viễn khóa ô Thức ăn");
                            DialogueResult("Huyết Thệ Thành Công!", $"Tế lễ đẫm máu nghịch thiên! Chú mèo <b>{victim.Name}</b> hiến tế kinh mạch tiêu hóa của bản thân. Đột phá cảnh giới vượt bậc vĩnh viễn (+10 Max HP, +15 Speed) nhưng <b><color=red>ô Thức ăn (Food/Ultimate Slot) vĩnh viễn bị KHÓA</color></b>!");
                        }
                        else
                        {
                            RunState.CorruptionLevel = Mathf.Max(0, RunState.CorruptionLevel - 25);
                            DialogueResult("Tâm Hồn Thanh Tịnh", "Toàn đội từ bỏ ý chí tham lam, ma chướng linh mạch được tẩy rửa gột sạch (-25 Corruption)!");
                        }
                    };
                }
                else if (eventRoll == 4)
                {
                    // Kiểm tra giấy phép thông hành lậu
                    title = MewtationsLoc.Translate("exp_license_check_title", "⚠️ KIỂM TRA GIẤY PHÉP ĐỘT XUẤT");
                    text = MewtationsLoc.Translate("exp_license_check_desc", "Một toán Lực Lượng Hành Pháp bọc giáp sắt bất ngờ chặn đội mèo của bạn lại tại chốt rẽ. Đèn linh áp quét thẳng qua chiếc balo khả nghi của bạn.\n\n\"Dừng lại! Kiểm tra giấy phép thông hành và quota khai thác linh thạch. Trình diện ngay lập tức!\"");
                    choices = new List<string> {
                        "Trình thẻ phép lậu (Đút lót 1 Vàng) / Show Forged Permit (1 Gold)",
                        "Chấp nhận tịch thu hàng lậu / Accept Confiscation",
                        "Chạy trốn lập tức (Yêu cầu Tốc độ > 120) / Flee"
                    };
                    onChoice = (idx) =>
                    {
                        if (idx == 0)
                        {
                            int goldIdx = CurrentBackpack.ContainedCardIds.IndexOf("resource_gold");
                            if (goldIdx >= 0)
                            {
                                CurrentBackpack.RemoveItemAt(goldIdx);
                                DialogueResult("Hối Lộ Thành Công", "Lực Lượng Hành Pháp liếc nhìn đồng Vàng, lờ đi đống quặng bất hợp pháp trong balo: \"Giấy phép hợp lệ. Đi mau!\"");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.HealthPoints = Mathf.Max(1, victim.HealthPoints - 10);
                                RunState.AddCorruption(20);
                                DialogueResult("Không Có Tiền Đút Lót", $"Bị phát hiện dùng giấy thông hành giả! Chúng lập tức dùng roi điện đánh thương nặng <b>{victim.Name}</b> (-10 HP) và nâng mức tà lực ma đạo (+20 Corruption)!");
                            }
                        }
                        else if (idx == 1)
                        {
                            if (CurrentBackpack.ContainedCardIds.Count > 0)
                            {
                                int randIdx = UnityEngine.Random.Range(0, CurrentBackpack.ContainedCardIds.Count);
                                string removed = CurrentBackpack.ContainedCardIds[randIdx];
                                CurrentBackpack.RemoveItemAt(randIdx);
                                DialogueResult("Hàng Lậu Bị Tịch Thu", $"Để giữ tính mạng, toàn đội giao nộp <b>{removed.Replace("item_", "").Replace("resource_", "")}</b>. Chúng hừ lạnh thu giữ rồi thả đi.");
                            }
                            else
                            {
                                DialogueResult("Balo Trống Rỗng", "Chúng khám xét balo nhưng không thấy gì khả nghi. Không có gì để tịch thu, chúng đành đá đít xua đuổi bạn đi.");
                            }
                        }
                        else
                        {
                            int avgSpeed = (int)ActiveCats.Average(c => c.Speed);
                            if (avgSpeed > 120)
                            {
                                DialogueResult("Chạy Thoát Thành Công", "Thần tốc! Toàn đội mèo phóng đi trong chớp mắt, cắt đuôi toán tuần tra Dogma một cách hoàn hảo!");
                            }
                            else
                            {
                                var victim = ActiveCats[UnityEngine.Random.Range(0, ActiveCats.Count)];
                                victim.HealthPoints = Mathf.Max(1, victim.HealthPoints - 12);
                                RunState.AddCorruption(20);
                                DialogueResult("Chạy Trốn Thất Bại", $"Tốc độ quá chậm! Toán tuần tra vây bắt và đánh trọng thương <b>{victim.Name}</b> (-12 HP), tà pháp giam giữ gia tăng (+20 Corruption)!");
                            }
                        }
                    };
                }
                else if (eventRoll == 5)
                {
                    // Dân nghèo cầu xin
                    title = MewtationsLoc.Translate("exp_beggar_title", "🐱 DÂN NGHÈO CẦU XIN LINH KHÍ");
                    text = MewtationsLoc.Translate("exp_beggar_desc", "Một chú mèo tiều tụy gầy trơ xương, cơ thể dị biến nặng nề đang quỳ bên đống phế thải công nghiệp, run rẩy van xin:\n\n\"Làm ơn... tôi chỉ xin một mẩu Linh Thạch vụn để duy trì linh căn đang héo úa của con tôi... Bọn Dogma đã siết hết quota của khu này rồi...\"");
                    choices = new List<string> {
                        "Bố thí 1 Quặng Linh Thạch thô / Give 1 Spirit Ore",
                        "Từ chối đi thẳng / Refuse"
                    };
                    onChoice = (idx) =>
                    {
                        if (idx == 0)
                        {
                            int oreIdx = CurrentBackpack.ContainedCardIds.FindIndex(id => id == "item_iron_ore" || id.Contains("ore"));
                            if (oreIdx >= 0)
                            {
                                CurrentBackpack.RemoveItemAt(oreIdx);
                                RunState.CorruptionLevel = Mathf.Max(0, RunState.CorruptionLevel - 30);
                                DialogueResult("Tích Đức Giải Nghiệp", "Chú mèo mừng rỡ ôm lấy mảnh quặng khóc nấc lên. Linh hồn toàn đội được thanh thản, gột rửa bớt tà khí ma kiếp (-30 Corruption)!");
                            }
                            else
                            {
                                DialogueResult("Không Có Linh Thạch", "Bạn rất muốn giúp nhưng balo viễn chinh không có bất kỳ mảnh Quặng Linh Thạch nào. Chú mèo nghèo thất vọng quay đi.");
                            }
                        }
                        else
                        {
                            RunState.GreedLevel = Mathf.Min(100, RunState.GreedLevel + 15);
                            DialogueResult("Quay Lưng Bỏ Đi", "Bạn lạnh lùng bước tiếp. Tiếng khóc than uất nghẹn của dân nghèo bám riết đạo tâm của bạn (+15 Greed)!");
                        }
                    };
                }
                else
                {
                    // Gặp Thương nhân lậu (Black Market Merchant)
                    int maxBreakthrough = ActiveCats.Count > 0 ? ActiveCats.Max(c => c.BreakthroughLevel) : 0;
                    title = MewtationsLoc.Translate("exp_merchant_encounter_title", "⚖️ THƯƠNG NHÂN CHỢ ĐEN");
                    if (maxBreakthrough >= 2)
                    {
                        text = MewtationsLoc.Translate("exp_merchant_high_rank_desc", "Một gã mèo trùm mũ kín mít hé mở chiếc hòm linh bảo giấu kín. Hắn thì thầm đầy tôn kính:\n\n\"Nhìn ngài có vẻ là một Hộ Pháp cao cấp... Tiểu nhân có vài món bảo vật giấu riêng, hoàn toàn không ghi trong sổ sách kiểm kê của Giáo Điều... Ngài có muốn xem qua?\"");
                        choices = new List<string> {
                            "Mua Hóa Thần Thạch / Revive Pill (Tiêu hao 15 Vàng) / 15 Gold",
                            "Mua Linh Dược Đột Phá / Breakthrough Pill (Tiêu hao 15 Vàng) / 15 Gold",
                            "Rút lui / Leave"
                        };
                        onChoice = (idx) =>
                        {
                            if (idx == 0 || idx == 1)
                            {
                                int goldIdx = CurrentBackpack.ContainedCardIds.IndexOf("resource_gold");
                                if (goldIdx >= 0)
                                {
                                    CurrentBackpack.RemoveItemAt(goldIdx);
                                    string itemSpawn = idx == 0 ? "item_revive_pill" : "item_breakthrough_pill";
                                    CurrentBackpack.AddItem(itemSpawn);
                                    DialogueResult("Giao Dịch Thành Công", $"Bảo vật bất hợp pháp <b>{itemSpawn.Replace("item_", "")}</b> đã được giao tay bí mật. Thương nhân đóng rương và lủi mất.");
                                }
                                else
                                {
                                    DialogueResult("Không Đủ Vàng", "Không đủ vàng thanh toán! Hắn lầu bầu đóng rương lại: \"Quay lại khi ngài mang đủ vàng!\"");
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
                        text = MewtationsLoc.Translate("exp_merchant_low_rank_desc", "Một gã mèo trùm mũ kín mít liếc nhìn đội mèo sơ cấp của bạn đầy khinh khỉnh, đóng sập hòm bảo vật lại:\n\n\"Biến đi! Loại tạp mèo thấp kém như các ngươi không đủ cấp để xem hàng này của ta. Đừng làm mất thời gian!\"");
                        choices = new List<string> {
                            "Mua Quặng Linh Thạch giá rẻ (Tiêu hao 3 Vàng) / 3 Gold",
                            "Rút lui / Leave"
                        };
                        onChoice = (idx) =>
                        {
                            if (idx == 0)
                            {
                                int goldIdx = CurrentBackpack.ContainedCardIds.IndexOf("resource_gold");
                                if (goldIdx >= 0)
                                {
                                    CurrentBackpack.RemoveItemAt(goldIdx);
                                    CurrentBackpack.AddItem("item_iron_ore");
                                    DialogueResult("Giao Dịch Hạng Thấp", "Hắn ném cho bạn một mảnh Quặng Linh Thạch thô rẻ tiền rồi thu tiền vàng đầy thô bạo.");
                                }
                                else
                                {
                                    DialogueResult("Không Có Vàng", "Không có vàng! Hắn phất tay xua đuổi: \"Không có tiền thì biến đi chỗ khác!\"");
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
            else if (type == NodeType.Lore)
            {
                // 50% chance to trigger GDD Weary Dog Patrol Officer Dialogue Encounter
                if (UnityEngine.Random.value <= 0.50f)
                {
                    TriggerWearyDogEncounter();
                    return; // Return early since TriggerWearyDogEncounter handles dialogue triggering
                }
                else
                {
                    title = "Bích Họa Cổ Xưa";
                    text = "Trải rộng trên bức tường đá rêu phong là những bích họa mô tả về thời kỳ 'Thần Mèo Sáng Thế' và cuộc viễn chinh cổ đại.\n\nLinh hồn của toàn đội được gột rửa, giúp gia tăng Speed tạm thời!";
                    choices = new List<string> { "Tiếp thu tinh hoa" };
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
                title = "Phế Tích Hoang Phế";
                text = "Đội hình mèo tiến vào một phế tích cung điện đổ nát. Ở giữa có một lò đan dược cũ kỹ vẫn đang cháy âm ỉ.\nBạn có muốn lục lọi không?";
                choices = new List<string> { "Mở lò đan dược", "Rút lui" };
                onChoice = (idx) =>
                {
                    if (idx == 0)
                    {
                        if (UnityEngine.Random.value < 0.5f)
                        {
                            CurrentBackpack.AddItem("item_revive_pill");
                            DialogueResult("Luyện Đan Kỳ Tích!", "Tuyệt vời! Bên trong lò đan vẫn còn lưu giữ một viên Linh Đan Hồi Sinh cực kỳ quý hiếm!");
                        }
                        else
                        {
                            DialogueResult("Khói đen mù mịt", "Lò đan nổ tung! Khói đen kịt phả thẳng vào mặt khiến toàn đội bám đầy tro bụi (Không có tổn thất thực tế).");
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
                MewtationsLoc.Translate("opt_fight", "⚔️ Force breakthrough (+20 Corruption)"),
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
                MewtationsLoc.Translate("opt_stealth", "🏃 Sneak past silently (Requires Speed > 115)"),
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

            // Option 3: Comfort (Thiền Đạo Cảm Hóa)
            choices.Add(new Mewtations.Dialogue.DialogueChoice(
                MewtationsLoc.Translate("opt_comfort", "☯️ [Zen Dao Comfort] Teach human philosophy & soothe his soul"),
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
                MewtationsLoc.Translate("opt_comfort_req", "Cần có Mèo Thiền Đạo / Requires Zen Cat")
            ));

            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, choices);
        }

        private void DialogueResult(string title, string text)
        {
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { "Tiếp tục" }, (idx) =>
            {
                CompleteNodeResolution();
            });
        }

        public void CompleteNodeResolution()
        {
            State = ExpeditionState.MapNavigation;

            // Automation Relic Tick logic
            ApplyRelicAutomationProgress();

            // Node is cleared. Check connections of visited nodes to unlock next layer nodes
            UpdateConnections();

            // Check if final boss node was visited and solved
            if (ActiveNode != null && ActiveNode.Type == NodeType.Boss)
            {
                Debug.Log("[Expedition] Hoàn thành boss viễn chinh! Thắng lợi lớn!");
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
            Debug.Log($"[Relic Automation] Kích hoạt Cổ Vật {relic} tự động hóa căn cứ từ xa!");

            foreach (var gc in WorldManager.instance.AllCards)
            {
                if (gc != null && !gc.Destroyed && gc.CardData != null && gc.TimerRunning)
                {
                    string cid = gc.CardData.Id.ToLower();
                    
                    if (relic == "item_ancient_relic_smelt" && (cid.Contains("smelter") || cid.Contains("furnace")))
                    {
                        gc.CurrentTimerTime += 15f; // Smelting automation ticks by 15s!
                        Debug.Log($"   • [Cổ Vật Tự Động Nung] Tự động thúc tiến +15s cho {gc.CardData.Name}!");
                    }
                    else if (relic == "item_ancient_relic_wood" && (cid.Contains("sawmill") || cid.Contains("mill")))
                    {
                        gc.CurrentTimerTime += 15f; // Wood processing automation ticks by 15s!
                        Debug.Log($"   • [Cổ Vật Tự Động Xẻ] Tự động thúc tiến +15s cho {gc.CardData.Name}!");
                    }
                    else if (relic == "item_ancient_relic_booster")
                    {
                        gc.CurrentTimerTime += 5f; // Universal booster ticks all timers by 5s!
                        Debug.Log($"   • [Linh Thần Thu Hoạch] Tự động thúc tiến +5s cho công trình {gc.CardData.Name}!");
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
            IsExpeditionActive = false;
            State = ExpeditionState.Idle;

            // Close UI overlays
            if (ExpeditionMapUI.Instance != null) ExpeditionMapUI.Instance.HideWindow();
            if (CombatOverlayUI.Instance != null) CombatOverlayUI.Instance.HideWindow();
            if (Mewtations.Dialogue.DialogueSystem.Instance != null) Mewtations.Dialogue.DialogueSystem.Instance.HideWindow();

            // Resume base board time
            Time.timeScale = 1f;

            if (PortalCardSource != null)
            {
                Vector3 spawnPos = PortalCardSource.transform.position + Vector3.back * 1.5f;

                                // Return cats to base board and clear active temporary mutations
                foreach (var cat in ActiveCats)
                {
                    if (cat != null)
                    {
                        cat.ClearMutations(); // Mutations cleared upon returning to base!
                        
                        // Phase 3: Expedition Aftermath (Exhaustion Debt)
                        int staminaDebt = 20; // Base stamina cost of going on an expedition
                        if (RunState != null) {
                            staminaDebt += (RunState.CurrentLayer * 5); // +5 stamina per layer deepened
                        }
                        cat.Stamina = UnityEngine.Mathf.Max(0, cat.Stamina - staminaDebt);
                        
                        // Adding Memoirs
                        if (cat.Stamina == 0) {
                            cat.AddMemoir("Tr? v? trong tr?ng thi ki?t s?c! (Exhausted Return)");
                        }
                        if (RunState != null && RunState.CorruptionLevel > 50) {
                            cat.AddMemoir("Tr? v? v?i t kh (Corrupted Return)");
                        }
                        if (isManualRetreat) {
                            cat.AddMemoir("B? tr?n kh?i vi?n chinh (Retreat)");
                        }

                        if (cat.MyGameCard != null)
                        {
                            // Clean combat overlay links and set position
                            cat.MyGameCard.transform.position = spawnPos;
                            cat.MyGameCard.gameObject.SetActive(true);
                        }
                    }
                }

                if (!isDefeat)
                {
                    // Dung hợp thiên phú vĩnh viễn (Song Trọng Dị Biến: tối đa 2 đột biến vĩnh viễn)
                    MutationPersistenceSystem.ProcessRunVictoryTraits(ActiveCats);

                    // Spawn Backpack loot items around the portal safely
                    ExpeditionRewardSystem.SpawnBackpackLoot(CurrentBackpack, spawnPos);

                    // Special Boss Victory Reward: A new Heavenly Talent cat!
                    if (ActiveNode != null && ActiveNode.Type == NodeType.Boss)
                    {
                        var summoning = new CatSummoningSystem(WorldManager.instance);
                        summoning.SummonCat(spawnPos, highestBreakthroughLevel: 2); // Guaranteed breakthrough potential
                        Debug.Log("[Expedition] Triệu hồi Thiên Kiêu mèo làm phần thưởng chiến thắng Boss!");
                    }
                }
                else
                {
                    // Calculate and apply scaled drop penalty on force abandon/retreat/defeat
                    if (CurrentBackpack != null)
                    {
                        if (isManualRetreat)
                        {
                            // Cowardice Tax: lose exactly 50% of backpack items randomly, and add +15 Greed!
                            if (RunState != null)
                            {
                                RunState.GreedLevel = Mathf.Min(100, RunState.GreedLevel + 15);
                            }
                            ExpeditionExtractionSystem.ApplyManualRetreatPenalty(CurrentBackpack);
                            Debug.Log("[Expedition] Người chơi chủ động rút lui! Áp dụng Thuế Nhát Gan: Mất ngẫu nhiên 50% balo, +15 Greed khí vận.");
                        }
                        else
                        {
                            float rate = ExpeditionExtractionSystem.CalculateLootRetentionRate(RunState, CurrentBackpack);
                            ExpeditionExtractionSystem.ApplyAbandonPenalty(CurrentBackpack, rate);
                            Debug.Log("[Expedition] Viễn chinh thất bại hoặc bị tiêu diệt! Áp dụng hình phạt hao hụt balo nghiêm trọng.");
                        }
                        ExpeditionRewardSystem.SpawnBackpackLoot(CurrentBackpack, spawnPos);
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

            Debug.Log("[Expedition] Kết thúc viễn chinh. Trở về base.");
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

            // Re-freeze timescale if expedition is active
            Time.timeScale = 0f;

            // Hide the actual game cards of cats and backpack from board
            foreach (var cat in ActiveCats)
            {
                if (cat != null && cat.MyGameCard != null)
                {
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



