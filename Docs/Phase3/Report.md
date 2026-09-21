# Phase 3: Complete production campaign

Completed 21 September 2026. All 50 levels now exist in the approved production schema and ordered campaign. No migration blockers remain. Phase 4 has not begun.

## A. Migration summary and repository baseline

Started on clean `main` at `7c8ddea`, with Phase 2 commits `1854fbd` and `7c8ddea` present. Unity stayed at **6000.3.11f1**, revision `3000ef702840`. Original repository remained clean and read-only; all 216 recorded source hashes matched.

- Imported **39 new levels**, bringing production content to **50**.
- The original 11 representatives retained their exact asset/meta bytes, GUIDs and stable IDs. Their review campaign also stayed unchanged (24 files in total).
- No new source anomaly or rejected production level was discovered. No tolerance was loosened and no intentional maze redesign was needed.
- Existing normalization applies consistently: omit historical corner posts, normalize vertical placement, use shared production edges and half-logical point snapping. Known exceptions remain Level 15 (one duplicate edge occurrence), Level 40 (four snapped segments), and Level 50 (five duplicate occurrences).
- All 50 remain **NeedsCalibration**. Historical ideal times are editor provenance only.

Runtime code, schema, geometry constants, physics/input tuning, scene serialization, prefabs and shared Dark profile were not changed in this phase.

## B. Complete level inventory

[Migration.md](Migration.md) contains all 50 stable IDs, feature classifications, solid/reveal counts, start/goal/ring coordinates, normalization notes and validation/traversal results. All passed.

| Exclusive classification | Count |
|---|---:|
| Normal | 17 |
| Unlock | 9 |
| Dark | 8 |
| Reveal | 7 |
| Unlock + Dark | 5 |
| Unlock + Reveal | 4 |

Inclusive totals: **18 Unlock, 13 Dark, 11 Reveal**. No Dark + Reveal or triple combination. Every level has one goal.

## C. Campaign and launch

`Assets/Levels/Production Campaign.asset` explicitly lists **Level 1 through Level 50**, exactly once each. Runtime identity comes from stable LevelId; array position defines ordering. Historical build IDs and filenames do not define runtime identity.

Use **Mirlini → Play full production campaign in Sandbox**. This opens Sandbox and selects the full campaign for that play session. It does not permanently save over the default Sandbox configuration. The existing representative campaign and launch menu remain available.

## D. Importer verification

The extractor's `--all` option explicitly selects all 50 source levels. The existing default representative extraction remains available. Both paths use the same normalization and BoardGrid definitions.

Full import normalizes and validates the entire batch before writing. Missing/repeated levels, wrong distribution, identity conflicts and unsupported geometry fail cleanly. Unknown off-grid geometry is tested without relaxing the established tolerance; existing conflicting-wall tests remain intact.

Verified evidence:

- 216 original scene/prefab/script hashes matched the audited source manifest.
- Repeated extraction produced identical JSON bytes.
- All 50 normalized deterministically and matched their saved production assets.
- First full import: **39 new, 11 unchanged**.
- Second actual Unity full import: **50 unchanged**, zero new/changed.
- All **113 level-library asset/meta files** remained byte-identical across that second import, including the representatives and both campaigns.
- All 50 passed point/schema and conservative traversal validation. For all 18 Unlock levels, ring-to-goal traversal was also checked explicitly.

Evidence: [Summary.json](Verification/Summary.json), [RepeatImportHashes.json](Verification/RepeatImportHashes.json), [RepresentativesBefore.json](Verification/RepresentativesBefore.json). Structural traversal is not a physical marble playthrough.

## E. Actual Unity tests

Both complete suites ran in the exact recorded editor, without filters.

| Suite | Total | Passed | Failed | Skipped | Inconclusive |
|---|---:|---:|---:|---:|---:|
| EditMode | 104 | 104 | 0 | 0 | 0 |
| PlayMode | 21 | 21 | 0 | 0 | 0 |

Runner durations: 4.09 seconds EditMode and 13.22 seconds PlayMode, excluding editor startup. EditMode used headless graphics; PlayMode used graphics to capture real camera output. Results: [EditMode.xml](Verification/EditMode.xml), [PlayMode.xml](Verification/PlayMode.xml).

Six new EditMode tests cover campaign identity/order, independent distribution totals, all-level deterministic normalization and objective routes, representative byte/GUID preservation, incomplete/duplicate batch rejection, and unknown off-grid rejection. One new data-driven PlayMode test configures all 50 real Sandbox attempts, checks all 480 wall states/colliders/renderers per level, goal lock and optional features, marble start, Dark following, distinct attempt state, asset immutability, and safe end-of-campaign behavior.

All existing Phase 1/2 regression tests remain passing. No existing tests were disabled or removed. These are actual Unity runs, not substitute compilation checks.

## F. Targeted review

[ManualReview.md](ManualReview.md) records the 16 inspected levels and observations. Review used actual engine-rendered captures, including hidden-wall diagnostics. No hands-on playthrough was performed during this phase.

No migration-blocking issue was observed. Non-blocking presentation findings include thin/low-contrast Unlock rings, harsh Dark lighting/marble contrast, and partial goal occlusion in dense levels 48 and 50. No polish or tuning changes were made.

## G. Remaining risks and later work

Content completeness and setup correctness are verified. Remaining work includes Android gyro/device checks, physical playtesting and movement tuning, camera/board/readability polish, reveal presentation, Unlock presentation, Dark tuning, audio/effects, Call for Help UI, timer/session flow, scoring calibration/reference runner, progression/save, menus/results/selection, and player builds/release setup.

No new schema requirement or migration blocker emerged. All-level setup and structural routes do not establish every maze's physical feel, difficulty, or release-platform behavior.

## H. Local commits and files by system

- `c1f7f68` — **Migrate remaining levels and add full production campaign**: extractor, importer, import instructions, 39 assets/metas, full campaign, source JSON and migration table.
- **Verify and document complete fifty-level campaign** — following local commit: two test files/metas and Phase 3 report/evidence. Resolve its hash from Git history; the final delivery lists it explicitly.

Nothing was pushed. The original repository was not modified.

## I. Phase 4 recommendation

Recommend a bounded **gameplay feel and readability pass** using representative mechanics plus dense late levels. The user's Phase 2 playtest already identified handling refinements; full-campaign review now gives concrete ring, Dark and goal-occlusion examples. Establish movement and visibility before measuring production timing or calibrating stars, since tuning those afterward would invalidate timing baselines.

Agree the playtest targets and acceptance criteria first. Session/timer and progression/UI remain later implementation steps. Phase 4 has not started.
