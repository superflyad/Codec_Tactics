# Milestones

## Milestone 0: Repo Foundation

Build the repository structure, validation workflow, documentation, and test harness. Do not implement real gameplay.

Exit criteria:

- `README.md`, `CODEX.md`, contribution docs, roadmap, and architecture docs exist.
- Validation script runs from the repo root.
- Automated tests are available and deterministic.
- Project can be committed and pushed.

## Milestone 1: Playable 2D Network Prototype

Create a minimal 2D network map with player expansion. The current implementation is a pure C# prototype so the core loop can be tested without frontend dependencies.

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

- The connected frontend is intentionally minimal and debug-style.
- No layered board, cube visualization, advanced AI, save/load, resources, or polish.
- Win/loss conditions are placeholders only.

## Milestone 1.5: Retired Visible Prototype

Connect the tested 2D network core to the first launchable frontend scene.

Exit criteria:

- The retired frontend had a configured main scene.
- The scene renders current board nodes and active connections.
- Neutral, player, enemy/corruption, and reinforced nodes are visually distinguishable.
- Valid adjacent neutral clicks call core claim behavior.
- Invalid clicks do not mutate gameplay state.
- End Turn calls current core behavior and updates the board.
- HUD text shows turn, phase, status, and result state.

Current status:

- End Turn was a placeholder pass action in Milestone 1.5; Milestone 2 replaces it with a real core end-turn action.
- Rendering is temporary debug-style UI.
- This prototype is retired and no longer part of the active repository.

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
- The frontend displays energy, pressure, ownership, node types, action failures, costs, and corruption spread results.
- Core tests cover node types, energy, Relay reach, Firewall resistance, pressure progression, and deterministic turn progression.

Current limitations:

- Node type placement is fixed for the default 4x4 prototype.
- Energy and resistance values are prototype constants.
- No layers, cube visualization, advanced AI, save/load, real art, or production UI.

## Milestone 3: Network Integrity and Threat System

Transform the prototype from node expansion into a strategy game where network structure matters.

Exit criteria:

- Every player-owned node has calculated integrity and threat.
- Integrity accounts for distance from core, owned connection count, Relay support, Firewall support, density, reinforcement, and isolation.
- Threat accounts for nearby corruption, corruption pressure, weak connections, frontier exposure, and isolation.
- Nodes become unstable when threat exceeds integrity.
- Persistent instability collapses nodes to corruption deterministically.
- Corruption expansion prioritizes unstable, weak, and exposed targets deterministically.
- The frontend displays integrity, threat, unstable nodes, danger reasons, and collapse events.
- Core tests cover the integrity, threat, instability, collapse, and targeting contracts.

Current limitations:

- Formulas are prototype balance values.
- The board is still a fixed 4x4 2D grid.
- No layers, cube visualization, advanced graphics, save/load, or advanced AI.

## Future Milestone: Layers

Add deeper network layers.

Exit criteria:

- Nodes can belong to different layers.
- Player can transition between layers.
- Deeper layers increase complexity.

## Milestone 4: Visual Identity

Replace the grid-first MonoGame presentation with a network-first visual identity for the active vertical-slice mission.

Exit criteria:

- The authored board renders as an irregular digital network topology rather than a square-cell grid.
- Connections are first-class animated visual elements.
- Node ownership, type, objective, corruption, instability, and selection state are readable through silhouette, color, glow, outline, and compact indicators before text.
- Camera zoom, pan, and recenter behavior keeps the network comfortable to inspect.
- The HUD remains concise so the board is the primary visual focus.
- Core gameplay rules, layers, cube visualization, balance, and retired frontend scope remain unchanged.

## Future Visualization: Cube

Prototype cube-inspired visualization after the flat network rules and mission feel are stable.

Exit criteria:

- Core game state can be displayed through cube-based views.
- 2D and cube visualization share the same underlying model.
- Selection and feedback remain readable.

## Milestone 5: Interaction, Animation, and Audio

Polish the active MonoGame vertical slice without changing mechanics.

Exit criteria:

- Hover, selection, deselection, ownership changes, capture, corruption, objective progress, integrity/threat feedback, camera movement, and mission results have responsive animation or visual effects.
- The network has idle motion, directional packet flow, relay amplification, corruption pulses, and readable event pulses.
- Successful and invalid actions produce immediate visual and audio feedback.
- Audio is routed through a centralized presentation service and backed by committed synthesized WAV assets.
- Core gameplay rules, balance, mission shape, layers, cube visualization, and retired frontend scope remain unchanged.
- Validation covers existing deterministic rule behavior and the MonoGame project builds cleanly.

## Milestone 6: Procedural Mission Generation

Create replayable generated missions without changing the existing player mechanics.

Exit criteria:

- Procedural missions are deterministic from a seed.
- Generated missions include explicit graph links, readable layout positions, player start, corruption starts, objective placement, and node type placement.
- Generated missions use the same `MissionDefinition` and `NetworkGame` flow as authored missions.
- The active frontend supports replaying the current seed and rolling a new seed.
- Core tests cover determinism, graph validity, placement constraints, and layout readability.

Status: implemented as the procedural mission foundation.

## Tactical AI and Enemy Personalities

Improve opponent decision quality without adding player mechanics or changing combat rules.

Exit criteria:

- Enemy turns evaluate legal corruption actions from the current board state instead of following a fixed script.
- Tactical scoring considers objective proximity, Relay and Resource value, network control, corruption opportunities, player expansion, defensive value, reachable territory, pressure efficiency, and future positioning.
- Aggressive, Defensive, Economic, Opportunistic, and Corruption-Focused personalities use different weights over the same rules.
- Difficulty changes which evaluated action is selected, without granting hidden resources or map knowledge.
- `GameActionResult` reports enemy action type, source, target, profile, difficulty, primary factor, score, and intent summary for presentation.
- The active frontend visualizes enemy intent with source-to-target highlights, target emphasis, AI profile text, and turn summaries.
- Automated tests cover deterministic decisions, valid reachable action selection, objective prioritization, personality differences, difficulty quality, no illegal moves, and stable summaries.

Current limitations:

- Personalities are first-pass balance values.
- Enemy actions still use the existing corruption pressure, spread, focus, and collapse rules.
- AI has no hidden information model yet because the current game state is fully visible.

## Milestone 7: Production Presentation Pass

Elevate the active MonoGame vertical slice from polished prototype to production-quality presentation while preserving gameplay mechanics, balance, mission generation, tactical AI, layers, cube visualization, and save/load scope.

Exit criteria:

- The network viewport has a distinctive layered digital environment with subtle scan, circuit, distant activity, data-field, and atmosphere motion that does not interfere with readability.
- Connections read as active communication channels through glow, packet trails, relay amplification, ownership color, and subdued inactive signal noise.
- Node ownership, type, objective, relay, corruption, instability, hover, selection, and action feedback are readable through layered lighting, silhouettes, distortion, and event pulses before relying on text.
- Camera pan, zoom, and recenter behavior use smoothed inertial movement.
- The HUD is visually integrated as compact trace instrumentation and keeps the board dominant.
- Synthesized audio remains centralized in `AudioService`, uses committed assets, and supports pressure-reactive ambient sound.
- Core gameplay rules, balance values, tactical AI behavior, mission generation, layers, cube visualization, and retired frontend scope remain unchanged.

Status: implemented as the production presentation pass for the MonoGame vertical slice.
