from audit_mirlini import *
import html, zipfile, shutil
ls=json.loads((OUT/'levels.json').read_text());repo=Repo('Mirlini-Original')
def pos(v):return '('+', '.join(f'{x:g}' for x in v)+')'
def compact(g):
    return {k:g[k] for k in ['id','name','source','position','worldMatrix','gridIndices','colliderGeometry','incidentEdgeCount'] if k in g}
inventory=[];lines=['# Complete 50-level inventory','', 'Coordinates are original world (x, y, z). N counts visible wall segments plus four boundary walls, excluding corner posts. I counts invisible wall objects, including duplicates. E counts unique internal unit edges; Level 40 excludes its one off-grid segment. C counts solid corner posts. Every level has one active gameplay hole and one marble. Unlock duration is 2 seconds everywhere. See inventory.json for every wall, cell index, post, instruction variant and light configuration.','', '| Level | Features | Start | Goal | N / I / C | E | Unlock ring | Ideal seconds¹ |','|---|---|---|---|---|---|---|---|']
    # boundary classification must not depend on tags (Level 50 boundaries are Untagged).
for l in ls:
    internal=[g for g in l['walls'] if not g['source'].endswith('Game Board.prefab')]
    normal=sum(not bool(script(g,'InvisibleWalls')) for g in internal)+len(l['boundaryWalls'])
    item=dict(level=l['level'],sceneSha256=l['sha256'],features=l['features'],marbleStart=l['marbles'][0]['position'],goalPosition=l['holes'][0]['position'],holeCount=len(l['holes']),normalWallObjectCount=normal,invisibleWallObjectCount=l['invisibleWallCount'],cornerPostCount=len(l['cornerPosts']),uniqueGridEdgeCount=l['gridAudit']['uniqueEdges'],sceneLevelInfo=l['originalLevelInfo'],instructions=l['instructions'][0],walls=[dict(**compact(g),visibility='RevealOnCollision' if script(g,'InvisibleWalls') else 'Solid') for g in internal],boundaryWalls=[compact(g) for g in l['boundaryWalls']],cornerPosts=[compact(g) for g in l['cornerPosts']],gridAudit=l['gridAudit'],unlock=[dict(position=g['position'],duration=script(g,'UnlockRing')[0]['data']['requiredTime'],source=g['source']) for g in l['rings']],lights=l['lights'],renderSettings=l['renderSettings'],scriptInventory=l['scriptInventory'])
    inventory.append(item)
    ring=pos(l['rings'][0]['position']) if l['rings'] else '—'
    lines.append(f"| {l['level']} | {' + '.join(l['features']) or 'Normal'} | {pos(item['marbleStart'])} | {pos(item['goalPosition'])} | {normal} / {l['invisibleWallCount']} / {len(l['cornerPosts'])} | {l['gridAudit']['uniqueEdges']} | {ring} | {l['originalLevelInfo']['idealTime']} |")
lines+=['','¹ Scene-resolved catalog anchor. Level 50 is 17.92 in its own scene, but Boot has 1.5 and the wrong build ID; do not silently import Boot’s Level 50 entry.','', '## Exact instructions by level','']
for l in ls:
    lines+= [f"### Level {l['level']}",'']
    for k,v in l['instructions'][0].items():lines += [f"**{k}**",'',v.replace('\n','  \n'),'']
(OUT/'Inventory.md').write_text('\n'.join(lines),encoding='utf8');dump('inventory.json',inventory)
# A static visual index from resolved collider dimensions. All hidden walls are shown in purple for audit.
cards=[]
for l in ls:
    shapes=['<rect x="-8" y="-8" width="16" height="16" fill="#f3ead9"/>']
    for g in l['walls']+l['cornerPosts']:
        if g['source'].endswith('Game Board.prefab'):continue
        m=g['worldMatrix'];c=next((c['data'] for c in g['components'] if c['type']==65),None)
        if not c:continue
        size=c['m_Size'];center=c['m_Center'];pts=[]
        for a,b in [(-1,-1),(1,-1),(1,1),(-1,1)]:
            v=[center['x']+a*size['x']/2,center['y'],center['z']+b*size['z']/2]
            x=sum(m[0][j]*v[j] for j in range(3))+m[0][3];z=sum(m[2][j]*v[j] for j in range(3))+m[2][3];pts.append(f'{x:g},{-z:g}')
        color='#8d42c4' if script(g,'InvisibleWalls') else '#344151'
        shapes.append(f'<polygon points="{" ".join(pts)}" fill="{color}"><title>{html.escape(g["name"])} {pos(g["position"])}</title></polygon>')
    for g in l['rings']:
        x,y,z=g['position'];shapes.append(f'<circle cx="{x}" cy="{-z}" r="1.35" fill="none" stroke="#bc7900" stroke-width=".12"/>')
    for g,color,label in [(l['marbles'][0],'#087f6d','S'),(l['holes'][0],'#136cc4','G')]:
        x,y,z=g['position'];shapes.append(f'<circle cx="{x}" cy="{-z}" r=".35" fill="{color}"/><text x="{x}" y="{-z+.13}" font-size=".4" text-anchor="middle" fill="white">{label}</text>')
    cards.append(f'<article><h2>Level {l["level"]} <small>{html.escape(" + ".join(l["features"]) or "Normal")}</small></h2><svg viewBox="-8.5 -8.5 17 17" aria-label="Level {l["level"]} maze">{"".join(shapes)}</svg></article>')
(OUT/'Maze-Atlas.html').write_text('<!doctype html><html lang="en"><meta charset="utf-8"><title>Mirlini — 50-level audit atlas</title><style>body{font:16px system-ui;background:#e7ecf1;color:#162b40;margin:32px}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(300px,1fr));gap:20px}article{background:white;padding:16px;border-radius:10px}h2{font-size:20px}small{display:block;font-size:13px;font-weight:400}svg{width:100%}p{max-width:900px}</style><h1>Mirlini · Original maze atlas</h1><p>All 50 scenes, resolved from prefab defaults and scene overrides. Green S = start; blue G = goal; gold ring = unlock; purple = initially invisible solid wall; charcoal = visible walls and corner posts. Top is +Z, right is +X. This is a geometry audit, not a game-camera or lighting preview. Ring outline illustrates nominal trigger radius; Unity contact behavior still needs verification.</p><main>'+''.join(cards)+'</main></html>',encoding='utf8')
# File-level evidence fingerprints for reproducibility.
manifest={}
for p in sorted(set(cache)|set((repo.root/'Assets/Scripts').rglob('*.cs'))|set((repo.root/'Assets/Prefabs').rglob('*.prefab'))|set((repo.root/'Assets/Scenes').rglob('*.unity'))):
    if p.is_file():manifest[str(p.relative_to(repo.root))]=hashlib.sha256(p.read_bytes()).hexdigest()
dump('source-hashes.json',manifest)
print('Created inventory and atlas',len(inventory),'levels')
