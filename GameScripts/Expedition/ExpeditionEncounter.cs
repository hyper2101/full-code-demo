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

            for (int i = 0; i < lootCount; i++)
            {
                string loot = possibleLoot[UnityEngine.Random.Range(0, possibleLoot.Length)];
                manager.CurrentBackpack.AddItem(loot);
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
                          "• <b>Khai thác chuẩn mực:</b> Nhận tài nguyên cơ bản an toàn.\n" +
                          "• <b>Khai thác cạn kiệt (Tham lam):</b> Nhận gấp đôi tài nguyên nhưng tăng <b>+20 Greed</b> (Quái vật tầng sâu mạnh lên).";

            var choices = new List<string> { "Khai thác chuẩn mực", "Khai thác cạn kiệt (+20 Greed)" };
            
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, choices, (choiceIdx) =>
            {
                int harvests = (choiceIdx == 1) ? 2 : 1;
                if (choiceIdx == 1)
                {
                    runState.AddGreed(20);
                }

                int lootCount = UnityEngine.Random.Range(2, 4) * harvests;
                string[] resources = { "resource_food", "item_wood", "item_stone", "resource_gold", "item_iron_ore" };

                List<string> added = new List<string>();
                for (int i = 0; i < lootCount; i++)
                {
                    string res = resources[UnityEngine.Random.Range(0, resources.Length)];
                    if (manager.CurrentBackpack.AddItem(res))
                    {
                        added.Add(res);
                    }
                }

                string resMsg = added.Count > 0 
                    ? string.Join(", ", added.Select(id => id.Replace("resource_", "").Replace("item_", "")))
                    : "Không có đủ khoảng trống balo!";

                runState.AddCorruption(15);

                string resTitle = (choiceIdx == 1) ? "Khai Thác Cạn Kiệt!" : "Khai Thác Hoàn Tất";
                string resText = (choiceIdx == 1)
                    ? $"Sự tham lam thúc đẩy bạn đào bới đến tận gốc rễ!\n\nNhận được lượng tài nguyên dồi dào: {resMsg}\n\nGreed tăng lên: <b>{runState.GreedLevel}/100</b>."
                    : $"Khai thác ôn hòa an toàn.\n\nNhận được: {resMsg}";

                Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(resTitle, resText, new List<string> { "Tiếp tục" }, (idx) =>
                {
                    onComplete?.Invoke();
                });
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

            string title = "Miệng Thần Mèo - Hiến Tế Đàn";
            string text = "Một khe vực khổng lồ mở ra, tạo hình như một chiếc Miệng Thần Mèo đói khát cổ xưa. Linh khí từ miệng vực thốt ra rợn người.\n\n" +
                          "Để đổi lấy sự bình yên hoặc linh căn thiên phú, thần linh đòi hỏi một sự đánh đổi công bằng.\n\n" +
                          $"Balo hiện tại có: <b>{backpack.ContainedCardIds.Count} vật phẩm</b>.";

            var choices = new List<string> { "Thanh tẩy Ô Nhiễm (-30 Corruption - Cần 2 vật phẩm)", "Hối lộ Luật Pháp (-35 Greed - Cần 2 vật phẩm)", "Cầu nguyện Thiên Phú (Ngẫu nhiên Thiên Kiêu - Tăng +25 Corruption)", "Rời đi bình yên" };

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
                    runState.AddCorruption(25);

                    string talentDesc = $"★ {cat.Name} được ban phúc thành tựu <b>{HeavenlyTalent.GetDisplayName(rolledTalent)}</b>!\n\nCorruption tăng thêm +25% vì dám cướp đoạt sinh cơ.";

                    Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue("Linh Căn Tẩy Tủy", talentDesc, new List<string> { "Nhận Thần Lực!" }, (idx) => onComplete?.Invoke());
                }
                else
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
