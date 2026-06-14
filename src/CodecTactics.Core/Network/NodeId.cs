namespace CodecTactics.Core.Network;

public readonly record struct NodeId(int X, int Y) : IComparable<NodeId>
{
    public int CompareTo(NodeId other)
    {
        var yComparison = Y.CompareTo(other.Y);
        return yComparison != 0 ? yComparison : X.CompareTo(other.X);
    }

    public override string ToString() => $"({X},{Y})";
}
