namespace CodecTactics.Core.Network;

public static class TacticalEnemyPlanner
{
    public static TacticalEnemyDecision SelectDecision(
        NetworkBoard board,
        GameConfiguration configuration,
        NodeId playerCore,
        NodeId? objectiveNode,
        int corruptionPressure,
        int turnNumber)
    {
        var profile = TacticalEnemyProfile.Create(configuration.EnemyPersonality);
        var candidates = EvaluateCandidates(board, configuration, profile, playerCore, objectiveNode, corruptionPressure)
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Target)
            .ToList();

        if (candidates.Count == 0)
        {
            return TacticalEnemyDecision.None;
        }

        var index = SelectCandidateIndex(candidates, configuration.EnemyDifficulty, turnNumber);
        return candidates[index];
    }

    public static IReadOnlyList<TacticalEnemyDecision> EvaluateCandidates(
        NetworkBoard board,
        GameConfiguration configuration,
        TacticalEnemyProfile profile,
        NodeId playerCore,
        NodeId? objectiveNode,
        int corruptionPressure)
    {
        var candidates = new List<TacticalEnemyDecision>();
        var enemyNodes = board.Nodes
            .Where(node => node.Owner == NodeOwner.Enemy)
            .OrderBy(node => node.Id)
            .ToList();

        foreach (var enemyNode in enemyNodes)
        {
            foreach (var target in board.GetAdjacentNodes(enemyNode.Id).Where(node => node.Owner != NodeOwner.Enemy))
            {
                if (candidates.Any(candidate => candidate.Target == target.Id))
                {
                    continue;
                }

                candidates.Add(ScoreCandidate(board, configuration, profile, playerCore, objectiveNode, corruptionPressure, enemyNode.Id, target));
            }
        }

        return candidates
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Target)
            .ToList();
    }

    private static TacticalEnemyDecision ScoreCandidate(
        NetworkBoard board,
        GameConfiguration configuration,
        TacticalEnemyProfile profile,
        NodeId playerCore,
        NodeId? objectiveNode,
        int corruptionPressure,
        NodeId source,
        NodeState target)
    {
        var resistance = GetCorruptionResistance(configuration, target);
        var actionType = target.Owner == NodeOwner.Neutral && corruptionPressure >= resistance
            ? TacticalEnemyActionType.CorruptNode
            : TacticalEnemyActionType.FocusPressure;
        var adjacent = board.GetAdjacentNodes(target.Id);
        var distanceToCore = GetShortestActiveDistance(board, target.Id, playerCore);
        var distanceToObjective = objectiveNode.HasValue
            ? GetShortestActiveDistance(board, target.Id, objectiveNode.Value)
            : int.MaxValue;

        var factors = new Dictionary<string, double>
        {
            ["objective proximity"] = objectiveNode.HasValue ? NormalizeDistance(distanceToObjective, board.Nodes.Count) * 12d : 0d,
            ["relay value"] = target.Type == NodeType.Relay ? 10d : 0d,
            ["resource value"] = target.Type == NodeType.Resource ? 10d : 0d,
            ["network control"] = adjacent.Count(node => node.Owner != NodeOwner.Enemy) * 2d + adjacent.Count(node => node.Owner == NodeOwner.Player) * 3d,
            ["corruption opportunities"] = GetCorruptionOpportunity(target, configuration),
            ["player expansion"] = target.Owner == NodeOwner.Player ? 12d : adjacent.Count(node => node.Owner == NodeOwner.Player) * 4d,
            ["defensive value"] = GetDefensiveValue(target, adjacent),
            ["reachable territory"] = adjacent.Count(node => node.Owner == NodeOwner.Neutral) * 3d,
            ["energy efficiency"] = Math.Clamp(corruptionPressure / (double)Math.Max(1, resistance), 0d, 2d) * 6d,
            ["future positioning"] = NormalizeDistance(distanceToCore, board.Nodes.Count) * 8d + Math.Min(4d, adjacent.Count * 1.25d)
        };

        var score =
            factors["objective proximity"] * profile.ObjectiveProximity +
            factors["relay value"] * profile.RelayValue +
            factors["resource value"] * profile.ResourceValue +
            factors["network control"] * profile.NetworkControl +
            factors["corruption opportunities"] * profile.CorruptionOpportunity +
            factors["player expansion"] * profile.PlayerExpansion +
            factors["defensive value"] * profile.DefensiveValue +
            factors["reachable territory"] * profile.ReachableTerritory +
            factors["energy efficiency"] * profile.EnergyEfficiency +
            factors["future positioning"] * profile.FuturePositioning;

        if (actionType == TacticalEnemyActionType.CorruptNode)
        {
            score += 14d;
        }

        if (target.Type == NodeType.Firewall && profile.Personality == EnemyPersonality.Defensive)
        {
            score += 80d;
        }
        else if (target.Type == NodeType.Firewall)
        {
            score -= configuration.FirewallTargetPriorityPenalty * 3d;
        }

        if (target.Id == objectiveNode)
        {
            score += 8d * profile.ObjectiveProximity;
        }

        var primaryFactor = factors
            .OrderByDescending(factor => factor.Value * GetWeight(profile, factor.Key))
            .ThenBy(factor => factor.Key)
            .First()
            .Key;
        var verb = actionType == TacticalEnemyActionType.CorruptNode ? "capture" : "pressure";
        var summary = $"{profile.Personality} AI will {verb} {target.Id} to exploit {primaryFactor}.";

        return new TacticalEnemyDecision(actionType, target.Id, source, score, primaryFactor, summary, factors);
    }

    private static int SelectCandidateIndex(IReadOnlyList<TacticalEnemyDecision> candidates, EnemyDifficulty difficulty, int turnNumber)
    {
        if (difficulty is EnemyDifficulty.Hard or EnemyDifficulty.Expert || candidates.Count == 1)
        {
            return 0;
        }

        var maxRank = difficulty == EnemyDifficulty.Easy ? Math.Min(2, candidates.Count - 1) : Math.Min(1, candidates.Count - 1);
        if (maxRank == 0)
        {
            return 0;
        }

        var bestScore = candidates[0].Score;
        var viable = candidates
            .Take(maxRank + 1)
            .TakeWhile(candidate => bestScore - candidate.Score <= (difficulty == EnemyDifficulty.Easy ? 18d : 10d))
            .ToList();

        return viable.Count <= 1 ? 0 : Math.Abs(turnNumber) % viable.Count;
    }

    private static double GetCorruptionOpportunity(NodeState target, GameConfiguration configuration)
    {
        var score = target.Threat + Math.Max(0, configuration.LowIntegrityTargetPriorityAnchor - target.Integrity) / 2d;
        if (target.IsUnstable)
        {
            score += 20d;
        }

        if (target.Owner == NodeOwner.Player)
        {
            score += 6d;
        }

        return score;
    }

    private static double GetDefensiveValue(NodeState target, IReadOnlyList<NodeState> adjacent)
    {
        var score = adjacent.Count(node => node.Owner == NodeOwner.Enemy) * 4d;
        if (target.Type == NodeType.Firewall)
        {
            score += 9d;
        }

        if (adjacent.Count <= 2)
        {
            score += 5d;
        }

        return score;
    }

    private static int GetCorruptionResistance(GameConfiguration configuration, NodeState node)
    {
        return node.Type == NodeType.Firewall
            ? configuration.FirewallCorruptionResistance
            : configuration.StandardCorruptionResistance;
    }

    private static double NormalizeDistance(int distance, int nodeCount)
    {
        if (distance == int.MaxValue)
        {
            return 0d;
        }

        return Math.Max(0d, nodeCount - distance) / Math.Max(1d, nodeCount);
    }

    private static int GetShortestActiveDistance(NetworkBoard board, NodeId start, NodeId target)
    {
        if (start.Equals(target))
        {
            return 0;
        }

        var distances = new Dictionary<NodeId, int> { [start] = 0 };
        var frontier = new Queue<NodeId>();
        frontier.Enqueue(start);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            foreach (var adjacent in board.GetAdjacentNodes(current))
            {
                if (distances.ContainsKey(adjacent.Id))
                {
                    continue;
                }

                distances[adjacent.Id] = distances[current] + 1;
                if (adjacent.Id.Equals(target))
                {
                    return distances[adjacent.Id];
                }

                frontier.Enqueue(adjacent.Id);
            }
        }

        return int.MaxValue;
    }

    private static double GetWeight(TacticalEnemyProfile profile, string factor)
    {
        return factor switch
        {
            "objective proximity" => profile.ObjectiveProximity,
            "relay value" => profile.RelayValue,
            "resource value" => profile.ResourceValue,
            "network control" => profile.NetworkControl,
            "corruption opportunities" => profile.CorruptionOpportunity,
            "player expansion" => profile.PlayerExpansion,
            "defensive value" => profile.DefensiveValue,
            "reachable territory" => profile.ReachableTerritory,
            "energy efficiency" => profile.EnergyEfficiency,
            "future positioning" => profile.FuturePositioning,
            _ => 1d
        };
    }
}
