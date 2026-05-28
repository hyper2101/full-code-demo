using System;

namespace Mewtations.Expedition
{
    public enum MutationEffect
    {
        None,
        UnlockSecondaryWeaponSlot,
        UnlockSecondaryHeadSlot,
        UnlockSecondaryBodySlot,
        UnlockSecondaryRelicSlot,
        // More can be added here
    }

    public enum CatSlotType
    {
        Weapon,
        Torso,
        Head,
        Pill,
        Skill,
        Passive1,
        Passive2
    }

    [Serializable]
    public class EquipmentSlotData
    {
        public CatSlotType SlotType;
        public bool IsUnlocked;
        public CardData EquippedItem;
        public string Title;
        
        public EquipmentSlotData(CatSlotType type, string title, bool unlocked)
        {
            SlotType = type;
            Title = title;
            IsUnlocked = unlocked;
            EquippedItem = null;
        }

        public bool CanEquip(CardData item)
        {
            if (!IsUnlocked) return false;
            if (item == null) return false;
            
            switch (SlotType)
            {
                case CatSlotType.Weapon:
                    return item is Equipable eqW && eqW.EquipableType == EquipableType.Weapon;
                case CatSlotType.Torso:
                    return item is Equipable eqT && eqT.EquipableType == EquipableType.Torso;
                case CatSlotType.Head:
                    return item is Equipable eqH && eqH.EquipableType == EquipableType.Head;
                case CatSlotType.Pill:
                    return item.IsCultivationPill;
                case CatSlotType.Skill:
                    // Thức ăn hoặc kỹ năng (Food)
                    return item.MyCardType == CardType.Food || (item is Equipable eqF && eqF.EquipableType == EquipableType.Food);
                case CatSlotType.Passive1:
                case CatSlotType.Passive2:
                    return item.IsPassiveTalisman;
            }
            return false;
        }
    }
}
