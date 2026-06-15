# Network Integrity and Threat

Milestone 3 makes network structure matter. Every player-owned node now has calculated integrity, calculated threat, instability tracking, and a deterministic collapse path.

All rules live in `CodecTactics.Core`.

## Integrity Formula

Integrity is recalculated whenever the board changes and at enemy-turn resolution.

```text
integrity =
  4 base
  + reinforcement level
  + 1 per adjacent player-owned node
  + 3 if connected to the player core through player-owned active links
  - 1 per distance step from the core after the first step
  - 4 if isolated from the player core
  + 2 if this node is a Relay or adjacent to a player-owned Relay
  + 3 if this node is a Firewall or adjacent to a player-owned Firewall
  + 2 if adjacent to at least three player-owned nodes
```

Integrity is clamped to a minimum of 1.

## Threat Formula

Threat is recalculated with the current corruption pressure.

```text
threat =
  4 per adjacent corrupted node
  + 1 per adjacent neutral frontier node
  + corruption pressure / 2, rounded down
  + 4 if isolated from the player core
  + 2 if the node has 0 or 1 adjacent player-owned nodes
  + nearby corruption distance bonus
```

Nearby corruption distance bonus:

```text
distance 1: +3
distance 2: +2
distance 3: +1
distance 4 or more: +0
```

Threat represents visible structural danger, not randomness. The same board state always produces the same threat values.

## Instability and Collapse

A player-owned node is unstable when:

```text
threat > integrity
```

Instability only advances during enemy-turn resolution. If a node remains unstable for 2 enemy turns, it collapses to corruption.

Collapse is deterministic:

1. Recalculate integrity and threat after corruption pressure increases.
2. Increment unstable turns for nodes whose threat exceeds integrity.
3. Reset unstable turns for nodes that recover.
4. Collapse any player-owned node with 2 unstable turns.
5. Recalculate risk after collapse.
6. Resolve corruption expansion.

The player can prevent collapse by restoring the node before the second unstable enemy turn.

## Corruption Targeting

Corruption expansion is deterministic and uses the same target order every run.

Candidates are active neighbors of corrupted nodes that are not already corrupted. Each candidate receives a priority:

```text
priority =
  +100 if player-owned and unstable
  + (20 - integrity), minimum 0
  + threat
  - 4 if Firewall
```

The highest priority target is selected. Ties are resolved by node id in row-major order. Neutral targets are corrupted when corruption pressure is at least the target resistance:

- Standard, Resource, Relay: 1
- Firewall: 2

Stable player-owned nodes are not directly taken by expansion. They must first become unstable and persist long enough to collapse.

## Examples

Core node `(0,0)` at game start:

```text
integrity = 4 base + 3 core connection = 7
threat = 2 frontier edges + 2 weak connections = 4
stable
```

Isolated player node near corruption:

```text
integrity = 4 base - 4 isolation penalty = 1
threat includes isolation, weak connections, frontier exposure, and nearby corruption
unstable when threat exceeds integrity
```

Relay-supported chain:

```text
node integrity gains +2 when it is a Relay or adjacent to a player-owned Relay
relay reach can still claim through active links, but poor topology can remain dangerous
```

Firewall position:

```text
firewall integrity gains +3 when player-owned
neutral firewalls still require 2 corruption pressure to corrupt
```

## Strategy Implications

- Expanding in a single-file chain is fast but dangerous.
- Capturing support nodes matters most when they are connected to the core.
- Relay nodes improve reach and structural support, making them good anchors.
- Firewall nodes are strong defensive pivots, especially near corruption.
- Leaving isolated owned nodes beside corruption creates predictable collapse risk.
- Dense local clusters are safer than long exposed tendrils.
