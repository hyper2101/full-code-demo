using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CatRole { DPS, Tank, ShieldSupport, RageSupport, Debuff, Disruption, Attrition }
public enum CatElement { None, Fire, Poison, Ice, Lightning }

public class CatCardData : Combatable
{
    [Header("Cat Specifics")]
    [ExtraData("cat_role")]
    public CatRole Role;

    [ExtraData("cat_element")]
    public CatElement Element;

    [Header("Breakthrough System")]
    [ExtraData("breakthrough_level")]
    public int BreakthroughLevel = 0;

    [ExtraData("has_pill_slot")]
    public bool HasPillSlot = false;
    
    [ExtraData("has_food_slot")]
    public bool HasFoodSlot = false;

    [ExtraData("has_passive1_slot")]
    public bool HasPassive1Slot = false;

    [ExtraData("has_passive2_slot")]
    public bool HasPassive2Slot = false;

    [Header("Turn-Based Combat Stats")]
    [ExtraData("current_rage")]
    public int CurrentRage = 0;

    [ExtraData("speed_stat")]
    public int Speed = 100;

    public override void UpdateCard()
    {
        base.UpdateCard();

        // Intercept stack to start a Breakthrough Timer when a breakthrough pill is stacked on the cat
        if (this.MyGameCard != null && !this.MyGameCard.TimerRunning && this.MyGameCard.HasChild)
        {
            CardData childData = this.MyGameCard.Child.CardData;
            if (childData.Id == "item_breakthrough_pill")
            {
                float time = Mathf.Max(3f, 10f - (Speed * 0.02f)); // Speed influences breakthrough speed
                this.MyGameCard.StartTimer(time, new TimerAction(this.PerformBreakthrough), "Đột phá Cảnh giới...", "breakthrough");
            }
        }
    }

    public void PerformBreakthrough()
    {
        if (this.MyGameCard.HasChild && this.MyGameCard.Child.CardData.Id == "item_breakthrough_pill")
        {
            GameCard pill = this.MyGameCard.Child;
            pill.DestroyCard(true, true);
        }

        BreakthroughLevel++;
        string cảnhGiới = "";
        
        switch (BreakthroughLevel)
        {
            case 1:
                HasPillSlot = true;
                cảnhGiới = "Luyện Khí Cảnh (Mở ô Linh Đan)";
                break;
            case 2:
                HasFoodSlot = true;
                cảnhGiới = "Trúc Cơ Cảnh (Mở ô Thức Ăn - Ultimate Skill)";
                break;
            case 3:
                HasPassive1Slot = true;
                cảnhGiới = "Kim Đan Cảnh (Mở ô Thiên Phú 1)";
                break;
            case 4:
                HasPassive2Slot = true;
                cảnhGiới = "Nguyên Anh Cảnh (Mở ô Thiên Phú 2)";
                break;
            default:
                cảnhGiới = $"Hóa Thần Cảnh Tầng {BreakthroughLevel - 4} (Tăng mạnh Sinh mệnh & Thần tốc)";
                break;
        }

        // Upgrade core combat stats
        this.BaseCombatStats.MaxHealth += 10;
        this.HealthPoints = this.ProcessedCombatStats.MaxHealth;
        this.Speed += 15;

        // Show elegant breakthrough dialog using DialogueSystem
        string title = "ĐỘT PHÁ THÀNH CÔNG!";
        string text = $"Thần Miêu <b>{Name}</b> đã đập vỡ xiềng xích phàm trần, đột phá thành công lên <b><color=#ffcc00>{cảnhGiới}</color></b>!\n\n" +
                      $"• Sinh mệnh tối đa tăng lên: <b>{this.ProcessedCombatStats.MaxHealth} HP</b>\n" +
                      $"• Thần tốc tăng lên: <b>{Speed} Speed</b>\n" +
                      $"• Cực hạn võ đạo mới đã được khai mở!";

        if (Mewtations.Dialogue.DialogueSystem.Instance != null)
        {
            Mewtations.Dialogue.DialogueSystem.Instance.StartDialogue(title, text, new List<string> { "Chúc mừng!" }, (choiceIdx) => { });
        }
    }

    public override bool CanHaveCard(CardData otherCard)
    {
        // 1. Validate food slot (BT level 2)
        if (otherCard.MyCardType == CardType.Food) 
        {
            return HasFoodSlot;
        }

        // 2. Validate Pill slot (BT level 1)
        if (otherCard.Id == "item_pill" || otherCard.Id.Contains("pill"))
        {
            // breakthrough pill can be stacked on anyone to trigger breakthrough
            if (otherCard.Id == "item_breakthrough_pill")
            {
                return true;
            }
            return HasPillSlot;
        }

        // 3. Equipment slots (Weapon & Talismans) are allowed by default
        if (otherCard.MyCardType == CardType.Equipment)
        {
            return true;
        }

        // Default parent validation
        return base.CanHaveCard(otherCard);
    }
}
