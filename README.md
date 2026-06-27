# Codec_Tactics

Codec_Tactics is an early-stage C# MonoGame project for a turn-based network tactics game.

The player builds a growing 2D network. Each expansion opens useful paths, but poor choices can expose routes for enemy corruption. Future milestones may explore layers and cube-inspired visualization, but the active playable mission intentionally keeps gameplay flat and focused.

This repository is currently at Milestone 7: production presentation pass. It contains a deterministic core loop in pure C# plus a MonoGame frontend that renders generated missions as responsive living digital networks with layered digital environment motion, active communication channels, reactive lighting, camera inertia, integrated trace UI, and synthesized network audio. It still intentionally avoids layers, cube visualization, save/load, real art, and new player mechanics.

## Current Foundation

- C# solution for deterministic domain code, tests, and the MonoGame frontend
- Console-based automated test runner with no third-party test dependency
- Validation script for repeatable local checks
- Documentation for architecture, milestones, contribution workflow, and Codex usage
- Pure C# 2D network prototype with configurable board definitions, explicit graph links, layout positions, ownership, node types, energy costs, network integrity, threat, instability, deterministic corruption pressure, collapse, turn counter, and objective outcomes
- `BoardDefinition`, `GameConfiguration`, `ProceduralMissionSettings`, and deterministic procedural generation models that keep existing mechanics data-driven and prepare the core for later layers and cube faces
- Network-first MonoGame presentation that draws generated missions through deterministic topology positions, layered animated digital backdrop, glowing active communication channels, animated packet flow, relay amplification pulses, smooth inertial camera zoom/pan/recenter, silhouette/icon node identities, corruption overlays, objective beacons, event pulse rings, hover/selection/capture transitions, integrated trace UI, hover details, highlights, mission log, seed replay/new-seed controls, and win/loss banner while routing input through `CodecTactics.Core`
- Centralized MonoGame `AudioService` with committed synthesized digital WAV assets for hover, selection, confirmation, invalid actions, capture, reinforcement, weakening, corruption, objective progress, victory, defeat, reset, and ambient network hum
- Deterministic procedural missions with generated graph topology, mission placement, objective hold win condition, loss states, player feedback, replayable seeds, and a restartable game loop
- Modular tactical enemy AI with Aggressive, Defensive, Economic, Opportunistic, and Corruption-Focused profiles that score legal corruption actions from the visible board state without hidden bonuses
- Retired frontend files have been removed from the active repository.

## Requirements

- .NET SDK 8.0 or newer
- Visual Studio 2022 or newer with .NET desktop development tools
- MonoGame templates:

```powershell
dotnet new install MonoGame.Templates.CSharp
```

- MonoGame Content Builder editor:

```powershell
dotnet tool install --global dotnet-mgcb-editor
```

If the MGCB editor is already installed, update it instead:

```powershell
dotnet tool update --global dotnet-mgcb-editor
```

## Play Mission

Open `Codec_Tactics.sln` in Visual Studio and run the MonoGame project.

To set the startup project:

1. Right-click `CodecTactics.MonoGame` in Solution Explorer.
2. Select `Set as Startup Project`.
3. Press `F5` to run with the debugger, or `Ctrl+F5` to run without debugging.

The current MonoGame input surface supports both keyboard shortcuts and visible HUD buttons:

```text
1 = Claim
2 = Reinforce
3 = Weaken
Space = End Turn
R = Replay current seed
N = New random seed
C = Recenter camera
Esc = Exit
Left click = apply the selected action to a node
Left click HUD button = select action, end turn, or restart
Mouse wheel = smooth zoom
Right or middle mouse drag = smooth pan
Mouse hover = inspect node owner, type, integrity, threat, selected-action cost, expected result, blocked reason, instability, and danger reason
```

The app launches a generated procedural mission with a deterministic enemy personality chosen from the seed.

- Start: generated player core.
- Corruption start: generated far-side corruption location.
- Objective: generated Firewall objective, held for 2 turns by default.
- Win: claim and hold the objective for the required hold turns.
- Lose: the player core collapses or corruption captures the objective.
- Enemy: tactical corruption AI evaluates legal pressure targets each turn and reports its profile, target, and intent in the HUD.

Use the action buttons or number keys to choose Claim, Reinforce, or Weaken. The selected action is highlighted in the integrated trace UI, valid targets pulse on the network, and invalid targets are visually suppressed while the hover tooltip explains the block. The objective has a beacon glow, unstable nodes pulse orange, relay nodes emit amplifier ticks, corrupted nodes carry red disruption marks, and the selected node receives a white ring. Click a node to apply the selected action, or use End Turn to let corruption act without spending energy. Hover nodes to inspect ownership, type, integrity, threat, cost, expected action result, blocked reason, instability, and danger reason. After enemy turns, the board emphasizes the enemy source and target so the player can read the pressure path and intent. The board uses layered background motion, active link packets, hover/selection easing, capture and corruption shockwaves, relay packet amplification, invalid-action shake, objective pulses, screen-level mission-result emphasis, and synthesized digital audio cues with ambient intensity that reacts to mission pressure. Replay resets the current seed deterministically; New Seed rolls a different generated network and AI personality.

## Validate

Run from the repository root:

```powershell
.\scripts\validate.ps1
```

The script checks required repository files, builds the .NET solution, and runs tests.

## Project Layout

```text
docs/                         Design and architecture notes
scripts/                      Developer automation
src/CodecTactics.Core/        Pure C# game-domain foundation
src/CodecTactics.MonoGame/    Active MonoGame frontend
tests/CodecTactics.Core.Tests/ Console test runner
Codec_Tactics.sln             .NET validation solution
```

## Milestone 7 Presentation Pass

- The production presentation pass is presentation-only and does not change player mechanics, tactical AI, balance, mission generation, layers, cube visualization, or save/load.
- The network viewport uses layered animated environment rendering: scanlines, drifting circuit paths, distant infrastructure activity, foreground sweep, and subtle color atmosphere that stays behind gameplay.
- Connections render as active communication channels with glow lanes, packet trails, relay amplification pulses, and subdued inactive-link signal noise.
- Nodes use layered ownership/type lighting, objective beacon rings, relay amplifier ticks, corruption distortion marks, unstable warning glow, selection/hover easing, and existing event pulse effects.
- Camera movement uses smoothed target following with inertial pan/zoom/recenter behavior.
- The HUD is styled as an integrated trace panel with stronger hierarchy, translucent signal bands, action-state markers, and compact recent event history so the board remains dominant.
- `AudioService` keeps the committed synthesized cues and modulates the ambient network hum based on corruption pressure and objective progress.

## Current Mechanics

- The default prototype board is a 4x4 orthogonal node grid defined by `BoardDefinition.CreateDefaultPrototype()`.
- The core engine can initialize authored grids or explicit generated graph topologies from `BoardDefinition`.
- `ProceduralMissionGenerator` creates deterministic missions from integer or text seeds using configurable node count, graph density, branching, corruption start count, objective distance, node type frequencies, starting energy, and objective hold settings.
- Generated missions include explicit links, generated layout positions, seed metadata, player start, corruption starts, objective placement, Resource placement, Relay placement, and Firewall placement.
- Default costs, corruption growth, integrity values, threat values, and collapse timing are provided by `GameConfiguration`.
- Nodes can be neutral, player-owned, or enemy-owned.
- The player starts at `(0,0)` and enemy corruption starts at `(3,3)`.
- Nodes have types: Standard, Resource, Relay, and Firewall.
- The player has energy. Claiming costs 2 energy; reinforcing costs 1 energy.
- Player-owned Resource nodes generate 2 energy at the start of each new player turn.
- Player-owned Relay nodes extend claim range to neutral nodes within two active connections.
- Player-owned nodes calculate integrity from core connection, distance, owned neighbors, Relay support, Firewall support, dense structure, reinforcement, and isolation.
- Player-owned nodes calculate threat from nearby corruption, corruption pressure, weak owned connections, frontier exposure, and isolation.
- Nodes become unstable when threat exceeds integrity. If instability persists for 2 enemy turns, the node collapses to corruption.
- Corruption pressure increases by 1 each enemy turn. Tactical AI scores legal adjacent targets by objective proximity, Relay value, Resource value, network control, corruption opportunities, player expansion, defensive value, reachable territory, pressure efficiency, and future positioning.
- Enemy personalities weight those factors differently: Aggressive, Defensive, Economic, Opportunistic, and Corruption-Focused profiles create different tactical pressure without changing player mechanics.
- Difficulty changes decision quality rather than resources. Hard and Expert select the best evaluated action; lower difficulties can choose from near-best evaluated options.
- Firewall nodes require 2 corruption pressure before corruption can claim them.
- Player actions include claiming reachable neutral nodes, reinforcing owned nodes, weakening reachable enemy connections, and ending the turn.
- Procedural missions use the same objective-hold win/loss rules as the vertical-slice mission.
- Objective hold progress increases after successful player actions or end turns that leave the objective player-owned after corruption resolves.
- The mission ends in loss if the player core is no longer player-owned or the objective becomes corrupted.
- See `docs/network-integrity.md` for formulas, examples, collapse behavior, and strategy implications.
- See `docs/board-definition.md` for board definition and configuration details.
- See `docs/playable-vertical-slice.md` for the authored regression mission route, feedback, tests, and limitations.

## Milestone 1.5 Visible Prototype

- The original Milestone 1.5 legacy prototype is now retired from the active workflow.
- The active MonoGame frontend renders the current authored mission board as an irregular network topology with animated links, camera zoom/pan/recenter, icon-driven nodes, action buttons, hover tooltip, valid move highlights, dimmed invalid targets, objective/danger pulse outlines, result log, and compact win/loss banner.
- Neutral, player, enemy/corruption, and reinforced nodes have distinct visual treatments.
- Node types use distinct silhouettes, iconography, colors, glow, and overlays before relying on text.
- Valid reachable neutral clicks claim nodes through `CodecTactics.Core`.
- Invalid clicks do not mutate core state and update the HUD status text.
- End Turn uses the real core corruption turn.
- The HUD shows selected action, turn, energy, objective progress, corruption pressure, enemy AI profile, result feedback, invalid move reasons, and recent action history.
- Replay starts a fresh deterministic copy of the active seed. New Seed creates a different generated mission.
- Milestone 4 is presentation-only: it establishes Codec_Tactics' network-first visual identity without adding layers, cube visualization, retired frontend work, or new mechanics.
- Milestone 5 is presentation-only: it adds responsive interaction animation, event visual effects, living-network motion, and centralized real audio assets without changing core gameplay rules or mission balance.

## Current Limitations

- MonoGame visuals and audio now establish the intended responsive network feel, but the visuals are still code-drawn prototype art rather than final production assets.
- Retired frontend files are no longer part of the active repository.
- Procedural generation is still a 2D layered graph foundation; campaign progression and saved seed history are not implemented yet.
- Network integrity and threat formulas are prototype balance values.
- Alternate board definitions are engine-supported but not yet balanced scenarios.
- Tactical AI personalities are first-pass balance values and will need broader playtesting.
- No layers, cube visualization, save/load, broader balancing, final art, or production UI.
