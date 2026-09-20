# Reproducing the audit

Requires Python 3 and PyYAML 6.0.3. The scripts default to the two local paths supplied for this audit; edit ROOT in audit_mirlini.py if relocating repositories.

Run in order:

1. python audit_mirlini.py
2. python analyze_geometry.py
3. python validate_audit.py
4. python audit_extras.py
5. python final_checks.py
6. python create_reports.py

Output is tools/Mirlini-Audit. The scripts read the original and rebuild and only write their own report directory. Report.md is the authored assessment, not automatically regenerated. package_audit.py is retained for provenance of the packaging step; it expects Report.md in its output folder before use.

Evidence IDs combine scene prefab-instance file IDs and source file IDs. worldMatrix includes inherited transforms; position is world-space. gridIndices are logical topology indices, not an assertion of exact rebuild collision equivalence. The resolver is scoped to the serialized forms encountered here, not a replacement for Unity serialization.
