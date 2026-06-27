namespace CodecTactics.Core.Network;

public static class CorruptionTargetPolicy
{
    public static NodeId? SelectExpansionTarget(NetworkBoard board, GameConfiguration configuration)
    {
        var decision = TacticalEnemyPlanner.SelectDecision(
            board,
            configuration,
            board.Nodes.OrderBy(node => node.Id).FirstOrDefault(node => node.Owner == NodeOwner.Player)?.Id ?? new NodeId(0, 0),
            null,
            configuration.StandardCorruptionResistance,
            1);
        return decision.Target;
    }
}
