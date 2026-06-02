using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mewtations.Legacy.Stacklands
{
    public class OrderingCardData : CardData, IUpgradeableItem
    {
        [ExtraData("upgrade_tier")]
        public int UpgradeTierData = 0;

        [ExtraData("insured_bonus")]
        public int InsuredBonus = 0;

        public int UpgradeTier => UpgradeTierData;
        public int MaxUpgradeTier => 5;

        public int StorageCapacity => 10 + (UpgradeTier * 2);
        
        public int InsuredSlots => (WorldManager.instance != null && ShrineCardData.IsRelicActiveInShrine("item_ancient_relic_insurance") ? 5 : 0) + InsuredBonus;

        public override bool HasInventory => true;

        public bool CanUpgrade(RefinementMaterialCardData material)
        {
            return UpgradeTier < MaxUpgradeTier;
        }

        public void ApplyUpgrade(RefinementMaterialCardData material)
        {
            UpgradeTierData++;
            if (material.MaterialTier >= 3)
            {
                InsuredBonus++;
            }
        }

        protected override bool CanHaveCard(CardData otherCard)
        {
            // Outside combat we can freely add. During combat active phase, locked.
            if (Mewtations.Combat.Core.TurnBasedCombatManager.Instance != null &&
                Mewtations.Combat.Core.TurnBasedCombatManager.Instance.IsCombatActive &&
                Mewtations.Combat.Core.TurnBasedCombatManager.Instance.State == Mewtations.Combat.Core.MewtationsCombatState.Active)
            {
                return false;
            }
            return true;
        }

        public override void UpdateCard()
        {
            base.UpdateCard();
            if (this.MyGameCard == null) return;

            // Ensure the Ordering container capacity is updated correctly
            if (this.MyGameCard.InventoryContainer == null)
            {
                this.MyGameCard.SetInventoryContainer(new HiddenInventoryContainer(this.MyGameCard, StorageCapacity));
            }
            else if (this.MyGameCard.InventoryContainer.GetCapacity() != StorageCapacity)
            {
                if (this.MyGameCard.InventoryContainer is HiddenInventoryContainer hidden)
                {
                    hidden.SetCapacity(StorageCapacity);
                }
            }

            // Handle cards dropped/stacked on top of the Storage Ring
            if (this.MyGameCard.Child != null && !this.MyGameCard.Child.BeingDragged)
            {
                bool isCombatActiveAndLocked = Mewtations.Combat.Core.TurnBasedCombatManager.Instance != null &&
                    Mewtations.Combat.Core.TurnBasedCombatManager.Instance.IsCombatActive &&
                    Mewtations.Combat.Core.TurnBasedCombatManager.Instance.State == Mewtations.Combat.Core.MewtationsCombatState.Active;

                if (!isCombatActiveAndLocked)
                {
                    GameCard child = this.MyGameCard.Child;
                    child.RemoveFromParent();

                    // Drag & drop transaction to store actual card instances
                    var context = new ContainerInsertContext { SourceCard = this.MyGameCard, ContextSource = "OrderingDragDrop" };
                    var result = ContainerTransactionSystem.Instance.RequestInsert(child, this.MyGameCard.InventoryContainer, context);
                    if (result.Success)
                    {
                        child.gameObject.SetActive(false); // Hide physically from board
                    }
                    else
                    {
                        child.SendIt();
                    }
                }
            }

            // Synchronize status and descriptive UI texts
            int itemsCount = this.MyGameCard.InventoryContainer != null ? this.MyGameCard.InventoryContainer.GetChildren().Count : 0;
            
            string descTemplate = SokLoc.Translate("ordering_description");
            if (!string.IsNullOrEmpty(descTemplate))
            {
                this.descriptionOverride = string.Format(descTemplate, itemsCount, StorageCapacity, InsuredSlots);
            }
        }
    }
}
