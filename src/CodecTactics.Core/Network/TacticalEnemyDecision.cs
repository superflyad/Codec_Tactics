namespace CodecTactics.Core.Network;

public sealed record TacticalEnemyDecision(
    TacticalEnemyActionType ActionType,
    NodeId? Target,
    NodeId? Source,
    double Score,
    string PrimaryFactor,
    string Summary,
    IReadOnlyDictionary<string, double> Factors)
{
    public static TacticalEnemyDecision None { get; } = new(
        TacticalEnemyActionType.None,
        null,
        null,
        0d,
        "No access",
        "Corruption found no reachable pressure point.",
        new Dictionary<string, double>());
}
