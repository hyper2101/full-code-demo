using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mewtations.Legacy.Stacklands
{
    public class StorageRingCardData : CardData
    {
        public override bool HasInventory => true;

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

            // Ensure the Ordering container capacity is exactly 30
            if (this.MyGameCard.InventoryContainer == null || this.MyGameCard.InventoryContainer.GetCapacity() != 30)
            {
                this.MyGameCard.SetInventoryContainer(new HiddenInventoryContainer(this.MyGameCard, 30));
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
            bool hasRelic = WorldManager.instance != null && 
                ShrineCardData.IsRelicActiveInShrine("item_ancient_relic_insurance");
            string relicStatus = hasRelic ? "\n<color=#00ffcc>🛡️ Cổ Vật Bảo Hiểm kích hoạt: 5 ô đầu tiên được bảo vệ vĩnh viễn!</color>" : "";

            this.descriptionOverride = $"<b>Nhẫn Trữ Vật (Storage Ring)</b>\nSức chứa: {itemsCount} / 30 ô.\nMang vào Combat Prep để trang bị/sử dụng đạo cụ.{relicStatus}\n<i>Phạt mất 50% vật phẩm ngẫu nhiên khi rút lui/thất bại viễn chinh.</i>";
        }
    }
}
