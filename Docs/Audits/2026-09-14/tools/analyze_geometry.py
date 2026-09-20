from audit_mirlini import *
ls=json.loads((OUT/'levels.json').read_text());obs=json.loads((OUT/'rebuild-resolved.json').read_text());byid={g['id']:g for g in obs}
manager=next(c['data'] for g in obs for c in script(g,'LevelManager'))
grid=[byid[r['key']] for r in manager['walls']]
print('GRID',len(grid),'unique',len(set(g['id'] for g in grid)))
print([(i,g['position'],g['worldMatrix']) for i,g in enumerate(grid) if i in [0,1,14,15,16,30,31,32,464,479]])
def geometry(g):
    c=next((c['data'] for c in g['components'] if c['type']==65),None)
    if not c:return None
    m=g['worldMatrix'];size=c['m_Size'];cen=c['m_Center'];pos=[sum(m[i][j]*cen['xyz'[j]] for j in range(3))+m[i][3] for i in range(3)]
    dims=[sum(abs(m[i][j])*size['xyz'[j]] for j in range(3)) for i in range(3)]
    return {'center':[round(x,6) for x in pos],'size':[round(x,6) for x in dims]}
gridout=[dict(index=i,**geometry(g)) for i,g in enumerate(grid)];dump('rebuild-grid.json',gridout)
for l in ls:
    edges={};exceptions=[];bounds=[]
    for g in l['walls']:
        geo=geometry(g);g['colliderGeometry']=geo
        x,y,z=geo['center'];dx,dy,dz=geo['size']
        if 'Game Board.prefab' in g['source']:bounds.append(g);continue
        if min(dx,dz)>.31:exceptions.append(dict(id=g['id'],name=g['name'],reason='non-segment',geometry=geo));continue
        axis='x' if dx>dz else 'z';length=max(dx,dz);fixed=z if axis=='x' else x;mid=x if axis=='x' else z
        a=mid-length/2;b=mid+length/2
        if max(abs(a-round(a)),abs(b-round(b)),abs(fixed-round(fixed)))>.001:
            exceptions.append(dict(id=g['id'],name=g['name'],reason='off unit lattice',geometry=geo));continue
        assigned=[]
        for t in range(round(a),round(b)):
            px,pz=(t+.5,round(fixed)) if axis=='x' else (round(fixed),t+.5)
            # Editor orders x-normal boundaries first, then z-normal boundaries; z descends outside, x descends inside.
            index=(round(7.5-pz)*31+round(7-px)) if axis=='z' else (round(7-pz)*31+15+round(7.5-px))
            if not 0<=index<480 or abs(fixed)>=8:
                exceptions.append(dict(id=g['id'],reason='outside internal grid',position=[px,pz]));continue
            key=f'{axis}:{px}:{pz}';assigned.append(index)
            edges.setdefault(key,[]).append(dict(wall=g['id'],index=index,state='RevealOnCollision' if script(g,'InvisibleWalls') else 'Solid'))
        g['gridIndices']=assigned
    l['gridAudit']={'uniqueEdges':len(edges),'boundaryWallObjects':len(bounds),'exceptions':exceptions,'overlaps':{k:v for k,v in edges.items() if len(v)>1},'edges':edges}
    print(l['level'],'edges',len(edges),'exceptions',exceptions,'overlaps',len(l['gridAudit']['overlaps']))
dump('levels.json',ls)
dump('geometry-summary.json',[dict(level=l['level'],**{k:v for k,v in l['gridAudit'].items() if k!='edges'}) for l in ls])
