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
    ("vertical slice mission initializes authored board", VerticalSliceMissionInitializesAuthoredBoard),
    ("vertical slice mission wins after objective hold", VerticalSliceMissionWinsAfterObjectiveHold),
    ("vertical slice mission loses when core collapses", VerticalSliceMissionLosesWhenCoreCollapses),
    ("vertical slice mission loses when corruption captures objective", VerticalSliceMissionLosesWhenCorruptionCapturesObjective),
    ("unsecured objective can be punished by tactical AI", UnsecuredObjectiveCanBePunishedByTacticalAi),
    ("action mode routes claim reinforce and weaken", ActionModeRoutesClaimReinforceAndWeaken),
    ("vertical slice restart is deterministic", VerticalSliceRestartIsDeterministic),
    ("invalid actions after game over do not mutate mission", InvalidActionsAfterGameOverDoNotMutateMission),
    ("procedural mission generation is deterministic", ProceduralMissionGenerationIsDeterministic),
    ("procedural mission seeds create different layouts", ProceduralMissionSeedsCreateDifferentLayouts),
    ("procedural mission satisfies graph validity", ProceduralMissionSatisfiesGraphValidity),
    ("procedural mission placement follows gameplay constraints", ProceduralMissionPlacementFollowsGameplayConstraints),
    ("procedural layout is readable and complete", ProceduralLayoutIsReadableAndComplete),
    ("tactical AI is deterministic for identical seeded missions", TacticalAiIsDeterministicForIdenticalSeededMissions),
    ("tactical AI selects valid reachable actions", TacticalAiSelectsValidReachableActions),
    ("tactical AI prioritizes objective pressure", TacticalAiPrioritizesObjectivePressure),
    ("enemy personalities choose different targets", EnemyPersonalitiesChooseDifferentTargets),
    ("difficulty changes decision quality without bonuses", DifficultyChangesDecisionQualityWithoutBonuses),
    ("tactical AI produces stable intent summaries", TacticalAiProducesStableIntentSummaries),
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

static void VerticalSliceMissionInitializesAuthoredBoard()
{
    var game = NetworkGame.CreateVerticalSliceMission();

    AssertEqual("Secure the Uplink", game.MissionDefinition?.Name);
    AssertEqual(5, game.Board.Width);
    AssertEqual(5, game.Board.Height);
    AssertEqual(new NodeId(0, 2), game.PlayerCore);
    AssertEqual(new NodeId(3, 2), game.ObjectiveNode);
    AssertEqual(2, game.RequiredObjectiveHoldTurns);
    AssertEqual(6, game.PlayerEnergy);
    AssertEqual(NodeOwner.Player, game.Board.GetNode(new NodeId(0, 2)).Owner);
    AssertEqual(NodeOwner.Enemy, game.Board.GetNode(new NodeId(4, 4)).Owner);
    AssertEqual(NodeType.Resource, game.Board.GetNode(new NodeId(1, 2)).Type);
    AssertEqual(NodeType.Relay, game.Board.GetNode(new NodeId(2, 2)).Type);
    AssertEqual(NodeType.Firewall, game.Board.GetNode(new NodeId(3, 2)).Type);
}

static void VerticalSliceMissionWinsAfterObjectiveHold()
{
    var game = PlayToObjectiveClaim();

    AssertEqual(GameResult.InProgress, game.Result);
    AssertEqual(1, game.ObjectiveHoldTurns);

    var result = game.ExecutePlayerAction(PlayerActionMode.Reinforce, game.ObjectiveNode!.Value);

    AssertTrue(result.Succeeded, "Expected end turn to complete the objective hold.");
    AssertEqual(GameResult.PlayerWin, game.Result);
    AssertEqual(2, game.ObjectiveHoldTurns);
    AssertContains("Mission complete", result.Message);
}

static void VerticalSliceMissionLosesWhenCoreCollapses()
{
    var game = NetworkGame.CreateVerticalSliceMission();
    game.Board.GetNode(game.PlayerCore).SetOwner(NodeOwner.Enemy);
    game.RefreshNetworkRisk();

    var result = game.EndPlayerTurnWithResult();

    AssertTrue(result.Succeeded, "Expected mission to evaluate after end turn.");
    AssertEqual(GameResult.PlayerLoss, game.Result);
    AssertEqual(0, game.ObjectiveHoldTurns);
    AssertContains("Mission failed", result.Message);
}

static void VerticalSliceMissionLosesWhenCorruptionCapturesObjective()
{
    var game = NetworkGame.CreateVerticalSliceMission();
    game.Board.GetNode(game.ObjectiveNode!.Value).SetOwner(NodeOwner.Enemy);
    game.RefreshNetworkRisk();

    var result = game.EndPlayerTurnWithResult();

    AssertTrue(result.Succeeded, "Expected mission to evaluate after end turn.");
    AssertEqual(GameResult.PlayerLoss, game.Result);
    AssertEqual(0, game.ObjectiveHoldTurns);
}

static void UnsecuredObjectiveCanBePunishedByTacticalAi()
{
    var game = PlayToObjectiveClaim();
    AssertEqual(1, game.ObjectiveHoldTurns);

    game.Board.GetNode(game.ObjectiveNode!.Value).SetOwner(NodeOwner.Neutral);
    game.RefreshNetworkRisk();
    var result = game.EndPlayerTurnWithResult();

    AssertTrue(result.Succeeded, "Expected end turn to resolve after objective is lost.");
    AssertEqual(GameResult.PlayerLoss, game.Result);
    AssertEqual(0, game.ObjectiveHoldTurns);
    AssertEqual(game.ObjectiveNode, result.CorruptionTarget);
    AssertContains("objective proximity", result.EnemyIntentSummary);
}

static void ActionModeRoutesClaimReinforceAndWeaken()
{
    var game = NetworkGame.CreateVerticalSliceMission();

    var claim = game.ExecutePlayerAction(PlayerActionMode.Claim, new NodeId(1, 2));
    AssertTrue(claim.Succeeded, "Expected claim mode to claim a reachable node.");
    AssertEqual(NodeOwner.Player, game.Board.GetNode(new NodeId(1, 2)).Owner);

    var reinforce = game.ExecutePlayerAction(PlayerActionMode.Reinforce, new NodeId(1, 2));
    AssertTrue(reinforce.Succeeded, "Expected reinforce mode to reinforce an owned node.");
    AssertEqual(1, game.Board.GetNode(new NodeId(1, 2)).ReinforcementLevel);

    game.Board.GetNode(new NodeId(4, 3)).SetOwner(NodeOwner.Player);
    game.RefreshNetworkRisk();
    var connection = game.Board.FindConnection(new NodeId(4, 3), new NodeId(4, 4))
        ?? throw new InvalidOperationException("Expected setup connection to exist.");
    var weaken = game.ExecutePlayerAction(PlayerActionMode.Weaken, new NodeId(4, 4));

    AssertTrue(weaken.Succeeded, "Expected weaken mode to attack adjacent corruption.");
    AssertEqual(1, connection.Strength);
}

static void VerticalSliceRestartIsDeterministic()
{
    var first = NetworkGame.CreateVerticalSliceMission();
    AssertTrue(first.ClaimNode(new NodeId(1, 2)), "Expected setup claim to succeed.");
    var restarted = NetworkGame.CreateVerticalSliceMission();
    var secondRestart = NetworkGame.CreateVerticalSliceMission();

    AssertEqual(DescribeBoard(restarted.Board), DescribeBoard(secondRestart.Board));
    AssertEqual(1, restarted.TurnNumber);
    AssertEqual(0, restarted.ObjectiveHoldTurns);
    AssertEqual(GameResult.InProgress, restarted.Result);
}

static void InvalidActionsAfterGameOverDoNotMutateMission()
{
    var game = PlayToObjectiveClaim();
    AssertTrue(game.ExecutePlayerAction(PlayerActionMode.Reinforce, game.ObjectiveNode!.Value).Succeeded, "Expected objective reinforcement to win the mission.");
    var boardBefore = DescribeBoard(game.Board);
    var turnBefore = game.TurnNumber;
    var energyBefore = game.PlayerEnergy;

    var claim = game.ExecutePlayerAction(PlayerActionMode.Claim, new NodeId(0, 0));
    var reinforce = game.ExecutePlayerAction(PlayerActionMode.Reinforce, game.PlayerCore);
    var endTurn = game.EndPlayerTurnWithResult();

    AssertFalse(claim.Succeeded, "Expected claim after game over to fail.");
    AssertFalse(reinforce.Succeeded, "Expected reinforce after game over to fail.");
    AssertFalse(endTurn.Succeeded, "Expected end turn after game over to fail.");
    AssertEqual(boardBefore, DescribeBoard(game.Board));
    AssertEqual(turnBefore, game.TurnNumber);
    AssertEqual(energyBefore, game.PlayerEnergy);
}

static void ProceduralMissionGenerationIsDeterministic()
{
    var first = ProceduralMissionGenerator.Generate("regression-seed-6");
    var second = ProceduralMissionGenerator.Generate("regression-seed-6");

    AssertEqual(DescribeDefinition(first.BoardDefinition), DescribeDefinition(second.BoardDefinition));
    AssertEqual(first.ObjectiveNode, second.ObjectiveNode);
    AssertEqual(first.ObjectiveText, second.ObjectiveText);
    AssertEqual("regression-seed-6", first.BoardDefinition.Metadata["seedText"]);
}

static void ProceduralMissionSeedsCreateDifferentLayouts()
{
    var first = ProceduralMissionGenerator.Generate("alpha-network");
    var second = ProceduralMissionGenerator.Generate("beta-network");

    AssertFalse(DescribeDefinition(first.BoardDefinition) == DescribeDefinition(second.BoardDefinition), "Expected different seeds to produce different mission topology or layout.");
}

static void ProceduralMissionSatisfiesGraphValidity()
{
    var settings = ProceduralMissionSettings.Default with
    {
        NodeCount = 22,
        ObjectiveDistance = 6,
        GraphDensity = 0.34d,
        MaxBranchingFactor = 4
    };
    var mission = ProceduralMissionGenerator.Generate(20260627, settings);
    var board = mission.BoardDefinition;

    AssertEqual(22, board.Nodes.Count);
    AssertTrue(board.Links.Count >= board.Nodes.Count - 1, "Expected enough links for a connected graph.");
    AssertEqual(board.Nodes.Count, GetReachableNodes(board, board.PlayerStart).Count);
    AssertTrue(GetShortestPathLength(board, board.PlayerStart, mission.ObjectiveNode) >= 3, "Expected objective to require traversal.");
    AssertTrue(board.Nodes.All(node => GetDegree(board, node) > 0), "Expected no isolated nodes.");
    AssertTrue(board.Nodes.Average(node => GetDegree(board, node)) <= 5.2d, "Expected a readable branching factor.");
}

static void ProceduralMissionPlacementFollowsGameplayConstraints()
{
    var mission = ProceduralMissionGenerator.Generate("placement-check");
    var game = NetworkGame.CreateMission(mission);

    AssertEqual(NodeOwner.Player, game.Board.GetNode(game.PlayerCore).Owner);
    AssertEqual(NodeOwner.Neutral, game.Board.GetNode(mission.ObjectiveNode).Owner);
    AssertTrue(mission.BoardDefinition.CorruptionStarts.All(node => game.Board.GetNode(node).Owner == NodeOwner.Enemy), "Expected all corruption starts to initialize as enemy-owned.");
    AssertFalse(mission.BoardDefinition.CorruptionStarts.Contains(game.PlayerCore), "Expected corruption to start away from player core.");
    AssertFalse(mission.BoardDefinition.CorruptionStarts.Contains(mission.ObjectiveNode), "Expected corruption to start away from the objective.");
    AssertTrue(mission.BoardDefinition.ResourceNodes.Count >= 1, "Expected at least one generated Resource.");
    AssertTrue(mission.BoardDefinition.RelayNodes.Count >= 1, "Expected at least one generated Relay.");
    AssertTrue(mission.BoardDefinition.FirewallNodes.Count >= 1, "Expected at least one generated Firewall.");
    AssertTrue(game.PlayerEnergy >= NetworkRules.InitialPlayerEnergy, "Expected procedural starting energy to preserve existing action pacing.");
}

static void ProceduralLayoutIsReadableAndComplete()
{
    var mission = ProceduralMissionGenerator.Generate("layout-check");
    var board = mission.BoardDefinition;

    AssertEqual(board.Nodes.Count, board.Layout.Count);
    foreach (var node in board.Nodes)
    {
        AssertTrue(board.Layout.ContainsKey(node), $"Expected layout position for {node}.");
    }

    var closestDistance = board.Nodes
        .SelectMany(first => board.Nodes.Where(second => first.CompareTo(second) < 0).Select(second => GetLayoutDistance(board.Layout[first], board.Layout[second])))
        .Min();
    AssertTrue(closestDistance >= 72f, "Expected generated nodes to be visibly separated.");

    var crossings = CountEdgeCrossings(board);
    AssertTrue(crossings <= board.Links.Count / 3, $"Expected restrained edge crossings, got {crossings} for {board.Links.Count} links.");
}

static void TacticalAiIsDeterministicForIdenticalSeededMissions()
{
    var configuration = new GameConfiguration
    {
        EnemyPersonality = EnemyPersonality.CorruptionFocused,
        EnemyDifficulty = EnemyDifficulty.Expert
    };
    var first = NetworkGame.CreateMission(ProceduralMissionGenerator.Generate("ai-determinism"), configuration);
    var second = NetworkGame.CreateMission(ProceduralMissionGenerator.Generate("ai-determinism"), configuration);

    var firstResult = first.EndPlayerTurnWithResult();
    var secondResult = second.EndPlayerTurnWithResult();

    AssertEqual(DescribeBoard(first.Board), DescribeBoard(second.Board));
    AssertEqual(firstResult.CorruptionTarget, secondResult.CorruptionTarget);
    AssertEqual(firstResult.CorruptionFocusTarget, secondResult.CorruptionFocusTarget);
    AssertEqual(firstResult.EnemyIntentSummary, secondResult.EnemyIntentSummary);
}

static void TacticalAiSelectsValidReachableActions()
{
    var game = NetworkGame.CreateMission(ProceduralMissionGenerator.Generate("ai-valid-action"), new GameConfiguration
    {
        EnemyPersonality = EnemyPersonality.Opportunistic,
        EnemyDifficulty = EnemyDifficulty.Hard
    });

    var result = game.EndPlayerTurnWithResult();
    var target = result.CorruptionTarget ?? result.CorruptionFocusTarget;

    AssertTrue(target.HasValue, "Expected tactical AI to choose a reachable target.");
    AssertTrue(result.EnemyActionSource.HasValue, "Expected tactical AI to report the source of its action.");
    var targetId = target!.Value;
    var source = result.EnemyActionSource!.Value;
    AssertTrue(game.Board.AreConnected(source, targetId), "Expected enemy action to use an active adjacent connection.");
    AssertEqual(NodeOwner.Enemy, game.Board.GetNode(source).Owner);
    AssertFalse(result.EnemyActionType == TacticalEnemyActionType.CorruptNode && game.Board.GetNode(targetId).Owner != NodeOwner.Enemy, "Expected corruption actions to produce an enemy-owned target.");
}

static void TacticalAiPrioritizesObjectivePressure()
{
    var game = NetworkGame.CreateMission(CreateObjectivePressureMission(), new GameConfiguration
    {
        EnemyPersonality = EnemyPersonality.Aggressive,
        EnemyDifficulty = EnemyDifficulty.Expert
    });

    var decision = TacticalEnemyPlanner.SelectDecision(game.Board, game.Configuration, game.PlayerCore, game.ObjectiveNode, corruptionPressure: 1, turnNumber: 1);

    AssertEqual(game.ObjectiveNode, decision.Target);
    AssertContains("objective", decision.PrimaryFactor);
}

static void EnemyPersonalitiesChooseDifferentTargets()
{
    var mission = CreatePersonalityTestMission();
    var aggressive = NetworkGame.CreateMission(mission, new GameConfiguration { EnemyPersonality = EnemyPersonality.Aggressive, EnemyDifficulty = EnemyDifficulty.Expert });
    var defensive = NetworkGame.CreateMission(mission, new GameConfiguration { EnemyPersonality = EnemyPersonality.Defensive, EnemyDifficulty = EnemyDifficulty.Expert });
    var economic = NetworkGame.CreateMission(mission, new GameConfiguration { EnemyPersonality = EnemyPersonality.Economic, EnemyDifficulty = EnemyDifficulty.Expert });

    var aggressiveTarget = TacticalEnemyPlanner.SelectDecision(aggressive.Board, aggressive.Configuration, aggressive.PlayerCore, aggressive.ObjectiveNode, 1, 1).Target;
    var defensiveTarget = TacticalEnemyPlanner.SelectDecision(defensive.Board, defensive.Configuration, defensive.PlayerCore, defensive.ObjectiveNode, 1, 1).Target;
    var economicTarget = TacticalEnemyPlanner.SelectDecision(economic.Board, economic.Configuration, economic.PlayerCore, economic.ObjectiveNode, 1, 1).Target;

    AssertEqual(new NodeId(1, 1), aggressiveTarget);
    AssertEqual(new NodeId(3, 1), defensiveTarget);
    AssertEqual(new NodeId(2, 0), economicTarget);
}

static void DifficultyChangesDecisionQualityWithoutBonuses()
{
    var mission = CreatePersonalityTestMission();
    var easy = NetworkGame.CreateMission(mission, new GameConfiguration { EnemyPersonality = EnemyPersonality.Opportunistic, EnemyDifficulty = EnemyDifficulty.Easy });
    var expert = NetworkGame.CreateMission(mission, new GameConfiguration { EnemyPersonality = EnemyPersonality.Opportunistic, EnemyDifficulty = EnemyDifficulty.Expert });

    var easyDecision = TacticalEnemyPlanner.SelectDecision(easy.Board, easy.Configuration, easy.PlayerCore, easy.ObjectiveNode, 1, 1);
    var expertDecision = TacticalEnemyPlanner.SelectDecision(expert.Board, expert.Configuration, expert.PlayerCore, expert.ObjectiveNode, 1, 1);

    AssertTrue(expertDecision.Score >= easyDecision.Score, "Expected higher difficulty to select an equal or better evaluated action.");
    AssertEqual(NetworkRules.CorruptionPressureGrowthPerTurn, easy.Configuration.CorruptionPressureGrowthPerTurn);
    AssertEqual(NetworkRules.CorruptionPressureGrowthPerTurn, expert.Configuration.CorruptionPressureGrowthPerTurn);
    AssertEqual(NetworkRules.StandardCorruptionResistance, easy.Configuration.StandardCorruptionResistance);
    AssertEqual(NetworkRules.StandardCorruptionResistance, expert.Configuration.StandardCorruptionResistance);
}

static void TacticalAiProducesStableIntentSummaries()
{
    var game = NetworkGame.CreateDefault();

    var result = game.EndPlayerTurnWithResult();

    AssertTrue(result.EnemyActionType != TacticalEnemyActionType.None, "Expected tactical AI to report an action type.");
    AssertFalse(string.IsNullOrWhiteSpace(result.EnemyPrimaryFactor), "Expected tactical AI to report the primary scoring factor.");
    AssertContains(result.CorruptionTarget?.ToString() ?? result.CorruptionFocusTarget?.ToString() ?? string.Empty, result.EnemyIntentSummary);
}

static NetworkGame PlayToObjectiveClaim()
{
    var game = NetworkGame.CreateVerticalSliceMission();

    AssertTrue(game.ExecutePlayerAction(PlayerActionMode.Claim, new NodeId(1, 2)).Succeeded, "Expected resource claim to succeed.");
    AssertTrue(game.ExecutePlayerAction(PlayerActionMode.Claim, new NodeId(2, 2)).Succeeded, "Expected relay claim to succeed.");
    AssertTrue(game.ExecutePlayerAction(PlayerActionMode.Claim, new NodeId(3, 2)).Succeeded, "Expected objective claim to succeed from the relay anchor.");

    return game;
}

static MissionDefinition CreateObjectivePressureMission()
{
    var nodeTypes = new Dictionary<NodeId, NodeType>
    {
        [new NodeId(1, 0)] = NodeType.Resource
    };
    var board = BoardDefinition.CreateGrid(
        3,
        2,
        new NodeId(0, 0),
        new[] { new NodeId(2, 0) },
        nodeTypes,
        initialOwnership: new Dictionary<NodeId, NodeOwner> { [new NodeId(1, 0)] = NodeOwner.Neutral });

    return new MissionDefinition("Objective Pressure", board, new NodeId(1, 0), 1, "Hold the exposed service.");
}

static MissionDefinition CreatePersonalityTestMission()
{
    var nodeTypes = new Dictionary<NodeId, NodeType>
    {
        [new NodeId(2, 0)] = NodeType.Resource,
        [new NodeId(3, 1)] = NodeType.Firewall
    };
    var initialOwnership = new Dictionary<NodeId, NodeOwner>
    {
        [new NodeId(1, 1)] = NodeOwner.Player
    };
    var board = BoardDefinition.CreateGrid(
        4,
        3,
        new NodeId(0, 1),
        new[] { new NodeId(2, 1) },
        nodeTypes,
        initialOwnership: initialOwnership);

    return new MissionDefinition("Personality Lab", board, new NodeId(3, 2), 1, "Test enemy intent.");
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

static string DescribeDefinition(BoardDefinition definition)
{
    var nodes = string.Join(",", definition.Nodes);
    var links = string.Join(",", definition.Links.Select(link => $"{link.First}-{link.Second}"));
    var types = string.Join(",", definition.NodeTypes.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key}:{pair.Value}"));
    var owners = string.Join(",", definition.InitialOwnership.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key}:{pair.Value}"));
    var layout = string.Join(",", definition.Layout.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key}:{pair.Value.X:0.00}:{pair.Value.Y:0.00}"));
    return $"{nodes}|{links}|{types}|{owners}|{definition.PlayerStart}|{string.Join(",", definition.CorruptionStarts)}|{layout}";
}

static IReadOnlySet<NodeId> GetReachableNodes(BoardDefinition definition, NodeId start)
{
    var visited = new HashSet<NodeId> { start };
    var frontier = new Queue<NodeId>();
    frontier.Enqueue(start);

    while (frontier.Count > 0)
    {
        var current = frontier.Dequeue();
        foreach (var adjacent in GetAdjacent(definition, current))
        {
            if (visited.Add(adjacent))
            {
                frontier.Enqueue(adjacent);
            }
        }
    }

    return visited;
}

static int GetShortestPathLength(BoardDefinition definition, NodeId start, NodeId target)
{
    var distances = new Dictionary<NodeId, int> { [start] = 0 };
    var frontier = new Queue<NodeId>();
    frontier.Enqueue(start);

    while (frontier.Count > 0)
    {
        var current = frontier.Dequeue();
        if (current.Equals(target))
        {
            return distances[current];
        }

        foreach (var adjacent in GetAdjacent(definition, current))
        {
            if (distances.ContainsKey(adjacent))
            {
                continue;
            }

            distances[adjacent] = distances[current] + 1;
            frontier.Enqueue(adjacent);
        }
    }

    return -1;
}

static IReadOnlyList<NodeId> GetAdjacent(BoardDefinition definition, NodeId node)
{
    return definition.Links
        .Where(link => link.Contains(node))
        .Select(link => link.First.Equals(node) ? link.Second : link.First)
        .OrderBy(adjacent => adjacent)
        .ToList();
}

static int GetDegree(BoardDefinition definition, NodeId node)
{
    return definition.Links.Count(link => link.Contains(node));
}

static float GetLayoutDistance(NetworkNodePosition first, NetworkNodePosition second)
{
    var x = first.X - second.X;
    var y = first.Y - second.Y;
    return MathF.Sqrt(x * x + y * y);
}

static int CountEdgeCrossings(BoardDefinition definition)
{
    var crossings = 0;
    for (var i = 0; i < definition.Links.Count; i++)
    {
        for (var j = i + 1; j < definition.Links.Count; j++)
        {
            var first = definition.Links[i];
            var second = definition.Links[j];
            if (first.Contains(second.First) || first.Contains(second.Second))
            {
                continue;
            }

            if (SegmentsCross(definition.Layout[first.First], definition.Layout[first.Second], definition.Layout[second.First], definition.Layout[second.Second]))
            {
                crossings++;
            }
        }
    }

    return crossings;
}

static bool SegmentsCross(NetworkNodePosition a, NetworkNodePosition b, NetworkNodePosition c, NetworkNodePosition d)
{
    return Direction(a, c, d) != Direction(b, c, d) && Direction(a, b, c) != Direction(a, b, d);
}

static bool Direction(NetworkNodePosition a, NetworkNodePosition b, NetworkNodePosition c)
{
    return ((c.X - a.X) * (b.Y - a.Y) - (b.X - a.X) * (c.Y - a.Y)) > 0;
}
