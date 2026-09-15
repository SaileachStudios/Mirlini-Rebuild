using System;

namespace SaileachStudios.Mirlini.Board
{
    // Pure logical/physical geometry contract. No Unity object or scene dependencies.
    public readonly struct BoardPoint
    {
        public readonly float X, Z;
        public BoardPoint(float x, float z) { X = x; Z = z; }
    }

    public readonly struct BoardRectangle
    {
        public readonly float MinX, MinZ, MaxX, MaxZ;
        public BoardRectangle(float minX, float minZ, float maxX, float maxZ) {
            MinX = minX; MinZ = minZ; MaxX = maxX; MaxZ = maxZ;
        }
        public BoardRectangle Expanded(float margin) => new BoardRectangle(MinX - margin, MinZ - margin, MaxX + margin, MaxZ + margin);
        public bool ContainsInterior(BoardPoint p) => p.X > MinX && p.X < MaxX && p.Z > MinZ && p.Z < MaxZ;
    }

    public readonly struct BoardEdge
    {
        public readonly BoardPoint Center;
        public readonly bool AlongZ;
        public BoardEdge(BoardPoint center, bool alongZ) { Center = center; AlongZ = alongZ; }
    }

    public static class BoardGrid
    {
        public const int Cells = 16;
        public const int EdgeCount = 2 * Cells * (Cells - 1);
        public const float CellSize = 1.8f;
        public const float PlacementStep = 0.5f;
        public const float WallThickness = 0.4f;
        public const float WallHeight = 1.5f;
        public const float WallLength = CellSize + WallThickness; // overlapping joins close diagonal gaps
        public const float HalfExtent = Cells * CellSize / 2f; // boundary INNER faces
        public const float OuterSize = 2f * HalfExtent + 2f * WallThickness;
        public const float FloorY = -0.5f;
        public const float PlayY = 0f;
        public const float WallCenterY = FloorY + WallHeight / 2f;

        public static BoardEdge GetEdge(int index) {
            if (index < 0 || index >= EdgeCount) throw new ArgumentOutOfRangeException(nameof(index));
            int row = index / (2 * Cells - 1), slot = index % (2 * Cells - 1);
            return slot < Cells - 1
                ? new BoardEdge(new BoardPoint(Cells / 2f - 1 - slot, (Cells - 1) / 2f - row), true)
                : new BoardEdge(new BoardPoint((Cells - 1) / 2f - (slot - Cells + 1), Cells / 2f - 1 - row), false);
        }

        public static bool TryGetEdgeIndex(BoardPoint center, bool alongZ, out int index) {
            float row = alongZ ? (Cells - 1) / 2f - center.Z : Cells / 2f - 1 - center.Z;
            float slot = alongZ ? Cells / 2f - 1 - center.X : (Cells - 1) / 2f - center.X;
            int r = (int)Math.Round(row), s = (int)Math.Round(slot);
            index = -1;
            if (Math.Abs(row - r) > 0.0001f || Math.Abs(slot - s) > 0.0001f || r < 0 || s < 0 ||
                r >= (alongZ ? Cells : Cells - 1) || s >= (alongZ ? Cells - 1 : Cells)) return false;
            index = r * (2 * Cells - 1) + s + (alongZ ? 0 : Cells - 1);
            return true;
        }

        public static BoardPoint ToWorld(BoardPoint logical) => new BoardPoint(logical.X * CellSize, logical.Z * CellSize);
        public static bool TrySnapEdge(BoardPoint logical, bool alongZ, out int index) {
            index = -1;
            if (!IsFinite(logical.X) || !IsFinite(logical.Z)) return false;
            double best = double.PositiveInfinity;
            for (int i=0;i<EdgeCount;i++) {
                BoardEdge edge = GetEdge(i);
                if (edge.AlongZ != alongZ) continue;
                double dx = (double)logical.X-edge.Center.X, dz = (double)logical.Z-edge.Center.Z;
                double distance = dx*dx+dz*dz;
                if (distance >= best) continue;
                best = distance; index = i;
            }
            return index >= 0;
        }
        public static BoardPoint ToLogical(BoardPoint physical) => new BoardPoint(physical.X / CellSize, physical.Z / CellSize);
        public static BoardPoint SnapPlacement(BoardPoint logical) => new BoardPoint(
            (float)Math.Round(logical.X / PlacementStep, MidpointRounding.AwayFromZero) * PlacementStep,
            (float)Math.Round(logical.Z / PlacementStep, MidpointRounding.AwayFromZero) * PlacementStep);
        public static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        public static bool IsInside(BoardPoint physical, float clearance = 0f) =>
            IsFinite(physical.X) && IsFinite(physical.Z) && clearance >= 0 &&
            Math.Abs(physical.X) <= HalfExtent - clearance && Math.Abs(physical.Z) <= HalfExtent - clearance;

        public static BoardRectangle GetWallRectangle(int index) {
            BoardEdge edge = GetEdge(index); BoardPoint p = ToWorld(edge.Center);
            float x = (edge.AlongZ ? WallThickness : WallLength) / 2f;
            float z = (edge.AlongZ ? WallLength : WallThickness) / 2f;
            return new BoardRectangle(p.X - x, p.Z - z, p.X + x, p.Z + z);
        }

        // Boundaries ordered east, north, west, south.
        public static BoardRectangle GetBoundary(int index) {
            if (index < 0 || index > 3) throw new ArgumentOutOfRangeException(nameof(index));
            float h = HalfExtent, t = WallThickness, outer = h + t;
            switch (index) {
                case 0: return new BoardRectangle(h, -outer, h + t, outer);
                case 1: return new BoardRectangle(-outer, h, outer, h + t);
                case 2: return new BoardRectangle(-h - t, -outer, -h, outer);
                default: return new BoardRectangle(-outer, -h - t, outer, -h);
            }
        }
    }
}
