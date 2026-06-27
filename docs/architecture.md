# Initial Architecture

This document describes the intended architecture. It is a guide for future implementation, not a claim that these systems are complete today.

## Grid and Network Model

The core model should treat the game space as a graph-like network. A 2D grid can be the first representation, but rules should not depend on rendering details.

Expected concepts:

- Stable node identifiers.
- Edges or links between nodes.
- Valid expansion rules.
- Reachability and exposure calculations.
- Deterministic state transitions for tests.

Milestone 1 implemented this as `NetworkBoard`, a fixed-size orthogonal grid graph with stable `NodeId` coordinates and explicit `ConnectionState` links. Milestone 6 keeps the same gameplay graph model but moves board definitions to explicit `NetworkLink` topology data so generated missions are no longer required to be full rectangular grids.

Procedural generation is separated from gameplay:

- `ProceduralMissionGenerator` creates a deterministic mission definition from a seed and `ProceduralMissionSettings`.
- `ProceduralNetworkLayout` calculates visual positions for the generated graph.
- `NetworkGame` consumes a `MissionDefinition` and remains responsible for turn flow, action validation, corruption, and objective evaluation.
- MonoGame renders the current board and uses `BoardDefinition.Layout` when available.

## Node Model

Nodes are the primary tactical objects.

Potential node state:

- Ownership or control state.
- Corruption state.
- Layer coordinate.
- Network links.
- Visibility or discovered status.
- Future resource or defense properties.

Milestone 3 node state includes ownership, calculated integrity, threat, and instability so network structure can drive deterministic collapse behavior.

## Layer Model

Layers represent depth and complexity. A layer should be addressable independently while still allowing transitions to adjacent layers.

Expected responsibilities:

- Define available nodes.
- Define layer-specific risk or rule modifiers.
- Provide entry and exit points.
- Support progression into deeper systems.

## Cube Model

The cube model is a future visualization and spatial organization goal. It may map layers onto cube faces, cube slices, or a navigable 3D network volume.

The cube should not own game rules directly. It should present and navigate the rule state produced by the core model.

## Turn System

The turn system should be explicit and deterministic.

Expected phases:

- Player planning.
- Player expansion or action resolution.
- Enemy/corruption access evaluation.
- Enemy/corruption spread.
- Cleanup and win/loss checks.

Milestone 1 uses a compact `NetworkGame` flow: player action, enemy expansion, outcome evaluation, then turn increment back to the player when the game remains in progress.

## Enemy and Corruption System

Corruption should spread through access created by the network. It should punish careless expansion without making the player feel that outcomes are arbitrary.

Implementation goals:

- Deterministic spread for automated tests.
- Clear source and path of corruption.
- Visible risk before player commitment where possible.
- Rules that can grow from simple 2D prototypes to layered spaces.

Milestone 7 replaces the fixed corruption target priority with a tactical enemy planner in `CodecTactics.Core`. The planner evaluates legal adjacent pressure targets every enemy turn, scores them through a `TacticalEnemyProfile`, and returns a `TacticalEnemyDecision` for `NetworkGame` to execute through the existing corruption pressure and collapse rules. Personality configuration, action scoring, selection difficulty, and execution are intentionally separate so future personalities can be added without changing player actions or presentation code.

Difficulty changes decision quality by selecting from evaluated candidates. It does not change corruption pressure growth, resistance values, player energy, or map knowledge.

## Future Visualization Goals

- Start with readable 2D prototype scenes.
- Separate core state from frontend rendering.
- Add clear node selection and path feedback.
- Explore cube-based navigation after rules are testable.
- Keep debug overlays available for topology, layer, and corruption state.

## Frontend Workflow

`src/CodecTactics.MonoGame` is the active frontend project. It references `CodecTactics.Core`, renders current board state, and routes player input through core commands.

Milestone 5 keeps animation, visual effects, camera feel, and audio in the MonoGame layer. `AudioService` owns playback of committed sound assets, while `Game1` translates core action results into presentation events such as pulse rings, shake, ownership interpolation, and sound cues. The core model remains deterministic and does not know about rendering, animation timing, or audio.

The Milestone 7 production presentation pass extends the same separation. `Game1` owns layered digital environment rendering, active connection glow and packet trails, node lighting, corruption distortion, relay pulses, trace-panel styling, and inertial camera motion. `AudioService` remains the audio boundary and can modulate the existing synthesized ambient hum from presentation-readable state such as corruption pressure or objective progress. None of these systems create gameplay facts or change core rule evaluation.

The previous frontend scene and project files are legacy artifacts. They should not be used for validation or new active frontend work unless a future task explicitly reopens that path.

The MonoGame frontend reads AI intent from `GameActionResult` and `NetworkGame.LastEnemyDecision`. It can highlight source-to-target pressure, target emphasis, profile, difficulty, and concise turn summaries without owning AI rules.
