using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mewtations.Expedition
{
    public enum NodeType
    {
        Combat,     // Standard fight
        Resource,   // Resource gathering
        Event,      // Dialogue events
        Ruins,      // Random encounters / mini puzzles
        Lore,       // Unlocking lore cards
        Boss        // Final progressive boss fight
    }

    public enum NodeState
    {
        Locked,
        Available,
        Visited
    }

    [Serializable]
    public class ExpeditionNode
    {
        public int Id;
        public int Layer; // Floor index (0 to MaxLayers)
        public int Position; // Horizontal slot on the layer
        public NodeType Type;
        public NodeState State = NodeState.Locked;
        public List<int> OutgoingConnections = new List<int>(); // List of destination Node IDs

        public ExpeditionNode(int id, int layer, int position, NodeType type)
        {
            Id = id;
            Layer = layer;
            Position = position;
            Type = type;
        }
    }

    [Serializable]
    public class Backpack
    {
        public int MaxCapacity = 10;
        public List<string> ContainedCardIds = new List<string>();

        public Backpack(int maxCapacity)
        {
            MaxCapacity = maxCapacity;
        }

        public bool IsFull => ContainedCardIds.Count >= MaxCapacity;

        public bool AddItem(string cardId)
        {
            if (IsFull) return false;
            ContainedCardIds.Add(cardId);
            return true;
        }

        public void RemoveItemAt(int index)
        {
            if (index >= 0 && index < ContainedCardIds.Count)
            {
                ContainedCardIds.RemoveAt(index);
            }
        }

        public void Clear()
        {
            ContainedCardIds.Clear();
        }
    }
}
