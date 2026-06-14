# Game Design Overview

Codec_Tactics is planned as a turn-based network tactics game about building controlled access through increasingly risky systems.

## Core Fantasy

The player is not simply moving units across a battlefield. The player is expanding a network. Every new connection creates opportunity and exposure.

## Network Expansion

- The player starts with a small trusted network.
- Each turn presents possible expansion choices.
- Expansion can unlock resources, visibility, defensive options, or deeper layers.
- Poor expansion can expose the network to enemy corruption.

## Layered Structure

The network is organized into layers. Upper layers should be easier to read and safer to explore. Deeper layers should add complexity through denser topology, hidden risks, and more aggressive enemy access.

## Cube-Based Direction

Future visualization may represent layers as cube faces, cube slices, or cube-like network volumes. The foundation should keep game rules independent from visualization so the project can prototype 2D first and move toward cube visualization later.

## Enemy Corruption

Enemy pressure comes from access. Badly planned routes, overextended connections, and exposed nodes allow corruption to enter or spread. The enemy system should be deterministic enough to test while still creating interesting strategic pressure.

## Non-Goals For Milestone 0

- No real gameplay loop.
- No balancing.
- No production art.
- No full Godot scene implementation.
