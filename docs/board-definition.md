# Board Definition and Game Configuration

Milestone 3.25 moves board setup and rule values into explicit core models. This keeps the current 4x4 prototype behavior intact while removing assumptions that would block later board sizes, layers, or cube faces.

All models live in `CodecTactics.Core`.

## BoardDefinition

`BoardDefinition` describes how a game board should be initialized.

Current fields:

- `Width` and `Height`: rectangular board dimensions.
- `Nodes`: deterministic node layout in row-major order.
- `Links`: explicit graph links between nodes.
- `Layout`: optional visual positions for renderers.
- `NodeLayers`: explicit layer assignment for each node.
- `TransitionLinks`: links whose endpoints belong to different layers.
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
- Connections are stored as explicit `NetworkLink` values. Grid boards still generate links from dimensions in a fixed horizontal-then-vertical pattern.
- Corruption starts are ordered by `NodeId`.
- Corruption targeting still uses deterministic priority and row-major tie breaks.

## Procedural Topology

Milestone 6 adds procedural topology without changing combat, corruption, energy, or objective rules.

`BoardDefinition.CreateTopology()` accepts explicit nodes, links, ownership, node types, and layout positions. `ProceduralMissionGenerator` uses that path to create connected layered infrastructure graphs with deterministic seed metadata:

- `seed`
- `seedText`
- `nodeCount`
- `edgeCount`
- `layerCount`
- `transitionCount`
- `scenario=procedural-network`
- `topology=layered-infrastructure-graph`

Renderers should use `BoardDefinition.Layout` when present. Gameplay systems should use `BoardDefinition.Links` and must not infer rules from visual positions.

## Layers

`BoardDefinition.NodeLayers` assigns every node to a deterministic integer layer. Grid boards default to layer `0`. Procedural missions assign layer indices from their generated depth structure, so the player start is layer `0`, the objective sits on the deepest generated layer, and transition links identify routes between layer depths.

Layer assignments are topology facts first. Distance, Relay reach, corruption pressure, and objective logic still use explicit active graph links. `GameConfiguration.LayerModifiers` can add opt-in per-layer integrity, threat, and corruption resistance modifiers, which lets generated campaign stages tune deeper layers without changing topology or adding traversal actions. The MonoGame frontend can focus one layer at a time and draw a cube-inspired layer inset from the same topology data.

## Future Cube Faces

Cube faces will need a face model beyond layer indices. The engine now has explicit layer topology and transition links, and the MonoGame frontend has a presentation-only layer cube inset. The core does not yet add cube adjacency, cube-face ordering, or cube-face gameplay rules.

Before cube faces are implemented, the engine still needs explicit face IDs, stable multi-face ordering, and tests for cross-face adjacency.
