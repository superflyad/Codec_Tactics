namespace CodecTactics.Core.Network;

public sealed class NodeState
{
    public NodeState(NodeId id, NodeType type = NodeType.Standard)
    {
        Id = id;
        Type = type;
    }

    public NodeId Id { get; }

    public NodeType Type { get; }

    public NodeOwner Owner { get; private set; } = NodeOwner.Neutral;

    public int Integrity { get; private set; } = 1;

    public void SetOwner(NodeOwner owner)
    {
        Owner = owner;
    }

    public void Reinforce()
    {
        Integrity++;
    }
}
