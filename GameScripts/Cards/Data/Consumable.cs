using System;
using System.Collections.Generic;
using UnityEngine;

public enum ConsumableEffectType
{
    BonusAttack,
    WorkSpeedMultiplier,
    RemoveDebuff,
    MaxHpBonus
}

[Serializable]
public class ConsumableEffectData
{
    public ConsumableEffectType EffectType;
    public float Value;
    public int DurationMonths;
}

[Serializable]
public class ActiveConsumableEffect
{
    public ConsumableEffectType EffectType;
    public float Value;
    public int RemainingMonths;
}

public class Consumable : CardData
{
    public bool IsSpoiling
    {
        get
        {
            return base.HasStatusEffectOfType<StatusEffect_Spoiling>();
        }
    }

    public override void UpdateCard()
    {
        if (!this.MyGameCard.IsDemoCard && this.MyGameCard.MyBoard.BoardOptions.FoodSpoils && !this.IsSpoiling && this.CanSpoil)
        {
            this.SpoilTime += Time.deltaTime * WorldManager.instance.TimeScale;
            float num = WorldManager.instance.MonthTime;
            if (this.IsCookedFood)
            {
                num = WorldManager.instance.MonthTime * 2f;
            }
            if (this.SpoilTime >= num)
            {
                base.AddStatusEffect(new StatusEffect_Spoiling());
            }
        }
        base.UpdateCard();
    }

    protected override bool CanHaveCard(CardData otherCard)
    {
        return otherCard is Consumable || otherCard.MyCardType == CardType.Resources;
    }

    [Header("Consumable Attributes")]
    public int HpRecovery;
    public int StaminaRecovery;
    public float ConsumeDuration = 3f;
    public List<ConsumableEffectData> Effects = new List<ConsumableEffectData>();

    [HideInInspector]
    [System.Obsolete("Temporary compatibility field only. Must not be referenced by new gameplay.")]
    public int FoodValue = 1;

    [HideInInspector]
    [System.Obsolete("Legacy Stacklands villager feeding.")]
    public bool CanBePlacedOnVillager;

    [ExtraData("spoil_time")]
    [HideInInspector]
    public float SpoilTime;

    public bool CanSpoil = true;

    [Header("Special Actions")]
    public string ResultAction;

    public string FullyConsumeResultAction;

    [HideInInspector]
    public bool IsReserved;

    [HideInInspector]
    public bool IsConsumed;
}
