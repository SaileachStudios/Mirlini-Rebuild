from audit_mirlini import *
ls=json.loads((OUT/'levels.json').read_text());obs=json.loads((OUT/'rebuild-resolved.json').read_text());allnodes={c['id']:c for g in obs for c in g['components']};byid={g['id']:g for g in obs}
for g in obs:
    if script(g,'BoardTiltBehavior'):
        root=g['transform']
        descendants=[]
        for h in obs:
            k=h.get('transform');seen=set()
            while k and k not in seen:
                seen.add(k)
                if k==root:descendants.append(h['name']);break
                k=allnodes.get(k,{}).get('data',{}).get('m_Father',{}).get('key')
        print('TILT DESCENDANTS',descendants)
for g in obs:
    if any(script(g,n) for n in ['MarbleBehaviour','AudioSFXBehavior','HoleBehavior','LevelEndFlowController']):
        print('COMP',g['name'],[(Path(c.get('script','')).name,{k:v for k,v in c['data'].items() if not k.startswith('m_')}) for c in g['components'] if c['type']==114])
for typ in ['rings','holes','spotlights']:
    variants=defaultdict(list)
    for l in ls:
        for g in l[typ]:
            key=json.dumps(dict(scale=[round(sum(g['worldMatrix'][i][j]**2 for i in range(3))**.5,4) for j in range(3)],colliders=[{k:v for k,v in c['data'].items() if k in ['m_Radius','m_Size','m_Center','m_IsTrigger','m_Enabled']} for c in g['components'] if c['type'] in [65,135,136]],height=g['position'][1]),sort_keys=True)
            variants[key].append(l['level'])
    print(typ,dict(variants))
print('POST DEGREES',Counter(g['incidentEdgeCount'] for l in ls for g in l['cornerPosts']))
print('POST OFFGRID',[(l['level'],g['position']) for l in ls for g in l['cornerPosts'] if any(abs(g['position'][i]-round(g['position'][i]))>.001 for i in [0,2])])
print('INACTIVE SPECIALS',[(l['level'],g['name']) for l in ls for g in json.loads((OUT/f"level-{l['level']:02}-resolved.json").read_text()) if not g['active'] and any(script(g,n) for n in ['Hole','UnlockableHole','UnlockRing','InvisibleWalls','PlayerSpotLight','Marble'])])
print('OTHER HOLETAGS',[(l['level'],g['name']) for l in ls for g in json.loads((OUT/f"level-{l['level']:02}-resolved.json").read_text()) if g['tag']=='Hole' and not (script(g,'Hole') or script(g,'UnlockableHole'))])
# Audit local references in relevant resolved components, excluding source metadata.
issues=[]
for l in ls:
    oo=json.loads((OUT/f"level-{l['level']:02}-resolved.json").read_text());keys={g['id'] for g in oo}|{c['id'] for g in oo for c in g['components']}
    for g in oo:
        for c in g['components']:
            if c['type']==114 and c.get('script')!='UNRESOLVED':
                for k,v in c['data'].items():
                    if not k.startswith('m_') and isinstance(v,dict) and v.get('fileID') and not v.get('guid') and v.get('key') not in keys:issues.append((l['level'],g['name'],k,v))
print('GAMEPLAY REF ISSUES',issues)
dump('gameplay-reference-issues.json',issues)
