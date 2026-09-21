"""Verify the read-only audit sources and emit a compact intermediate; never opens Unity."""
import argparse, hashlib, json, re
from pathlib import Path
REPO=Path(__file__).resolve().parents[2]
SELECTED=[1,6,10,15,21,27,29,40,45,46,50]
def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('--original',type=Path,required=True)
    selection=parser.add_mutually_exclusive_group()
    selection.add_argument('--levels',type=int,nargs='+')
    selection.add_argument('--all',action='store_true',help='Extract the explicit Level 1-50 production batch')
    parser.add_argument('--output',type=Path)
    args=parser.parse_args()
    args.levels=list(range(1,51)) if args.all else (args.levels or SELECTED)
    args.output=args.output or REPO/("Docs/Phase3/full-campaign.json" if args.all else "Docs/Phase2/representatives.json")
    audit=REPO/'Docs/Audits/2026-09-14'
    for relative,expected in json.loads((audit/'source-hashes.json').read_text(encoding='utf-8')).items():
        path=args.original/Path(relative.replace('\\','/'))
        if hashlib.sha256(path.read_bytes()).hexdigest()!=expected:
            raise ValueError('Source changed since audit: '+relative)
    rows={item['level']:item for item in json.loads((audit/'inventory.json').read_text(encoding='utf-8'))}
    result=[]
    def point(v):return dict(x=v[0],y=v[1],z=v[2])
    for number in sorted(set(args.levels)):
        row=rows[number]
        assert row['holeCount']==1
        path=args.original/f'Assets/Scenes/Maze Scenes/Level {number}.unity'
        guid=re.search(r'guid: (\w+)',Path(str(path)+'.meta').read_text(encoding='utf-8')).group(1)
        rings=row['unlock'];assert len(rings)<=1
        result.append(dict(number=number,sourceGuid=guid,sourceHash=row['sceneSha256'],
            displayName=row['sceneLevelInfo']['name'],historicalTime=row['sceneLevelInfo']['idealTime'],
            start=point(row['marbleStart']),goal=point(row['goalPosition']),
            unlock=bool(rings),ring=point(rings[0]['position']) if rings else point([0,0,0]),
            dark='Dark' in row['features'],cornerPosts=row['cornerPostCount'],
            instructions=dict(Keyboard=row['instructions']['levelGoalKeyboard'],Touch=row['instructions']['levelGoalTouch'],Gyro=row['instructions']['levelGoalGryo']),
            walls=[dict(sourceId=w['id'],center=point(w['colliderGeometry']['center']),
                size=point(w['colliderGeometry']['size']),reveal=w['visibility']=='RevealOnCollision') for w in row['walls']]))
    args.output.parent.mkdir(parents=True,exist_ok=True)
    args.output.write_text(json.dumps(dict(version=1,levels=result),indent=2,ensure_ascii=False)+'\n',encoding='utf-8',newline='\n')
    print('Verified audit source hashes; extracted '+str(len(result))+' levels to '+str(args.output))
if __name__=='__main__':main()
