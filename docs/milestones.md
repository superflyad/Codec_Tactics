# Milestones

## Milestone 0: Repo Foundation

Build the repository structure, validation workflow, documentation, and test harness. Do not implement real gameplay.

Exit criteria:

- `README.md`, `CODEX.md`, contribution docs, roadmap, and architecture docs exist.
- Validation script runs from the repo root.
- Automated tests are available and deterministic.
- Project can be committed and pushed.

## Milestone 1: Playable 2D Network Prototype

Create a minimal 2D network map with player expansion. The current implementation is a pure C# prototype so the core loop can be tested without Godot.

Exit criteria:

- Player can select valid expansion targets.
- Network state updates visibly.
- Core expansion rules are covered by tests.
- Deterministic enemy expansion, turn progression, and placeholder outcomes are covered by tests.

Current mechanics:

- Fixed 4x4 board with orthogonal adjacent connections.
- Node ownership is neutral, player, or enemy.
- Player actions claim adjacent neutral nodes, reinforce owned nodes, or weaken reachable enemy connections.
- Enemy corruption expands into the first adjacent neutral node in row-major order after each successful player action.

Current limitations:

- The connected Godot scene is intentionally minimal and debug-style.
- No layered board, cube visualization, advanced AI, save/load, resources, or polish.
- Win/loss conditions are placeholders only.

## Milestone 1.5: Visible Godot Prototype

Connect the tested 2D network core to the first launchable Godot scene.

Exit criteria:

- Godot has a configured main scene.
- The scene renders current board nodes and active connections.
- Neutral, player, enemy/corruption, and reinforced nodes are visually distinguishable.
- Valid adjacent neutral clicks call core claim behavior.
- Invalid clicks do not mutate gameplay state.
- End Turn calls current core behavior and updates the board.
- HUD text shows turn, phase, status, and result state.

Current limitations:

- End Turn was a placeholder pass action in Milestone 1.5; Milestone 2 replaces it with a real core end-turn action.
- Rendering is temporary debug-style UI.
- Godot validation is optional when the CLI is unavailable.

## Milestone 2: Strategic 2D Node Decisions

Refine the visible 2D prototype with meaningful node choices, player energy, and corruption pressure before adding layers or cube visualization.

Exit criteria:

- Standard, Resource, Relay, and Firewall node types exist in core state.
- Claiming and reinforcing spend deterministic player energy.
- Player-owned Resource nodes generate energy at the start of each player turn.
- Player-owned Relay nodes extend claim reach through active connections.
- Corruption pressure increases on enemy turns and expands deterministically.
- Firewall nodes delay corruption by requiring additional pressure.
- End Turn resolves a real enemy/corruption turn.
- Godot displays energy, pressure, ownership, node types, action failures, costs, and corruption spread results.
- Core tests cover node types, energy, Relay reach, Firewall resistance, pressure progression, and deterministic turn progression.

Current limitations:

- Node type placement is fixed for the default 4x4 prototype.
- Energy and resistance values are prototype constants.
- No layers, cube visualization, advanced AI, save/load, real art, or production UI.

## Milestone 3: Layers

Add deeper network layers.

Exit criteria:

- Nodes can belong to different layers.
- Player can transition between layers.
- Deeper layers increase complexity.

## Milestone 4: Cube Visualization

Prototype cube-inspired visualization.

Exit criteria:

- Core game state can be displayed through cube-based views.
- 2D and cube visualization share the same underlying model.
- Selection and feedback remain readable.

## Milestone 5: Vertical Slice

Deliver a complete small scenario.

Exit criteria:

- Scenario includes expansion, corruption, layers, and win/loss conditions.
- Validation covers critical rule behavior.
- Documentation reflects the implemented slice.
