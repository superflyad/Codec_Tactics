namespace CodecTactics.Core.Network;

public sealed class ConnectionState
{
    public ConnectionState(NodeId first, NodeId second, int strength = 2)
    {
        if (first.Equals(second))
        {
            throw new ArgumentException("A connection requires two distinct nodes.", nameof(second));
        }

        if (strength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(strength), "Connection strength cannot be negative.");
        }

        if (second.CompareTo(first) < 0)
        {
            (first, second) = (second, first);
        }

        First = first;
        Second = second;
        Strength = strength;
    }

    public NodeId First { get; }

    public NodeId Second { get; }

    public int Strength { get; private set; }

    public bool IsActive => Strength > 0;

    public bool Contains(NodeId nodeId) => First.Equals(nodeId) || Second.Equals(nodeId);

    public bool Connects(NodeId first, NodeId second)
    {
        if (second.CompareTo(first) < 0)
        {
            (first, second) = (second, first);
        }

        return First.Equals(first) && Second.Equals(second);
    }

    public NodeId GetOther(NodeId nodeId)
    {
        if (First.Equals(nodeId))
        {
            return Second;
        }

        if (Second.Equals(nodeId))
        {
            return First;
        }

        throw new InvalidOperationException($"Node {nodeId} is not part of this connection.");
    }

    public void Weaken()
    {
        if (Strength > 0)
        {
            Strength--;
        }
    }
}
