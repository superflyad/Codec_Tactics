namespace CodecTactics.Core.Network;

public sealed record CampaignArc(
    string Id,
    string Title,
    IReadOnlyList<CampaignArcMission> Missions);

public sealed record CampaignArcMission(
    int Stage,
    string Title,
    string Briefing,
    string RecoveryBriefing);
