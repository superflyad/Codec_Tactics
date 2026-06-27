namespace CodecTactics.Core.Network;

public static class ProceduralNetworkLayout
{
    public static IReadOnlyDictionary<NodeId, NetworkNodePosition> CreateLayout(
        IReadOnlyList<NodeId> nodes,
        IReadOnlyList<NetworkLink> links,
        NodeId playerStart,
        NodeId objectiveNode,
        int seed)
    {
        var random = new DeterministicRandom(seed ^ unchecked((int)0xA53A9B1Du));
        var byDepth = nodes
            .GroupBy(node => node.X)
            .OrderBy(group => group.Key)
            .ToDictionary(group => group.Key, group => group.OrderBy(node => node.Y).ToList());

        var positions = new Dictionary<NodeId, NetworkNodePosition>();
        const float depthSpacing = 190f;
        const float laneSpacing = 136f;

        foreach (var (depth, depthNodes) in byDepth)
        {
            var center = (depthNodes.Count - 1) / 2f;
            for (var i = 0; i < depthNodes.Count; i++)
            {
                var node = depthNodes[i];
                var jitterX = node.Equals(playerStart) || node.Equals(objectiveNode) ? 0f : (float)(random.NextDouble() - 0.5d) * 34f;
                var jitterY = node.Equals(playerStart) || node.Equals(objectiveNode) ? 0f : (float)(random.NextDouble() - 0.5d) * 26f;
                positions[node] = new NetworkNodePosition(depth * depthSpacing + jitterX, (i - center) * laneSpacing + jitterY);
            }
        }

        ReduceOverlap(nodes, links, positions);
        return positions;
    }

    private static void ReduceOverlap(
        IReadOnlyList<NodeId> nodes,
        IReadOnlyList<NetworkLink> links,
        IDictionary<NodeId, NetworkNodePosition> positions)
    {
        var degree = nodes.ToDictionary(node => node, _ => 0);
        foreach (var link in links)
        {
            degree[link.First]++;
            degree[link.Second]++;
        }

        foreach (var node in nodes.OrderBy(node => node.X).ThenByDescending(node => degree[node]).ThenBy(node => node.Y))
        {
            var position = positions[node];
            var offset = (degree[node] - 2) * 8f;
            if (Math.Abs(offset) <= 0f)
            {
                continue;
            }

            positions[node] = new NetworkNodePosition(position.X, position.Y + (node.Y % 2 == 0 ? offset : -offset));
        }
    }
}
