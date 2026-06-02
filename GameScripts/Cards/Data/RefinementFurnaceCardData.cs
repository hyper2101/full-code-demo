using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Mewtations.Legacy.Stacklands
{
    public class RefinementFurnaceCardData : CardData
    {
        public override bool UsesHorizontalSlots => true;

        protected override bool CanHaveCard(CardData otherCard)
        {
            if (otherCard is RefinementMaterialCardData)
            {
                int materialCount = 0;
                if (this.MyGameCard != null)
                {
                    foreach (var child in this.MyGameCard.GetChildCards())
                    {
                        if (child.CardData is RefinementMaterialCardData) materialCount++;
                    }
                }
                return materialCount < 1;
            }

            if (otherCard is IUpgradeableItem)
            {
                int itemCount = 0;
                if (this.MyGameCard != null)
                {
                    foreach (var child in this.MyGameCard.GetChildCards())
                    {
                        if (child.CardData is IUpgradeableItem) itemCount++;
                    }
                }
                return itemCount < 1;
            }

            return false;
        }

        public override void UpdateCard()
        {
            base.UpdateCard();
            if (this.MyGameCard == null) return;

            var children = this.MyGameCard.GetChildCards();
            GameCard targetItemCard = children.FirstOrDefault(c => c.CardData is IUpgradeableItem);
            GameCard materialCard = children.FirstOrDefault(c => c.CardData is RefinementMaterialCardData);

            if (targetItemCard != null && materialCard != null)
            {
                IUpgradeableItem item = targetItemCard.CardData as IUpgradeableItem;
                RefinementMaterialCardData material = materialCard.CardData as RefinementMaterialCardData;

                if (item != null && material != null)
                {
                    if (item.CanUpgrade(material))
                    {
                        this.MyGameCard.StartTimer(
                            10f, 
                            new TimerAction(ApplyUpgrade), 
                            SokLoc.Translate(MewtationsTerms.refinement_in_progress),
                            GetActionId("RefinementAction")
                        );
                    }
                    else
                    {
                        this.MyGameCard.CancelTimer(GetActionId("RefinementAction"));
                    }
                }
            }
            else
            {
                this.MyGameCard.CancelTimer(GetActionId("RefinementAction"));
            }
        }

        [TimedAction("refinement_upgrade")]
        public void ApplyUpgrade()
        {
            var children = this.MyGameCard.GetChildCards();
            GameCard targetItemCard = children.FirstOrDefault(c => c.CardData is IUpgradeableItem);
            GameCard materialCard = children.FirstOrDefault(c => c.CardData is RefinementMaterialCardData);

            if (targetItemCard == null || materialCard == null)
            {
                return;
            }

            IUpgradeableItem item = targetItemCard.CardData as IUpgradeableItem;
            RefinementMaterialCardData material = materialCard.CardData as RefinementMaterialCardData;

            if (item != null && material != null)
            {
                if (item.CanUpgrade(material))
                {
                    item.ApplyUpgrade(material);
                    
                    // Consume material
                    materialCard.DestroyCard(true, true);

                    AudioManager.me.PlaySound2D(AudioManager.me.CardCreate, 1f);
                    if (WorldManager.instance != null)
                    {
                        WorldManager.instance.CreateSmoke(targetItemCard.transform.position);
                    }
                    
                    if (Mewtations.Combat.Core.TurnBasedCombatManager.Instance != null)
                    {
                        Mewtations.Combat.Core.TurnBasedCombatManager.Instance.AddLog(SokLoc.Translate(MewtationsTerms.refinement_upgrade_success));
                    }
                }
                else
                {
                    if (Mewtations.Combat.Core.TurnBasedCombatManager.Instance != null)
                    {
                        Mewtations.Combat.Core.TurnBasedCombatManager.Instance.AddLog(SokLoc.Translate(MewtationsTerms.refinement_max_tier));
                    }
                }
            }
        }
    }
}
