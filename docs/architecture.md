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

## Node Model

Nodes are the primary tactical objects.

Potential node state:

- Ownership or control state.
- Corruption state.
- Layer coordinate.
- Network links.
- Visibility or discovered status.
- Future resource or defense properties.

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

## Enemy and Corruption System

Corruption should spread through access created by the network. It should punish careless expansion without making the player feel that outcomes are arbitrary.

Implementation goals:

- Deterministic spread for automated tests.
- Clear source and path of corruption.
- Visible risk before player commitment where possible.
- Rules that can grow from simple 2D prototypes to layered spaces.

## Future Visualization Goals

- Start with readable 2D prototype scenes.
- Separate core state from Godot rendering.
- Add clear node selection and path feedback.
- Explore cube-based navigation after rules are testable.
- Keep debug overlays available for topology, layer, and corruption state.
