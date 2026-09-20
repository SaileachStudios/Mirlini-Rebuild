from audit_mirlini import *
ls=json.loads((OUT/'levels.json').read_text());repo=Repo('Mirlini-Original');variants=defaultdict(list);extras=[]
for l in ls:
    obs=json.loads((OUT/f"level-{l['level']:02}-resolved.json").read_text());corners=[g for g in obs if g['active'] and g['source'].endswith('Corner.prefab')]
    l['cornerPosts']=corners
    boundary=[g for g in obs if g['active'] and g['source'].endswith('Game Board.prefab') and any(c['type']==65 for c in g['components'])]
    l['boundaryWalls']=boundary
    print(l['level'],'corners',len(corners),'boundary',len(boundary),'tags',set(g['tag'] for g in boundary),'ideal',l['originalLevelInfo']['idealTime'])
    for g in corners:
        x,y,z=g['position'];incident=[]
        for axis,px,pz in [key.split(':') for key in l['gridAudit']['edges']]:
            px,pz=float(px),float(pz)
            if (axis=='x' and abs(pz-z)<.001 and abs(abs(px-x)-.5)<.001) or (axis=='z' and abs(px-x)<.001 and abs(abs(pz-z)-.5)<.001):incident.append((axis,px,pz))
        g['incidentEdgeCount']=len(incident)
        if not incident:extras.append(dict(level=l['level'],position=g['position'],name=g['name'],reason='corner without incident grid edge'))
    for g in obs:
        for c in script(g,'LevelManager'):
            variants[json.dumps(c['data']['levels'],sort_keys=True)].append(l['level'])
dump('levels.json',ls);dump('catalog-variants.json',[dict(sceneLevels=nums,catalog=json.loads(cat)) for cat,nums in variants.items()]);dump('corner-exceptions.json',extras)
print('CORNER EXCEPTIONS',extras)
print('CATALOG VARIANTS',[(v,json.loads(k)[-1]) for k,v in variants.items()])
print('OVERLAPS',[(l['level'],l['gridAudit']['overlaps']) for l in ls if l['gridAudit']['overlaps']])
print('UNRESOLVED ORIGINAL',json.loads((OUT/'reference-and-test-audit.json').read_text())['Mirlini-Original']['unresolvedAssetGuids'].keys())
print('CORNER SAMPLE',[(g['position'],g['worldMatrix'],g['tag']) for g in ls[1]['cornerPosts'][:4]])
# Boot and menu catalog determine what persists when playing through the shipped flow.
for p in (repo.root/'Assets/Scenes').glob('*.unity'):
    obs,_,_=repo.objects(p)
    for g in obs:
        for c in script(g,'LevelManager'):
            dump(p.stem+'-catalog.json',c['data']);print('BOOT CATALOG',p.name,c['data']['levels'][-1])
