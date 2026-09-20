using System;
namespace SaileachStudios.Mirlini.Board {
    public static class LevelMechanics {
        public const float UnlockSeconds = 2f;
        // Original ring's sphere radius (0.27) times its scale (5), in logical units.
        public const float RingRadius = 1.35f * BoardGrid.CellSize;
    }
    // Live state only. Never write progress back into a LevelData asset.
    public sealed class LevelAttempt {
        private readonly bool[] revealed = new bool[BoardGrid.EdgeCount];
        public bool GoalUnlocked { get; private set; }
        public float UnlockProgress { get; private set; }
        public LevelAttempt(bool needsUnlock) { GoalUnlocked = !needsUnlock; }
        public bool IsRevealed(int edge) => revealed[edge];
        public void Reveal(int edge) { revealed[edge] = true; }
        public void OccupyRing(bool occupied, bool playable, float deltaTime) {
            if (GoalUnlocked) return;
            if (!occupied) { UnlockProgress = 0; return; }
            if (!playable || !BoardGrid.IsFinite(deltaTime) || deltaTime <= 0) return;
            UnlockProgress = Math.Min(LevelMechanics.UnlockSeconds, UnlockProgress + deltaTime);
            GoalUnlocked = UnlockProgress >= LevelMechanics.UnlockSeconds;
        }
    }
}
