# Phase 2 — production schema and representative migration

Completed 20 September 2026 against Unity **6000.3.11f1 (3000ef702840)**. Baseline was clean `main` at `68c9260`, containing Phase 1 closeout `8701b67`. No push, Unity upgrade, movement tuning or Phase 3 migration was performed. Mirlini-Original remained read-only; all audit source hashes matched and its working tree remained clean.

## Result / Phase 3 readiness

**Technically ready for Phase 3: Full 50-Level Migration, after the requested human review gate.** One schema builds and runs all six shipped mechanic categories and the representative geometry exceptions. Exactly the requested **11 production levels** were imported: **1, 6, 10, 15, 21, 27, 29, 40, 45, 46 and 50**. The other 39 were not imported.

The full exact-editor suites passed: **98 EditMode + 20 PlayMode = 118 tests**, zero failed, skipped or inconclusive. Existing Phase 1 lifecycle tests still pass. Representative features were exercised through real collisions/triggers. No new manual visual playtest was performed; headless functional verification does not certify lighting polish or game feel.

## A. Production schema

`Assets/Scripts/Board/LevelData.cs` holds initial authored content only:

| Runtime field | Purpose / content requiring it |
|---|---|
| `LevelId` | Permanent identity independent of campaign position, asset filename and scene build index. Imported IDs are `mirlini-` plus the source scene's stable meta GUID; new authored levels receive a GUID once. Prototype IDs preserve each existing asset's identity. Hidden from the default inspector; Level Editor displays it. |
| `LevelName` | Display metadata, preserved from scene metadata; never used as identity. |
| `Instructions.Keyboard/Touch/Gyro` | Three independent text variants, copied exactly from the audit. A small serializable record can later be replaced by localization keys. No localization system or instruction UI was added. |
| `MarbleStartPosition`, `HolePosition` | One start and exactly one goal, in board-local physical X/Z with playable Y = 0. No general holes collection. |
| `Walls[480]` | Compact enum array: `Empty`, `Solid`, `RevealOnCollision`. One state per authoritative BoardGrid edge prevents duplicate runtime geometry. Reveal behavior is per edge, with no redundant level-wide invisible flag. |
| `Unlock.Enabled`, `Unlock.RingPosition` | Independent feature presence and one board-local ring position. An enabled feature starts the single goal locked. Disabled ring coordinates are ignored. |
| `Dark` | Null means normal lighting; a shared `DarkLightingProfile` reference enables Dark independently of Unlock and wall states. |
| `StarCalibration` | Explicit `NeedsCalibration`. There is deliberately no calibrated value or production ideal-time field yet, so historical times cannot become a scoring target. |

Editor-only fields, compiled out of players: `MigrationSourceGuid`, `MigrationSourceHash`, `MigrationLevelNumber`, `HistoricalIdealTime`, `MigrationNotes`. They record origin, extraction evidence and corrections; none controls runtime ordering, movement or scoring.

`LevelCampaign.Levels` supplies ordered references independently of identity. The representative campaign is an asset; Sandbox keeps its two existing prototypes as its default fallback. The three existing prototype LevelData assets were converted in place, preserving asset GUIDs, wall occupancy and positions.

### Shared configuration

- `BoardGrid` is unchanged: 16×16, 480 edges, logical ±8, spacing 1.8, walls 2.2×1.5×0.4, floor Y −0.5, wall center Y 0.25, boundary inner faces ±14.4, half-logical-unit point snap. Runtime, importer, editor and validation all consume it.
- `LevelMechanics.UnlockSeconds = 2`. One shared dwell rule, no per-level override.
- Ring radius is **2.43 world units**: original sphere radius 0.27 × prefab X/Z scale 5 × production logical spacing 1.8. Its center is standardized to the authored point at playable height; the small historical collider offset is not retained. Occupancy means trigger overlap with the assigned marble, as in the intended original rule.
- `Shared Dark.asset`: height **10.8**, range **38.61**, spot angle **46.6°**, intensity **3**, white light. Original lengths are scaled into production units. Inner angle remains the shared original 4°. Dark disables scene lights and ambient/reflection illumination, follows marble X/Z, and restores prior lighting on switching back or disabling its owner. No per-level lighting variants were needed.

### Attempt state

`LevelAttempt` owns revealed edges, continuous unlock progress and latched goal-unlocked state. `LevelFeaturesBehavior` owns that attempt and its presentation. Marble lifecycle and independent pause reasons stay in the existing Phase 1 components. No timer or scoring state was introduced.

`TrySetupLevel` means a new attempt and replaces feature state. Failed-hole recovery and Call for Help do not call setup; they preserve the same attempt instance. Reveal and unlock data are never written into ScriptableObjects.

## B. Representative migration results

See **[Migration.md](Migration.md)** for every level's physical start, goal, ring, wall counts, features and normalization. **Every row passed schema, identity, point/wall clearance and conservative traversal validation, and built successfully in the Unity PlayMode suite.** Blank feature cells mean Normal. Counts are internal wall edges; the four shared boundaries are not included.

Highlights:

- Levels 1/6/10 prove Normal, Unlock and Dark without internal walls.
- Level 15: 15 historical invisible wall objects become **14 reveal edges**, removing one overlap.
- Level 21: **96 solid edges**, Unlock + Dark simultaneously.
- Levels 27/45: **94 reveal edges each**, Unlock + Reveal; original control-specific instructions are retained exactly, including Level 45's combined-mechanics instruction.
- Level 29: **96 solid edges**; all 16 historical posts, including the unusual post, are omitted in favor of shared production joins.
- Level 40: the one 4-section wall centered at logical Z 6.05 is snapped to Z 6.0, yielding **98 solid edges**, with four unit segments corrected. No sparse override is retained.
- Level 46: **225 solid edges**, dense-maze coverage.
- Level 50: **225 solid edges**, five duplicate edge occurrences removed; broken build/catalog IDs are never consumed.

No additional layout redesign was needed. Point X/Z are snapped with BoardGrid's half-unit rule then converted to production spacing. Historical Y values for floor-mounted goals/rings are standardized to the current prefab/feature convention. Original ideal times survive only in editor provenance; all 11 levels remain NeedsCalibration.

## C. Deterministic importer

Pipeline: verified historical audit → `Docs/Phase2/representatives.json` → `ProductionLevelImporter.Normalize` → whole-batch validation → LevelData assets + ordered representative campaign.

1. `Tools/LevelImport/extract.py` reads the audit, verifies its original scene/prefab/script hash manifest against the read-only original repository, and extracts only explicitly selected levels. Python standard library only; no Unity launch or save in the original project.
2. The intermediate preserves resolved wall collider centers/spans, visibility, source IDs, points, text, feature presence and provenance. It contains no production grid-placement formula.
3. C# splits source spans into unit logical edges and resolves them through `BoardGrid`. Known Level 40 snapping is explicitly bounded; unknown off-grid geometry or conflicting visibility fails. Identical overlapping states collapse to one edge. Historical posts are ignored.
4. All levels are normalized and validated before writing any level asset. Existing identity conflicts fail before writes. Reimport updates matching assets in place, preserving their GUIDs; unchanged serialized content is left untouched.
5. Traversal uses a conservative half-logical-unit graph, marble-radius-expanded walls, and checked intermediate movement points. It proves a route from the start to the goal and enabled ring for these snapped representatives. It is not a dynamic physics solver, a speedrun runner, or a proof for arbitrary unsnapped authoring positions.

**Repeatability checked:** normalizing each source twice produced identical content; saved assets match normalization; an actual second Unity import reported all 11 unchanged and left existing level assets/meta files byte-for-byte unchanged. `Preview representative import` runs the same normalization and validation and reports new/changed/unchanged without writing assets.

See **[../../Tools/LevelImport/README.md](../../Tools/LevelImport/README.md)** for commands and future use. The same pipeline accepts the remaining levels after approval; defaults remain restricted to these 11. A future full batch should build an explicitly reviewed 50-entry campaign, not infer progression from old build IDs.

## D. Authoring and review workflow

Open **Mirlini → Level Editor**, then choose a level in `Assets/Levels/Production`.

- Click an edge to cycle **Empty → Solid → Reveal**. Reveal is magenta; solid is black. All placement comes from BoardGrid.
- Edit start/goal logical X/Z numerically. Half-unit snapping is enabled by default; disable it for exact numeric authoring.
- Toggle Unlock independently and edit its ring with the same point controls. The U marker shows its location.
- Assign `Shared Dark` to enable Dark; clear it to disable. Unlock remains independent.
- Expand Instructions for keyboard/touch/gyro text; edit the display name. Permanent identity is generated only for a new/missing ID, never on rename.
- Validation appears in the window. Malformed array/configuration repair is explicit and Undoable. `Validate level library identities` checks the whole authored library and reports duplicate IDs or invalid data.
- All authoring changes retain Undo through `Undo.RecordObject` or `SerializedObject` editing. Migration is an explicit disk operation, not an Undo-based authoring action; use its preview first.

For runtime review choose **Mirlini → Play representative campaign in Sandbox**. This opens Sandbox, assigns the representative campaign for the review session, and enters Play Mode. The saved Sandbox default remains the Phase 1 prototypes unless the reviewer explicitly saves this selection. The existing temporary next-level flow advances through the representative order. There are no new menus, final results UI or help binding.

## E. Runtime status

| Behavior | Status |
|---|---|
| Normal / Empty walls | Built with shared geometry; inactive empty edges, visible solid edges. |
| Reveal walls | Renderer hidden, collider active; only the assigned playable marble reveals the contacted edge. Revealed state persists through help and failed-hole recovery; new attempts hide it again. |
| Unlock | Assigned-marble-only trigger occupancy; continuous 2 seconds; leaving early resets progress; pause does not accumulate time. Success latches and makes the existing goal correct. |
| Dark | Shared spotlight follows marble, other scene lights disabled, previous lighting restored on return to Normal. |
| Combined features | Unlock + Dark and both requested Unlock + Reveal examples run without overwriting each other's configuration. |
| Hole lifecycle | Frozen accepted result, duplicate rejection, external pause preservation, unavailable help while resolving, and return to Playing after growth retained. Real locked-goal trigger recovery is verified. |

No feature silently falls back to Normal. Missing/invalid feature data and missing authored scene ownership fail setup. Reveal-wall collision geometry is checked in scene setup and tested across all 11 representatives.

## F. Verification

Full Unity 6000.3.11f1 test runs, unfiltered, using `-batchmode -nographics`:

| Suite | Total | Passed | Failed | Skipped | Inconclusive |
|---|---:|---:|---:|---:|---:|
| EditMode | 98 | 98 | 0 | 0 | 0 |
| PlayMode, editor hosted | 20 | 20 | 0 | 0 | 0 |

Runner durations: approximately **0.70 s EditMode**, **8.89 s PlayMode**, excluding startup/import. Exact times and XML evidence are in `Verification/`.

Added **19 EditMode cases**: enum/combined-feature serialization, stable/duplicate IDs, malformed data, reveal collision clearance, pure dwell/reset/pause rules, attempt-local reveal state, deterministic normalization of all representatives, anomaly normalization, conflicting-source rejection and unreachable-goal detection.

Added **6 PlayMode cases**: all 11 runtime wall configurations and asset immutability; actual marble/non-marble reveal collisions with help/recovery/replay; actual locked-goal recovery; marble-only ring occupancy/early exit; Unlock + Dark including pause, persistence and lighting restoration; Unlock + Reveal on Levels 27 and 45.

Updated the existing scene-contract tests for the production schema and authored feature owner. No tests were removed. Phase 1 input, gyro lifecycle, geometry, help and hole lifecycle suites remain passing. No standalone harness result is substituted for these Unity runs.

Not executed: Android device tests, player builds, a new manual visual smoke pass or GUI interaction automation. Lighting appearance, editor ergonomics and full-maze feel still warrant human review; engine tests verify configuration/physics behavior, not presentation quality.

## G. Remaining risks and future work

- Human review of the 11 representatives is the agreed gate before bulk migration; conservative traversal does not replace playtesting corners and routes.
- Android hardware/gyro activation, orientation and suspend/resume remain unverified on a device.
- Movement feel and final marble speed remain deliberately unchanged; prior slight slowness is a later tuning item.
- Call for Help still has no UI. Its API and new feature-state preservation pass automated tests.
- Final timer/game-flow UI, star calibration/reference runner and production scoring are not implemented. Original times cannot serve as production baselines.
- Dark/ring presentation is functional, not polished. Player builds and shader/rendering behavior on target devices remain part of the release pipeline work.
- Import conflict/unknown-geometry failures require explicit review. Do not loosen normalization tolerances to conceal a new source anomaly.

## H. Local commits

1. `1854fbd` — `Add production level schema and composable attempt mechanics`.
2. `Import and verify eleven representative production levels` — importer, selected content, feature tests and this report (the closeout commit; see Git log for its hash).

No remote push was made.
