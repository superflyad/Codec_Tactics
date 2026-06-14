# Roadmap

## Milestone 0: Repo Foundation

- Create a clean Godot/C# repository structure.
- Add validation, tests, documentation, and contribution workflow.
- Avoid gameplay implementation until the project can be safely modified.

## Milestone 1: Playable 2D Network Prototype

- Represent a small 2D network map.
- Allow the player to select and expand network nodes.
- Provide immediate feedback for valid and invalid expansion choices.

## Milestone 2: Turn System and Enemy Spread

- Add explicit player and enemy turns.
- Introduce deterministic corruption spread rules.
- Add tests for turn order and spread outcomes.

## Milestone 3: Layers

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
