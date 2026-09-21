# Representative level migration

Use Unity **6000.3.11f1**. The original project is read-only and is never opened in Unity by this tool.

## Extract (Python standard library)

From the rebuild repository:

```powershell
python Tools/LevelImport/extract.py --original "C:\Users\coyot\Documents\Unity Projects\Github\Mirlini-Original"
```

This verifies every file in the recorded audit hash manifest, reads resolved audit geometry/text, and writes `Docs/Phase2/representatives.json`. Default levels are 1, 6, 10, 15, 21, 27, 29, 40, 45, 46, 50. Output is sorted and deterministic, with no timestamps or machine-specific paths. `--levels` and `--output` support a later explicitly approved batch; do not import the remaining content during Phase 2.

If hashes differ, stop and refresh/review the audit; do not bypass verification. Stable identities come from scene meta GUIDs, not build indices or filenames.

## Preview / apply

In Unity:

1. **Mirlini → Migration → Preview representative import**: normalize and validate the entire batch; log new/changed/unchanged; write nothing.
2. **Mirlini → Migration → Import representative levels**: the same checks, then update/create assets in `Assets/Levels/Production`, preserving existing asset GUIDs.

Headless equivalents:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath . -executeMethod ProductionLevelImporter.Preview -quit -logFile preview.log
& "C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath . -executeMethod ProductionLevelImporter.Apply -quit -logFile import.log
```

Close the interactive editor before a batch command. `PreparePhase2` is the one-time bootstrap used to author the required feature component in Sandbox and create the shared profile/output directory; normal reimports use Apply, not PreparePhase2.

All edge poses/indexing and point conversions come from BoardGrid. Identical edge overlaps are deduplicated; conflicting states fail. Only Level 40's explicitly audited 0.05-logical-unit offset is snapped. Original corner posts are omitted; standardized joined walls provide the geometry. Point X/Z use half-logical snapping; playable Y is standardized. Historical ideal times are editor provenance only.

The complete batch must pass shape/enum/ID/profile/clearance checks and conservative marble-clearance traversal before any level asset is written. Unknown off-grid walls, identity conflicts and unreachable objectives abort. This is validation-before-write, not an OS-level transactional filesystem operation; use Git to review writes.

The current intermediate and representative campaign contain exactly 11 entries. For Phase 3, after approval, extract the reviewed expanded set into the same input path (or adapt the explicit path), preview it, and review the resulting campaign order. The importer does not consume historical catalog/build IDs. Do not silently replace an asset whose identity differs.

Results: `Docs/Phase2/Migration.md`; full model, tests and limitations: `Docs/Phase2/Report.md`.
