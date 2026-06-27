namespace CodecTactics.Core.Network;

public readonly record struct NetworkLink
{
    public NetworkLink(NodeId first, NodeId second)
    {
        if (first.Equals(second))
        {
            throw new ArgumentException("A network link requires two distinct nodes.", nameof(second));
        }

        if (second.CompareTo(first) < 0)
        {
            (first, second) = (second, first);
        }

        First = first;
        Second = second;
    }

    public NodeId First { get; }

    public NodeId Second { get; }

    public bool Contains(NodeId nodeId) => First.Equals(nodeId) || Second.Equals(nodeId);
}
