namespace CodecTactics.Core.Network;

public sealed record TacticalEnemyProfile(
    EnemyPersonality Personality,
    double ObjectiveProximity,
    double RelayValue,
    double ResourceValue,
    double NetworkControl,
    double CorruptionOpportunity,
    double PlayerExpansion,
    double DefensiveValue,
    double ReachableTerritory,
    double EnergyEfficiency,
    double FuturePositioning)
{
    public static TacticalEnemyProfile Create(EnemyPersonality personality)
    {
        return personality switch
        {
            EnemyPersonality.Aggressive => new TacticalEnemyProfile(personality, 1.35d, 0.8d, 0.7d, 1.1d, 1.55d, 2.2d, 0.55d, 1.0d, 1.25d, 1.35d),
            EnemyPersonality.Defensive => new TacticalEnemyProfile(personality, 1.05d, 1.25d, 0.85d, 1.45d, 0.9d, 0.65d, 2.25d, 0.85d, 1.1d, 1.05d),
            EnemyPersonality.Economic => new TacticalEnemyProfile(personality, 0.85d, 1.15d, 4.25d, 1.05d, 0.65d, 0.55d, 0.85d, 1.15d, 1.2d, 1.35d),
            EnemyPersonality.CorruptionFocused => new TacticalEnemyProfile(personality, 1.05d, 0.9d, 0.75d, 1.15d, 2.55d, 1.25d, 0.8d, 1.0d, 1.45d, 1.0d),
            _ => new TacticalEnemyProfile(personality, 1.25d, 1.1d, 1.1d, 1.25d, 1.45d, 1.35d, 1.1d, 1.2d, 1.2d, 1.25d)
        };
    }
}
