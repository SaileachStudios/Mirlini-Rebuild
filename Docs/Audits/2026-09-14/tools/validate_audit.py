from audit_mirlini import *
ls=json.loads((OUT/'levels.json').read_text());reb=Repo('Mirlini-Rebuild');orig=Repo('Mirlini-Original')
health={}
for repo in [orig,reb]:
    refs=defaultdict(list);missingmeta=[]
    for p in (repo.root/'Assets').rglob('*'):
        if p.is_file() and p.suffix in ('.unity','.prefab','.asset','.cs'):
            if not Path(str(p)+'.meta').exists():missingmeta.append(str(p.relative_to(repo.root)))
            if p.suffix!='.cs':
                for guid in set(re.findall(r'guid: ([0-9a-f]{32})',read(p))):
                    if guid not in repo.guids and not guid.startswith('0000000000000000'):
                        refs[guid].append(str(p.relative_to(repo.root)))
    tests={str(p.relative_to(repo.root)):dict(test=len(re.findall(r'\[Test\]',read(p))),unityTest=len(re.findall(r'\[UnityTest\]',read(p))),testCase=len(re.findall(r'\[TestCase\(',read(p)))) for p in (repo.root/'Assets').rglob('*Tests.cs')}
    health[repo.root.name]=dict(unresolvedAssetGuids=dict(refs),missingMeta=missingmeta,tests=tests)
dump('reference-and-test-audit.json',health)
print('REBUILD HEALTH',json.dumps(health['Mirlini-Rebuild'],indent=1))
allinfo=[];unusual=[];settings=defaultdict(lambda:defaultdict(list));scriptsettings=defaultdict(lambda:defaultdict(list))
for l in ls:
    obs=json.loads((OUT/f"level-{l['level']:02}-resolved.json").read_text())
    info=next(c['data']['levels'] for g in obs for c in script(g,'LevelManager'));allinfo.append(info)
    for g in obs:
        for c in g['components']:
            if c['type'] in (65,135,136,64) and g['active'] and not (g['tag']=='Wall' or script(g,'Marble') or script(g,'Hole') or script(g,'UnlockableHole') or script(g,'UnlockRing')):
                unusual.append(dict(level=l['level'],name=g['name'],source=g['source'],type=c['type'],data=c['data']))
            if c['type']==108 and g['active']:
                d={k:v for k,v in c['data'].items() if k not in ['m_GameObject','m_CorrespondingSourceObject','m_PrefabInstance','m_PrefabAsset']}
                settings['light'][json.dumps(d,sort_keys=True)].append(l['level'])
            if c.get('script') and Path(c['script']).name in ['Marble.cs','UnlockRing.cs','Hole.cs','UnlockableHole.cs','GameManager.cs']:
                d={k:v for k,v in c['data'].items() if not k.startswith('m_') and not isinstance(v,dict)}
                scriptsettings[Path(c['script']).name][json.dumps(d,sort_keys=True)].append(l['level'])
    info_n=next(x for x in info if x['levelNum']==l['level']);l['originalLevelInfo']=info_n
print('Level catalog consistent?',all(x==allinfo[0] for x in allinfo),'length',len(allinfo[0]))
print('Catalog',allinfo[0]);print('script settings',dict(scriptsettings));print('Other colliders',Counter((x['name'],x['source'],x['type']) for x in unusual))
dump('level-catalog.json',allinfo[0]);dump('other-colliders.json',unusual);dump('serialized-setting-variants.json',dict(scriptsettings));dump('lighting-variants.json',dict(settings));dump('levels.json',ls)
# Internal logical edges must reproduce collider centerlines, except recorded deviations.
for l in ls:
    assert len(l['marbles'])==1 and len(l['holes'])==1
    for g in l['walls']:
        for i in g.get('gridIndices',[]):assert 0<=i<480
    for g in l['rings']:assert script(g,'UnlockRing')[0]['data']['requiredTime']==2
assert sum(len(l['gridAudit']['exceptions']) for l in ls)==1
assert all(len(l['instructions'])==1 for l in ls)
print('AUDIT ASSERTIONS PASS')
