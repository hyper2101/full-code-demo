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
            Passive2,
            SpecialCombat
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
                
                // Equipment Slot Safety Framework: Reject stack merge and drag
                if (item.MyGameCard != null && (item.MyGameCard.HasChild || item.MyGameCard.HasParent || item.MyGameCard.GetStackCount() > 1)) return false;

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
                        return item is Equipable eqS && eqS.EquipableType == EquipableType.Skill;
                    case CatSlotType.Passive1:
                    case CatSlotType.Passive2:
                        return item.IsPassiveTalisman;
                    case CatSlotType.SpecialCombat:
                        return item is Equipable eqSC && eqSC.EquipableType == EquipableType.SpecialCombat;
                }
                return false;
            }
    }
}
