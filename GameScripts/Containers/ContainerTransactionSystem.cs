using System.Collections.Generic;
using UnityEngine;

public class ContainerTransactionSystem : MonoBehaviour
{
    public static ContainerTransactionSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public ContainerTransactionResult RequestInsert(GameCard cardToInsert, ICardContainer targetContainer, ContainerInsertContext context)
    {
        if (cardToInsert == null || targetContainer == null)
            return ContainerTransactionResult.Fail("Null card or container.");

        // Validate
        if (!targetContainer.CanInsert(cardToInsert, context))
        {
            return ContainerTransactionResult.Fail("Container rejected insertion based on rules.");
        }

        // Resolve Ownership
        // If the card is currently in another container, remove it first
        // In the future we will use the state to track current membership and remove it properly from the old container
        var currentState = RuntimeCardRegistry.Instance.GetState(cardToInsert.CardData.UniqueId);
        if (currentState != null)
        {
            // Update the runtime state to point to the new container's host
            // (Assumes targetContainer.HostCard has a UniqueId)
            currentState.ContainerId = targetContainer.HostCard != null ? targetContainer.HostCard.CardData.UniqueId : "board";
        }

        // Apply
        return targetContainer.Insert(cardToInsert, context);
        // Events are emitted by the container itself upon successful insert
    }
    
    public void RequestRemove(GameCard cardToRemove, ICardContainer sourceContainer)
    {
        if (cardToRemove == null || sourceContainer == null) return;
        
        var currentState = RuntimeCardRegistry.Instance.GetState(cardToRemove.CardData.UniqueId);
        if (currentState != null && currentState.ContainerId == (sourceContainer.HostCard != null ? sourceContainer.HostCard.CardData.UniqueId : "board"))
        {
            currentState.ContainerId = ""; // No longer in a container
        }
        
        sourceContainer.Remove(cardToRemove);
    }
}
