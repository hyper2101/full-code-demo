using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [Header("Traits and Mutations")]
    [ExtraData("lineage_generation")]
    public int LineageGeneration = 1;

    [Header("GDD Punishment States")]
    [ExtraData("is_food_slot_locked")]
    public bool IsFoodSlotLocked = false;

    [ExtraData("is_pill_slot_locked")]
    public bool IsPillSlotLocked = false;

    [ExtraData("is_passive_slots_locked")]
    public bool IsPassiveSlotsLocked = false;

    [ExtraData("is_equipment_slots_locked")]
    public bool IsEquipmentSlotsLocked = false;

    [ExtraData("is_ultimate_locked")]
    public bool IsUltimateLocked = false;

    [ExtraData("character_memoirs")]
    public string CharacterMemoirsString = "";

    public List<Mewtations.Expedition.MemoirEntry> CharacterMemoirs
    {
        get
        {
            return string.IsNullOrEmpty(CharacterMemoirsString)
                ? new List<Mewtations.Expedition.MemoirEntry>()
                : CharacterMemoirsString.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(Mewtations.Expedition.MemoirEntry.Parse)
                    .Where(e => e != null)
                    .ToList();
        }
    }

    public void AddMemoir(Mewtations.Expedition.MemoirType type, string paramA = "", string paramB = "")
    {
        int day = (WorldManager.instance != null) ? WorldManager.instance.CurrentMonth : 1;
        var list = CharacterMemoirs;
        list.Add(new Mewtations.Expedition.MemoirEntry(type, paramA, paramB, day));
        CharacterMemoirsString = string.Join(";", list.Select(m => m.ToString()));
    }

    public void AddMemoir(string milestone)
    {
        AddMemoir(Mewtations.Expedition.MemoirType.Birth, milestone);
    }

    [ExtraData("permanent_traits")]
    public string PermanentTraitsString = "";

    [ExtraData("active_mutations")]
    public string ActiveMutationsString = "";

    public List<string> PermanentTraits
    {
        get
        {
            return string.IsNullOrEmpty(PermanentTraitsString) 
                ? new List<string>() 
                : new List<string>(PermanentTraitsString.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries));
        }
    }

    public List<string> ActiveMutations
    {
        get
        {
            return string.IsNullOrEmpty(ActiveMutationsString) 
                ? new List<string>() 
                : new List<string>(ActiveMutationsString.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries));
        }
    }

    public void AddTrait(string traitId)
    {
        var list = PermanentTraits;
        if (list.Count >= 2)
        {
            Debug.LogWarning($"[Song Trọng Dị Biến] Không thể dung hợp thêm thiên phú vĩnh cửu {traitId} cho {Name} vì đã đạt cực hạn (2).");
            return;
        }

        if (!list.Contains(traitId))
        {
            list.Add(traitId);
            PermanentTraitsString = string.Join(",", list);
        }
    }

    public void RemoveTrait(string traitId)
    {
        var list = PermanentTraits;
        if (list.Remove(traitId))
        {
            PermanentTraitsString = string.Join(",", list);
        }
    }

    public void AddMutation(string mutationId)
    {
        var list = ActiveMutations;
        if (!list.Contains(mutationId))
        {
            list.Add(mutationId);
            ActiveMutationsString = string.Join(",", list);
        }
    }

    public void RemoveMutation(string mutationId)
    {
        var list = ActiveMutations;
        if (list.Remove(mutationId))
        {
            ActiveMutationsString = string.Join(",", list);
        }
    }

    public void ClearMutations()
    {
        ActiveMutationsString = "";
    }

    public override void OnEquipItem(Equipable equipable)
    {
        base.OnEquipItem(equipable);
        if (equipable != null && !string.IsNullOrEmpty(equipable.Name))
        {
            AddMemoir(Mewtations.Expedition.MemoirType.Equip, equipable.Name);
        }
    }

    public override void OnUnequipItem(Equipable equipable)
    {
        base.OnUnequipItem(equipable);
        if (equipable != null && !string.IsNullOrEmpty(equipable.Name))
        {
            AddMemoir(Mewtations.Expedition.MemoirType.Unequip, equipable.Name);
        }
    }

    public bool HasTrait(string id)
    {
        return PermanentTraits.Contains(id);
    }

    public bool HasMutation(string id)
    {
        return ActiveMutations.Contains(id);
    }

    public override void UpdateCard()
    {
        base.UpdateCard();

        // Intercept stack to start a Breakthrough Timer when a breakthrough pill is stacked on the cat
        if (this.MyGameCard != null)
        {
            if (this.MyGameCard.TimerRunning && this.MyGameCard.TimerActionId == "breakthrough")
            {
                if (!this.MyGameCard.HasChild || this.MyGameCard.Child.CardData.Id != "item_breakthrough_pill")
                {
                    this.MyGameCard.CancelTimer("breakthrough");
                }
            }
            else if (!this.MyGameCard.TimerRunning && this.MyGameCard.HasChild)
            {
                CardData childData = this.MyGameCard.Child.CardData;
                if (childData.Id == "item_breakthrough_pill")
                {
                    float time = Mathf.Max(3f, 10f - (Speed * 0.02f)); // Speed influences breakthrough speed
                    this.MyGameCard.StartTimer(time, new TimerAction(this.PerformBreakthrough), "Đột phá Cảnh giới...", "breakthrough");
                }
            }
        }
    }

    public void PerformBreakthrough()
    {
        if (this.MyGameCard != null && this.MyGameCard.HasChild && this.MyGameCard.Child.CardData.Id == "item_breakthrough_pill")
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

        // Add Memoir hook
        string simpleCảnhGiới = cảnhGiới.Split(' ')[0];
        AddMemoir("Đột phá thành công lên " + simpleCảnhGiới);

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

    protected override bool CanHaveCard(CardData otherCard)
    {
        // 1. Check Equipment locks (Weapon / Talismans / Food are equipment types)
        if (IsEquipmentSlotsLocked && otherCard.MyCardType == CardType.Equipment)
        {
            return false;
        }

        // 2. Validate food slot (BT level 2)
        if (otherCard.MyCardType == CardType.Food || (otherCard is Equipable eqFood && eqFood.EquipableType == EquipableType.Food)) 
        {
            if (IsFoodSlotLocked) return false;
            return HasFoodSlot;
        }

        // 3. Validate Pill slot (BT level 1)
        if (otherCard.Id == "item_pill" || otherCard.Id.Contains("pill"))
        {
            // breakthrough pill can be stacked on anyone to trigger breakthrough
            if (otherCard.Id == "item_breakthrough_pill")
            {
                return true;
            }
            if (IsPillSlotLocked) return false;
            return HasPillSlot;
        }

        // 4. Validate Passive Slots (BT level 3 & 4: Max 1 for level 3, Max 2 for level 4)
        if (otherCard.Id.StartsWith("item_passive_") || otherCard.Id.Contains("passive") || (otherCard is Equipable eqTal && eqTal.EquipableType == EquipableType.Talisman))
        {
            if (IsPassiveSlotsLocked) return false;

            int maxPassives = 0;
            if (BreakthroughLevel >= 4) maxPassives = 2;
            else if (BreakthroughLevel == 3) maxPassives = 1;

            int currentPassives = ChildrenMatchingPredicateCount(c => c.Id.StartsWith("item_passive_") || c.Id.Contains("passive"));
            int equippedTalismans = GetAllEquipables().Count(eq => eq.EquipableType == EquipableType.Talisman);
            return (currentPassives + equippedTalismans) < maxPassives;
        }

        // 5. Equipment slots (Weapon & Talismans) are allowed by default
        if (otherCard.MyCardType == CardType.Equipment)
        {
            return true;
        }

        // Default parent validation
        return base.CanHaveCard(otherCard);
    }

    public string GetCảnhGiớiName()
    {
        switch (BreakthroughLevel)
        {
            case 0: return "Phàm Nhân Mèo";
            case 1: return "Luyện Khí Cảnh";
            case 2: return "Trúc Cơ Cảnh";
            case 3: return "Kim Đan Cảnh";
            case 4: return "Nguyên Anh Cảnh";
            default: return $"Hóa Thần Cảnh Tầng {BreakthroughLevel - 4}";
        }
    }

    public override void UpdateCardText()
    {
        string desc = $"<b>CẢNH GIỚI:</b> <color=#ffcc00>{GetCảnhGiớiName()}</color>\n";
        desc += $"<b>VAI TRÒ:</b> <color=#5dade2>{Role}</color> | <b>LINH CĂN:</b> <color=#ff33cc>{Element}</color>\n";
        desc += $"<b>SINH MỆNH:</b> {HealthPoints}/{ProcessedCombatStats.MaxHealth} HP\n";
        desc += $"<b>THẦN TỐC:</b> {Speed} Speed\n\n";

        var traits = PermanentTraits;
        if (traits.Count > 0)
        {
            desc += "<b>★ THIÊN PHÚ VĨNH CỬU:</b>\n";
            foreach (var t in traits)
            {
                desc += $"• <color=#00ffcc>{Mewtations.Expedition.HeavenlyTalent.GetDisplayName(t)}</color>: {Mewtations.Expedition.HeavenlyTalent.GetDescription(t)}\n";
            }
            desc += "\n";
        }

        var mutations = ActiveMutations;
        if (mutations.Count > 0)
        {
            desc += "<b>☣️ DỊ BIẾN TẠM THỜI:</b>\n";
            foreach (var m in mutations)
            {
                desc += $"• <color=#ff3333>{Mewtations.Expedition.UnstableMutation.GetDisplayName(m)}</color>: {Mewtations.Expedition.UnstableMutation.GetDescription(m)}\n";
            }
            desc += "\n";
        }

        bool hasPunishment = IsFoodSlotLocked || IsPillSlotLocked || IsPassiveSlotsLocked || IsEquipmentSlotsLocked || IsUltimateLocked;
        if (hasPunishment)
        {
            desc += "<b>☠️ HÌNH PHẠT NGÔN NGỮ / KHÓA:</b>\n";
            if (IsUltimateLocked) desc += "• <color=red>[KHÓA KỸ NĂNG NỘ]</color>: Không thể thi triển Ultimate Skill.\n";
            if (IsFoodSlotLocked) desc += "• <color=red>[KHÓA Ô THỨC ĂN]</color>: Slot Ultimate Skill bị phong ấn.\n";
            if (IsPillSlotLocked) desc += "• <color=red>[KHÓA Ô LINH ĐAN]</color>: Slot Linh Đan bị phong ấn.\n";
            if (IsPassiveSlotsLocked) desc += "• <color=red>[KHÓA Ô THIÊN PHÚ]</color>: Slot Thiên Phú bị phong ấn.\n";
            if (IsEquipmentSlotsLocked) desc += "• <color=red>[KHÓA Ô TRANG BỊ]</color>: Không thể trang bị vũ khí/bùa.\n";
            desc += "\n";
        }

        desc += "<i>" + SokLoc.Translate(this.DescriptionTerm) + "</i>";
        this.descriptionOverride = desc;
        base.UpdateCardText();
    }
}
