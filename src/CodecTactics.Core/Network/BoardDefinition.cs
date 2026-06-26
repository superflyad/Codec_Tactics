namespace CodecTactics.Core.Network;

public sealed class BoardDefinition
{
    private BoardDefinition(
        int width,
        int height,
        IEnumerable<NodeId> nodes,
        IReadOnlyDictionary<NodeId, NodeType> nodeTypes,
        IReadOnlyDictionary<NodeId, NodeOwner> initialOwnership,
        NodeId playerStart,
        IReadOnlyList<NodeId> corruptionStarts,
        int? startingPlayerEnergy,
        IReadOnlyDictionary<string, string> metadata)
    {
        Width = width;
        Height = height;
        Nodes = nodes.OrderBy(node => node).ToList();
        NodeTypes = new Dictionary<NodeId, NodeType>(nodeTypes);
        InitialOwnership = new Dictionary<NodeId, NodeOwner>(initialOwnership);
        PlayerStart = playerStart;
        CorruptionStarts = corruptionStarts.OrderBy(node => node).ToList();
        StartingPlayerEnergy = startingPlayerEnergy;
        Metadata = new Dictionary<string, string>(metadata);
    }

    public int Width { get; }

    public int Height { get; }

    public IReadOnlyList<NodeId> Nodes { get; }

    public IReadOnlyDictionary<NodeId, NodeType> NodeTypes { get; }

    public IReadOnlyDictionary<NodeId, NodeOwner> InitialOwnership { get; }

    public NodeId PlayerStart { get; }

    public IReadOnlyList<NodeId> CorruptionStarts { get; }

    public int? StartingPlayerEnergy { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }

    public IReadOnlyList<NodeId> ResourceNodes => GetNodesOfType(NodeType.Resource);

    public IReadOnlyList<NodeId> RelayNodes => GetNodesOfType(NodeType.Relay);

    public IReadOnlyList<NodeId> FirewallNodes => GetNodesOfType(NodeType.Firewall);

    public static BoardDefinition CreateDefaultPrototype()
    {
        var nodeTypes = new Dictionary<NodeId, NodeType>
        {
            [new NodeId(1, 0)] = NodeType.Resource,
            [new NodeId(0, 1)] = NodeType.Relay,
            [new NodeId(2, 1)] = NodeType.Resource,
            [new NodeId(1, 2)] = NodeType.Relay,
            [new NodeId(2, 3)] = NodeType.Firewall
        };

        return CreateGrid(
            4,
            4,
            new NodeId(0, 0),
            new[] { new NodeId(3, 3) },
            nodeTypes,
            metadata: new Dictionary<string, string>
            {
                ["scenario"] = "default-4x4-prototype",
                ["topology"] = "single-layer-grid"
            });
    }

    public static BoardDefinition CreateGrid(
        int width,
        int height,
        NodeId playerStart,
        IEnumerable<NodeId> corruptionStarts,
        IReadOnlyDictionary<NodeId, NodeType>? nodeTypes = null,
        int? startingPlayerEnergy = null,
        IReadOnlyDictionary<string, string>? metadata = null,
        IReadOnlyDictionary<NodeId, NodeOwner>? initialOwnership = null)
    {
        ValidateDimensions(width, height);

        var nodes = new List<NodeId>();
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                nodes.Add(new NodeId(x, y));
            }
        }

        var nodeSet = nodes.ToHashSet();
        ValidateNodeInLayout(playerStart, nodeSet, nameof(playerStart));

        var orderedCorruptionStarts = corruptionStarts
            .OrderBy(node => node)
            .ToList();
        if (orderedCorruptionStarts.Count == 0)
        {
            throw new ArgumentException("At least one corruption start is required.", nameof(corruptionStarts));
        }

        foreach (var corruptionStart in orderedCorruptionStarts)
        {
            ValidateNodeInLayout(corruptionStart, nodeSet, nameof(corruptionStarts));
        }

        if (orderedCorruptionStarts.Contains(playerStart))
        {
            throw new ArgumentException("Player and corruption starts must be distinct.", nameof(corruptionStarts));
        }

        var ownedNodeTypes = nodeTypes is null
            ? new Dictionary<NodeId, NodeType>()
            : new Dictionary<NodeId, NodeType>(nodeTypes);

        foreach (var nodeId in ownedNodeTypes.Keys)
        {
            ValidateNodeInLayout(nodeId, nodeSet, nameof(nodeTypes));
        }

        var ownedInitialOwnership = initialOwnership is null
            ? new Dictionary<NodeId, NodeOwner>()
            : new Dictionary<NodeId, NodeOwner>(initialOwnership);

        foreach (var nodeId in ownedInitialOwnership.Keys)
        {
            ValidateNodeInLayout(nodeId, nodeSet, nameof(initialOwnership));
        }

        ownedInitialOwnership[playerStart] = NodeOwner.Player;

        foreach (var corruptionStart in orderedCorruptionStarts)
        {
            ownedInitialOwnership[corruptionStart] = NodeOwner.Enemy;
        }

        return new BoardDefinition(
            width,
            height,
            nodes,
            ownedNodeTypes,
            ownedInitialOwnership,
            playerStart,
            orderedCorruptionStarts,
            startingPlayerEnergy,
            metadata ?? new Dictionary<string, string>());
    }

    private static void ValidateDimensions(int width, int height)
    {
        if (width < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "The board must be at least two nodes wide.");
        }

        if (height < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "The board must be at least two nodes high.");
        }
    }

    private static void ValidateNodeInLayout(NodeId nodeId, ISet<NodeId> nodes, string parameterName)
    {
        if (!nodes.Contains(nodeId))
        {
            throw new ArgumentException($"Node {nodeId} is outside the board definition.", parameterName);
        }
    }

    private IReadOnlyList<NodeId> GetNodesOfType(NodeType type)
    {
        return NodeTypes
            .Where(pair => pair.Value == type)
            .Select(pair => pair.Key)
            .OrderBy(node => node)
            .ToList();
    }
}
