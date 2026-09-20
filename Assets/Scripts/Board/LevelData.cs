using System;
using UnityEngine;

namespace SaileachStudios.Mirlini.Board
{
    public enum WallState { Empty, Solid, RevealOnCollision }
    // No calibrated state or numeric target is offered until calibration is implemented.
    public enum StarCalibrationState { NeedsCalibration }
    [Serializable]
    public sealed class LevelInstructions {
        [TextArea] public string Keyboard;
        [TextArea] public string Touch;
        [TextArea] public string Gyro;
    }
    [Serializable]
    public sealed class UnlockFeature {
        public bool Enabled;
        public Vector3 RingPosition;
    }
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "Mirlini/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Tooltip("Permanent identity. Do not change when renaming or reordering a published level.")]
        [HideInInspector] public string LevelId;
        public string LevelName;
        public LevelInstructions Instructions = new LevelInstructions();
        [Tooltip("Board-local physical coordinates. Author in logical units in the Level Editor.")]
        public Vector3 MarbleStartPosition;
        public Vector3 HolePosition;
        public WallState[] Walls = new WallState[BoardGrid.EdgeCount];
        public UnlockFeature Unlock = new UnlockFeature();
        [Tooltip("Null means ordinary lighting; a profile enables Dark independently of Unlock.")]
        public DarkLightingProfile Dark;
        public StarCalibrationState StarCalibration = StarCalibrationState.NeedsCalibration;
#if UNITY_EDITOR
        [HideInInspector] public string MigrationSourceGuid;
        [HideInInspector] public string MigrationSourceHash;
        [HideInInspector] public int MigrationLevelNumber;
        [HideInInspector] public float HistoricalIdealTime;
        [HideInInspector] public string MigrationNotes;
#endif
    }
}
