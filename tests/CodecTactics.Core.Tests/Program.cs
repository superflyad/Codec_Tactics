using CodecTactics.Core;
using CodecTactics.Core.Network;

var tests = new (string Name, Action Test)[]
{
    ("project name is stable", () => AssertEqual("Codec_Tactics", ProjectInfo.Name)),
    ("foundation milestone is one", () => AssertEqual(1, ProjectInfo.FoundationMilestone)),
    ("current focus documents prototype", () => AssertContains("prototype", ProjectInfo.CurrentFocus)),
    ("board creation builds fixed adjacent grid", BoardCreationBuildsFixedAdjacentGrid),
    ("default board initializes node types", DefaultBoardInitializesNodeTypes),
    ("board definition supports multiple board sizes", BoardDefinitionSupportsMultipleBoardSizes),
    ("board definition supports rectangular boards", BoardDefinitionSupportsRectangularBoards),
    ("game supports alternate spawn positions", GameSupportsAlternateSpawnPositions),
    ("custom board definition loads node types and ownership", CustomBoardDefinitionLoadsNodeTypesAndOwnership),
    ("game configuration defaults preserve network rules", GameConfigurationDefaultsPreserveNetworkRules),
    ("custom board initialization is deterministic", CustomBoardInitializationIsDeterministic),
    ("player can claim adjacent neutral node", PlayerCanClaimAdjacentNeutralNode),
    ("player cannot claim non-adjacent neutral node", PlayerCannotClaimNonAdjacentNeutralNode),
    ("player cannot claim enemy-owned node", PlayerCannotClaimEnemyOwnedNode),
    ("invalid claim does not trigger enemy expansion", InvalidClaimDoesNotTriggerEnemyExpansion),
    ("player can reinforce owned node", PlayerCanReinforceOwnedNode),
    ("connected core node has calculated network integrity", ConnectedCoreNodeHasCalculatedNetworkIntegrity),
    ("isolation applies integrity penalty and threat", IsolationAppliesIntegrityPenaltyAndThreat),
    ("relay support increases integrity", RelaySupportIncreasesIntegrity),
    ("firewall support increases integrity", FirewallSupportIncreasesIntegrity),
    ("claiming spends player energy", ClaimingSpendsPlayerEnergy),
    ("resource node generates energy on next player turn", ResourceNodeGeneratesEnergyOnNextPlayerTurn),
    ("insufficient energy prevents player action", InsufficientEnergyPreventsPlayerAction),
    ("relay extends player claim range", RelayExtendsPlayerClaimRange),
    ("player can weaken reachable enemy connection", PlayerCanWeakenReachableEnemyConnection),
    ("enemy expands into adjacent neutral node", EnemyExpandsIntoAdjacentNeutralNode),
    ("firewall resists first corruption pressure", FirewallResistsFirstCorruptionPressure),
    ("corruption pressure progresses deterministically", CorruptionPressureProgressesDeterministically),
    ("threat progression marks exposed nodes unstable", ThreatProgressionMarksExposedNodesUnstable),
    ("persistent instability collapses node", PersistentInstabilityCollapsesNode),
    ("corruption targeting prioritizes unstable nodes deterministically", CorruptionTargetingPrioritizesUnstableNodesDeterministically),
    ("turn progresses after successful player action", TurnProgressesAfterSuccessfulPlayerAction),
    ("enemy expansion is deterministic", EnemyExpansionIsDeterministic),
    ("end turn resolves real corruption turn", EndTurnResolvesRealCorruptionTurn),
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

static void DefaultBoardInitializesNodeTypes()
{
    var board = NetworkBoard.CreateGrid();

    AssertEqual(NodeType.Standard, board.GetNode(new NodeId(0, 0)).Type);
    AssertEqual(NodeType.Resource, board.GetNode(new NodeId(1, 0)).Type);
    AssertEqual(NodeType.Relay, board.GetNode(new NodeId(0, 1)).Type);
    AssertEqual(NodeType.Firewall, board.GetNode(new NodeId(2, 3)).Type);
}

static void BoardDefinitionSupportsMultipleBoardSizes()
{
    var definition = BoardDefinition.CreateGrid(5, 5, new NodeId(1, 1), new[] { new NodeId(4, 4) });
    var game = NetworkGame.Create(definition);

    AssertEqual(5, game.Board.Width);
    AssertEqual(5, game.Board.Height);
    AssertEqual(25, game.Board.Nodes.Count);
    AssertEqual(NodeOwner.Player, game.Board.GetNode(new NodeId(1, 1)).Owner);
    AssertEqual(NodeOwner.Enemy, game.Board.GetNode(new NodeId(4, 4)).Owner);
}

static void BoardDefinitionSupportsRectangularBoards()
{
    var definition = BoardDefinition.CreateGrid(3, 5, new NodeId(0, 4), new[] { new NodeId(2, 0) });
    var board = NetworkBoard.FromDefinition(definition);

    AssertEqual(3, board.Width);
    AssertEqual(5, board.Height);
    AssertEqual(15, board.Nodes.Count);
    AssertEqual(22, board.Connections.Count);
    AssertTrue(board.AreConnected(new NodeId(0, 4), new NodeId(1, 4)), "Expected rectangular board horizontal connection.");
    AssertTrue(board.AreConnected(new NodeId(2, 0), new NodeId(2, 1)), "Expected rectangular board vertical connection.");
}

static void GameSupportsAlternateSpawnPositions()
{
    var definition = BoardDefinition.CreateGrid(4, 4, new NodeId(2, 2), new[] { new NodeId(0, 3) });
    var game = NetworkGame.Create(definition);

    AssertEqual(new NodeId(2, 2), game.PlayerCore);
    AssertEqual(NodeOwner.Player, game.Board.GetNode(new NodeId(2, 2)).Owner);
    AssertEqual(NodeOwner.Enemy, game.Board.GetNode(new NodeId(0, 3)).Owner);
    AssertEqual(NodeOwner.Neutral, game.Board.GetNode(NetworkGame.DefaultPlayerStart).Owner);
    AssertEqual(NodeOwner.Neutral, game.Board.GetNode(NetworkGame.DefaultEnemyStart).Owner);
}

static void CustomBoardDefinitionLoadsNodeTypesAndOwnership()
{
    var nodeTypes = new Dictionary<NodeId, NodeType>
    {
        [new NodeId(1, 0)] = NodeType.Resource,
        [new NodeId(2, 0)] = NodeType.Relay,
        [new NodeId(3, 1)] = NodeType.Firewall
    };
    var definition = BoardDefinition.CreateGrid(
        4,
        2,
        new NodeId(0, 0),
        new[] { new NodeId(3, 1) },
        nodeTypes,
        startingPlayerEnergy: 7,
        metadata: new Dictionary<string, string> { ["layer"] = "future-ready" },
        initialOwnership: new Dictionary<NodeId, NodeOwner> { [new NodeId(1, 0)] = NodeOwner.Player });

    var game = NetworkGame.Create(definition);

    AssertEqual(7, game.PlayerEnergy);
    AssertEqual(NodeOwner.Player, game.Board.GetNode(new NodeId(1, 0)).Owner);
    AssertEqual(NodeType.Resource, game.Board.GetNode(new NodeId(1, 0)).Type);
    AssertEqual(NodeType.Relay, game.Board.GetNode(new NodeId(2, 0)).Type);
    AssertEqual(NodeType.Firewall, game.Board.GetNode(new NodeId(3, 1)).Type);
    AssertEqual(new NodeId(1, 0), definition.ResourceNodes.Single());
    AssertEqual(new NodeId(2, 0), definition.RelayNodes.Single());
    AssertEqual(new NodeId(3, 1), definition.FirewallNodes.Single());
    AssertEqual("future-ready", definition.Metadata["layer"]);
}

static void GameConfigurationDefaultsPreserveNetworkRules()
{
    var configuration = new GameConfiguration();

    AssertEqual(NetworkRules.InitialPlayerEnergy, configuration.InitialPlayerEnergy);
    AssertEqual(NetworkRules.ClaimEnergyCost, configuration.ClaimEnergyCost);
    AssertEqual(NetworkRules.ReinforceEnergyCost, configuration.ReinforceEnergyCost);
    AssertEqual(NetworkRules.WeakenConnectionEnergyCost, configuration.WeakenConnectionEnergyCost);
    AssertEqual(NetworkRules.ResourceEnergyPerTurn, configuration.ResourceEnergyPerTurn);
    AssertEqual(NetworkRules.CorruptionPressureGrowthPerTurn, configuration.CorruptionPressureGrowthPerTurn);
    AssertEqual(NetworkRules.BaseNetworkIntegrity, configuration.BaseNetworkIntegrity);
    AssertEqual(NetworkRules.NearbyCorruptionThreat, configuration.NearbyCorruptionThreat);
    AssertEqual(NetworkRules.InstabilityTurnsBeforeCollapse, configuration.InstabilityTurnsBeforeCollapse);
}

static void CustomBoardInitializationIsDeterministic()
{
    var nodeTypes = new Dictionary<NodeId, NodeType>
    {
        [new NodeId(2, 2)] = NodeType.Firewall,
        [new NodeId(1, 1)] = NodeType.Resource,
        [new NodeId(3, 0)] = NodeType.Relay
    };
    var definition = BoardDefinition.CreateGrid(5, 3, new NodeId(4, 2), new[] { new NodeId(0, 0), new NodeId(0, 2) }, nodeTypes);

    var first = NetworkGame.Create(definition);
    var second = NetworkGame.Create(definition);

    AssertEqual(DescribeBoard(first.Board), DescribeBoard(second.Board));
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

static void PlayerCannotClaimEnemyOwnedNode()
{
    var game = NetworkGame.CreateDefault();

    var claimed = game.ClaimNode(NetworkGame.DefaultEnemyStart);

    AssertFalse(claimed, "Expected enemy-owned node claim to fail.");
    AssertEqual(NodeOwner.Enemy, game.Board.GetNode(NetworkGame.DefaultEnemyStart).Owner);
    AssertEqual(1, game.TurnNumber);
}

static void InvalidClaimDoesNotTriggerEnemyExpansion()
{
    var game = NetworkGame.CreateDefault();

    var claimed = game.ClaimNode(new NodeId(2, 0));

    AssertFalse(claimed, "Expected non-adjacent claim to fail.");
    AssertEqual(1, game.Board.Nodes.Count(node => node.Owner == NodeOwner.Enemy));
    AssertEqual(NodeOwner.Neutral, game.Board.GetNode(new NodeId(3, 2)).Owner);
    AssertEqual(TurnPhase.Player, game.Phase);
}

static void PlayerCanReinforceOwnedNode()
{
    var game = NetworkGame.CreateDefault();
    var start = game.Board.GetNode(NetworkGame.DefaultPlayerStart);

    var reinforced = game.ReinforceNode(NetworkGame.DefaultPlayerStart);

    AssertTrue(reinforced, "Expected owned node reinforcement to succeed.");
    AssertEqual(1, start.ReinforcementLevel);
    AssertTrue(start.Integrity > NetworkRules.BaseNetworkIntegrity, "Expected calculated integrity to include reinforcement and core support.");
}

static void ConnectedCoreNodeHasCalculatedNetworkIntegrity()
{
    var game = NetworkGame.CreateDefault();
    var start = game.Board.GetNode(NetworkGame.DefaultPlayerStart);

    AssertEqual(7, start.Integrity);
    AssertEqual(4, start.Threat);
    AssertFalse(start.IsUnstable, "Expected the connected core node to start stable.");
}

static void IsolationAppliesIntegrityPenaltyAndThreat()
{
    var game = NetworkGame.CreateDefault();
    var isolated = game.Board.GetNode(new NodeId(2, 0));
    isolated.SetOwner(NodeOwner.Player);

    game.RefreshNetworkRisk();

    AssertEqual(1, isolated.Integrity);
    AssertTrue(isolated.Threat >= 9, "Expected isolation, weak links, and frontier exposure to create high threat.");
    AssertTrue(isolated.IsUnstable, "Expected isolated player node to be unstable.");
    AssertContains("isolated from core", isolated.DangerReason);
}

static void RelaySupportIncreasesIntegrity()
{
    var game = NetworkGame.CreateDefault();
    game.Board.GetNode(new NodeId(0, 1)).SetOwner(NodeOwner.Player);
    game.Board.GetNode(new NodeId(0, 2)).SetOwner(NodeOwner.Player);

    game.RefreshNetworkRisk();

    var relaySupported = game.Board.GetNode(new NodeId(0, 2));
    AssertTrue(relaySupported.Integrity >= 8, "Expected adjacent Relay support to increase integrity.");
}

static void FirewallSupportIncreasesIntegrity()
{
    var game = NetworkGame.CreateDefault();
    var firewall = game.Board.GetNode(new NodeId(2, 3));
    firewall.SetOwner(NodeOwner.Player);

    game.RefreshNetworkRisk();

    AssertTrue(firewall.Integrity >= 3, "Expected Firewall node support to offset isolation penalty.");
}

static void ClaimingSpendsPlayerEnergy()
{
    var game = NetworkGame.CreateDefault();

    var result = game.ClaimNodeWithResult(new NodeId(0, 1));

    AssertTrue(result.Succeeded, "Expected relay claim to succeed.");
    AssertEqual(NetworkRules.InitialPlayerEnergy - NetworkRules.ClaimEnergyCost, game.PlayerEnergy);
    AssertEqual(NetworkRules.ClaimEnergyCost, result.EnergySpent);
}

static void ResourceNodeGeneratesEnergyOnNextPlayerTurn()
{
    var game = NetworkGame.CreateDefault();

    var result = game.ClaimNodeWithResult(new NodeId(1, 0));

    AssertTrue(result.Succeeded, "Expected resource claim to succeed.");
    AssertEqual(NetworkRules.ResourceEnergyPerTurn, result.EnergyGenerated);
    AssertEqual(NetworkRules.InitialPlayerEnergy, game.PlayerEnergy);
}

static void InsufficientEnergyPreventsPlayerAction()
{
    var game = NetworkGame.CreateDefault();

    for (var i = 0; i < NetworkRules.InitialPlayerEnergy; i++)
    {
        AssertTrue(game.ReinforceNode(NetworkGame.DefaultPlayerStart), "Expected setup reinforcement to spend energy.");
    }

    var turnBefore = game.TurnNumber;
    var pressureBefore = game.CorruptionPressure;
    var startIntegrity = game.Board.GetNode(NetworkGame.DefaultPlayerStart).Integrity;
    var result = game.ReinforceNodeWithResult(NetworkGame.DefaultPlayerStart);

    AssertFalse(result.Succeeded, "Expected reinforcement without energy to fail.");
    AssertEqual(0, game.PlayerEnergy);
    AssertEqual(turnBefore, game.TurnNumber);
    AssertEqual(pressureBefore, game.CorruptionPressure);
    AssertEqual(startIntegrity, game.Board.GetNode(NetworkGame.DefaultPlayerStart).Integrity);
}

static void RelayExtendsPlayerClaimRange()
{
    var game = NetworkGame.CreateDefault();

    AssertTrue(game.ClaimNode(new NodeId(0, 1)), "Expected adjacent relay claim to succeed.");
    var claimedThroughRelay = game.ClaimNodeWithResult(new NodeId(0, 3));

    AssertTrue(claimedThroughRelay.Succeeded, "Expected relay to extend claim range by active connections.");
    AssertEqual(NodeOwner.Player, game.Board.GetNode(new NodeId(0, 3)).Owner);
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

    game.EndPlayerTurn();

    AssertEqual(NodeOwner.Enemy, game.Board.GetNode(new NodeId(3, 2)).Owner);
}

static void FirewallResistsFirstCorruptionPressure()
{
    var game = NetworkGame.CreateDefault();
    game.Board.GetNode(new NodeId(3, 2)).SetOwner(NodeOwner.Player);

    var result = game.EndPlayerTurnWithResult();

    AssertTrue(result.Succeeded, "Expected end turn to resolve.");
    AssertEqual(NodeOwner.Neutral, game.Board.GetNode(new NodeId(2, 3)).Owner);
    AssertEqual(1, game.CorruptionPressure);
}

static void CorruptionPressureProgressesDeterministically()
{
    var game = NetworkGame.CreateDefault();
    game.Board.GetNode(new NodeId(3, 2)).SetOwner(NodeOwner.Player);

    game.EndPlayerTurn();
    AssertEqual(1, game.CorruptionPressure);

    game.EndPlayerTurn();
    AssertEqual(NodeOwner.Enemy, game.Board.GetNode(new NodeId(3, 2)).Owner);
    AssertEqual(1, game.CorruptionPressure);
}

static void ThreatProgressionMarksExposedNodesUnstable()
{
    var game = NetworkGame.CreateDefault();
    var exposed = game.Board.GetNode(new NodeId(3, 2));
    exposed.SetOwner(NodeOwner.Player);

    game.RefreshNetworkRisk();

    AssertTrue(exposed.Threat > exposed.Integrity, "Expected adjacent corruption pressure to exceed isolated node integrity.");
    AssertTrue(exposed.IsUnstable, "Expected exposed node to be unstable.");
    AssertContains("adjacent corruption", exposed.DangerReason);
}

static void PersistentInstabilityCollapsesNode()
{
    var game = NetworkGame.CreateDefault();
    var exposedId = new NodeId(3, 2);
    game.Board.GetNode(exposedId).SetOwner(NodeOwner.Player);
    game.RefreshNetworkRisk();

    var firstTurn = game.EndPlayerTurnWithResult();
    AssertTrue(firstTurn.Succeeded, "Expected first enemy turn to resolve.");
    AssertEqual(NodeOwner.Player, game.Board.GetNode(exposedId).Owner);
    AssertEqual(1, game.Board.GetNode(exposedId).UnstableTurns);

    var secondTurn = game.EndPlayerTurnWithResult();
    AssertTrue(secondTurn.Succeeded, "Expected second enemy turn to resolve.");
    AssertEqual(NodeOwner.Enemy, game.Board.GetNode(exposedId).Owner);
    AssertTrue(secondTurn.CollapsedNodes?.Contains(exposedId) == true, "Expected collapse event to include exposed node.");
}

static void CorruptionTargetingPrioritizesUnstableNodesDeterministically()
{
    var game = NetworkGame.CreateDefault();
    var exposedId = new NodeId(3, 2);
    game.Board.GetNode(exposedId).SetOwner(NodeOwner.Player);
    game.RefreshNetworkRisk();

    var target = CorruptionTargetPolicy.SelectExpansionTarget(game.Board, game.Configuration);

    AssertEqual(exposedId, target);
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

static void EndTurnResolvesRealCorruptionTurn()
{
    var game = NetworkGame.CreateDefault();

    var result = game.EndPlayerTurnWithResult();

    AssertTrue(result.Succeeded, "Expected end turn to succeed.");
    AssertEqual(0, result.EnergySpent);
    AssertEqual(NodeOwner.Enemy, game.Board.GetNode(new NodeId(3, 2)).Owner);
    AssertEqual(2, game.TurnNumber);
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

static string DescribeBoard(NetworkBoard board)
{
    return string.Join("|", board.Nodes
        .OrderBy(node => node.Id)
        .Select(node => $"{node.Id}:{node.Owner}:{node.Type}:{node.Integrity}:{node.Threat}"));
}
