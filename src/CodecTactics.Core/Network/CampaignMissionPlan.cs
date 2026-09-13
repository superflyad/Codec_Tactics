namespace CodecTactics.Core.Network;

public sealed record CampaignMissionPlan(
    int Stage,
    ProceduralSeed Seed,
    ProceduralMissionSettings MissionSettings,
    GameConfiguration Configuration,
    string Briefing,
    string ArcId = "",
    string ArcTitle = "",
    string MissionTitle = "",
    string BranchLabel = "",
    string BranchSummary = "");
