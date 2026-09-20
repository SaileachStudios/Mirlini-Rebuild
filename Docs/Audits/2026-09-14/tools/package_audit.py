from audit_mirlini import *
import shutil, zipfile
ls=json.loads((OUT/'inventory.json').read_text())
assert len(ls)==50 and [l['level'] for l in ls]==list(range(1,51))
assert sum(l['cornerPostCount'] for l in ls)==698
assert sum(bool(l['cornerPostCount']) for l in ls)==35
assert all(l['holeCount']==1 for l in ls)
assert sum('Unlock' in l['features'] for l in ls)==18
assert sum('Dark' in l['features'] for l in ls)==13
assert sum('InvisibleWalls' in l['features'] for l in ls)==11
assert all(l['normalWallObjectCount']>=4 for l in ls)
grid=json.loads((OUT/'rebuild-grid.json').read_text());errors=[]
for i,g in enumerate(grid):
    row,offset=divmod(i,31)
    if offset<15:x,z=7-offset,7.5-row
    else:x,z=7.5-(offset-15),7-row
    expected=[1.8*x-.07,0,1.8*z-.01]
    errors.append(max(abs(a-b) for a,b in zip(expected,g['center'])))
assert max(errors)<.0001
print('Final inventory assertions PASS; grid affine max residual',max(errors))
# Expand hashes to metadata actually used for GUID resolution.
manifest=json.loads((OUT/'source-hashes.json').read_text());repo=ROOT/'Mirlini-Original'
for rel in list(manifest):
    p=repo/(rel+'.meta')
    if p.exists():manifest[rel+'.meta']=hashlib.sha256(p.read_bytes()).hexdigest()
dump('source-hashes.json',manifest)
publish=Path(__file__).parent/'Mirlini-Audit-Package';publish.mkdir(exist_ok=True)
keep=['Report.md','Inventory.md','inventory.json','Maze-Atlas.html','rebuild-grid.json','catalog-variants.json','Boot-catalog.json','reference-and-test-audit.json','source-hashes.json']
for name in keep:shutil.copy2(OUT/name,publish/name)
with zipfile.ZipFile(publish/'evidence.zip','w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
    for p in OUT.glob('*.json'):
        if p.name not in keep:z.write(p,p.name)
toolout=publish/'tools';toolout.mkdir(exist_ok=True)
for name in ['audit_mirlini.py','analyze_geometry.py','validate_audit.py','audit_extras.py','final_checks.py','create_reports.py','package_audit.py']:
    text=read(Path(__file__).parent/name)
    if name=='audit_mirlini.py':
        text=text.replace("sys.path.insert(0, str(Path(__file__).parent/'audit_dependencies'))", "# Install PyYAML 6.0.3 in your Python environment before running.")
    (toolout/name).write_text(text,encoding='utf8')
(toolout/'README.md').write_text('''# Reproducing the audit

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
''',encoding='utf8')
print('Package bytes',sum(p.stat().st_size for p in publish.rglob('*') if p.is_file()))
