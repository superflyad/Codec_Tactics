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
    ("board definition supports explicit layers and transitions", BoardDefinitionSupportsExplicitLayersAndTransitions),
    ("game configuration defaults preserve network rules", GameConfigurationDefaultsPreserveNetworkRules),
    ("layer rule modifiers affect integrity and threat", LayerRuleModifiersAffectIntegrityAndThreat),
    ("layer rule modifiers affect corruption resistance", LayerRuleModifiersAffectCorruptionResistance),
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
    ("game snapshot restores active mission continuation", GameSnapshotRestoresActiveMissionContinuation),
    ("procedural mission generation is deterministic", ProceduralMissionGenerationIsDeterministic),
    ("procedural mission seeds create different layouts", ProceduralMissionSeedsCreateDifferentLayouts),
    ("procedural mission keeps generated nodes reachable", ProceduralMissionKeepsGeneratedNodesReachable),
    ("procedural mission satisfies graph validity", ProceduralMissionSatisfiesGraphValidity),
    ("procedural mission placement follows gameplay constraints", ProceduralMissionPlacementFollowsGameplayConstraints),
    ("procedural missions expose layer metadata", ProceduralMissionsExposeLayerMetadata),
    ("procedural layout is readable and complete", ProceduralLayoutIsReadableAndComplete),
    ("scenario catalog provides balanced regression scenarios", ScenarioCatalogProvidesBalancedRegressionScenarios),
    ("generated mission corpus passes balance screening", GeneratedMissionCorpusPassesBalanceScreening),
    ("balance screening covers personality pressure matrix", BalanceScreeningCoversPersonalityPressureMatrix),
    ("tactical AI profiles stay legal across seed corpus", TacticalAiProfilesStayLegalAcrossSeedCorpus),
    ("campaign progression starts from a deterministic opening trace", CampaignProgressionStartsFromDeterministicOpeningTrace),
    ("campaign progression advances after wins", CampaignProgressionAdvancesAfterWins),
    ("campaign progression recovery softens after a loss", CampaignProgressionRecoverySoftensAfterLoss),
    ("campaign progression uses authored arc missions", CampaignProgressionUsesAuthoredArcMissions),
    ("profile progression summarizes completed missions", ProfileProgressionSummarizesCompletedMissions),
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

static void BoardDefinitionSupportsExplicitLayersAndTransitions()
{
    var nodes = new[] { new NodeId(0, 0), new NodeId(0, 1), new NodeId(1, 0), new NodeId(1, 1) };
    var links = new[]
    {
        new NetworkLink(new NodeId(0, 0), new NodeId(0, 1)),
        new NetworkLink(new NodeId(0, 1), new NodeId(1, 0)),
        new NetworkLink(new NodeId(1, 0), new NodeId(1, 1))
    };
    var layers = new Dictionary<NodeId, int>
    {
        [new NodeId(0, 0)] = 0,
        [new NodeId(0, 1)] = 0,
        [new NodeId(1, 0)] = 1,
        [new NodeId(1, 1)] = 1
    };

    var definition = BoardDefinition.CreateTopology(
        2,
        2,
        nodes,
        links,
        new NodeId(0, 0),
        new[] { new NodeId(1, 1) },
        nodeLayers: layers);

    AssertEqual(2, definition.LayerCount);
    AssertEqual(0, definition.GetLayer(new NodeId(0, 1)));
    AssertEqual(1, definition.GetLayer(new NodeId(1, 0)));
    AssertEqual(1, definition.TransitionLinks.Count);
    AssertEqual(new NetworkLink(new NodeId(0, 1), new NodeId(1, 0)), definition.TransitionLinks.Single());
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
    AssertEqual(0, configuration.LayerModifiers.Count);
}

static void LayerRuleModifiersAffectIntegrityAndThreat()
{
    var playerCore = new NodeId(0, 0);
    var tunedNode = new NodeId(1, 0);
    var enemyNode = new NodeId(2, 0);
    var definition = BoardDefinition.CreateTopology(
        3,
        2,
        new[] { playerCore, tunedNode, enemyNode },
        new[] { new NetworkLink(playerCore, tunedNode), new NetworkLink(tunedNode, enemyNode) },
        playerCore,
        new[] { enemyNode },
        initialOwnership: new Dictionary<NodeId, NodeOwner>
        {
            [playerCore] = NodeOwner.Player,
            [tunedNode] = NodeOwner.Player,
            [enemyNode] = NodeOwner.Enemy
        },
        nodeLayers: new Dictionary<NodeId, int>
        {
            [playerCore] = 0,
            [tunedNode] = 1,
            [enemyNode] = 1
        });
    var game = NetworkGame.Create(definition, new GameConfiguration
    {
        LayerModifiers = new Dictionary<int, LayerRuleModifier>
        {
            [1] = new(IntegrityBonus: 2, ThreatBonus: 3)
        }
    });

    var node = game.Board.GetNode(tunedNode);

    AssertEqual(10, node.Integrity);
    AssertEqual(12, node.Threat);
    AssertContains("layer 2 tuning", node.DangerReason);
}

static void LayerRuleModifiersAffectCorruptionResistance()
{
    var playerCore = new NodeId(0, 0);
    var target = new NodeId(1, 0);
    var enemyNode = new NodeId(2, 0);
    var definition = BoardDefinition.CreateTopology(
        3,
        2,
        new[] { playerCore, target, enemyNode },
        new[] { new NetworkLink(playerCore, target), new NetworkLink(target, enemyNode) },
        playerCore,
        new[] { enemyNode },
        nodeLayers: new Dictionary<NodeId, int>
        {
            [playerCore] = 0,
            [target] = 1,
            [enemyNode] = 1
        });
    var game = NetworkGame.Create(definition, new GameConfiguration
    {
        EnemyDifficulty = EnemyDifficulty.Expert,
        LayerModifiers = new Dictionary<int, LayerRuleModifier>
        {
            [1] = new(CorruptionResistanceBonus: 2)
        }
    });

    var result = game.EndPlayerTurnWithResult();

    AssertEqual(NodeOwner.Neutral, game.Board.GetNode(target).Owner);
    AssertEqual(target, result.CorruptionFocusTarget);
    AssertEqual(1, game.CorruptionPressure);
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

static void GameSnapshotRestoresActiveMissionContinuation()
{
    var game = PlayToObjectiveClaim();
    var snapshot = game.CreateSnapshot();
    var restored = NetworkGame.RestoreSnapshot(snapshot);

    AssertEqual(game.TurnNumber, restored.TurnNumber);
    AssertEqual(game.PlayerEnergy, restored.PlayerEnergy);
    AssertEqual(game.CorruptionPressure, restored.CorruptionPressure);
    AssertEqual(game.ObjectiveHoldTurns, restored.ObjectiveHoldTurns);
    AssertEqual(game.Result, restored.Result);
    AssertEqual(game.LastActionResult.Message, restored.LastActionResult.Message);
    AssertEqual(DescribeBoard(game.Board), DescribeBoard(restored.Board));

    var originalResult = game.ExecutePlayerAction(PlayerActionMode.Reinforce, game.ObjectiveNode!.Value);
    var restoredResult = restored.ExecutePlayerAction(PlayerActionMode.Reinforce, restored.ObjectiveNode!.Value);

    AssertEqual(originalResult.Succeeded, restoredResult.Succeeded);
    AssertEqual(originalResult.Result, restoredResult.Result);
    AssertEqual(originalResult.CorruptionTarget, restoredResult.CorruptionTarget);
    AssertEqual(originalResult.CorruptionFocusTarget, restoredResult.CorruptionFocusTarget);
    AssertEqual(originalResult.EnemyIntentSummary, restoredResult.EnemyIntentSummary);
    AssertEqual(DescribeBoard(game.Board), DescribeBoard(restored.Board));
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

static void ProceduralMissionKeepsGeneratedNodesReachable()
{
    var screenshotSeedMission = ProceduralMissionGenerator.Generate("codec-milestone-6");
    AssertEqual(screenshotSeedMission.BoardDefinition.Nodes.Count, GetReachableNodes(screenshotSeedMission.BoardDefinition, screenshotSeedMission.BoardDefinition.PlayerStart).Count);

    var settings = ProceduralMissionSettings.Default with
    {
        NodeCount = 24,
        ObjectiveDistance = 6,
        MaxBranchingFactor = 2,
        GraphDensity = 0.08d
    };

    for (var seed = 0; seed < 40; seed++)
    {
        var mission = ProceduralMissionGenerator.Generate(seed, settings);
        AssertEqual(mission.BoardDefinition.Nodes.Count, GetReachableNodes(mission.BoardDefinition, mission.BoardDefinition.PlayerStart).Count);
    }
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

static void ProceduralMissionsExposeLayerMetadata()
{
    var mission = ProceduralMissionGenerator.Generate("layer-metadata-check");
    var board = mission.BoardDefinition;

    AssertTrue(board.LayerCount >= 4, "Expected generated missions to expose multiple topology layers.");
    AssertEqual(board.Nodes.Count, board.NodeLayers.Count);
    AssertEqual(0, board.GetLayer(board.PlayerStart));
    AssertEqual(board.LayerCount - 1, board.GetLayer(mission.ObjectiveNode));
    AssertTrue(board.TransitionLinks.Count >= board.LayerCount - 1, "Expected generated missions to expose transition links between layers.");
    AssertEqual(board.LayerCount.ToString(), board.Metadata["layerCount"]);
    AssertEqual(board.TransitionLinks.Count.ToString(), board.Metadata["transitionCount"]);
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

static void ScenarioCatalogProvidesBalancedRegressionScenarios()
{
    var scenarios = ScenarioCatalog.CreateBalancedRegressionScenarios();

    AssertTrue(scenarios.Count >= 3, "Expected at least three curated regression scenarios.");
    foreach (var mission in scenarios)
    {
        var board = mission.BoardDefinition;
        AssertEqual(board.Nodes.Count, GetReachableNodes(board, board.PlayerStart).Count);
        AssertTrue(GetShortestPathLength(board, board.PlayerStart, mission.ObjectiveNode) >= 3, $"Expected {mission.Name} objective to require a real route.");
        AssertTrue(board.ResourceNodes.Count >= 1, $"Expected {mission.Name} to include at least one Resource.");
        AssertTrue(board.RelayNodes.Count >= 1, $"Expected {mission.Name} to include at least one Relay.");
        AssertTrue(board.FirewallNodes.Count >= 1, $"Expected {mission.Name} to include at least one Firewall.");

        var game = NetworkGame.CreateMission(mission, new GameConfiguration { EnemyPersonality = EnemyPersonality.Opportunistic, EnemyDifficulty = EnemyDifficulty.Hard });
        var report = BalanceScreening.Run(game, maxActions: 14);
        AssertFalse(report.Result == GameResult.PlayerLoss, $"Expected {mission.Name} to avoid forced loss under the screening route. Trace: {report.Trace}");
        AssertTrue(report.PlayerOwnedNodes >= 4, $"Expected {mission.Name} screening route to establish a real foothold. Trace: {report.Trace}");
    }
}

static void GeneratedMissionCorpusPassesBalanceScreening()
{
    var seeds = new[]
    {
        "balance-entry-01",
        "balance-entry-02",
        "balance-entry-03",
        "balance-wide-01",
        "balance-branch-01",
        "balance-pressure-01"
    };

    var settings = ProceduralMissionSettings.Default with
    {
        NodeCount = 20,
        ObjectiveDistance = 5,
        GraphDensity = 0.24d,
        StartingPlayerEnergy = 7
    };

    foreach (var seed in seeds)
    {
        var mission = ProceduralMissionGenerator.Generate(seed, settings);
        var game = NetworkGame.CreateMission(mission, new GameConfiguration
        {
            EnemyPersonality = CampaignProgressionPlanner.SelectPersonality(ProceduralSeed.FromText(seed)),
            EnemyDifficulty = EnemyDifficulty.Hard
        });
        var report = BalanceScreening.Run(game, maxActions: 10);

        AssertFalse(report.Result == GameResult.PlayerLoss && report.ActionsTaken < 6, $"Expected generated seed {seed} to avoid immediate forced loss. Trace: {report.Trace}");
        AssertTrue(report.MaxPlayerOwnedNodes >= 3, $"Expected generated seed {seed} to allow early expansion. Trace: {report.Trace}");
        AssertTrue(report.BestObjectiveDistance <= settings.ObjectiveDistance, $"Expected generated seed {seed} to preserve or improve objective distance. Trace: {report.Trace}");
    }
}

static void BalanceScreeningCoversPersonalityPressureMatrix()
{
    var seeds = new[]
    {
        "matrix-entry-01",
        "matrix-entry-02",
        "matrix-wide-01",
        "matrix-branch-01",
        "matrix-deep-01",
        "matrix-pressure-01"
    };
    var personalities = new[]
    {
        EnemyPersonality.Aggressive,
        EnemyPersonality.Defensive,
        EnemyPersonality.Economic,
        EnemyPersonality.Opportunistic,
        EnemyPersonality.CorruptionFocused
    };
    var settings = ProceduralMissionSettings.Default with
    {
        NodeCount = 22,
        ObjectiveDistance = 6,
        GraphDensity = 0.28d,
        StartingPlayerEnergy = 8
    };

    var forcedEarlyLosses = 0;
    var stalledOpenings = 0;
    var earlyLossDetails = new List<string>();
    foreach (var seed in seeds)
    {
        foreach (var personality in personalities)
        {
            var mission = ProceduralMissionGenerator.Generate(seed, settings);
            var game = NetworkGame.CreateMission(mission, new GameConfiguration
            {
                EnemyPersonality = personality,
                EnemyDifficulty = EnemyDifficulty.Hard
            });
            var report = BalanceScreening.Run(game, maxActions: 12);

            if (report.Result == GameResult.PlayerLoss && report.ActionsTaken < 5)
            {
                forcedEarlyLosses++;
                earlyLossDetails.Add($"{seed}/{personality}: {report.Trace}");
            }

            if (report.MaxPlayerOwnedNodes < 3 || report.BestObjectiveDistance > settings.ObjectiveDistance)
            {
                stalledOpenings++;
            }
        }
    }

    AssertTrue(forcedEarlyLosses == 0, $"Expected no forced early losses. {string.Join(" || ", earlyLossDetails)}");
    AssertTrue(stalledOpenings <= 2, $"Expected matrix screening to leave only a small number of stalled openings, got {stalledOpenings}.");
}

static void TacticalAiProfilesStayLegalAcrossSeedCorpus()
{
    var seeds = new[] { "ai-corpus-01", "ai-corpus-02", "ai-corpus-03", "ai-corpus-04" };
    var personalities = new[]
    {
        EnemyPersonality.Aggressive,
        EnemyPersonality.Defensive,
        EnemyPersonality.Economic,
        EnemyPersonality.Opportunistic,
        EnemyPersonality.CorruptionFocused
    };

    foreach (var seed in seeds)
    {
        foreach (var personality in personalities)
        {
            var game = NetworkGame.CreateMission(ProceduralMissionGenerator.Generate(seed), new GameConfiguration
            {
                EnemyPersonality = personality,
                EnemyDifficulty = EnemyDifficulty.Expert
            });

            for (var turn = 0; turn < 3 && game.Result == GameResult.InProgress; turn++)
            {
                var result = game.EndPlayerTurnWithResult();
                var target = result.CorruptionTarget ?? result.CorruptionFocusTarget;
                AssertTrue(target.HasValue, $"Expected {personality} on {seed} to choose a target on turn {turn + 1}.");
                AssertTrue(result.EnemyActionSource.HasValue, $"Expected {personality} on {seed} to report an action source.");
                var source = result.EnemyActionSource.GetValueOrDefault();
                var targetId = target.GetValueOrDefault();
                AssertTrue(game.Board.AreConnected(source, targetId), $"Expected {personality} on {seed} to use an active link.");
                AssertEqual(NodeOwner.Enemy, game.Board.GetNode(source).Owner);
                AssertFalse(string.IsNullOrWhiteSpace(result.EnemyIntentSummary), $"Expected {personality} on {seed} to report intent.");
            }
        }
    }
}

static void CampaignProgressionStartsFromDeterministicOpeningTrace()
{
    var first = CampaignProgressionPlanner.CreateInitialPlan();
    var second = CampaignProgressionPlanner.CreatePlan(Array.Empty<CampaignMissionRecord>());

    AssertEqual(1, first.Stage);
    AssertEqual(first.Seed, second.Seed);
    AssertEqual("campaign-01-entry-001", first.Seed.Text);
    AssertEqual(EnemyDifficulty.Standard, first.Configuration.EnemyDifficulty);
    AssertEqual("signal-recovery", first.ArcId);
    AssertEqual("Signal Recovery", first.ArcTitle);
    AssertEqual("Wake the Lattice", first.MissionTitle);
    AssertEqual("Opening Route", first.BranchLabel);
    AssertContains("First contact", first.BranchSummary);
    AssertContains("Stage 1", first.Briefing);
    AssertContains("Wake the Lattice", first.Briefing);
}

static void CampaignProgressionAdvancesAfterWins()
{
    var history = new[]
    {
        new CampaignMissionRecord(ProceduralSeed.FromText("campaign-01-entry-001"), GameResult.PlayerWin, 6, EnemyPersonality.Opportunistic),
        new CampaignMissionRecord(ProceduralSeed.FromText("campaign-02-advance-002"), GameResult.PlayerWin, 8, EnemyPersonality.Defensive)
    };

    var plan = CampaignProgressionPlanner.CreatePlan(history);

    AssertEqual(3, plan.Stage);
    AssertEqual("campaign-03-advance-003", plan.Seed.Text);
    AssertEqual("Gate the Firewall", plan.MissionTitle);
    AssertEqual("Advance Route", plan.BranchLabel);
    AssertContains("Advancing", plan.BranchSummary);
    AssertTrue(plan.MissionSettings.NodeCount > ProceduralMissionSettings.Default.NodeCount, "Expected later campaign stages to grow the generated network.");
    AssertEqual(EnemyDifficulty.Hard, plan.Configuration.EnemyDifficulty);
    AssertTrue(plan.Configuration.LayerModifiers.Count > 0, "Expected campaign stages to opt into layer-specific tuning.");
}

static void CampaignProgressionRecoverySoftensAfterLoss()
{
    var history = new[]
    {
        new CampaignMissionRecord(ProceduralSeed.FromText("campaign-01-entry-001"), GameResult.PlayerLoss, 5, EnemyPersonality.Aggressive)
    };

    var plan = CampaignProgressionPlanner.CreatePlan(history);

    AssertEqual(1, plan.Stage);
    AssertEqual("campaign-01-recover-002", plan.Seed.Text);
    AssertTrue(plan.MissionSettings.StartingPlayerEnergy > ProceduralMissionSettings.Default.StartingPlayerEnergy, "Expected recovery missions to grant a small energy cushion.");
    AssertEqual(1, plan.Configuration.CorruptionPressureGrowthPerTurn);
    AssertEqual("Recovery Route", plan.BranchLabel);
    AssertContains("charge cushion", plan.BranchSummary);
    AssertContains("recovery", plan.Briefing);
    AssertContains("Re-enter the damaged mesh", plan.Briefing);
}

static void CampaignProgressionUsesAuthoredArcMissions()
{
    AssertEqual("Signal Recovery", CampaignArcCatalog.Default.Title);
    AssertEqual(8, CampaignArcCatalog.Default.Missions.Count);

    var lateHistory = Enumerable.Range(1, 7)
        .Select(stage => new CampaignMissionRecord(
            ProceduralSeed.FromText($"campaign-{stage:00}-advance-{stage:000}"),
            GameResult.PlayerWin,
            6 + stage,
            EnemyPersonality.Opportunistic))
        .ToArray();

    var plan = CampaignProgressionPlanner.CreatePlan(lateHistory);

    AssertEqual(8, plan.Stage);
    AssertEqual("Seal the Kernel", plan.MissionTitle);
    AssertContains("maximum corruption pressure", plan.Briefing);
    AssertEqual("signal-recovery", plan.ArcId);
}

static void ProfileProgressionSummarizesCompletedMissions()
{
    var history = new[]
    {
        new CampaignMissionRecord(ProceduralSeed.FromText("campaign-01-entry-001"), GameResult.PlayerWin, 6, EnemyPersonality.Opportunistic, Stage: 1),
        new CampaignMissionRecord(ProceduralSeed.FromText("campaign-02-advance-002"), GameResult.PlayerLoss, 9, EnemyPersonality.Defensive, Stage: 2),
        new CampaignMissionRecord(ProceduralSeed.FromText("campaign-02-recover-003"), GameResult.PlayerWin, 7, EnemyPersonality.Economic, Stage: 2),
        new CampaignMissionRecord(ProceduralSeed.FromText("campaign-03-advance-004"), GameResult.PlayerWin, 8, EnemyPersonality.Aggressive, Stage: 3),
        new CampaignMissionRecord(ProceduralSeed.FromText("campaign-04-advance-005"), GameResult.PlayerWin, 9, EnemyPersonality.CorruptionFocused, Stage: 4)
    };

    var progress = ProfileProgression.Calculate(history);

    AssertEqual(5, progress.Level);
    AssertEqual(3, progress.CurrentWinStreak);
    AssertEqual(4, progress.BestStage);
    AssertEqual(4, progress.Wins);
    AssertEqual(1, progress.Losses);
    AssertEqual("Signal Climber", progress.Title);
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

    var decision = TacticalEnemyPlanner.SelectDecision(game.BoardDefinition, game.Board, game.Configuration, game.PlayerCore, game.ObjectiveNode, corruptionPressure: 1, turnNumber: 1);

    AssertEqual(game.ObjectiveNode, decision.Target);
    AssertContains("objective", decision.PrimaryFactor);
}

static void EnemyPersonalitiesChooseDifferentTargets()
{
    var mission = CreatePersonalityTestMission();
    var aggressive = NetworkGame.CreateMission(mission, new GameConfiguration { EnemyPersonality = EnemyPersonality.Aggressive, EnemyDifficulty = EnemyDifficulty.Expert });
    var defensive = NetworkGame.CreateMission(mission, new GameConfiguration { EnemyPersonality = EnemyPersonality.Defensive, EnemyDifficulty = EnemyDifficulty.Expert });
    var economic = NetworkGame.CreateMission(mission, new GameConfiguration { EnemyPersonality = EnemyPersonality.Economic, EnemyDifficulty = EnemyDifficulty.Expert });

    var aggressiveTarget = TacticalEnemyPlanner.SelectDecision(aggressive.BoardDefinition, aggressive.Board, aggressive.Configuration, aggressive.PlayerCore, aggressive.ObjectiveNode, 1, 1).Target;
    var defensiveTarget = TacticalEnemyPlanner.SelectDecision(defensive.BoardDefinition, defensive.Board, defensive.Configuration, defensive.PlayerCore, defensive.ObjectiveNode, 1, 1).Target;
    var economicTarget = TacticalEnemyPlanner.SelectDecision(economic.BoardDefinition, economic.Board, economic.Configuration, economic.PlayerCore, economic.ObjectiveNode, 1, 1).Target;

    AssertEqual(new NodeId(1, 1), aggressiveTarget);
    AssertEqual(new NodeId(3, 1), defensiveTarget);
    AssertEqual(new NodeId(2, 0), economicTarget);
}

static void DifficultyChangesDecisionQualityWithoutBonuses()
{
    var mission = CreatePersonalityTestMission();
    var easy = NetworkGame.CreateMission(mission, new GameConfiguration { EnemyPersonality = EnemyPersonality.Opportunistic, EnemyDifficulty = EnemyDifficulty.Easy });
    var expert = NetworkGame.CreateMission(mission, new GameConfiguration { EnemyPersonality = EnemyPersonality.Opportunistic, EnemyDifficulty = EnemyDifficulty.Expert });

    var easyDecision = TacticalEnemyPlanner.SelectDecision(easy.BoardDefinition, easy.Board, easy.Configuration, easy.PlayerCore, easy.ObjectiveNode, 1, 1);
    var expertDecision = TacticalEnemyPlanner.SelectDecision(expert.BoardDefinition, expert.Board, expert.Configuration, expert.PlayerCore, expert.ObjectiveNode, 1, 1);

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
    var nodes = string.Join("|", board.Nodes
        .OrderBy(node => node.Id)
        .Select(node => $"{node.Id}:{node.Owner}:{node.Type}:{node.Integrity}:{node.ReinforcementLevel}:{node.Threat}:{node.UnstableTurns}:{node.DangerReason}"));
    var connections = string.Join("|", board.Connections
        .OrderBy(connection => connection.First)
        .ThenBy(connection => connection.Second)
        .Select(connection => $"{connection.First}-{connection.Second}:{connection.Strength}"));
    return $"{nodes}||{connections}";
}

static string DescribeDefinition(BoardDefinition definition)
{
    var nodes = string.Join(",", definition.Nodes);
    var links = string.Join(",", definition.Links.Select(link => $"{link.First}-{link.Second}"));
    var types = string.Join(",", definition.NodeTypes.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key}:{pair.Value}"));
    var owners = string.Join(",", definition.InitialOwnership.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key}:{pair.Value}"));
    var layout = string.Join(",", definition.Layout.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key}:{pair.Value.X:0.00}:{pair.Value.Y:0.00}"));
    var layers = string.Join(",", definition.NodeLayers.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key}:{pair.Value}"));
    return $"{nodes}|{links}|{types}|{owners}|{definition.PlayerStart}|{string.Join(",", definition.CorruptionStarts)}|{layout}|{layers}";
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

enum ScreeningActionMode
{
    Claim = PlayerActionMode.Claim,
    Reinforce = PlayerActionMode.Reinforce,
    Weaken = PlayerActionMode.Weaken,
    EndTurn
}

readonly record struct ScreeningAction(ScreeningActionMode Mode, NodeId? Target);

readonly record struct BalanceScreenReport(GameResult Result, int ActionsTaken, int TurnNumber, int PlayerOwnedNodes, int MaxPlayerOwnedNodes, int ObjectiveDistanceRemaining, int BestObjectiveDistance, string Trace);
