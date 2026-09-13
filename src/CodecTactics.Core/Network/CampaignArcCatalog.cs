namespace CodecTactics.Core.Network;

public static class CampaignArcCatalog
{
    private static readonly CampaignArc SignalRecovery = new(
        "signal-recovery",
        "Signal Recovery",
        new[]
        {
            new CampaignArcMission(
                1,
                "Wake the Lattice",
                "Reclaim the entry mesh and prove the network can still route clean signal.",
                "Re-enter the damaged mesh with extra charge and rebuild a stable foothold."),
            new CampaignArcMission(
                2,
                "Relay the Spine",
                "Push through the relay spine before corruption learns the new route.",
                "Reopen the spine from a safer entry budget after the last trace collapsed."),
            new CampaignArcMission(
                3,
                "Gate the Firewall",
                "Secure the first firewall gate and hold it long enough to seal the breach.",
                "Approach the firewall gate with reserve energy and fewer assumptions."),
            new CampaignArcMission(
                4,
                "Cross the Backplane",
                "Cross a denser backplane where every expansion exposes a new pressure path.",
                "Stabilize the backplane with a recovery trace before pressing deeper."),
            new CampaignArcMission(
                5,
                "Split the Intrusion",
                "Contain two intrusion fronts while preserving a route to the objective.",
                "Rebuild from the split-front failure with a small recovery cushion."),
            new CampaignArcMission(
                6,
                "Thread the Deep Bus",
                "Thread deeper infrastructure without letting the enemy isolate the core.",
                "Use the recovery trace to keep the core connected through the deep bus."),
            new CampaignArcMission(
                7,
                "Burn the Mirror",
                "Break the mirrored corruption pattern before it turns your relays against you.",
                "Reroute through the mirror with enough energy to repair the first break."),
            new CampaignArcMission(
                8,
                "Seal the Kernel",
                "Take the final kernel seal and hold it under maximum corruption pressure.",
                "Return to the kernel seal with recovery charge and finish the containment.")
        });

    public static IReadOnlyList<CampaignArc> All { get; } = new[] { SignalRecovery };

    public static CampaignArc Default => SignalRecovery;

    public static CampaignArcMission GetMission(int stage)
    {
        var boundedStage = Math.Clamp(stage, 1, Default.Missions.Count);
        return Default.Missions.First(mission => mission.Stage == boundedStage);
    }
}
