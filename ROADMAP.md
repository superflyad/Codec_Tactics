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

## Milestone 3.5: Layers

- Add deeper network layers with increasing complexity.
- Define transitions between layers.
- Connect player progression to layer descent.

## Milestone 4: Cube Visualization

- Explore cube-based visualization for layered network space.
- Keep core rules separate from rendering.
- Prototype camera, selection, and readable node state display.

## Milestone 5: Vertical Slice

- Combine expansion, corruption, layers, and visualization.
- Add one complete playable scenario.
- Establish the first pass of game feel and tuning.
