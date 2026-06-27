# Roadmap

## Milestone 0: Repo Foundation

- Create a clean C# repository structure.
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
- Status: implemented historically and now retired as a legacy frontend.

## Workflow Migration: MonoGame Frontend

- Add a Visual Studio-first MonoGame project at `src/CodecTactics.MonoGame`.
- Reference `CodecTactics.Core` from the MonoGame frontend.
- Keep all gameplay and mission rules in the core project.
- Remove Godot from validation and active launch instructions while retaining legacy files until safe removal.
- Status: active workflow.

## Milestone 2: Strategic 2D Node Decisions

- Add Standard, Resource, Relay, and Firewall node types.
- Add player energy, action costs, Resource income, Relay claim reach, and Firewall corruption resistance.
- Add deterministic corruption pressure and structured frontend-facing action feedback.
- Status: implemented as the strategic 2D prototype slice.

## Milestone 3: Network Integrity and Threat System

- Make network structure strategically meaningful.
- Add calculated integrity and threat values to player-owned nodes.
- Add instability and deterministic collapse into corruption.
- Update corruption expansion to prioritize weak, exposed, and unstable targets.
- Surface danger reasons in the active frontend.
- Status: implemented as the network integrity strategy slice.

## Milestone 3.25: Engine Hardening and Board Generalization

- Move the default 4x4 prototype into a `BoardDefinition`.
- Add configurable board dimensions, start positions, node type placement, and initial ownership.
- Add `GameConfiguration` for costs, corruption growth, integrity constants, threat constants, and collapse timing.
- Preserve deterministic behavior and existing gameplay balance.
- Keep the frontend as a thin presentation layer that loads the default board definition.
- Status: implemented as the board/configuration hardening slice.

## Milestone 3.75: Playable Vertical Slice Mission

- Define one authored mission with a fixed 5x5 board, player start, corruption start, objective node, and balanced node type placement.
- Add objective hold win condition and loss conditions for player core collapse or objective corruption.
- Add player action modes for Claim, Reinforce, and Weaken, plus End Turn and deterministic Restart Mission.
- Surface selected action, click feedback, energy, objective progress, collapse/corruption events, danger warnings, and clear win/loss state in the active frontend.
- Keep all mission and action rules in `CodecTactics.Core`.
- Status: implemented as the first complete start-to-finish playable mission.

## MonoGame Playability Pass 1

- Increase the MonoGame window size and center the authored board.
- Add a readable HUD with action buttons, selected action highlight, keyboard shortcut parity, action log, invalid move feedback, and mission status.
- Add readable node labels, hover tooltips, valid move highlights, objective highlight, corruption/collapse feedback, and win/loss banner.
- Keep the pass presentation-only with no new gameplay rules, layers, cubes, or Godot work.
- Status: implemented as the first readability and playability pass for the MonoGame vertical slice.

## MonoGame Playability Pass 2

- Make player, corruption, neutral, objective, and unstable nodes more visually distinct.
- Replace repeated neutral text with concise owner/type badges and a small board legend.
- Expand hover details with selected-action cost, valid-result preview, and blocked-target explanations.
- Show valid targets clearly and dim invalid targets for the selected action.
- Improve mission feed, objective progress, corruption/collapse feedback, action-log density, selected-node feedback, and end-state banner placement.
- Keep the pass presentation-only with no new gameplay rules, layers, cubes, or Godot work.
- Status: implemented as the second clarity and game-feel pass for the MonoGame vertical slice.

## Milestone 4: Visual Identity

- Replace the grid-first MonoGame presentation with a network-first topology renderer.
- Make the board dominate the screen through deterministic visual positions, animated links, glow, ownership color, corruption overlays, and compact node state indicators.
- Differentiate Core, Resource, Relay, Firewall, Objective, Standard, and Corrupted nodes through silhouette, iconography, color, animation, and outline rather than abbreviated text.
- Add presentation-focused camera zoom, pan, and recenter behavior with comfortable margins for larger future maps.
- Reduce the HUD to concise action, resource, objective, status, and trace indicators so the network remains the primary visual focus.
- Keep the pass presentation-only with no new gameplay rules, layers, cubes, or Godot work.
- Status: implemented as the first visual identity pass for the MonoGame vertical slice.

## Milestone 5: Interaction, Animation, and Audio

- Replace abrupt presentation changes with responsive hover, selection, ownership, capture, corruption, objective, and end-state animation.
- Make the network feel alive through directional packet flow, relay amplification pulses, corruption pressure pulses, and idle node activity.
- Add lightweight visual effects for valid actions, invalid actions, captures, reinforcements, weakening, corruption spread, objective progress, victory, and defeat.
- Add a centralized MonoGame audio service with committed synthesized digital sound assets instead of placeholders.
- Keep the pass presentation-only with no new gameplay rules, balance changes, layers, cubes, or Godot work.
- Status: implemented as the interaction, animation, and audio polish pass for the MonoGame vertical slice.

## Milestone 3.5: Layers

- Add deeper network layers with increasing complexity.
- Define transitions between layers.
- Connect player progression to layer descent.

## Future Visualization: Cube

- Explore cube-based visualization for layered network space.
- Keep core rules separate from rendering.
- Prototype camera, selection, and readable node state display.

## Future Vertical Slice Expansion

- Expand the playable mission into a broader slice after layers and cube visualization exist.
- Combine expansion, corruption, layers, and visualization.
- Establish deeper game feel, tuning, and presentation.
