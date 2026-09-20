# Mirlini production foundation audit

Audit date: 14 September 2026. Scope: both local repositories, all 50 original gameplay scenes, their resolved prefabs, gameplay scripts, metadata, original boot/catalog/build settings, and rebuild scene/data/tests. No gameplay code or original files were changed. No Unity upgrade, asset migration, or schema implementation was performed.

## Executive assessment

**Keep Mirlini-Rebuild as the production foundation.** Its controller separation, input abstractions, marble lifecycle and data-driven direction are useful. There is no evidence justifying another project. It is a prototype with several concrete integration gaps, not yet a production-complete game.

The original content requires a single goal whose success condition can be locked, independent Unlock and Dark features, and visibility state on individual walls. It also contains solid corner posts, a slightly displaced wall, and duplicated geometry. A literal `bool[480]` conversion would lose information.

### Findings that change the migration plan

1. **Every one of the 50 scenes contains exactly one active gameplay hole.** There are 32 ordinary correct holes and 18 unlockable holes. No additional inactive gameplay holes or hole-tagged objects without the expected hole scripts were found. The unused Incorrect Hole prefab is not evidence of shipped multi-hole content. An unlockable goal is an incorrect/failure hole until unlocked, so failure/respawn behavior remains relevant.
2. **Nine levels combine mechanics:** Unlock + Dark in 21, 26, 32, 38, 44; Unlock + Invisible Walls in 27, 33, 39, 45. No Dark + Invisible Walls or triple combination was found.
3. **480 means internal wall edges, not board cells.** The logical board has 16 × 16 cells and 480 possible internal unit edges. Almost all original wall centerlines fit it, but exact geometry does not fit a boolean edge array alone.
4. **Solid corner posts matter.** There are 698 active untagged corner-post objects across 35 levels. They have colliders. Level 29 has one at `(6, 0, -4.5)` off the integer junction lattice. Preserve the post inventory until geometry verification establishes which can be generated automatically.
5. **Level 40 has an off-grid wall:** `4 Sections (6)` is centered at `(0, 0, 6.05)`, with collider bounds approximately `(0.3, 0.8, 4)`. Its ends are at Z 4.05 and 8.05. Snapping it to Z 6 is a proposed correction, not a faithful extraction.
6. **Duplicate geometry:** Level 15 has two invisible objects at the same unit edge centered `(7.5, 0, 0)`; its 15 invisible objects cover 14 distinct edges. Level 50 has two overlapping five-unit walls along Z 4, covering X −7 through −2. Deduplicate only with an explicit migration diagnostic.
7. **The rebuild scene and editor disagree geometrically.** The editor uses spacing 1.824 and cell centers ±13.68. Sandbox wall centers use approximately 1.8 spacing, with X offset −0.07 and Z offset −0.01. Index order and orientation are consistent with the logical 480-edge topology; exact placement and collision dimensions are not.
8. **Original Level 50 catalog data is inconsistent.** Its scene has level number 50, build ID 53 and ideal time 17.92. Boot has number 50, build ID 4 and ideal time 1.5. The shared catalog inherited by Levels 1–49 has a final entry named Regular 16 but number 3, build ID 4 and ideal time 1.5. Build settings place Level 50 at index 53. Do not migrate these catalog errors as gameplay rules.

## 1. Repository health

### Verified baseline

| Item | Finding |
|---|---|
| Rebuild HEAD | `4e520d2fd024697593b141365bee13101caae156` — Extract end-of-level flow controller and remove LevelManager runtime component repair |
| Rebuild working tree | Clean on `main`, tracking `origin/main` at inspection |
| Original HEAD | `c66e8971e7d80522b5cf795e000e40478b0a486c` — Importing original code |
| Original working tree | Clean on `main`, tracking `origin/main` |
| Recorded Unity version | `6000.3.11f1` |
| Runtime content | One Sandbox scene, two LevelData assets, 480 distinct assigned wall objects |
| Build list | Empty `EditorBuildSettings.m_Scenes`; no build profile assets found in Assets |
| Automated tests | 67 `[Test]` methods and 3 `[UnityTest]` methods in ten test files |
| Serialized asset references | No unresolved non-built-in GUIDs or missing `.meta` files in the scanned rebuild `.unity`, `.prefab`, `.asset` and `.cs` set |

The Git status required an unrestricted read because Git LFS writes temporary metadata while checking files. No Git configuration was changed. Remote history was not fetched; these are local HEAD and local tracking-ref facts, not a claim about the live GitHub tip.

### Keep

- `MarbleController` and `BallStateMachine`: explicit velocity/acceleration logic and legal lifecycle transitions provide good test boundaries. Retain shrink/grow and the post-shrink completion event.
- Behavior/controller separation and input interfaces/factory: useful seams for device handling and tests. Refine the contracts where behavior is wrong rather than replace the pattern.
- `GameEvents`: retain event-driven coordination, with clear ownership and lifecycle cleanup. A rewrite to a new event framework is unnecessary.
- `LevelData` assets and editor workflow: retain reusable board construction and asset authoring. Extend content representation based on the audit.
- `LevelEndFlowController`: separating completion routing from geometry setup is sound. Replace its temporary delay with game-flow/summary behavior later.
- Visual tilt: the Sandbox `BoardTilt` hierarchy contains the camera, not wall colliders. It is currently a camera-based visual effect, not physically tilted board geometry.

### Concrete defects and provisional areas

| Priority | Evidence and consequence | Recommendation |
|---|---|---|
| High | Sandbox has `LevelManager` but no serialized `LevelEndFlowController`. Its GUID `e9298ff0bc2ee3140ba9e52035db14e1` does not occur in the scene. Runtime repair was removed. | Wire the component and add a scene-contract EditMode test. `RequireComponent` is not a substitute for validating already-authored scenes. Runtime confirmation remains pending. |
| High | `UnityInputWrapper.GyroEnabled` setter is a no-op; the provider/factory never enables the gyroscope. | Enable/disable the sensor through a real lifecycle and test the wrapper integration on Android. |
| High | `GameManagerBehavior.FixedUpdate` calls `.normalized` on all input. | Preserve analog magnitude and clamp only values outside the accepted range. Device multipliers currently disappear for nonzero input. |
| High | Automatic stuck tracking still transitions to Stuck and respawns. | Replace with explicit Call for Help, as already directed. Keep penalty and UX unresolved. |
| High | `LevelType` is mutually exclusive and unused by runtime setup; setup always marks the hole correct. | Add independently configured Unlock/Dark behavior and per-wall visibility after schema review. |
| High | Scene grid spacing and editor coordinates disagree; wall lengths/thicknesses also differ from a uniform scaling of original geometry. | Share one GridDefinition between editor, importer and runtime. Validate passage widths and corner joins. |
| Medium | Input provider coordinates retain the original swapped/negated axes, while the new controller maps them directly to X/Z. | Verify screen-space directions with the camera before changing them; do not blindly revert formulas. |
| Medium | `BoardTiltBehavior` subscribes in Start and never unsubscribes. Persistent manager events can retain destroyed listeners. | Pair subscriptions with teardown/enable-disable policy. |
| Medium | `MarbleBehaviour` subscribes to state changes in Start, but `LevelManager.Start` can call StartPlaying earlier. Manual lifecycle invocation in tests does not cover Unity's actual start ordering. | Make initialization ordering explicit and test the real scene startup. |
| Medium | `OnMarbleDropped` overwrites pending hole data before rejecting an invalid state transition. | Gate accepted drop requests so duplicate trigger events cannot alter an ongoing fall's outcome. |
| Medium | `LevelManager` logs missing objects/arrays but can continue to dereference them; null level entries and partial setup remain possible. | Validate before mutation and report a coherent setup failure. |
| Medium | Hole status broadcasts only update each behavior's indicator, while correctness lives in the controller. | Give the single goal a clear state-setting API; avoid a visual-only unlock event. |
| Medium | Audio has a drop subscription, but `clangSFX` is never played by the class and drops are suppressed while its source is playing. | Finish collision SFX and choose mixing/overlap deliberately. Preserve existing assets. |
| Low | Editor lacks Undo support and shared validation, indexes wall arrays directly, and dirties on GUI changes without a null guard. | Fix as part of schema tooling, not as a standalone style rewrite. |
| Low | Old serialized GameManager fields (`levelManager`, `clangAudio`) remain although the current class does not declare them. | Clean stale serialization when touching the affected assets. |

The two existing LevelData assets remain prototype content, not authoritative migrated originals. No timer, scoring, versioned saves, production menus, level selection or results flow was found. No CI/build pipeline was found. ExternalDependencyManager and broad package dependencies warrant a dependency review before release; their presence alone is not a reason to delete them.

### Tests and verification limits

Existing tests cover controller math, ball transitions, input providers/factory, hole events, marble lifecycle and basic completion routing. Several Unity-facing tests use reflection to invoke Awake/Start and set zero animation duration. They provide value but do not prove real timed animation, scene wiring, physics contact or device behavior. The stuck tests currently encode behavior that the new design intentionally supersedes.

No Unity tests or builds were run. The recorded `6000.3.11f1` editor was not found in the standard installed Hub editor directory; that directory contains other versions including `6000.6.0f1`. Opening with a different version would risk an unrequested upgrade. No Library folder was present in the rebuild checkout. Test counts above are source inventory, not pass results or coverage percentages.

The audit itself successfully resolved all scene prefab override targets without resolver warnings, validated all 50 scene identities and instruction sets, checked one marble/one gameplay hole per level, checked all ring durations, and classified geometry exceptions. Original unresolved asset GUIDs are retained in the reference report; many appear to be package UI scripts, so they are not labeled missing gameplay scripts without a Unity/package import check.

## 2. Content inventory and mechanics

See [Inventory.md](Inventory.md) for all 50 rows and every exact input-specific instruction. [inventory.json](inventory.json) includes wall transforms, colliders, logical edge indices, invisible-wall positions, corner positions, lighting, source identities and exceptions. [Maze-Atlas.html](Maze-Atlas.html) shows all 50 layouts with invisible geometry exposed for inspection.

### Complete classification

| Exclusive category | Levels |
|---|---|
| Normal — 17 | 1, 2, 3, 4, 5, 8, 12, 17, 22, 28, 34, 40, 46, 47, 48, 49, 50 |
| Unlock only — 9 | 6, 7, 9, 13, 18, 23, 29, 35, 41 |
| Dark only — 8 | 10, 11, 14, 19, 24, 30, 36, 42 |
| Invisible Walls only — 7 | 15, 16, 20, 25, 31, 37, 43 |
| Unlock + Dark — 5 | 21, 26, 32, 38, 44 |
| Unlock + Invisible Walls — 4 | 27, 33, 39, 45 |

Inclusive totals: Unlock 18; Dark 13; Invisible Walls 11. Level 45 is confirmed by component data, not just its instruction text. Every Invisible Walls level retains four visible boundary walls. Level 50 also has four boundaries, but they are Untagged, which is why tag-only counting is insufficient.

### Serialized and script-derived behavior

- **Unlock:** one ring in each Unlock level, always serialized as 2 seconds. Its sphere trigger has local radius 0.27, center X −0.015 and transform scale `(5,1,5)`. Store its exact source geometry for comparison; do not assume the visible circle is the contact footprint. Script timer accumulation occurs in OnTriggerStay and resets in OnTriggerExit. The unlocked boolean remains true until the scene is reconstructed. Trigger callbacks do not filter for the marble: preserve intended continuous marble occupancy, not this bug.
- **Locked goal:** `UnlockableHole.Start` sets correctness false; `Unlock` sets it true. Entering it before unlock uses the original failure-reset path. A single goal model must preserve that distinction.
- **Invisible walls:** all audited invisible wall pieces are individual unit segments with a solid collider. `InvisibleWalls.Start` disables the renderer; collision enables it. Reveals are object-local and last until scene reload. Its unfiltered collision callback should become marble-specific.
- **Dark:** one following spotlight and no directional light in each Dark level. The shared spotlight is at Y 6 with intensity 3, range 21.45 and spot angle 46.6. Follow behavior copies marble X/Z every Update. RenderSettings and full light data are preserved in the machine report. Actual visibility depends on rendering/materials and requires playtesting.
- **Marble tuning:** all 50 scenes use the same serialized movement settings: speed 100, gyro multiplier 3, touch multiplier 0.2, side clamp 7.69 and height clamp 0.43; marble scale is 0.6 and rigidbody mass 1. The touch multiplier is not actually applied in the original touch handler. These belong to a shared feel profile/reference, not fifty duplicated tuning records.
- **Hole geometry:** all 50 goal roots are at Y −0.5, scale 1, with the same trigger dimensions and child rim collider arrangement. Marble starts use Y 0. Ring roots use Y −0.49. These elevations can become shared prefab/profile offsets rather than editable per-level fields.
- **Other objects:** the remaining audited collider structures are wall corner posts, the goal prefab's four surrounding pieces, and board boundaries. No additional custom production mechanic scripts were found beyond the listed marble, hole, unlock, light, wall and manager/UI behavior. Full resolved objects retain cameras, UI, renderers and inactive presentation children for follow-up.

### Grid compatibility: precise answer

The 16 × 16 logical lattice spans X/Z −8 to +8, with cell centers at half-integers. Walls use integer junction endpoints and axis-aligned unit edges. Many early levels place start/goal/ring positions at integers, not half-integer cell centers; restricting point placement to cell buttons would shift those positions.

All ordinary wall segments except the single Level 40 offset can be split into logical unit edges. Invisible segments already use that granularity. Four outer boundaries are shared board geometry. Corner posts need separate preservation or a verified procedural join rule. Therefore **all 50 levels cannot be represented exactly by the present 480 booleans and cell-center-only editor**.

Index mapping for a source unit-edge midpoint `(x,z)`:

- Segment parallel to Z: `31 × (7.5 − z) + (7 − x)`.
- Segment parallel to X: `31 × (7 − z) + 15 + (7.5 − x)`.

These map to existing editor/index ordering, after validating integer indices and bounds. Sandbox positions approximately follow `(1.8x − 0.07, 1.8z − 0.01)`; the editor expects a different scale, 1.824 with no such offset. The current runtime wall length is 2.2 and thickness 0.4; a uniformly 1.8-scaled original unit wall would be length 1.8 and thickness 0.54. Similar index order does not prove similar collision feel.

## 3. Recommended production schema — proposal only

Keep the smallest structured model that preserves actual content. Do not build a general modifier/plugin framework for hypothetical mechanics.

| Field/model | Why it exists; content requiring it |
|---|---|
| Stable `LevelId` | All 50; saves must not depend on filename, asset rename or array position. Campaign ordering can remain a separate ordered list. |
| Display name and instruction key/content | All levels; preserve three original control-specific instruction variants initially. Localization can replace raw copy later. |
| `IdealCompletionTime` | All levels; keep existing field. Level 50's conflicting values require an explicit provenance note and approval of the intended anchor. |
| Board-local `StartPosition` and `GoalPosition` in X/Z | All levels. Preserve continuous/half-unit placements; do not force cell centers. Shared prefab offsets supply Y. |
| `WallState[480]`: Empty, Solid, RevealOnCollision | All levels; reveal state required by 15, 16, 20, 25, 27, 31, 33, 37, 39, 43, 45. Derive the Invisible Walls label from wall states, rather than maintain a redundant level flag. |
| Optional `UnlockFeature` with ring X/Z | The 18 Unlock levels. Feature presence means goal begins locked. Shared mechanics profile supplies the common 2-second duration and ring geometry; avoid per-level duration overrides until content needs one. |
| Optional Dark lighting profile | The 13 Dark levels. Use one shared profile for the existing spotlight/environment settings. Independent of Unlock. |
| Corner-post positions, initially explicit | The 35 levels with corner posts, particularly Level 29's off-grid post. These can be reduced to generated joins only after comparing collision geometry. |
| Sparse geometry override list, narrowly scoped | Level 40's displaced wall if exact preservation is needed. Record center/orientation/length or an offset to a named wall run; leave its four corresponding grid edges empty to avoid double geometry. If approved as an authoring error and corrected, this runtime extension may be unnecessary. |
| Shared `BoardGeometryProfile` | All levels: grid size/spacing, wall width/height, boundary construction, corner profile, marble/goal vertical placement. One coordinate definition feeds runtime and editor. |
| Import provenance, editor-only | Original scene GUID/hash and object-to-edge mapping, duplicated geometry decisions and migration version. Auditability without runtime clutter. |

No holes array is warranted by current production content. The goal still has runtime locked/unlocked correctness. No per-level physics tuning is justified by the serialized originals. No arbitrary object-spawn list is warranted. Unlock and Dark are independent optional settings, and invisible behavior belongs on walls; combinations follow naturally.

Level assets describe initial state only. Revealed walls, ring progress, goal lock, current time and ball lifecycle belong to runtime attempt state, not mutable ScriptableObject fields. Full replay starts a new attempt. The original preserves unlock/reveals across a failure respawn within the same loaded scene; retain that intent unless a design decision changes it. Call for Help's effect on attempt state remains unresolved.

## 4. Migration strategy

1. **Freeze evidence:** retain current source hashes, full prefab-resolved extraction and object IDs. Treat the local original as immutable. Do not open it in a newer Unity editor to extract content.
2. **Fix the coordinate contract first:** define 16 × 16 topology, 480-index mapping, point coordinates and shared geometry dimensions. Compare original and rebuild passage widths before selecting production scale. Keep current movement architecture while testing feel.
3. **Make importer validation strict:** each source must yield one marble, one goal, expected feature components, known geometry and positive ideal time. Reject unknown/missing references, non-axis geometry, unhandled overrides and overlapping conflicting wall states. Use the static report as the interchange input to a rebuild Editor importer.
4. **Pilot varied levels:** 1 (open), 2 (walls/posts), 6 (unlock), 10 (dark), 15 (reveal and duplicate), 21 (unlock/dark), 27 and 45 (unlock/reveal), 29 (post exception), 40 (wall exception), 46 (dense maze), 50 (overlap/catalog conflict).
5. **Automate the remaining assets:** deterministic stable IDs and ordered campaign list, preserving existing asset GUIDs if deliberately replacing an existing level. Support dry-run and per-level diff reports. No manual transcription of walls or instructions.
6. **Compare every level:** generate top-down overlays of source collider outlines versus rebuilt geometry, plus feature/configuration diffs. Perform a traversal check with marble radius, not just empty-grid flood fill. Validate start/ring/goal accessibility and thin-wall contacts.
7. **Human verification:** play all 50; focus on Levels 29, 40, 46–50, ring dwell/reset, locked-goal failure, reveal reset scope and Dark visibility. Device playtests decide physics feel and screen-axis conventions.

Automatically migratable: transforms, ordinary unit wall occupancy, per-piece visibility, most geometry, ring positions, shared duration, dark presence/settings, instruction variants, ordinary catalog values and level order from actual scene numbers. Explicit review required: duplicate geometry handling, corner generation, Level 29 post, Level 40 offset, Level 50 catalog/time, production scale/clearances and original-versus-rebuild input feel.

The custom YAML resolver is an audit tool, not a general Unity importer. It resolves the prefab forms present here, local references, active parents, transforms, property overrides and removed components. Full original data remains available for checking unsupported cases. A future importer should validate against Unity's loaded rebuild objects before content is accepted.

## 5. Timing, scoring and behavior decisions

### Timing contract already decided

Use a separate session/game-flow state model. Start the clock only after instructions are dismissed. At accepted hole entry, stop accumulating time immediately, before shrink. Correct goal stays stopped through completion/summary. Failure stays stopped through shrink, reposition and growth; resume only on restored playability. Menu pause and app suspension must not be accidentally cleared by a marble lifecycle event. Do not use a single mutable pause boolean as the owner of all these reasons.

### Original scoring formula, documented only

Let ideal time be `I`, scoring range `R = max(I, 10)`, and step `D = R / 12`. The intended mathematical equivalent of the loop is:

`pieces = clamp(ceil((I + R − time) / D), 0, 12)`

At `time >= I + R`, award zero; at ideal time or faster, award 12. Each star receives four pieces. Floating-point comparisons in the original repeated-subtraction loop can affect exact boundaries. Production should test all thresholds explicitly rather than preserve numerical artifacts.

The summary selects `min(storedBest, currentTime)`. However, the original calls `ShowingSummary` before `StopTimer` updates the best time. On the first completion, the stored zero sentinel can therefore produce full stars regardless of elapsed time. Preserve the intended best/current comparison; treat zero as “no stored best,” not a competitive time. Propose computing an immutable completion result and saving it before rendering the summary. Scoring has not been implemented.

### Difference classification

| Difference | Classification |
|---|---|
| Reusable level assets instead of 50 scenes | Architectural improvement |
| Behavior/controllers, factory and explicit marble transitions | Architectural improvement |
| Shrink/grow retained; animations excluded from timer | Intentional improvement / current design |
| Explicit help replacing automatic stuck respawn | Intentional improvement / current design |
| Target velocity, acceleration, speed cap and camera tilt | Unresolved until playtest; promising improvements to retain for comparison |
| Lost analog magnitude / gyro never enabled | Likely regression |
| Automatic next-level delay without summary, incomplete menus and special mechanics | Provisional implementation; missing player-facing parity |
| Scene missing completion-flow component | Likely regression |
| Original zero-best star bug, Level 50 build-ID errors and unfiltered trigger callbacks | Obsolete original behavior; preserve intent instead |
| PlayerPrefs-coupled storage | Replace architecture while preserving unlock/best-time intent |

## 6. Small, testable rebuild roadmap

| Step | Deliverable | Acceptance check |
|---|---|---|
| 1 | Wire current completion flow; validate Sandbox dependencies | Scene-contract EditMode check; existing completion tests run in the recorded editor |
| 2 | Shared grid/coordinate definition and geometry validator | All 480 indices round-trip; explicit exception fixtures for 29/40/50 |
| 3 | Reviewed LevelData schema, wall state and optional features | Serialization round-trip; invalid content rejected; existing two assets preserved/migrated |
| 4 | Deterministic importer and representative pilot assets | Dry-run/diff/overlay matches and stable GUIDs |
| 5 | Complete 50-level asset migration | Fifty validated assets; one campaign list; all exceptions recorded |
| 6 | Session-flow state and attempt ownership | ShowingGoal → Playing → ResolvingHole → Summary; explicit pause/retry/exit paths distinct from ball states |
| 7 | Clock and hole lifecycle integration | Pure clock tests plus critical timed correct/failure PlayMode flows; growth never counts as gameplay |
| 8 | Unlock feature | Continuous 2 seconds, exit resets, only marble counts, goal remains unlocked within attempt |
| 9 | Individual wall reveal | One collision reveals only that wall; solid while hidden; replay resets |
| 10 | Dark profile and combined-feature validation | Following light, readable goal/ring; playtests of 21 and 45; features do not disable each other |
| 11 | Call for Help action replacing auto-stuck logic | Input/API works only in valid flow states; no spontaneous respawn; UX/penalty explicitly left for design review |
| 12 | Device input and feel pass | Sensor lifecycle, magnitude, dead zone, calibration/control choice and camera directions verified on Windows/Android |
| 13 | Pure scoring/result calculation | All twelve boundaries, first completion, best-time preservation and fast/slow runs covered |
| 14 | Versioned save model and progression | Level 1 unlocked; completing N unlocks N+1; stars never gate; corrupt/missing save and version migration tested |
| 15 | Level selection | All 50 entries, lock state, best/stars, keyboard/focus and touch navigation |
| 16 | Summary/retry/continue/return/final credits | Immutable results; replay isolation; final level cannot index past campaign |
| 17 | Boot, main menu and settings | Reliable navigation, persistent settings, suspend/resume behavior |
| 18 | Audio and accessibility polish | Impact/drop/mix settings, reduced camera motion, readable UI, non-color-only cues, input alternatives |
| 19 | Release pipeline | Windows/Android build profiles, repeatable builds, smoke tests, device performance and save-upgrade validation; Steam packaging after core release flow |
| 20 | Optional analytics and future Web review | Only if desired; define purpose/privacy first. No accounts, ads, monetization or paid services added |

Build configuration and a Windows smoke build should begin early once the exact editor is available; final packaging follows gameplay validation. Favor pure domain tests for clock, scoring, progression and state rules; targeted EditMode checks for scene/assets/importing; PlayMode only for critical animation/physics integrations.

## Open decisions / verification queue

- Call for Help presentation, penalty, and effect on reveal/unlock state: unresolved; no penalty invented.
- Level 40 displacement: likely authoring drift, but inspect clearance before approving a snap.
- Level 29 post: compare its actual contact footprint; it may sit within a wall body rather than represent a standalone obstacle. Keep exact position until verified.
- Level 50: 17.92 is the strongest scene-local candidate ideal time, not yet an approved scoring-design change. Boot's build ID 4 clearly points at Level 1, unlike Level 50's build index 53; the actual shipped binary's behavior cannot be established from this checkout alone.
- Scoring: confirm the documented intended thresholds and corrected first-completion semantics before implementation.
- Unity/device verification: startup order, collision clearances, gyro axes, lighting visibility, full 50-level traversal and feel remain untested in-engine.

## Evidence map

- `Inventory.md`: concise 50-row content table and verbatim instruction variants.
- `inventory.json`: machine-readable content, per-wall cells/positions and all exceptions.
- `Maze-Atlas.html`: static geometry atlas; not a game rendering.
- `rebuild-grid.json`: every assigned scene wall index and collider bounds.
- `catalog-variants.json`, `Boot-catalog.json`: original persistence/catalog contradictions.
- `reference-and-test-audit.json`: unresolved GUID candidates and test inventory.
- `source-hashes.json`: scene/prefab/script fingerprints.
- `evidence.zip`: complete resolved scene objects and supplementary diagnostics.
- `tools/`: reproducible extraction and report-generation scripts; PyYAML 6.0.3 required. Re-running tools writes only to their output directory, never the original.

Primary script evidence: original `Assets/Scripts/UnlockRing.cs`, `UnlockableHole.cs`, `InvisibleWalls.cs`, `PlayerSpotLight.cs`, `Marble.cs`, `MarbleTimer.cs`, `Managers/GameManager.cs`, `Managers/LevelManager.cs`, `Managers/Manager.cs` and `UI/LevelUI.cs`; rebuild `Assets/Scripts/Marble`, `Board`, `Core`, `Input`, `Audio`, `Assets/Editor/LevelEditorWindow.cs`, and `Assets/Scenes/Sandbox.unity`.
