using UnityEngine;

public enum ChildRelationType
{
    Stack,
    StructureSlot,
    Equipment,
    Hidden
}

public class CardOwnershipData
{
    public string ParentCardId;
    public string SlotId;
    public ChildRelationType RelationType;
    public Vector3 LocalOffset;
}
