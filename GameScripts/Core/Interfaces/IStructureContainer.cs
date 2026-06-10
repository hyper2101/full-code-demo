using System.Collections.Generic;

public interface IStructureContainer
{
    /// <summary>
    /// Returns the SlotId of a valid slot for the given card, or null if none are valid or available.
    /// </summary>
    string GetValidSlotFor(CardData cardData);

    /// <summary>
    /// Returns the slot data by ID.
    /// </summary>
    StructureSlotData GetSlotById(string slotId);

    /// <summary>
    /// Returns all slots in this structure.
    /// </summary>
    IEnumerable<StructureSlotData> GetAllSlots();

    /// <summary>
    /// Callback triggered by the Core when a card is successfully attached to a slot.
    /// Used by the structure to handle gameplay-specific logic (e.g. start growing seed).
    /// </summary>
    void OnCardAttached(GameCard childCard, string slotId);

    /// <summary>
    /// Callback triggered by the Core when a card is successfully detached from a slot.
    /// Used by the structure to handle gameplay-specific logic (e.g. stop growing seed).
    /// </summary>
    void OnCardDetached(GameCard childCard, string slotId);
}
