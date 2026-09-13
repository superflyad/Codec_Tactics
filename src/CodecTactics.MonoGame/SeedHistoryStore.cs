using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CodecTactics.Core.Network;

namespace CodecTactics.MonoGame;

internal sealed class SeedHistoryStore
{
    private const int MaxCompletedRuns = 24;
    public const int SlotCount = 3;
    private readonly string _path;

    public SeedHistoryStore()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Codec_Tactics",
            "seed-history.json"))
    {
    }

    internal SeedHistoryStore(string path)
    {
        _path = path;
    }

    public PersistedSeedHistory Load()
    {
        if (!File.Exists(_path))
        {
            return new PersistedSeedHistory();
        }

        try
        {
            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<PersistedSeedHistory>(json) ?? new PersistedSeedHistory();
        }
        catch (IOException)
        {
            return new PersistedSeedHistory();
        }
        catch (JsonException)
        {
            return new PersistedSeedHistory();
        }
    }

    public void Save(PersistedSeedHistory history)
    {
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var normalized = history with
        {
            CompletedRuns = history.CompletedRuns.TakeLast(MaxCompletedRuns).ToList(),
            SelectedSlot = NormalizeSlot(history.SelectedSlot),
            SaveSlots = NormalizeSlots(history),
            Profile = PersistedPlayerProfile.FromProgress(ProfileProgression.Calculate(LoadCampaignRecords(history)))
        };
        var json = JsonSerializer.Serialize(normalized, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_path, json);
    }

    public int GetSelectedSlot()
    {
        return NormalizeSlot(Load().SelectedSlot);
    }

    public void SelectSlot(int slot)
    {
        var history = Load() with
        {
            SelectedSlot = NormalizeSlot(slot)
        };
        Save(history);
    }

    public IReadOnlyList<SaveSlotSummary> LoadSlotSummaries()
    {
        return NormalizeSlots(Load())
            .Select(slot => new SaveSlotSummary(slot.Slot, slot.ActiveGame?.SeedText ?? string.Empty, slot.ActiveGame?.TurnNumber ?? 0, slot.ActiveGame?.Result, slot.ActiveGame is not null))
            .ToList();
    }

    public void SaveActiveGame(CampaignMissionPlan plan, NetworkGame game)
    {
        SaveActiveGame(GetSelectedSlot(), plan, game);
    }

    public void SaveActiveGame(int slot, CampaignMissionPlan plan, NetworkGame game)
    {
        var history = Load();
        var normalizedSlot = NormalizeSlot(slot);
        var slots = NormalizeSlots(history)
            .Select(saveSlot => saveSlot.Slot == normalizedSlot
                ? saveSlot with { ActiveGame = PersistedActiveGame.FromGame(plan, game), SavedAt = DateTimeOffset.UtcNow }
                : saveSlot)
            .ToList();

        Save(history with
        {
            LastSeedText = plan.Seed.Text,
            SelectedSlot = normalizedSlot,
            SaveSlots = slots
        });
    }

    public bool TryLoadActiveGame(out CampaignMissionPlan plan, out GameSnapshot snapshot)
    {
        return TryLoadActiveGame(GetSelectedSlot(), out plan, out snapshot);
    }

    public bool TryLoadActiveGame(int slot, out CampaignMissionPlan plan, out GameSnapshot snapshot)
    {
        var history = Load();
        var normalizedSlot = NormalizeSlot(slot);
        var activeGame = NormalizeSlots(history).First(saveSlot => saveSlot.Slot == normalizedSlot).ActiveGame;
        if (activeGame is null)
        {
            plan = CampaignProgressionPlanner.CreateInitialPlan();
            snapshot = NetworkGame.CreateMission(ProceduralMissionGenerator.Generate(plan.Seed, plan.MissionSettings), plan.Configuration).CreateSnapshot();
            return false;
        }

        plan = activeGame.ToPlan();
        var mission = ProceduralMissionGenerator.Generate(plan.Seed, plan.MissionSettings);
        snapshot = activeGame.ToSnapshot(mission, plan.Configuration);
        return true;
    }

    public void RecordStarted(ProceduralSeed seed)
    {
        var history = Load() with
        {
            LastSeedText = seed.Text
        };
        Save(history);
    }

    public void RecordCompleted(ProceduralSeed seed, GameResult result, int turnNumber, EnemyPersonality personality)
    {
        RecordCompleted(seed, result, turnNumber, personality, stage: 1);
    }

    public void RecordCompleted(ProceduralSeed seed, GameResult result, int turnNumber, EnemyPersonality personality, int stage)
    {
        if (result == GameResult.InProgress)
        {
            return;
        }

        var history = Load();
        var runs = history.CompletedRuns.ToList();
        runs.Add(new PersistedSeedRun(seed.Text, seed.Value, result, turnNumber, personality, DateTimeOffset.UtcNow, Math.Max(1, stage)));
        Save(history with
        {
            LastSeedText = seed.Text,
            CompletedRuns = runs
        });
    }

    public IReadOnlyList<CampaignMissionRecord> LoadCampaignRecords()
    {
        return LoadCampaignRecords(Load());
    }

    public PlayerProfileProgress LoadProfileProgress()
    {
        return ProfileProgression.Calculate(LoadCampaignRecords());
    }

    private static IReadOnlyList<CampaignMissionRecord> LoadCampaignRecords(PersistedSeedHistory history)
    {
        return history.CompletedRuns
            .Where(run => run.Result is GameResult.PlayerWin or GameResult.PlayerLoss)
            .Select(run => new CampaignMissionRecord(ProceduralSeed.FromText(run.SeedText), run.Result, run.TurnNumber, run.EnemyPersonality, Math.Max(1, run.Stage)))
            .ToList();
    }

    private static int NormalizeSlot(int slot)
    {
        return Math.Clamp(slot, 1, SlotCount);
    }

    private static IReadOnlyList<PersistedSaveSlot> NormalizeSlots(PersistedSeedHistory history)
    {
        var slots = history.SaveSlots
            .Where(slot => slot.Slot >= 1 && slot.Slot <= SlotCount)
            .GroupBy(slot => slot.Slot)
            .ToDictionary(group => group.Key, group => group.Last());

        if (history.ActiveGame is not null && !slots.ContainsKey(1))
        {
            slots[1] = new PersistedSaveSlot(1, history.ActiveGame, DateTimeOffset.UtcNow);
        }

        return Enumerable.Range(1, SlotCount)
            .Select(slot => slots.TryGetValue(slot, out var saveSlot) ? saveSlot : new PersistedSaveSlot(slot, null, null))
            .OrderBy(slot => slot.Slot)
            .ToList();
    }
}

internal sealed record SaveSlotSummary(int Slot, string SeedText, int TurnNumber, GameResult? Result, bool HasSave);

internal sealed record PersistedSeedHistory
{
    public string LastSeedText { get; init; } = string.Empty;

    public IReadOnlyList<PersistedSeedRun> CompletedRuns { get; init; } = Array.Empty<PersistedSeedRun>();

    public PersistedActiveGame ActiveGame { get; init; }

    public int SelectedSlot { get; init; } = 1;

    public IReadOnlyList<PersistedSaveSlot> SaveSlots { get; init; } = Array.Empty<PersistedSaveSlot>();

    public PersistedPlayerProfile Profile { get; init; } = PersistedPlayerProfile.Empty;
}

internal sealed record PersistedSeedRun(
    string SeedText,
    int SeedValue,
    GameResult Result,
    int TurnNumber,
    EnemyPersonality EnemyPersonality,
    DateTimeOffset CompletedAt,
    int Stage = 1);

internal sealed record PersistedPlayerProfile(
    int Level,
    int Experience,
    int Wins,
    int Losses,
    int BestStage,
    int CurrentWinStreak,
    string Title)
{
    public static PersistedPlayerProfile Empty { get; } = FromProgress(ProfileProgression.Calculate(Array.Empty<CampaignMissionRecord>()));

    public static PersistedPlayerProfile FromProgress(PlayerProfileProgress progress)
    {
        return new PersistedPlayerProfile(
            progress.Level,
            progress.Experience,
            progress.Wins,
            progress.Losses,
            progress.BestStage,
            progress.CurrentWinStreak,
            progress.Title);
    }
}

internal sealed record PersistedSaveSlot(
    int Slot,
    PersistedActiveGame ActiveGame,
    DateTimeOffset? SavedAt);

internal sealed record PersistedActiveGame(
    int Stage,
    string SeedText,
    int SeedValue,
    ProceduralMissionSettings MissionSettings,
    GameConfiguration Configuration,
    int TurnNumber,
    int PlayerEnergy,
    int CorruptionPressure,
    TurnPhase Phase,
    GameResult Result,
    int ObjectiveHoldTurns,
    string LastMessage,
    IReadOnlyList<NodeSnapshot> Nodes,
    IReadOnlyList<ConnectionSnapshot> Connections)
{
    public static PersistedActiveGame FromGame(CampaignMissionPlan plan, NetworkGame game)
    {
        var snapshot = game.CreateSnapshot();
        return new PersistedActiveGame(
            plan.Stage,
            plan.Seed.Text,
            plan.Seed.Value,
            plan.MissionSettings,
            plan.Configuration,
            snapshot.TurnNumber,
            snapshot.PlayerEnergy,
            snapshot.CorruptionPressure,
            snapshot.Phase,
            snapshot.Result,
            snapshot.ObjectiveHoldTurns,
            snapshot.LastActionResult.Message,
            snapshot.Nodes,
            snapshot.Connections);
    }

    public CampaignMissionPlan ToPlan()
    {
        var seed = new ProceduralSeed(SeedValue, SeedText);
        if (Stage <= 0)
        {
            return new CampaignMissionPlan(
                Stage,
                seed,
                MissionSettings,
                Configuration,
                $"Loaded Free Trace / Unbound Signal: trace seed {SeedText}.",
                "free-trace",
                "Free Trace",
                "Unbound Signal",
                "Loaded Trace",
                "Resuming an unbound generated route from the selected local slot.");
        }

        var arc = CampaignArcCatalog.Default;
        var mission = CampaignArcCatalog.GetMission(Stage);
        return new CampaignMissionPlan(
            Stage,
            seed,
            MissionSettings,
            Configuration,
            $"Loaded {arc.Title} / {mission.Title}: trace seed {SeedText}.",
            arc.Id,
            arc.Title,
            mission.Title,
            "Loaded Route",
            $"Resuming stage {Stage} from the selected local slot.");
    }

    public GameSnapshot ToSnapshot(MissionDefinition mission, GameConfiguration configuration)
    {
        return new GameSnapshot(
            mission.BoardDefinition,
            configuration,
            mission,
            TurnNumber,
            PlayerEnergy,
            CorruptionPressure,
            Phase,
            Result,
            ObjectiveHoldTurns,
            new GameActionResult(true, string.IsNullOrWhiteSpace(LastMessage) ? "Loaded saved trace." : LastMessage, Result: Result, ObjectiveHoldTurns: ObjectiveHoldTurns),
            TacticalEnemyDecision.None,
            Nodes,
            Connections);
    }
}
