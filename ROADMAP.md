# Roadmap

## Milestone 0: Repo Foundation

- Create a clean Godot/C# repository structure.
- Add validation, tests, documentation, and contribution workflow.
- Avoid gameplay implementation until the project can be safely modified.

## Milestone 1: Playable 2D Network Prototype

- Represent a small 2D network map.
- Allow the player to select and expand network nodes.
- Provide immediate feedback for valid and invalid expansion choices.
- Implemented in pure C# with deterministic enemy spread, turn progression, and console test coverage.

## Milestone 1.5: Visible Godot Prototype

- Add a launchable Godot main scene.
- Render the tested 2D network board with temporary visual nodes and connections.
- Route node clicks and the End Turn button through `CodecTactics.Core`.
- Keep Godot scene code thin and free of gameplay rules.
- Status: implemented as the first visual integration slice.

## Milestone 2: Strategic 2D Node Decisions

- Add Standard, Resource, Relay, and Firewall node types.
- Add player energy, action costs, Resource income, Relay claim reach, and Firewall corruption resistance.
- Add deterministic corruption pressure and structured Godot-facing action feedback.
- Status: implemented as the strategic 2D prototype slice.

## Milestone 3: Network Integrity and Threat System

- Make network structure strategically meaningful.
- Add calculated integrity and threat values to player-owned nodes.
- Add instability and deterministic collapse into corruption.
- Update corruption expansion to prioritize weak, exposed, and unstable targets.
- Surface danger reasons in the Godot prototype.
- Status: implemented as the network integrity strategy slice.

## Milestone 3.25: Engine Hardening and Board Generalization

- Move the default 4x4 prototype into a `BoardDefinition`.
- Add configurable board dimensions, start positions, node type placement, and initial ownership.
- Add `GameConfiguration` for costs, corruption growth, integrity constants, threat constants, and collapse timing.
- Preserve deterministic behavior and existing gameplay balance.
- Keep Godot as a thin presentation layer that loads the default board definition.
- Status: implemented as the board/configuration hardening slice.

## Milestone 3.75: Playable Vertical Slice Mission

- Define one authored mission with a fixed 5x5 board, player start, corruption start, objective node, and balanced node type placement.
- Add objective hold win condition and loss conditions for player core collapse or objective corruption.
- Add player action modes for Claim, Reinforce, and Weaken, plus End Turn and deterministic Restart Mission.
- Surface selected action, hover/click feedback, energy, objective progress, collapse/corruption events, danger warnings, and clear win/loss state in the Godot prototype.
- Keep all mission and action rules in `CodecTactics.Core`.
- Status: implemented as the first complete start-to-finish playable mission.

## Milestone 3.5: Layers

- Add deeper network layers with increasing complexity.
- Define transitions between layers.
- Connect player progression to layer descent.

## Milestone 4: Cube Visualization

- Explore cube-based visualization for layered network space.
- Keep core rules separate from rendering.
- Prototype camera, selection, and readable node state display.

## Milestone 5: Vertical Slice

- Expand the playable mission into a broader slice after layers and cube visualization exist.
- Combine expansion, corruption, layers, and visualization.
- Establish deeper game feel, tuning, and presentation.
