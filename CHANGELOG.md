# Changelog

All notable changes to Codec_Tactics will be documented in this file.

## Unreleased

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
