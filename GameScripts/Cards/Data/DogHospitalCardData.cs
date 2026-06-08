using UnityEngine;
using System.Linq;

public class DogHospitalCardData : CardData
{
    public override bool CanHaveCardsWhileHasStatus()
    {
        return true;
    }

    protected override bool CanHaveCard(CardData otherCard)
    {
        if (otherCard is CatCardData cat)
        {
            return cat.IsParalyzed;
        }
        return false;
    }

    public override bool DetermineCanHaveCardsWhenIsRoot => true;

    public override void UpdateCard()
    {
        base.UpdateCard();
        
        var paralyzedCat = this.MyGameCard.GetAllCardsInStack().Select(c => c.CardData).OfType<CatCardData>().FirstOrDefault(c => c.IsParalyzed);
        if (paralyzedCat != null)
        {
            if (this.MyGameCard.TimerActionId != base.GetActionId("HealParalyzedCat"))
            {
                this.MyGameCard.StartTimer(60f, new TimerAction(this.HealParalyzedCat), "Chữa Liệt (60s)", base.GetActionId("HealParalyzedCat"));
            }
        }
        else
        {
            if (this.MyGameCard.TimerActionId == base.GetActionId("HealParalyzedCat"))
            {
                this.MyGameCard.CancelTimer(base.GetActionId("HealParalyzedCat"));
            }
        }
    }

    [TimedAction("heal_paralyzed_cat")]
    public void HealParalyzedCat()
    {
        CatCardData cat = this.MyGameCard.GetAllCardsInStack().Select(c => c.CardData).OfType<CatCardData>().FirstOrDefault(c => c.IsParalyzed);
        if (cat != null)
        {
            cat.IsParalyzed = false;
            cat.Stamina = 0; 
            cat.AddMemoir(Mewtations.Core.MewtationsLoc.Translate("dog_hospital_memoir", "Bệnh viện Chó: Phục hồi từ Tê liệt, nhưng cơ thể vẫn Kiệt sức."));
            
            if (GameScripts.Systems.DogTax.DogTaxEventManager.Instance != null)
            {
                GameScripts.Systems.DogTax.DogTaxEventManager.Instance.AddDebtAmount(2);
            }
            
            // Remove from hospital
            if (cat.MyGameCard != null)
            {
                cat.MyGameCard.RemoveFromStack();
                Vector2 spawnPos = cat.MyGameCard.transform.position;
                spawnPos.x += 1.5f;
                WorldManager.instance.SendToBoard(cat.MyGameCard, WorldManager.instance.CurrentBoard, spawnPos);
            }
        }
    }
}
