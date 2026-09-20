using System.Collections.Generic;
using UnityEngine;
namespace SaileachStudios.Mirlini.Board {
    public static class LevelTraversal {
        private const int Count=(int)(BoardGrid.Cells/BoardGrid.PlacementStep)+1;
        public static bool CanReachObjectives(LevelData level) {
            const int count=Count;
            var clear=new bool[count,count];
            for(int x=0;x<count;x++) for(int z=0;z<count;z++) clear[x,z]=Clear(level,Point(x,z));
            Vector2Int start=Index(level.MarbleStartPosition),goal=Index(level.HolePosition),ring=Index(level.Unlock.RingPosition);
            if(!clear[start.x,start.y] || !clear[goal.x,goal.y]) return false;
            var seen=new bool[count,count];var queue=new Queue<Vector2Int>();queue.Enqueue(start);seen[start.x,start.y]=true;
            int[] dx={1,-1,0,0},dz={0,0,1,-1};
            while(queue.Count>0) {
                var a=queue.Dequeue();
                for(int i=0;i<4;i++) {
                    int x=a.x+dx[i],z=a.y+dz[i];
                    if(x<0 || z<0 || x>=count || z>=count || seen[x,z] || !clear[x,z]) continue;
                    Vector3 p=Point(a.x,a.y),q=Point(x,z);
                    if(!Clear(level,Vector3.Lerp(p,q,.25f)) || !Clear(level,Vector3.Lerp(p,q,.5f)) || !Clear(level,Vector3.Lerp(p,q,.75f))) continue;
                    seen[x,z]=true;queue.Enqueue(new Vector2Int(x,z));
                }
            }
            return seen[goal.x,goal.y] && (!level.Unlock.Enabled || seen[ring.x,ring.y]);
        }
        private static Vector3 Point(int x,int z) {
            var p=BoardGrid.ToWorld(new BoardPoint(-BoardGrid.Cells/2f+x*BoardGrid.PlacementStep,-BoardGrid.Cells/2f+z*BoardGrid.PlacementStep));
            return new Vector3(p.X,0,p.Z);
        }
        private static Vector2Int Index(Vector3 p) {
            var logical=BoardGrid.ToLogical(new BoardPoint(p.x,p.z));
            return new Vector2Int(Mathf.Clamp(Mathf.RoundToInt((logical.X+BoardGrid.Cells/2f)/BoardGrid.PlacementStep),0,Count-1),Mathf.Clamp(Mathf.RoundToInt((logical.Z+BoardGrid.Cells/2f)/BoardGrid.PlacementStep),0,Count-1));
        }
        private static bool Clear(LevelData level,Vector3 p) {
            var point=new BoardPoint(p.x,p.z);if(!BoardGrid.IsInside(point,.5f)) return false;
            for(int i=0;i<BoardGrid.EdgeCount;i++) if(level.Walls[i]!=WallState.Empty && BoardGrid.GetWallRectangle(i).Expanded(.5f).ContainsInterior(point)) return false;
            return true;
        }
    }
}
