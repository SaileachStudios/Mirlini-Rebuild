import sys, re, json, copy, math, hashlib
from pathlib import Path
from collections import Counter, defaultdict
# Install PyYAML 6.0.3 in your Python environment before running.
import yaml
ROOT=Path(r'C:\Users\coyot\Documents\Unity Projects\Github')
OUT=Path(__file__).parent/'Mirlini-Audit'
OUT.mkdir(exist_ok=True)
cache={}
def read(p): return p.read_text(encoding='utf-8-sig')
def docs(p):
    if p not in cache:
        txt=read(p); result={}
        for m in re.finditer(r'^--- !u!(\d+) &(-?\d+)( stripped)?\n(.*?)(?=^--- !u!|\Z)',txt,re.M|re.S):
            result[int(m[2])]={'type':int(m[1]),'stripped':bool(m[3]),'data':next(iter(yaml.load(m[4],Loader=yaml.CSafeLoader).values()))}
        cache[p]=result
    return copy.deepcopy(cache[p])
class Repo:
    def __init__(self,name):
        self.root=ROOT/name; self.guids={}; self.warnings=[]
        for p in (self.root/'Assets').rglob('*.meta'):
            m=re.search(r'^guid: (\w+)',read(p),re.M)
            if m:self.guids[m[1]]=Path(str(p)[:-5])
    def expand(self,p,prefix=''):
        ds=docs(p); nodes={}; aliases={}; instances={}
        for fid,d in ds.items():
            if d['type']==1001:
                src=self.guids.get(d['data']['m_SourcePrefab']['guid'])
                if not src: self.warnings.append(('missing prefab',str(p),fid));continue
                child,ca=self.expand(src,prefix+str(fid)+'/');nodes.update(child)
                instances[fid]=(child,ca,src)
        for fid,d in ds.items():
            key=prefix+str(fid)
            if d['stripped']:
                inst=d['data']['m_PrefabInstance']['fileID']; source=d['data']['m_CorrespondingSourceObject']['fileID']
                aliases[fid]=instances[inst][1].get(source,prefix+str(inst)+'/'+str(source))
            elif d['type']!=1001: aliases[fid]=key
        def refs(v):
            if isinstance(v,dict):
                if 'fileID' in v and 'guid' not in v and v['fileID']!=0:
                    return dict(v,key=aliases.get(v['fileID'],prefix+str(v['fileID'])))
                return {k:refs(x) for k,x in v.items()}
            if isinstance(v,list):return [refs(x) for x in v]
            return v
        for fid,d in ds.items():
            if not d['stripped'] and d['type']!=1001:
                nodes[aliases[fid]]={'type':d['type'],'data':refs(d['data']),'source':str(p.relative_to(self.root)),'fileID':fid}
        for fid,(child,ca,src) in instances.items():
            mod=ds[fid]['data']['m_Modification']
            for ov in mod.get('m_Modifications',[]):
                target=ca.get(ov['target']['fileID'])
                if target not in nodes:self.warnings.append(('override target',str(p),fid,ov));continue
                path=ov['propertyPath']; data=nodes[target]['data']
                val=ov.get('value'); ref=ov.get('objectReference',{})
                if ref.get('fileID',0)!=0:val=refs(ref)
                elif val is None or val=='':val=refs(ref) if isinstance(data.get(path),dict) else ''
                elif not isinstance(val,str):pass
                elif re.fullmatch(r'-?\d+',val):val=int(val)
                elif re.fullmatch(r'-?\d*\.?\d+(?:[eE][+-]?\d+)?',val):val=float(val)
                parts=path.split('.')
                try:
                    obj=data
                    for part in parts[:-1]:
                        if part=='Array':continue
                        if part.startswith('data['):obj=obj[int(part[5:-1])]
                        else:obj=obj.setdefault(part,{})
                    if parts[-1]=='size' and isinstance(obj,list):
                        obj[:]=obj[:int(val)]+[None]*max(0,int(val)-len(obj))
                    elif parts[-1].startswith('data['):obj[int(parts[-1][5:-1])]=val
                    else:obj[parts[-1]]=val
                except Exception as e:self.warnings.append(('override path',str(p),path,str(e)))
            for removed in mod.get('m_RemovedComponents',[]) or []:
                nodes.pop(ca.get(removed.get('fileID')),None)
            parent=refs(mod.get('m_TransformParent',{'fileID':0}))
            for key in child:
                if key in nodes and nodes[key]['type'] in (4,224) and nodes[key]['data'].get('m_Father',{}).get('fileID')==0:
                    nodes[key]['data']['m_Father']=parent
        return nodes,aliases
    def objects(self,p):
        nodes,aliases=self.expand(p); gos={}; transforms={}
        for k,n in nodes.items():
            if n['type']==1:gos[k]={'id':k,'name':n['data'].get('m_Name'),'tag':n['data'].get('m_TagString'),'activeSelf':n['data'].get('m_IsActive',1),'source':n['source'],'components':[]}
        for k,n in nodes.items():
            g=n['data'].get('m_GameObject',{}).get('key')
            if g in gos:
                comp=dict(n,id=k)
                if n['type']==114:comp['script']=str(self.guids.get(n['data'].get('m_Script',{}).get('guid'),'UNRESOLVED'))
                gos[g]['components'].append(comp)
                if n['type'] in (4,224):transforms[k]=n;gos[g]['transform']=k
        def mat(k):
            if k not in transforms:return [[int(i==j) for j in range(4)] for i in range(4)]
            n=transforms[k];d=n['data']
            if 'matrix' in n:return n['matrix']
            q=d.get('m_LocalRotation',{});x,y,z,w=[q.get(a, int(a=='w')) for a in 'xyzw'];s=d.get('m_LocalScale',{});v=d.get('m_LocalPosition',{})
            r=[[1-2*(y*y+z*z),2*(x*y-z*w),2*(x*z+y*w)], [2*(x*y+z*w),1-2*(x*x+z*z),2*(y*z-x*w)],[2*(x*z-y*w),2*(y*z+x*w),1-2*(x*x+y*y)]]
            local=[[r[i][j]*s.get('xyz'[j],1) for j in range(3)]+[v.get('xyz'[i],0)] for i in range(3)]+[[0,0,0,1]]
            par=mat(d.get('m_Father',{}).get('key'));n['matrix']=[[sum(par[i][a]*local[a][j] for a in range(4)) for j in range(4)] for i in range(4)];return n['matrix']
        def active(g,seen=None):
            if not g['activeSelf']:return False
            t=transforms.get(g.get('transform'),{}).get('data',{});par=transforms.get(t.get('m_Father',{}).get('key'),{}).get('data',{}).get('m_GameObject',{}).get('key')
            return active(gos[par]) if par in gos else True
        for g in gos.values():
            m=mat(g.get('transform'));g['worldMatrix']=m;g['position']=[round(m[i][3],6) for i in range(3)];g['active']=active(g)
        return list(gos.values()),nodes,aliases
def script(g,name):return [c for c in g['components'] if Path(c.get('script','')).name==name+'.cs']
def dump(name,data):(OUT/name).write_text(json.dumps(data,indent=2),encoding='utf8')
def run():
    orig=Repo('Mirlini-Original'); rebuild=Repo('Mirlini-Rebuild'); levels=[]
    for num in range(1,51):
        p=orig.root/f'Assets/Scenes/Maze Scenes/Level {num}.unity';obs,nodes,_=orig.objects(p)
        dump(f'level-{num:02}-resolved.json',obs)
        walls=[g for g in obs if g['active'] and g['tag']=='Wall'];holes=[g for g in obs if g['active'] and (script(g,'Hole') or script(g,'UnlockableHole'))]
        rings=[g for g in obs if g['active'] and script(g,'UnlockRing')];spots=[g for g in obs if g['active'] and script(g,'PlayerSpotLight')]
        inv=[g for g in walls if script(g,'InvisibleWalls')];marbles=[g for g in obs if g['active'] and script(g,'Marble')]
        ui=[c['data'] for g in obs for c in script(g,'LevelUI')]
        levels.append(dict(level=num,sha256=hashlib.sha256(p.read_bytes()).hexdigest(),marbles=marbles,holes=holes,rings=rings,spotlights=spots,walls=walls,normalWallCount=len(walls)-len(inv),invisibleWallCount=len(inv),instructions=[{k:v for k,v in d.items() if k.startswith('levelGoal')} for d in ui],features=(['Unlock'] if rings else [])+(['InvisibleWalls'] if inv else [])+(['Dark'] if spots else []),scriptInventory=dict(Counter(Path(c['script']).name for g in obs if g['active'] for c in g['components'] if 'script' in c)),lights=[g for g in obs if any(c['type']==108 for c in g['components'])],renderSettings=[n['data'] for n in nodes.values() if n['type']==104]))
    dump('levels.json',levels);dump('resolver-warnings.json',orig.warnings)
    obs,nodes,aliases=rebuild.objects(rebuild.root/'Assets/Scenes/Sandbox.unity');dump('rebuild-resolved.json',obs);dump('rebuild-resolver-warnings.json',rebuild.warnings)
    print(json.dumps([dict(level=l['level'],features=l['features'],walls=l['normalWallCount'],invisible=l['invisibleWallCount'],holes=len(l['holes']),start=[g['position'] for g in l['marbles']],goal=[g['position'] for g in l['holes']]) for l in levels],indent=1))
    print('Warnings',len(orig.warnings),len(rebuild.warnings))
if __name__=='__main__':run()
