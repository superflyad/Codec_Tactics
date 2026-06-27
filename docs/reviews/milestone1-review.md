# Milestone 1 Review

Date: 2026-06-14

Scope: senior engineering review after Milestone 1 completion. This review covers architecture, code quality, test quality, documentation, scalability, frontend integration risk, and likely breakpoints for future layer and cube work. No gameplay behavior was changed.

## Overall Assessment

Milestone 1 establishes a solid foundation for a deterministic, testable network tactics core. The most important architectural choice is good: gameplay rules currently live in a pure C# project instead of frontend scene code. That keeps the prototype easy to validate and gives future rendering work a clean model to consume.

The main risk is that the current model is still shaped around a fixed 2D grid and a compact all-in-one `NetworkGame` coordinator. That is reasonable for Milestone 1, but it will become fragile as soon as layers, scenario rules, richer enemy behavior, and frontend UI feedback arrive.

Risk rating: medium.

## Strengths

- The core domain is isolated in `src/CodecTactics.Core`, which is the right boundary before frontend integration.
- The rules are deterministic, making tests straightforward and repeatable.
- `NodeId`, `NodeState`, `ConnectionState`, `NetworkBoard`, and `NetworkGame` are small enough to reason about.
- The test project has no third-party dependency, which keeps validation lightweight.
- `scripts/validate.ps1` gives contributors a single repeatable local check.
- Documentation clearly distinguishes implemented Milestone 1 behavior from future layers, cube visualization, advanced AI, save/load, and polish.
- The architecture notes already state that the cube should present core state rather than own game rules.

## Weaknesses

- `NetworkGame` currently owns player action validation, state mutation, enemy turn policy, outcome evaluation, turn progression, and default scenario setup. This is the biggest near-term source of technical debt.
- `NetworkBoard.CreateGrid` is the only board construction path. The core still assumes rectangular 2D coordinates, width, height, and orthogonal adjacency.
- `NodeId` only stores `X` and `Y`, so adding layers will require either changing a core identity type or adding parallel identity concepts later.
- `NodeState` is mutable and exposes rule-changing methods directly. This is fine for a prototype, but frontend UI and future systems could accidentally mutate game state outside an intended command flow.
- Enemy spread is implemented as inline LINQ inside `NetworkGame`. It is deterministic, but not an explicit strategy or policy.
- Outcome rules are placeholders but are not separated from the turn engine, so real scenario win/loss rules may be awkward to add.
- Connection lookup is linear over all connections. This is acceptable for 16 nodes, but it will not scale cleanly to dense layers or cube-like topology.
- The tests prove happy-path mechanics but do not deeply cover invalid actions, outcome transitions, exhausted boards, disconnected links, or mutation boundaries.
- frontend integration is not validated beyond project metadata. `retired project metadata` has no main scene and `scenes/` only contains `.gitkeep`.

## Technical Debt

- All game flow is concentrated in `NetworkGame`.
- Default scenario constants are mixed into the general game type.
- Board generation and board topology are coupled.
- Node identity is 2D-only.
- There is no command/result model for player actions, so UI will only receive `bool` success/failure rather than actionable feedback.
- There is no event, log, or turn summary object describing what changed during a move.
- No serialization boundary exists for future save/load, replay, editor debug tools, or frontend scene hydration.

## Unnecessary Complexity

- The current codebase is generally simple. The one place that feels more complex than necessary is enemy expansion query composition in `NetworkGame.ResolveEnemyTurn`. The behavior is small, but the LINQ pipeline hides the policy details that future AI work will need to expose.
- `WeakenEnemyConnection` encodes reachability, enemy adjacency, connection lookup, action cost, and turn completion in one method. It is not overbuilt today, but it is already mixing concerns that will likely split soon.

## Missing Abstractions

- A topology abstraction for graph/layer/cube-compatible adjacency instead of direct grid construction.
- A scenario or ruleset abstraction for starts, board shape, win/loss conditions, and enemy policy.
- A player command abstraction that can return structured failure reasons and changed-state summaries.
- An enemy spread policy abstraction to keep deterministic AI testable while allowing richer behavior.
- A read-only game snapshot or DTO boundary for frontend UI rendering.
- A turn result/event model for UI animation, combat logs, replays, and debugging.

## Test Quality

The current tests are appropriate for proving Milestone 1 did not regress, but they are still closer to smoke tests than durable gameplay-contract tests.

Missing tests:

- Claiming an enemy-owned node fails.
- Claiming a neutral node through a weakened or inactive connection fails.
- Reinforcing neutral or enemy nodes fails and does not advance the turn.
- Weakening a non-enemy connection fails.
- Weakening an unreachable enemy connection fails.
- Weakening an already inactive connection fails.
- Invalid actions do not trigger enemy spread, phase changes, outcome changes, or turn increments.
- Outcome transitions for player win and player loss.
- Enemy spread behavior when multiple enemy fronts exist.
- Enemy spread behavior when no neutral adjacent target exists.
- `NetworkBoard.CreateGrid` argument validation.
- `ConnectionState` ordering, duplicate-equivalent lookup, and non-negative strength rules.
- Integrity behavior beyond a single reinforcement.

Recommended test improvements:

- Keep the console runner for now, but group tests by domain area as the file grows.
- Add helper methods for common game setup to keep future layer scenarios readable.
- Start testing state deltas, not just final ownership, before legacy frontend animation depends on move summaries.
- Add tests for negative paths before expanding the action set.

## Documentation Quality

The docs are clear and honest about current scope. `README.md`, `docs/architecture.md`, `docs/game-design.md`, `docs/milestones.md`, and `docs/decisions.md` align well on Milestone 1 status.

Documentation gaps:

- No review or risk log existed before this document.
- No explicit domain glossary for node, connection, integrity, layer, cube, corruption, and exposure.
- No planned public API contract for what legacy frontend should consume from the core.
- No examples of expected turn summaries or UI-facing state snapshots.
- Milestone 2 in `docs/milestones.md` overlaps with behavior already present in Milestone 1: enemy spread and basic turn progression. The milestone text should be clarified before planning the next task.

## Future Scalability

Performance risk rating: low for Milestone 1, medium for layers/cube.

Current linear scans are fine for a 4x4 board. They may become a problem when board size, dense links, layered visibility, enemy pathing, or repeated UI previews grow.

Likely performance concerns:

- `FindConnection` scans every connection.
- `GetAdjacentNodes` scans every connection for each adjacency request.
- Enemy spread repeatedly sorts and traverses node and adjacency collections.
- No cached adjacency map exists.
- No distinction exists between command-time mutation and read-time preview queries.

Recommended scalability direction:

- Add an adjacency index keyed by `NodeId` or a future generalized node identity.
- Keep deterministic ordering explicit in the topology rather than sorting repeatedly at call sites.
- Add read-only snapshots for UI rendering and previews so legacy frontend does not repeatedly query mutable internals.
- Avoid optimizing prematurely, but set the topology boundary before implementing layers.

## Layer Implementation Risks

Risk rating: high if layers are added directly to the current types; medium if topology and identity are generalized first.

Areas likely to break:

- `NodeId` only supports `X` and `Y`.
- `NetworkBoard.Width` and `NetworkBoard.Height` imply a single rectangular plane.
- `CreateGrid` owns topology construction and has no concept of layer entry/exit points.
- `DefaultPlayerStart` and `DefaultEnemyStart` are hard-coded 2D coordinates.
- `HasAdjacentOwner` cannot distinguish same-layer adjacency from inter-layer transitions.
- Enemy spread has no rule hook for layer-specific risk, visibility, or traversal costs.
- Outcome evaluation treats the whole board as one flat space.

Recommended fixes before layer work:

- Introduce a topology model that can describe nodes and links without assuming a 2D grid.
- Decide whether identity becomes `NodeId(x, y, layer)` or a stable opaque ID plus coordinates metadata.
- Move default starts and board shape into a scenario definition.
- Add tests for cross-layer links before adding visual layer navigation.

## Cube Implementation Risks

Risk rating: medium now, high if cube visuals start before the core exposes stable snapshots.

Areas likely to break:

- Cube visualization will need spatial metadata that is not currently represented in the core.
- If frontend scenes bind directly to mutable `NodeState` objects, cube view state and gameplay state may become coupled.
- The core has no view model or projection layer for mapping gameplay topology to 2D or cube coordinates.
- Selection feedback will need structured reasons and available action previews, not just `bool` return values.
- Turn-by-turn animation will need event summaries that do not exist yet.

Recommended fixes before cube work:

- Keep cube rendering outside rule ownership, as `docs/architecture.md` already says.
- Add a core snapshot/projection DTO that can support both 2D and cube views.
- Add an action preview API for valid targets, affected connections, and risk exposure.
- Preserve deterministic ordering independent of visual face orientation.

## Frontend Integration Risks

Risk rating: medium.

The project has legacy frontend metadata but no gameplay scene, main scene, C# frontend scripts, or editor validation. The current core is ready to be consumed by legacy frontend, but the integration path is not proven.

Specific risks:

- The legacy editor may require project or assembly settings that are not covered by the current .NET-only validation.
- The core targets `net8.0`; compatibility should be verified against the retired editor .NET version used locally.
- Without a main scene, validation cannot catch scene loading, script binding, input, or rendering issues.
- UI code may be tempted to mutate `NodeState` directly because the model exposes mutable objects.
- frontend-facing code will need structured turn results for animation and messaging; `bool` action methods are not enough.

Recommended fixes:

- Add the smallest possible frontend scene that renders a read-only board snapshot.
- Keep frontend scripts thin and route gameplay changes through core commands.
- Add optional legacy editor CLI validation once a scene exists, while preserving .NET-only validation for environments without legacy frontend.
- Define one frontend-facing adapter before adding cube or layer UI.

## Recommended Priorities

1. Split scenario setup, player action resolution, enemy policy, and outcome evaluation out of `NetworkGame`.
2. Introduce graph/topology construction that does not assume a rectangular 2D grid.
3. Decide the future node identity model before adding layers.
4. Add negative-path tests and outcome tests around current Milestone 1 behavior.
5. Add structured action results and turn summaries for UI and future animation.
6. Add a read-only snapshot/projection boundary for legacy frontend.
7. Clarify Milestone 2 because basic turn progression and deterministic enemy spread already exist.
8. Add a minimal frontend scene only after the snapshot boundary exists.
9. Add adjacency indexing when topology expands beyond the 4x4 prototype.

## Recommended Fixes

Near term:

- Create `GameScenario` or similar to hold board shape and starting nodes.
- Create an `EnemySpreadPolicy` interface or concrete strategy class.
- Create an `OutcomeEvaluator` for win/loss rules.
- Add `ActionResult` with success, failure reason, changed nodes, changed connections, phase, turn number, and game result.
- Add tests for invalid actions and no-side-effect guarantees.
- Clarify documentation around what Milestone 2 still means.

Before layers:

- Generalize node identity and topology.
- Add explicit layer metadata and inter-layer links.
- Add layer-focused tests before UI work.

Before cube:

- Add read-only snapshots and projection metadata.
- Add action preview APIs.
- Keep cube view code free of gameplay mutation.

## Final Risk Rating

Medium.

The repository is in a healthy state for Milestone 1. The risk is not current instability; it is that the next features will push directly against the current 2D grid assumptions and compact `NetworkGame` design. Addressing the topology, scenario, action result, and snapshot boundaries before layers and cube visualization will prevent most likely breakage.
