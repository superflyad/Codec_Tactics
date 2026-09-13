namespace CodecTactics.Core.Network;

public static class ProfileProgression
{
    public static PlayerProfileProgress Calculate(IEnumerable<CampaignMissionRecord> completedMissions)
    {
        var records = completedMissions.ToList();
        var wins = records.Count(record => record.Result == GameResult.PlayerWin);
        var losses = records.Count(record => record.Result == GameResult.PlayerLoss);
        var xp = records.Sum(GetExperience);
        var level = Math.Clamp(1 + xp / 100, 1, 20);
        var bestStage = records
            .Where(record => record.Result == GameResult.PlayerWin)
            .Select(record => record.Stage)
            .DefaultIfEmpty(0)
            .Max();
        var streak = GetCurrentWinStreak(records);
        var title = GetTitle(level, bestStage, streak);

        return new PlayerProfileProgress(level, xp, wins, losses, bestStage, streak, title);
    }

    private static int GetExperience(CampaignMissionRecord record)
    {
        var stage = Math.Max(1, record.Stage);
        return record.Result switch
        {
            GameResult.PlayerWin => 70 + stage * 15 + Math.Max(0, 12 - record.TurnNumber),
            GameResult.PlayerLoss => 25 + stage * 5,
            _ => 0
        };
    }

    private static int GetCurrentWinStreak(IReadOnlyList<CampaignMissionRecord> records)
    {
        var streak = 0;
        for (var i = records.Count - 1; i >= 0; i--)
        {
            if (records[i].Result != GameResult.PlayerWin)
            {
                break;
            }

            streak++;
        }

        return streak;
    }

    private static string GetTitle(int level, int bestStage, int streak)
    {
        if (bestStage >= 8)
        {
            return "Kernel Sealer";
        }

        if (streak >= 3)
        {
            return "Signal Climber";
        }

        return level switch
        {
            >= 12 => "Deep Trace Operator",
            >= 7 => "Backplane Runner",
            >= 4 => "Relay Specialist",
            _ => "Lattice Initiate"
        };
    }
}

public sealed record PlayerProfileProgress(
    int Level,
    int Experience,
    int Wins,
    int Losses,
    int BestStage,
    int CurrentWinStreak,
    string Title);
