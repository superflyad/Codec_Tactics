namespace CodecTactics.Core.Network;

public sealed record GameConfiguration
{
    public int InitialPlayerEnergy { get; init; } = NetworkRules.InitialPlayerEnergy;

    public int ClaimEnergyCost { get; init; } = NetworkRules.ClaimEnergyCost;

    public int ReinforceEnergyCost { get; init; } = NetworkRules.ReinforceEnergyCost;

    public int WeakenConnectionEnergyCost { get; init; } = NetworkRules.WeakenConnectionEnergyCost;

    public int ResourceEnergyPerTurn { get; init; } = NetworkRules.ResourceEnergyPerTurn;

    public int RelayClaimRange { get; init; } = NetworkRules.RelayClaimRange;

    public int CorruptionPressureGrowthPerTurn { get; init; } = NetworkRules.CorruptionPressureGrowthPerTurn;

    public int StandardCorruptionResistance { get; init; } = NetworkRules.StandardCorruptionResistance;

    public int FirewallCorruptionResistance { get; init; } = NetworkRules.FirewallCorruptionResistance;

    public int BaseNetworkIntegrity { get; init; } = NetworkRules.BaseNetworkIntegrity;

    public int CoreConnectionIntegrityBonus { get; init; } = NetworkRules.CoreConnectionIntegrityBonus;

    public int IsolationIntegrityPenalty { get; init; } = NetworkRules.IsolationIntegrityPenalty;

    public int RelayIntegritySupport { get; init; } = NetworkRules.RelayIntegritySupport;

    public int FirewallIntegritySupport { get; init; } = NetworkRules.FirewallIntegritySupport;

    public int AdjacentSupportIntegrityBonus { get; init; } = NetworkRules.AdjacentSupportIntegrityBonus;

    public int DenseNetworkIntegrityBonus { get; init; } = NetworkRules.DenseNetworkIntegrityBonus;

    public int DenseNetworkAdjacentThreshold { get; init; } = NetworkRules.DenseNetworkAdjacentThreshold;

    public int LongChainDistancePenalty { get; init; } = NetworkRules.LongChainDistancePenalty;

    public int NearbyCorruptionThreat { get; init; } = NetworkRules.NearbyCorruptionThreat;

    public int CorruptionPressureThreatDivisor { get; init; } = NetworkRules.CorruptionPressureThreatDivisor;

    public int WeakConnectionThreat { get; init; } = NetworkRules.WeakConnectionThreat;

    public int FrontierExposureThreat { get; init; } = NetworkRules.FrontierExposureThreat;

    public int IsolationThreatPenalty { get; init; } = NetworkRules.IsolationThreatPenalty;

    public int InstabilityTurnsBeforeCollapse { get; init; } = NetworkRules.InstabilityTurnsBeforeCollapse;

    public int UnstableTargetPriority { get; init; } = NetworkRules.UnstableTargetPriority;

    public int LowIntegrityTargetPriorityAnchor { get; init; } = NetworkRules.LowIntegrityTargetPriorityAnchor;

    public int FirewallTargetPriorityPenalty { get; init; } = NetworkRules.FirewallTargetPriorityPenalty;
}
