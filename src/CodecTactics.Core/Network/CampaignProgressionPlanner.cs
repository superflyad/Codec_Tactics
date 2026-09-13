namespace CodecTactics.Core.Network;

public static class CampaignProgressionPlanner
{
    public static CampaignMissionPlan CreateInitialPlan()
    {
        return CreatePlan(Array.Empty<CampaignMissionRecord>());
    }

    public static CampaignMissionPlan CreatePlan(IEnumerable<CampaignMissionRecord> completedMissions)
    {
        var history = completedMissions.ToList();
        var wins = history.Count(mission => mission.Result == GameResult.PlayerWin);
        var losses = history.Count(mission => mission.Result == GameResult.PlayerLoss);
        var lastResult = history.LastOrDefault()?.Result ?? GameResult.InProgress;
        var stage = Math.Clamp(wins + 1, 1, 8);
        var recoveryAttempt = lastResult == GameResult.PlayerLoss;
        var seed = BuildSeed(stage, history.Count, lastResult);
        var arc = CampaignArcCatalog.Default;
        var arcMission = CampaignArcCatalog.GetMission(stage);

        var missionSettings = ProceduralMissionSettings.Default with
        {
            NodeCount = 18 + Math.Min(stage - 1, 5) * 2,
            ObjectiveDistance = Math.Min(8, 5 + (stage - 1) / 2),
            MaxBranchingFactor = Math.Min(5, 3 + stage / 3),
            GraphDensity = Math.Min(0.38d, 0.24d + stage * 0.025d),
            CorruptionStartCount = stage >= 5 ? 2 : 1,
            RequiredObjectiveHoldTurns = stage >= 4 ? 3 : 2,
            StartingPlayerEnergy = 6 + (recoveryAttempt ? 1 : 0)
        };

        var difficulty = stage switch
        {
            <= 1 => EnemyDifficulty.Standard,
            <= 3 => EnemyDifficulty.Hard,
            _ => EnemyDifficulty.Expert
        };

        var configuration = new GameConfiguration
        {
            EnemyPersonality = SelectPersonality(seed),
            EnemyDifficulty = difficulty,
            CorruptionPressureGrowthPerTurn = recoveryAttempt ? 1 : NetworkRules.CorruptionPressureGrowthPerTurn,
            InitialPlayerEnergy = missionSettings.StartingPlayerEnergy,
            LayerModifiers = CreateLayerModifiers(stage, missionSettings.ObjectiveDistance)
        };

        var branchLabel = lastResult switch
        {
            GameResult.PlayerWin => "Advance Route",
            GameResult.PlayerLoss => "Recovery Route",
            _ => "Opening Route"
        };
        var branchSummary = lastResult switch
        {
            GameResult.PlayerWin => $"Previous trace secured. Advancing from {wins} win(s) toward stage {stage}.",
            GameResult.PlayerLoss => $"Last trace collapsed. Re-entering stage {stage} with a recovery charge cushion.",
            _ => "First contact with the Signal Recovery arc."
        };

        var missionBriefing = recoveryAttempt ? arcMission.RecoveryBriefing : arcMission.Briefing;
        var briefing = $"{arc.Title} / {arcMission.Title}. Stage {stage} {branchLabel.ToLowerInvariant()}: {missionBriefing} Trace seed {seed.Text}. Wins {wins}, losses {losses}.";
        return new CampaignMissionPlan(stage, seed, missionSettings, configuration, briefing, arc.Id, arc.Title, arcMission.Title, branchLabel, branchSummary);
    }

    public static EnemyPersonality SelectPersonality(ProceduralSeed seed)
    {
        var personalities = new[]
        {
            EnemyPersonality.Aggressive,
            EnemyPersonality.Defensive,
            EnemyPersonality.Economic,
            EnemyPersonality.Opportunistic,
            EnemyPersonality.CorruptionFocused
        };
        var hash = 17;
        foreach (var character in seed.Text)
        {
            hash = hash * 31 + character;
        }

        return personalities[Math.Abs(hash) % personalities.Length];
    }

    private static ProceduralSeed BuildSeed(int stage, int completedCount, GameResult lastResult)
    {
        var branch = lastResult switch
        {
            GameResult.PlayerWin => "advance",
            GameResult.PlayerLoss => "recover",
            _ => "entry"
        };

        return ProceduralSeed.FromText($"campaign-{stage:00}-{branch}-{completedCount + 1:000}");
    }

    private static IReadOnlyDictionary<int, LayerRuleModifier> CreateLayerModifiers(int stage, int objectiveDistance)
    {
        var modifiers = new Dictionary<int, LayerRuleModifier>
        {
            [0] = new(IntegrityBonus: 1)
        };

        for (var layer = 1; layer <= objectiveDistance; layer++)
        {
            var deepThreat = layer >= 4 ? 1 : 0;
            var deepResistance = stage >= 5 && layer >= objectiveDistance - 1 ? 1 : 0;
            if (deepThreat != 0 || deepResistance != 0)
            {
                modifiers[layer] = new LayerRuleModifier(ThreatBonus: deepThreat, CorruptionResistanceBonus: deepResistance);
            }
        }

        return modifiers;
    }
}
