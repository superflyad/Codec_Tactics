# Milestone 3 Review: Network Integrity and Threat System

Review date: 2026-06-26

## Review Scope

This is a review-only balance-readiness audit of Milestone 3 before adding layers, cube faces, or multiple board sizes. It covers correctness, deterministic behavior, test coverage, readability, Godot/core separation, balance risks, hard-to-tune formulas, and readiness for future topology work.

No mechanics, layers, cubes, behavior refactors, or tuning changes were made as part of this review.

Reviewed files:

- `CODEX.md`
- `README.md`
- `ROADMAP.md`
- `CHANGELOG.md`
- `docs/mechanics.md`
- `docs/network-integrity.md`
- `src/CodecTactics.Core/Network/*`
- `src/CodecTactics.Godot/PrototypeScene.cs`
- `tests/CodecTactics.Core.Tests/Program.cs`

## Summary of Milestone 3

Milestone 3 adds calculated integrity, calculated threat, instability tracking, deterministic collapse into corruption, and deterministic corruption target selection. It successfully makes topology matter: connected clusters are safer, isolated or long-chain expansion is dangerous, Relay and Firewall nodes provide structural value, and corruption pressure escalates predictably.

The current system is a solid 4x4 deterministic prototype. It is not yet balance-ready for layers, cube faces, or arbitrary board sizes because scenario assumptions, targeting constants, and topology meaning are still tightly coupled to the default 2D board.

## Strengths

- Core gameplay remains in `CodecTactics.Core`; Godot reads state and sends actions.
- Determinism is strong. Node ordering, row-major tie breaks, fixed pressure changes, and no random inputs make behavior reproducible.
- The main formulas are documented in `docs/network-integrity.md` and mostly map cleanly to `NetworkIntegrityEvaluator`.
- Invalid player actions fail without spending energy, advancing turns, or resolving corruption.
- Collapse and corruption focus data flow through `GameActionResult`, giving the UI enough information to explain important state changes.
- Tests cover key Milestone 3 paths: integrity, isolation, Relay support, Firewall support, pressure progression, instability, collapse, and deterministic targeting.
- The implementation is still small enough to reason about directly.

## Weaknesses

- `NetworkGame.CreateDefault()` hard-codes the player core at `(0,0)`, corruption at `(3,3)`, and the default 4x4 scenario.
- `NetworkBoard.CreateGrid(width, height)` accepts other dimensions, but special node placement only exists for 4x4 and `NetworkGame` still assumes default starts.
- Corruption targeting uses inline balance values: unstable `+100`, low-integrity anchor `20`, and Firewall priority `-4`.
- `WeakenEnemyConnectionWithResult` charges `NetworkRules.ReinforceEnergyCost`; there is no separately named weaken cost.
- The term `WeakConnectionThreat` means weak ownership support, not weakened `ConnectionState.Strength`, which is easy to misread.
- Stable player-owned nodes can be selected as a corruption focus target but cannot be taken by expansion. That matches the docs, but it can look like corruption stalled unless the message is read carefully.
- Reinforce and weaken actions exist in core but are not exposed in the Godot prototype, so the visible prototype cannot yet play the full Milestone 3 response loop.

## Correctness

The implemented turn flow is internally consistent: successful player actions spend energy, mutate state, resolve the enemy turn, evaluate outcome, start the next player turn, and refresh risk. Failed actions leave the board and turn state unchanged.

The biggest correctness risk is not a current bug; it is an API readiness issue. The board can be created at alternate sizes, but the game and balance model are still effectively authored for one 4x4 scenario. That mismatch will become a bug source if layers, cubes, or scenario selection start using the wider API before configuration exists.

## Deterministic Behavior

Determinism is good for the current slice. `NodeId.CompareTo` gives row-major order, board traversal orders nodes, corruption target selection sorts candidates, and tests compare repeatable ownership results.

The main future risk is that row-major ordering is a 2D grid policy. Cube faces and layers will need an explicit deterministic ordering policy that is stable across face IDs, layer IDs, portals, and generated adjacency.

## Test Coverage

Milestone 3 has meaningful core tests for the implemented happy paths and several failure paths. The tests are deterministic and lightweight.

Missing or thin coverage:

- Alternate board sizes: 2x2, rectangular grids, and larger grids.
- Any `NetworkGame` setup that is not `CreateDefault()`.
- Stable player-owned nodes competing with neutral nodes in corruption targeting.
- Weakened/inactive connection effects on reach, integrity, threat, and corruption pathing.
- A distinct weaken cost test.
- Dense network bonus at exactly three owned adjacent nodes.
- Long-chain penalties across several distances.
- Instability recovery after reinforcement or topology repair.
- Multiple simultaneous collapses and collapse ordering.
- Godot-facing reinforce and weaken workflows.
- Scripted multi-turn balance snapshots for energy, pressure, ownership, instability, and collapse.

## Readability

The code is readable for the current prototype. `NetworkRules` centralizes most constants, `NetworkIntegrityEvaluator` keeps the formula in one place, and `CorruptionTargetPolicy` isolates expansion priority.

Readability will degrade if more modifiers are added to the existing evaluator without decomposition. Integrity, threat, danger text, distance checks, instability advancement, and collapse collection currently share one pass. That is acceptable now, but layers and cube faces will add enough context that separate named steps or a risk breakdown object would help.

## Godot/Core Separation

Separation is healthy. Godot does not compute integrity, threat, collapse, claim reach, energy, or corruption spread. It renders core state and calls core actions.

The visible prototype is behind the core mechanically: it exposes claiming and end turn, but not reinforce or weaken. That is not a separation violation, but it does block balance assessment because players cannot use two of the core responses to danger.

## Technical Debt

- Scenario configuration is missing for board size, starts, node types, and rules.
- Balance values are split between named `NetworkRules` constants and inline targeting literals.
- Outcome rules are placeholders and may misrepresent success/failure on non-default topologies.
- `NodeState.SetOwner` preserves the prior danger reason when a node becomes player-owned until the next risk refresh.
- The model is coordinate-first. Cube faces need graph/topology concepts that do not depend on only X/Y adjacency.
- There is no scripted balance harness for comparing the same scenario after formula changes.

## Balance Constants

| Constant or value | Current value | Source | Readiness concern |
|---|---:|---|---|
| Initial player energy | 5 | `NetworkRules.InitialPlayerEnergy` | Works for 4x4; may not scale to longer scenarios. |
| Claim cost | 2 | `NetworkRules.ClaimEnergyCost` | Expansion is expensive but Resource capture can refund quickly. |
| Reinforce cost | 1 | `NetworkRules.ReinforceEnergyCost` | Cheap response that may dominate once available in UI. |
| Weaken cost | 1 | Uses `NetworkRules.ReinforceEnergyCost` | Needs its own named constant before tuning. |
| Resource income | 2 per owned Resource per new player turn | `NetworkRules.ResourceEnergyPerTurn` | Can snowball on larger maps if Resource density scales up. |
| Relay claim range | 2 active connections | `NetworkRules.RelayClaimRange` | Strong on sparse maps; may be mandatory across layers. |
| Corruption pressure increase | +1 each enemy turn | `NetworkGame.ResolveEnemyTurn` | Predictable but not scenario-scaled. |
| Standard corruption resistance | 1 | `NetworkRules.StandardCorruptionResistance` | One pressure spreads into normal neutral nodes. |
| Firewall corruption resistance | 2 | `NetworkRules.FirewallCorruptionResistance` | Only one turn stronger than standard as a neutral obstacle. |
| Base integrity | 4 | `NetworkRules.BaseNetworkIntegrity` | Several bonuses can exceed the base quickly. |
| Core connection bonus | +3 | `NetworkRules.CoreConnectionIntegrityBonus` | Makes one core the dominant safety anchor. |
| Isolation integrity penalty | -4 | `NetworkRules.IsolationIntegrityPenalty` | Very severe; isolated nodes usually clamp to 1. |
| Relay integrity support | +2 | `NetworkRules.RelayIntegritySupport` | Relay combines reach and defense, increasing centrality. |
| Firewall integrity support | +3 | `NetworkRules.FirewallIntegritySupport` | Strong when owned, modest when neutral. |
| Adjacent owned support | +1 per adjacent player-owned node | `NetworkRules.AdjacentSupportIntegrityBonus` | Good cluster incentive; topology-dependent. |
| Dense network bonus | +2 at 3+ adjacent player-owned nodes | `NetworkRules.DenseNetworkIntegrityBonus` | Only some grid positions can earn it; cube topology may skew value. |
| Long-chain penalty | -1 per core distance after first step | `NetworkRules.LongChainDistancePenalty` | Needs a clear layer/cube distance definition. |
| Adjacent corruption threat | +4 per adjacent enemy node | `NetworkRules.NearbyCorruptionThreat` | Strong and clear immediate danger. |
| Corruption pressure threat | pressure / 2 rounded down | `NetworkRules.CorruptionPressureThreatDivisor` | Slow global danger signal; may hide pressure until late. |
| Weak ownership support threat | +2 at 0 or 1 adjacent player-owned nodes | `NetworkRules.WeakConnectionThreat` | Name reads like connection strength, but means ownership support. |
| Frontier exposure threat | +1 per adjacent neutral node | `NetworkRules.FrontierExposureThreat` | Larger maps may impose a constant frontier tax. |
| Isolation threat penalty | +4 | `NetworkRules.IsolationThreatPenalty` | Pairs with integrity penalty for fast instability. |
| Collapse threshold | 2 unstable enemy turns | `NetworkRules.InstabilityTurnsBeforeCollapse` | Readable on 4x4; may be too fast for remote layered threats. |
| Unstable target priority | +100 | Inline in `CorruptionTargetPolicy` | Hard override; should be named. |
| Low-integrity target priority | max(0, 20 - integrity) | Inline in `CorruptionTargetPolicy` | The `20` anchor is unexplained and brittle. |
| Threat target priority | + current threat | Inline in `CorruptionTargetPolicy` | Good reuse, but target priority is not independently tunable. |
| Firewall target priority modifier | -4 | Inline in `CorruptionTargetPolicy` | Duplicates defensive identity outside resistance/integrity. |
| Initial connection strength | 2 | `ConnectionState` constructor default | Weaken requires two uses to break a fresh link, but this is not surfaced in docs as a balance constant. |

## Balance Risks

- Relay nodes provide both reach and integrity. On layered maps, that may make Relay capture mandatory rather than situational.
- Resource income scales with count while costs remain flat. Larger boards can turn Resource density into runaway energy.
- Firewall identity is split: strong owned support, modest neutral resistance, and a separate targeting penalty.
- Two unstable enemy turns is understandable, but it may be too punishing when threats are off-screen, on another layer, or on another cube face.
- Corruption pressure grows at the same rate regardless of board size or scenario length.
- Dense local bonuses depend heavily on graph degree. Cube edges and portals could accidentally create over-safe or under-safe regions.
- Stable player nodes cannot be directly corrupted by expansion, so the system relies on instability/collapse to convert player territory. That is readable, but it reduces the threat vocabulary.

## Hard-to-Tune Formulas

- Integrity and threat mix topology, ownership, node type, pressure, distance, and reinforcement in one evaluator without a per-factor breakdown.
- Corruption targeting combines an absolute unstable override, an arbitrary low-integrity anchor, current threat, and Firewall penalty in one priority number.
- Nearby corruption uses fixed distance bands for a 2D grid. Layer and cube distances may need different bands.
- Frontier exposure counts neutral neighbors equally. On boards with higher degree, this becomes a topology tax rather than a strategic signal.
- Dense network support uses a fixed 3+ adjacent-owned threshold. That threshold is board-topology-specific.

## UX and Readability Risks

- `I#/T#` labels are compact but not self-explanatory for non-developer playtesting.
- Danger reasons are useful but dense and debug-like.
- There is no preview that says a node will collapse on the next enemy turn.
- Stable corruption focus without spread can feel like nothing happened.
- Reinforce and weaken are missing from the visible prototype, so players cannot act on all displayed risk.
- Larger boards will likely cause text overlap unless the display model changes.

## Risks Before Adding Layers

- Layer distance is undefined for integrity penalties, Relay reach, nearby corruption, and frontier exposure.
- Start positions and layer transitions are not configurable.
- Corruption pressure is not scaled by layer count or layer depth.
- The current evaluator assumes one player core and one connected safety concept.
- The UI cannot yet communicate off-layer danger or collapse countdowns.

## Risks Before Adding Cube Faces

- `NodeId` is only `(X,Y)`; cube faces need at least face identity or a topology node key.
- Row-major tie breaking is not enough for multi-face deterministic ordering.
- Orthogonal grid adjacency does not describe cube seams, portals, or wrapped edges.
- Dense bonus and frontier exposure will change meaning at face edges.
- The Godot prototype is 2D debug drawing and does not establish how cube-face state remains readable.

## Risks Before Adding Multiple Board Sizes

- Non-4x4 boards receive no special node placement.
- Default start positions may be invalid or poorly balanced on small or rectangular boards.
- Resource, Relay, and Firewall density has no scaling rule.
- Fixed pressure, costs, and collapse threshold may not match longer maps.
- Tests do not yet assert alternate-size behavior or supported-size boundaries.

## Recommended Fixes

1. Add scenario configuration for board size, start positions, node types, and rule presets.
2. Move targeting literals into named constants or a tunable targeting rules object.
3. Add `WeakenConnectionEnergyCost` and use it in code, docs, and tests.
4. Rename or document `WeakConnectionThreat` as weak ownership support threat.
5. Add alternate-board tests and explicitly declare which board sizes are supported.
6. Add inactive-link tests for claim reach, integrity, threat, and corruption pathing.
7. Add instability recovery tests.
8. Add multiple-collapse tests.
9. Expose reinforce and weaken in the Godot prototype before balance playtesting.
10. Add a scripted multi-turn balance snapshot suite for the default scenario.

## Recommended Next Milestone

The next milestone should be Milestone 3.25: balance-readiness hardening.

Suggested scope:

- Scenario configuration.
- Named targeting and weaken-cost constants.
- Alternate-size and inactive-link tests.
- Instability recovery and multi-collapse tests.
- Visible reinforce and weaken controls.
- A small scripted balance suite for the 4x4 scenario.

After that, Milestone 3.5 Layers can build on a clearer and more testable rule model.

## Go/No-Go Recommendation for Adding Layers

No-go for layers right now.

Milestone 3 is deterministic, understandable, and valuable, but it is still tuned and structured as a single 4x4 scenario. Adding layers now would stack new topology, distance, balance, and UX complexity on hard-coded assumptions.

Go condition: add layers only after scenario configuration, named targeting constants, alternate-size tests, inactive-link risk tests, instability recovery tests, and visible reinforce/weaken player responses are in place.
