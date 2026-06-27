namespace CodecTactics.Core.Network;

public sealed class NetworkBoard
{
    private readonly Dictionary<NodeId, NodeState> _nodes;
    private readonly List<ConnectionState> _connections;

    private NetworkBoard(int width, int height, IEnumerable<NodeState> nodes, IEnumerable<ConnectionState> connections)
    {
        Width = width;
        Height = height;
        _nodes = nodes.ToDictionary(node => node.Id);
        _connections = connections.ToList();
    }

    public int Width { get; }

    public int Height { get; }

    public IReadOnlyCollection<NodeState> Nodes => _nodes.Values;

    public IReadOnlyCollection<ConnectionState> Connections => _connections;

    public static NetworkBoard CreateGrid(int width = 4, int height = 4)
    {
        return width == 4 && height == 4
            ? FromDefinition(BoardDefinition.CreateDefaultPrototype())
            : FromDefinition(BoardDefinition.CreateGrid(width, height, new NodeId(0, 0), new[] { new NodeId(width - 1, height - 1) }));
    }

    public static NetworkBoard FromDefinition(BoardDefinition definition)
    {
        var nodes = new List<NodeState>();
        var connections = new List<ConnectionState>();

        foreach (var nodeId in definition.Nodes)
        {
            var type = definition.NodeTypes.TryGetValue(nodeId, out var configuredType)
                ? configuredType
                : NodeType.Standard;
            var node = new NodeState(nodeId, type);
            if (definition.InitialOwnership.TryGetValue(nodeId, out var owner))
            {
                node.SetOwner(owner);
            }

            nodes.Add(node);
        }

        connections.AddRange(definition.Links.Select(link => new ConnectionState(link.First, link.Second)));

        return new NetworkBoard(definition.Width, definition.Height, nodes, connections);
    }

    public NodeState GetNode(NodeId nodeId)
    {
        if (_nodes.TryGetValue(nodeId, out var node))
        {
            return node;
        }

        throw new InvalidOperationException($"Node {nodeId} does not exist on the board.");
    }

    public bool Contains(NodeId nodeId) => _nodes.ContainsKey(nodeId);

    public IReadOnlyList<NodeState> GetAdjacentNodes(NodeId nodeId)
    {
        if (!Contains(nodeId))
        {
            throw new InvalidOperationException($"Node {nodeId} does not exist on the board.");
        }

        return _connections
            .Where(connection => connection.IsActive && connection.Contains(nodeId))
            .Select(connection => GetNode(connection.GetOther(nodeId)))
            .OrderBy(node => node.Id)
            .ToList();
    }

    public bool AreConnected(NodeId first, NodeId second)
    {
        return FindConnection(first, second)?.IsActive == true;
    }

    public ConnectionState? FindConnection(NodeId first, NodeId second)
    {
        return _connections.FirstOrDefault(connection => connection.Connects(first, second));
    }

    public bool HasAdjacentOwner(NodeId nodeId, NodeOwner owner)
    {
        return GetAdjacentNodes(nodeId).Any(node => node.Owner == owner);
    }

    public bool IsReachableForPlayerClaim(NodeId nodeId, GameConfiguration configuration)
    {
        if (HasAdjacentOwner(nodeId, NodeOwner.Player))
        {
            return true;
        }

        return Nodes
            .Where(node => node.Owner == NodeOwner.Player && node.Type == NodeType.Relay)
            .Any(relay => IsWithinActiveConnectionRange(relay.Id, nodeId, configuration.RelayClaimRange));
    }

    private bool IsWithinActiveConnectionRange(NodeId start, NodeId target, int maxDistance)
    {
        var visited = new HashSet<NodeId> { start };
        var frontier = new Queue<(NodeId Id, int Distance)>();
        frontier.Enqueue((start, 0));

        while (frontier.Count > 0)
        {
            var (current, distance) = frontier.Dequeue();
            if (distance >= maxDistance)
            {
                continue;
            }

            foreach (var adjacent in GetAdjacentNodes(current))
            {
                if (!visited.Add(adjacent.Id))
                {
                    continue;
                }

                if (adjacent.Id.Equals(target))
                {
                    return true;
                }

                frontier.Enqueue((adjacent.Id, distance + 1));
            }
        }

        return false;
    }
}
