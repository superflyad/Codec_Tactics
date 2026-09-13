namespace CodecTactics.Core.Network;

public static class CorruptionTargetPolicy
{
    public static NodeId? SelectExpansionTarget(NetworkBoard board, GameConfiguration configuration)
    {
        var decision = TacticalEnemyPlanner.SelectDecision(
            CreateFlatDefinition(board),
            board,
            configuration,
            board.Nodes.OrderBy(node => node.Id).FirstOrDefault(node => node.Owner == NodeOwner.Player)?.Id ?? new NodeId(0, 0),
            null,
            configuration.StandardCorruptionResistance,
            1);
        return decision.Target;
    }

    private static BoardDefinition CreateFlatDefinition(NetworkBoard board)
    {
        var nodes = board.Nodes.Select(node => node.Id).ToList();
        var links = board.Connections.Select(connection => new NetworkLink(connection.First, connection.Second)).ToList();
        var playerStart = board.Nodes.OrderBy(node => node.Id).FirstOrDefault(node => node.Owner == NodeOwner.Player)?.Id ?? nodes[0];
        var corruptionStarts = board.Nodes
            .Where(node => node.Owner == NodeOwner.Enemy)
            .Select(node => node.Id)
            .DefaultIfEmpty(nodes[^1])
            .ToList();

        return BoardDefinition.CreateTopology(
            Math.Max(2, nodes.Max(node => node.X) + 1),
            Math.Max(2, nodes.Max(node => node.Y) + 1),
            nodes,
            links,
            playerStart,
            corruptionStarts,
            board.Nodes.ToDictionary(node => node.Id, node => node.Type),
            initialOwnership: board.Nodes.ToDictionary(node => node.Id, node => node.Owner));
    }
}
