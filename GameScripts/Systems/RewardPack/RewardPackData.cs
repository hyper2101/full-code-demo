using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewRewardPack", menuName = "Dogma/Reward Pack Data")]
public class RewardPackData : ScriptableObject
{
    public string PackId;
    public string PackNameLocId;
    public Sprite Icon;
    public string DescriptionLocId;

    [Header("Generation Settings")]
    public bool GenerateOnSpawn = true;
    public int MinCards = 3;
    public int MaxCards = 5;
    
    [Header("Fallback Random Loot Pool")]
    public List<RewardPackEntry> Entries = new List<RewardPackEntry>();
    
    [Header("Guaranteed Fixed Cards (For Story/Boss/Legendary Packs)")]
    public List<RewardPackFixedEntry> GuaranteedEntries = new List<RewardPackFixedEntry>();
}

[Serializable]
public class RewardPackEntry
{
    public string CardId;
    public int Weight;
    public int MaxCopiesPerPack = 99;
}

[Serializable]
public class RewardPackFixedEntry
{
    public string CardId;
    public int Count = 1;
}
