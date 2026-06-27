# Changelog

All notable changes to Codec_Tactics will be documented in this file.

## Unreleased

- Added Milestone 7 tactical AI with modular enemy decision scoring, configurable Aggressive/Defensive/Economic/Opportunistic/Corruption-Focused personalities, decision-quality difficulty, deterministic intent summaries, AI source/target visualization in MonoGame, and tests for deterministic decisions, valid actions, objective pressure, personality differences, difficulty quality, and stable summaries.
- Added Milestone 5 interaction, animation, and audio polish for the MonoGame frontend with hover/selection easing, ownership transition feedback, action/capture/corruption/objective pulse effects, invalid-action shake, relay packet amplification, centralized `AudioService`, ambient network hum, and committed synthesized WAV cues while preserving core gameplay behavior.
- Added Milestone 4 visual identity pass for the MonoGame frontend with a network-first topology renderer, animated connection flow, smooth zoom/pan/recenter camera controls, silhouette/icon-based node identities, corruption overlays, compact HUD indicators, and reduced board text while preserving core gameplay behavior.
- Added MonoGame Playability Pass 2 with concise node badges, stronger owner/type/objective/danger contrast, a board legend, selected-node outline, pulsing objective and unstable-node outlines, hover action previews, visible invalid-target dimming, clearer mission feed messages, compact action log entries, an objective progress bar, and a less obstructive mission result banner.
- Added MonoGame Playability Pass 1 with a larger window, centered mission board, readable HUD panel, visible Claim/Reinforce/Weaken/End Turn/Restart buttons, keyboard shortcuts, selected-action highlight, valid move highlight, objective highlight, node hover tooltip, action log, invalid move feedback, corruption/collapse messages, readable node labels, and clear win/loss banner.
- Added `src/CodecTactics.MonoGame` as the active Visual Studio-first MonoGame frontend.
- Referenced `CodecTactics.Core` from the MonoGame project and added the project to `Codec_Tactics.sln`.
- Removed Godot from active validation while retaining legacy Godot files for reference.
- Updated documentation for MonoGame install requirements, Visual Studio startup workflow, and Godot legacy status.
- Established initial repository foundation.
- Added Godot project metadata.
- Added .NET solution, core project, and console test runner.
- Added validation script and project documentation.
- Added Milestone 1 deterministic 2D network prototype with fixed grid board, node ownership, player actions, enemy expansion, turn progression, and placeholder outcomes.
- Added console tests for board creation, node claiming, reinforcement, enemy spread, turn progression, and deterministic behavior.
- Updated documentation for current Milestone 1 mechanics and limitations.
- Added Milestone 1.5 visible Godot prototype with a launchable main scene, temporary board rendering, click-to-claim interaction, End Turn button, HUD status text, and core-backed state updates.
- Added core tests for invalid claim behavior used by the Godot prototype.
- Updated validation to build the Godot C# project when Godot CLI is available and clearly skip editor validation when it is not.
- Added Milestone 2 strategic node mechanics with Standard, Resource, Relay, and Firewall node types.
- Added deterministic player energy costs, Resource income, Relay claim reach, corruption pressure, and Firewall corruption resistance in `CodecTactics.Core`.
- Updated the Godot prototype HUD and rendering to show energy, corruption pressure, node types, ownership, costs, failures, and real end-turn corruption resolution.
- Added mechanics documentation and expanded deterministic core tests for Milestone 2 behavior.
- Added Milestone 3 network integrity and threat calculations for player-owned nodes.
- Added deterministic instability tracking, node collapse into corruption, and collapse event reporting.
- Updated corruption targeting to prioritize unstable, weak, and exposed targets deterministically.
- Updated the Godot prototype to display integrity, threat, unstable nodes, danger reasons, and collapse messages.
- Added `docs/network-integrity.md` with formulas, examples, collapse rules, and strategy implications.
- Added deterministic tests for integrity calculations, isolation penalties, Relay and Firewall support, threat progression, instability, collapse, and corruption targeting.
- Added Milestone 3.25 engine hardening with `BoardDefinition` and `GameConfiguration`.
- Generalized core initialization for alternate board dimensions, player starts, corruption starts, node type placement, starting resources, and future metadata while preserving the default 4x4 prototype.
- Updated the Godot prototype to load the default board through `BoardDefinition`.
- Added deterministic tests for multiple board sizes, rectangular boards, alternate spawn positions, custom board definitions, configuration defaults, and deterministic initialization.
- Added `docs/board-definition.md` for board definition and configuration guidance.
- Added a playable vertical-slice mission, `Secure the Uplink`, with a fixed 5x5 board, player start, corruption start, target objective, objective hold win condition, and core/objective loss conditions.
- Added core mission state, player action mode routing, objective hold tracking, deterministic restart construction, game-over action blocking, and Godot-facing action results.
- Updated the Godot prototype with Claim, Reinforce, Weaken, End Turn, and Restart Mission controls plus selected action, hover status, objective progress, energy, danger, result, and mission feedback.
- Added deterministic mission tests for initialization, win/loss conditions, objective hold turns, action modes, restart determinism, and invalid actions after game over.
- Added `docs/playable-vertical-slice.md` for the authored mission, play loop, feedback, validation, and limitations.
