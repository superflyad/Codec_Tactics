namespace CodecTactics.Core.Network;

public static class CorruptionTargetPolicy
{
    public static NodeId? SelectExpansionTarget(NetworkBoard board, GameConfiguration configuration)
    {
        return board.Nodes
            .Where(node => node.Owner == NodeOwner.Enemy)
            .OrderBy(node => node.Id)
            .SelectMany(enemyNode => board.GetAdjacentNodes(enemyNode.Id)
                .Where(adjacent => adjacent.Owner != NodeOwner.Enemy))
            .DistinctBy(node => node.Id)
            .OrderByDescending(node => GetPriority(node, configuration))
            .ThenBy(node => node.Id)
            .Select(node => node.Id)
            .Cast<NodeId?>()
            .FirstOrDefault();
    }

    private static int GetPriority(NodeState node, GameConfiguration configuration)
    {
        var priority = 0;

        if (node.IsUnstable)
        {
            priority += configuration.UnstableTargetPriority;
        }

        priority += Math.Max(0, configuration.LowIntegrityTargetPriorityAnchor - node.Integrity);
        priority += node.Threat;

        if (node.Type == NodeType.Firewall)
        {
            priority -= configuration.FirewallTargetPriorityPenalty;
        }

        return priority;
    }
}
