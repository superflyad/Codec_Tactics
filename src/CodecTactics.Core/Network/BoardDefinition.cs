namespace CodecTactics.Core.Network;

public sealed class BoardDefinition
{
    private BoardDefinition(
        int width,
        int height,
        IEnumerable<NodeId> nodes,
        IReadOnlyList<NetworkLink> links,
        IReadOnlyDictionary<NodeId, NodeType> nodeTypes,
        IReadOnlyDictionary<NodeId, NodeOwner> initialOwnership,
        NodeId playerStart,
        IReadOnlyList<NodeId> corruptionStarts,
        int? startingPlayerEnergy,
        IReadOnlyDictionary<NodeId, NetworkNodePosition> layout,
        IReadOnlyDictionary<string, string> metadata)
    {
        Width = width;
        Height = height;
        Nodes = nodes.OrderBy(node => node).ToList();
        Links = links.OrderBy(link => link.First).ThenBy(link => link.Second).ToList();
        NodeTypes = new Dictionary<NodeId, NodeType>(nodeTypes);
        InitialOwnership = new Dictionary<NodeId, NodeOwner>(initialOwnership);
        PlayerStart = playerStart;
        CorruptionStarts = corruptionStarts.OrderBy(node => node).ToList();
        StartingPlayerEnergy = startingPlayerEnergy;
        Layout = new Dictionary<NodeId, NetworkNodePosition>(layout);
        Metadata = new Dictionary<string, string>(metadata);
    }

    public int Width { get; }

    public int Height { get; }

    public IReadOnlyList<NodeId> Nodes { get; }

    public IReadOnlyList<NetworkLink> Links { get; }

    public IReadOnlyDictionary<NodeId, NodeType> NodeTypes { get; }

    public IReadOnlyDictionary<NodeId, NodeOwner> InitialOwnership { get; }

    public NodeId PlayerStart { get; }

    public IReadOnlyList<NodeId> CorruptionStarts { get; }

    public int? StartingPlayerEnergy { get; }

    public IReadOnlyDictionary<NodeId, NetworkNodePosition> Layout { get; }

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

        return CreateTopology(
            width,
            height,
            nodes,
            CreateGridLinks(width, height),
            playerStart,
            orderedCorruptionStarts,
            ownedNodeTypes,
            startingPlayerEnergy,
            metadata,
            ownedInitialOwnership,
            CreateGridLayout(nodes));
    }

    public static BoardDefinition CreateTopology(
        int width,
        int height,
        IEnumerable<NodeId> nodes,
        IEnumerable<NetworkLink> links,
        NodeId playerStart,
        IEnumerable<NodeId> corruptionStarts,
        IReadOnlyDictionary<NodeId, NodeType>? nodeTypes = null,
        int? startingPlayerEnergy = null,
        IReadOnlyDictionary<string, string>? metadata = null,
        IReadOnlyDictionary<NodeId, NodeOwner>? initialOwnership = null,
        IReadOnlyDictionary<NodeId, NetworkNodePosition>? layout = null)
    {
        ValidateDimensions(width, height);

        var orderedNodes = nodes
            .OrderBy(node => node)
            .Distinct()
            .ToList();
        if (orderedNodes.Count == 0)
        {
            throw new ArgumentException("At least one node is required.", nameof(nodes));
        }

        var nodeSet = orderedNodes.ToHashSet();
        foreach (var nodeId in orderedNodes)
        {
            ValidateNodeInBounds(nodeId, width, height, nameof(nodes));
        }

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

        var orderedLinks = links
            .Distinct()
            .OrderBy(link => link.First)
            .ThenBy(link => link.Second)
            .ToList();
        if (orderedLinks.Count == 0)
        {
            throw new ArgumentException("At least one network link is required.", nameof(links));
        }

        foreach (var link in orderedLinks)
        {
            ValidateNodeInLayout(link.First, nodeSet, nameof(links));
            ValidateNodeInLayout(link.Second, nodeSet, nameof(links));
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

        var ownedLayout = layout is null
            ? new Dictionary<NodeId, NetworkNodePosition>()
            : new Dictionary<NodeId, NetworkNodePosition>(layout);

        foreach (var nodeId in ownedLayout.Keys)
        {
            ValidateNodeInLayout(nodeId, nodeSet, nameof(layout));
        }

        return new BoardDefinition(
            width,
            height,
            orderedNodes,
            orderedLinks,
            ownedNodeTypes,
            ownedInitialOwnership,
            playerStart,
            orderedCorruptionStarts,
            startingPlayerEnergy,
            ownedLayout,
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

    private static void ValidateNodeInBounds(NodeId nodeId, int width, int height, string parameterName)
    {
        if (nodeId.X < 0 || nodeId.X >= width || nodeId.Y < 0 || nodeId.Y >= height)
        {
            throw new ArgumentException($"Node {nodeId} is outside the board dimensions.", parameterName);
        }
    }

    private static IReadOnlyList<NetworkLink> CreateGridLinks(int width, int height)
    {
        var links = new List<NetworkLink>();
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var nodeId = new NodeId(x, y);
                if (x > 0)
                {
                    links.Add(new NetworkLink(new NodeId(x - 1, y), nodeId));
                }

                if (y > 0)
                {
                    links.Add(new NetworkLink(new NodeId(x, y - 1), nodeId));
                }
            }
        }

        return links;
    }

    private static IReadOnlyDictionary<NodeId, NetworkNodePosition> CreateGridLayout(IEnumerable<NodeId> nodes)
    {
        return nodes.ToDictionary(node => node, node => new NetworkNodePosition(node.X, node.Y));
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
