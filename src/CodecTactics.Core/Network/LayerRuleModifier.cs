namespace CodecTactics.Core.Network;

public sealed record LayerRuleModifier(
    int IntegrityBonus = 0,
    int ThreatBonus = 0,
    int CorruptionResistanceBonus = 0);
