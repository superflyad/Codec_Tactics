namespace CodecTactics.Core.Network;

public sealed record CampaignMissionRecord(
    ProceduralSeed Seed,
    GameResult Result,
    int TurnNumber,
    EnemyPersonality EnemyPersonality,
    int Stage = 1);
