using System.Collections.Generic;
using UnityEngine;

public enum OccupancyPolicy
{
    Single,
    Stack
}

public class StructureSlotData
{
    public string SlotId;
    public Vector3 LocalOffset;
    public List<string> AcceptedTypes = new List<string>();
    public List<GameCard> SlotOccupants = new List<GameCard>();
    public OccupancyPolicy OccupancyPolicy = OccupancyPolicy.Single;
    public bool IsLocked;
}
