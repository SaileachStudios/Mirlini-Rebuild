# Production level migration

Use Unity **6000.3.11f1**. The original project is read-only and is never opened in Unity by these tools.

## Extract verified source (Python standard library)

From the rebuild repository:

```powershell
# Existing 11 representatives; preserves the Phase 2 review input.
python Tools/LevelImport/extract.py --original "C:\Users\coyot\Documents\Unity Projects\Github\Mirlini-Original"

# Explicit complete Level 1-50 batch.
python Tools/LevelImport/extract.py --original "C:\Users\coyot\Documents\Unity Projects\Github\Mirlini-Original" --all
```

Extraction verifies every file in the recorded audit hash manifest, reads resolved audit geometry/text, and emits sorted deterministic JSON without timestamps or machine-specific paths. Defaults select 1, 6, 10, 15, 21, 27, 29, 40, 45, 46, 50 and write `Docs/Phase2/representatives.json`. `--all` selects exactly 1 through 50 and writes `Docs/Phase3/full-campaign.json`. `--levels` and `--all` are mutually exclusive; `--output` is an optional path override.

If hashes differ, stop and refresh/review the audit; do not bypass verification. Stable identities derive from source scene meta GUIDs, not build indices or filenames.

## Preview / apply

Unity menu commands:

- **Mirlini → Migration → Preview representative import** / **Import representative levels** retain the original 11-entry review workflow.
- **Mirlini → Migration → Preview full production import** / **Import full production campaign** process all 50 and maintain a separate production campaign.

Preview runs normalization and whole-batch validation and logs new/changed/unchanged; it writes no assets. Apply performs the same checks, then updates matching assets in place and creates only missing assets. Existing GUIDs are preserved. A full batch must contain exactly 1–50 in intended order and match the verified mechanic distribution before writes.

Headless full-batch equivalents:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath . -executeMethod ProductionLevelImporter.PreviewFull -quit -logFile preview.log
& "C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath . -executeMethod ProductionLevelImporter.ApplyFull -quit -logFile import.log
```

Close the interactive editor before batch commands. The Phase 2 one-time `PreparePhase2` bootstrap is not needed for ordinary imports; use Apply or ApplyFull.

## Rules and validation

All poses/indexing/conversions come from BoardGrid. Identical edge overlaps collapse; conflicting visibility states fail. Only Level 40's explicitly audited 0.05-logical-unit offset can snap. Unknown off-grid geometry is rejected; normalization errors are collected so other levels can still be analyzed before the entire write phase is aborted. Historical posts are omitted, points use half-logical snapping, and playable Y is standardized. Historical ideal times remain editor provenance only.

All levels must pass array/enum/ID/profile/clearance checks and conservative marble-clearance traversal before any level asset is written. These undirected routes connect start, goal and enabled ring; EditMode tests additionally check ring-to-goal explicitly. Traversal is a structural check, not a physics playthrough. Validation-before-write is not an OS-level transactional write; review changes in Git.

Outputs:

- Shared content: `Assets/Levels/Production/Level NN.asset`.
- Full ordered campaign: `Assets/Levels/Production Campaign.asset`.
- Retained review campaign: `Assets/Levels/Representative Campaign.asset`.
- Full table: `Docs/Phase3/Migration.md`; full verification report: `Docs/Phase3/Report.md`.

Runtime order comes from campaign references. Provenance level numbers establish the imported order in editor tooling only; runtime identity and progression must use stable LevelId, never filenames or historical build IDs.

## Review and test capture

Use **Mirlini → Play full production campaign in Sandbox** for an unsaved review selection. The representative menu remains available; neither command permanently replaces the saved prototype default unless you explicitly save the selection.

`FullCampaignRuntimeTests` normally runs without images. To capture actual Sandbox views, run PlayMode with graphics enabled and add `-mirliniReviewDirectory <absolute directory>`. Do not combine that flag with `-nographics`. It records 50 initial camera views plus 11 separately named `layout-diagnostic` views that temporarily expose hidden renderers for inspection and restore them afterward. It does not mutate LevelData or reveal-state progress.
