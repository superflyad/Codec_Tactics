# Codec_Tactics

Codec_Tactics is an early-stage C# MonoGame project for a turn-based network tactics game.

The player builds a growing 2D network. Each expansion opens useful paths, but poor choices can expose routes for enemy corruption. Layer topology and a cube-inspired layer readout are now visible in the MonoGame frontend, while active gameplay rules remain graph-based and focused.

This repository is currently at Milestone 8: visual identity redesign. It contains a deterministic core loop in pure C# plus a MonoGame frontend that renders generated missions as a fiber-lattice infrastructure: bundled signal routes, shape-first junction silhouettes, corruption fractures, diagnostic rail UI, reactive lighting, camera inertia, synthesized network audio, explicit layer topology metadata, layer focus navigation, layer-specific risk modifiers, a code-drawn layer cube readout, persisted seed history, local profile progression, authored campaign arc scaffolding over generated missions, branch-aware Operations presentation, and local three-slot save/load. It still intentionally avoids campaign cinematics, cloud sync, full cube-face gameplay, and new player mechanics.

## Current Foundation

- C# solution for deterministic domain code, tests, and the MonoGame frontend
- Console-based automated test runner with no third-party test dependency
- Validation script for repeatable local checks
- Documentation for architecture, milestones, contribution workflow, and Codex usage
- Pure C# 2D network prototype with configurable board definitions, explicit graph links, layout positions, ownership, node types, energy costs, network integrity, threat, instability, deterministic corruption pressure, collapse, turn counter, and objective outcomes
- `BoardDefinition`, `GameConfiguration`, `ProceduralMissionSettings`, explicit node layer assignments, transition links, and deterministic procedural generation models that keep existing mechanics data-driven and prepare the core for later cube faces
- Opt-in `LayerRuleModifier` tuning for per-layer integrity, threat, and corruption resistance
- Network-first MonoGame presentation that draws generated missions through deterministic topology positions, fiber-lattice backdrop, bundled communication paths, diamond packet flow, relay amplification cells, smooth inertial camera zoom/pan/recenter, shape-first node silhouettes, corruption fracture overlays, objective beacons, event pulse rings, hover/selection/capture transitions, code-drawn layer cube inset, integrated diagnostic rail UI, hover details, highlights, mission log, seed replay/new-seed controls, and win/loss banner while routing input through `CodecTactics.Core`
- Centralized MonoGame `AudioService` with committed synthesized digital WAV assets for hover, selection, confirmation, invalid actions, capture, reinforcement, weakening, corruption, objective progress, victory, defeat, reset, and ambient network hum
- Deterministic procedural missions with generated graph topology, mission placement, objective hold win condition, loss states, player feedback, replayable seeds, and a restartable game loop
- Authored `Signal Recovery` campaign arc scaffolding that names eight generated stages, including recovery briefings after losses
- Lightweight campaign progression that uses completed seed history to advance through harder generated stages after wins and recovery traces after losses, with branch route context shown before launch
- Local profile progression with level, XP, title, win/loss totals, best stage, and current win streak derived from completed campaign runs
- Local persisted seed history for the active MonoGame frontend under the user's local application data
- Core game snapshots plus MonoGame local three-slot save/load for active generated missions
- Curated regression scenario catalog and deterministic balance screening over curated scenarios, generated seed corpus, and tactical AI personalities
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
Operations:
Enter = Continue campaign
N = Free trace
L = Load selected slot
F1/F2/F3 = Select save slot
Esc = Exit from Operations

Mission:
1 = Claim
2 = Reinforce
3 = Weaken
Space = End Turn
R = Replay current seed
N = New random seed
C = Recenter camera
S = Save active trace locally
L = Load saved trace
F1/F2/F3 = Select save slot
[ / ] = Cycle layer focus
Esc = Return to Operations
Left click = apply the selected action to a node
Left click HUD button = select action, end turn, or restart
Mouse wheel = smooth zoom
Right or middle mouse drag = smooth pan
Mouse hover = inspect node owner, type, integrity, threat, selected-action cost, expected result, blocked reason, instability, and danger reason
```

The app opens on an Operations screen that shows local profile progress, the next `Signal Recovery` campaign trace, the current opening/advance/recovery route context, the selected save slot, and actions for continuing the campaign, loading a slot, or launching a free trace. Campaign and free-trace missions use deterministic generated networks with enemy personality chosen from the seed.

- Start: generated player core.
- Corruption start: generated far-side corruption location.
- Objective: generated Firewall objective, held for 2 turns by default.
- Win: claim and hold the objective for the required hold turns.
- Lose: the player core collapses or corruption captures the objective.
- Enemy: tactical corruption AI evaluates legal pressure targets each turn and reports its profile, target, and intent in the HUD.

Use the action buttons or number keys to choose Claim, Reinforce, or Weaken. The selected action is highlighted in the integrated trace UI, valid targets pulse on the network, and invalid targets are visually suppressed while the hover tooltip explains the block. The objective has a beacon glow, unstable nodes pulse orange, relay nodes emit amplifier ticks, corrupted nodes carry red disruption marks, and the selected node receives a white ring. Click a node to apply the selected action, or use End Turn to let corruption act without spending energy. If an action has zero targets, the HUD explains whether the block is energy, reach, ownership, or adjacency; the rail also shows that owned Resource nodes restore energy after corruption resolves and that corruption pressure is enemy pressure for spread or collapse. The HUD Forecast panel previews the next likely corruption pressure if the current board is left alone, identifies urgent unstable nodes before they collapse, and summarizes tactical roles for Resource, Relay, and Firewall nodes. After win or loss, that panel becomes an After Action guide that names the loss cause, last collapse or corruption hit, and a replay suggestion for the same seed. Hover nodes to inspect ownership, type, integrity, threat, cost, expected action result, blocked reason, instability, and danger reason. After enemy turns, the board emphasizes the enemy source and target so the player can read the pressure path and intent. The board uses layered background motion, active link packets, hover/selection easing, capture and corruption shockwaves, relay packet amplification, invalid-action shake, objective pulses, screen-level mission-result emphasis, a layer cube inset, and synthesized digital audio cues with ambient intensity that reacts to mission pressure. Replay resets the current seed deterministically. New Seed rolls a different generated network during an unfinished mission; after a win or loss, it loads the next campaign trace from persisted seed history. Save and Load persist or restore the active trace in the selected local slot; F1, F2, and F3 select among three slots. Esc returns to the Operations screen. Layer focus can show all layers or dim the board around one layer with `[` and `]`.

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

## Milestone 8 Visual Identity Redesign

- The visual identity redesign itself is presentation-only and does not change player mechanics, tactical AI, balance, mission generation, layer tuning, full cube-face gameplay, or profile progression.
- The chosen artistic direction is fiber-optic infrastructure under live intrusion; see `docs/visual-style-guide.md`.
- The network viewport emphasizes bundled fiber routes, pale signal cores, diamond packets, relay amplifier cells, distant junction activity, and subdued infrastructure motion.
- Nodes use shape-first silhouettes: octagonal player core, hexagonal standard junctions, diamond resources, triangular relays, shielded firewalls, hexagonal objective keystones, and red corruption fractures.
- Camera movement continues to use smoothed target following with inertial pan/zoom/recenter behavior.
- The HUD is styled as an attached lattice diagnostics rail with compact action controls, signal status, objective progress, enemy profile, and recent event history so the network remains dominant.
- `AudioService` keeps the committed synthesized cues and modulates the ambient network hum based on corruption pressure and objective progress.

## Current Mechanics

- The default prototype board is a 4x4 orthogonal node grid defined by `BoardDefinition.CreateDefaultPrototype()`.
- The core engine can initialize authored grids or explicit generated graph topologies from `BoardDefinition`.
- `ProceduralMissionGenerator` creates deterministic missions from integer or text seeds using configurable node count, graph density, branching, corruption start count, objective distance, node type frequencies, starting energy, and objective hold settings.
- Generated missions include explicit links, generated layout positions, seed metadata, player start, corruption starts, objective placement, Resource placement, Relay placement, and Firewall placement.
- `CampaignProgressionPlanner` selects deterministic generated stages from completed seed history inside the authored `Signal Recovery` arc. Wins advance to larger and harder traces; losses stay at the current stage with a small recovery cushion and recovery briefing.
- The MonoGame frontend persists completed seed runs locally and uses that history for the next campaign trace.
- `ProfileProgression` turns completed campaign runs into local level, XP, title, win/loss, best-stage, and streak progress; the MonoGame HUD shows the current profile line.
- `NetworkGame.CreateSnapshot()` and `NetworkGame.RestoreSnapshot()` preserve active mission state, and the MonoGame frontend exposes that through Save and Load controls with three local slots.
- `ScenarioCatalog` and `BalanceScreening` provide curated regression scenarios, generated-seed screening, and personality/pressure matrix coverage for balance regressions.
- `BoardDefinition.NodeLayers` assigns nodes to explicit topology layers and `BoardDefinition.TransitionLinks` identifies links that move between layers.
- `GameConfiguration.LayerModifiers` can tune layer-specific integrity, threat, and corruption resistance; the campaign planner uses this to make deeper generated layers more dangerous without adding a new player action.
- The MonoGame frontend can focus all layers or a single layer, dimming out-of-focus nodes and links without changing gameplay rules.
- The MonoGame frontend includes a code-drawn layer cube inset that summarizes layer slices, cross-layer transitions, ownership pressure, and objective location.
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
- See `docs/visual-style-guide.md` for the Milestone 8 visual language.

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

- Retired frontend files are no longer part of the active repository.
- The code-drawn MonoGame presentation is the active art direction: a fiber-lattice network with procedural motion, shape-first node language, reactive lighting, diagnostic UI, and synthesized audio.
- Procedural generation and campaign progression now have authored arc scaffolding over generated missions, branch-aware Operations presentation, and recovery/advance routes, but not cinematic story scenes or bespoke mission scripting yet.
- Network integrity, threat formulas, generated mission settings, alternate board definitions, and tactical AI personality weights now have deterministic curated-scenario, generated-seed, and personality/pressure matrix screening, but still need human playtesting for feel and final tuning.
- Layer assignments, transition links, layer-specific risk tuning, layer focus navigation, local profile progression, a cube-inspired layer inset, and a broader Operations-to-mission UI flow exist, but there is no cloud sync or full cube-face gameplay yet.
