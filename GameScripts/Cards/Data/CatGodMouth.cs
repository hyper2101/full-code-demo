using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CatGodMouth : CardData
{
    [Header("Cat God Mouth Settings")]
    [ExtraData("offering_progress")]
    public int OfferingProgress = 0;

    [ExtraData("consecutive_trash")]
    public int ConsecutiveTrash = 0;

    public override bool DetermineCanHaveCardsWhenIsRoot
    {
        get
        {
            return true; // Can have cards stacked on top
        }
    }

    public int GetAppeasementGreed()
    {
        if (Mewtations.Expedition.ExpeditionManager.Instance != null && Mewtations.Expedition.ExpeditionManager.Instance.RunState != null)
        {
            return Mewtations.Expedition.ExpeditionManager.Instance.RunState.BaseAppeasementGreed;
        }
        return 0;
    }

    public int GetAppeasementCorruption()
    {
        if (Mewtations.Expedition.ExpeditionManager.Instance != null && Mewtations.Expedition.ExpeditionManager.Instance.RunState != null)
        {
            return Mewtations.Expedition.ExpeditionManager.Instance.RunState.BaseAppeasementCorruption;
        }
        return 0;
    }

    public override void UpdateCard()
    {
        this.descriptionOverride = "Kéo thẻ bài vào Miệng Thần Mèo để hiến tế vật phẩm, xoa dịu thiên địa hoặc đổi lấy thiên cơ báu vật ngẫu nhiên.\n\n" +
                                   $"<b>Linh lực tích lũy:</b> <color=#ffdd22>{OfferingProgress}</color>\n" +
                                   $"<b>Mức độ Xoa Dịu hiện tại:</b>\n" +
                                   $"• Xoa dịu Tham Lam: {GetAppeasementGreed()} điểm\n" +
                                   $"• Xoa dịu Ô Nhiễm: {GetAppeasementCorruption()} điểm";

        base.UpdateCard();

        // If a card is stacked on top and timer is not running, start consuming the offering
        if (this.MyGameCard != null && !this.MyGameCard.TimerRunning && this.MyGameCard.HasChild)
        {
            CardData childData = this.MyGameCard.Child.CardData;
            // Do not consume other players/cats! Only consume item, food, resource cards
            if (childData.MyCardType != CardType.Humans && !(childData is CatCardData))
            {
                this.MyGameCard.StartTimer(2.0f, new TimerAction(this.ConsumeOffering), "Tiếp nhận Lễ Vật...", "offering");
            }
        }
    }

    private void ConsumeOffering()
    {
        if (this.MyGameCard == null || !this.MyGameCard.HasChild) return;

        GameCard offeringCard = this.MyGameCard.Child;
        if (offeringCard == null || offeringCard.CardData == null) return;
        CardData offeringData = offeringCard.CardData;

        // Calculate offering value: use custom value if set, else base gold value * 2
        int val = offeringData.HiddenOfferingValue;
        if (val <= 0)
        {
            val = offeringData.Value * 2;
        }
        if (val <= 0) val = 1; // Minimum 1 progress

        OfferingProgress += val;

        // Dynamic Greed shift for rare items
        if (offeringData.Value >= 15 || offeringData.Id.Contains("pill") || offeringData.Id.Contains("relic"))
        {
            ConsecutiveTrash = 0;
            if (Mewtations.Expedition.ExpeditionManager.Instance != null && Mewtations.Expedition.ExpeditionManager.Instance.RunState != null)
            {
                Mewtations.Expedition.ExpeditionManager.Instance.RunState.GreedLevel = Mathf.Max(0, Mewtations.Expedition.ExpeditionManager.Instance.RunState.GreedLevel - 15);
                Debug.Log($"[CatGodMouth] Hiến tế bảo vật hiếm: Giảm -15 Greed khí vận toàn cục.");
            }
        }
        else if (offeringData.Value <= 2)
        {
            ConsecutiveTrash++;
            if (ConsecutiveTrash >= 3)
            {
                ConsecutiveTrash = 0;
                if (Mewtations.Expedition.ExpeditionManager.Instance != null && Mewtations.Expedition.ExpeditionManager.Instance.RunState != null)
                {
                    Mewtations.Expedition.ExpeditionManager.Instance.RunState.GreedLevel = Mathf.Min(100, Mewtations.Expedition.ExpeditionManager.Instance.RunState.GreedLevel + 20);
                    Debug.Log($"[CatGodMouth] Liên tục dâng rác thải! Tăng +20 Greed ma khí toàn cục.");
                }
                TriggerCosmicTemptation();
            }
        }
        else
        {
            ConsecutiveTrash = 0;
        }

        // Accumulate Base Sacrifice points if within Expedition
        if (Mewtations.Expedition.ExpeditionManager.Instance != null && Mewtations.Expedition.ExpeditionManager.Instance.RunState != null)
        {
            string lowerId = offeringData.Id.ToLower();
            int points = Mathf.Max(1, val / 2);
            if (lowerId.Contains("gold") || lowerId.Contains("ore") || lowerId.Contains("stone") || lowerId.Contains("wood"))
            {
                Mewtations.Expedition.ExpeditionManager.Instance.RunState.BaseAppeasementGreed += points;
                Debug.Log($"[CatGodMouth] Hiến tế khoáng sản/kim loại: Tích lũy {points} điểm Xoa Dịu Tham Lam (Base Greed Appeasement).");
            }
            else if (offeringData.MyCardType == CardType.Food || lowerId.Contains("food") || lowerId.Contains("potion") || lowerId.Contains("pill"))
            {
                Mewtations.Expedition.ExpeditionManager.Instance.RunState.BaseAppeasementCorruption += points;
                Debug.Log($"[CatGodMouth] Hiến tế lương thực/linh dược: Tích lũy {points} điểm Xoa Dịu Ô Nhiễm (Base Corruption Appeasement).");
            }
        }
        
        // Destroy the consumed card
        offeringCard.DestroyCard(true, true);

        // Spawn beautiful offering sparkle visual using simple debug or log
        Debug.Log($"[CatGodMouth] Đã dâng tế {offeringData.Name}. Nhận {val} Linh lực. Tiến độ: {OfferingProgress}.");

        CheckThresholdRewards();
    }

    private void CheckThresholdRewards()
    {
        Vector3 spawnPos = this.transform.position + Vector3.back * 1.0f;
        string rewardId = "";
        string rewardName = "";

        // Check milestones
        if (OfferingProgress >= 700)
        {
            OfferingProgress = 0; // Reset progress

            // 15% chance to award a unique Heavenly Talent
            float roll = UnityEngine.Random.value;
            if (roll <= 0.15f)
            {
                var cats = new List<CatCardData>();
                foreach (var gc in WorldManager.instance.AllCards)
                {
                    if (gc != null && gc.CardData is CatCardData c) cats.Add(c);
                }

                if (cats.Count > 0)
                {
                    var cat = cats[UnityEngine.Random.Range(0, cats.Count)];
                    string[] talents = new string[] {
                        Mewtations.Expedition.HeavenlyTalent.HeavenlyPoisonBody,
                        Mewtations.Expedition.HeavenlyTalent.DivineShieldProtection,
                        Mewtations.Expedition.HeavenlyTalent.RageOvercharger,
                        Mewtations.Expedition.HeavenlyTalent.MartialArtsCleave
                    };
                    string chosenTalent = talents[UnityEngine.Random.Range(0, talents.Length)];
                    cat.AddTrait(chosenTalent);

                    string title = "THIÊN PHÚ THỨC TỈNH!";
                    string text = $"Sự thành tâm vô bờ bến đã cảm động Thần Mèo!\n\n" +
                                  $"Thần Mèo phóng xuất linh quang, tẩy tủy và khai thông kinh mạch cho <b>{cat.Name}</b>!\n" +
                                  $"🌟 <b>{cat.Name}</b> đã thức tỉnh Thiên Phú Thiên Kiêu: <b><color=#ffcc00>{chosenTalent}</color></b>!";

                    if (Mewtations.Dialogue.DialogueSystem.Instance != null)
                    {
                        Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { "Đại cát đại lợi!" }, (choiceIdx) => { });
                    }
                    return;
                }
            }

            // 1% chance for highly-sought relic item
            if (roll <= 0.16f && roll > 0.15f)
            {
                rewardId = "item_heavenly_relic";
                rewardName = "Chí Tôn Cổ Khí (1% Cực Hiếm)";
            }
            else
            {
                rewardId = "cat_basic"; // Rare Heavenly Talent Cat
                rewardName = "Một Thần Miêu Mới (Được gia trì Thiên Kiêu)";
            }
        }
        else if (OfferingProgress >= 350)
        {
            rewardId = "item_breakthrough_pill";
            rewardName = "Linh Đan Đột Phá";
        }
        else if (OfferingProgress >= 150)
        {
            rewardId = "item_revive_pill";
            rewardName = "Linh Đan Hồi Sinh";
        }
        else if (OfferingProgress >= 50)
        {
            // Give 3-5 gold coins
            rewardId = "resource_gold";
            rewardName = "Túi Tiền Vàng của Thần Mèo";
        }

        if (!string.IsNullOrEmpty(rewardId))
        {
            if (rewardId == "resource_gold")
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector3 jitter = spawnPos + new Vector3(UnityEngine.Random.Range(-0.3f, 0.3f), 0, UnityEngine.Random.Range(-0.3f, 0.3f));
                    WorldManager.instance.CreateCard(jitter, rewardId, true, true, true);
                }
            }
            else if (rewardId == "cat_basic")
            {
                var summoning = new CatSummoningSystem(WorldManager.instance);
                // Guaranteed breakthrough level 1
                summoning.SummonCat(spawnPos, highestBreakthroughLevel: 1); 
            }
            else
            {
                WorldManager.instance.CreateCard(spawnPos, rewardId, true, true, true);
            }

            // Deduct threshold (except 700 which resets)
            if (rewardId != "cat_basic" && rewardId != "item_heavenly_relic")
            {
                if (rewardId == "item_breakthrough_pill") OfferingProgress -= 350;
                else if (rewardId == "item_revive_pill") OfferingProgress -= 150;
                else OfferingProgress -= 50;
            }

            // Visual celebration in DialogueSystem
            string title = "PHÚC LÀNH CỦA THẦN MÈO!";
            string text = $"Thanh tiến độ hiến tế linh lực đã chạm ngưỡng tích lũy tôn nghiêm!\n\n" +
                          $"Thần Mèo hiển linh và ban tặng phần thưởng cao quý:\n" +
                          $"🌟 <b><color=#ffdd22>{rewardName}</color></b>!\n\n" +
                          $"Linh lực hiện tại còn lại: {OfferingProgress}.";

            if (Mewtations.Dialogue.DialogueSystem.Instance != null)
            {
                Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { "Tạ ơn Thần Mèo!" }, (choiceIdx) => { });
            }
        }
    }

    private void TriggerCosmicTemptation()
    {
        string title = "⚠️ CÁM DỖ TÂM MA / TÀ THẦN PHẪN NỘ";
        string text = "Một khe nứt hư không đen tối mở ra từ Miệng Thần Mèo! Tà linh cổ xưa thì thầm đầy phẫn nộ vì những cống phẩm rác rưởi mà ngươi dâng hiến:\n\n" +
                      "<i>\"Phàm nhân ngu muội! Ngươi dám sỉ nhục tôn nghiêm của thần linh bằng đống phế phẩm này sao? Hãy hiến tế sinh cơ hoặc gánh chịu nghiệp chướng trừng phạt!\"</i>";

        // Check if player has 10 gold on board
        var goldCards = new List<GameCard>();
        foreach (var gc in WorldManager.instance.AllCards)
        {
            if (gc != null && gc.CardData.Id == "resource_gold")
            {
                goldCards.Add(gc);
            }
        }
        bool hasEnoughGold = goldCards.Count >= 10;

        var choices = new List<Mewtations.Dialogue.DialogueChoice>();

        choices.Add(new Mewtations.Dialogue.DialogueChoice(
            "Dâng hiến 10 Vàng để xoa dịu.",
            () => {
                // Destroy 10 gold coins
                int destroyed = 0;
                for (int i = goldCards.Count - 1; i >= 0 && destroyed < 10; i--)
                {
                    if (goldCards[i] != null)
                    {
                        goldCards[i].DestroyCard(true, true);
                        destroyed++;
                    }
                }
                
                string subTitle = "THẦN DUNG THỨ";
                string subText = "Thần Mèo nuốt chửng số vàng cúng tế và dần yên vị trở lại. Bầu không khí tà ác tan biến, nhưng ma khí vẫn để lại dư âm âm ỉ...";
                if (Mewtations.Dialogue.DialogueSystem.Instance != null)
                {
                    Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(subTitle, subText, new List<string> { "Hú vía!" }, (cIdx) => {});
                }
            },
            () => hasEnoughGold,
            "Cần 10 Vàng cúng tế"
        ));

        choices.Add(new Mewtations.Dialogue.DialogueChoice(
            "Từ chối! Ta tự gánh nghiệp chướng.",
            () => {
                // Increases global Greed and inflicts unstable mutation
                if (Mewtations.Expedition.ExpeditionManager.Instance != null && Mewtations.Expedition.ExpeditionManager.Instance.RunState != null)
                {
                    Mewtations.Expedition.ExpeditionManager.Instance.RunState.GreedLevel = Mathf.Min(100, Mewtations.Expedition.ExpeditionManager.Instance.RunState.GreedLevel + 25);
                }

                // Add random unstable mutation to a random cat
                var cats = new List<CatCardData>();
                foreach (var gc in WorldManager.instance.AllCards)
                {
                    if (gc != null && gc.CardData is CatCardData c) cats.Add(c);
                }

                string affectedCatName = "phàm nhân";
                if (cats.Count > 0)
                {
                    var cat = cats[UnityEngine.Random.Range(0, cats.Count)];
                    string[] mutations = new string[] {
                        Mewtations.Expedition.UnstableMutation.UnstableClaws,
                        Mewtations.Expedition.UnstableMutation.LethargicNap,
                        Mewtations.Expedition.UnstableMutation.CursedFur
                    };
                    string chosenMut = mutations[UnityEngine.Random.Range(0, mutations.Length)];
                    cat.AddMutation(chosenMut);
                    affectedCatName = cat.Name;
                }

                string subTitle = "NGHIỆP CHƯỚNG QUẤN THÂN";
                string subText = $"Tà thần gầm thét! Hư không bạo phát tà khí dày đặc làm tăng phế ma khí (+25 Greed toàn cục)!\n\n" +
                                 $"<b>{affectedCatName}</b> đã gánh chịu hình phạt Dị Biến ma hóa linh căn!";
                if (Mewtations.Dialogue.DialogueSystem.Instance != null)
                {
                    Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(subTitle, subText, new List<string> { "Đương đầu ma kiếp!" }, (cIdx) => {});
                }
            }
        ));

        if (Mewtations.Dialogue.DialogueSystem.Instance != null)
        {
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, choices);
        }
    }

    protected override bool CanHaveCard(CardData otherCard)
    {
        // Don't stack human/cat cards directly, only stack resource/item cards
        if (otherCard.MyCardType == CardType.Humans || otherCard is CatCardData)
        {
            return false;
        }
        return base.CanHaveCard(otherCard);
    }
}
