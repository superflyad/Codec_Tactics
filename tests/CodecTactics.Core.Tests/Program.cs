using CodecTactics.Core;
using CodecTactics.Core.Network;

var tests = new (string Name, Action Test)[]
{
    ("project name is stable", () => AssertEqual("Codec_Tactics", ProjectInfo.Name)),
    ("foundation milestone is one", () => AssertEqual(1, ProjectInfo.FoundationMilestone)),
    ("current focus documents prototype", () => AssertContains("prototype", ProjectInfo.CurrentFocus)),
    ("board creation builds fixed adjacent grid", BoardCreationBuildsFixedAdjacentGrid),
    ("player can claim adjacent neutral node", PlayerCanClaimAdjacentNeutralNode),
    ("player cannot claim non-adjacent neutral node", PlayerCannotClaimNonAdjacentNeutralNode),
    ("player can reinforce owned node", PlayerCanReinforceOwnedNode),
    ("player can weaken reachable enemy connection", PlayerCanWeakenReachableEnemyConnection),
    ("enemy expands into adjacent neutral node", EnemyExpandsIntoAdjacentNeutralNode),
    ("turn progresses after successful player action", TurnProgressesAfterSuccessfulPlayerAction),
    ("enemy expansion is deterministic", EnemyExpansionIsDeterministic),
};

var failed = 0;

foreach (var (name, test) in tests)
{
    try
    {
        test();
        Console.WriteLine($"PASS {name}");
    }
    catch (Exception ex)
    {
        failed++;
        Console.Error.WriteLine($"FAIL {name}: {ex.Message}");
    }
}

if (failed > 0)
{
    Console.Error.WriteLine($"{failed} test(s) failed.");
    return 1;
}

Console.WriteLine($"{tests.Length} test(s) passed.");
return 0;

static void AssertEqual<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"Expected '{expected}', got '{actual}'.");
    }
}

static void AssertContains(string expected, string actual)
{
    if (actual.IndexOf(expected, StringComparison.OrdinalIgnoreCase) < 0)
    {
        throw new InvalidOperationException($"Expected '{actual}' to contain '{expected}'.");
    }
}

static void BoardCreationBuildsFixedAdjacentGrid()
{
    var board = NetworkBoard.CreateGrid();

    AssertEqual(4, board.Width);
    AssertEqual(4, board.Height);
    AssertEqual(16, board.Nodes.Count);
    AssertEqual(24, board.Connections.Count);
    AssertTrue(board.AreConnected(new NodeId(0, 0), new NodeId(1, 0)), "Expected horizontal neighbors to be connected.");
    AssertTrue(board.AreConnected(new NodeId(0, 0), new NodeId(0, 1)), "Expected vertical neighbors to be connected.");
    AssertFalse(board.AreConnected(new NodeId(0, 0), new NodeId(1, 1)), "Expected diagonal nodes to be disconnected.");
}

static void PlayerCanClaimAdjacentNeutralNode()
{
    var game = NetworkGame.CreateDefault();

    var claimed = game.ClaimNode(new NodeId(1, 0));

    AssertTrue(claimed, "Expected adjacent neutral claim to succeed.");
    AssertEqual(NodeOwner.Player, game.Board.GetNode(new NodeId(1, 0)).Owner);
}

static void PlayerCannotClaimNonAdjacentNeutralNode()
{
    var game = NetworkGame.CreateDefault();

    var claimed = game.ClaimNode(new NodeId(2, 0));

    AssertFalse(claimed, "Expected non-adjacent neutral claim to fail.");
    AssertEqual(NodeOwner.Neutral, game.Board.GetNode(new NodeId(2, 0)).Owner);
    AssertEqual(1, game.TurnNumber);
}

static void PlayerCanReinforceOwnedNode()
{
    var game = NetworkGame.CreateDefault();
    var start = game.Board.GetNode(NetworkGame.DefaultPlayerStart);

    var reinforced = game.ReinforceNode(NetworkGame.DefaultPlayerStart);

    AssertTrue(reinforced, "Expected owned node reinforcement to succeed.");
    AssertEqual(2, start.Integrity);
}

static void PlayerCanWeakenReachableEnemyConnection()
{
    var game = NetworkGame.CreateDefault();
    game.Board.GetNode(new NodeId(2, 3)).SetOwner(NodeOwner.Player);
    var connection = game.Board.FindConnection(new NodeId(2, 3), NetworkGame.DefaultEnemyStart)
        ?? throw new InvalidOperationException("Expected setup connection to exist.");

    var weakened = game.WeakenEnemyConnection(new NodeId(2, 3), NetworkGame.DefaultEnemyStart);

    AssertTrue(weakened, "Expected reachable enemy connection weaken action to succeed.");
    AssertEqual(1, connection.Strength);
}

static void EnemyExpandsIntoAdjacentNeutralNode()
{
    var game = NetworkGame.CreateDefault();

    game.ReinforceNode(NetworkGame.DefaultPlayerStart);

    AssertEqual(NodeOwner.Enemy, game.Board.GetNode(new NodeId(3, 2)).Owner);
}

static void TurnProgressesAfterSuccessfulPlayerAction()
{
    var game = NetworkGame.CreateDefault();

    var acted = game.ClaimNode(new NodeId(1, 0));

    AssertTrue(acted, "Expected claim to succeed.");
    AssertEqual(2, game.TurnNumber);
    AssertEqual(TurnPhase.Player, game.Phase);
    AssertEqual(GameResult.InProgress, game.Result);
}

static void EnemyExpansionIsDeterministic()
{
    var first = NetworkGame.CreateDefault();
    var second = NetworkGame.CreateDefault();

    first.ReinforceNode(NetworkGame.DefaultPlayerStart);
    second.ReinforceNode(NetworkGame.DefaultPlayerStart);

    var firstEnemyNodes = string.Join("|", first.Board.Nodes
        .Where(node => node.Owner == NodeOwner.Enemy)
        .OrderBy(node => node.Id)
        .Select(node => node.Id.ToString()));
    var secondEnemyNodes = string.Join("|", second.Board.Nodes
        .Where(node => node.Owner == NodeOwner.Enemy)
        .OrderBy(node => node.Id)
        .Select(node => node.Id.ToString()));

    AssertEqual(firstEnemyNodes, secondEnemyNodes);
    AssertEqual("(3,2)|(3,3)", firstEnemyNodes);
}

static void AssertTrue(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

static void AssertFalse(bool condition, string message)
{
    if (condition)
    {
        throw new InvalidOperationException(message);
    }
}
