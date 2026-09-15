using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.Board
{
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "Mirlini/Level Data")]
    public class LevelData : ScriptableObject
    {
        public string LevelName;
        [Tooltip("Board-local physical coordinates. Use Level Editor for logical half-unit placement.")]
        public Vector3 MarbleStartPosition;
        [Tooltip("Board-local physical coordinates; playable Y is defined by BoardGrid.")]
        public Vector3 HolePosition;
        public bool[] wallInfo = new bool[BoardGrid.EdgeCount]; // Reference which wall positions are active
        [Tooltip("Legacy prototype value only. Production star baseline needs calibration; not used for scoring.")]
        public float IdealCompletionTime;
        public LevelType Type;
    }

    public enum LevelType
    {
        Normal,
        Unlock,
        Dark,
        InvisibleWalls
    }
}
