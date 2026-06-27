namespace CodecTactics.Core.Network;

public sealed record ProceduralMissionSettings
{
    public int NodeCount { get; init; } = 18;

    public int ObjectiveDistance { get; init; } = 5;

    public int MaxBranchingFactor { get; init; } = 4;

    public double GraphDensity { get; init; } = 0.28d;

    public double ResourceFrequency { get; init; } = 0.16d;

    public double RelayFrequency { get; init; } = 0.16d;

    public double FirewallFrequency { get; init; } = 0.14d;

    public int CorruptionStartCount { get; init; } = 1;

    public int RequiredObjectiveHoldTurns { get; init; } = 2;

    public int StartingPlayerEnergy { get; init; } = 6;

    public static ProceduralMissionSettings Default { get; } = new();

    public void Validate()
    {
        if (NodeCount < 8)
        {
            throw new ArgumentOutOfRangeException(nameof(NodeCount), "Procedural missions require at least 8 nodes.");
        }

        if (ObjectiveDistance < 3)
        {
            throw new ArgumentOutOfRangeException(nameof(ObjectiveDistance), "Objective distance must be at least 3.");
        }

        if (MaxBranchingFactor < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxBranchingFactor), "Branching factor must be at least 2.");
        }

        if (CorruptionStartCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(CorruptionStartCount), "At least one corruption start is required.");
        }

        ValidateFrequency(GraphDensity, nameof(GraphDensity));
        ValidateFrequency(ResourceFrequency, nameof(ResourceFrequency));
        ValidateFrequency(RelayFrequency, nameof(RelayFrequency));
        ValidateFrequency(FirewallFrequency, nameof(FirewallFrequency));
    }

    private static void ValidateFrequency(double value, string parameterName)
    {
        if (value < 0d || value > 1d)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Frequencies must be between 0 and 1.");
        }
    }
}
