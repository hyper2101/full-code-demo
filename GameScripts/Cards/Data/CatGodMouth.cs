using System;
using System.Collections.Generic;
using UnityEngine;

public class CatGodMouth : CardData
{
    [Header("Cat God Mouth Settings")]
    [ExtraData("offering_progress")]
    public int OfferingProgress = 0;

    public override bool DetermineCanHaveCardsWhenIsRoot
    {
        get
        {
            return true; // Can have cards stacked on top
        }
    }

    public override void UpdateCard()
    {
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
            rewardId = "cat_basic"; // Rare Heavenly Talent Cat
            rewardName = "Một Thần Miêu Mới (Được gia trì Thiên Kiêu)";
            OfferingProgress = 0; // Reset progress
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
            if (rewardId != "cat_basic")
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
