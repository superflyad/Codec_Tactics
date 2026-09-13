namespace CodecTactics.Core.Network;

public static class ScenarioCatalog
{
    public static IReadOnlyList<MissionDefinition> CreateBalancedRegressionScenarios()
    {
        return new[]
        {
            MissionDefinition.CreateVerticalSlice(),
            CreateRelayLadder(),
            CreateFirewallGate()
        };
    }

    public static MissionDefinition CreateRelayLadder()
    {
        var nodeTypes = new Dictionary<NodeId, NodeType>
        {
            [new NodeId(1, 1)] = NodeType.Resource,
            [new NodeId(2, 1)] = NodeType.Relay,
            [new NodeId(3, 1)] = NodeType.Relay,
            [new NodeId(4, 1)] = NodeType.Firewall,
            [new NodeId(2, 2)] = NodeType.Resource,
            [new NodeId(4, 2)] = NodeType.Firewall
        };

        var board = BoardDefinition.CreateGrid(
            6,
            4,
            new NodeId(0, 1),
            new[] { new NodeId(5, 3) },
            nodeTypes,
            startingPlayerEnergy: 7,
            metadata: new Dictionary<string, string>
            {
                ["scenario"] = "relay-ladder",
                ["topology"] = "single-layer-grid",
                ["balance"] = "regression-screen"
            });

        return new MissionDefinition(
            "Relay Ladder",
            board,
            new NodeId(3, 1),
            2,
            "Build through Relay anchors, then hold the ladder service before the far firewall falls.");
    }

    public static MissionDefinition CreateFirewallGate()
    {
        var nodeTypes = new Dictionary<NodeId, NodeType>
        {
            [new NodeId(1, 2)] = NodeType.Resource,
            [new NodeId(2, 1)] = NodeType.Relay,
            [new NodeId(2, 2)] = NodeType.Firewall,
            [new NodeId(2, 3)] = NodeType.Relay,
            [new NodeId(3, 2)] = NodeType.Firewall,
            [new NodeId(3, 1)] = NodeType.Resource,
            [new NodeId(3, 3)] = NodeType.Firewall
        };

        var board = BoardDefinition.CreateGrid(
            5,
            5,
            new NodeId(0, 2),
            new[] { new NodeId(4, 4) },
            nodeTypes,
            startingPlayerEnergy: 7,
            metadata: new Dictionary<string, string>
            {
                ["scenario"] = "firewall-gate",
                ["topology"] = "single-layer-grid",
                ["balance"] = "regression-screen"
            });

        return new MissionDefinition(
            "Firewall Gate",
            board,
            new NodeId(3, 2),
            1,
            "Secure the firewall gate before the corruption front reaches it.");
    }
}
