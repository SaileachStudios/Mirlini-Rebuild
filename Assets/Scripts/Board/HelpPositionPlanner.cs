using System;
using System.Collections.Generic;

namespace SaileachStudios.Mirlini.Board
{
    public static class HelpPositionPlanner
    {
        // Nearest point in the complement of radius-expanded axis-aligned rectangles.
        // Conservative at corners, deterministic X/Z tie-break; no physics queries or runtime state.
        public static bool TryFind(BoardPoint current, float radius, float margin, float maxDistance,
            IReadOnlyList<BoardRectangle> obstacles, out BoardPoint result) {
            result = current;
            if (!BoardGrid.IsFinite(current.X) || !BoardGrid.IsFinite(current.Z) ||
                !BoardGrid.IsFinite(radius) || !BoardGrid.IsFinite(margin) || !BoardGrid.IsFinite(maxDistance) ||
                radius <= 0 || margin < 0 || maxDistance < 0 || obstacles == null) return false;
            float clearance = radius + margin;
            float limit = BoardGrid.HalfExtent - clearance;
            if (limit < 0) return false;
            var expanded = new List<BoardRectangle>();
            var xs = new SortedSet<float> { Clamp(current.X, -limit, limit), -limit, limit };
            var zs = new SortedSet<float> { Clamp(current.Z, -limit, limit), -limit, limit };
            foreach (BoardRectangle obstacle in obstacles) {
                if (!BoardGrid.IsFinite(obstacle.MinX) || !BoardGrid.IsFinite(obstacle.MaxX) ||
                    !BoardGrid.IsFinite(obstacle.MinZ) || !BoardGrid.IsFinite(obstacle.MaxZ) ||
                    obstacle.MinX > obstacle.MaxX || obstacle.MinZ > obstacle.MaxZ) return false;
                var box = obstacle.Expanded(clearance);
                expanded.Add(box);
                Add(xs, box.MinX, limit); Add(xs, box.MaxX, limit);
                Add(zs, box.MinZ, limit); Add(zs, box.MaxZ, limit);
            }
            bool found = false; float best = maxDistance * maxDistance;
            foreach (float x in xs) {
                if (Math.Abs(x - current.X) > maxDistance) continue;
                foreach (float z in zs) {
                    float distance = (x-current.X)*(x-current.X) + (z-current.Z)*(z-current.Z);
                    if (distance > best || (found && distance >= best)) continue;
                    var candidate = new BoardPoint(x,z); bool legal = true;
                    foreach (var box in expanded) if (box.ContainsInterior(candidate)) { legal = false; break; }
                    if (!legal) continue;
                    found = true; best = distance; result = candidate;
                }
            }
            return found;
        }
        private static float Clamp(float v, float min, float max) => Math.Max(min, Math.Min(max, v));
        private static void Add(SortedSet<float> values, float v, float limit) { if (v >= -limit && v <= limit) values.Add(v); }
    }
}
