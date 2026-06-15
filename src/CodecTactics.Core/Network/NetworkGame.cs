namespace CodecTactics.Core.Network;

public sealed class NetworkGame
{
    public static readonly NodeId DefaultPlayerStart = new(0, 0);
    public static readonly NodeId DefaultEnemyStart = new(3, 3);

    private NetworkGame(NetworkBoard board)
    {
        Board = board;
    }

    public NetworkBoard Board { get; }

    public int TurnNumber { get; private set; } = 1;

    public int PlayerEnergy { get; private set; } = NetworkRules.InitialPlayerEnergy;

    public int CorruptionPressure { get; private set; }

    public TurnPhase Phase { get; private set; } = TurnPhase.Player;

    public GameResult Result { get; private set; } = GameResult.InProgress;

    public GameActionResult LastActionResult { get; private set; } = new(true, "Player turn ready.");

    public static NetworkGame CreateDefault()
    {
        var game = new NetworkGame(NetworkBoard.CreateGrid());
        game.Board.GetNode(DefaultPlayerStart).SetOwner(NodeOwner.Player);
        game.Board.GetNode(DefaultEnemyStart).SetOwner(NodeOwner.Enemy);
        game.RefreshNetworkRisk();
        return game;
    }

    public IReadOnlyList<NodeId> RefreshNetworkRisk(bool advanceInstability = false)
    {
        return NetworkIntegrityEvaluator.Evaluate(Board, DefaultPlayerStart, CorruptionPressure, advanceInstability);
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

        if (!Board.IsReachableForPlayerClaim(target))
        {
            return SetLastAction(false, $"{target} is outside player claim range.");
        }

        if (!SpendEnergy(NetworkRules.ClaimEnergyCost))
        {
            return SetLastAction(false, $"Claim needs {NetworkRules.ClaimEnergyCost} energy.");
        }

        node.SetOwner(NodeOwner.Player);
        RefreshNetworkRisk();
        var result = CompletePlayerAction($"Claimed {target} for {NetworkRules.ClaimEnergyCost} energy.", NetworkRules.ClaimEnergyCost);
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

        if (!SpendEnergy(NetworkRules.ReinforceEnergyCost))
        {
            return SetLastAction(false, $"Reinforce needs {NetworkRules.ReinforceEnergyCost} energy.");
        }

        node.Reinforce();
        RefreshNetworkRisk();
        return CompletePlayerAction($"Reinforced {target} for {NetworkRules.ReinforceEnergyCost} energy.", NetworkRules.ReinforceEnergyCost);
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

        if (!SpendEnergy(NetworkRules.ReinforceEnergyCost))
        {
            return SetLastAction(false, $"Weaken needs {NetworkRules.ReinforceEnergyCost} energy.");
        }

        connection.Weaken();
        RefreshNetworkRisk();
        return CompletePlayerAction($"Weakened corruption link for {NetworkRules.ReinforceEnergyCost} energy.", NetworkRules.ReinforceEnergyCost);
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
            ? $" Collapse: {string.Join(", ", enemyTurn.CollapsedNodes)} fell to corruption after {NetworkRules.InstabilityTurnsBeforeCollapse} unstable turns."
            : string.Empty;
        var spreadMessage = enemyTurn.CorruptionTarget.HasValue
            ? $" Corruption spread to {enemyTurn.CorruptionTarget.Value}."
            : enemyTurn.CorruptionFocusTarget.HasValue
                ? $" Corruption focused {enemyTurn.CorruptionFocusTarget.Value} but pressure was contained."
                : " Corruption pressure built but did not spread.";
        var resourceMessage = energyGenerated > 0
            ? $" Resource nodes generated {energyGenerated} energy."
            : string.Empty;

        return SetLastAction(true, actionMessage + collapseMessage + spreadMessage + resourceMessage, energySpent, energyGenerated, enemyTurn.CorruptionTarget, enemyTurn.CorruptionFocusTarget, enemyTurn.CollapsedNodes);
    }

    private EnemyTurnResult ResolveEnemyTurn()
    {
        CorruptionPressure++;
        var collapsed = RefreshNetworkRisk(advanceInstability: true);
        var expansionTarget = CorruptionTargetPolicy.SelectExpansionTarget(Board);

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
            * NetworkRules.ResourceEnergyPerTurn;
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

    private static int GetCorruptionResistance(NodeState node)
    {
        return node.Type == NodeType.Firewall
            ? NetworkRules.FirewallCorruptionResistance
            : NetworkRules.StandardCorruptionResistance;
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
        LastActionResult = new GameActionResult(succeeded, message, energySpent, energyGenerated, corruptionTarget, corruptionFocusTarget, collapsedNodes);
        return LastActionResult;
    }

    private void EvaluateOutcome()
    {
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

    private sealed record EnemyTurnResult(NodeId? CorruptionTarget, NodeId? CorruptionFocusTarget, IReadOnlyList<NodeId> CollapsedNodes);
}
