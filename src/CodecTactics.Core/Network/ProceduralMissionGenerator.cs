namespace CodecTactics.Core.Network;

public static class ProceduralMissionGenerator
{
    public static MissionDefinition Generate(int seed, ProceduralMissionSettings? settings = null)
    {
        return Generate(ProceduralSeed.FromInt(seed), settings);
    }

    public static MissionDefinition Generate(string seed, ProceduralMissionSettings? settings = null)
    {
        return Generate(ProceduralSeed.FromText(seed), settings);
    }

    public static MissionDefinition Generate(ProceduralSeed seed, ProceduralMissionSettings? settings = null)
    {
        var generationSettings = settings ?? ProceduralMissionSettings.Default;
        generationSettings.Validate();

        var random = new DeterministicRandom(seed.Value);
        var depthCount = Math.Clamp(generationSettings.ObjectiveDistance + 1, 4, generationSettings.NodeCount - 2);
        var layers = BuildLayers(generationSettings.NodeCount, depthCount, random);
        var nodes = layers.SelectMany(layer => layer).OrderBy(node => node).ToList();
        var playerStart = layers[0][0];
        var objectiveNode = layers[^1][0];
        var links = BuildLinks(layers, generationSettings, random);
        var distances = CalculateDistances(nodes, links, playerStart);
        var corruptionStarts = SelectCorruptionStarts(nodes, objectiveNode, distances, generationSettings.CorruptionStartCount);
        var nodeTypes = SelectNodeTypes(nodes, playerStart, objectiveNode, corruptionStarts, distances, generationSettings, random);
        var layout = ProceduralNetworkLayout.CreateLayout(nodes, links, playerStart, objectiveNode, seed.Value);
        var width = nodes.Max(node => node.X) + 1;
        var height = nodes.Max(node => node.Y) + 1;
        var board = BoardDefinition.CreateTopology(
            width,
            height,
            nodes,
            links,
            playerStart,
            corruptionStarts,
            nodeTypes,
            generationSettings.StartingPlayerEnergy,
            new Dictionary<string, string>
            {
                ["scenario"] = "procedural-network",
                ["topology"] = "layered-infrastructure-graph",
                ["seed"] = seed.Value.ToString(),
                ["seedText"] = seed.Text,
                ["nodeCount"] = nodes.Count.ToString(),
                ["edgeCount"] = links.Count.ToString()
            },
            layout: layout);

        return new MissionDefinition(
            $"Generated Network {seed.Text}",
            board,
            objectiveNode,
            generationSettings.RequiredObjectiveHoldTurns,
            $"Trace seed {seed.Text}: claim the core objective and hold it for {generationSettings.RequiredObjectiveHoldTurns} player turns.");
    }

    private static IReadOnlyList<IReadOnlyList<NodeId>> BuildLayers(int nodeCount, int depthCount, DeterministicRandom random)
    {
        var layerCounts = Enumerable.Repeat(1, depthCount).ToArray();
        var remaining = nodeCount - depthCount;

        while (remaining > 0)
        {
            var layer = random.NextInclusive(1, depthCount - 1);
            var maxLayerSize = layer == depthCount - 1 ? 5 : 4;
            if (layerCounts[layer] >= maxLayerSize)
            {
                continue;
            }

            layerCounts[layer]++;
            remaining--;
        }

        var layers = new List<IReadOnlyList<NodeId>>();
        for (var depth = 0; depth < layerCounts.Length; depth++)
        {
            var layerNodes = new List<NodeId>();
            for (var lane = 0; lane < layerCounts[depth]; lane++)
            {
                layerNodes.Add(new NodeId(depth, lane));
            }

            layers.Add(layerNodes);
        }

        return layers;
    }

    private static IReadOnlyList<NetworkLink> BuildLinks(IReadOnlyList<IReadOnlyList<NodeId>> layers, ProceduralMissionSettings settings, DeterministicRandom random)
    {
        var links = new HashSet<NetworkLink>();
        var degree = layers.SelectMany(layer => layer).ToDictionary(node => node, _ => 0);

        for (var depth = 1; depth < layers.Count; depth++)
        {
            foreach (var node in layers[depth])
            {
                var previousLayer = layers[depth - 1];
                var anchors = previousLayer
                    .OrderBy(candidate => Math.Abs(candidate.Y - node.Y))
                    .ThenBy(_ => random.Next(int.MaxValue))
                    .ThenBy(candidate => candidate.Y)
                    .Take(Math.Min(2, previousLayer.Count))
                    .ToList();
                var anchor = anchors[random.Next(anchors.Count)];
                AddLink(links, degree, anchor, node, settings.MaxBranchingFactor + 1);
            }
        }

        for (var depth = 1; depth < layers.Count; depth++)
        {
            var previousLayer = layers[depth - 1];
            foreach (var node in layers[depth].OrderBy(node => node.Y))
            {
                var nearest = previousLayer.OrderBy(candidate => Math.Abs(candidate.Y - node.Y)).ThenBy(candidate => candidate.Y).First();
                AddLink(links, degree, nearest, node, settings.MaxBranchingFactor + 1);
            }
        }

        var candidates = new List<NetworkLink>();
        for (var depth = 0; depth < layers.Count; depth++)
        {
            foreach (var first in layers[depth])
            {
                foreach (var second in layers[Math.Min(depth + 1, layers.Count - 1)])
                {
                    if (!first.Equals(second) && Math.Abs(first.Y - second.Y) <= 1)
                    {
                        candidates.Add(new NetworkLink(first, second));
                    }
                }

                if (depth > 0)
                {
                    var sameLayer = layers[depth].Where(second => Math.Abs(second.Y - first.Y) == 1);
                    foreach (var second in sameLayer)
                    {
                        candidates.Add(new NetworkLink(first, second));
                    }
                }
            }
        }

        var targetEdges = Math.Max(links.Count, (int)Math.Round(settings.NodeCount * (1.08d + settings.GraphDensity)));
        foreach (var candidate in candidates.OrderBy(_ => random.Next(int.MaxValue)).ThenBy(link => link.First).ThenBy(link => link.Second))
        {
            if (links.Count >= targetEdges)
            {
                break;
            }

            AddLink(links, degree, candidate.First, candidate.Second, settings.MaxBranchingFactor);
        }

        return links.OrderBy(link => link.First).ThenBy(link => link.Second).ToList();
    }

    private static void AddLink(ISet<NetworkLink> links, IDictionary<NodeId, int> degree, NodeId first, NodeId second, int maxDegree)
    {
        if (first.Equals(second) || degree[first] >= maxDegree || degree[second] >= maxDegree)
        {
            return;
        }

        if (links.Add(new NetworkLink(first, second)))
        {
            degree[first]++;
            degree[second]++;
        }
    }

    private static IReadOnlyDictionary<NodeId, int> CalculateDistances(IReadOnlyList<NodeId> nodes, IReadOnlyList<NetworkLink> links, NodeId start)
    {
        var adjacency = nodes.ToDictionary(node => node, _ => new List<NodeId>());
        foreach (var link in links)
        {
            adjacency[link.First].Add(link.Second);
            adjacency[link.Second].Add(link.First);
        }

        var distances = new Dictionary<NodeId, int> { [start] = 0 };
        var frontier = new Queue<NodeId>();
        frontier.Enqueue(start);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            foreach (var next in adjacency[current].OrderBy(node => node))
            {
                if (distances.ContainsKey(next))
                {
                    continue;
                }

                distances[next] = distances[current] + 1;
                frontier.Enqueue(next);
            }
        }

        return distances;
    }

    private static IReadOnlyList<NodeId> SelectCorruptionStarts(
        IReadOnlyList<NodeId> nodes,
        NodeId objectiveNode,
        IReadOnlyDictionary<NodeId, int> distances,
        int count)
    {
        return nodes
            .Where(node => !node.Equals(objectiveNode))
            .OrderByDescending(node => distances.TryGetValue(node, out var distance) ? distance : -1)
            .ThenByDescending(node => node.X)
            .ThenBy(node => node.Y)
            .Take(count)
            .OrderBy(node => node)
            .ToList();
    }

    private static IReadOnlyDictionary<NodeId, NodeType> SelectNodeTypes(
        IReadOnlyList<NodeId> nodes,
        NodeId playerStart,
        NodeId objectiveNode,
        IReadOnlyList<NodeId> corruptionStarts,
        IReadOnlyDictionary<NodeId, int> distances,
        ProceduralMissionSettings settings,
        DeterministicRandom random)
    {
        var reserved = corruptionStarts.Append(playerStart).ToHashSet();
        var candidates = nodes
            .Where(node => !reserved.Contains(node) && !node.Equals(objectiveNode))
            .OrderBy(node => node.X)
            .ThenBy(node => node.Y)
            .ToList();
        var nodeTypes = new Dictionary<NodeId, NodeType>
        {
            [objectiveNode] = NodeType.Firewall
        };

        AssignType(nodeTypes, candidates, NodeType.Relay, Math.Max(1, (int)Math.Round(nodes.Count * settings.RelayFrequency)), random, node => distances[node] >= 1 && distances[node] <= Math.Max(2, settings.ObjectiveDistance - 1));
        AssignType(nodeTypes, candidates, NodeType.Resource, Math.Max(1, (int)Math.Round(nodes.Count * settings.ResourceFrequency)), random, node => distances[node] <= Math.Max(3, settings.ObjectiveDistance - 1));
        AssignType(nodeTypes, candidates, NodeType.Firewall, Math.Max(1, (int)Math.Round(nodes.Count * settings.FirewallFrequency)), random, node => distances[node] >= Math.Max(2, settings.ObjectiveDistance / 2));

        return nodeTypes;
    }

    private static void AssignType(
        IDictionary<NodeId, NodeType> nodeTypes,
        IEnumerable<NodeId> candidates,
        NodeType type,
        int count,
        DeterministicRandom random,
        Func<NodeId, bool> predicate)
    {
        foreach (var node in candidates
            .Where(node => !nodeTypes.ContainsKey(node) && predicate(node))
            .OrderBy(_ => random.Next(int.MaxValue))
            .ThenBy(node => node)
            .Take(count))
        {
            nodeTypes[node] = type;
        }
    }
}
