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
                        node.Biome = GetBiomeForLayer(layerIndex);
                        node.Theme = RollRouteTheme(type);
                        node.State = NodeState.Available; // Start layer is immediately available
                        layers[layerIndex].Add(node);
                        map.Add(node);
                    }
                }
                else if (layerIndex == maxLayers - 1)
                {
                    // Boss Layer: exactly 1 Boss node
                    var node = new ExpeditionNode(nextId++, layerIndex, 0, NodeType.Boss);
                    node.Biome = GetBiomeForLayer(layerIndex);
                    node.Theme = RouteTheme.Standard;
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
                        node.Biome = GetBiomeForLayer(layerIndex);
                        node.Theme = RollRouteTheme(type);
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

        private static ExpeditionBiome GetBiomeForLayer(int layerIndex)
        {
            if (layerIndex == 0 || layerIndex == 1) return ExpeditionBiome.Forest;
            if (layerIndex == 2) return ExpeditionBiome.Swamp;
            if (layerIndex == 3) return ExpeditionBiome.Peak;
            return ExpeditionBiome.Abyss;
        }

        private static NodeType RollNodeType(int layerIndex, int maxLayers)
        {
            var bag = new WeightedRandomBag<NodeType>();

            if (layerIndex == maxLayers - 2)
            {
                // Floor before boss: Combat, Elite, Ruins, or Extraction
                bag.AddEntry(NodeType.Combat, 40f);
                bag.AddEntry(NodeType.Elite, 20f);
                bag.AddEntry(NodeType.Extraction, 15f);
                bag.AddEntry(NodeType.Ruins, 15f);
                bag.AddEntry(NodeType.Event, 10f);
                return bag.Choose();
            }

            bag.AddEntry(NodeType.Combat, 30f);     // 30% combat
            bag.AddEntry(NodeType.Resource, 15f);   // 15% resource
            bag.AddEntry(NodeType.Elite, 10f);      // 10% elite
            bag.AddEntry(NodeType.Extraction, 10f); // 10% extraction portal
            bag.AddEntry(NodeType.SafeRetreat, 10f);// 10% safe retreat
            bag.AddEntry(NodeType.Event, 10f);      // 10% event/choice dialogue
            bag.AddEntry(NodeType.Altar, 5f);       // 5% Cat God's Altar
            bag.AddEntry(NodeType.Ruins, 5f);       // 5% ruins
            bag.AddEntry(NodeType.Lore, 5f);        // 5% lore/story card
            return bag.Choose();
        }

        private static RouteTheme RollRouteTheme(NodeType type)
        {
            if (type == NodeType.Boss || type == NodeType.Lore) return RouteTheme.Standard;

            var bag = new WeightedRandomBag<RouteTheme>();
            if (type == NodeType.Combat)
            {
                bag.AddEntry(RouteTheme.ThuTrieu, 25f);
                bag.AddEntry(RouteTheme.ThienLoi, 25f);
                bag.AddEntry(RouteTheme.TaDao, 25f);
                bag.AddEntry(RouteTheme.Standard, 25f);
                return bag.Choose();
            }
            else if (type == NodeType.Resource)
            {
                bag.AddEntry(RouteTheme.ThamLam, 40f);
                bag.AddEntry(RouteTheme.TaDao, 30f);
                bag.AddEntry(RouteTheme.Standard, 30f);
                return bag.Choose();
            }
            else if (type == NodeType.Altar || type == NodeType.Ruins)
            {
                bag.AddEntry(RouteTheme.TaDao, 50f);
                bag.AddEntry(RouteTheme.ThienLoi, 30f);
                bag.AddEntry(RouteTheme.Standard, 20f);
                return bag.Choose();
            }

            // Fallback
            bag.AddEntry(RouteTheme.TaDao, 20f);
            bag.AddEntry(RouteTheme.ThienLoi, 20f);
            bag.AddEntry(RouteTheme.ThamLam, 20f);
            bag.AddEntry(RouteTheme.ThuTrieu, 20f);
            bag.AddEntry(RouteTheme.Standard, 20f);
            return bag.Choose();
        }
    }
}
