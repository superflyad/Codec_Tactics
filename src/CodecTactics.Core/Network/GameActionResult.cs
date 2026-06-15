namespace CodecTactics.Core.Network;

public sealed record GameActionResult(
    bool Succeeded,
    string Message,
    int EnergySpent = 0,
    int EnergyGenerated = 0,
    NodeId? CorruptionTarget = null,
    NodeId? CorruptionFocusTarget = null,
    IReadOnlyList<NodeId>? CollapsedNodes = null);
