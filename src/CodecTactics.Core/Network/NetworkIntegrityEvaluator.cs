namespace CodecTactics.Core.Network;

public static class NetworkIntegrityEvaluator
{
    public static IReadOnlyList<NodeId> Evaluate(
        NetworkBoard board,
        NodeId playerCore,
        int corruptionPressure,
        GameConfiguration configuration,
        bool advanceInstability)
    {
        var collapsed = new List<NodeId>();

        foreach (var node in board.Nodes.OrderBy(node => node.Id))
        {
            if (node.Owner != NodeOwner.Player)
            {
                node.SetNetworkRisk(GetEnemyOrNeutralIntegrity(node, configuration), 0, "Stable.", advanceInstability: false);
                continue;
            }

            var ownedAdjacent = board.GetAdjacentNodes(node.Id)
                .Where(adjacent => adjacent.Owner == NodeOwner.Player)
                .ToList();
            var enemyAdjacent = board.GetAdjacentNodes(node.Id)
                .Where(adjacent => adjacent.Owner == NodeOwner.Enemy)
                .ToList();
            var neutralAdjacent = board.GetAdjacentNodes(node.Id)
                .Where(adjacent => adjacent.Owner == NodeOwner.Neutral)
                .ToList();
            var distanceFromCore = GetOwnedDistance(board, playerCore, node.Id);
            var connectedToCore = distanceFromCore.HasValue;
            var nearestCorruptionDistance = GetDistanceToOwner(board, node.Id, NodeOwner.Enemy);

            var integrity = configuration.BaseNetworkIntegrity
                + node.ReinforcementLevel
                + ownedAdjacent.Count * configuration.AdjacentSupportIntegrityBonus;

            if (connectedToCore)
            {
                integrity += configuration.CoreConnectionIntegrityBonus;
                integrity -= Math.Max(0, distanceFromCore!.Value - 1) * configuration.LongChainDistancePenalty;
            }
            else
            {
                integrity -= configuration.IsolationIntegrityPenalty;
            }

            if (node.Type == NodeType.Relay || ownedAdjacent.Any(adjacent => adjacent.Type == NodeType.Relay))
            {
                integrity += configuration.RelayIntegritySupport;
            }

            if (node.Type == NodeType.Firewall || ownedAdjacent.Any(adjacent => adjacent.Type == NodeType.Firewall))
            {
                integrity += configuration.FirewallIntegritySupport;
            }

            if (ownedAdjacent.Count >= configuration.DenseNetworkAdjacentThreshold)
            {
                integrity += configuration.DenseNetworkIntegrityBonus;
            }

            var threat = enemyAdjacent.Count * configuration.NearbyCorruptionThreat
                + neutralAdjacent.Count * configuration.FrontierExposureThreat
                + corruptionPressure / configuration.CorruptionPressureThreatDivisor;

            if (!connectedToCore)
            {
                threat += configuration.IsolationThreatPenalty;
            }

            if (ownedAdjacent.Count <= 1)
            {
                threat += configuration.WeakConnectionThreat;
            }

            if (nearestCorruptionDistance.HasValue)
            {
                threat += nearestCorruptionDistance.Value switch
                {
                    1 => 3,
                    2 => 2,
                    3 => 1,
                    _ => 0
                };
            }

            var reason = BuildDangerReason(connectedToCore, distanceFromCore, ownedAdjacent.Count, enemyAdjacent.Count, neutralAdjacent.Count, nearestCorruptionDistance);
            node.SetNetworkRisk(integrity, threat, reason, advanceInstability);

            if (node.UnstableTurns >= configuration.InstabilityTurnsBeforeCollapse)
            {
                collapsed.Add(node.Id);
            }
        }

        foreach (var nodeId in collapsed)
        {
            board.GetNode(nodeId).SetOwner(NodeOwner.Enemy);
        }

        return collapsed;
    }

    private static int GetEnemyOrNeutralIntegrity(NodeState node, GameConfiguration configuration)
    {
        return node.Type == NodeType.Firewall
            ? configuration.FirewallCorruptionResistance
            : configuration.StandardCorruptionResistance;
    }

    private static string BuildDangerReason(
        bool connectedToCore,
        int? distanceFromCore,
        int ownedConnections,
        int enemyConnections,
        int neutralConnections,
        int? nearestCorruptionDistance)
    {
        var reasons = new List<string>();

        if (!connectedToCore)
        {
            reasons.Add("isolated from core");
        }
        else if (distanceFromCore > 2)
        {
            reasons.Add($"long chain distance {distanceFromCore}");
        }

        if (ownedConnections <= 1)
        {
            reasons.Add("weak owned connections");
        }

        if (enemyConnections > 0)
        {
            reasons.Add($"{enemyConnections} adjacent corruption");
        }
        else if (nearestCorruptionDistance <= 3)
        {
            reasons.Add($"corruption distance {nearestCorruptionDistance}");
        }

        if (neutralConnections > 0)
        {
            reasons.Add($"{neutralConnections} frontier edges");
        }

        return reasons.Count == 0 ? "Dense connected structure." : string.Join("; ", reasons);
    }

    private static int? GetOwnedDistance(NetworkBoard board, NodeId start, NodeId target)
    {
        return GetDistance(board, start, target, node => node.Owner == NodeOwner.Player);
    }

    private static int? GetDistanceToOwner(NetworkBoard board, NodeId start, NodeOwner owner)
    {
        var visited = new HashSet<NodeId> { start };
        var frontier = new Queue<(NodeId Id, int Distance)>();
        frontier.Enqueue((start, 0));

        while (frontier.Count > 0)
        {
            var (current, distance) = frontier.Dequeue();
            foreach (var adjacent in board.GetAdjacentNodes(current))
            {
                if (!visited.Add(adjacent.Id))
                {
                    continue;
                }

                if (adjacent.Owner == owner)
                {
                    return distance + 1;
                }

                frontier.Enqueue((adjacent.Id, distance + 1));
            }
        }

        return null;
    }

    private static int? GetDistance(NetworkBoard board, NodeId start, NodeId target, Func<NodeState, bool> canTraverse)
    {
        if (!board.Contains(start) || !board.Contains(target) || !canTraverse(board.GetNode(start)) || !canTraverse(board.GetNode(target)))
        {
            return null;
        }

        var visited = new HashSet<NodeId> { start };
        var frontier = new Queue<(NodeId Id, int Distance)>();
        frontier.Enqueue((start, 0));

        while (frontier.Count > 0)
        {
            var (current, distance) = frontier.Dequeue();
            if (current.Equals(target))
            {
                return distance;
            }

            foreach (var adjacent in board.GetAdjacentNodes(current))
            {
                if (!canTraverse(adjacent) || !visited.Add(adjacent.Id))
                {
                    continue;
                }

                frontier.Enqueue((adjacent.Id, distance + 1));
            }
        }

        return null;
    }
}
