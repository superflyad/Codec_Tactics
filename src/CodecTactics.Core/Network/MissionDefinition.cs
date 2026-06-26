namespace CodecTactics.Core.Network;

public sealed record MissionDefinition(
    string Name,
    BoardDefinition BoardDefinition,
    NodeId ObjectiveNode,
    int RequiredObjectiveHoldTurns,
    string ObjectiveText)
{
    public static MissionDefinition CreateVerticalSlice()
    {
        var nodeTypes = new Dictionary<NodeId, NodeType>
        {
            [new NodeId(1, 1)] = NodeType.Relay,
            [new NodeId(2, 1)] = NodeType.Resource,
            [new NodeId(3, 1)] = NodeType.Firewall,
            [new NodeId(1, 2)] = NodeType.Resource,
            [new NodeId(2, 2)] = NodeType.Relay,
            [new NodeId(3, 2)] = NodeType.Firewall,
            [new NodeId(4, 2)] = NodeType.Resource
        };

        var boardDefinition = BoardDefinition.CreateGrid(
            5,
            5,
            new NodeId(0, 2),
            new[] { new NodeId(4, 4) },
            nodeTypes,
            startingPlayerEnergy: 6,
            metadata: new Dictionary<string, string>
            {
                ["scenario"] = "vertical-slice-uplink",
                ["topology"] = "single-layer-grid",
                ["objective"] = "secure-uplink"
            });

        return new MissionDefinition(
            "Secure the Uplink",
            boardDefinition,
            new NodeId(3, 2),
            2,
            "Claim the uplink and keep it secured for 2 player turns before corruption captures it.");
    }
}
