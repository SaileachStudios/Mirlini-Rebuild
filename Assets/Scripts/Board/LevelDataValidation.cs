using System.Collections.Generic;
using UnityEngine;
namespace SaileachStudios.Mirlini.Board {
    public static class LevelDataValidation {
        public static bool TryValidate(LevelData level, out string error, float marbleRadius = .5f) {
            error = null;
            if (level == null) { error = "Level asset is missing."; return false; }
            if (string.IsNullOrWhiteSpace(level.LevelId)) { error = "A permanent LevelId is required."; return false; }
            if (level.Walls == null || level.Walls.Length != BoardGrid.EdgeCount) {
                error = $"Level '{level.name}' needs exactly {BoardGrid.EdgeCount} wall entries."; return false;
            }
            if (level.Instructions == null || level.Unlock == null || level.StarCalibration != StarCalibrationState.NeedsCalibration) {
                error = "Instructions, feature configuration and an uncalibrated star state are required."; return false;
            }
            if (level.Dark != null && !level.Dark.IsValid) { error = "Dark lighting profile is invalid."; return false; }
            if (!ValidPoint(level.MarbleStartPosition,marbleRadius) || !ValidPoint(level.HolePosition,marbleRadius) ||
                (level.Unlock.Enabled && !ValidPoint(level.Unlock.RingPosition,LevelMechanics.RingRadius))) {
                error = "Start, goal and enabled ring must be finite board-local points at playable height and inside the boundaries."; return false;
            }
            if ((level.MarbleStartPosition-level.HolePosition).sqrMagnitude < 4*marbleRadius*marbleRadius) {
                error = "Start and goal must remain separate."; return false;
            }
            for (int i=0;i<level.Walls.Length;i++) {
                WallState state=level.Walls[i];
                if (state<WallState.Empty || state>WallState.RevealOnCollision) { error=$"Invalid wall state at edge {i}."; return false; }
                if (state==WallState.Empty) continue;
                var wall=BoardGrid.GetWallRectangle(i).Expanded(marbleRadius);
                if (Contains(wall,level.MarbleStartPosition) || Contains(wall,level.HolePosition) ||
                    (level.Unlock.Enabled && Contains(wall,level.Unlock.RingPosition))) {
                    error=$"Start, goal or ring center overlaps wall edge {i} at the marble's collision radius."; return false;
                }
            }
            return true;
        }
        public static bool TryValidateCampaign(IEnumerable<LevelData> levels,out string error) {
            error=null;var ids=new HashSet<string>();
            if(levels==null) {error="Campaign list is missing.";return false;}
            foreach(var level in levels) {
                if(!TryValidate(level,out error)) return false;
                if(!ids.Add(level.LevelId)) {error="Duplicate LevelId: "+level.LevelId;return false;}
            }
            return true;
        }
        private static bool Contains(BoardRectangle r,Vector3 p) => r.ContainsInterior(new BoardPoint(p.x,p.z));
        private static bool ValidPoint(Vector3 p,float clearance) => BoardGrid.IsFinite(p.y) &&
            Mathf.Abs(p.y-BoardGrid.PlayY)<.0001f && BoardGrid.IsInside(new BoardPoint(p.x,p.z),clearance);
    }
}
