# Milestones

## Milestone 0: Repo Foundation

Build the repository structure, validation workflow, documentation, and test harness. Do not implement real gameplay.

Exit criteria:

- `README.md`, `CODEX.md`, contribution docs, roadmap, and architecture docs exist.
- Validation script runs from the repo root.
- Automated tests are available and deterministic.
- Project can be committed and pushed.

## Milestone 1: Playable 2D Network Prototype

Create a minimal 2D network map with player expansion.

Exit criteria:

- Player can select valid expansion targets.
- Network state updates visibly.
- Core expansion rules are covered by tests.

## Milestone 2: Turn System and Enemy Spread

Add turn phases and deterministic corruption spread.

Exit criteria:

- Player and enemy/corruption turns are distinct.
- Corruption spread can be tested without Godot rendering.
- UI communicates spread results.

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
