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

    public int ReinforcementLevel { get; private set; }

    public int Threat { get; private set; }

    public int UnstableTurns { get; private set; }

    public bool IsUnstable => Owner == NodeOwner.Player && Threat > Integrity;

    public string DangerReason { get; private set; } = "Stable.";

    public void SetOwner(NodeOwner owner)
    {
        Owner = owner;
        Threat = 0;
        UnstableTurns = 0;
        DangerReason = owner == NodeOwner.Player ? DangerReason : "Stable.";
    }

    public void Reinforce()
    {
        ReinforcementLevel++;
        Integrity++;
    }

    public void SetNetworkRisk(int integrity, int threat, string dangerReason, bool advanceInstability)
    {
        Integrity = Math.Max(1, integrity);
        Threat = Math.Max(0, threat);
        DangerReason = dangerReason;

        if (Owner != NodeOwner.Player)
        {
            UnstableTurns = 0;
            return;
        }

        if (Threat > Integrity)
        {
            if (advanceInstability)
            {
                UnstableTurns++;
            }
        }
        else
        {
            UnstableTurns = 0;
        }
    }

    internal void RestoreSnapshot(NodeSnapshot snapshot)
    {
        if (!Id.Equals(snapshot.Id))
        {
            throw new ArgumentException($"Snapshot for {snapshot.Id} cannot restore node {Id}.", nameof(snapshot));
        }

        Owner = snapshot.Owner;
        Integrity = Math.Max(1, snapshot.Integrity);
        ReinforcementLevel = Math.Max(0, snapshot.ReinforcementLevel);
        Threat = Math.Max(0, snapshot.Threat);
        UnstableTurns = Math.Max(0, snapshot.UnstableTurns);
        DangerReason = string.IsNullOrWhiteSpace(snapshot.DangerReason) ? "Stable." : snapshot.DangerReason;
    }
}
