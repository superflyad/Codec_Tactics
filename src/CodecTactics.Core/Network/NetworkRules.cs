namespace CodecTactics.Core.Network;

public static class NetworkRules
{
    public const int InitialPlayerEnergy = 5;
    public const int ClaimEnergyCost = 2;
    public const int ReinforceEnergyCost = 1;
    public const int ResourceEnergyPerTurn = 2;
    public const int RelayClaimRange = 2;
    public const int StandardCorruptionResistance = 1;
    public const int FirewallCorruptionResistance = 2;
    public const int BaseNetworkIntegrity = 4;
    public const int CoreConnectionIntegrityBonus = 3;
    public const int IsolationIntegrityPenalty = 4;
    public const int RelayIntegritySupport = 2;
    public const int FirewallIntegritySupport = 3;
    public const int AdjacentSupportIntegrityBonus = 1;
    public const int DenseNetworkIntegrityBonus = 2;
    public const int LongChainDistancePenalty = 1;
    public const int NearbyCorruptionThreat = 4;
    public const int CorruptionPressureThreatDivisor = 2;
    public const int WeakConnectionThreat = 2;
    public const int FrontierExposureThreat = 1;
    public const int IsolationThreatPenalty = 4;
    public const int InstabilityTurnsBeforeCollapse = 2;
}
