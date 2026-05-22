using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatCorpseData : CardData
{
    [Header("Corpse Data")]
    [ExtraData("original_cat_id")]
    public string OriginalCatId; // Lưu ID thẻ mèo gốc để biết sẽ hồi sinh thành loại mèo nào

    [ExtraData("original_cat_name")]
    public string OriginalCatName; // Tên của mèo đã chết

    [ExtraData("original_cat_role")]
    public CatRole OriginalCatRole;

    [ExtraData("original_cat_element")]
    public CatElement OriginalCatElement;

    [ExtraData("original_breakthrough_level")]
    public int OriginalBreakthroughLevel = 0;

    [ExtraData("original_has_pill_slot")]
    public bool OriginalHasPillSlot = false;

    [ExtraData("original_has_food_slot")]
    public bool OriginalHasFoodSlot = false;

    [ExtraData("original_has_passive1_slot")]
    public bool OriginalHasPassive1Slot = false;

    [ExtraData("original_has_passive2_slot")]
    public bool OriginalHasPassive2Slot = false;

    [ExtraData("original_speed")]
    public int OriginalSpeed = 100;

    [ExtraData("original_max_health")]
    public int OriginalMaxHealth = 0;

    [ExtraData("original_lineage_generation")]
    public int OriginalLineageGeneration = 1;

    [ExtraData("original_character_memoirs")]
    public string OriginalCharacterMemoirs = "";

    public override void UpdateCard()
    {
        base.UpdateCard();

        // If item_revive_pill is stacked on top of the corpse, start the resurrection timer
        if (this.MyGameCard != null)
        {
            if (this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == "resurrect")
            {
                if (!this.MyGameCard.HasChild || this.MyGameCard.Child.CardData.Id != "item_revive_pill")
                {
                    this.MyGameCard.CancelTimer("resurrect");
                }
            }
            else if (!this.MyGameCard.TimerRunning && this.MyGameCard.HasChild)
            {
                CardData childData = this.MyGameCard.Child.CardData;
                if (childData.Id == "item_revive_pill")
                {
                    this.MyGameCard.StartTimer(5.0f, new TimerAction(this.PerformResurrection), "Hồi sinh Thần Miêu...", "resurrect");
                }
            }
        }
    }

    private void PerformResurrection()
    {
        if (this.MyGameCard != null && this.MyGameCard.HasChild && this.MyGameCard.Child.CardData.Id == "item_revive_pill")
        {
            GameCard pill = this.MyGameCard.Child;
            pill.DestroyCard(true, true);
        }

        Vector3 spawnPos = this.transform.position;
        string catId = string.IsNullOrEmpty(OriginalCatId) ? "cat_basic" : OriginalCatId;

        GameCard newCat = WorldManager.instance.CreateCard(spawnPos, catId, true, true, true);
        CatCardData catData = newCat.CardData as CatCardData;

        if (catData != null)
        {
            catData.Role = OriginalCatRole;
            catData.Element = OriginalCatElement;
            if (!string.IsNullOrEmpty(OriginalCatName))
            {
                catData.CustomName = OriginalCatName;
            }

            // Restore cultivation level & breakthrough slots progress
            catData.BreakthroughLevel = OriginalBreakthroughLevel;
            catData.HasPillSlot = OriginalHasPillSlot;
            catData.HasFoodSlot = OriginalHasFoodSlot;
            catData.HasPassive1Slot = OriginalHasPassive1Slot;
            catData.HasPassive2Slot = OriginalHasPassive2Slot;
            catData.Speed = OriginalSpeed;
            if (OriginalMaxHealth > 0)
            {
                catData.BaseCombatStats.MaxHealth = OriginalMaxHealth;
            }

            // Restore and advance Lineage/Memoirs
            catData.LineageGeneration = OriginalLineageGeneration + 1;
            catData.CharacterMemoirsString = OriginalCharacterMemoirs;
            catData.AddMemoir(Mewtations.Expedition.MemoirType.Resurrection, catData.LineageGeneration.ToString());

            catData.HealthPoints = catData.ProcessedCombatStats.MaxHealth / 2; // Resurrect with 50% HP
            catData.CurrentRage = 0;
        }

        // Celebrate via dialogue system
        string title = "THẦN MIÊU PHỤC SINH!";
        string text = $"Linh Đan Hồi Sinh dung nhập cốt tủy, thần tích xuất hiện!\n\n" +
                      $"Thần Miêu <b>{OriginalCatName}</b> ({OriginalCatRole}) đã đập tan u minh giới, phục sinh quay trở lại nhân gian!\n\n" +
                      $"Lực lượng phục hồi: <b>{catData?.HealthPoints} HP</b>. Hãy chuẩn bị bồi bổ linh thực để khôi phục đỉnh phong!";

        if (Mewtations.Dialogue.DialogueSystem.Instance != null)
        {
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { "Chào mừng trở lại!" }, (choiceIdx) => { });
        }

        // Destroy corpse
        if (this.MyGameCard != null)
        {
            this.MyGameCard.DestroyCard(true, true);
        }
    }

    public override bool DetermineCanHaveCardsWhenIsRoot
    {
        get
        {
            return true; // Có thể nhận thẻ bài khác lên trên (ví dụ thẻ Hồi Sinh)
        }
    }

    protected override bool CanHaveCard(CardData otherCard)
    {
        // Có thể ghép với Linh Đan Hồi Sinh, hoặc thẻ Tế Lễ
        if (otherCard.Id == "item_revive_pill")
        {
            return true;
        }
        return base.CanHaveCard(otherCard);
    }
}
