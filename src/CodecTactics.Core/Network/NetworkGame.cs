namespace CodecTactics.Core.Network;

public sealed class NetworkGame
{
    public static readonly NodeId DefaultPlayerStart = new(0, 0);
    public static readonly NodeId DefaultEnemyStart = new(3, 3);

    private NetworkGame(BoardDefinition boardDefinition, GameConfiguration configuration, MissionDefinition? missionDefinition)
    {
        BoardDefinition = boardDefinition;
        Configuration = configuration;
        MissionDefinition = missionDefinition;
        Board = NetworkBoard.FromDefinition(boardDefinition);
        PlayerCore = boardDefinition.PlayerStart;
        PlayerEnergy = boardDefinition.StartingPlayerEnergy ?? configuration.InitialPlayerEnergy;
    }

    public BoardDefinition BoardDefinition { get; }

    public GameConfiguration Configuration { get; }

    public MissionDefinition? MissionDefinition { get; }

    public NetworkBoard Board { get; }

    public NodeId PlayerCore { get; }

    public NodeId? ObjectiveNode => MissionDefinition?.ObjectiveNode;

    public int RequiredObjectiveHoldTurns => MissionDefinition?.RequiredObjectiveHoldTurns ?? 0;

    public int ObjectiveHoldTurns { get; private set; }

    public string ObjectiveText => MissionDefinition?.ObjectiveText ?? "No mission objective configured.";

    public int TurnNumber { get; private set; } = 1;

    public int PlayerEnergy { get; private set; }

    public int CorruptionPressure { get; private set; }

    public TurnPhase Phase { get; private set; } = TurnPhase.Player;

    public GameResult Result { get; private set; } = GameResult.InProgress;

    public GameActionResult LastActionResult { get; private set; } = new(true, "Player turn ready.");

    public static NetworkGame CreateDefault()
    {
        return Create(BoardDefinition.CreateDefaultPrototype());
    }

    public static NetworkGame Create(BoardDefinition boardDefinition, GameConfiguration? configuration = null)
    {
        var game = new NetworkGame(boardDefinition, configuration ?? new GameConfiguration(), null);
        game.RefreshNetworkRisk();
        return game;
    }

    public static NetworkGame CreateMission(MissionDefinition missionDefinition, GameConfiguration? configuration = null)
    {
        var game = new NetworkGame(missionDefinition.BoardDefinition, configuration ?? new GameConfiguration(), missionDefinition);
        game.RefreshNetworkRisk();
        return game;
    }

    public static NetworkGame CreateVerticalSliceMission()
    {
        return CreateMission(MissionDefinition.CreateVerticalSlice());
    }

    public IReadOnlyList<NodeId> RefreshNetworkRisk(bool advanceInstability = false)
    {
        return NetworkIntegrityEvaluator.Evaluate(Board, PlayerCore, CorruptionPressure, Configuration, advanceInstability);
    }

    public bool ClaimNode(NodeId target)
    {
        return ClaimNodeWithResult(target).Succeeded;
    }

    public GameActionResult ClaimNodeWithResult(NodeId target)
    {
        if (!CanAct() || !Board.Contains(target))
        {
            return SetLastAction(false, "Cannot claim right now.");
        }

        var node = Board.GetNode(target);
        if (node.Owner != NodeOwner.Neutral)
        {
            return SetLastAction(false, $"{target} is already {node.Owner}.");
        }

        if (!Board.IsReachableForPlayerClaim(target, Configuration))
        {
            return SetLastAction(false, $"{target} is outside player claim range.");
        }

        if (!SpendEnergy(Configuration.ClaimEnergyCost))
        {
            return SetLastAction(false, $"Claim needs {Configuration.ClaimEnergyCost} energy.");
        }

        node.SetOwner(NodeOwner.Player);
        RefreshNetworkRisk();
        var result = CompletePlayerAction($"Claimed {target} for {Configuration.ClaimEnergyCost} energy.", Configuration.ClaimEnergyCost);
        return result;
    }

    public bool ReinforceNode(NodeId target)
    {
        return ReinforceNodeWithResult(target).Succeeded;
    }

    public GameActionResult ReinforceNodeWithResult(NodeId target)
    {
        if (!CanAct() || !Board.Contains(target))
        {
            return SetLastAction(false, "Cannot reinforce right now.");
        }

        var node = Board.GetNode(target);
        if (node.Owner != NodeOwner.Player)
        {
            return SetLastAction(false, $"{target} is not player-owned.");
        }

        if (!SpendEnergy(Configuration.ReinforceEnergyCost))
        {
            return SetLastAction(false, $"Reinforce needs {Configuration.ReinforceEnergyCost} energy.");
        }

        node.Reinforce();
        RefreshNetworkRisk();
        return CompletePlayerAction($"Reinforced {target} for {Configuration.ReinforceEnergyCost} energy.", Configuration.ReinforceEnergyCost);
    }

    public bool WeakenEnemyConnection(NodeId first, NodeId second)
    {
        return WeakenEnemyConnectionWithResult(first, second).Succeeded;
    }

    public GameActionResult WeakenEnemyConnectionWithResult(NodeId first, NodeId second)
    {
        if (!CanAct())
        {
            return SetLastAction(false, "Cannot weaken right now.");
        }

        var connection = Board.FindConnection(first, second);
        if (connection is null || !connection.IsActive)
        {
            return SetLastAction(false, "Connection is unavailable.");
        }

        var firstNode = Board.GetNode(first);
        var secondNode = Board.GetNode(second);
        var touchesEnemy = firstNode.Owner == NodeOwner.Enemy || secondNode.Owner == NodeOwner.Enemy;
        var reachableByPlayer = Board.HasAdjacentOwner(first, NodeOwner.Player) || Board.HasAdjacentOwner(second, NodeOwner.Player);

        if (!touchesEnemy || !reachableByPlayer)
        {
            return SetLastAction(false, "Connection is not a reachable corruption link.");
        }

        if (!SpendEnergy(Configuration.WeakenConnectionEnergyCost))
        {
            return SetLastAction(false, $"Weaken needs {Configuration.WeakenConnectionEnergyCost} energy.");
        }

        connection.Weaken();
        RefreshNetworkRisk();
        return CompletePlayerAction($"Weakened corruption link for {Configuration.WeakenConnectionEnergyCost} energy.", Configuration.WeakenConnectionEnergyCost);
    }

    public GameActionResult WeakenCorruptionAtNodeWithResult(NodeId target)
    {
        if (!CanAct() || !Board.Contains(target))
        {
            return SetLastAction(false, "Cannot weaken right now.");
        }

        var targetNode = Board.GetNode(target);
        if (targetNode.Owner != NodeOwner.Enemy)
        {
            return SetLastAction(false, $"{target} is not corrupted.");
        }

        var playerNeighbor = Board.GetAdjacentNodes(target)
            .Where(node => node.Owner == NodeOwner.Player)
            .OrderBy(node => node.Id)
            .FirstOrDefault();
        if (playerNeighbor is null)
        {
            return SetLastAction(false, $"{target} has no reachable player connection to weaken.");
        }

        return WeakenEnemyConnectionWithResult(playerNeighbor.Id, target);
    }

    public GameActionResult ExecutePlayerAction(PlayerActionMode mode, NodeId target)
    {
        return mode switch
        {
            PlayerActionMode.Claim => ClaimNodeWithResult(target),
            PlayerActionMode.Reinforce => ReinforceNodeWithResult(target),
            PlayerActionMode.Weaken => WeakenCorruptionAtNodeWithResult(target),
            _ => SetLastAction(false, "Unknown player action.")
        };
    }

    public bool EndPlayerTurn()
    {
        return EndPlayerTurnWithResult().Succeeded;
    }

    public GameActionResult EndPlayerTurnWithResult()
    {
        if (!CanAct())
        {
            return SetLastAction(false, "Cannot end turn right now.");
        }

        return CompletePlayerAction("Ended turn without spending energy.", 0);
    }

    private bool CanAct() => Phase == TurnPhase.Player && Result == GameResult.InProgress;

    private GameActionResult CompletePlayerAction(string actionMessage, int energySpent)
    {
        Phase = TurnPhase.Enemy;
        var enemyTurn = ResolveEnemyTurn();
        EvaluateOutcome();

        var energyGenerated = 0;
        if (Result == GameResult.InProgress)
        {
            TurnNumber++;
            energyGenerated = BeginPlayerTurn();
            Phase = TurnPhase.Player;
            RefreshNetworkRisk();
        }

        var collapseMessage = enemyTurn.CollapsedNodes.Count > 0
            ? $" Collapse: {string.Join(", ", enemyTurn.CollapsedNodes)} fell to corruption after {Configuration.InstabilityTurnsBeforeCollapse} unstable turns."
            : string.Empty;
        var spreadMessage = enemyTurn.CorruptionTarget.HasValue
            ? $" Corruption spread to {enemyTurn.CorruptionTarget.Value}."
            : enemyTurn.CorruptionFocusTarget.HasValue
                ? $" Corruption focused {enemyTurn.CorruptionFocusTarget.Value} but pressure was contained."
                : " Corruption pressure built but did not spread.";
        var resourceMessage = energyGenerated > 0
            ? $" Resource nodes generated {energyGenerated} energy."
            : string.Empty;
        var objectiveMessage = FormatObjectiveProgressMessage();
        var resultMessage = Result switch
        {
            GameResult.PlayerWin => " Mission complete.",
            GameResult.PlayerLoss => " Mission failed.",
            _ => string.Empty
        };

        return SetLastAction(true, actionMessage + collapseMessage + spreadMessage + resourceMessage + objectiveMessage + resultMessage, energySpent, energyGenerated, enemyTurn.CorruptionTarget, enemyTurn.CorruptionFocusTarget, enemyTurn.CollapsedNodes);
    }

    private EnemyTurnResult ResolveEnemyTurn()
    {
        CorruptionPressure += Configuration.CorruptionPressureGrowthPerTurn;
        var collapsed = RefreshNetworkRisk(advanceInstability: true);
        var expansionTarget = CorruptionTargetPolicy.SelectExpansionTarget(Board, Configuration);

        if (expansionTarget.HasValue)
        {
            var targetNode = Board.GetNode(expansionTarget.Value);
            var resistance = GetCorruptionResistance(targetNode);
            if (targetNode.Owner == NodeOwner.Neutral && CorruptionPressure >= resistance)
            {
                targetNode.SetOwner(NodeOwner.Enemy);
                CorruptionPressure -= resistance;
                RefreshNetworkRisk();
                return new EnemyTurnResult(targetNode.Id, targetNode.Id, collapsed);
            }
        }

        RefreshNetworkRisk();
        return new EnemyTurnResult(null, expansionTarget, collapsed);
    }

    private int BeginPlayerTurn()
    {
        var generated = Board.Nodes.Count(node => node.Owner == NodeOwner.Player && node.Type == NodeType.Resource)
            * Configuration.ResourceEnergyPerTurn;
        PlayerEnergy += generated;
        return generated;
    }

    private bool SpendEnergy(int amount)
    {
        if (PlayerEnergy < amount)
        {
            return false;
        }

        PlayerEnergy -= amount;
        return true;
    }

    private int GetCorruptionResistance(NodeState node)
    {
        return node.Type == NodeType.Firewall
            ? Configuration.FirewallCorruptionResistance
            : Configuration.StandardCorruptionResistance;
    }

    private GameActionResult SetLastAction(
        bool succeeded,
        string message,
        int energySpent = 0,
        int energyGenerated = 0,
        NodeId? corruptionTarget = null)
    {
        return SetLastAction(succeeded, message, energySpent, energyGenerated, corruptionTarget, null, Array.Empty<NodeId>());
    }

    private GameActionResult SetLastAction(
        bool succeeded,
        string message,
        int energySpent,
        int energyGenerated,
        NodeId? corruptionTarget,
        NodeId? corruptionFocusTarget,
        IReadOnlyList<NodeId> collapsedNodes)
    {
        LastActionResult = new GameActionResult(succeeded, message, energySpent, energyGenerated, corruptionTarget, corruptionFocusTarget, collapsedNodes, ObjectiveHoldTurns, Result);
        return LastActionResult;
    }

    private void EvaluateOutcome()
    {
        if (MissionDefinition is not null)
        {
            EvaluateMissionOutcome();
            return;
        }

        var playerCount = Board.Nodes.Count(node => node.Owner == NodeOwner.Player);
        var enemyCount = Board.Nodes.Count(node => node.Owner == NodeOwner.Enemy);
        var neutralCount = Board.Nodes.Count(node => node.Owner == NodeOwner.Neutral);

        Result = (playerCount, enemyCount, neutralCount) switch
        {
            (0, > 0, _) => GameResult.PlayerLoss,
            ( > 0, 0, _) => GameResult.PlayerWin,
            ( > 0, > 0, 0) when playerCount > enemyCount => GameResult.PlayerWin,
            ( > 0, > 0, 0) => GameResult.PlayerLoss,
            _ => GameResult.InProgress
        };
    }

    private void EvaluateMissionOutcome()
    {
        var core = Board.GetNode(PlayerCore);
        var objective = Board.GetNode(MissionDefinition!.ObjectiveNode);

        if (core.Owner != NodeOwner.Player)
        {
            ObjectiveHoldTurns = 0;
            Result = GameResult.PlayerLoss;
            return;
        }

        if (objective.Owner == NodeOwner.Enemy)
        {
            ObjectiveHoldTurns = 0;
            Result = GameResult.PlayerLoss;
            return;
        }

        if (objective.Owner == NodeOwner.Player)
        {
            ObjectiveHoldTurns++;
        }
        else
        {
            ObjectiveHoldTurns = 0;
        }

        Result = ObjectiveHoldTurns >= MissionDefinition.RequiredObjectiveHoldTurns
            ? GameResult.PlayerWin
            : GameResult.InProgress;
    }

    private string FormatObjectiveProgressMessage()
    {
        if (MissionDefinition is null)
        {
            return string.Empty;
        }

        return $" Objective hold: {ObjectiveHoldTurns}/{MissionDefinition.RequiredObjectiveHoldTurns}.";
    }

    private sealed record EnemyTurnResult(NodeId? CorruptionTarget, NodeId? CorruptionFocusTarget, IReadOnlyList<NodeId> CollapsedNodes);
}
