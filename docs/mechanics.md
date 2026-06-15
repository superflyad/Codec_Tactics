# Mechanics

This document describes the implemented 2D prototype rules. These rules live in `CodecTactics.Core`; Godot only displays state and sends player actions.

## Node Types

- Standard: basic node with normal claim and reinforcement behavior.
- Resource: when player-owned, generates energy at the start of each new player turn.
- Relay: when player-owned, extends player claim reach to neutral nodes within two active connections.
- Firewall: resists corruption. Neutral Firewall nodes require two corruption pressure instead of one before corruption can claim them.

The default 4x4 board uses a small fixed mix of node types:

- Resource: `(1,0)` and `(2,1)`
- Relay: `(0,1)` and `(1,2)`
- Firewall: `(2,3)`
- All other nodes are Standard

## Energy

Player energy is deterministic and configured in `NetworkRules`.

- Initial energy: 5
- Claim cost: 2
- Reinforce cost: 1
- Weaken corruption connection cost: 1
- Player-owned Resource node income: 2 energy per new player turn

If the player does not have enough energy, the action fails, the board does not mutate, corruption does not act, pressure does not change, and the turn number does not advance.

## Claim Reach

A neutral node can be claimed when it is reachable from the player network.

- Standard reach: the target is adjacent to any player-owned node through an active connection.
- Relay reach: the target is within two active connections of any player-owned Relay node.

Relay reach does not ignore inactive connections and does not claim through ownership by itself. It only extends the list of valid neutral claim targets.

## Turn Flow

Each successful player action follows this sequence:

1. Spend the action cost, if any.
2. Apply the player action.
3. Resolve the enemy/corruption turn.
4. Evaluate placeholder outcome rules.
5. If the game is still in progress, advance the turn number.
6. Generate energy from player-owned Resource nodes.
7. Return to the player phase.

Ending the turn is a real core action with no energy cost. It skips player mutation but still resolves corruption pressure and expansion.

## Corruption Pressure

Corruption pressure starts at zero. Each enemy/corruption turn adds one pressure, then corruption checks for one expansion target.

Expansion target selection is deterministic:

1. Look at enemy-owned nodes in row-major order.
2. For each enemy node, look at active adjacent neutral nodes in row-major order.
3. Deduplicate candidates and pick the first row-major target.

If the current pressure is at least the target node's resistance, corruption claims that node and spends pressure equal to the resistance. Standard, Resource, and Relay nodes have resistance 1. Firewall nodes have resistance 2.

If the current pressure is lower than the target resistance, corruption does not spread that turn and pressure carries forward.
