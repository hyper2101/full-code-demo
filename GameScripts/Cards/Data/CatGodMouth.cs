using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CatGodState { Idle, OfferingPull, Consume, RewardSpit, Anger }

public class CatGodMouth : CardData
{
    [Header("Cat God Mouth Settings")]
    public CatGodState CurrentState = CatGodState.Idle;

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

    private int _lastOfferingProgress = -1;
    private int _lastAppeasementGreed = -1;
    private int _lastAppeasementCorruption = -1;

    public override void UpdateCard()
    {
        int currentGreed = GetAppeasementGreed();
        int currentCorruption = GetAppeasementCorruption();

        if (OfferingProgress != _lastOfferingProgress ||
            currentGreed != _lastAppeasementGreed ||
            currentCorruption != _lastAppeasementCorruption)
        {
            _lastOfferingProgress = OfferingProgress;
            _lastAppeasementGreed = currentGreed;
            _lastAppeasementCorruption = currentCorruption;

            this.descriptionOverride = "Kéo thẻ bài vào Miệng Thần Mèo để hiến tế vật phẩm, xoa dịu thiên địa hoặc đổi lấy thiên cơ báu vật ngẫu nhiên.\n\n" +
                                       $"<b>Linh lực tích lũy:</b> <color=#ffdd22>{OfferingProgress}</color>\n" +
                                       $"<b>Mức độ Xoa Dịu hiện tại:</b>\n" +
                                       $"• Xoa dịu Tham Lam: {currentGreed} điểm\n" +
                                       $"• Xoa dịu Ô Nhiễm: {currentCorruption} điểm";
        }

        base.UpdateCard();
 
        // If a card is stacked on top and timer is not running, start consuming the offering
        if (this.MyGameCard != null)
        {
            // Intercept for Meridian Cure ritual (Tẩu Hỏa Nhập Ma)
            if (this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == "meridian_cure")
            {
                if (!this.MyGameCard.HasChild || !(this.MyGameCard.Child.CardData is CatCardData cat) || 
                    (!cat.IsPillSlotLocked && !cat.IsFoodSlotLocked && !cat.IsPassiveSlotsLocked && !cat.IsEquipmentSlotsLocked))
                {
                    this.MyGameCard.CancelTimer("meridian_cure");
                }
            }
            else if (!this.MyGameCard.TimerRunning && this.MyGameCard.HasChild && this.MyGameCard.Child.CardData is CatCardData cat && 
                     (cat.IsPillSlotLocked || cat.IsFoodSlotLocked || cat.IsPassiveSlotsLocked || cat.IsEquipmentSlotsLocked))
            {
                this.MyGameCard.StartTimer(5.0f, new TimerAction(this.PerformMeridianCure), "Nghi Lễ Hộ Mệnh Trị Liệu...", "meridian_cure");
            }
            // Intercept for Scar Cleansing ritual
            else if (this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == "cleansing")
            {
                if (!this.MyGameCard.HasChild || !(this.MyGameCard.Child.CardData is CatCardData c) || c.PermanentScars.Count == 0)
                {
                    this.MyGameCard.CancelTimer("cleansing");
                }
            }
            else if (!this.MyGameCard.TimerRunning && this.MyGameCard.HasChild && this.MyGameCard.Child.CardData is CatCardData c && c.PermanentScars.Count > 0)
            {
                this.MyGameCard.StartTimer(5.0f, new TimerAction(this.PerformScarCleansing), "Nghi Lễ Tẩy Tủy Sẹo...", "cleansing");
            }
            // Standard offering consume
            else if (this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == "offering")
            {
                if (!this.MyGameCard.HasChild || this.MyGameCard.Child.CardData.MyCardType == CardType.Humans || this.MyGameCard.Child.CardData is CatCardData)
                {
                    if (this.MyGameCard.HasChild)
                    {
                        this.MyGameCard.Child.transform.localScale = Vector3.one;
                    }
                    this.MyGameCard.CancelTimer("offering");
                    CurrentState = CatGodState.Idle;
                }
                else
                {
                    CurrentState = CatGodState.OfferingPull;
                    // offering pull animation: scale card down and pull towards center
                    float t = Mathf.Clamp01(this.MyGameCard.CurrentTimerTime / 2.0f);
                    float scale = 1.0f - 0.8f * t;
                    this.MyGameCard.Child.transform.localScale = new Vector3(scale, scale, scale);
                    this.MyGameCard.Child.transform.position = Vector3.Lerp(this.MyGameCard.Child.transform.position, this.transform.position, 0.12f);
                }
            }
            else if (!this.MyGameCard.TimerRunning && this.MyGameCard.HasChild)
            {
                CardData childData = this.MyGameCard.Child.CardData;
                // Do not consume other players/cats! Only consume item, food, resource cards
                if (childData.MyCardType != CardType.Humans && !(childData is CatCardData))
                {
                    CurrentState = CatGodState.OfferingPull;
                    this.MyGameCard.StartTimer(2.0f, new TimerAction(this.ConsumeOffering), "Tiếp nhận Lễ Vật...", "offering");
                }
                else
                {
                    CurrentState = CatGodState.Idle;
                }
            }
            else
            {
                CurrentState = CatGodState.Idle;
            }
        }
    }

    private void PerformMeridianCure()
    {
        if (this.MyGameCard == null || !this.MyGameCard.HasChild || !(this.MyGameCard.Child.CardData is CatCardData cat)) return;

        string title = "☯️ NGHI LỄ HỘ MỆNH TRỊ LIỆU KINH MẠCH";
        string text = $"Thần Miêu <b>{cat.Name}</b> bị tẩu hỏa nhập ma, bế tắc linh mạch nghiêm trọng sau lôi kiếp đột phá thất bại.\n\n" +
                      $"Linh khí bạo phát đòi hỏi cúng tế vàng và linh dược cụ thể để hồi phục mạch tượng hoàn hảo:\n\n" +
                      $"• <b>Tế phẩm yêu cầu:</b> 15 Vàng & 1 Thuốc hồi máu (`item_healing_potion`).\n" +
                      $"• <b>Hiệu quả:</b> Gỡ bỏ hoàn toàn tình trạng bế tắc, giải phóng tất cả các ô bị khóa an toàn 100%!";

        // Tìm tất cả vàng và thuốc trên bàn
        var goldCards = new List<GameCard>();
        var potionCards = new List<GameCard>();

        foreach (var gc in WorldManager.instance.AllCards)
        {
            if (gc != null && !gc.Destroyed)
            {
                if (gc.CardData.Id == "resource_gold") goldCards.Add(gc);
                else if (gc.CardData.Id.ToLower() == "item_healing_potion") potionCards.Add(gc);
            }
        }

        bool hasResources = goldCards.Count >= 15 && potionCards.Count >= 1;

        var choices = new List<Mewtations.Dialogue.DialogueChoice>();

        choices.Add(new Mewtations.Dialogue.DialogueChoice(
            "Cúng tế 15 Vàng & 1 Thuốc hồi máu để trị liệu.",
            () => {
                // Tiêu hủy 15 vàng
                int destroyedGold = 0;
                for (int i = goldCards.Count - 1; i >= 0 && destroyedGold < 15; i--)
                {
                    if (goldCards[i] != null && !goldCards[i].Destroyed)
                    {
                        goldCards[i].DestroyCard(true, true);
                        destroyedGold++;
                    }
                }

                // Tiêu hủy 1 thuốc
                if (potionCards.Count > 0 && potionCards[0] != null && !potionCards[0].Destroyed)
                {
                    potionCards[0].DestroyCard(true, true);
                }

                // Chữa trị thành công!
                cat.IsPillSlotLocked = false;
                cat.IsFoodSlotLocked = false;
                cat.IsPassiveSlotsLocked = false;
                cat.IsEquipmentSlotsLocked = false;

                string subTitle = "☯️ KINH MẠCH KHAI THÔNG!";
                string subText = $"Dược lực bùng nổ kết hợp với linh lực cúng tế đã gột rửa hoàn toàn các bế tắc trong kinh mạch của <b>{cat.Name}</b>!\n\n" +
                                 $"🌟 Mọi ô chứa bị khóa đã được mở khóa an toàn. <b>{cat.Name}</b> đã khôi phục mạch tượng hoàn hảo để tiếp tục tu luyện!";
                
                if (Mewtations.Dialogue.DialogueSystem.Instance != null)
                {
                    Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(subTitle, subText, new List<string> { "Đại cát đại lợi!" }, (cIdx) => {});
                }
            },
            () => hasResources,
            "Cần 15 Vàng & 1 Thuốc hồi máu"
        ));

        choices.Add(new Mewtations.Dialogue.DialogueChoice(
            "Rút lui",
            () => {}
        ));

        if (Mewtations.Dialogue.DialogueSystem.Instance != null)
        {
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, choices);
        }
    }

    private void PerformScarCleansing()
    {
        if (this.MyGameCard == null || !this.MyGameCard.HasChild || !(this.MyGameCard.Child.CardData is CatCardData cat)) return;

        string title = "☯️ NGHI LỄ TẨY TỦY SẸO";
        string text = $"Thần Miêu <b>{cat.Name}</b> sở hữu linh thể mang thương tổn nặng nề (Vết sẹo vĩnh cửu) đang thành kính quỳ trước Miệng Thần Mèo.\n\n" +
                      $"Tà linh cổ xưa thì thầm đòi hỏi cống nạp một số lượng tiền vàng khổng lồ (30 Vàng) để nghịch chuyển linh lực, tái sinh kinh mạch.\n\n" +
                      $"• <b>Tỷ lệ tẩy sẹo thành công:</b> 50%.\n" +
                      $"• <b>Hình phạt nếu thất bại:</b> Ma khí phản phệ dữ dội bạo phát <b>+40 Greed</b> toàn cục và khiến <b>{cat.Name}</b> gánh thêm một Vết sẹo phế mạch mới!";

        // Find all gold on the board
        var goldCards = new List<GameCard>();
        foreach (var gc in WorldManager.instance.AllCards)
        {
            if (gc != null && gc.CardData.Id == "resource_gold")
            {
                goldCards.Add(gc);
            }
        }
        bool hasEnoughGold = goldCards.Count >= 30;

        var choices = new List<Mewtations.Dialogue.DialogueChoice>();

        choices.Add(new Mewtations.Dialogue.DialogueChoice(
            "Cúng tế 30 Vàng để tẩy tủy.",
            () => {
                // Destroy 30 gold
                int destroyed = 0;
                for (int i = goldCards.Count - 1; i >= 0 && destroyed < 30; i--)
                {
                    if (goldCards[i] != null && !goldCards[i].Destroyed)
                    {
                        goldCards[i].DestroyCard(true, true);
                        destroyed++;
                    }
                }

                if (UnityEngine.Random.value <= 0.50f)
                {
                    // Success! Clear scars!
                    cat.PermanentScarsString = "";
                    string subTitle = "☯️ TẨY TỦY THÀNH CÔNG!";
                    string subText = $"Thần Mèo chấp nhận tế phẩm! Linh quang tím chói lòa chiếu rọi, tái sinh linh thể cho <b>{cat.Name}</b>. Toàn bộ các vết sẹo vĩnh cửu đã được gột rửa hoàn toàn!";
                    if (Mewtations.Dialogue.DialogueSystem.Instance != null)
                    {
                        Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(subTitle, subText, new List<string> { "Tạ ơn Thần Mèo!" }, (cIdx) => {});
                    }
                }
                else
                {
                    // Failure!
                    if (Mewtations.Expedition.ExpeditionManager.Instance != null && Mewtations.Expedition.ExpeditionManager.Instance.RunState != null)
                    {
                        Mewtations.Expedition.ExpeditionManager.Instance.RunState.GreedLevel = Mathf.Min(100, Mewtations.Expedition.ExpeditionManager.Instance.RunState.GreedLevel + 40);
                    }

                    // Add a crippling scar
                    cat.AddScar(Mewtations.Combat.PermanentScar.CrippledMeridians);

                    string subTitle = "☠️ NGHI THỨC THẤT BẠI!";
                    string subText = $"Thần Mèo nổi giận nuốt chửng 30 Vàng nhưng ma lực phản phệ dữ dội! \n\n" +
                                     $"• Sức ép lòng tham gia tăng: <b>+40 Greed</b> toàn cục.\n" +
                                     $"• <b>{cat.Name}</b> gánh chịu chấn thương linh mạch nghiêm trọng hơn, nhận thêm Vết sẹo: <b><color=red>Phế Mạch (-30 Speed)</color></b>!";
                    
                    if (Mewtations.Dialogue.DialogueSystem.Instance != null)
                    {
                        Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(subTitle, subText, new List<string> { "Đương đầu tai ách" }, (cIdx) => {});
                    }
                }
            },
            () => hasEnoughGold,
            "Cần 30 Vàng để cúng tế"
        ));

        choices.Add(new Mewtations.Dialogue.DialogueChoice(
            "Rút lui an toàn",
            () => {
                // Return safely
            }
        ));

        if (Mewtations.Dialogue.DialogueSystem.Instance != null)
        {
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, choices);
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
        if (offeringData.Value >= 15 || offeringData.IsCultivationPill || offeringData.IsAncientRelic)
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
            else if (offeringData.MyCardType == CardType.Food || offeringData.IsHealingPotion || offeringData.IsCultivationPill)
            {
                Mewtations.Expedition.ExpeditionManager.Instance.RunState.BaseAppeasementCorruption += points;
                Debug.Log($"[CatGodMouth] Hiến tế lương thực/linh dược: Tích lũy {points} điểm Xoa Dịu Ô Nhiễm (Base Corruption Appeasement).");
            }
        }
        
        // Play premium eat sound effect
        if (AudioManager.me != null && AudioManager.me.Eat != null)
        {
            AudioManager.me.PlaySound2D(AudioManager.me.Eat, UnityEngine.Random.Range(0.85f, 1.15f), 0.5f);
        }

        // Selective screenshake for rare items cúng tế
        bool isRare = offeringData.Value >= 15 || offeringData.IsCultivationPill || offeringData.IsAncientRelic;
        if (isRare && GameCamera.instance != null)
        {
            GameCamera.instance.Screenshake = 0.4f;
        }

        // Display lore-friendly floating text on offering items
        if (WorldManager.instance != null)
        {
            string floatingMsg = "";
            if (offeringData.Value <= 2)
            {
                floatingMsg = "Miệng thần khịt mũi khinh thường.";
            }
            else if (isRare)
            {
                floatingMsg = "✨ Một tia linh khí rơi xuống...";
            }
            else
            {
                floatingMsg = $"☯️ Dâng hiến {offeringData.Name}...";
            }
            WorldManager.instance.CreateFloatingText(this.MyGameCard, !offeringData.Id.Contains("trash"), 0, floatingMsg, "", !offeringData.Id.Contains("trash"), 0, 1.5f, true);
        }
        
        // Destroy the consumed card
        offeringCard.DestroyCard(true, true);

        // Spawn beautiful offering sparkle visual using simple debug or log
        Debug.Log($"[CatGodMouth] Đã dâng tế {offeringData.Name}. Nhận {val} Linh lực. Tiến độ: {OfferingProgress}.");

        // Tỷ lệ 40% rơi đồ vật hồi đáp từ Miệng Thần Mèo khi hiến tế thành công
        if (UnityEngine.Random.value <= 0.40f)
        {
            Vector3 dropPos = this.transform.position + Vector3.back * 0.8f + new Vector3(UnityEngine.Random.Range(-0.2f, 0.2f), 0, UnityEngine.Random.Range(-0.2f, 0.2f));
            string dropId = "";
            string dropName = "";

            if (UnityEngine.Random.value <= 0.05f)
            {
                // 5% rơi Cống Phẩm Đền Thờ
                dropId = "item_shrine_offering";
                dropName = "Cống Phẩm Đền Thờ";
            }
            else
            {
                // 95% rơi đồ linh tinh
                string[] junkItems = { "resource_gold", "resource_wood", "resource_stone", "resource_iron_ore", "raw_meat", "food_berry" };
                dropId = junkItems[UnityEngine.Random.Range(0, junkItems.Length)];
                dropName = dropId == "resource_gold" ? "Tiền Vàng" : 
                           dropId == "resource_wood" ? "Gỗ" :
                           dropId == "resource_stone" ? "Đá" :
                           dropId == "resource_iron_ore" ? "Quặng Sắt" :
                           dropId == "raw_meat" ? "Thịt Sống" : "Quả Mọng";
            }

            if (!string.IsNullOrEmpty(dropId))
            {
                WorldManager.instance.CreateCard(dropPos, dropId, true, true, true);
                Debug.Log($"[CatGodMouth] Thần Mèo hồi đáp nhả ra vật phẩm: {dropName} ({dropId})");

                if (WorldManager.instance != null)
                {
                    WorldManager.instance.CreateFloatingText(this.MyGameCard, true, 0, "Thần Mèo nhả ra cục gì đó nhớp nháp...", "", true, 0, 1.8f, true);
                }
            }
        }

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
            OfferingProgress -= 700; // Deduct the threshold cleanly instead of setting to 0!

            // Screenshake for massive legendary payout
            if (GameCamera.instance != null) GameCamera.instance.Screenshake = 0.8f;
            if (WorldManager.instance != null)
            {
                WorldManager.instance.CreateFloatingText(this.MyGameCard, true, 0, "✨ Một tia linh khí rơi xuống...", "", true, 0, 2f, true);
            }

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
                        Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { "Đại cát đại lợi!" }, (choiceIdx) => { 
                            TriggerCatGodWrath($"Thiên Phú {Mewtations.Expedition.HeavenlyTalent.GetDisplayName(chosenTalent)}");
                        });
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
            if (GameCamera.instance != null) GameCamera.instance.Screenshake = 0.5f;
            if (WorldManager.instance != null)
            {
                WorldManager.instance.CreateFloatingText(this.MyGameCard, true, 0, "✨ Linh quang hội tụ...", "", true, 0, 1.6f, true);
            }
        }
        else if (OfferingProgress >= 150)
        {
            rewardId = "item_revive_pill";
            rewardName = "Linh Đan Hồi Sinh";
            if (GameCamera.instance != null) GameCamera.instance.Screenshake = 0.4f;
            if (WorldManager.instance != null)
            {
                WorldManager.instance.CreateFloatingText(this.MyGameCard, true, 0, "💚 Linh quang sinh cơ hội tụ...", "", true, 0, 1.5f, true);
            }
        }
        else if (OfferingProgress >= 50)
        {
            // Give 3-5 gold coins
            rewardId = "resource_gold";
            rewardName = "Túi Tiền Vàng của Thần Mèo";
            if (WorldManager.instance != null)
            {
                WorldManager.instance.CreateFloatingText(this.MyGameCard, true, 0, "💰 Thần Mèo nhả ra Vàng...", "", true, 0, 1.2f, true);
            }
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
                Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { "Tạ ơn Thần Mèo!" }, (choiceIdx) => { 
                    if (rewardId == "cat_basic" || rewardId == "item_heavenly_relic" || rewardId == "item_breakthrough_pill")
                    {
                        TriggerCatGodWrath(rewardName);
                    }
                });
            }
        }
    }

    private void TriggerCatGodWrath(string rewardName)
    {
        // Screenshake for anger
        if (GameCamera.instance != null) GameCamera.instance.Screenshake = 0.6f;
        if (WorldManager.instance != null)
        {
            WorldManager.instance.CreateFloatingText(this.MyGameCard, false, 0, "☠️ Thiên đạo chú ý đến ngươi...", "", false, 0, 2f, true);
        }

        string title = "⚠️ THẦN MÈO PHẪN NỘ: LÒNG THAM QUÁ ĐỘ!";
        string text = $"Ngươi đã rút được báu vật tối cao **{rewardName}** từ Miệng Thần Mèo!\n\n" +
                      $"Lực lượng của báu vật quá lớn làm khuấy động phong ấn thiên địa. Tà thần hư không phẫn nộ đòi hỏi ngươi phải cúng nạp vàng cúng tế để xoa dịu lòng tham toàn cục, nếu không ma khí sẽ triệu hồi Tà Linh Hư Không tấn công tông môn!\n\n" +
                      $"• **Lòng Tham gia tăng:** **+30 Greed** khí vận.\n" +
                      $"• **Lựa chọn của ngươi là gì?**";

        // Tăng Greed mặc định
        if (Mewtations.Expedition.ExpeditionManager.Instance != null && Mewtations.Expedition.ExpeditionManager.Instance.RunState != null)
        {
            Mewtations.Expedition.ExpeditionManager.Instance.RunState.GreedLevel = Mathf.Min(100, Mewtations.Expedition.ExpeditionManager.Instance.RunState.GreedLevel + 30);
        }

        // Tìm tất cả vàng trên bàn
        var goldCards = new List<GameCard>();
        foreach (var gc in WorldManager.instance.AllCards)
        {
            if (gc != null && gc.CardData.Id == "resource_gold")
            {
                goldCards.Add(gc);
            }
        }
        bool hasEnoughGold = goldCards.Count >= 20;

        var choices = new List<Mewtations.Dialogue.DialogueChoice>();

        choices.Add(new Mewtations.Dialogue.DialogueChoice(
            "Cúng tế 20 Vàng để xoa dịu (Giảm 20 Greed)",
            () => {
                int destroyed = 0;
                for (int i = goldCards.Count - 1; i >= 0 && destroyed < 20; i--)
                {
                    if (goldCards[i] != null && !goldCards[i].Destroyed)
                    {
                        goldCards[i].DestroyCard(true, true);
                        destroyed++;
                    }
                }
                if (Mewtations.Expedition.ExpeditionManager.Instance != null && Mewtations.Expedition.ExpeditionManager.Instance.RunState != null)
                {
                    Mewtations.Expedition.ExpeditionManager.Instance.RunState.GreedLevel = Mathf.Max(0, Mewtations.Expedition.ExpeditionManager.Instance.RunState.GreedLevel - 20);
                }

                string subTitle = "THẦN LINH YÊN VỊ";
                string subText = "Thần Mèo chấp nhận 20 Vàng xoa dịu lòng tham, xua tan một phần ma khí hư không xung quanh!";
                if (Mewtations.Dialogue.DialogueSystem.Instance != null)
                {
                    Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(subTitle, subText, new List<string> { "Lành thay!" }, (idx) => {});
                }
            },
            () => hasEnoughGold,
            "Cần 20 Vàng"
        ));

        choices.Add(new Mewtations.Dialogue.DialogueChoice(
            "Từ chối! Chấp nhận chiến đấu với Tà Linh Hư Không (`mob_void_spirit`)",
            () => {
                Vector3 spawnPos = this.transform.position + Vector3.back * 1.5f;
                WorldManager.instance.CreateCard(spawnPos, "mob_void_spirit", true, true, true);

                string subTitle = "TÀ LINH GIÁNG THẾ!";
                string subText = "Hư không vỡ vụn! Một thực thể Tà Linh Hư Không gớm ghiếc (`mob_void_spirit`) chui ra từ kẽ nứt và lao vào tấn công quân đội của bạn!";
                if (Mewtations.Dialogue.DialogueSystem.Instance != null)
                {
                    Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(subTitle, subText, new List<string> { "Chuẩn bị chiến đấu!" }, (idx) => {});
                }
            }
        ));

        if (Mewtations.Dialogue.DialogueSystem.Instance != null)
        {
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, choices);
        }
    }

    private void TriggerCosmicTemptation()
    {
        // Screenshake and floating text for cosmic temptation
        if (GameCamera.instance != null) GameCamera.instance.Screenshake = 0.5f;
        if (WorldManager.instance != null)
        {
            WorldManager.instance.CreateFloatingText(this.MyGameCard, false, 0, "☠️ Thiên đạo chú ý đến ngươi...", "", false, 0, 2f, true);
        }

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
                    if (goldCards[i] != null && !goldCards[i].Destroyed)
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
        if (otherCard is CatCardData)
        {
            return true;
        }
        if (otherCard.MyCardType == CardType.Humans)
        {
            return false;
        }
        return base.CanHaveCard(otherCard);
    }

    // PerformSoulAbsorption removed per user request
}
