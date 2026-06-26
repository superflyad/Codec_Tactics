# Board Definition and Game Configuration

Milestone 3.25 moves board setup and rule values into explicit core models. This keeps the current 4x4 prototype behavior intact while removing assumptions that would block later board sizes, layers, or cube faces.

All models live in `CodecTactics.Core`.

## BoardDefinition

`BoardDefinition` describes how a game board should be initialized.

Current fields:

- `Width` and `Height`: rectangular board dimensions.
- `Nodes`: deterministic node layout in row-major order.
- `NodeTypes`: authored node type placement for Standard, Resource, Relay, and Firewall nodes.
- `InitialOwnership`: initial Neutral, Player, or Enemy ownership assignments.
- `PlayerStart`: the player core used by integrity distance checks.
- `CorruptionStarts`: one or more initial corruption nodes.
- `StartingPlayerEnergy`: optional scenario-specific starting energy.
- `Metadata`: string metadata for future scenario/topology labels.

The existing prototype is now `BoardDefinition.CreateDefaultPrototype()`. It is still a 4x4 single-layer grid with:

- Player start: `(0,0)`
- Corruption start: `(3,3)`
- Resource nodes: `(1,0)`, `(2,1)`
- Relay nodes: `(0,1)`, `(1,2)`
- Firewall node: `(2,3)`

## GameConfiguration

`GameConfiguration` contains tunable rule values. Its defaults mirror `NetworkRules`, so default gameplay remains unchanged.

Configurable groups:

- Costs: initial energy, claim, reinforce, weaken, and Resource income.
- Corruption growth: pressure gained per enemy turn and corruption resistance.
- Integrity: base integrity, core connection, isolation, Relay support, Firewall support, adjacent support, dense support, and long-chain penalty.
- Threat: adjacent corruption, corruption pressure divisor, weak ownership support, frontier exposure, and isolation threat.
- Collapse timing: unstable turns before collapse.
- Targeting priority: unstable target priority, low-integrity priority anchor, and Firewall target penalty.

`NetworkRules` remains as the named default value source. New scenarios should pass a `GameConfiguration` only when they need an explicit ruleset.

## Determinism

Board initialization remains deterministic:

- Nodes are ordered by `NodeId`.
- Connections are generated from the board dimensions in a fixed horizontal-then-vertical pattern.
- Corruption starts are ordered by `NodeId`.
- Corruption targeting still uses deterministic priority and row-major tie breaks.

## Future Layers

`BoardDefinition.Metadata` is the placeholder for future layer labels and scenario tags. Milestone 3.25 does not add layers, but it gives future layer work a place to attach metadata without changing gameplay rules.

Before layers are implemented, the core still needs a topology model that can describe cross-layer links and define how distance, Relay reach, and corruption pressure move between layers.

## Future Cube Faces

Cube faces will need a node identity or topology layer beyond plain `(X,Y)` grid coordinates. Milestone 3.25 does not add cube rendering or cube adjacency, but it separates scenario definition from game flow so a future cube-face board can be introduced as data before presentation changes.

Before cube faces are implemented, the engine still needs explicit face IDs, stable multi-face ordering, and tests for cross-face adjacency.
