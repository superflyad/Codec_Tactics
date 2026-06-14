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

    public TurnPhase Phase { get; private set; } = TurnPhase.Player;

    public GameResult Result { get; private set; } = GameResult.InProgress;

    public static NetworkGame CreateDefault()
    {
        var game = new NetworkGame(NetworkBoard.CreateGrid());
        game.Board.GetNode(DefaultPlayerStart).SetOwner(NodeOwner.Player);
        game.Board.GetNode(DefaultEnemyStart).SetOwner(NodeOwner.Enemy);
        return game;
    }

    public bool ClaimNode(NodeId target)
    {
        if (!CanAct() || !Board.Contains(target))
        {
            return false;
        }

        var node = Board.GetNode(target);
        if (node.Owner != NodeOwner.Neutral || !Board.HasAdjacentOwner(target, NodeOwner.Player))
        {
            return false;
        }

        node.SetOwner(NodeOwner.Player);
        CompletePlayerAction();
        return true;
    }

    public bool ReinforceNode(NodeId target)
    {
        if (!CanAct() || !Board.Contains(target))
        {
            return false;
        }

        var node = Board.GetNode(target);
        if (node.Owner != NodeOwner.Player)
        {
            return false;
        }

        node.Reinforce();
        CompletePlayerAction();
        return true;
    }

    public bool WeakenEnemyConnection(NodeId first, NodeId second)
    {
        if (!CanAct())
        {
            return false;
        }

        var connection = Board.FindConnection(first, second);
        if (connection is null || !connection.IsActive)
        {
            return false;
        }

        var firstNode = Board.GetNode(first);
        var secondNode = Board.GetNode(second);
        var touchesEnemy = firstNode.Owner == NodeOwner.Enemy || secondNode.Owner == NodeOwner.Enemy;
        var reachableByPlayer = Board.HasAdjacentOwner(first, NodeOwner.Player) || Board.HasAdjacentOwner(second, NodeOwner.Player);

        if (!touchesEnemy || !reachableByPlayer)
        {
            return false;
        }

        connection.Weaken();
        CompletePlayerAction();
        return true;
    }

    private bool CanAct() => Phase == TurnPhase.Player && Result == GameResult.InProgress;

    private void CompletePlayerAction()
    {
        Phase = TurnPhase.Enemy;
        ResolveEnemyTurn();
        EvaluateOutcome();

        if (Result == GameResult.InProgress)
        {
            TurnNumber++;
            Phase = TurnPhase.Player;
        }
    }

    private void ResolveEnemyTurn()
    {
        var expansionTarget = Board.Nodes
            .Where(node => node.Owner == NodeOwner.Enemy)
            .OrderBy(node => node.Id)
            .SelectMany(enemyNode => Board.GetAdjacentNodes(enemyNode.Id)
                .Where(adjacent => adjacent.Owner == NodeOwner.Neutral)
                .OrderBy(adjacent => adjacent.Id))
            .Select(node => node.Id)
            .Distinct()
            .OrderBy(nodeId => nodeId)
            .Cast<NodeId?>()
            .FirstOrDefault();

        if (expansionTarget.HasValue)
        {
            var targetNode = Board.GetNode(expansionTarget.Value);
            if (targetNode.Owner == NodeOwner.Neutral)
            {
                targetNode.SetOwner(NodeOwner.Enemy);
            }
        }
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
}
