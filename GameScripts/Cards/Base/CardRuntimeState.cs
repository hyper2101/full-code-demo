using System;
using System.Collections.Generic;

[Serializable]
public class CardRuntimeState
{
    // Identity
    public string RuntimeId = Guid.NewGuid().ToString();
    public string UniqueId => RuntimeId;
    public string DefinitionId; // References CardData.Id
    
    // Ownership/Membership
    public string ContainerId; // Replaces ParentUniqueId to support arbitrary containers
    
    // Status
    public bool IsOn = true;
    public bool IsDamaged = false;
    public bool IsFoil = false;
    public bool IsShiny = false;
    public int WorkerIndex = -1;
    public int CreationMonth = 0;
    public CardDamageType DamageType = CardDamageType.None;
    
    // Stats & Modifiers
    public int CurrentHealth;
    public List<StatusEffect> StatusEffects = new List<StatusEffect>();

    public CardRuntimeState() {}

    public void InitFromCardData(CardData data)
    {
        this.RuntimeId = data.UniqueId;
        this.DefinitionId = data.Id;
        this.IsOn = data.IsOn;
        this.IsDamaged = data.IsDamaged;
        this.IsFoil = data.IsFoil;
        this.IsShiny = data.IsShiny;
        this.WorkerIndex = data.WorkerIndex;
        this.CreationMonth = data.CreationMonth;
        this.DamageType = data.DamageType;
    }
}
