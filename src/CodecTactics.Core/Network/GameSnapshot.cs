namespace CodecTactics.Core.Network;

public sealed record GameSnapshot(
    BoardDefinition BoardDefinition,
    GameConfiguration Configuration,
    MissionDefinition? MissionDefinition,
    int TurnNumber,
    int PlayerEnergy,
    int CorruptionPressure,
    TurnPhase Phase,
    GameResult Result,
    int ObjectiveHoldTurns,
    GameActionResult LastActionResult,
    TacticalEnemyDecision LastEnemyDecision,
    IReadOnlyList<NodeSnapshot> Nodes,
    IReadOnlyList<ConnectionSnapshot> Connections);

public sealed record NodeSnapshot(
    NodeId Id,
    NodeOwner Owner,
    int Integrity,
    int ReinforcementLevel,
    int Threat,
    int UnstableTurns,
    string DangerReason);

public sealed record ConnectionSnapshot(
    NodeId First,
    NodeId Second,
    int Strength);
