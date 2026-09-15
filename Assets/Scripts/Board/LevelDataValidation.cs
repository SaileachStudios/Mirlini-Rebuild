using UnityEngine;

namespace SaileachStudios.Mirlini.Board
{
    public static class LevelDataValidation
    {
        public static bool TryValidate(LevelData level, out string error, float marbleRadius = 0f) {
            error = null;
            if (level == null) { error = "Level asset is missing."; return false; }
            if (level.wallInfo == null || level.wallInfo.Length != BoardGrid.EdgeCount) {
                error = $"Level '{level.name}' needs exactly {BoardGrid.EdgeCount} wall entries."; return false;
            }
            if (level.Type != LevelType.Normal) { error = "Special mechanics are not supported by Phase 1 runtime setup yet."; return false; }
            if (!ValidPoint(level.MarbleStartPosition, marbleRadius) || !ValidPoint(level.HolePosition, 0f)) {
                error = "Start and goal must be finite board-local points at playable height and inside the boundaries."; return false;
            }
            if ((level.MarbleStartPosition - level.HolePosition).sqrMagnitude < 0.0001f) {
                error = "Start and goal cannot share a position."; return false;
            }
            for (int i = 0; i < level.wallInfo.Length; i++) {
                if (!level.wallInfo[i]) continue;
                var wall = BoardGrid.GetWallRectangle(i).Expanded(marbleRadius);
                if (wall.ContainsInterior(new BoardPoint(level.MarbleStartPosition.x, level.MarbleStartPosition.z))) {
                    error = $"Start overlaps wall edge {i} at the marble's collision radius."; return false;
                }
            }
            return true;
        }
        private static bool ValidPoint(Vector3 p, float clearance) => BoardGrid.IsFinite(p.y) &&
            Mathf.Abs(p.y - BoardGrid.PlayY) < 0.0001f && BoardGrid.IsInside(new BoardPoint(p.x,p.z), clearance);
    }
}
