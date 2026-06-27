# Playable Vertical Slice

The current vertical slice is one complete authored mission named `Secure the Uplink`. It is intentionally small and uses only existing 2D network rules: claiming, reinforcing, weakening corruption, energy, corruption pressure, instability, collapse, and deterministic expansion.

All mission rules live in `CodecTactics.Core`. The active MonoGame frontend displays state through a deterministic network topology, lets the player choose an action mode, sends clicks to core, and renders the returned feedback. The previous Godot implementation is legacy-only.

## Mission Definition

The mission is created by `MissionDefinition.CreateVerticalSlice()`.

- Board: fixed 5x5 single-layer grid.
- Player start: `(0,2)`.
- Corruption start: `(4,4)`.
- Objective: Firewall node `(3,2)`.
- Required objective hold: 2 turns.
- Starting energy: 6.

Authored node type placement:

- Resource: `(2,1)`, `(1,2)`, `(4,2)`.
- Relay: `(1,1)`, `(2,2)`.
- Firewall: `(3,1)`, `(3,2)`.
- Standard: all other nodes.

## Win and Loss

The player wins when the objective is player-owned after corruption resolves for 2 objective hold ticks.

The player loses when either condition is true:

- The player core at `(0,2)` is no longer player-owned.
- The objective at `(3,2)` becomes corrupted.

Objective hold progress resets to 0 when the objective is not player-owned. Actions after win or loss fail without mutating the board.

## Player Loop

The active frontend exposes four main actions:

- Claim: click a reachable neutral node to claim it for energy.
- Reinforce: click a player-owned node to improve its integrity.
- Weaken: click an adjacent corrupted node to weaken the active corruption link.
- End Turn: spend no energy and let corruption resolve.

Restart Mission creates a fresh deterministic copy of the mission.

One validated route is:

1. Claim `(1,2)` to gain Resource income.
2. Claim `(2,2)` to anchor Relay reach.
3. Claim objective `(3,2)`.
4. Reinforce objective `(3,2)` to complete the hold while corruption presses the lower-right board.

## Feedback

The prototype reports:

- Selected action.
- Hovered node ownership, type, integrity, threat, selected-action cost, valid-result preview, blocked-target reason, instability, and danger reason.
- Current energy and corruption pressure.
- Current objective and hold progress.
- Valid and invalid action results from core.
- Energy spent and Resource income.
- Corruption spread or focus target.
- Collapse events.
- Clear win/loss result.

Unstable player-owned nodes render with a pulsing orange danger ring. The objective renders with a pulsing yellow objective ring. Valid targets use a green outline, invalid targets are dimmed for the selected action, and the board legend decodes concise owner and node-type badges.

Milestone 4 changes the active presentation from grid-first to network-first without changing mission rules. Nodes render at authored visual topology positions instead of square-cell centers. Connections are first-class animated links with ownership and corruption coloration. Core, Resource, Relay, Firewall, Objective, Standard, and Corrupted nodes use distinct silhouettes, iconography, glow, outline, and overlays before text. The camera supports smooth zoom, right or middle mouse panning, and `C` recentering so the board remains the visual focus.

Milestone 5 keeps the same mission and rule set, then improves game feel around the existing interactions. Hovering, selection, invalid clicks, successful actions, ownership changes, captures, corruption focus, collapse, objective progress, victory, and defeat now produce animation, pulse effects, or shake feedback. Active links carry directional packets, Relay links emit amplified packet bursts, corrupted nodes shimmer with an extra disruption ring, and the objective transmits visible progress pulses. Audio playback is centralized in the MonoGame `AudioService` and uses committed synthesized digital WAV assets, including an ambient network hum and specific cues for hover, selection, confirmation, invalid actions, capture, reinforcement, weakening, corruption, objective progress, victory, defeat, and reset.

## Tests

Core tests cover:

- Mission initialization.
- Objective hold win condition.
- Loss when the player core collapses.
- Loss when corruption captures the objective.
- Objective hold reset behavior.
- Claim, Reinforce, and Weaken action mode routing.
- Restart/reset determinism.
- Invalid actions after game over.

## Limitations

- The mission is one authored 5x5 board, not a campaign or procedural generator.
- Balance values are prototype-level and tuned only for this route.
- Visuals are still code-drawn prototype art, not final production assets.
- There are no layers, cubes, save/load, advanced AI, final art, or production UI.
