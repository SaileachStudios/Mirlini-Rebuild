# Phase 1 — Stabilize the production foundation

Completed source work: 15 September 2026. Production repository: Mirlini-Rebuild. Phase 2 has not started.

## Result and readiness

The requested foundation changes are implemented, with **51 standalone checks passing** and a successful source compilation of runtime, editor and tests. The scene's 480 wall placements and completion-flow wiring were also checked directly from resolved YAML.

**Ready for Phase 2 design review, conditionally ready for representative migration.** Before treating the foundation as verified in-engine, run the full Unity suite and smoke-test Sandbox with the recorded **6000.3.11f1** editor. That editor was not available in the standard installed Hub directory. No editor was launched and no Unity upgrade was performed. Android sensor behavior and collision feel remain pending. Do not mistake the standalone/compiler checks for Unity test results.

## Baseline and scope

- Started on `main`, HEAD `4e520d2fd024697593b141365bee13101caae156`. The only pre-existing untracked content was the previous `Docs/Audits` report package; it is not included in Phase 1 commits.
- ProjectVersion remains `6000.3.11f1`. Available newer reference assemblies from `6000.6.0f1` were read for compilation only. Project settings/packages were not upgraded.
- Mirlini-Original was not modified. No original levels were migrated.
- Corrected audit detail: Sandbox configures two levels, but the repository contains **three** prototype assets: Level 1, Level 2 and Test 1. All three now use the shared coordinate convention. Their wall occupancy is unchanged.
- No new menu, help button/binding, scoring formula, saves, analytics, monetization or generalized modifier framework was added.

## A. Changes and rationale

### Input and sensor ownership

`InputRange.Clamp` preserves vector magnitude below one and clamps only oversized input. Device modifiers still execute in the providers before that clamp. Keyboard's existing multiplier and axis conventions remain unchanged; touch and gyro sensitivity now retain meaning. Invalid non-finite input is rejected before reaching physics.

`UnityInputWrapper.GyroEnabled` now writes to the real sensor. `GyroInputProvider` implements an idempotent activation/deactivation contract. GameManager owns activation while enabled and not application-suspended, and deactivation on disable, destruction or application suspension. Sensor enabling does not occur in input polling. No gyro-axis retuning was done.

### Explicit initialization and event ownership

GameManager initializes its provider in Awake with an explicit earlier execution order. Marble initializes its body, controller and state machine in Awake and remains inert until LevelManager explicitly binds it and starts the selected level. Gameplay no longer depends on relative Start ordering.

Marble, camera tilt, audio and completion flow detach from the exact event source to which they subscribed. The hole no longer relies on a global visual-only status subscription: its explicit state-setting API updates controller correctness and indicators together. No event framework replacement was introduced.

External/menu pause, application suspension and accepted hole-resolution pause are separate reasons. Finishing a respawn clears only its own resolution reason; it cannot clear a menu/app pause.

### Hole lifecycle and completion

Only a playable, unpaused marble can accept a hole entry. The accepted outcome and destination are frozen for the entire shrink/resolve sequence; duplicate entries return false without replacing them. Rigidbody simulation is suspended during shrink/growth and restored on playability. Correct completion is still published after shrink. The fully playable collider radius is retained for next-level validation even while the visual marble is scaled to zero.

Sandbox now explicitly serializes one enabled `LevelEndFlowController`, referencing its LevelManager. No runtime component repair was reintroduced. A preview-scene contract test checks this wiring and shared wall geometry.

The existing short automatic next-level delay remains temporary scaffolding. Results UI and full session flow remain later work.

### Setup validation

LevelManager validates the selected asset and dependencies before changing geometry, positions, wall activation or current-level index. Checks include valid array length, unique wall references, collider conventions, active boundaries/floor, marble readiness, goal indicators/trigger, finite positions, board containment and start/goal clearance from configured walls.

Invalid configuration returns false with a clear error rather than applying a partial level. Normal levels are the supported Phase 1 content; unsupported special types are rejected instead of silently pretending their mechanics work. The serialized LevelData shape is otherwise retained.

### Automatic stuck behavior removed; Call for Help added

Removed the Stuck state, movement heuristic, timer polling and automatic respawn path. Existing shrink/grow for failed holes remains.

`LevelManager.TryCallForHelp()` is the public UI-free action. It is rejected unless the marble is enabled, Playing, unpaused and outside any hole-resolution coroutine. There is no button or binding yet.

The pure `HelpPositionPlanner` searches deterministic candidate positions against radius-expanded wall rectangles and board limits. It selects the closest legal point, with a stable X/Z tie-break. Clearance uses the SphereCollider's fully playable world radius plus configurable `helpSafetyMargin` (default 0.05 world units). Live wall/boundary collider bounds are used; the goal footprint is excluded. The search is bounded by a configurable maximum displacement, default two cells (3.6 world units). If no safe nearby point exists it returns false without moving the marble. A position already safe stays in place.

The rectangle expansion is intentionally conservative near wall corners. This gives the nearest solution within the conservative clearance model, not the exact Euclidean shortest distance around a rounded collider corner. The solver is suited to the standardized axis-aligned, flat production board; it is not a navigation system for arbitrary moving obstacles or ramps. It does not promise to stay on the same side of an obstructing wall. A short relocation can cross a thin wall if that is the nearest legal recovery point; it never picks a hole or an out-of-board point.

Successful help clears linear and angular velocity. It preserves the level index, ball state and all attempt state, does not invoke setup/reset, does not toggle goal/reveal state, and adds no pause or penalty. Vertical position is retained unless a small upward correction is needed to clear the floor. Actual future reveal/unlock implementations are not present yet, so their preservation follows from the absence of reset calls and still needs combined-feature integration tests in Phase 2.

## B. Files changed, grouped by system

| System | Files and responsibility |
|---|---|
| Shared geometry/help | `Assets/Scripts/Board/BoardGrid.cs`, `BoardGeometryPlacement.cs`, `HelpPositionPlanner.cs`, `LevelDataValidation.cs`: pure topology, physical conventions, Unity placement adapter, legal-position solver and validation |
| Runtime board | `LevelManager.cs`, `LevelEndFlowController.cs`, `HoleBehavior.cs`, `BoardTiltBehavior.cs`: safe setup, authored flow ownership, goal correctness and listener cleanup |
| Marble | `Assets/Scripts/Marble/BallState.cs`, `BallStateMachine.cs`, `MarbleBehaviour.cs`, `MarbleController.cs`: remove obsolete state, initialize explicitly, accept one hole outcome and expose guarded recovery |
| Input/manager | `Assets/Scripts/Input/InputRange.cs`, `IInputLifecycle.cs`, `IInputProvider.cs`, `GyroInputProvider.cs`, `UnityInputWrapper.cs`; `Core/GameManagerBehavior.cs`: magnitude contract, sensor lifecycle, distinct pause reasons |
| Audio | `Assets/Scripts/Audio/AudioSFXBehavior.cs`: symmetric event subscription; audio polish remains out of scope |
| Authoring/content | `Assets/Editor/LevelEditorWindow.cs`, `Board/LevelData.cs`, `Assets/Scenes/Sandbox.unity`, and the three prototype assets in `Assets/Levels`: common geometry, finer point editing, Undo, alignment and unset baseline |
| Tests | Existing state/marble/flow fixtures updated; new geometry/help/input/lifecycle/scene/validation fixtures and `SandboxPlayTest` lifecycle fixture |
| Verification/documentation | `Tools/Phase1Checks` standalone/compiler harness; `Docs/Phase1` report, test inventory and captured results |

New C# assets include stable `.meta` GUIDs. Existing scene/asset GUIDs are preserved.

## C. Tests and checks

### Added

- 6 BoardGrid tests: all 480 edges unique and round-trippable; known index landmarks; invalid edges; nearest edge snap; fine point placement; dimensions/boundaries.
- 8 help-planner tests: already-safe position, nearest displacement, stable ties, boundary/corner constraints, no nearby solution, fully blocked board, radius/margin and invalid inputs.
- 5 input-range tests: analog magnitude, saturation, touch/gyro modifiers, keyboard and non-finite values.
- 1 gyro-lifecycle test: no sensor writes in construction/polling, idempotent activation and deactivation, reactivation.
- 3 EditMode contract/data tests: authored Sandbox flow/geometry, valid prototype data with unset scoring baseline, malformed level data.
- 3 Unity-facing setup-validation tests: invalid index, missing wall and missing goal leave the level untouched.

### Changed or removed

- Removed 3 obsolete Stuck enum/transition tests; retained 15 ball-state tests.
- Replaced the previous 8 marble tests, including the 2 automatic-stuck heuristic cases, with 6 realistic lifecycle scenarios: startup, duplicate hole entry, failed-hole pause preservation, guarded help/velocity clearing, sustained input without automatic respawn and disabled-listener cleanup.
- Reworked the 3 completion-flow tests to load the actual Sandbox, including disabled and re-enabled listener cases.
- Touched integration tests no longer reflectively invoke Awake/Start/OnEnable. Their fixture enters Play Mode using Unity Test Tools and loads Sandbox through the editor's Play Mode scene-loading API.

The final source inventory contains **79 `[Test]` methods and 12 `[UnityTest]` methods** (91 total). This is a method count, not a coverage percentage.

### Actually executed

1. **51 standalone test methods passed, zero failed.** The actual NUnit methods for geometry, help, ball states and input were invoked by the local .NET harness. The geometry/help/state code is engine-independent; input tests use managed Unity vector types and mocked device wrappers. No sensor or native Unity runtime was exercised. See `standalone-results.txt`.
2. **All runtime/editor/test sources compiled with zero errors and zero warnings** against locally installed 6.6 reference assemblies. This is a secondary source/API check, not a recorded-editor build. See `compile-results.txt`.
3. Resolved Sandbox YAML: checked all 480 authored positions against the chosen grid, four boundary assignments, the completion-flow component/reference, and the prototype point positions against wall clearance. Original occupancy arrays were retained.
4. Git/diff and metadata checks: limited write set, no recorded-version change, no original-repository edits. Prior audit artifacts were preserved.

### Not executed

- Unity's full EditMode suite, its Play Mode integration scenarios, a player build and visual/physics smoke tests: exact `6000.3.11f1` editor unavailable in the standard installation directory.
- The remaining 40 test methods were compiled but not executed by the standalone harness. This includes Unity scene/lifecycle tests and other existing controller tests.
- Android sensor activation, suspend/resume, orientation and physical-device feel: no device run.

## D. Remaining risks

1. **Exact-editor import and execution are the main release gate.** The new tests must run in 6000.3.11f1, especially startup, disabled/re-enabled listeners, hole animations and next-level setup.
2. **Geometry needs playtesting.** Standardized origin, join overlaps and boundaries slightly shift prototype geometry. Confirm corner contacts, clearances, camera framing and maximum-speed collisions in Sandbox before migration.
3. **Help is a technical API, not finished UX.** Conservative corner clearance and bounded relocation need practical testing; failure returns false for the future UI to explain. No penalty was added.
4. **Future mechanics integration remains necessary.** Unlock/reveal state is not implemented; future feature code must not subscribe to help as an attempt reset.
5. **Pause/session ownership remains deliberately small.** Full menu/game-flow state and timer will come later. Their pause reason must remain separate from hole resolution.
6. **No star targets are calibrated.** Original ideal times are historical only; production scoring and reference-runner work are deferred.
7. **Source validation is not build readiness.** Build scene/profile setup and release pipeline remain later roadmap items.

## E. Authoritative production geometry

`BoardGrid` is the sole source for topology and geometry constants. Runtime placement, editor display/snap, asset validation and the future importer API consume it. There is no sparse historical-geometry override system.

| Property | Definition |
|---|---|
| Logical board | 16 × 16 cells, centered on (0,0), X/Z range −8 to +8 |
| Physical cell spacing | 1.8 world units per logical unit |
| Internal edges | 480: 240 along X and 240 along Z |
| Index order | Each descending-Z row has 15 Z-parallel edges followed by 16 X-parallel edges; the last row has only 15 Z-parallel edges. `GetEdge`/`TryGetEdgeIndex` are authoritative. |
| Index landmarks | 0: `(7,7.5)` along Z; 15: `(7.5,7)` along X; 479: `(−7,−7.5)` along Z, all logical coordinates |
| Internal wall | Length 2.2, thickness 0.4, height 1.5 world units; deliberate 0.4 join overlap |
| Floor and wall height | Floor Y −0.5; wall bottom at floor, center Y 0.25, top Y 1.0 |
| Boundaries | Inner faces at X/Z ±14.4; thickness 0.4 outward; centers ±14.6; outer board footprint 29.6 × 29.6 |
| Boundary order | East, North, West, South |
| Board transform | Translation allowed; identity rotation and unit scale required in Phase 1. Camera tilt remains visual. |
| Point authoring | Half-unit logical snap by default: 0.9 world-unit increments. Start/goal support numeric unsnapped coordinates as an escape hatch. The same conversion/snap API is available for future unlock rings. |
| Serialized point storage | Existing Vector3 fields remain board-local physical coordinates, with Y 0. No LevelData schema replacement yet. |
| Editor relationship | Wall controls query shared edge poses; point fields convert through shared logical/physical functions. Undo records edits. Malformed arrays require explicit repair. |

Why 1.8: it matches the existing Sandbox's dominant spacing while removing its small offsets and disagreement with the old 1.824 editor spacing. The existing 2.2 × 0.4 internal-wall footprint is retained. Normal adjacent-wall corridor width is 1.4, leaving 0.4 clearance beyond a diameter-one marble before the configurable help margin. This is a coherent starting convention, not a claim of finalized gameplay feel.

## F. Next review gate

Review this Phase 1 implementation, run the exact-editor checks and inspect Sandbox. Then proceed, by a separate instruction, to **Phase 2: Production LevelData schema + representative level migration**. Phase 2 should standardize accidental source offsets/duplicates and unnecessary corner posts rather than preserve them with exception machinery. Star baselines should remain Needs Calibration.
