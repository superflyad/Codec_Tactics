namespace CodecTactics.Core.Network;

public static class BalanceScreening
{
    public static BalanceScreenReport Run(NetworkGame game, int maxActions)
    {
        var trace = new List<string>();
        var maxPlayerOwned = game.Board.Nodes.Count(node => node.Owner == NodeOwner.Player);
        var bestObjectiveDistance = GetBestObjectiveDistance(game);

        for (var actionIndex = 0; actionIndex < maxActions && game.Result == GameResult.InProgress; actionIndex++)
        {
            var action = SelectAction(game);
            var result = action.Mode == BalanceScreenActionMode.EndTurn
                ? game.EndPlayerTurnWithResult()
                : game.ExecutePlayerAction((PlayerActionMode)action.Mode, action.Target!.Value);
            trace.Add($"{action.Mode}:{action.Target?.ToString() ?? "-"}:{result.Result}");
            maxPlayerOwned = Math.Max(maxPlayerOwned, game.Board.Nodes.Count(node => node.Owner == NodeOwner.Player));
            bestObjectiveDistance = Math.Min(bestObjectiveDistance, GetBestObjectiveDistance(game));

            if (!result.Succeeded && action.Mode != BalanceScreenActionMode.EndTurn)
            {
                result = game.EndPlayerTurnWithResult();
                trace.Add($"EndTurn:-:{result.Result}");
                maxPlayerOwned = Math.Max(maxPlayerOwned, game.Board.Nodes.Count(node => node.Owner == NodeOwner.Player));
                bestObjectiveDistance = Math.Min(bestObjectiveDistance, GetBestObjectiveDistance(game));
            }
        }

        return new BalanceScreenReport(
            game.Result,
            trace.Count,
            game.TurnNumber,
            game.Board.Nodes.Count(node => node.Owner == NodeOwner.Player),
            maxPlayerOwned,
            GetBestObjectiveDistance(game),
            bestObjectiveDistance,
            string.Join(" | ", trace));
    }

    private static BalanceScreenAction SelectAction(NetworkGame game)
    {
        if (game.PlayerEnergy >= game.Configuration.ClaimEnergyCost && game.ObjectiveNode.HasValue)
        {
            var objective = game.Board.GetNode(game.ObjectiveNode.Value);
            if (objective.Owner == NodeOwner.Neutral && game.Board.IsReachableForPlayerClaim(objective.Id, game.Configuration))
            {
                return new BalanceScreenAction(BalanceScreenActionMode.Claim, objective.Id);
            }
        }

        if (game.PlayerEnergy >= game.Configuration.ReinforceEnergyCost)
        {
            var unstable = game.Board.Nodes
                .Where(node => node.Owner == NodeOwner.Player && node.IsUnstable)
                .Where(node => node.Id == game.PlayerCore || node.UnstableTurns >= game.Configuration.InstabilityTurnsBeforeCollapse - 1)
                .OrderByDescending(node => node.Id == game.PlayerCore)
                .ThenByDescending(node => node.UnstableTurns)
                .ThenByDescending(node => node.Threat - node.Integrity)
                .ThenBy(node => GetObjectiveDistance(game, node.Id))
                .FirstOrDefault();
            if (unstable is not null)
            {
                return new BalanceScreenAction(BalanceScreenActionMode.Reinforce, unstable.Id);
            }
        }

        if (game.PlayerEnergy >= game.Configuration.WeakenConnectionEnergyCost)
        {
            var urgentWeaken = game.Board.Nodes
                .Where(node => node.Owner == NodeOwner.Enemy && game.Board.GetAdjacentNodes(node.Id).Any(adjacent => adjacent.Owner == NodeOwner.Player && adjacent.IsUnstable))
                .OrderBy(node => GetObjectiveDistance(game, node.Id))
                .ThenBy(node => node.Id)
                .FirstOrDefault();
            if (urgentWeaken is not null)
            {
                return new BalanceScreenAction(BalanceScreenActionMode.Weaken, urgentWeaken.Id);
            }
        }

        if (game.PlayerEnergy >= game.Configuration.ClaimEnergyCost)
        {
            var claim = game.Board.Nodes
                .Where(node => node.Owner == NodeOwner.Neutral && game.Board.IsReachableForPlayerClaim(node.Id, game.Configuration))
                .OrderByDescending(node => game.Board.HasAdjacentOwner(node.Id, NodeOwner.Player))
                .ThenByDescending(node => game.ObjectiveNode.HasValue && node.Id.Equals(game.ObjectiveNode.Value))
                .ThenBy(node => GetObjectiveDistance(game, node.Id))
                .ThenBy(node => GetTypePriority(node.Type))
                .ThenBy(node => node.Id)
                .FirstOrDefault();
            if (claim is not null)
            {
                return new BalanceScreenAction(BalanceScreenActionMode.Claim, claim.Id);
            }
        }

        if (game.PlayerEnergy >= game.Configuration.WeakenConnectionEnergyCost)
        {
            var weaken = game.Board.Nodes
                .Where(node => node.Owner == NodeOwner.Enemy && game.Board.GetAdjacentNodes(node.Id).Any(adjacent => adjacent.Owner == NodeOwner.Player))
                .OrderBy(node => GetObjectiveDistance(game, node.Id))
                .ThenBy(node => node.Id)
                .FirstOrDefault();
            if (weaken is not null)
            {
                return new BalanceScreenAction(BalanceScreenActionMode.Weaken, weaken.Id);
            }
        }

        return new BalanceScreenAction(BalanceScreenActionMode.EndTurn, null);
    }

    private static int GetTypePriority(NodeType type)
    {
        return type switch
        {
            NodeType.Resource => 0,
            NodeType.Relay => 1,
            NodeType.Firewall => 2,
            _ => 3
        };
    }

    private static int GetObjectiveDistance(NetworkGame game, NodeId node)
    {
        return game.ObjectiveNode.HasValue
            ? GetShortestPathLength(game.Board, node, game.ObjectiveNode.Value)
            : 0;
    }

    private static int GetBestObjectiveDistance(NetworkGame game)
    {
        return game.ObjectiveNode.HasValue
            ? game.Board.Nodes
                .Where(node => node.Owner == NodeOwner.Player)
                .Select(node => GetShortestPathLength(game.Board, node.Id, game.ObjectiveNode.Value))
                .Where(distance => distance >= 0)
                .DefaultIfEmpty(int.MaxValue)
                .Min()
            : 0;
    }

    private static int GetShortestPathLength(NetworkBoard board, NodeId start, NodeId target)
    {
        if (start.Equals(target))
        {
            return 0;
        }

        var visited = new HashSet<NodeId> { start };
        var queue = new Queue<(NodeId Id, int Distance)>();
        queue.Enqueue((start, 0));
        while (queue.Count > 0)
        {
            var (current, distance) = queue.Dequeue();
            foreach (var adjacent in board.GetAdjacentNodes(current))
            {
                if (!visited.Add(adjacent.Id))
                {
                    continue;
                }

                if (adjacent.Id.Equals(target))
                {
                    return distance + 1;
                }

                queue.Enqueue((adjacent.Id, distance + 1));
            }
        }

        return -1;
    }
}

public sealed record BalanceScreenReport(
    GameResult Result,
    int ActionsTaken,
    int TurnNumber,
    int PlayerOwnedNodes,
    int MaxPlayerOwnedNodes,
    int ObjectiveDistanceRemaining,
    int BestObjectiveDistance,
    string Trace);

public enum BalanceScreenActionMode
{
    Claim = PlayerActionMode.Claim,
    Reinforce = PlayerActionMode.Reinforce,
    Weaken = PlayerActionMode.Weaken,
    EndTurn = 100
}

internal readonly record struct BalanceScreenAction(BalanceScreenActionMode Mode, NodeId? Target);
