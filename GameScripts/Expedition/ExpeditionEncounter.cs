using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mewtations.Combat;

namespace Mewtations.Expedition
{
    public interface IExpeditionEncounter
    {
        void Resolve(Action onComplete);
    }

    public class CombatEncounter : IExpeditionEncounter
    {
        private bool _isBoss;
        private int _layer;

        public CombatEncounter(int layer, bool isBoss)
        {
            _layer = layer;
            _isBoss = isBoss;
        }

        public void Resolve(Action onComplete)
        {
            var manager = ExpeditionManager.Instance;
            var runState = manager.RunState;

            // Roll enemies
            List<Combatable> enemies = new List<Combatable>();
            int enemyCount = UnityEngine.Random.Range(1, 4);
            if (_isBoss) enemyCount = 1;

            bool isThuTrieu = manager.ActiveNode != null && manager.ActiveNode.Theme == RouteTheme.ThuTrieu;
            if (isThuTrieu && !_isBoss)
            {
                enemyCount += 1;
                Debug.Log("[Expedition] Lộ trình Thú Triều! Quái vật vây quanh tăng thêm 1 đơn vị.");
            }

            Vector3 spawnPos = Vector3.zero;
            for (int i = 0; i < enemyCount; i++)
            {
                string enemyId = _isBoss ? "boss_goblin_king" : RollEnemyId(_layer);
                var enemyCard = WorldManager.instance.CreateCard(spawnPos, enemyId, false, false, false);
                if (enemyCard != null && enemyCard.CardData is Combatable comb)
                {
                    // Scale enemy stats based on Greed Level (+5% HP, DMG and Speed per 10 Greed)
                    float scaleFactor = 1.0f + (runState.GreedLevel / 10f) * 0.05f;
                    
                    comb.BaseCombatStats.MaxHealth = Mathf.RoundToInt(comb.BaseCombatStats.MaxHealth * scaleFactor);
                    comb.HealthPoints = comb.BaseCombatStats.MaxHealth;
                    comb.BaseCombatStats.AttackDamage = Mathf.RoundToInt(comb.BaseCombatStats.AttackDamage * scaleFactor);

                    enemies.Add(comb);
                }
            }

            // Print warning log if Greed scaled enemies significantly
            if (runState.GreedLevel > 0)
            {
                float pct = (runState.GreedLevel / 10f) * 5f;
                Debug.LogWarning($"[Expedition] Greed Level {runState.GreedLevel}! Quái vật được tăng {pct}% chỉ số sinh mệnh & sát thương.");
            }

            // Start turn-based combat overlay
            List<Combatable> playerCombats = manager.ActiveCats.Cast<Combatable>().ToList();
            TurnBasedCombatManager.Instance.StartCombat(playerCombats, enemies, (result) =>
            {
                // Clean up enemy cards
                foreach (var enemy in enemies)
                {
                    if (enemy != null && enemy.MyGameCard != null)
                    {
                        enemy.MyGameCard.DestroyCard(true, true);
                    }
                }

                if (result == CombatResult.Victory)
                {
                    // Reward loot and increase Corruption (+15 per node traversed)
                    RollLoot(manager, _isBoss);
                    runState.AddCorruption(15);

                    // Thưởng kinh nghiệm tu vi tu tiên cho Mèo khi chiến thắng trận viễn chinh
                    int expReward = _isBoss ? 150 : 50;
                    foreach (var cat in manager.ActiveCats)
                    {
                        cat.GainExperience(expReward);
                    }
                    
                    // Add specific Route Theme and Boss memoirs
                    bool isThienLoi = manager.ActiveNode != null && manager.ActiveNode.Theme == RouteTheme.ThienLoi;
                    if (isThienLoi)
                    {
                        foreach (var cat in manager.ActiveCats)
                        {
                            cat.Speed += 15;
                            cat.AddMemoir(MemoirType.Breakthrough, "Lôi Đình Tẩy Tủy", "Vượt qua Kiếp Lôi, rèn luyện thân thể tăng 15 Thần Tốc");
                        }
                    }

                    if (isThuTrieu)
                    {
                        foreach (var cat in manager.ActiveCats)
                        {
                            cat.AddMemoir(MemoirType.BossKill, "Dị Thú Vương", "Trảm sát Thú Vương trong biển thú cuồng trào");
                        }
                    }

                    if (_isBoss)
                    {
                        foreach (var cat in manager.ActiveCats)
                        {
                            cat.AddMemoir(MemoirType.BossKill, "Goblin Đế Vương", "Trảm sát Thống soái viễn chinh");
                        }
                    }

                    // Trigger unstable mutations if corruption is high (> 50)
                    ApplyHighCorruptionCheck(manager);

                    onComplete?.Invoke();
                }
                else
                {
                    // Defeat or Retreat: Return to base
                    manager.ReturnToBase(isDefeat: true);
                }
            });
        }

        private string RollEnemyId(int layer)
        {
            string[] lowTier = { "goblin", "rat", "slime" };
            string[] medTier = { "skeleton", "wolf", "goblin" };
            string[] highTier = { "demon", "skeleton_mage", "wolf" };

            if (layer <= 1) return lowTier[UnityEngine.Random.Range(0, lowTier.Length)];
            if (layer <= 3) return medTier[UnityEngine.Random.Range(0, medTier.Length)];
            return highTier[UnityEngine.Random.Range(0, highTier.Length)];
        }

        private void RollLoot(ExpeditionManager manager, bool isBoss)
        {
            int lootCount = isBoss ? 4 : UnityEngine.Random.Range(1, 3);
            string[] possibleLoot = { "resource_gold", "resource_food", "item_healing_potion", "item_iron_ore", "item_wood", "item_stone" };

            bool isThamLam = manager.ActiveNode != null && manager.ActiveNode.Theme == RouteTheme.ThamLam;
            bool isThuTrieu = manager.ActiveNode != null && manager.ActiveNode.Theme == RouteTheme.ThuTrieu;
            if (isThamLam || isThuTrieu)
            {
                lootCount *= 2;
                Debug.Log($"[Expedition] Nhân đôi chiến lợi phẩm do lộ trình {(isThamLam ? "Tham Lam" : "Thú Triều")}.");
            }

            for (int i = 0; i < lootCount; i++)
            {
                string loot = possibleLoot[UnityEngine.Random.Range(0, possibleLoot.Length)];
                manager.CurrentBackpack.AddItem(loot);
            }

            // Nếu là Boss tiến độ, thưởng thêm Cổ Vật tự động hóa ngẫu nhiên
            if (isBoss)
            {
                string[] relics = { "item_ancient_relic_auto_farm", "item_ancient_relic_auto_collect", "item_ancient_relic_auto_heal" };
                string chosenRelic = relics[UnityEngine.Random.Range(0, relics.Length)];
                manager.CurrentBackpack.AddItem(chosenRelic);
                Debug.Log($"[Expedition] Boss chiến thắng! Nhận thêm Cổ Vật chí tôn: {chosenRelic}");
            }
        }

        private void ApplyHighCorruptionCheck(ExpeditionManager manager)
        {
            var runState = manager.RunState;
            if (runState.CorruptionLevel >= 50)
            {
                // 30% chance to contract an unstable mutation due to heavy corruption pressure
                if (UnityEngine.Random.value <= 0.35f)
                {
                    var cat = manager.ActiveCats[UnityEngine.Random.Range(0, manager.ActiveCats.Count)];
                    string[] possibleMutations = { 
                        UnstableMutation.UnstableClaws, 
                        UnstableMutation.LethargicNap, 
                        UnstableMutation.CursedFur 
                    };
                    string mutation = possibleMutations[UnityEngine.Random.Range(0, possibleMutations.Length)];
                    
                    if (!cat.HasMutation(mutation))
                    {
                        cat.AddMutation(mutation);
                        cat.AddMemoir(MemoirType.Mutation, UnstableMutation.GetDisplayName(mutation), "Ma khí thâm nhập gây dị biến linh mạch");
                        string msg = $"<color=red>☣️ Ô NHIỄM CỰC HẠN!</color>\n\nSát khí linh lực tích tụ quá cao ({runState.CorruptionLevel}%) đã thâm nhập cơ thể của <b>{cat.Name}</b>, gây ra dị biến: <b><color=#ff3333>{UnstableMutation.GetDisplayName(mutation)}</color></b>!\n\n<i>{UnstableMutation.GetDescription(mutation)}</i>";
                        
                        if (Mewtations.Dialogue.DialogueSystem.Instance != null)
                        {
                            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue("DỊ BIẾN LINH LỰC", msg, new List<string> { "Đành chấp nhận..." }, (idx) => {});
                        }
                    }
                }
            }
        }
    }

    public class ResourceGatherEncounter : IExpeditionEncounter
    {
        public void Resolve(Action onComplete)
        {
            var manager = ExpeditionManager.Instance;
            var runState = manager.RunState;

            string title = "Bãi Khai Thác Hoang Dã";
            string text = "Đội ngũ Thần Miêu phát hiện một mỏ quặng linh thạch khổng lồ hoang phế nằm ven đường.\n\n" +
                          "Bạn muốn khai thác nó như thế nào?\n\n" +
                          "• <b>Khai thác chuẩn mực:</b> Nhận tài nguyên linh thạch dồi dào trên bảng.\n" +
                          "• <b>Khai thác cạn kiệt (Tham lam):</b> Nhận gấp đôi tài nguyên linh thạch nhưng tăng <b>+20 Greed</b> (Quái vật tầng sâu mạnh lên).";

            var choices = new List<string> { "Khai thác chuẩn mực", "Khai thác cạn kiệt (+20 Greed)" };
            
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, choices, (choiceIdx) =>
            {
                int harvests = (choiceIdx == 1) ? 2 : 1;
                if (choiceIdx == 1)
                {
                    runState.AddGreed(20);
                }

                int lootCount = UnityEngine.Random.Range(3, 5) * harvests;
                string[] resources = { "resource_food", "item_wood", "item_stone", "resource_gold", "item_iron_ore" };

                // Get portal position for physical spawning
                Vector3 spawnPos = Vector3.zero;
                if (manager.PortalCardSource != null)
                {
                    spawnPos = manager.PortalCardSource.transform.position;
                }

                // Unfreeze the board so player can physically interact and collect
                Time.timeScale = 1f;

                // Spawn resources physically on the board!
                for (int i = 0; i < lootCount; i++)
                {
                    string res = resources[UnityEngine.Random.Range(0, resources.Length)];
                    Vector3 jitterPos = spawnPos + new Vector3(UnityEngine.Random.Range(-1.5f, 1.5f), 0, UnityEngine.Random.Range(-1.5f, 1.5f));
                    WorldManager.instance.CreateCard(jitterPos, res, true, true, true);
                }

                runState.AddCorruption(15);

                // Spawn Gathering Room Helper
                GameObject helperGo = new GameObject("GatheringRoomHelper");
                var helper = helperGo.AddComponent<GatheringRoomHelper>();
                helper.OnFinished = () =>
                {
                    // Clean up and complete
                    onComplete?.Invoke();
                };
            });
        }
    }

    public class CatGodAltarEncounter : IExpeditionEncounter
    {
        public void Resolve(Action onComplete)
        {
            var manager = ExpeditionManager.Instance;
            var runState = manager.RunState;
            var backpack = manager.CurrentBackpack;

            bool isTaDao = manager.ActiveNode != null && manager.ActiveNode.Theme == RouteTheme.TaDao;

            string title = "Miệng Thần Mèo - Hiến Tế Đàn";
            string text = "Một khe vực khổng lồ mở ra, tạo hình như một chiếc Miệng Thần Mèo đói khát cổ xưa. Linh khí từ miệng vực thốt ra rợn người.\n\n" +
                          "Để đổi lấy sự bình yên hoặc linh căn thiên phú, thần linh đòi hỏi một sự đánh đổi công bằng.\n\n" +
                          $"Balo hiện tại có: <b>{backpack.ContainedCardIds.Count} vật phẩm</b>.";

            if (isTaDao)
            {
                text += "\n\n<color=#ff33cc>☠️ NƠI NÀY LÀ TÀ ĐẠO HUYỆT! Trận pháp tà môn cho phép thực hiện hiến tế đẫm máu cướp đoạt thiên phú.</color>";
            }

            var choices = new List<string> { 
                "Thanh tẩy Ô Nhiễm (-30 Corruption - Cần 2 vật phẩm)", 
                "Hối lộ Luật Pháp (-35 Greed - Cần 2 vật phẩm)", 
                "Cầu nguyện Thiên Phú (Ngẫu nhiên Thiên Kiêu - Tăng +25 Corruption)", 
                "Trục xuất An Toàn về Base (Bảo toàn 100% Loot)"
            };

            if (isTaDao)
            {
                choices.Add("Nghi thức Tà Đạo: Tế lễ đồng đội (Nhận Thần Thể Kịch Độc - Tăng +15 Corruption)");
            }

            choices.Add("Rời đi bình yên");

            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, choices, (choiceIdx) =>
            {
                if (choiceIdx == 0) // Cleanse Corruption
                {
                    if (backpack.ContainedCardIds.Count >= 2)
                    {
                        backpack.RemoveItemAt(backpack.ContainedCardIds.Count - 1);
                        backpack.RemoveItemAt(backpack.ContainedCardIds.Count - 1);
                        runState.CorruptionLevel = Mathf.Max(0, runState.CorruptionLevel - 30);
                        
                        Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue("Linh Khí Gột Rửa", "Vật phẩm hiến tế tan biến vào hư vô. Linh lực thanh khiết lan tỏa, gột rửa 30% ô nhiễm ma đạo!\n\nCorruption hiện tại: <b>" + runState.CorruptionLevel + "%</b>.", new List<string> { "Tạ ơn thần!" }, (idx) => onComplete?.Invoke());
                    }
                    else
                    {
                        Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue("Hiến Tế Thất Bại", "Bạn không có đủ 2 vật phẩm trong balo để thực hiện tế lễ!", new List<string> { "Quay lại" }, (idx) => Resolve(onComplete));
                    }
                }
                else if (choiceIdx == 1) // Payoff Greed
                {
                    if (backpack.ContainedCardIds.Count >= 2)
                    {
                        backpack.RemoveItemAt(backpack.ContainedCardIds.Count - 1);
                        backpack.RemoveItemAt(backpack.ContainedCardIds.Count - 1);
                        runState.GreedLevel = Mathf.Max(0, runState.GreedLevel - 35);

                        Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue("Tài Sản Hóa Giải", "Đồ vật quý giá được dâng nộp. Luật nhân quả được xoa dịu, giảm bớt 35 điểm tham lam tích tụ!\n\nGreed hiện tại: <b>" + runState.GreedLevel + "/100</b>.", new List<string> { "Tuyệt vời" }, (idx) => onComplete?.Invoke());
                    }
                    else
                    {
                        Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue("Hiến Tế Thất Bại", "Bạn không có đủ 2 vật phẩm trong balo để hối lộ thiên địa!", new List<string> { "Quay lại" }, (idx) => Resolve(onComplete));
                    }
                }
                else if (choiceIdx == 2) // Pray for Talent
                {
                    // Add heavy corruption but give a random cat a Heavenly Talent!
                    var cat = manager.ActiveCats[UnityEngine.Random.Range(0, manager.ActiveCats.Count)];
                    
                    string[] talents = { 
                        HeavenlyTalent.HeavenlyPoisonBody, 
                        HeavenlyTalent.DivineShieldProtection, 
                        HeavenlyTalent.RageOvercharger, 
                        HeavenlyTalent.MartialArtsCleave 
                    };
                    string rolledTalent = talents[UnityEngine.Random.Range(0, talents.Length)];
                    
                    cat.AddTrait(rolledTalent);
                    cat.CustomName = $"{HeavenlyTalent.GetDisplayName(rolledTalent)} {cat.Name}";
                    cat.AddMemoir(MemoirType.Breakthrough, HeavenlyTalent.GetDisplayName(rolledTalent), "Khai mở Thiên phúc tại Miệng Thần Mèo");
                    runState.AddCorruption(25);

                    string talentDesc = $"★ {cat.Name} được ban phúc thành tựu <b>{HeavenlyTalent.GetDisplayName(rolledTalent)}</b>!\n\nCorruption tăng thêm +25% vì dám cướp đoạt sinh cơ.";

                    Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue("Linh Căn Tẩy Tủy", talentDesc, new List<string> { "Nhận Thần Lực!" }, (idx) => onComplete?.Invoke());
                }
                else if (choiceIdx == 3) // Safe extraction portal
                {
                    Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(
                        "Cổng Trục Xuất An Toàn", 
                        "Miệng Thần Mèo phát ra hào quang bao phủ toàn đội. Bạn sẽ quay về Base Camp an toàn với toàn bộ chiến lợi phẩm bảo toàn!", 
                        new List<string> { "Kích hoạt Cổng Dịch Chuyển" }, 
                        (idx) => manager.ReturnToBase(isDefeat: false)
                    );
                }
                else if (isTaDao && choiceIdx == 4) // Sacrifice teammate moral temptation
                {
                    if (manager.ActiveCats.Count < 2)
                    {
                        Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue("Hiến Tế Thất Bại", "Phải có ít nhất 2 Thần Miêu trong đội hình mới có thể thực hiện hiến tế đồng đội!", new List<string> { "Quay lại" }, (idx) => Resolve(onComplete));
                    }
                    else
                    {
                        // Select a cat to sacrifice
                        string sacTitle = "LỰA CHỌN TẾ PHẨM";
                        string sacText = "Chọn một đồng đội để hiến tế linh hồn vào Tà Huyệt. Chú mèo này sẽ vĩnh viễn biến mất, nhưng linh lực hiến tế sẽ gieo mầm 'Thần Thể Kịch Độc' vào kinh mạch của đồng đội còn lại.";
                        var catNames = manager.ActiveCats.Select(c => c.Name).ToList();

                        Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(sacTitle, sacText, catNames, (sacIdx) =>
                        {
                            var sacrificedCat = manager.ActiveCats[sacIdx];
                            manager.ActiveCats.RemoveAt(sacIdx);

                            // Destroy card
                            if (sacrificedCat.MyGameCard != null)
                            {
                                sacrificedCat.MyGameCard.DestroyCard(true, true);
                            }

                            // Pick another cat to receive the talent
                            var beneficiary = manager.ActiveCats[UnityEngine.Random.Range(0, manager.ActiveCats.Count)];
                            beneficiary.AddTrait(HeavenlyTalent.HeavenlyPoisonBody);
                            beneficiary.CustomName = $"{HeavenlyTalent.GetDisplayName(HeavenlyTalent.HeavenlyPoisonBody)} {beneficiary.Name}";
                            beneficiary.AddMemoir(MemoirType.Birth, $"Nhận dâng hiến tế lễ từ {sacrificedCat.Name}, thức tỉnh Thần Thể Kịch Độc");

                            runState.AddCorruption(15);

                            string resText = $"🔴 HIẾN TẾ HOÀN THÀNH!\n\n<b>{sacrificedCat.Name}</b> đã tan biến vào làn khói máu.\n\n" +
                                             $"Cơ thể của <b>{beneficiary.Name}</b> bùng phát độc lực mạnh mẽ, thức tỉnh thành công <b><color=red>{HeavenlyTalent.GetDisplayName(HeavenlyTalent.HeavenlyPoisonBody)}</color></b>!\n\n" +
                                             $"Ô Nhiễm linh mạch tăng thêm +15 Corruption.";

                            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue("Huyết Tế Ma Công", resText, new List<string> { "Chấp nhận lực lượng" }, (idx) => onComplete?.Invoke());
                        });
                    }
                }
                else // Rời đi bình yên
                {
                    onComplete?.Invoke();
                }
            });
        }
    }

    public class MysteryMutationEncounter : IExpeditionEncounter
    {
        public void Resolve(Action onComplete)
        {
            var manager = ExpeditionManager.Instance;
            var runState = manager.RunState;

            bool isTaDao = manager.ActiveNode != null && manager.ActiveNode.Theme == RouteTheme.TaDao;

            if (isTaDao)
            {
                string title = "Lò Đan Xác Chết - Phế Tích Tà Đạo";
                string text = "Bên trong phế tích âm u này, lò luyện đan cổ xưa đang rực lửa máu. Một làn khói cốt tủy bốc lên tỏa mùi hương cám dỗ ghê rợn.\n\n" +
                              "Lò đan ẩn chứa 'Xác Đan' chế luyện từ linh thể quái thú cổ đại. \n\n" +
                              "Bạn muốn chọn Thần Miêu nào để thực hiện?";
                
                var choices = new List<string>();
                choices.AddRange(manager.ActiveCats.Select(c => $"Chọn {c.Name} Nuốt Xác Đan (+10 Max HP, nhận Nguyền Rủa Lông Tơ +10 Corruption)"));
                choices.Add("Từ chối tà pháp, rút lui an toàn");

                Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, choices, (choiceIdx) =>
                {
                    if (choiceIdx < manager.ActiveCats.Count)
                    {
                        var cat = manager.ActiveCats[choiceIdx];
                        cat.BaseCombatStats.MaxHealth += 10;
                        cat.HealthPoints += 10;
                        cat.AddMutation(UnstableMutation.CursedFur);
                        cat.AddMemoir(MemoirType.Mutation, "Xác Đan Nghịch Thiên", "Nuốt đan dược ma đạo tăng +10 HP cực hạn, gánh chịu Nguyền rủa Lông Tơ");
                        
                        runState.AddCorruption(10);

                        string resText = $"🔴 NUỐT XÁC ĐAN THÀNH CÔNG!\n\n<b>{cat.Name}</b> đã nuốt chửng linh đan luyện từ xác chết. Khí huyết cuồn cuộn dâng trào vượt bậc (+10 Max HP)!\n\n" +
                                        $"Tuy nhiên tà khí ô nhiễm nặng nề đã ăn mòn lông tơ của chú: <b><color=red>{UnstableMutation.GetDisplayName(UnstableMutation.CursedFur)}</color></b> (Giảm 5 Giáp, không thể nhận Giáp bảo hộ)!\n\n" +
                                        $"Corruption tăng thêm +10%.";

                        Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue("Xác Đan Nghịch Thiên", resText, new List<string> { "Đành chịu vậy..." }, (idx) => onComplete?.Invoke());
                    }
                    else
                    {
                        onComplete?.Invoke();
                    }
                });
            }
            else
            {
                string title = "Vực Thẳm Dị Biến";
                string text = "Đội ngũ Thần Miêu vô tình giẫm phải một mạch khoáng linh thạch bị ô nhiễm nặng nề. Linh lực bạo tàn cuộn trào vây kín!\n\n" +
                              "Lực lượng này sẽ ép buộc một chú mèo trong đội tiếp nhận Đột biến linh khí bất ổn.\n\n" +
                              "Ai sẽ đứng ra chịu đựng luồng sức mạnh cuồng bạo này?";

                var choices = manager.ActiveCats.Select(c => c.Name).ToList();

                Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, choices, (choiceIdx) =>
                {
                    var cat = manager.ActiveCats[choiceIdx];

                    string[] possibleMutations = { 
                        UnstableMutation.UnstableClaws, 
                        UnstableMutation.LethargicNap, 
                        UnstableMutation.CursedFur 
                    };
                    string mutation = possibleMutations[UnityEngine.Random.Range(0, possibleMutations.Length)];
                    
                    cat.AddMutation(mutation);
                    cat.AddMemoir(MemoirType.Mutation, UnstableMutation.GetDisplayName(mutation), "Cường hành nạp linh bùng phát dị biến");
                    runState.AddCorruption(15);

                    string resText = $"<b>{cat.Name}</b> đã cắn răng tiếp thụ luồng linh lực cuồng bạo!\n\n" +
                                    $"Hậu quả dị biến: <b><color=red>{UnstableMutation.GetDisplayName(mutation)}</color></b>\n" +
                                    $"<i>{UnstableMutation.GetDescription(mutation)}</i>";

                    Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue("Dị Biến Kết Thúc", resText, new List<string> { "Tiếp tục" }, (idx) =>
                    {
                        onComplete?.Invoke();
                    });
                });
            }
        }
    }

    public class GatheringRoomHelper : MonoBehaviour
    {
        public Action OnFinished;

        private void OnGUI()
        {
            // Use curated dark aesthetics for the button
            GUI.backgroundColor = new Color(0.12f, 0.12f, 0.16f, 0.95f);
            GUI.contentColor = new Color(0.1f, 0.8f, 0.3f, 1f); // Vibrant green

            Rect buttonRect = new Rect((Screen.width - 280f) / 2f, Screen.height - 90f, 280f, 50f);
            
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            if (GUI.Button(buttonRect, "⚔️ HOÀN THÀNH THU THẬP", buttonStyle))
            {
                // Freeze the board again
                Time.timeScale = 0f;
                
                OnFinished?.Invoke();
                
                Destroy(gameObject);
            }
        }
    }

    public class EliteEncounter : IExpeditionEncounter
    {
        private int _layer;

        public EliteEncounter(int layer)
        {
            _layer = layer;
        }

        public void Resolve(Action onComplete)
        {
            var manager = ExpeditionManager.Instance;
            var runState = manager.RunState;

            List<Combatable> enemies = new List<Combatable>();
            Vector3 spawnPos = Vector3.zero;

            string enemyId = "skeleton_mage"; 
            if (_layer <= 2) enemyId = "demon";
            else enemyId = "boss_goblin_king";

            var enemyCard = WorldManager.instance.CreateCard(spawnPos, enemyId, false, false, false);
            if (enemyCard != null && enemyCard.CardData is Combatable comb)
            {
                float scaleFactor = 1.8f * (1.0f + (runState.GreedLevel / 10f) * 0.05f);
                comb.BaseCombatStats.MaxHealth = Mathf.RoundToInt(comb.BaseCombatStats.MaxHealth * scaleFactor);
                comb.HealthPoints = comb.BaseCombatStats.MaxHealth;
                comb.BaseCombatStats.AttackDamage = Mathf.RoundToInt(comb.BaseCombatStats.AttackDamage * scaleFactor);
                comb.BaseCombatStats.Speed += 15;
                comb.CustomName = $"🔥 Cương Giả {comb.Name}";
                enemies.Add(comb);
            }

            Debug.LogWarning($"[Expedition] Bắt đầu trận chiến CƯƠNG GIẢ (ELITE)! Quái vật được tăng 1.8x HP & sát thương.");

            List<Combatable> playerCombats = manager.ActiveCats.Cast<Combatable>().ToList();
            TurnBasedCombatManager.Instance.StartCombat(playerCombats, enemies, (result) =>
            {
                foreach (var enemy in enemies)
                {
                    if (enemy != null && enemy.MyGameCard != null)
                    {
                        enemy.MyGameCard.DestroyCard(true, true);
                    }
                }

                if (result == CombatResult.Victory)
                {
                    string[] rareLoot = { "item_breakthrough_pill", "talisman_heavy_armor", "talisman_rage_core", "talisman_health_regen" };
                    string rolled = rareLoot[UnityEngine.Random.Range(0, rareLoot.Length)];
                    manager.CurrentBackpack.AddItem(rolled);
                    
                    manager.CurrentBackpack.AddItem("resource_gold");
                    manager.CurrentBackpack.AddItem("resource_gold");

                    runState.AddCorruption(20);

                    foreach (var cat in manager.ActiveCats)
                    {
                        cat.AddMemoir(MemoirType.BossKill, "Hạ Cương Giả", "Trảm sát Cương Giả nhận bùa chú");
                    }

                    string title = "⚔️ CƯƠNG GIẢ PHÁT BẠI";
                    string text = $"Chúc mừng! Toàn đội đã tiêu diệt thành công Cương Giả hộ vệ.\n\n" +
                                  $"Thu về linh bảo: <b>{rolled.Replace("item_", "").Replace("talisman_", "").ToUpper()}</b> và Vàng.";

                    Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { "Thu hoạch và Đi tiếp" }, (idx) =>
                    {
                        onComplete?.Invoke();
                    });
                }
                else
                {
                    manager.ReturnToBase(isDefeat: true);
                }
            });
        }
    }

    public class ExtractionEncounter : IExpeditionEncounter
    {
        public void Resolve(Action onComplete)
        {
            var manager = ExpeditionManager.Instance;

            string title = "🌀 CỔNG TRỤC XUẤT CỔ ĐẠI";
            string text = "Trước mắt bạn là một Cổng Trục Xuất phát ra hào quang dịu nhẹ. Cổng này cho phép toàn đội kết thúc viễn chinh sớm và đem toàn bộ đồ vật trong Balo về Base an toàn.\n\n" +
                          "Bạn muốn làm gì?";

            var choices = new List<string> { "🌀 Trục xuất về Base (An toàn 100% loot)", "Rời đi, tiếp tục viễn chinh" };

            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, choices, (idx) =>
            {
                if (idx == 0)
                {
                    manager.ReturnToBase(isDefeat: false);
                }
                else
                {
                    onComplete?.Invoke();
                }
            });
        }
    }

    public class SafeRetreatEncounter : IExpeditionEncounter
    {
        public void Resolve(Action onComplete)
        {
            var manager = ExpeditionManager.Instance;

            string title = "⛺ ẨN TRÁNH CỔ LỘ (TRẠM NGHỈ)";
            string text = "Một hang động ẩn khuất tự nhiên cực kỳ an toàn. Nơi này linh lực ôn hòa, tránh xa mọi ma thú và ô nhiễm thiên địa.\n\n" +
                          "Toàn đội có thể chọn nghỉ ngơi để hồi phục thể trạng hoặc dưỡng thương.";

            var choices = new List<string> { "💖 Nghỉ ngơi hồi phục (+15 HP cho toàn đội)", "Rời đi bình thường" };

            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, choices, (idx) =>
            {
                if (idx == 0)
                {
                    foreach (var cat in manager.ActiveCats)
                    {
                        cat.HealthPoints = Mathf.Min(cat.ProcessedCombatStats.MaxHealth, cat.HealthPoints + 15);
                    }
                    Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue("⛺ Dưỡng Thương Hoàn Thành", "Toàn đội mèo phục hồi kinh mạch (+15 HP)!", new List<string> { "Đồng ý" }, (i) => onComplete?.Invoke());
                }
                else
                {
                    onComplete?.Invoke();
                }
            });
        }
    }
}
