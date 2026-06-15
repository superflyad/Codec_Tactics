namespace CodecTactics.Core.Network;

public sealed class NetworkBoard
{
    private static readonly IReadOnlyDictionary<NodeId, NodeType> DefaultNodeTypes = new Dictionary<NodeId, NodeType>
    {
        [new NodeId(1, 0)] = NodeType.Resource,
        [new NodeId(0, 1)] = NodeType.Relay,
        [new NodeId(2, 1)] = NodeType.Resource,
        [new NodeId(1, 2)] = NodeType.Relay,
        [new NodeId(2, 3)] = NodeType.Firewall
    };

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
        if (width < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "The board must be at least two nodes wide.");
        }

        if (height < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "The board must be at least two nodes high.");
        }

        var nodes = new List<NodeState>();
        var connections = new List<ConnectionState>();

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var nodeId = new NodeId(x, y);
                var type = width == 4 && height == 4 && DefaultNodeTypes.TryGetValue(nodeId, out var defaultType)
                    ? defaultType
                    : NodeType.Standard;

                nodes.Add(new NodeState(nodeId, type));

                if (x > 0)
                {
                    connections.Add(new ConnectionState(new NodeId(x - 1, y), new NodeId(x, y)));
                }

                if (y > 0)
                {
                    connections.Add(new ConnectionState(new NodeId(x, y - 1), new NodeId(x, y)));
                }
            }
        }

        return new NetworkBoard(width, height, nodes, connections);
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

    public bool IsReachableForPlayerClaim(NodeId nodeId)
    {
        if (HasAdjacentOwner(nodeId, NodeOwner.Player))
        {
            return true;
        }

        return Nodes
            .Where(node => node.Owner == NodeOwner.Player && node.Type == NodeType.Relay)
            .Any(relay => IsWithinActiveConnectionRange(relay.Id, nodeId, NetworkRules.RelayClaimRange));
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
