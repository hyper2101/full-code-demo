using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mewtations.Combat;

namespace Mewtations.Expedition
{
    public class ExpeditionManager : MonoBehaviour
    {
        public static ExpeditionManager Instance { get; private set; }

        public bool IsExpeditionActive = false;
        public List<ExpeditionNode> MapNodes = new List<ExpeditionNode>();
        public ExpeditionNode ActiveNode = null;
        public List<CatCardData> ActiveCats = new List<CatCardData>();
        public CardData BackpackCardSource = null;
        public Backpack CurrentBackpack = null;
        public GameCard PortalCardSource = null;

        private void Awake()
        {
            Instance = this;
        }

        public void StartExpedition(GameCard portalCard, List<CatCardData> cats, CardData backpackCard)
        {
            if (IsExpeditionActive) return;

            IsExpeditionActive = true;
            PortalCardSource = portalCard;
            ActiveCats = cats;
            BackpackCardSource = backpackCard;

            // Set up Backpack based on the card capacity, default to 10 if none
            int capacity = (backpackCard != null && backpackCard.BackpackCapacity > 0) ? backpackCard.BackpackCapacity : 10;
            CurrentBackpack = new Backpack(capacity);

            // Generate procedural node map
            int seed = UnityEngine.Random.Range(0, 100000);
            MapNodes = ExpeditionMapGenerator.GenerateMap(seed, maxLayers: 6, maxNodesPerLayer: 3);
            ActiveNode = null;

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

            // Hide Map UI while resolving node activities
            if (ExpeditionMapUI.Instance != null)
            {
                ExpeditionMapUI.Instance.HideWindow();
            }

            Debug.Log($"[Expedition] Tiến vào node {node.Id} ({node.Type}) ở Tầng {node.Layer}.");

            switch (node.Type)
            {
                case NodeType.Combat:
                    TriggerCombat(isBoss: false);
                    break;

                case NodeType.Boss:
                    TriggerCombat(isBoss: true);
                    break;

                case NodeType.Resource:
                    TriggerResourceNode();
                    break;

                case NodeType.Event:
                case NodeType.Lore:
                case NodeType.Ruins:
                    TriggerTextEventNode(node.Type);
                    break;
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

            // Start turn-based combat overlay
            List<Combatable> playerCombats = ActiveCats.Cast<Combatable>().ToList();
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
                title = "Kỳ Duyên Kỳ Ngộ";
                text = "Trước mặt bạn xuất hiện một thạch thất nhỏ tỏa ra linh khí mờ ảo. Có vẻ như có di tích của tiền bối để lại.\nBạn muốn làm gì?";
                choices = new List<string> { "Phá cửa tiến vào (Kiểm tra Speed)", "Thành tâm bái lạy", "Bỏ qua tránh bẫy" };
                onChoice = (idx) =>
                {
                    if (idx == 0)
                    {
                        // Speed check
                        int avgSpeed = (int)ActiveCats.Average(c => c.Speed);
                        if (avgSpeed > 100)
                        {
                            CurrentBackpack.AddItem("item_healing_potion");
                            CurrentBackpack.AddItem("resource_gold");
                            DialogueResult("Phá cửa thành công!", "Nhờ phản ứng nhanh nhẹn, đội của bạn đã phá cửa thạch thất thành công và tìm thấy Thần dược cùng một ít Vàng!");
                        }
                        else
                        {
                            // Debuff all cats a bit
                            foreach (var cat in ActiveCats)
                            {
                                cat.HealthPoints = Mathf.Max(1, cat.HealthPoints - 2);
                            }
                            DialogueResult("Bẫy kích hoạt!", "Tốc độ quá chậm! Cửa thạch thất sập xuống kích hoạt trận pháp lôi điện, khiến toàn đội bị thương nhẹ.");
                        }
                    }
                    else if (idx == 1)
                    {
                        // Worship
                        CurrentBackpack.AddItem("resource_gold");
                        DialogueResult("Tấm lòng thành kính", "Thành tâm bái lạy giúp linh hồn tiền bối an nghỉ. Một túi Vàng nhẹ nhàng rơi xuống trước mặt toàn đội.");
                    }
                    else
                    {
                        DialogueResult("Rút lui an toàn", "Toàn đội cẩn thận rút lui, không gặp phải bất kỳ tổn thất nào.");
                    }
                };
            }
            else if (type == NodeType.Lore)
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

        private void DialogueResult(string title, string text)
        {
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { "Tiếp tục" }, (idx) =>
            {
                CompleteNodeResolution();
            });
        }

        public void CompleteNodeResolution()
        {
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

        public void ReturnToBase(bool isDefeat)
        {
            IsExpeditionActive = false;

            // Close UI overlays
            if (ExpeditionMapUI.Instance != null) ExpeditionMapUI.Instance.HideWindow();
            if (CombatOverlayUI.Instance != null) CombatOverlayUI.Instance.HideWindow();
            if (Mewtations.Dialogue.DialogueSystem.Instance != null) Mewtations.Dialogue.DialogueSystem.Instance.HideWindow();

            // Resume base board time
            Time.timeScale = 1f;

            if (PortalCardSource != null)
            {
                Vector3 spawnPos = PortalCardSource.transform.position + Vector3.back * 1.5f;

                // Return cats to base board
                foreach (var cat in ActiveCats)
                {
                    if (cat != null && cat.MyGameCard != null)
                    {
                        // Clean combat overlay links and set position
                        cat.MyGameCard.transform.position = spawnPos;
                        cat.MyGameCard.gameObject.SetActive(true);
                    }
                }

                if (!isDefeat)
                {
                    // Spawn Backpack loot items around the portal
                    foreach (string lootId in CurrentBackpack.ContainedCardIds)
                    {
                        Vector3 jitterPos = spawnPos + new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, UnityEngine.Random.Range(-0.5f, 0.5f));
                        WorldManager.instance.CreateCard(jitterPos, lootId, true, true, true);
                    }

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
                    Debug.Log("[Expedition] Viễn chinh thất bại hoặc rút lui! Mất toàn bộ chiến lợi phẩm trong Balo.");
                }

                // Restore Backpack Card if present
                if (BackpackCardSource != null && BackpackCardSource.MyGameCard != null)
                {
                    BackpackCardSource.MyGameCard.transform.position = spawnPos + Vector3.right * 1.0f;
                    BackpackCardSource.MyGameCard.gameObject.SetActive(true);
                }

                // If portal is strange/one-time, destroy it
                if (PortalCardSource.CardData.Id == "strange_portal")
                {
                    PortalCardSource.DestroyCard(false, true);
                }
            }

            Debug.Log("[Expedition] Kết thúc viễn chinh. Trở về base.");
        }
    }
}
