using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mewtations.Expedition
{
    public static class ExpeditionMapGenerator
    {
        public static List<ExpeditionNode> GenerateMap(int seed, int maxLayers = 6, int maxNodesPerLayer = 3)
        {
            UnityEngine.Random.State oldState = UnityEngine.Random.state;
            UnityEngine.Random.InitState(seed);

            List<ExpeditionNode> map = new List<ExpeditionNode>();
            int nextId = 0;

            // List of nodes at each layer
            List<List<ExpeditionNode>> layers = new List<List<ExpeditionNode>>();

            // 1. Generate nodes for each layer
            for (int layerIndex = 0; layerIndex < maxLayers; layerIndex++)
            {
                layers.Add(new List<ExpeditionNode>());

                if (layerIndex == 0)
                {
                    // Start Layer: 2-3 nodes (Combat or Resource)
                    int nodeCount = UnityEngine.Random.Range(2, maxNodesPerLayer + 1);
                    for (int pos = 0; pos < nodeCount; pos++)
                    {
                        NodeType type = (UnityEngine.Random.value < 0.7f) ? NodeType.Combat : NodeType.Resource;
                        var node = new ExpeditionNode(nextId++, layerIndex, pos, type);
                        node.State = NodeState.Available; // Start layer is immediately available
                        layers[layerIndex].Add(node);
                        map.Add(node);
                    }
                }
                else if (layerIndex == maxLayers - 1)
                {
                    // Boss Layer: exactly 1 Boss node
                    var node = new ExpeditionNode(nextId++, layerIndex, 0, NodeType.Boss);
                    layers[layerIndex].Add(node);
                    map.Add(node);
                }
                else
                {
                    // Intermediate Layers: 2-3 nodes of mixed types
                    int nodeCount = UnityEngine.Random.Range(2, maxNodesPerLayer + 1);
                    for (int pos = 0; pos < nodeCount; pos++)
                    {
                        NodeType type = RollNodeType(layerIndex, maxLayers);
                        var node = new ExpeditionNode(nextId++, layerIndex, pos, type);
                        layers[layerIndex].Add(node);
                        map.Add(node);
                    }
                }
            }

            // 2. Procedurally generate connections between consecutive layers
            for (int layerIndex = 0; layerIndex < maxLayers - 1; layerIndex++)
            {
                var currentLayer = layers[layerIndex];
                var nextLayer = layers[layerIndex + 1];

                // Ensure every node in current layer connects to at least one node in next layer
                for (int i = 0; i < currentLayer.Count; i++)
                {
                    var currNode = currentLayer[i];
                    // Randomly pick 1 or 2 nodes in the next layer, preferring nearby positions to avoid extreme crossing lines
                    int targetCount = (nextLayer.Count > 1 && UnityEngine.Random.value < 0.4f) ? 2 : 1;
                    
                    HashSet<int> targets = new HashSet<int>();
                    while (targets.Count < targetCount)
                    {
                        int targetIdx = UnityEngine.Random.Range(0, nextLayer.Count);
                        targets.Add(targetIdx);
                    }

                    foreach (int targetIdx in targets)
                    {
                        currNode.OutgoingConnections.Add(nextLayer[targetIdx].Id);
                    }
                }

                // Ensure every node in next layer has at least one incoming connection (no orphan nodes)
                for (int j = 0; j < nextLayer.Count; j++)
                {
                    var nextNode = nextLayer[j];
                    bool hasIncoming = false;
                    foreach (var currNode in currentLayer)
                    {
                        if (currNode.OutgoingConnections.Contains(nextNode.Id))
                        {
                            hasIncoming = true;
                            break;
                        }
                    }

                    if (!hasIncoming)
                    {
                        // Connect the closest or random node from current layer to this next layer node
                        int randomCurrIdx = UnityEngine.Random.Range(0, currentLayer.Count);
                        currentLayer[randomCurrIdx].OutgoingConnections.Add(nextNode.Id);
                    }
                }
            }

            UnityEngine.Random.state = oldState; // Restore RNG state
            return map;
        }

        private static NodeType RollNodeType(int layerIndex, int maxLayers)
        {
            // Distribution change as we go deeper: Combat increases, lore/event are scattered
            float r = UnityEngine.Random.value;

            if (layerIndex == maxLayers - 2)
            {
                // Floor before boss: Combat or Ruins preferred
                if (r < 0.6f) return NodeType.Combat;
                if (r < 0.8f) return NodeType.Ruins;
                return NodeType.Event;
            }

            if (r < 0.40f) return NodeType.Combat;     // 40% combat
            if (r < 0.65f) return NodeType.Resource;   // 25% resource
            if (r < 0.80f) return NodeType.Event;      // 15% event/choice dialogue
            if (r < 0.90f) return NodeType.Ruins;      // 10% ruins
            return NodeType.Lore;                      // 10% lore/story card
        }
    }
}
