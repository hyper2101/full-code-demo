using System;
using System.Collections.Generic;

[Serializable]
public class RewardPackInstance
{
    public string PackId;
    public List<string> GeneratedCards = new List<string>();
    public int OpenedCount;
}

[Serializable]
public class RewardPackSaveData
{
    public List<RewardPackInstance> Packs = new List<RewardPackInstance>();
}
