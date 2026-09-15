using UnityEngine;

namespace SaileachStudios.Mirlini.Board
{
    // Unity adapter used by runtime setup and authoring. Geometry numbers live in BoardGrid only.
    public static class BoardGeometryPlacement
    {
        public static Vector3 Position(BoardPoint physical, Vector3 origin, float y = BoardGrid.PlayY) =>
            origin + new Vector3(physical.X, y, physical.Z);

        public static void ApplyWall(Transform target, int edge, Vector3 origin) {
            BoardEdge pose = BoardGrid.GetEdge(edge);
            target.SetPositionAndRotation(Position(BoardGrid.ToWorld(pose.Center), origin, BoardGrid.WallCenterY),
                pose.AlongZ ? Quaternion.Euler(0, 90, 0) : Quaternion.identity);
            target.localScale = new Vector3(BoardGrid.WallLength, BoardGrid.WallHeight, BoardGrid.WallThickness);
        }
        public static void ApplyBoundary(Transform target, int index, Vector3 origin) {
            var box = BoardGrid.GetBoundary(index);
            target.SetPositionAndRotation(Position(new BoardPoint((box.MinX+box.MaxX)/2f, (box.MinZ+box.MaxZ)/2f), origin, BoardGrid.WallCenterY), Quaternion.identity);
            target.localScale = new Vector3(box.MaxX-box.MinX, BoardGrid.WallHeight, box.MaxZ-box.MinZ);
        }
        public static void ApplyFloor(Transform target, Vector3 origin) {
            target.SetPositionAndRotation(origin + Vector3.up * BoardGrid.FloorY, Quaternion.Euler(90,0,0));
            target.localScale = Vector3.one * BoardGrid.OuterSize;
        }
    }
}
