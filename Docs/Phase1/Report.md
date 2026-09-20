# Phase 1 â€” Stabilize the production foundation

Completed source work: 15 September 2026. Exact-editor closeout: 20 September 2026. Production repository: Mirlini-Rebuild. Phase 2 has not started.

## Result and readiness

**Phase 1 complete with non-blocking follow-up items; ready for Phase 2 review.** Unity **6000.3.11f1 (3000ef702840)** compiled/imported the project and passed the full suites: **79/79 EditMode and 14/14 PlayMode**, zero failures, skips or inconclusive tests. These are actual Unity runs, including Sandbox physics/lifecycle tests, not the earlier standalone harness. Captured results are in `Verification/EditMode.xml` and `Verification/PlayMode.xml`.

The user's exact-editor manual smoke test found Sandbox healthy, with no obvious wall snagging. Movement felt slightly slow; tuning is deliberately deferred. Help has no UI and was verified through its actual Unity API instead. Android sensor/device verification and player builds remain separate follow-ups. No Unity upgrade, geometry/feel tuning or Phase 2 migration occurred.

## Closeout findings and changes

### Failed respawn assertion: numerical precision, not an incorrect destination

Reproduced the reported exact `Vector3` equality failure. A diagnostic run after a real fixed physics step measured:

- Before entry: `(-13.5000019, 0.00000147819458, 13.500001)`.
- After recovery: `(-13.5, 0, 13.5)`, the authored start.
- Delta: `(0.00000190734863, -0.00000147819458, -0.0000009536743)`; magnitude approximately **0.00000259 world units**.

The test captured a live, physics-adjusted resting position, then compared it exactly with the authored position restored by respawn. Frame timing made the tiny contact/float difference intermittent; the default vector formatting hid it. The runtime respawn position was correct. The test now reads the expected start from the selected level asset and allows a **0.00001 world-unit distance**, only 1/50,000 of the marble radius. No broad tolerance or respawn runtime change was introduced.

Both zero-duration recovery and an added nonzero-duration animation test assert the authored destination, full scale, return to Playing, dynamic body restoration, unchanged level index, continued external pause and rejected help during resolution/pause. The animated test observes Respawning explicitly. The correct-hole test now also runs a nonzero shrink and rejects duplicate outcomes throughout it.

### Reproduced setup timing defect

Early completion/setup could combine stale native `Collider.bounds` with a newly assigned transform position, incorrectly rejecting a valid goal footprint. A focused test deliberately moves the goal before the next physics step: it failed before the fix and passes afterward. `LevelManager.TryValidateSetup` now calls `Physics.SyncTransforms()` before reading goal bounds, matching the existing help-path policy. It still validates before applying the next level. This is the only production behavior change in closeout; no geometry or movement tuning changed.

### Real EditMode and PlayMode test discovery

Previously all 91 methods lived in the editor assembly; 12 entered/exited Play Mode through an EditMode fixture. They were not a separately discoverable PlayMode suite. Added a runtime assembly definition and separate EditMode/PlayMode test assembly definitions, moving the three scene test classes plus their fixture with their existing meta GUIDs. The PlayMode fixture loads the real Sandbox and cleans up the persistent manager between cases without calling lifecycle methods or repeatedly switching editor modes. These are **editor-hosted PlayMode tests**, with an `UNITY_EDITOR` constraint because they load the scene via editor APIs; they are not player-build tests.

No test was removed. Two regression tests were added, giving 79 EditMode + 14 PlayMode = **93 tests**. The existing help integration test already exercises `LevelManager.TryCallForHelp`, boundary recovery, velocity clearing, unchanged level/state and invalid-state rejection; duplicating it was unnecessary.

## Baseline and scope

- Started on `main`, HEAD `4e520d2fd024697593b141365bee13101caae156`. The only pre-existing untracked content was the previous `Docs/Audits` report package; it is not included in Phase 1 commits.
- ProjectVersion remains `6000.3.11f1`. The initial implementation used 6000.6 reference assemblies for a source-only check because the exact editor was unavailable then. Closeout used the now-installed exact 6000.3.11f1 editor. Project settings/packages were not upgraded.
- Closeout started on `main` at `6d6740c`, after local commits `1521c5f` and `2456643`. The pre-existing extra empty entry in `ProjectSettings/GvhProjectSettings.xml` and untracked `Docs/Audits/` were preserved and excluded from closeout. No push was performed.
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
- Integration tests do not reflectively invoke Awake/Start/OnEnable. At closeout they moved to the dedicated PlayMode suite and load Sandbox through the editor's Play Mode scene-loading API.
- Closeout added animated failure recovery and stale-goal-bounds regression tests. No obsolete test removals were needed during closeout.

The final source inventory contains **79 `[Test]` methods and 14 `[UnityTest]` methods** (93 total). This is a method count, not a coverage percentage.

### Actually executed in Unity 6000.3.11f1

| Full suite | Total | Passed | Failed | Skipped | Inconclusive | NUnit duration |
|---|---:|---:|---:|---:|---:|---:|
| EditMode | 79 | 79 | 0 | 0 | 0 | 0.159 s |
| PlayMode (editor hosted) | 14 | 14 | 0 | 0 | 0 | 0.970 s |

Runs used `-batchmode -nographics -runTests` with `-testPlatform EditMode` / `PlayMode`, without a test filter. Durations are the runner's reported suite durations, excluding editor startup/import. See `Verification/RunSummary.json` for exact timestamps, editor revision and result files. Negative setup tests intentionally expect error logs; neither final run contains an unexpected test failure or compile error.

Verified by these suites: real Sandbox startup and explicit marble initialization; authored completion wiring and 480-edge geometry; progression listener disable/re-enable; correct shrink and frozen duplicate outcome; failed shrink/respawn/growth and independent external pause; input magnitude/provider semantics; obsolete stuck behavior absence; real help API gating and velocity clearing; disabled listeners; safe invalid setup; immediate setup before a physics step. Android hardware and visual rendering are not exercised by headless tests.

### Historical source-only verification (15 September)

51 standalone methods passed; all then-current source compiled against installed 6.6 reference assemblies with zero errors/warnings. YAML checks covered 480 wall placements, boundaries, completion wiring and prototype points. `standalone-results.txt` and `compile-results.txt` remain historical evidence, superseded for engine verification by the exact-editor runs above.

### Manual verification and work not run

The user reported exact-editor Sandbox smoke testing healthy, no obvious snagging, and slightly slow movement. This is user-reported manual evidence; the closeout agent performed automated engine tests, not a new visual smoke pass. Help was not manually exercised because there is no UI; its API integration test passed. No Android device run, player build, prolonged collision stress test or final-feel tuning was performed.

## D. Remaining risks

1. **Android hardware remains unverified.** Real gyro enable/disable, suspend/resume, orientation and device feel still need a physical device. Mocked sensor lifecycle tests pass.
2. **Broader physics/feel testing remains.** Automated geometry checks and the user's smoke pass are healthy. Long-running corner/maximum-speed collision tests and cross-device feel remain later work; the reported slight slowness is a tuning follow-up, not a Phase 1 defect.
3. **Help is a technical API, not finished UX.** Conservative corner clearance and bounded relocation need practical testing; failure returns false for the future UI to explain. No penalty was added.
4. **Future mechanics integration remains necessary.** Unlock/reveal state is not implemented; future feature code must not subscribe to help as an attempt reset.
5. **Pause/session ownership remains deliberately small.** Full menu/game-flow state and timer will come later. Their pause reason must remain separate from hole resolution.
6. **No star targets are calibrated.** Original ideal times are historical only; production scoring and reference-runner work are deferred.
7. **Editor verification is not build certification.** Player builds, build profiles and release pipeline remain later roadmap items. New assembly boundaries passed exact-editor compilation and both suites, but were not exercised in a player build.

## E. Authoritative production geometry

`BoardGrid` is the sole source for topology and geometry constants. Runtime placement, editor display/snap, asset validation and the future importer API consume it. There is no sparse historical-geometry override system.

| Property | Definition |
|---|---|
| Logical board | 16 Ã— 16 cells, centered on (0,0), X/Z range âˆ’8 to +8 |
| Physical cell spacing | 1.8 world units per logical unit |
| Internal edges | 480: 240 along X and 240 along Z |
| Index order | Each descending-Z row has 15 Z-parallel edges followed by 16 X-parallel edges; the last row has only 15 Z-parallel edges. `GetEdge`/`TryGetEdgeIndex` are authoritative. |
| Index landmarks | 0: `(7,7.5)` along Z; 15: `(7.5,7)` along X; 479: `(âˆ’7,âˆ’7.5)` along Z, all logical coordinates |
| Internal wall | Length 2.2, thickness 0.4, height 1.5 world units; deliberate 0.4 join overlap |
| Floor and wall height | Floor Y âˆ’0.5; wall bottom at floor, center Y 0.25, top Y 1.0 |
| Boundaries | Inner faces at X/Z Â±14.4; thickness 0.4 outward; centers Â±14.6; outer board footprint 29.6 Ã— 29.6 |
| Boundary order | East, North, West, South |
| Board transform | Translation allowed; identity rotation and unit scale required in Phase 1. Camera tilt remains visual. |
| Point authoring | Half-unit logical snap by default: 0.9 world-unit increments. Start/goal support numeric unsnapped coordinates as an escape hatch. The same conversion/snap API is available for future unlock rings. |
| Serialized point storage | Existing Vector3 fields remain board-local physical coordinates, with Y 0. No LevelData schema replacement yet. |
| Editor relationship | Wall controls query shared edge poses; point fields convert through shared logical/physical functions. Undo records edits. Malformed arrays require explicit repair. |

Why 1.8: it matches the existing Sandbox's dominant spacing while removing its small offsets and disagreement with the old 1.824 editor spacing. The existing 2.2 Ã— 0.4 internal-wall footprint is retained. Normal adjacent-wall corridor width is 1.4, leaving 0.4 clearance beyond a diameter-one marble before the configurable help margin. This is a coherent starting convention, not a claim of finalized gameplay feel.

## F. Next review gate

Exact-editor verification is complete. Review the closeout, then proceed only by a separate instruction to **Phase 2: Production LevelData schema + representative level migration**. Phase 2 should standardize accidental source offsets/duplicates and unnecessary corner posts rather than preserve them with exception machinery. Star baselines should remain Needs Calibration.
