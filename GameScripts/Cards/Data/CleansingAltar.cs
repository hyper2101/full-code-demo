using System;
using System.Collections.Generic;
using UnityEngine;

public class CleansingAltar : CardData
{
    public override bool DetermineCanHaveCardsWhenIsRoot => true;

    protected override bool CanHaveCard(CardData otherCard)
    {
        return otherCard is CatCardData;
    }

    public override void UpdateCard()
    {
        base.UpdateCard();
        
        if (this.MyGameCard != null)
        {
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
                this.MyGameCard.StartTimer(5.0f, new TimerAction(this.PerformMeridianCure), MewtationsLoc.Translate("catgod_cure_timer", "Nghi Lễ Hộ Mệnh Trị Liệu..."), "meridian_cure");
            }
            else if (this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == "cleansing")
            {
                if (!this.MyGameCard.HasChild || !(this.MyGameCard.Child.CardData is CatCardData c) || c.PermanentScars.Count == 0)
                {
                    this.MyGameCard.CancelTimer("cleansing");
                }
            }
            else if (!this.MyGameCard.TimerRunning && this.MyGameCard.HasChild && this.MyGameCard.Child.CardData is CatCardData c && c.PermanentScars.Count > 0)
            {
                this.MyGameCard.StartTimer(5.0f, new TimerAction(this.PerformScarCleansing), MewtationsLoc.Translate("catgod_cleansing_timer", "Nghi Lễ Tẩy Tủy Sẹo..."), "cleansing");
            }
        }
    }

    private void PerformMeridianCure()
    {
        if (this.MyGameCard == null || !this.MyGameCard.HasChild || !(this.MyGameCard.Child.CardData is CatCardData cat)) return;

        string title = MewtationsLoc.Translate("catgod_cure_title", "☯️ NGHI LỄ HỘ MỆNH TRỊ LIỆU KINH MẠCH");
        string text = MewtationsLoc.TranslateFormat("catgod_cure_desc", 
                      "Thần Miêu <b>{0}</b> bị tẩu hỏa nhập ma, bế tắc linh mạch nghiêm trọng sau lôi kiếp đột phá thất bại.\n\nLinh khí bạo phát đòi hỏi cúng tế vàng và linh dược cụ thể để hồi phục mạch tượng hoàn hảo:\n\n• <b>Tế phẩm yêu cầu:</b> 15 Vàng & 1 Thuốc hồi máu (`item_healing_potion`).\n• <b>Hiệu quả:</b> Gỡ bỏ hoàn toàn tình trạng bế tắc, giải phóng tất cả các ô bị khóa an toàn 100%!",
                      cat.Name);

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
            MewtationsLoc.Translate("catgod_cure_opt", "Cúng tế 15 Vàng & 1 Thuốc hồi máu để trị liệu."),
            () => {
                int destroyedGold = 0;
                for (int i = goldCards.Count - 1; i >= 0 && destroyedGold < 15; i--)
                {
                    if (goldCards[i] != null && !goldCards[i].Destroyed)
                    {
                        goldCards[i].DestroyCard(true, true);
                        destroyedGold++;
                    }
                }

                if (potionCards.Count > 0 && potionCards[0] != null && !potionCards[0].Destroyed)
                {
                    potionCards[0].DestroyCard(true, true);
                }

                cat.IsPillSlotLocked = false;
                cat.IsFoodSlotLocked = false;
                cat.IsPassiveSlotsLocked = false;
                cat.IsEquipmentSlotsLocked = false;

                string subTitle = MewtationsLoc.Translate("catgod_cure_success_title", "☯️ KINH MẠCH KHAI THÔNG!");
                string subText = MewtationsLoc.TranslateFormat("catgod_cure_success_desc", 
                                 "Dược lực bùng nổ kết hợp với linh lực cúng tế đã gột rửa hoàn toàn các bế tắc trong kinh mạch của <b>{0}</b>!\n\n🌟 Mọi ô chứa bị khóa đã được mở khóa an toàn. <b>{0}</b> đã khôi phục mạch tượng hoàn hảo để tiếp tục tu luyện!",
                                 cat.Name);
                
                if (Mewtations.Dialogue.DialogueSystem.Instance != null)
                {
                    Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(subTitle, subText, new List<string> { MewtationsLoc.Translate("btn_great_fortune", "Đại cát đại lợi!") }, (cIdx) => {});
                }
            },
            () => hasResources,
            MewtationsLoc.Translate("catgod_cure_req", "Cần 15 Vàng & 1 Thuốc hồi máu")
        ));

        choices.Add(new Mewtations.Dialogue.DialogueChoice(
            MewtationsLoc.Translate("opt_retreat", "Rút lui"),
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

        string title = MewtationsLoc.Translate("catgod_cleansing_title", "☯️ NGHI LỄ TẨY TỦY SẸO");
        string text = MewtationsLoc.TranslateFormat("catgod_cleansing_desc", 
                      "Thần Miêu <b>{0}</b> sở hữu linh thể mang thương tổn nặng nề (Vết sẹo vĩnh cửu) đang thành kính quỳ trước Đài Tẩy Tủy.\n\nTà linh cổ xưa thì thầm đòi hỏi cống nạp một số lượng tiền vàng khổng lồ (30 Vàng) để nghịch chuyển linh lực, tái sinh kinh mạch.\n\n• <b>Tỷ lệ tẩy sẹo thành công:</b> 50%.\n• <b>Hình phạt nếu thất bại:</b> Ma khí phản phệ dữ dội bạo phát <b>+40 Greed</b> toàn cục và khiến <b>{0}</b> gánh thêm một Vết sẹo phế mạch mới!",
                      cat.Name);

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
            MewtationsLoc.Translate("catgod_cleansing_opt", "Cúng tế 30 Vàng để tẩy tủy."),
            () => {
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
                    cat.PermanentScarsString = "";
                    string subTitle = MewtationsLoc.Translate("catgod_cleansing_success_title", "☯️ TẨY TỦY THÀNH CÔNG!");
                    string subText = MewtationsLoc.TranslateFormat("catgod_cleansing_success_desc", 
                                     "Đài Tẩy Tủy chấp nhận tế phẩm! Linh quang tím chói lòa chiếu rọi, tái sinh linh thể cho <b>{0}</b>. Toàn bộ các vết sẹo vĩnh cửu đã được gột rửa hoàn toàn!",
                                     cat.Name);
                    if (Mewtations.Dialogue.DialogueSystem.Instance != null)
                    {
                        Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(subTitle, subText, new List<string> { MewtationsLoc.Translate("btn_thank_catgod", "Tạ ơn Thần Mèo!") }, (cIdx) => {});
                    }
                }
                else
反反            {
                    if (Mewtations.Expedition.ExpeditionManager.Instance != null && Mewtations.Expedition.ExpeditionManager.Instance.RunState != null)
                    {
                        Mewtations.Expedition.ExpeditionManager.Instance.RunState.GreedLevel = Mathf.Min(100, Mewtations.Expedition.ExpeditionManager.Instance.RunState.GreedLevel + 40);
                    }

                    cat.AddScar(Mewtations.Combat.PermanentScar.CrippledMeridians);

                    string subTitle = MewtationsLoc.Translate("catgod_cleansing_fail_title", "☠️ NGHI THỨC THẤT BẠI!");
                    string subText = MewtationsLoc.TranslateFormat("catgod_cleansing_fail_desc", 
                                     "Đài Tẩy Tủy nổ tung vì ma lực phản phệ dữ dội! \n\n• Sức ép lòng tham gia gia tăng: <b>+40 Greed</b> toàn cục.\n• <b>{0}</b> gánh chịu chấn thương linh mạch nghiêm trọng hơn, nhận thêm Vết sẹo: <b><color=red>Phế Mạch (-30 Speed)</color></b>!",
                                     cat.Name);
                    
                    if (Mewtations.Dialogue.DialogueSystem.Instance != null)
                    {
                        Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(subTitle, subText, new List<string> { MewtationsLoc.Translate("btn_face_adversity", "Đương đầu tai ách") }, (cIdx) => {});
                    }
                }
            },
            () => hasEnoughGold,
            MewtationsLoc.Translate("catgod_cleansing_req", "Cần 30 Vàng để cúng tế")
        ));

        choices.Add(new Mewtations.Dialogue.DialogueChoice(
            MewtationsLoc.Translate("opt_retreat_safely", "Rút lui an toàn"),
            () => {}
        ));

        if (Mewtations.Dialogue.DialogueSystem.Instance != null)
        {
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, choices);
        }
    }
}
