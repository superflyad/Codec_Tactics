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
                nodes.Add(new NodeState(new NodeId(x, y)));

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
}
