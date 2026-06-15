namespace CodecTactics.Core.Network;

public static class CorruptionTargetPolicy
{
    public static NodeId? SelectExpansionTarget(NetworkBoard board)
    {
        return board.Nodes
            .Where(node => node.Owner == NodeOwner.Enemy)
            .OrderBy(node => node.Id)
            .SelectMany(enemyNode => board.GetAdjacentNodes(enemyNode.Id)
                .Where(adjacent => adjacent.Owner != NodeOwner.Enemy))
            .DistinctBy(node => node.Id)
            .OrderByDescending(GetPriority)
            .ThenBy(node => node.Id)
            .Select(node => node.Id)
            .Cast<NodeId?>()
            .FirstOrDefault();
    }

    private static int GetPriority(NodeState node)
    {
        var priority = 0;

        if (node.IsUnstable)
        {
            priority += 100;
        }

        priority += Math.Max(0, 20 - node.Integrity);
        priority += node.Threat;

        if (node.Type == NodeType.Firewall)
        {
            priority -= 4;
        }

        return priority;
    }
}
